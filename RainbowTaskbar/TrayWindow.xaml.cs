using RainbowTaskbar.Configuration;
using RainbowTaskbar.UserControls;
using System;
using System.Collections.Generic;
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
            TrayIcon.ContextMenu = new TrayContextMenu(this);
            App.trayWindow = this;


        }

        private void TrayIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e) {
            (TrayIcon.ContextMenu as TrayContextMenu).Open_Click(null, null);
        }

    
}
}
