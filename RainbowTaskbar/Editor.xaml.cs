using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using ModernWpf.Controls;
using RainbowTaskbar.Configuration;
using RainbowTaskbar.Configuration.Instructions;
using RainbowTaskbar.Languages;
using RainbowTaskbar.UserControls;

namespace RainbowTaskbar;

/// <summary>
///     Interaction logic for Editor.xaml
/// </summary>
public partial class Editor : Window {
    public EditorViewModel viewModel;

    public Editor() {
        
        //

        viewModel = App.editorViewModel;
        DataContext = App.editorViewModel;
        InitializeComponent();

        App.localization.Enable(Resources.MergedDictionaries);
        Show();
        

        //TrayIcon.TrayMouseDoubleClick += TrayIcon_TrayMouseDoubleClick;
        //TrayIcon.ToolTipText = $"RainbowTaskbar {Assembly.GetExecutingAssembly().GetName().Version?.ToString()}";
        //TrayIcon.ContextMenu = new TrayContextMenu(this);

        
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

    private void Save_Click(object sender, RoutedEventArgs e) => App.Config.ToFile();

    private void Window_Closing(object sender, CancelEventArgs e) {
        //e.Cancel = true;
        //Hide();
        App.editor = null;
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
        App.ReloadTaskbars();
        App.Config.ToFile();
    }

    private void AppBarButton_Click(object sender, RoutedEventArgs e) {
        var contextMenu = (sender as AppBarButton).ContextMenu;
        contextMenu.PlacementTarget = (sender as AppBarButton);
        contextMenu.IsOpen = true;
    }
}