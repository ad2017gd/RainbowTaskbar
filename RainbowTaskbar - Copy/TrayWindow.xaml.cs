using RainbowTaskbar.Configuration;
//using RainbowTaskbar.UserControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RainbowTaskbar {
    /// <summary>
    /// Interaction logic for TrayWindow.xaml
    /// </summary>
    public partial class TrayWindow : Window {
        

        public TrayWindow() {
            InitializeComponent();

            TrayIcon.TrayMouseDoubleClick += TrayIcon_TrayMouseDoubleClick;
            TrayIcon.ToolTipText = $"RainbowTaskbar {Assembly.GetExecutingAssembly().GetName().Version?.ToString()}";
            TrayIcon.ContextMenu = this.ContextMenu;
            App.localization.Enable(TrayIcon.ContextMenu.Resources.MergedDictionaries);
            App.trayWindow = this;
            



        }

        private void TrayIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e) {

            App.LaunchEditor();
        }

        private void Open_Click(object sender, RoutedEventArgs e) {
            e.Handled = true;
            App.LaunchEditor();
        }

        private void Exit_Click(object sender, RoutedEventArgs e) {
            e.Handled = true;
            App.Exit();
        }

        private void Issue_Click(object sender, RoutedEventArgs e) {
            e.Handled = true;
            Process.Start(new ProcessStartInfo("https://github.com/ad2017gd/RainbowTaskbar/issues/new/choose") { UseShellExecute = true });
        }

        private void ProjectPage_Click(object sender, RoutedEventArgs e) {
            e.Handled = true;
            Process.Start(new ProcessStartInfo("https://ad2017.dev/rnb") { UseShellExecute = true });
        }
    }
}
