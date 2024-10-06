using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

using RainbowTaskbar.Drawing;
using RainbowTaskbar.Helpers;
using RainbowTaskbar.HTTPAPI;
using RainbowTaskbar.V2Legacy.Configuration;
using RainbowTaskbar.V2Legacy.Configuration.Instructions;
using RainbowTaskbar.WebSocketServices;

namespace RainbowTaskbar;

/// <summary>
///     Interaction logic for Taskbar.xaml
/// </summary>
public partial class Taskbar : Window {
    // TODO: config-specific
    //public CanvasManager canvasManager;
    public bool secondary;
    public TaskbarHelper taskbarHelper;
    public TaskbarViewModel viewModel;
    public WindowHelper windowHelper;

    public Taskbar(IntPtr HWND, bool secondary) {
        InitializeComponent();
        this.secondary = secondary;

        viewModel = new TaskbarViewModel(this, HWND);
        Closing += viewModel.OnWindowClosing;
        DataContext = viewModel;
    }


    public Taskbar(IntPtr HWND) {
        InitializeComponent();

        viewModel = new TaskbarViewModel(this, HWND);
        Closing += viewModel.OnWindowClosing;
        DataContext = viewModel;
    }

    #region Ugly win32 message handler
    
    public IntPtr hwndInsertAfter = IntPtr.Zero;
    public struct WINDOWPOS {
        public IntPtr hwnd;
        public IntPtr hwndInsertAfter;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public uint flags;
    }
    public const int WM_WINDOWPOSCHANGING = 0x46;
    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
        if(msg == WM_WINDOWPOSCHANGING) {
            WINDOWPOS wp = (WINDOWPOS) System.Runtime.InteropServices.Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
            if (wp.hwndInsertAfter != hwndInsertAfter) {
                taskbarHelper.SetWindowZUnder(this);
                taskbarHelper.SetBlur();
                hwndInsertAfter = taskbarHelper.HWND;
            }
        }
            return IntPtr.Zero;
    }

    protected override void OnSourceInitialized(EventArgs e) {
        base.OnSourceInitialized(e);
        HwndSource source = (HwndSource) HwndSource.FromHwnd(new WindowInteropHelper(this).EnsureHandle());
        source.AddHook(WndProc);
    }
    #endregion

    private void RainbowTaskbar_Closed(object sender, EventArgs e) {
        taskbarHelper.Style = TaskbarHelper.TaskbarStyle.ForceDefault;
        taskbarHelper.SetBlur();
    }


    public static void SetupLayers() {
        // TODO: config-specifc
       // App.Current.Dispatcher.Invoke(() => {
       //     if (App.Config.GraphicsRepeat) {
       //         App.taskbars.ForEach(t => {
       //             t.canvasManager.layers = new LayerManager(t);
       //         });
       //     }
       //     else {
       //         App.layers = new LayerManager();
       //         App.layers.width = (int) App.taskbars.Sum(t => t.Width);
       //         App.layers.height = (int) App.taskbars[0].Height;
       //     }
       // });
    }

    public static void SoftReset() {
        // TODO: config-specific once again
       // SetupLayers();
       // new List<Instruction>(App.Config.Instructions).ForEach((i) => {
       //     if (i is ImageInstruction) {
       //         ((ImageInstruction) i).drawn = false;
       //     }
       //     if (i is ShapeInstruction) {
       //         ((ShapeInstruction) i).drawn = false;
       //     }
       //     if (i is TextInstruction) {
       //         ((TextInstruction) i).drawn = false;
       //     }
       // });
       // 
       // App.Config.StopThread();
       // App.Config.configStep = -1;
       // App.Config.StartThread();
    }
}