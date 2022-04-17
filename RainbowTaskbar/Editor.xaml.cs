using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using RainbowTaskbar.UserControls;

namespace RainbowTaskbar;

/// <summary>
///     Interaction logic for Editor.xaml
/// </summary>
public partial class Editor : Window {
    private readonly Taskbar taskbar;
    public EditorViewModel viewModel;

    public Editor() {
        viewModel = new EditorViewModel();
        DataContext = viewModel;
        taskbar = new Taskbar();
        //TODO: multiple taskbar support
        App.taskbars.Add(taskbar);
        taskbar.Show();
        InitializeComponent();
        Hide();

        TrayIcon.TrayMouseDoubleClick += TrayIcon_TrayMouseDoubleClick;
        TrayIcon.ToolTipText = $"RainbowTaskbar {Assembly.GetExecutingAssembly().GetName().Version?.ToString()}";
        TrayIcon.ContextMenu = new TrayContextMenu(this);
    }

    private void TrayIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e) {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        BringIntoView();
        Focus();
    }


    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) {UseShellExecute = true});
        e.Handled = true;
    }

    private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        Process.Start(new ProcessStartInfo((string) ((Image) sender).Tag) {UseShellExecute = true});
        e.Handled = true;
    }

    private void Button_Click(object sender, RoutedEventArgs e) => App.ReloadTaskbars();

    private void Window_Closing(object sender, CancelEventArgs e) {
        e.Cancel = true;
        Hide();
    }

    private void Window_Closed(object sender, EventArgs e) => TrayIcon.Dispose();

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
        App.ReloadTaskbars();
        App.Config.ToFile();
    }
}