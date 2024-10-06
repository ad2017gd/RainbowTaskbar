using RainbowTaskbar.Configuration.Web;
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

namespace RainbowTaskbar.Editor.Pages.Controls.WebControls {
    /// <summary>
    /// Interaction logic for AddPropertyDialogControl.xaml
    /// </summary>
    public partial class AddPropertyDialogControl : UserControl {
        public WebConfigUserSetting Setting { get; set; } = new();
        public AddPropertyDialogControl() {
            InitializeComponent();
            DataContext = this;
        }
    }
}
