using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public DebugWindow()
        {

            DataContext = this;
            InitializeComponent();
            ApplicationThemeManager.ApplySystemTheme(true);
            Task.Run(() => {
                while (true) {
                    DebugText = ExplorerTAP.ExplorerTAP.DebugGetUITree();
                    Thread.Sleep(5);
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
