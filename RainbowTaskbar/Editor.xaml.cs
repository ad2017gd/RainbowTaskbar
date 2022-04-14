using RainbowTaskbar.UserControls;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace RainbowTaskbar
{
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor : Window
    {
        public EditorViewModel viewModel;
        Taskbar taskbar;

        public Editor()
        {
            viewModel = new EditorViewModel();
            DataContext = viewModel;
            taskbar = new Taskbar();
            App.taskbars.Add(taskbar);
            taskbar.Show();
            InitializeComponent();
            this.Hide();

            TrayIcon.TrayMouseDoubleClick += TrayIcon_TrayMouseDoubleClick;
            TrayIcon.ToolTipText = $"RainbowTaskbar {Assembly.GetExecutingAssembly().GetName().Version.ToString()}";
            TrayIcon.ContextMenu = new TrayContextMenu(this);
            
        }

        private void TrayIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Show();
        }


        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo((string)((Image)sender).Tag) { UseShellExecute = true });
            e.Handled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            App.ReloadTaskbars();
            App.Config.ToFile();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            TrayIcon.Dispose();
        }
    }
}
