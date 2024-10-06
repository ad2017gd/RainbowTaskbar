using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using RainbowTaskbar.Configuration;
using RainbowTaskbar.Editor;
using RainbowTaskbar.Languages;
using RainbowTaskbar.UserControls;
using Wpf.Ui.Controls;

namespace RainbowTaskbar;

/// <summary>
///     Interaction logic for Editor.xaml
/// </summary>
public partial class EditorWindow : FluentWindow {
    public EditorViewModel viewModel;

    public EditorWindow() {
        viewModel = App.editorViewModel;
        DataContext = App.editorViewModel;
        InitializeComponent();

        App.localization.Enable(Resources.MergedDictionaries);
        Show();

        
    }


    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) {UseShellExecute = true});
        e.Handled = true;
    }

    private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        Process.Start(new ProcessStartInfo((string) ((System.Drawing.Image) sender).Tag) {UseShellExecute = true});
        e.Handled = true;
    }

    private void Button_Click(object sender, RoutedEventArgs e) /*=> App.ReloadTaskbars()*/ { }

    private void Save_Click(object sender, RoutedEventArgs e) /* => App.Config.ToFile() */ { }

    private static bool shownTip = false;
    private void Window_Closing(object sender, CancelEventArgs e) {
        //e.Cancel = true;
        //Hide();
        if (App.firstRun && !shownTip && App.trayWindow.TrayIcon is not null && !App.trayWindow.TrayIcon.IsDisposed) {
            App.trayWindow.TrayIcon.ShowBalloonTip("RainbowTaskbar", "The editor has been minimized to the system tray. Double-click to open the editor or right-click to open the context menu.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
            shownTip = true;
        }
        App.editor = null;
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
        // TODO: more config specific all in this file
        // App.ReloadTaskbars();
        // App.Config.ToFile();
    }

    private void AppBarButton_Click(object sender, RoutedEventArgs e) {
        //var contextMenu = (sender as AppBarButton).ContextMenu;
        //contextMenu.PlacementTarget = (sender as AppBarButton);
        //contextMenu.IsOpen = true;
    }

    private void OpenContextMenu_Click(object sender, RoutedEventArgs e) {
        var contextMenu = App.trayWindow.TrayIcon.ContextMenu;
        System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
        contextMenu.IsOpen = true;
    }
}