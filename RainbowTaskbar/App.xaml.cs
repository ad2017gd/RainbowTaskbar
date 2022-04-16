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
using RainbowTaskbar.WebSocketServices;
using WebSocketSharp.Server;

namespace RainbowTaskbar;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
    public static WebSocketServer ws;
    public static Random rnd = new();
    public static HttpServer http;
    public static List<Taskbar> taskbars = new();
    public static List<string> APISubscribed = new();
    public static Mutex mutex = new(true, "RainbowTaskbar Mutex");

    public App() {
        if (mutex.WaitOne(TimeSpan.Zero, true)) {
            // First process
            // todo: preset manager, multiple taskbar, refractor and clean up code
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
            ConfigureServer();
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

    public static void ConfigureServer() {
        http?.Stop();

        http = Config.APIPort is > 0 and < 65536 ? new HttpServer(Config.APIPort) : new HttpServer(9093);

        http.AddWebSocketService<WebSocketAPIServer>("/rnbws");
        http.OnGet += HTTPAPIServer.Get;
        http.OnPost += HTTPAPIServer.Post;
        http.OnOptions += HTTPAPIServer.Options;
        http.KeepClean = false;

        if (Config.IsAPIEnabled) http.Start();
    }

    public new static void Exit() {
        (Current.MainWindow as Editor)?.TrayIcon?.Dispose();

        taskbars.ForEach(t => {
            t.taskbarHelper.Radius = 0;
            t.taskbarHelper.UpdateRadius();
            t.Close();
            TaskbarHelper.SendMessage(t.taskbarHelper.HWND, TaskbarHelper.WM_DWMCOMPOSITIONCHANGED, 1, null);
            t.taskbarHelper.SetAlpha(1);
        });

        Current.Shutdown();
    }
}