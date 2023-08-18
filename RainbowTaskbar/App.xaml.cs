using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using H.Pipes;
using PropertyChanged;
using RainbowTaskbar.Configuration;
using RainbowTaskbar.Configuration.Instructions;
using RainbowTaskbar.Drawing;
using RainbowTaskbar.Helpers;
using RainbowTaskbar.HTTPAPI;
using RainbowTaskbar.Languages;
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



    public static Random rnd = new();

    //public static HttpServer http;
    public static List<Taskbar> taskbars = new();

    public static Editor editor = null;

    public static TrayWindow trayWindow = (TrayWindow) Current.MainWindow;

    public static EditorViewModel editorViewModel = null;

    public static LayerManager layers = null;

    public JsonSerializerOptions jsonSerializerOptions = null;

    //public static List<string> APISubscribed = new();
    public static Mutex mutex = new(true, "RainbowTaskbar Mutex");

    public static Localization localization;

    public static List<Taskbar> FindAllTaskbars() {
        List<Taskbar> tsk = new List<Taskbar>();

        var nw = new Taskbar(TaskbarHelper.FindWindow("Shell_TrayWnd", null));
        nw.Show();
        tsk.Add(nw);
        IntPtr next = IntPtr.Zero;
        while (true) {
            next = TaskbarHelper.FindWindowEx(IntPtr.Zero, next, "Shell_SecondaryTrayWnd", null);
            if (next == IntPtr.Zero) break;
            var newWindow = new Taskbar(next, true);
            newWindow.Show();
            tsk.Add(newWindow);
        }

        return tsk;
    }
    
    public App() {
        localization = new Localization();
        

        if (mutex.WaitOne(TimeSpan.Zero, true)) {
            Task.Run(async () => {
                await using var pipe = new PipeServer<string>("RainbowTaskbar Pipe");
                pipe.MessageReceived += (sender, args) => {
                    if (args.Message == "OpenEditor")
                        Current.Dispatcher.Invoke(() => {
                            if (editor == null) {
                                editor = new Editor();
                            }
                            editor.Show();
                            editor.WindowState = WindowState.Normal;
                            editor.Activate();
                            editor.BringIntoView();
                            editor.Focus();
                            editor.Topmost = true;
                            editor.Topmost = false;
                        });
                };
                await pipe.StartAsync();
                await Task.Delay(Timeout.InfiniteTimeSpan);
            });


            editorViewModel = new EditorViewModel();




            Config = Config.FromFile();
            if (Config.CheckUpdate) AutoUpdate.CheckForUpdate();

            taskbars = FindAllTaskbars();

            SetupTaskbars();

            Taskbar.SetupLayers();

            if(taskbars.Count > 0)
                ExplorerTAP.ExplorerTAP.TryInject();

            App.Config.StartThread();
            API.Start();

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

    [OnChangedMethod(nameof(ReloadTaskbars))]
    public static Config Config { get; set; }

    public static void SetupTaskbars() {
        taskbars.MinBy(t => t.Left).taskbarHelper.first = true;
        taskbars.MaxBy(t => t.Left).taskbarHelper.last = true;
        taskbars.ForEach(t => {
            t.taskbarHelper.UpdateRadius();
            int fals = 1;
            TaskbarHelper.DwmSetWindowAttribute(new WindowInteropHelper(t).EnsureHandle(), TaskbarHelper.DWMWINDOWATTRIBUTE.ExcludedFromPeek, ref fals, sizeof(int));
        });

        Task.Factory.StartNew(() => {
            while(TaskbarHelper.IsWindow(taskbars.First(x => !x.secondary).taskbarHelper.HWND)) {
                Thread.Sleep(1000);
            }
            while (TaskbarHelper.FindWindow("Shell_TrayWnd", null) == IntPtr.Zero) {
                Thread.Sleep(1000);
            }
            Thread.Sleep(2500);

            ReloadTaskbars();
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);


    }

    public static void ReloadTaskbars() =>
        Current.Dispatcher.Invoke(() => {


            taskbars.ForEach(taskbar => {
                taskbar.Close();
            });

            taskbars = App.FindAllTaskbars();

            SetupTaskbars();

            Taskbar.SoftReset();


            
        });

    public new static void Exit() {
        if(trayWindow is not null) trayWindow.TrayIcon.Dispose();

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
}