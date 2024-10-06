using RainbowTaskbar.Configuration;
using RainbowTaskbar.HTTPAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
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
        public SortBy Sort { get; set; } = SortBy.Match;
        public int Page { get; set; } = 0;
        private bool end = false;
        public Browse()
        {
            DataContext = this;
            InitializeComponent();
            Search();
        }
        public void OnSortChanged() {
            ResultList.Clear();
            Page = 0;
            end = false;
            Search();
        }

        public void Search() {
            var configsreq = App.Settings.workshopAPI.SearchConfigsAsync(search.Text.Trim(), Page);
            Task.Run(() => {
                var cfgs = configsreq.Result;
                if (cfgs is null) {
                    end = true;
                    return;
                }
                var parsed = cfgs.Parse();
                if(parsed.Count() == 0) {
                    end = true;
                    return;
                }
                if(Sort == SortBy.Likes) parsed = parsed.OrderByDescending(x => x.CachedLikeCount); 
                Dispatcher.Invoke(() => parsed.ToList().ForEach(x=>ResultList.Add(x)));
            });
            
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
            ResultList.Clear();
            Page = 0;
            end = false;
            Search();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            var scroll = (ScrollViewer)sender;
            if (scroll.VerticalOffset == scroll.ScrollableHeight && e.ExtentHeightChange == 0 && !end) {
                Page++;
                Search();
            }
        }
    }
}
