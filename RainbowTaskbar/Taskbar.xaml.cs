using System;
using System.Windows;
using System.Windows.Input;
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