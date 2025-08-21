using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Wpf;
using RainbowTaskbar.Configuration;
using RainbowTaskbar.Configuration.Instruction;
using RainbowTaskbar.Configuration.Instruction.Instructions;
using RainbowTaskbar.Drawing;
using RainbowTaskbar.Helpers;
using RainbowTaskbar.HTTPAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Wpf.Ui.Controls;
using static RainbowTaskbar.Taskbar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using WebView2 = Microsoft.Web.WebView2.Wpf.WebView2;

namespace RainbowTaskbar;

/// <summary>
///     Interaction logic for Taskbar.xaml
/// </summary>
public partial class Taskbar : System.Windows.Window {

    public CanvasManager canvasManager;
    public bool secondary;
    public TaskbarHelper taskbarHelper;
    public TaskbarViewModel viewModel;
    public WindowHelper windowHelper;
    public Mutex webViewReady_ = new Mutex();
    public Mutex webViewReady { get => App.Settings.GraphicsRepeat ? webViewReady_ : App.webViewReady; }
    public WebView2 webView { get => App.Settings.GraphicsRepeat ?  webView_ : App.webView; }

    public int UnderlayOffset { get => taskbarHelper.YOffset; set => taskbarHelper.YOffset = value; }

    
    public Taskbar(IntPtr HWND, bool secondary) {
        webViewReady_.WaitOne();
        InitializeComponent();
        this.secondary = secondary;

        viewModel = new TaskbarViewModel(this, HWND);
        Closing += viewModel.OnWindowClosing;
        DataContext = viewModel;

        
        if(HWND != IntPtr.Zero && !App.Settings.GraphicsRepeat) {
            //webView_.Dispose();
            webView_ = null;
        }

    }
    

    public Taskbar(IntPtr HWND) : this(HWND, false) { }

    #region Ugly win32 message handler
    
