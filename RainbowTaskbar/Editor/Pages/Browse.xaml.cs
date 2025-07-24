using RainbowTaskbar.Configuration;
using RainbowTaskbar.Editor.Pages.Controls;
using RainbowTaskbar.HTTPAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
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
using Xceed.Wpf.AvalonDock.Controls;

namespace RainbowTaskbar.Editor.Pages
{
    public enum SortBy {
        Likes,
        Match
    }
    /// <summary>
    /// Interaction logic for Browse.xaml
    /// </summary>
    public partial class Browse : Page, INotifyPropertyChanged
    {
        public ObservableCollection<Config> ResultList { get; set; } = new();
        public SortBy Sort { get; set; } = SortBy.Likes;
        public int Page { get; set; } = -1;
        private bool end = false;
        public bool SearchLoaded { get; set; } = true;
        public Browse()
        {
            DataContext = this;
            InitializeComponent();
            ApplicationThemeManager.ApplySystemTheme(true);
            //App.localization.Enable(Resources.MergedDictionaries);




        }

        public void Clear() {
            itemscontrol.FindVisualChildren<ResultListItemControl>().ToList().ForEach(x => {
                x.Dispose();
            });
            ResultList.Clear();
        }

        public void OnSortChanged() {

            if (!SearchLoaded) return;
            SearchLoaded = false;
            scrollViewer.ScrollToHome();
            Clear();
            Page = -1;
            end = false;
            Task.Run(() => {
                Thread.Sleep(300);
                while (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight && !end) {
                    Page++;
                    Task t = null;
                    Dispatcher.Invoke(() => t = Search());
                    t.Wait();
                    Thread.Sleep(300);

                }
            });
        }

        public Task Search() {
            SearchLoaded = false;
            var configsreq = App.Settings.workshopAPI.SearchConfigsAsync(search.Text.Trim(), Sort, Page);
            return Task.Run(() => {
                var cfgs = configsreq.Result;
                if (cfgs is null) {
                    end = true;
                    SearchLoaded = true;
                    return;
                }
                var parsed = cfgs.Parse();
                if(parsed.Count() == 0) {
                    end = true;
                    SearchLoaded = true;
                    return;
                }
                Dispatcher.Invoke(() => parsed.ToList().ForEach(x=>ResultList.Add(x)));
                SearchLoaded = true;
            });
            
        }
        Timer searchTimer;
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (!SearchLoaded) return;
            if (searchTimer != null) searchTimer.Dispose();
            searchTimer = new Timer((_) => {
                Dispatcher.Invoke(() => {
                    Clear();
                    Page = 0;
                    end = false;
                    Search();
                });
            });
            searchTimer.Change(250, Timeout.Infinite);
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            if (!SearchLoaded) return;
            var scroll = (ScrollViewer)sender;
            if (scroll.VerticalOffset == scroll.ScrollableHeight && e.ExtentHeightChange == 0 && !end) {
                Page++;
                Search();
            }
        }
    }
}
