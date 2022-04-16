using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace RainbowTaskbar.UserControls;

/// <summary>
///     Interaction logic for TrayContextMenu.xaml
/// </summary>
public partial class TrayContextMenu : ContextMenu {
    private readonly Editor parent;

    public TrayContextMenu(Editor editor) {
        parent = editor;
        InitializeComponent();
    }

    private void Open_Click(object sender, RoutedEventArgs e) {
        parent.Show();
        parent.WindowState = WindowState.Normal;
        parent.BringIntoView();
        parent.Focus();
    }

    private void Donate_Click(object sender, RoutedEventArgs e) {
        Process.Start(new ProcessStartInfo("https://paypal.me/ad2k17") {UseShellExecute = true});
        e.Handled = true;
    }

    private void ProjectPage_Click(object sender, RoutedEventArgs e) {
        Process.Start(new ProcessStartInfo("https://ad2017.dev/rnb") {UseShellExecute = true});
        e.Handled = true;
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => App.Exit();
}