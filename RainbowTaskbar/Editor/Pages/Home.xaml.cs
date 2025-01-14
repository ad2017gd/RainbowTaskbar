using RainbowTaskbar.Helpers;
using RainbowTaskbar.Interpolation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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

namespace RainbowTaskbar.Editor.Pages {
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page {
        public Home() {
            InitializeComponent();

            ApplicationThemeManager.ApplySystemTheme(true);

            App.localization.Enable(Resources.MergedDictionaries);
            
            Task.Run( () => {
                
                if(App.editorViewModel.LatestUpdateInfo is null) {
                    var str = AutoUpdate.GetLatestBody().Result;
                    App.editorViewModel.LatestUpdateInfo = Markdig.Markdown.ToHtml(str);
                }
                Task t = null;
                Dispatcher.Invoke(() => {
                    t = wv.EnsureCoreWebView2Async();
                    wv.DefaultBackgroundColor = System.Drawing.Color.Transparent;
                });
                t.Wait();
                Dispatcher.Invoke(() => {
                    wv.NavigateToString(
                        $$"""
                    <style>
                    body {
                        background: transparent;
                        color: {{ColorTranslator.ToHtml((title.Foreground as SolidColorBrush).Color.ToDrawingColor())}};
                        font-family: "{{title.FontFamily.ToString()}}", "Segoe UI", sans-serif;
                        font-size: large;
                    }
                    </style>
                    """ + App.editorViewModel.LatestUpdateInfo);

                    lastupdate.Visibility = Visibility.Visible;
                });
            });

        }
    }
}
