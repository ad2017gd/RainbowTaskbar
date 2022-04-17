using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using RainbowTaskbar.Drawing;
using RainbowTaskbar.Helpers;
using RainbowTaskbar.HTTPAPI;
using RainbowTaskbar.WebSocketServices;

namespace RainbowTaskbar;

public class TaskbarViewModel {
    private readonly CancellationTokenSource cts;
    private readonly Taskbar Window;
    public int ConfigStep;
    public Thread DrawThread;
    public Thread ZThread;


    public TaskbarViewModel(Taskbar window) {
        Window = window;
        window.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

        window.taskbarHelper =
            new TaskbarHelper(
                TaskbarHelper.FindWindow(window.Secondary ? "Shell_SecondaryTrayWnd" : "Shell_TrayWnd", null),
                window.Secondary);
        window.windowHelper = new WindowHelper(window, window.taskbarHelper);
        window.taskbarHelper.window = window;
        window.taskbarHelper.PositionChangedHook();
        window.taskbarHelper.UpdateRadius();

        var Taskbar = window.taskbarHelper.GetRectangle();

        window.Width = Taskbar.Width - Taskbar.X;
        window.Height = Taskbar.Height - Taskbar.Y;

        window.layers = new Layers(window, new Canvas[] {
                window.Layer0, window.Layer1, window.Layer2, window.Layer3, window.Layer4, window.Layer5,
                window.Layer6, window.Layer7, window.Layer8, window.Layer9, window.Layer10, window.Layer11,
                window.Layer12, window.Layer13, window.Layer14, window.Layer15
            });

        foreach (var layer in window.layers.canvases)
            layer.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

        cts = new CancellationTokenSource();
        LoadConfig(window);
    }

    public void LoadConfig(Taskbar window) {
        var token = cts.Token;
        DrawThread = new Thread(() => {
                while (!token.IsCancellationRequested) {
                    var slept = false;

                    for (ConfigStep = 0;
                         ConfigStep < App.Config.Instructions.Count && !token.IsCancellationRequested;
                         ConfigStep++) {
                        if (API.APISubscribed.Count > 0) {
                            var data = new JObject();
                            data.Add("type", "InstructionStep");
                            data.Add("index", ConfigStep);
                            data.Add("instruction", App.Config.Instructions[ConfigStep].ToJSON());
                            WebSocketAPIServer.SendToSubscribed(data.ToString());
                        }

                        try {
                            if (App.Config.Instructions[ConfigStep].Execute(window, token)) slept = true;
                        }
                        catch (Exception e) {
                            MessageBox.Show(
                                $"The \"{App.Config.Instructions[ConfigStep].Name}\" instruction at index {ConfigStep} (starting from 0) threw an exception, it will be removed from the config.\n${e.Message}",
                                "RainbowTaskbar", MessageBoxButton.OK, MessageBoxImage.Error);
                            Application.Current.Dispatcher.Invoke(() => {
                                App.Config.Instructions.RemoveAt(ConfigStep);
                                App.Config.ToFile();
                                App.ReloadTaskbars();
                            });
                            return;
                        }
                    }

                    if (!slept) break;
                }
            })
            {IsBackground = true};

        ZThread = new Thread(() => {
                Stopwatch stopwatch = new();
                stopwatch.Start();
                while (!token.IsCancellationRequested) {
                    TaskbarHelper taskbar = null;
                    window.Dispatcher.Invoke(() => { taskbar = window.taskbarHelper; });

                    taskbar.UpdateRadius();

                    taskbar.SetWindowZUnder(window);
                    taskbar.SetBlur();
                    token.WaitHandle.WaitOne(40);
                }

                stopwatch.Stop();
            })
            {IsBackground = true};

        DrawThread.Start();
        ZThread.Start();
    }

    public void OnWindowClosing(object sender, CancelEventArgs e) {
        Window.taskbarHelper.PositionChangedUnhook();
        Task.Run(() => {
            cts.Cancel();
            DrawThread.Join();
            ZThread.Join();
            cts.Dispose();
        });
    }
}