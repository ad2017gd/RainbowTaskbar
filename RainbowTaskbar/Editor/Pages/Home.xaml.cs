using RainbowTaskbar.Configuration.Web;
using RainbowTaskbar.Editor.Pages.Controls;
using RainbowTaskbar.Editor.Pages.Controls.WebControls;
using RainbowTaskbar.Editor.Pages.Edit;
using RainbowTaskbar.Helpers;
using RainbowTaskbar.Interpolation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace RainbowTaskbar.Editor.Pages {
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page {
        public Home() {
            InitializeComponent();

            ApplicationThemeManager.ApplySystemTheme(true);

            App.localization.Enable(Resources.MergedDictionaries);
            DataContext = App.editorViewModel;
            
            Task.Run( () => {
                
                if(App.editorViewModel.LatestUpdateInfo is null) {
                    string str;
                    try {
                        str = AutoUpdate.GetLatestBody().Result;
                    }
                    catch (Exception ex) {
                        str = "Failed to get latest update info.";
                    }
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

        private void Button_Click(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo("ms-windows-store:Review?PFN=48822ad2017.30397FC5B3C66_32727fk258az6") { UseShellExecute = true });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            var ctrl = new IssueControl();

            lastupdate.Visibility = Visibility.Hidden;

            Task.Run(() => {
                Task<ContentDialogResult> result = null;
                Dispatcher.Invoke(() => result = App.editor.contentDialogService.ShowSimpleDialogAsync(new() {
                    Content = ctrl,
                    Title = App.localization.Get("issueorreq"),
                    PrimaryButtonText = "OK",
                    SecondaryButtonText = App.localization.Get("msgbox_issuegithub"),
                    CloseButtonText = App.localization.Get("msgbox_button_cancel")
                }));
                result.Wait();

                if (result.Result == ContentDialogResult.Primary) {
                    Task<HTTPAPI.ResultResponse> res = null;
                    Dispatcher.Invoke(() => {
                        res = App.Settings.workshopAPI.SubmitIssue(ctrl.Title, ctrl.Description, ctrl.Contact);
                    });
                    res.Wait();
                    if (!res.Result.Result) {
                        Dispatcher.Invoke(() => App.editor.contentDialogService.ShowSimpleDialogAsync(new() {
                            Content = App.localization.Get("msgbox_fail_title"),
                            Title = App.localization.Get("msgbox_fail_title"),
                            CloseButtonText = "OK"
                        }).ContinueWith((t) => Dispatcher.Invoke(() => lastupdate.Visibility = Visibility.Visible)));
                    } else {
                        Dispatcher.Invoke(() => lastupdate.Visibility = Visibility.Visible);
                    }
                }
                else if(result.Result == ContentDialogResult.Secondary) {
                    Process.Start(new ProcessStartInfo("https://github.com/ad2017gd/RainbowTaskbar/issues/new/choose") { UseShellExecute = true });
                    Dispatcher.Invoke(() => lastupdate.Visibility = Visibility.Visible);
                } else {
                    Dispatcher.Invoke(() => lastupdate.Visibility = Visibility.Visible);
                }

                
            });
        }
    }
}
