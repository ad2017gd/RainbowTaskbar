using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

using RainbowTaskbar.Drawing;
using RainbowTaskbar.Helpers;
using RainbowTaskbar.HTTPAPI;
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

        IntPtr thisHwnd = new WindowInteropHelper(window).Handle;
        TaskbarHelper.SetWindowLong(thisHwnd, TaskbarHelper.GWL_EXSTYLE, (uint) (TaskbarHelper.GetWindowLong(thisHwnd, TaskbarHelper.GWL_EXSTYLE).ToInt32() | 0x00000080L) /*WS_EX_TOOLWINDOW*/);

        var Taskbar = window.taskbarHelper.GetRectangle(true);
        window.Left = 0;
        window.Top = 0;
        double scale = window.taskbarHelper.GetScalingFactor();
        window.MinWidth = Taskbar.Width * (1/ scale);
        window.Width = Taskbar.Width * (1 / scale);
        window.MinHeight = Taskbar.Height * (1 / scale);
        window.Height = Taskbar.Height * (1 / scale);


        window.TaskbarClip.RadiusX = window.TaskbarClip.RadiusY = 0;
        window.TaskbarClip.Rect = new(0, 0, window.Width, window.Height);

        window.TaskbarClipHide.RadiusX = window.TaskbarClipHide.RadiusY = 0;
        window.TaskbarClipHide.Rect = new(0, 0, 0, 0);

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
        Window.windowHelper.alive = false;
        //cts.Cancel();
        //cts.Dispose();


    }
}