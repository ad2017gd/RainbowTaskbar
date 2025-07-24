using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Wpf;
using RainbowTaskbar.Configuration;
using RainbowTaskbar.Configuration.Instruction;
using RainbowTaskbar.Configuration.Instruction.Instructions;
using RainbowTaskbar.Drawing;
using RainbowTaskbar.Helpers;
using RainbowTaskbar.HTTPAPI;
using Wpf.Ui.Controls;
using System.Text.Json.Nodes;

using System.Text.Json;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using Microsoft.Web.WebView2.Core;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using WebView2 = Microsoft.Web.WebView2.Wpf.WebView2;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
            if ((uint) wParam == 53) 
                App.IsAppFullscreen = true;
            if ((uint) wParam == 54) 
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
        HwndSource source = (HwndSource) HwndSource.FromHwnd(new WindowInteropHelper(this).EnsureHandle());
        if(!secondary) {
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
                        webView.CoreWebView2.IsMuted = false;



                        webView.CoreWebView2.MemoryUsageTargetLevel = Microsoft.Web.WebView2.Core.CoreWebView2MemoryUsageTargetLevel.Low;

                        webView.CoreWebView2.ProcessFailed += (_, _) => {
                            if (!App.Settings.GraphicsRepeat) {
                                App.hiddenWebViewHost = new();
                            }
                            App.ReloadTaskbars();
                        };
                    });
                });
            });
        });

        
    }
    public void SetupWebViewMessageReceiver() {
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

                            App.hiddenWebViewHost.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
                            App.hiddenWebViewHost.Width = App.taskbars.Sum(t2 => t2.Width);
                            App.hiddenWebViewHost.Height = App.taskbars.Max(t2 => t2.Height);
                            var ttt = App.taskbars.Max(t2 => t2.Height);

                            App.hiddenWebViewHost.Top = 9999;
                            App.hiddenWebViewHost.Left = 9999;

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
    }
}