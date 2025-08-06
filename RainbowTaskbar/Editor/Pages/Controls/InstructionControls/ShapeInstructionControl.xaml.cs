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

namespace RainbowTaskbar.Editor.Pages.Controls.InstructionControls {
    /// <summary>
    /// Interaction logic for ShapeInstructionControl.xaml
    /// </summary>
    public partial class ShapeInstructionControl : UserControl {
        public ShapeInstructionControl() {
            InitializeComponent();
            App.localization.Enable(Resources.MergedDictionaries);
            ApplicationThemeManager.ApplySystemTheme(true);
        }
    }
}
