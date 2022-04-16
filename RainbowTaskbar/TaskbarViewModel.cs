using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using RainbowTaskbar.Drawing;
using RainbowTaskbar.Helpers;
using RainbowTaskbar.WebSocketServices;

namespace RainbowTaskbar;

public class TaskbarViewModel {
    private readonly CancellationTokenSource cts;
    private readonly Taskbar Window;
    public int ConfigStep;
    private Thread DrawThread;
    private Thread ZThread;

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


        window.layers = new Layers(window) {
            MainDrawRectangles = new[] {
                window.RectLayer0, window.RectLayer1, window.RectLayer2, window.RectLayer3, window.RectLayer4,
                window.RectLayer5, window.RectLayer6, window.RectLayer7, window.RectLayer8, window.RectLayer9,
                window.RectLayer10, window.RectLayer11, window.RectLayer12, window.RectLayer13, window.RectLayer14,
                window.RectLayer15
            }
        };

        var Taskbar = window.taskbarHelper.GetRectangle();

        window.Width = Taskbar.Width - Taskbar.X;
        window.Height = Taskbar.Height - Taskbar.Y;

        foreach (var rect in window.layers.MainDrawRectangles) {
            rect.Width = Taskbar.Width - Taskbar.X;
            rect.Height = Taskbar.Height - Taskbar.Y;
        }

        foreach (var layer in window.layers.MainDrawRectangles)
            layer.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

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
                        if (App.APISubscribed.Count > 0) {
                            var data = new JObject();
                            data.Add("type", "InstructionStep");
                            data.Add("index", ConfigStep);
                            data.Add("instruction", App.Config.Instructions[ConfigStep].ToJSON());
                            WebSocketAPIServer.SendToSubscribed(data.ToString());
                        }

                        try {
                            if (App.Config.Instructions[ConfigStep].Execute(window, token)) slept = true;
                        }
                        catch {
                            MessageBox.Show(
                                $"The \"{App.Config.Instructions[ConfigStep].Name}\" instruction at index {ConfigStep} (starting from 0) threw an exception, it will be removed from the config.",
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