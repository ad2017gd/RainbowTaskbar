using RainbowTaskbar.Configuration;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RainbowTaskbar.UserControls;

/// <summary>
///     Interaction logic for TrayContextMenu.xaml
/// </summary>
public partial class TrayContextMenu : ContextMenu {
    private readonly TrayWindow parent;

    public Config Config {
        get => App.Config;
        set => App.Config = value;
    }

    public TrayContextMenu(TrayWindow wnd) {
        parent = wnd;
        DataContext = App.editorViewModel;
        InitializeComponent();
    }



    public void Open_Click(object sender, RoutedEventArgs e) {
        if(App.editor == null) App.editor = new Editor();
        App.editor.Show();
        App.editor.WindowState = WindowState.Normal;
        App.editor.BringIntoView();
        App.editor.Focus();
        
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

    private void IssueOrRequest_Click(object sender, RoutedEventArgs e) {
        Process.Start(new ProcessStartInfo("https://github.com/ad2017gd/RainbowTaskbar/issues/new") { UseShellExecute = true });
        e.Handled = true;
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e) {
        App.localization.Enable(this.Resources.MergedDictionaries);
    }
}