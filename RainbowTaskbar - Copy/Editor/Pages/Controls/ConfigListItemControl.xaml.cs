using Kasay;
using PropertyChanged;
using RainbowTaskbar.Configuration;
using RainbowTaskbar.Configuration.Web;
using RainbowTaskbar.Editor.Pages.Edit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Xceed.Wpf.AvalonDock.Controls;

namespace RainbowTaskbar.Editor.Pages.Controls
{
    /// <summary>
    /// Interaction logic for ConfigListItemControl.xaml
    /// </summary>
    public partial class ConfigListItemControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ConfigProperty =
                   DependencyProperty.Register(
                         "Config",
                          typeof(Config),
                          typeof(ConfigListItemControl));

        public Config Config {
            get {
                return (Config) GetValue(ConfigProperty);
            }
            set {
                SetValue(ConfigProperty, value);
                
            }
        }
        public string ConfigType { get => (Config is WebConfig) ? App.localization["enum_web"] : App.localization["enum_classic"]; }

        public void OnConfigChanged() {
            image.Source = Config.LoadImage();
        }
        public ConfigListItemControl()
        {

            InitializeComponent();
            DependencyPropertyDescriptor
                .FromProperty(ConfigProperty, typeof(ConfigListItemControl))
                .AddValueChanged(this, (s, e) => OnConfigChanged());

        }

        private void Delete(object sender, RoutedEventArgs e) {
            e.Handled = true;
            var button = (UIElement) sender;
            var control = button.FindVisualAncestor<ConfigListItemControl>();
            App.Configs.Remove(control.Config);
            control.Config.DeleteFile();
        }
        private void Edit(object sender, RoutedEventArgs e) {
            e.Handled = true;
            var page = new EditInfo(Config);
            App.editor.nav.ReplaceContent(page);
        }

        private void Select(object sender, RoutedEventArgs e) {
            if(App.Settings.SelectedConfig is not null) App.Settings.SelectedConfig.Stop();
            if (App.Settings.SelectedConfig != Config) {
                App.Settings.SelectedConfig = Config;
            } else {
                App.ReloadTaskbars();
            }
            App.Settings.ToFile();
        }
    }
}
