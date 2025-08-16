using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace RainbowTaskbar.Editor
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : FluentWindow, INotifyPropertyChanged
    {
        public string DebugText { get; set; }
        private bool active = true;
        public DebugWindow()
        {

            DataContext = this;
            InitializeComponent();
            ApplicationThemeManager.ApplySystemTheme(true);
            Task.Run(() => {
                if (ExplorerTAP.ExplorerTAP.IsInjected) {
                    while (active) {
                        if(ExplorerTAP.ExplorerTAP.IsInjected) {
                            DebugText = $"{ExplorerTAP.ExplorerTAP.GetUIDataStr(App.taskbars[0])}\n";
                        }
                        Thread.Sleep(100);
                    }
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void FluentWindow_Closing(object sender, CancelEventArgs e) {
            active = false;
        }
    }
}
