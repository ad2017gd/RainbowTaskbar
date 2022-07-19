using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Newtonsoft.Json.Linq;
using RainbowTaskbar.Drawing;
using RainbowTaskbar.Helpers;
using RainbowTaskbar.HTTPAPI;
using RainbowTaskbar.WebSocketServices;

namespace RainbowTaskbar;

/// <summary>
///     Interaction logic for Taskbar.xaml
/// </summary>
public partial class Taskbar : Window {
    public CanvasManager canvasManager;
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

    private void RainbowTaskbar_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
        if (API.APISubscribed.Count > 0) {
            var data = new JObject();
            data.Add("type", "MouseMove");
            data.Add("x", e.GetPosition(this).X);
            data.Add("y", e.GetPosition(this).Y);
            WebSocketAPIServer.SendToSubscribed(data.ToString());
        }
    }

    private void RainbowTaskbar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
        if (API.APISubscribed.Count > 0) {
            var data = new JObject();
            data.Add("type", "MouseDown");
            data.Add("x", e.GetPosition(this).X);
            data.Add("y", e.GetPosition(this).Y);
            var states = new JObject();
            states.Add("left", JToken.FromObject(e.LeftButton));
            states.Add("right", JToken.FromObject(e.RightButton));
            states.Add("middle", JToken.FromObject(e.MiddleButton));
            data.Add("button_states", states);
            WebSocketAPIServer.SendToSubscribed(data.ToString());
        }
    }

    private void RainbowTaskbar_MouseUp(object sender, MouseButtonEventArgs e) {
        if (API.APISubscribed.Count > 0) {
            var data = new JObject();
            data.Add("type", "MouseUp");
            data.Add("x", e.GetPosition(this).X);
            data.Add("y", e.GetPosition(this).Y);
            var states = new JObject();
            states.Add("left", JToken.FromObject(e.LeftButton));
            states.Add("right", JToken.FromObject(e.RightButton));
            states.Add("middle", JToken.FromObject(e.MiddleButton));
            data.Add("button_states", states);
            WebSocketAPIServer.SendToSubscribed(data.ToString());
        }
    }
}