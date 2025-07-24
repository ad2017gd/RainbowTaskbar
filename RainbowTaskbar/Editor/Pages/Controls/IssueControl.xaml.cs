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

namespace RainbowTaskbar.Editor.Pages.Controls
{
    /// <summary>
    /// Interaction logic for IssueControl.xaml
    /// </summary>
    public partial class IssueControl : UserControl
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Contact { get; set; } = "";
        public IssueControl()
        {
            DataContext = this;
            InitializeComponent();
        }
    }
}
