using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using RainbowTaskbar.Drawing;
using RainbowTaskbar.Helpers;
using RainbowTaskbar.HTTPAPI;
using RainbowTaskbar.WebSocketServices;

namespace RainbowTaskbar;

public class TaskbarViewModel {
    //public readonly CancellationTokenSource cts;
    private readonly Taskbar Window;
    //public Thread drawThread;
    //public Thread zThread;


    public TaskbarViewModel(Taskbar window, IntPtr HWND) {
        Window = window;
        window.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

        window.taskbarHelper =
            new TaskbarHelper(
                HWND,
                window.secondary);
        window.windowHelper = new WindowHelper(window, window.taskbarHelper);
        window.taskbarHelper.window = window;
        window.taskbarHelper.PositionChangedHook();
        window.taskbarHelper.UpdateRadius();

        var Taskbar = window.taskbarHelper.GetRectangle();

        window.Width = Taskbar.Width;
        window.Height = Taskbar.Height;
        if (HWND == IntPtr.Zero) return;
        window.canvasManager = new CanvasManager(window, new Canvas[] {
                window.Layer0, window.Layer1, window.Layer2, window.Layer3, window.Layer4, window.Layer5,
                window.Layer6, window.Layer7, window.Layer8, window.Layer9, window.Layer10, window.Layer11,
                window.Layer12, window.Layer13, window.Layer14, window.Layer15
            });

        foreach (var cvs in window.canvasManager.canvases) {
            cvs.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }
       
        

        //cts = new CancellationTokenSource();
        //StartZThread(window);
    }
    /*
    public void StartZThread(Taskbar window) {
        var token = cts.Token;
        

        zThread = new Thread(() => {
                while (!token.IsCancellationRequested) {
                    TaskbarHelper taskbar = null;
                    window.Dispatcher.Invoke(() => { taskbar = window.taskbarHelper; });

                    taskbar.SetWindowZUnder(window);
                    taskbar.SetBlur();
                    token.WaitHandle.WaitOne(100);
                }
            })
            {IsBackground = true};

        zThread.Start();
    }
    */
    public void OnWindowClosing(object sender, CancelEventArgs e) {
        Window.taskbarHelper.PositionChangedUnhook();
        //cts.Cancel();
        //cts.Dispose();
        

    }
}