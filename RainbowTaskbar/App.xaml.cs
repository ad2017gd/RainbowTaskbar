using H.Pipes;
using Microsoft.Web.WebView2.WinForms;
using PropertyChanged;
using RainbowTaskbar.Configuration;
using RainbowTaskbar.Configuration.Instruction;
using RainbowTaskbar.Configuration.Web;
using RainbowTaskbar.Drawing;
using RainbowTaskbar.Editor;
using RainbowTaskbar.Helpers;
using RainbowTaskbar.HTTPAPI;
using RainbowTaskbar.Languages;
using RainbowTaskbar.Preferences;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Localization = RainbowTaskbar.Languages.Localization;

namespace RainbowTaskbar;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
    [DllImport("KERNEL32.DLL", EntryPoint = "SetProcessWorkingSetSize", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool SetProcessWorkingSetSize(IntPtr pProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);

    [DllImport("KERNEL32.DLL", EntryPoint = "GetCurrentProcess", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr GetCurrentProcess();



    [StructLayout(LayoutKind.Sequential)]
    struct Point {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MOUSEHOOKSTRUCT {
        public Point pt;
        public IntPtr hwnd;
        public uint wHitTestCode;
        public IntPtr dwExtraInfo;
    }

    public delegate int MouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    public static extern int SetWindowsHookEx(int idHook, MouseProc lpfn, int hInstance, int threadId);

    [DllImport("user32", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

    static int mouseHookId = -1;
    static MouseProc callback = new MouseProc(HookCallback);
    const int WH_MOUSE_LL = 14;

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT {
        public int X;
        public int Y;
    }
    [DllImport("user32.dll")]
    static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);
    static DateTime last = DateTime.MinValue;
    [DllImport("user32.dll", EntryPoint = "RealChildWindowFromPoint", SetLastError = false)]
    public static extern IntPtr RealChildWindowFromPoint(IntPtr hwndParent, int x, int y);
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool UnhookWindowsHookEx(IntPtr hhk);

    public static IntPtr FindLastChildAtPoint(IntPtr parent, int x, int y) {
        IntPtr neww = RealChildWindowFromPoint(parent, x, y);
        if (neww != 0 && neww != parent) return FindLastChildAtPoint(neww, x, y);
        else return parent;
    }
    private static int HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
        var mhs = Marshal.PtrToStructure<MOUSEHOOKSTRUCT>(lParam);

        
        if (nCode >= 0) {

            if (wParam == 0x0201 && trayWindow.TrayIcon.ContextMenu.IsOpen || wParam == 0x0204) return CallNextHookEx(mouseHookId, nCode, wParam, lParam);
            var farLeft = (int) (App.taskbars.Count > 0 ? App.taskbars.Min(x => x.Left) : 0);
            App.taskbars.ForEach(x => {
                if (x.webView is null) return;

                // passing WM_LBUTTONDOWN interferes with tray icon, too bad
                
                var pn = System.Windows.Forms.Control.MousePosition;
                var scale = x.windowHelper.scale;
                if (!new System.Drawing.Rectangle(new((int) (x.Left * scale), (int) (x.Top * scale)), new((int) (x.ActualWidth * scale), (int) (x.ActualHeight * scale))).Contains(pn)) return;

                POINT p = new POINT { X = (int) (pn.X), Y = (int) (pn.Y ) };
                IntPtr cch = FindLastChildAtPoint(x.webView.Handle, 0, 0);
                ScreenToClient(App.Settings.GraphicsRepeat ? cch : x.windowHelper.HWND, ref p);
                if (!App.Settings.GraphicsRepeat) p.X += (int) x.Left - farLeft;
                PostMessage(cch, (uint) wParam, 0x0000, (IntPtr) (((uint) (p.Y) << 16) | ((((ushort) (p.X)) & 0xFFFF))));
            });

            
        }

        return CallNextHookEx(mouseHookId, nCode, wParam, lParam);
    }


    public static List<Taskbar> taskbars = new();
    public static bool monacoExtracted = false;
    public static EditorWindow editor = null;
    public static TrayWindow trayWindow = (TrayWindow) Current.MainWindow;
    public static Mutex mutex = new(true, "RainbowTaskbar Mutex");
    public static Localization localization;
    public static EditorViewModel editorViewModel = new();
    public static bool firstRun = false;
    public static string rainbowTaskbarDir = Path.Join(Environment.GetEnvironmentVariable("AppData"), "RainbowTaskbar");
    public static string configDir = Path.Join(rainbowTaskbarDir, "configurations");
    public static string monacoDir = Path.Join(rainbowTaskbarDir, "monaco");
    public static LayerManager layers = null;
    public static Random rnd = new();
    public static HiddenWebViewHost hiddenWebViewHost = null;
    public static Microsoft.Web.WebView2.Wpf.WebView2 webView { get => hiddenWebViewHost?.webView_; }
    public static Mutex webViewReady = new Mutex();
    public static int farLeft;

    public static List<Config> AllConfigsFromFiles() {
        List<Config> configs = new();

        if(!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);
        foreach (string f in Directory.EnumerateFiles(configDir)) {
            try {
                Config cfg = Config.FromFile(f);
                cfg.fileName = f;
                configs.Add(cfg);
            } catch { }
        }
        return configs;
    }

    public static ObservableCollection<Config> Configs { get; set; } = new ObservableCollection<Config>(AllConfigsFromFiles().OrderBy(x => x.Created).Reverse());
    public static Settings Settings { get; set; } = Settings.FromFile();

    public static void LaunchEditor() {
        if (editor == null) {
            editor = new EditorWindow();
        }
        editor.Show();
        editor.WindowState = WindowState.Normal;
        editor.Activate();
        editor.BringIntoView();
        editor.Focus();
    }
    public App() {
        localization = new Localization();
    }
    
   

    public new static void Exit() {
        if(trayWindow is not null) trayWindow.TrayIcon.Dispose();

        StopHook();

        taskbars.ForEach(t => {
            t.taskbarHelper.Radius = 0;
            t.taskbarHelper.UpdateRadius();

            t.Close();
            t.taskbarHelper.SetAlpha(1);
            TaskbarHelper.SendMessage(t.taskbarHelper.HWND, TaskbarHelper.WM_DWMCOMPOSITIONCHANGED, 1, null);
            t.taskbarHelper.Style = TaskbarHelper.TaskbarStyle.ForceDefault;
            t.taskbarHelper.SetBlur();
            // win11 fix
            ExplorerTAP.ExplorerTAP.Reset();
        });
        Current.Dispatcher.Invoke(() => { Current.Shutdown(); });
    }

    public static void StartHook() {
        if (mouseHookId != -1) return;
        mouseHookId = SetWindowsHookEx(WH_MOUSE_LL, callback, 0, 0);
    }
    public static void StopHook() {
        UnhookWindowsHookEx(mouseHookId);
        mouseHookId = -1;
    }
    private static bool isappfullscreen = false;
    public static bool IsAppFullscreen { get => isappfullscreen; set {
            isappfullscreen = value;
            if(value) {
                App.taskbars.ForEach(x => x.Hide());
            } else {
                App.taskbars.ForEach(x => x.Show());
            }
        } }

    public static bool IsAppMicrosoftStore { get => IsMicrosoftStore(); }
    public static bool IsMicrosoftStore() {
        return System.Environment.ProcessPath.ToLower().StartsWith(@"c:\program files\windowsapps");
       
    }
    private void Application_Startup(object sender, StartupEventArgs e) {
        foreach (string s in e.Args) {
        }


        if (mutex.WaitOne(TimeSpan.Zero, true)) {
            Task.Run(async () => {
                await using var pipe = new PipeServer<string>("RainbowTaskbar Pipe");
                pipe.MessageReceived += (sender, args) => {
                    if (args.Message == "OpenEditor")
                        Dispatcher.Invoke(() => {
                            LaunchEditor();
                        });
                };
                await pipe.StartAsync();
                await Task.Delay(Timeout.InfiniteTimeSpan);
            });



            if (!Settings.GraphicsRepeat) {
                webViewReady.WaitOne();
                //App.webView = new();
            }


            App.localization.Switch(Settings.language);
            if (Settings.CheckUpdate && !IsMicrosoftStore()) AutoUpdate.CheckForUpdate();


            // port old config bleh

            var oldconfigpath = Environment.ExpandEnvironmentVariables("%appdata%/rnbconf.xml");
            var nwoldconfigpath = Environment.ExpandEnvironmentVariables("%appdata%/rnbconf.bak.xml");
            if (File.Exists(oldconfigpath)) {
                var res = MessageBox.Show(App.localization.Get("msgbox_migrate"), "RainbowTaskbar", MessageBoxButton.YesNo);

                if (res == MessageBoxResult.Yes) {
                    File.WriteAllText(oldconfigpath, File.ReadAllText(oldconfigpath).Replace("RainbowTaskbar.Configuration", "RainbowTaskbar.V2Legacy.Configuration"));


                    using var fileStream = new FileStream(oldconfigpath, FileMode.OpenOrCreate);
                    using var reader = XmlDictionaryReader.CreateTextReader(fileStream, new XmlDictionaryReaderQuotas());

                    var serializerSettings = new DataContractSerializerSettings {
                        PreserveObjectReferences = true
                    };

                    var serializer = new DataContractSerializer(typeof(V2Legacy.Configuration.Config), serializerSettings);
                    var cfg = serializer.ReadObject(reader) as V2Legacy.Configuration.Config;

                    var curcfg = InstructionConfig.FromLegacyConfig(cfg);
                    curcfg.ToFile();
                    App.Configs.Add(curcfg);
                    foreach (var preset in cfg.Presets) {
                        var portcfg = InstructionConfig.FromLegacyPreset(preset);
                        portcfg.ToFile();
                        App.Configs.Add(portcfg);
                    }

                    reader.Close();
                    fileStream.Close();

                    File.Delete(oldconfigpath);
                } else {
                    File.Delete(oldconfigpath);
                }
            }

            

            



            Settings.workshopAPI = new WorkshopAPI() {};

            Settings.OnLoginKeyChanged();

            if (Settings.FirstStart) {
                Settings.FirstStart = false;
                Settings.ToFile();
                firstRun = true;
            }
            //TODO:remove this its for debugfging
            // LaunchEditor();

            if (Settings.WebTouchThrough) StartHook();

            if (App.Settings.Version < Assembly.GetExecutingAssembly().GetName().Version) {
                if (App.Settings.Version > new Version("1.0")) {
                    // Valid update
                    if (App.Settings.Version <= new Version("3.1.3") && App.IsMicrosoftStore()) {
                        App.Settings.RunAtStartup = true;
                    }
                }

                App.Settings.Version = Assembly.GetExecutingAssembly().GetName().Version;
                App.Settings.SaveChanged();
            }

            Task.Run(() => {
                ExplorerTAP.ExplorerTAP.TryInject();

                App.Current.Dispatcher.Invoke(() => {
                    taskbars = FindAllTaskbars();
                    SetupTaskbars();

                    Taskbar.SetupLayers();
                    Taskbar.SetupWebViews();

                    if(App.Settings.SelectedConfig is not null) App.Settings.SelectedConfig.Start();

                    Settings.OnGlobalOpacityChanged();



                    if (firstRun == true) {
                        Task.Run(() => {
                            Thread.Sleep(1000); // amazing coding
                            Dispatcher.Invoke(() => LaunchEditor());
                        });
                    }

                    // todo: add back?
                   // API.Start();
                });



            });


        }
        else {
            // Other processes
            var pipe = new PipeClient<string>("RainbowTaskbar Pipe");
            pipe.ConnectAsync().Wait();
            pipe.WriteAsync("OpenEditor").Wait();
            pipe.DisconnectAsync().Wait();
            Environment.Exit(0);
        }
    }

    public delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

    [DllImport("user32.dll")]
    private static extern int EnumWindows(EnumWindowsProc proc, int lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    public static List<Taskbar> FindAllTaskbars() {
        List<Taskbar> tsk = new List<Taskbar>();

        var nw = new Taskbar(TaskbarHelper.FindWindow("Shell_TrayWnd", null));
        nw.Show();
        tsk.Add(nw);

        EnumWindows(new EnumWindowsProc((hWnd, lParam) =>
        {
            StringBuilder className = new StringBuilder(255);
            GetClassName(hWnd, className, 255);
            if (className.ToString() == "Shell_SecondaryTrayWnd") {
                var newWindow = new Taskbar(hWnd, true);
                newWindow.Show();
                tsk.Add(newWindow);
            }

            return true;
        }), 0);
        
        return tsk;
    }
    public static void SetupTaskbars() {
        taskbars.MinBy(t => t.Left).taskbarHelper.first = true;
        taskbars.MaxBy(t => t.Left).taskbarHelper.last = true;
        taskbars.ForEach(t => {
            t.taskbarHelper.UpdateRadius();
            int fals = 1;
            TaskbarHelper.DwmSetWindowAttribute(new WindowInteropHelper(t).EnsureHandle(), TaskbarHelper.DWMWINDOWATTRIBUTE.ExcludedFromPeek, ref fals, sizeof(int));
        });



    }
    public static void ReloadTaskbars(bool startConfig = true) =>
        Current.Dispatcher.Invoke(() => {
            if (App.hiddenWebViewHost is not null) {
                if (App.hiddenWebViewHost.webView_ is not null) {
                    App.hiddenWebViewHost.webView_.Dispose();
                    App.hiddenWebViewHost.webView_ = null;
                }
                App.hiddenWebViewHost.Close();
                App.hiddenWebViewHost = null;
            }
            if (!App.Settings.GraphicsRepeat) {
                webViewReady.WaitOne();
                App.hiddenWebViewHost = new();
            }
            taskbars.ForEach(taskbar => {
                if(taskbar.webView_ is not null) taskbar.webView_.Dispose();
                taskbar.webView_ = null;
                taskbar.windowHelper.RemoveDuplicate();
                taskbar.Close();
            });

            Config.currentlyRunning = null;

            taskbars = FindAllTaskbars();

            SetupTaskbars();
            Taskbar.SetupLayers();
            Taskbar.SetupWebViews();

            Task.Run(() => {
                Thread.Sleep(500);
                App.Current.Dispatcher.Invoke(() => Taskbar.SoftReset(startConfig));
            });
        });
}