using RainbowTaskbar.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

namespace RainbowTaskbar.UserControls
{
    /// <summary>
    /// Interaction logic for TrayContextMenu.xaml
    /// </summary>
    public partial class TrayContextMenu : ContextMenu
    {
        Editor parent;

        public TrayContextMenu(Editor editor)
        {
            parent = editor;
            InitializeComponent();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            parent.Show();
            parent.WindowState = WindowState.Normal;
            parent.BringIntoView();
            parent.Focus();
        }

        private void Donate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://paypal.me/ad2k17") { UseShellExecute = true });
            e.Handled = true;
        }

        private void ProjectPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://ad2017.dev/rnb") { UseShellExecute = true });
            e.Handled = true;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            App.taskbars.ForEach((t) => {
                t.taskbarHelper.Radius = 0;
                t.taskbarHelper.UpdateRadius();
                t.Close();
                Helpers.TaskbarHelper.SendMessage(t.taskbarHelper.HWND, Helpers.TaskbarHelper.WM_DWMCOMPOSITIONCHANGED, 1, null);
                t.taskbarHelper.SetAlpha(1);
            });
            

            Application.Current.Shutdown();
        }

        private void RainbowPreset_Click(object sender, RoutedEventArgs e)
        {
            App.Config.Instructions = new BindingList<Instruction>(Configuration.Presets.Rainbow());
            App.ReloadTaskbars();
            App.Config.ToFile();
        }

        private void ChillPreset_Click(object sender, RoutedEventArgs e)
        {
            App.Config.Instructions = new BindingList<Instruction>(Configuration.Presets.Chill());
            App.ReloadTaskbars();
            App.Config.ToFile();
        }

        private void UnknownPreset_Click(object sender, RoutedEventArgs e)
        {
            App.Config.Instructions = new BindingList<Instruction>(Configuration.Presets.Unknown());
            App.ReloadTaskbars();
            App.Config.ToFile();
        }
    }
}
