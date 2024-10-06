using System.Windows;
using Wpf.Ui.Controls;

namespace RainbowTaskbar.UserControls;

public partial class PresetNameDialog : FluentWindow {
    public PresetNameDialog() {
        InitializeComponent();
    }

    private void OkButton_OnClick(object sender, RoutedEventArgs e) => DialogResult = true;

    private void CancelButton_OnClick(object sender, RoutedEventArgs e) => DialogResult = false;
}