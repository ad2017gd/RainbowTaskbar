using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using H.Pipes;
using PropertyChanged;
using RainbowTaskbar.Configuration;
using RainbowTaskbar.Helpers;
using RainbowTaskbar.HTTPAPI;

namespace RainbowTaskbar;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
    public static Random rnd = new();

    //public static HttpServer http;
    public static List<Taskbar> taskbars = new();

    //public static List<string> APISubscribed = new();
    public static Mutex mutex = new(true, "RainbowTaskbar Mutex");

    public App() {
        if (mutex.WaitOne(TimeSpan.Zero, true)) {
            // First process
            // todo: multiple taskbar, refractor and clean up code, newtonsoft.json to datacontract json
            Task.Run(async () => {
                await using var pipe = new PipeServer<string>("RainbowTaskbar Pipe");
                pipe.MessageReceived += (sender, args) => {
                    if (args.Message == "OpenEditor")
                        Current.Dispatcher.Invoke(() => {
                            MainWindow.Show();
                            MainWindow.WindowState = WindowState.Normal;
                            MainWindow.Activate();
                            MainWindow.BringIntoView();
                            MainWindow.Focus();
                            MainWindow.Topmost = true;
                            MainWindow.Topmost = false;
                        });
                };
                await pipe.StartAsync();
                await Task.Delay(Timeout.InfiniteTimeSpan);
            });

            Config = Config.FromFile();
            if (Config.CheckUpdate) AutoUpdate.CheckForUpdate();
            API.Start();
            if (TaskbarHelper.FindWindow("Shell_SecondaryTrayWnd", null) != (IntPtr) 0) {
                var newWindow = new Taskbar(true);
                newWindow.Show();

                taskbars.Add(newWindow);
            }
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

    public static void ReloadTaskbars() =>
        Current.Dispatcher.Invoke(() => {
            taskbars = taskbars.Select(taskbar => {
                taskbar.Close();
                return new Taskbar(taskbar.Secondary);
            }).ToList();
            taskbars.ForEach(taskbar => taskbar.Show());
        });

    public new static void Exit() {
        (Current.MainWindow as Editor)?.TrayIcon?.Dispose();

        taskbars.ForEach(t => {
            t.taskbarHelper.Radius = 0;
            t.taskbarHelper.UpdateRadius();
            t.Close();
            Task.Run(() => {
                t.viewModel.ZThread.Join();
                t.viewModel.DrawThread.Join();

                t.taskbarHelper.SetAlpha(1);
                TaskbarHelper.SendMessage(t.taskbarHelper.HWND, TaskbarHelper.WM_DWMCOMPOSITIONCHANGED, 1, null);

                Current.Dispatcher.Invoke(() => { Current.Shutdown(); });
            });
        });
    }
}