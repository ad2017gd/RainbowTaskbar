using System;
using System.Windows;
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
}