using PropertyChanged;
using RainbowTaskbar.Configuration;
using RainbowTaskbar.Configuration.Instruction;
using RainbowTaskbar.Configuration.Web;
using RainbowTaskbar.Helpers;
using RainbowTaskbar.HTTPAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Page = System.Windows.Controls.Page;

namespace RainbowTaskbar.Editor.Pages.Edit {
    /// <summary>
    /// Interaction logic for ViewComments.xaml
    /// </summary>
    public partial class ViewComments : Page, INotifyPropertyChanged {
        public Config Config { get; set; }
        public Page OldPage { get; set; }
        public bool SignedIn { get => App.Settings.LoggedIn; }

        public BindingList<CommentData> Comments { get; set; }

        public ViewComments(Config config) {
            InitializeComponent();
            this.DataContext = this;
            ApplicationThemeManager.ApplySystemTheme(true);
            App.localization.Enable(Resources.MergedDictionaries);
            Config = config;
            App.Settings.workshopAPI.GetConfigComments(Config).ContinueWith(c => {
                var res = c?.Result;
                if (res.Result)
                    Comments = new BindingList<CommentData>(c.Result.Comments);
                });



        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Button_Click(object sender, RoutedEventArgs e) {
            App.editor.nav.Navigate(typeof(EmptyPageBadFix));
            Task.Run(() => {
                Thread.Sleep(50);
                Dispatcher.Invoke(() => App.editor.nav.Navigate(typeof(EmptyPageBadFix2)));
                Thread.Sleep(50);
                Dispatcher.Invoke(() => App.editor.nav.ReplaceContent(OldPage));
            });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            App.Settings.workshopAPI.AddConfigComment(Config, textb.Text).ContinueWith(c=> {
                var res = c.Result;
                if(res.Result) {
                    Dispatcher.Invoke(() => {
                        Comments.Insert(0, new() { AuthorUsername = App.Settings.AccountUsername, Content = textb.Text, ID = res.ID });
                        Config.CachedCommentCount++;

                        textb.Text = "";
                    });
                    
                }

            });
            
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            var btn = (sender as Wpf.Ui.Controls.Button);
            var cfg = btn.Tag as CommentData;

            App.Settings.workshopAPI.DeleteConfigComment(Config, cfg.ID);
            Comments.Remove(Comments.First(x => x.ID == cfg.ID));
            Config.CachedCommentCount--;
        }
    }
}
