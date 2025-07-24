using RainbowTaskbar.Editor.Pages.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Security.Policy;
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
using System.Net.Http.Json;
using System.Text.Json;
using System.Dynamic;
using Wpf.Ui.Extensions;

namespace RainbowTaskbar.Editor.Pages {
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page {

        public Preferences.Settings SettingsCfg { get => App.Settings; }
        public List<string> Languages {
            get => RainbowTaskbar.Languages.Localization.languages;
        }

        public Settings() {
            DataContext = this;
            InitializeComponent();

            App.localization.Enable(Resources.MergedDictionaries);
        }

        private void genericLogin(string oauthUrl) {
            Process.Start(new ProcessStartInfo($"{oauthUrl}{(SettingsCfg.LoginKey is not null && SettingsCfg.LoginKey != string.Empty ? $"&state={SettingsCfg.LoginKey}" : "")}") { UseShellExecute = true });

            var loginControl = new LoginControl();
            var task = App.editor.contentDialogService.ShowSimpleDialogAsync(new() {
                Content = loginControl,
                Title = App.localization.Get("msgbox_login_title"),
                PrimaryButtonText = App.localization.Get("msgbox_login_button"),
                CloseButtonText = "Cancel"
            });
            Task.Run(() => {
                var res = task.Result;
                if (res == Wpf.Ui.Controls.ContentDialogResult.Primary) {
                    string code = "";
                    Dispatcher.Invoke(() => {
                        code = loginControl.code.Text;
                    });
                    using var http = new HttpClient();
                    using var web = new WebClient();
                    try {
                        var content = http.PostAsJsonAsync("https://rnbsrv.ad2017.dev/user/code", new { code }).Result;
                        dynamic json = content.Content.ReadFromJsonAsync<ExpandoObject>().Result;
                        SettingsCfg.LoginKey = json.key.ToString();
                        SettingsCfg.ToFile();
                    }
                    catch { }

                }
            });
        }

        private void github_Click(object sender, RoutedEventArgs e) {
            genericLogin("https://github.com/login/oauth/authorize?client_id=Ov23li82ypW7MrlXLsQx");

        }

        private void google_Click(object sender, RoutedEventArgs e) {
            genericLogin("https://accounts.google.com/o/oauth2/auth?client_id=1034862162125-eo3oug90ecm84gpt2tuodfmmtfuuovle.apps.googleusercontent.com&redirect_uri=https://rnbsrv.ad2017.dev/google/auth&scope=email&response_type=code");
        }

        private void login_Click(object sender, RoutedEventArgs e) {
            
        }

        private void login2_Click(object sender, RoutedEventArgs e) {
            SettingsCfg.LoginKey = null;
            SettingsCfg.ToFile();
            SettingsCfg.OnLanguageChanged();

        }
    }
}
