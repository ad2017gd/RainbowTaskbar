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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Appearance;

namespace RainbowTaskbar.Editor.Pages {
    /// <summary>
    /// Interaction logic for EmptyPageBadFix2.xaml
    /// </summary>
    public partial class EmptyPageBadFix2 : Page {
        public EmptyPageBadFix2() {
            InitializeComponent();
            ApplicationThemeManager.ApplySystemTheme(true);
        }
    }
}
