using Kasay;
using PropertyChanged;
using RainbowTaskbar.Configuration;
using RainbowTaskbar.Configuration.Web;
using RainbowTaskbar.Editor.Pages.Edit;
using RainbowTaskbar.HTTPAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for ResultListItemControl.xaml
    /// </summary>
    public partial class ResultListItemControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ConfigProperty =
                   DependencyProperty.Register(
                         "Config",
                          typeof(Config),
                          typeof(ResultListItemControl));

        public Config Config {
            get {
                return (Config) GetValue(ConfigProperty);
            }
            set {
                SetValue(ConfigProperty, value);
            }
        }
        public static readonly DependencyProperty PageParentProperty =
                   DependencyProperty.Register(
                         "PageParent",
                          typeof(Browse),
                          typeof(ResultListItemControl));

        public Browse PageParent {
            get {
                return (Browse) GetValue(PageParentProperty);
            }
            set {
                SetValue(PageParentProperty, value);
            }
        }
        public string ConfigType { get => (Config is WebConfig) ? App.localization["enum_web"] : App.localization["enum_classic"]; }

        public bool Downloaded { get => App.Configs.Any(x=>x.PublishedID == Config.PublishedID || (x.PublishedID is null && x.PreviousPublishedID == Config.PublishedID)); }
        public bool CanDelete { get => Config.CachedPublisherUsername == App.Settings.AccountUsername || App.Settings.AccountUsername == "ad2017gd"; }

        public bool Unverified { get; set; } = false;

        public async void OnConfigChanged() {
            Config.CachedBase64Thumbnail = await App.Settings.workshopAPI.DownloadThumbnailBase64(Config);
            if(Config.CachedBase64Thumbnail is null && App.Settings.AccountUsername == "ad2017gd") {
                Config.CachedBase64Thumbnail = await App.Settings.workshopAPI.DownloadThumbnailBase64(Config, true);
                if (Config.CachedBase64Thumbnail is not null) Unverified = true;
            }
            image.Source = Config.LoadImage();
        }

        public ResultListItemControl()
        {
            InitializeComponent();
            DependencyPropertyDescriptor
                .FromProperty(ConfigProperty, typeof(ConfigListItemControl))
                .AddValueChanged(this, (s, e) => OnConfigChanged());

        }


        private void Select(object sender, RoutedEventArgs e) {
            e.Handled = true;
            var page = new ViewInfo(Config);
            App.editor.nav.ReplaceContent(page);
        }

        private void Download(object sender, RoutedEventArgs e) {
            e.Handled = true;
            var copy = Config.Copy();
            if(copy.CachedPublisherUsername != App.Settings.AccountUsername) {
                copy.PreviousPublishedID = copy.PublishedID;
                copy.PublishedID = null;
            }
            copy.ToFile();
            App.Configs.Insert(0, copy);
            OnPropertyChanged("Downloaded");
        }

        private void Delete(object sender, RoutedEventArgs e) {
            e.Handled = true;
            var res = App.Settings.workshopAPI.DeleteConfigAsync(Config);
            Task.Run(() => {
                var result = res.Result;
                Dispatcher.InvokeAsync(() => {
                    if (result.Result) {
                        var existing = App.Configs.FirstOrDefault(x => x.PublishedID == Config.PublishedID, null);
                        if(existing is not null) {

                            existing.PublishedID = null;
                            existing.Published = DateTime.MinValue;
                            PageParent.ResultList.Remove(Config);
                        }
                    }
                });
            });
        }

        private void Verify(object sender, RoutedEventArgs e) {
            e.Handled = true;

            var res = App.Settings.workshopAPI.VerifyThumbnail(Config);
            Task.Run(() => {
                var result = res.Result;
                Dispatcher.InvokeAsync(() => {
                    if (result.Result) {
                        Unverified = false;
                    }
                });
            });
        }
    }
}
