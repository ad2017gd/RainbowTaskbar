using Newtonsoft.Json.Linq;
using RainbowTaskbar.Configuration;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RainbowTaskbar
{
    public class TaskbarViewModel
    { 
        CancellationTokenSource cts;
        Thread DrawThread;
        Thread ZThread;
        public int ConfigStep = 0;
        Taskbar Window;

        public TaskbarViewModel(Taskbar window)
        {
            this.Window = window;
            window.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            window.taskbarHelper = new Helpers.TaskbarHelper(Helpers.TaskbarHelper.FindWindow(window.Secondary ? "Shell_SecondaryTrayWnd" : "Shell_TrayWnd", null), window.Secondary);
            window.windowHelper = new Helpers.WindowHelper(window, window.taskbarHelper);
            window.taskbarHelper.window = window;
            window.taskbarHelper.PositionChangedHook();
            window.taskbarHelper.UpdateRadius();


            window.layers = new Drawing.Layers(window);
            window.layers.MainDrawRectangles = new System.Windows.Shapes.Rectangle[] { window.RectLayer0, window.RectLayer1, window.RectLayer2, window.RectLayer3, window.RectLayer4, window.RectLayer5, window.RectLayer6, window.RectLayer7, window.RectLayer8, window.RectLayer9, window.RectLayer10, window.RectLayer11, window.RectLayer12, window.RectLayer13, window.RectLayer14, window.RectLayer15 };

            var Taskbar = window.taskbarHelper.GetRectangle();

            window.Width = Taskbar.Width - Taskbar.X;
            window.Height = Taskbar.Height - Taskbar.Y;

            foreach (System.Windows.Shapes.Rectangle rect in window.layers.MainDrawRectangles)
            {
                rect.Width = Taskbar.Width - Taskbar.X;
                rect.Height = Taskbar.Height - Taskbar.Y;
            }

            foreach (Rectangle layer in window.layers.MainDrawRectangles)
            {
                layer.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }

            cts = new CancellationTokenSource();
            LoadConfig(window);
        }

        public void LoadConfig(Taskbar window)
        {
            var token = cts.Token;
            DrawThread = new Thread(new ThreadStart(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    var slept = false;

                    for (ConfigStep = 0; ConfigStep < App.Config.Instructions.Count && !token.IsCancellationRequested; ConfigStep++)
                    {
                        if (App.APISubscribed.Count > 0)
                        {
                            JObject data = new JObject();
                            data.Add("type", "InstructionStep");
                            data.Add("index", ConfigStep);
                            data.Add("instruction", App.Config.Instructions[ConfigStep].ToJSON());
                            WebSocketServices.WebSocketAPIServer.SendToSubscribed(data.ToString());
                        }
                        if (App.Config.Instructions[ConfigStep].Execute(window, token)) slept = true;

                    }
                    if (!slept) break;
                }
            }))
            { IsBackground = true };

            ZThread = new Thread(new ThreadStart(() =>
            {
                Stopwatch stopwatch = new();
                stopwatch.Start();
                while (!token.IsCancellationRequested)
                {
                    Helpers.TaskbarHelper taskbar = null;
                    window.Dispatcher.Invoke(() =>
                    {
                        taskbar = window.taskbarHelper;
                    });

                    taskbar.UpdateRadius();

                    taskbar.SetWindowZUnder(window);
                    taskbar.SetBlur();
                    token.WaitHandle.WaitOne(40);
                }
                stopwatch.Stop();
            }))
            { IsBackground = true };

            DrawThread.Start();
            ZThread.Start();
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            Window.taskbarHelper.PositionChangedUnhook();
            Task.Run(() =>
            {
                cts.Cancel();
                DrawThread.Join();
                ZThread.Join();
                cts.Dispose();

                
            });
        }

    }
}
