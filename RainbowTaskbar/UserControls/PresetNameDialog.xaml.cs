using System.Windows;

namespace RainbowTaskbar.UserControls;

public partial class PresetNameDialog : Window {
    public PresetNameDialog() {
        InitializeComponent();
    }

    private void OkButton_OnClick(object sender, RoutedEventArgs e) => DialogResult = true;

    private void CancelButton_OnClick(object sender, RoutedEventArgs e) => DialogResult = false;
}