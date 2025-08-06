using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Appearance;

namespace RainbowTaskbar.Editor.Pages {
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Page {
        public About() {
            InitializeComponent();
            ApplicationThemeManager.ApplySystemTheme(true);

            App.localization.Enable(Resources.MergedDictionaries);


        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Process.Start(new ProcessStartInfo("https://paypal.me/ad2k17") { UseShellExecute = true });
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo("https://github.com/ad2017gd") { UseShellExecute = true });
        }

        private void Hyperlink_Click_1(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo("https://github.com/ad2017gd/RainbowTaskbar/issues/new/choose") { UseShellExecute = true });
        }

        private void Hyperlink_Click_2(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo("https://ad2017.dev/rnb") { UseShellExecute = true });
        }
    }
}
