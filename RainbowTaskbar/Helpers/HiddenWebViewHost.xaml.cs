using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace RainbowTaskbar.Helpers {
    /// <summary>
    /// Interaction logic for HiddenWebViewHost.xaml
    /// </summary>
    public partial class HiddenWebViewHost : Window {
        public HiddenWebViewHost() {
            InitializeComponent();
            WindowHelper.InitOther(this);
            Width = 2000;
            Height = 100;
            Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }
    }
}
