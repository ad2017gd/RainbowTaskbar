
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using RainbowTaskbar.Configuration;
using RainbowTaskbar.Configuration.Web;
using RainbowTaskbar.Editor.Pages.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
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

namespace RainbowTaskbar.Editor.Pages.Edit {
    /// <summary>
    /// Interaction logic for WebEdit.xaml
    /// </summary>
    public partial class WebEditPage : EditPage {

        
        public bool OpenDevTools { get; set; } = false;

        public void OnOpenDevToolsChanged() {
            if(OpenDevTools) {
                if(!App.Settings.GraphicsRepeat) {
                    App.hiddenWebViewHost.webView_.CoreWebView2?.OpenDevToolsWindow();
                } else {
                    App.taskbars.ForEach(x=>x.webView.CoreWebView2?.OpenDevToolsWindow());
                }
            }
        }

        public WebEditPage() {
            InitializeComponent();
            this.DataContext = this;
            ApplicationThemeManager.ApplySystemTheme(true);
            App.localization.Enable(Resources.MergedDictionaries);

            var envasync = CoreWebView2Environment.CreateAsync(null, System.IO.Path.GetTempPath(), new CoreWebView2EnvironmentOptions());
            Task.Run(() => {
                var env = envasync.Result;
                Dispatcher.Invoke(() =>
                    webView.EnsureCoreWebView2Async(env).ContinueWith((t) => {
                        Dispatcher.Invoke(() => {
                            webView.Source = new Uri(System.IO.Path.Join(App.monacoDir, "index.html"));
                            webView.SetCurrentValue(WebView2.DefaultBackgroundColorProperty, System.Drawing.Color.Transparent);
                        });

                    })
                );
            });
            
            var theme = Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme() == Wpf.Ui.Appearance.ApplicationTheme.Light ? "vs" : "vs-dark";
            webView.NavigationCompleted += (_, _) => {
                webView.ExecuteScriptAsync(
                    $$$"""
                    monaco.editor.defineTheme('editortheme', {
                        base: '{{{theme}}}',
                        inherit: true,
                        rules: [{ background: 'FFFFFF00' }],
                        colors: {'editor.background': '#FFFFFF00','minimap.background': '#FFFFFF00',}});
                    monaco.editor.setTheme('editortheme');

                                        
                    window.addEventListener('resize', () => {
                      editor.layout({ width: 0, height: 0 })
                      window.requestAnimationFrame(() => {
                        editor.layout({ width: document.documentElement.clientWidth - 2, height: document.documentElement.clientHeight - 2})
                      })
                    })
                    
                    editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyS, () => window.chrome.webview.postMessage({m:"save"}));
                    """
                );


                webView.ExecuteScriptAsync($"editor.getModel().setValue(atob('{Convert.ToBase64String(Encoding.UTF8.GetBytes((Config as WebConfig).Data.WebContent))}'));")
                .ContinueWith((_) => {
                    Dispatcher.Invoke(() => webView.ExecuteScriptAsync("""
                        editor.getModel().onDidChangeContent((event) => {
                          	window.chrome.webview.postMessage({m:"changed"});
                        });
                        """));
                });
            };
            webView.WebMessageReceived += (_, args) => {
                var message = JsonSerializer.Deserialize<JsonNode>(args.WebMessageAsJson);
                switch(message["m"].GetValue<string>()) {
                    case "changed":
                        Modified = true;
                        break;
                    case "save":
                        Save();
                        Modified = false;
                        break;
                }
            };

            
        }
        public async Task<string> GetContent() {
            var res = await webView.ExecuteScriptAsync("editor.getModel().getValue()");
            return Regex.Unescape(res).TrimStart('"').TrimEnd('"');
        }
        public void Save() {
            GetContent().ContinueWith((t) => {
                var res = t.Result;
                Dispatcher.Invoke(() => { (Config as WebConfig).Data.WebContent = res; Config.Updated = DateTime.Now; Config.ToFile(); });
            });
        }
        public WebConfig Current { get; set; } = new WebConfig();
        private void RunConfig(object sender, RoutedEventArgs e) {
            if (App.Settings.SelectedConfig is not null) {
                App.Settings.SelectedConfig.Stop();
                App.ReloadTaskbars(false);
            }
            GetContent().ContinueWith((t) => {
                Current.Data.UserSettings = (Config as WebConfig).Data.UserSettings;
                Current.Data.WebContent = t.Result;
                Thread.Sleep(500);
                Dispatcher.Invoke(() => {
                    Current.Start();
                    });
                Thread.Sleep(1000);
                Dispatcher.Invoke(() => {
                    OnOpenDevToolsChanged();
                });
            });
        }
        

    }
}
