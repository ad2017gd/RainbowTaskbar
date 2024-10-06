using System;
using System.Collections.Generic;
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

namespace RainbowTaskbar.Editor.Pages {
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page {
        public Home() {
            InitializeComponent();

            Task.Run(() => {
                while (true) {

                    try {
                        string tree = ExplorerTAP.ExplorerTAP.DebugGetUITree();
                        Dispatcher.Invoke(() => {
                            debug.Text = tree;
                        });
                    } catch { }
                    Thread.Sleep(100);
                }
            });
        }
    }
}
