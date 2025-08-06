using PropertyChanged;
using RainbowTaskbar.Configuration;
using RainbowTaskbar.Configuration.Instruction;
using RainbowTaskbar.Configuration.Web;
using RainbowTaskbar.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace RainbowTaskbar.Editor.Pages.Edit {
    /// <summary>
    /// Interaction logic for EditInfo.xaml
    /// </summary>
    public partial class ViewInfo : Page, INotifyPropertyChanged {
        public Config Config { get; set; }
        public bool IsClassic { get => Config is InstructionConfig; }
        public bool Downloaded { get => App.Configs.Any(x => x.PublishedID == Config.PublishedID); }
        public string FileSize { get => (Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Config)).Length / 1024.0).ToString("N1"); }
        public bool Liked { get; set; }

        public string CommentButtonText { get => App.localization.Get("comments") + $" ({Config.CachedCommentCount})"; }
        public ViewInfo(Config config) {
            InitializeComponent();
            this.DataContext = this;
            ApplicationThemeManager.ApplySystemTheme(true);
            App.localization.Enable(Resources.MergedDictionaries);
            Config = config;

            var content = new TextRange(description.Document.ContentStart, description.Document.ContentEnd);

            if (content.CanLoad(DataFormats.Rtf)) {
                using (var stream = new MemoryStream(Encoding.Default.GetBytes(Config.Description)))
                    content.Load(stream, DataFormats.Rtf);
            }

            thumbPreview.Source = config.LoadImage();

            if (App.Settings.LoggedIn) App.Settings.workshopAPI.GetLikedConfigs().ContinueWith(x => Liked = x?.Result.Search.Contains(Config.PublishedID) ?? false);
        }

        private void Download(object sender, RoutedEventArgs e) {
            var copy = Config.Copy();
            if (copy.CachedPublisherUsername != App.Settings.AccountUsername || Downloaded) {
                copy.PreviousPublishedID = copy.PublishedID;
                copy.PublishedID = null;
            }
            copy.ToFile();
            App.Configs.Insert(0, copy);
        }

        private void Like(object sender, RoutedEventArgs e) {
            if(!App.Settings.LoggedIn) {
                App.editor.contentDialogService.ShowSimpleDialogAsync(new() {
                    Content = App.localization.Get("msgbox_notloggedin2"),
                    Title = App.localization.Get("msgbox_notloggedin_title"),
                    CloseButtonText = "OK"
                });
                return;
            }

            Liked = !Liked;
            Config.CachedLikeCount += Liked ? 1 : -1;
            App.Settings.workshopAPI.LikeConfig(Config, Liked);
        }

        private void Comment(object sender, RoutedEventArgs e) {
           var page = new ViewComments(Config);
            page.OldPage = this;

            App.editor.nav.Navigate(typeof(EmptyPageBadFix));
            Task.Run(() => {
                Thread.Sleep(50);
                Dispatcher.Invoke(() => App.editor.nav.Navigate(typeof(EmptyPageBadFix2)));
                Thread.Sleep(50);
                Dispatcher.Invoke(() => App.editor.nav.ReplaceContent(page));
            });
        }
        private void OpenWeb(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo("https://ad2017.dev/rnb/web/config/"+Config.PublishedID) { UseShellExecute = true });
        }
    }
}
