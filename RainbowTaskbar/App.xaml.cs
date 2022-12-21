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
using static System.Windows.Forms.Design.AxImporter;

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

            App.Config.StartThread();
            API.Start();
        }
        else {
            // Other processes
            Task.Run(async () => {
                await using var pipe = new PipeClient<string>("RainbowTaskbar Pipe");
                await pipe.ConnectAsync();
                await pipe.WriteAsync("OpenEditor");
                Exit();
            }).Wait();
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

        if (Config.GraphicsRepeat) {
            taskbars.ForEach(t => {
                t.canvasManager.layers = new LayerManager(t);
            });
        }
        else {
            App.layers = new LayerManager();
            App.layers.width = (int) taskbars.Sum(t => t.Width);
            App.layers.height = (int) taskbars[0].Height;
        }
    }

    public static void ReloadTaskbars() =>
        Current.Dispatcher.Invoke(() => {

            taskbars.ForEach(taskbar => {
                taskbar.Close();
            });

            taskbars = App.FindAllTaskbars();

            SetupTaskbars();


            new List<Instruction>(Config.Instructions).ForEach((i) => {
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
            App.Config.StopThread();
            Config.configStep = -1;
            App.Config.StartThread();
        });

    public new static void Exit() {
        trayWindow.TrayIcon.Dispose();

        taskbars.ForEach(t => {
            t.taskbarHelper.Radius = 0;
            t.taskbarHelper.UpdateRadius();

            t.Close();
            t.taskbarHelper.SetAlpha(1);
            TaskbarHelper.SendMessage(t.taskbarHelper.HWND, TaskbarHelper.WM_DWMCOMPOSITIONCHANGED, 1, null);
        });
        Current.Dispatcher.Invoke(() => { Current.Shutdown(); });
    }
}