    public IntPtr hwndInsertAfter = IntPtr.Zero;
    public struct WINDOWPOS {
        public IntPtr hwnd;
        public IntPtr hwndInsertAfter;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public uint flags;
    }
    public const int WM_WINDOWPOSCHANGING = 0x46;
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
        if (msg == WM_WINDOWPOSCHANGING) {
            WINDOWPOS wp = (WINDOWPOS) System.Runtime.InteropServices.Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
            if (GetWindow(wp.hwndInsertAfter, 3) != hwndInsertAfter) {
                //taskbarHelper.PlaceWindowUnder(this);
                
                taskbarHelper.SetBlur();
                hwndInsertAfter = taskbarHelper.HWND;
            }
        }
        if (msg == WM_Shell) {
            var wnd = lParam;
            var clname = new StringBuilder(256);
            GetClassName(wnd, clname, clname.Capacity);

            // fix: used to hide underlay when peeking window on taskbar
            var dontcare = clname.ToString() == "XamlExplorerHostIslandWindow" || clname.ToString() == "Shell_InputSwitchTopLevelWindow";

            if ((uint) wParam == 53 && !dontcare) 
                App.IsAppFullscreen = true;
            if ((uint) wParam == 54 && !dontcare) 
                App.IsAppFullscreen = false;
        }
        return IntPtr.Zero;
    }


    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern uint RegisterWindowMessage(string lpString);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool RegisterShellHookWindow(IntPtr window);

    private uint WM_Shell = 0;
    

    protected override void OnSourceInitialized(EventArgs e) {
        base.OnSourceInitialized(e);
        var hWnd = new WindowInteropHelper(this).EnsureHandle();
        HwndSource source = (HwndSource) HwndSource.FromHwnd(hWnd);
        if (!secondary) {
            RegisterShellHookWindow(new WindowInteropHelper(this).EnsureHandle());
            WM_Shell = RegisterWindowMessage("SHELLHOOK");
        }
        source.AddHook(WndProc);
    }
    #endregion

    private void RainbowTaskbar_Closed(object sender, EventArgs e) {
        taskbarHelper.Style = TaskbarHelper.TaskbarStyle.ForceDefault;
        taskbarHelper.SetBlur();
        
    }


    public static void SetupLayers() {
       App.Current.Dispatcher.Invoke(() => {
           if (App.Settings.GraphicsRepeat) {
               App.taskbars.ForEach(t => {
                   t.canvasManager.layers = new LayerManager(t);
               });
           }
           else {
               App.layers = new LayerManager();
                
               App.layers.width = (int) App.taskbars.Sum(t => t.Width);
               App.layers.height = (int) App.taskbars[0].Height;

           }
       });
    }
    private static void SetupWebView(WebView2 webView, Mutex mutex = null) {
        


        var envasync = CoreWebView2Environment.CreateAsync(null, Path.GetTempPath(), new CoreWebView2EnvironmentOptions());
        Task.Run(() => {
            var env = envasync.Result;
            App.Current.Dispatcher.Invoke(() => {
                Task t = null;
                // THIS IS MAD 
                try {
                    t = webView.EnsureCoreWebView2Async(env);
                }
                catch {
                    try {
                        t = webView.EnsureCoreWebView2Async();
                    } catch {
                        return;
                    }
                }
                Task.Run(() => {
                    try { t.Wait(); } catch { return; }
                    webView.Dispatcher.Invoke(() => {
                        if(mutex is not null) mutex.ReleaseMutex();

                        

                        webView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
                        webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                        webView.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;
                        webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
                        webView.CoreWebView2.Settings.IsPinchZoomEnabled = false;
                        webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                        webView.CoreWebView2.Settings.UserAgent = "RainbowTaskbar Web Display https://ad2017.dev/rnb";
                        webView.CoreWebView2.IsMuted = true;



                        webView.CoreWebView2.MemoryUsageTargetLevel = Microsoft.Web.WebView2.Core.CoreWebView2MemoryUsageTargetLevel.Low;

                        webView.CoreWebView2.ProcessFailed += (_, _) => {
                            App.ReloadTaskbars();
                        };
                    });
                });
            });
        });

        
    }

    [Serializable]
    public class RectangleF {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public RectangleF(System.Drawing.Rectangle rectangle) {
            X = rectangle.X;
            Y = rectangle.Y;
            Width = rectangle.Width;
            Height = rectangle.Height;
        }
        public RectangleF(System.Drawing.RectangleF rectangle) {
            X = rectangle.X;
            Y = rectangle.Y;
            Width = rectangle.Width;
            Height = rectangle.Height;
        }
        public RectangleF(RectangleF rectangle) {
            X = rectangle.X;
            Y = rectangle.Y;
            Width = rectangle.Width;
            Height = rectangle.Height;
        }

        public static RectangleF operator*(RectangleF left, float right) {
            var nw = new RectangleF(left);
            nw.X *= right;
            nw.Y *= right;
            nw.Width *= right;
            nw.Height *= right;
            return nw;
        }
    }
    [Serializable]
    public class UIData {
        [JsonPropertyName("systemTrayFrame")]
        public RectangleF SystemTrayFrame { get; set; }
        [JsonPropertyName("taskbarFrameRepeater")]
        public RectangleF TaskbarFrameRepeater { get; set; }
        [JsonPropertyName("taskbar")]
        public RectangleF Taskbar { get; set; }
        [JsonPropertyName("taskbarIndex")]
        public int Index { get; set; }

        public UIData(string rawUIData, Taskbar t) {
            var form = JsonSerializer.Deserialize<ExplorerTAP.ExplorerTAP.UIData>(rawUIData);
            SystemTrayFrame = new(form.SystemTrayFrame);
            var off = ExplorerTAP.ExplorerTAP.GetYPosition(t);
            SystemTrayFrame.Y -= off;
            TaskbarFrameRepeater = new(form.TaskbarFrameRepeater);
            TaskbarFrameRepeater.Y -= off;

            SystemTrayFrame *= (t.windowHelper.scale);
            TaskbarFrameRepeater *= (t.windowHelper.scale);

            Taskbar = new(t.taskbarHelper.GetRectangle());

            SystemTrayFrame.Height = Taskbar.Height;
            Index = App.taskbars.IndexOf(t);
        }
    }

    public class CS2WVMessage<T> {
        [JsonPropertyName("message")]
        public string Message { get; set; } = "";
        [JsonPropertyName("data")]
        public T Data { get; set; }
        public CS2WVMessage(string message, T data) {
            Message = message;
            Data = data;
        }
    }

    public List<UIData> UIDataForAllTaskbars() {
        try {
            return App.taskbars.Select(x => new UIData(x.windowHelper.LastUIData, x)).ToList();
        } catch { return new List<UIData>(); }
    }

    EventHandler uiChangedHandler = null;

    public void SetupWebViewMessageReceiver() {
        uiChangedHandler = (_, args) => {
            if (webView.CoreWebView2 is null) return;
            webView.CoreWebView2.PostWebMessageAsString(JsonSerializer.Serialize(new CS2WVMessage<List<UIData>>("ui",UIDataForAllTaskbars())));
        };
        windowHelper.TaskbarUIChanged += uiChangedHandler;
        webView.WebMessageReceived += (_, args) => {
            var message = JsonSerializer.Deserialize<JsonNode>(args.WebMessageAsJson);
            switch (message["m"]?.GetValue<string?>()) {
                case "style": {
                        var taskbars = new List<Taskbar>(App.Settings.GraphicsRepeat ? new List<Taskbar>() { this } : App.taskbars);
                        taskbars.ForEach(x => {
                            new TransparencyInstruction() {
                                Type = TransparencyInstruction.TransparencyInstructionType.Style,
                                Style = (TransparencyInstruction.TransparencyInstructionStyle) (message["style"]?.GetValue<int?>() ?? (int) TransparencyInstruction.TransparencyInstructionStyle.Default)
                            }.Execute(x);
                        });
                        break;
                    }
                case "transparency": {
                        var taskbars = new List<Taskbar>(App.Settings.GraphicsRepeat ? new List<Taskbar>() { this } : App.taskbars);
                        taskbars.ForEach(x => {
                            new TransparencyInstruction() {
                                Type = (TransparencyInstruction.TransparencyInstructionType) (message["which"]?.GetValue<int?>() ?? (int) TransparencyInstruction.TransparencyInstructionType.RainbowTaskbar),
                                Layer = message["layer"]?.GetValue<int?>() ?? 0,
                                Opacity = message["v"]?.GetValue<double?>() ?? 1
                            }.Execute(x);
                        });
                        break;
                    }
                case "offset": {
                        var taskbars = new List<Taskbar>(App.Settings.GraphicsRepeat ? new List<Taskbar>() { this } : App.taskbars);
                        taskbars.ForEach(x => {
                            var old = UnderlayOffset;
                            UnderlayOffset = Math.Max(Math.Min(message["v"]?.GetValue<int?>() ?? 0, 96), -96);
                            int nwh = App.layers is not null ? (App.layers.height - UnderlayOffset) : ((canvasManager.layers?.height ?? 48) - UnderlayOffset);
                            x.Height = nwh;
                        });

                        break;
                    }
                case "ui": {
                        webView.CoreWebView2.PostWebMessageAsString(JsonSerializer.Serialize(new CS2WVMessage<List<UIData>>("ui", UIDataForAllTaskbars())));
                        break;
                    }
                case "audio":
                    // request audio stream
                    // todo

                    break;
            }
        };
    }
    public static void SetupWebViews() {
        
        App.Current.Dispatcher.Invoke(() => {
            if (App.Settings.GraphicsRepeat) {
                App.taskbars.ForEach((t) => {
                    SetupWebView(t.webView, t.webViewReady_);
                    t.SetupWebViewMessageReceiver();
                });
            }
            else {
                if (App.hiddenWebViewHost is null) App.hiddenWebViewHost = new();
                App.hiddenWebViewHost.Show();
                SetupWebView(App.hiddenWebViewHost.webView_, App.webViewReady);
                App.taskbars.ForEach((t) => {
                    t.SetupWebViewMessageReceiver();
                    Task.Run(() => {
                        App.webViewReady.WaitOne();
                        App.webViewReady.ReleaseMutex();

                        App.Current.Dispatcher.Invoke(() => {

                            App.hiddenWebViewHost.Background = Brushes.Transparent;
                            var taskbars = App.taskbars;
                            App.hiddenWebViewHost.MinWidth = App.taskbars.Sum(t2 => t2.Width);
                            App.hiddenWebViewHost.MinHeight = App.taskbars.Max(t2 => t2.Height);

                            App.hiddenWebViewHost.Top = 19999;
                            App.hiddenWebViewHost.Left = 19999;

                            t.windowHelper.Duplicate(new WindowInteropHelper(App.hiddenWebViewHost).EnsureHandle());
                        });

                    });
                });
            }
        });
    }

    public static void SoftReset(bool startConfig = true, Config cfg = null) {
        if (cfg is null) cfg = App.Settings.SelectedConfig;
        if (cfg is InstructionConfig) {
            var config = cfg as InstructionConfig;
            
            // insane coding
            ;
            (new List<Instruction>(config.Data.RunOnceGroup.Instructions)).Concat(config.Data.LoopGroups.Select(x => x.Instructions).SelectMany(x => x)).ToList().ForEach((i) => {
                if (i is ImageInstruction) {
                    ((ImageInstruction) i).drawn = false;
                }
                if (i is ShapeInstruction) {
                    ((ShapeInstruction) i).drawn = false;
                }
                if (i is TextInstruction) {
                    ((TextInstruction) i).drawn = false;
                }
            });
        }

        if(cfg is not null && startConfig) cfg.Start();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
        windowHelper.TaskbarUIChanged -= uiChangedHandler;
    }
}