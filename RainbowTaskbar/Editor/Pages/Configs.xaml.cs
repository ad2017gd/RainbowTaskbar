using PropertyChanged;
using RainbowTaskbar.Configuration;
using RainbowTaskbar.Configuration.Instruction;
using RainbowTaskbar.Configuration.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

namespace RainbowTaskbar.Editor.Pages
{
    /// <summary>
    /// Interaction logic for Configs.xaml
    /// </summary>
    public partial class Configs : Page
    {
        public ObservableCollection<Config> ConfigList { get => App.Configs; }
        public Configs()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void AddNewConfig(object sender, RoutedEventArgs e) {
            var menu = (MenuItem)sender;
            Config cfg = null;
            if(menu.Header.ToString() == "Classic") {
                cfg = new InstructionConfig();
            } else {
                cfg = new WebConfig();
            }
            cfg.InitNew();
            cfg.ToFile();
            App.Configs.Insert(0, cfg);
        }
    }
}
