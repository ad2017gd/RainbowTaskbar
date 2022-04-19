using System;
using System.Windows;
using System.Windows.Input;
using RainbowTaskbar.API.WebSocket;
using RainbowTaskbar.API.WebSocket.Events;
using RainbowTaskbar.Drawing;
using RainbowTaskbar.Helpers;

namespace RainbowTaskbar;

/// <summary>
///     Interaction logic for Taskbar.xaml
/// </summary>
public partial class Taskbar : Window {
    public Layers layers;
    public bool Secondary;
    public TaskbarHelper taskbarHelper;
    public TaskbarViewModel viewModel;
    public WindowHelper windowHelper;

    public Taskbar(bool secondary) {
        InitializeComponent();
        Secondary = secondary;

        viewModel = new TaskbarViewModel(this);
        Closing += viewModel.OnWindowClosing;
        DataContext = viewModel;
    }


    public Taskbar() {
        InitializeComponent();

        viewModel = new TaskbarViewModel(this);
        Closing += viewModel.OnWindowClosing;
        DataContext = viewModel;
    }

    private void RainbowTaskbar_Closed(object sender, EventArgs e) {
        taskbarHelper.Style = TaskbarHelper.TaskbarStyle.ForceDefault;
        taskbarHelper.SetBlur();
    }

    private void RainbowTaskbar_MouseMove(object sender, MouseEventArgs e) {
        if (API.API.APISubscribed.Count > 0) {
            WebSocketAPIServer.SendToSubscribed(new MouseMoveEvent(e.GetPosition(this).X, e.GetPosition(this).Y));
        }
    }

    private void RainbowTaskbar_MouseDown(object sender, MouseButtonEventArgs e) {
        if (API.API.APISubscribed.Count > 0) {
            var @event = new MouseDownEvent(e.GetPosition(this).X, e.GetPosition(this).Y,
                new MouseStates(e.LeftButton == MouseButtonState.Pressed, e.RightButton == MouseButtonState.Pressed,
                    e.MiddleButton == MouseButtonState.Pressed));
            WebSocketAPIServer.SendToSubscribed(@event);
        }
    }

    private void RainbowTaskbar_MouseUp(object sender, MouseButtonEventArgs e) {
        if (API.API.APISubscribed.Count > 0) {
            var @event = new MouseUpEvent(e.GetPosition(this).X, e.GetPosition(this).Y,
                new MouseStates(e.LeftButton == MouseButtonState.Pressed, e.RightButton == MouseButtonState.Pressed,
                    e.MiddleButton == MouseButtonState.Pressed));
            WebSocketAPIServer.SendToSubscribed(@event);
        }
    }
}