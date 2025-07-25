using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using RainbowTaskbar.Helpers;
using RainbowTaskbar.Interpolation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace RainbowTaskbar.Configuration.Web {
    public enum WebConfigUserSettingDataType {
        String,
        Number,
        Color,
        Boolean
    }
    [Serializable]
    public class WebConfigUserSetting {
        public string Name { get; set; } = "New property";
        public string Key { get; set; } = "property_1";
        public string _value { get; set; } = string.Empty;
        // probably should have used a converter but i gave up trying
        [JsonIgnore]
        public string ValueJS {
            get {
                switch (DataType) {
                    case WebConfigUserSettingDataType.Boolean: {
                            var suc = bool.TryParse(_value, out bool res);
                            return suc ? res.ToString().ToLower() : "false";
                        }
                    case WebConfigUserSettingDataType.Number: {
                            var suc = double.TryParse(_value, out double res);
                            return suc ? res.ToString() : "0";
                        }
                    default:
                        return $"{JsonSerializer.Serialize(_value)}";
                }
            }
        }
        [JsonIgnore]
        public dynamic Value { 
            get {
                switch (DataType) {
                    case WebConfigUserSettingDataType.Boolean: {
                            var suc = bool.TryParse(_value, out bool res);
                            return suc ? res : false;
                        }
                    case WebConfigUserSettingDataType.Number: {
                            var suc = double.TryParse(_value, out double res);
                            return suc ? res : 0;
                        }
                    case WebConfigUserSettingDataType.Color: {
                            try {
                                return (System.Windows.Media.Color) System.Windows.Media.ColorConverter.ConvertFromString(_value);
                            }
                            catch { return System.Windows.Media.Color.FromArgb(255, 0, 0, 0); }
                        }
                    default:
                        return _value;
                }
            } 
            set {
                switch (DataType) {
                    case WebConfigUserSettingDataType.Boolean: {
                            _value = value ? "true" : "false";
                            break;
                        }
                    case WebConfigUserSettingDataType.Number: {
                            _value = (value as object).ToString();
                            break;
                        }
                    case WebConfigUserSettingDataType.Color: {
                            _value = ColorTranslator.ToHtml(((System.Windows.Media.Color) value).ToDrawingColor());
                            break;
                        }
                    default:
                        _value = (value as object).ToString();
                        break;
                }
            } 
        }
        public WebConfigUserSettingDataType DataType { get; set; } = WebConfigUserSettingDataType.String;
    }
    [Serializable]
    public class WebConfigData : ConfigData {
        public string WebContent { get; set; }
        public BindingList<WebConfigUserSetting> UserSettings { get; set; } = new() { new() { } };

    }
    [Serializable]
    public class WebConfig : Config {
        [JsonIgnore]
        public WebConfigData Data { get => ConfigData as WebConfigData; set 
            {
                ConfigData = value;
            }
        }

        public WebConfig() {
            ConfigData = new WebConfigData() { WebContent = """
<html>
    <head>
        <script>
            // !!! Helper taskbar interop functions !!! //
            const TASKBAR=0;const UNDERLAY=1;const DEFAULT=0;const BLUR=1;const TRANSPARENT=2;const setOpacity = (which,v) => window.chrome.webview.postMessage({m:"transparency",v,which});const setStyle = (style) => window.chrome.webview.postMessage({m:"style",style});const setOffset = (offset) => window.chrome.webview.postMessage({m:"offset",v:offset});
            let last = performance.now() / 1000;let fpsTh = 0;

            // ---------------------- !!! Place your code (except any requestAnimationFrame stuff) inside of this self-executing function !!! ---------------------- //
            (function() {
                // DEFAULT, BLUR, TRANSPARENT
                setStyle(TRANSPARENT);
                // TASKBAR, UNDERLAY
                setOpacity(TASKBAR, 1.0);
                setOpacity(UNDERLAY, 1.0);
                // UNDERLAY Y OFFSET
                setOffset(0);

                // All config-specific properties can be found in the 'rtUserConfig' variable using the specified keys

                // Uncomment line below if you use the update loop ('loop') function.
                // _startLoop();

                // ...
            })();

            function loop() {
                 window.requestAnimationFrame(loop);let now = performance.now() / 1000;let dt = Math.min(now - last, 1);last = now;if (rtMaxFPS > 0) {fpsTh += dt;if (fpsTh < 1 / rtMaxFPS) {return;}fpsTh -= 1 / rtMaxFPS;}
                 
                 // ------------- !!! PLACE YOUR ANIMATION/UPDATE CODE BELOW THIS LINE, INSIDE THIS FUNCTION !!! ------------------- //
             }
             function _startLoop() {
                // Restore original requestAnimationFrame, since this script is FPS-option-aware.
                window.requestAnimationFrame = __raf;
                setTimeout(loop, 50);
             }
        </script>
    </head>
    <body>
        <!-- Your body here... -->
    </body>


</html>
""" };
        }

        private void _Start(WebView2 webView, Mutex webViewReady) {
            webViewReady.WaitOne();
            webViewReady.ReleaseMutex();
            App.Current.Dispatcher.Invoke(() => {
                webView?.CoreWebView2.Resume();
                webView.CoreWebView2.MemoryUsageTargetLevel = Microsoft.Web.WebView2.Core.CoreWebView2MemoryUsageTargetLevel.Low;

                File.WriteAllText(Path.Join(App.rainbowTaskbarDir, "current.html"), Data.WebContent);

                var code = $$"""
                        // code adapted from https://github.com/PixelsCommander/fps-control-chrome-extension/blob/master/src/js/content.js

                        window.rtMaxFPS = {{App.Settings.MaxWebFPS}};

                        let __rafs = 0;
                        let __raf = window.requestAnimationFrame;
                        let __nextRAFTime = Date.now();
                        let __mockedRaf = (callback) => {
                            let skip = (window.rtMaxFPS !== 0 && Date.now() < __nextRAFTime);

                            if (skip) {
                                __skippingRaf(callback);
                            } else {
                                __nextRAFTime = Date.now() + 1000/window.rtMaxFPS;
                                __raf(callback);
                            }

                            return __rafs++;
                        }
                        function __skippingRaf(func) {
                            __raf(() => {
                                window.requestAnimationFrame(func);
                            })
                        }
                        window.requestAnimationFrame = __mockedRaf;
                        window.rtUserConfig = {};
                        {{string.Join('\n', Data.UserSettings.Select((x) => {
                    return "window.rtUserConfig[" + JsonSerializer.Serialize(new Regex("[^\\w$]").Replace(x.Key, " ")) + "]=" + x.ValueJS + ";";
                })) ?? ""}}

                        """;
                webView?.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
                    code).ContinueWith((s) => {
                        App.Current.Dispatcher.Invoke(() => {
                            webView?.CoreWebView2.Navigate(Path.Join(App.rainbowTaskbarDir, "current.html"));
                            EventHandler<CoreWebView2NavigationCompletedEventArgs> handler = null;
                            handler = (sender, args) => {
                                webView.CoreWebView2.RemoveScriptToExecuteOnDocumentCreated(s.Result);
                                webView.NavigationCompleted -= handler;
                            };
                            webView.NavigationCompleted += handler;
                        });
                    });

                
            });
        }

        public override void Start() {
            base.Start();

            App.taskbars.ForEach(x => {
                x.ClassicGrid.Visibility = System.Windows.Visibility.Collapsed;
                x.WebGrid.Visibility = System.Windows.Visibility.Visible;
            });

            if(App.Settings.GraphicsRepeat) {
                App.taskbars.ForEach(x => {
                    Task.Run(() => {
                        _Start(x.webView, x.webViewReady_);
                    });
                });
            } else {
                Task.Run(() => {
                    _Start(App.webView, App.webViewReady);
                });
            }
        }

        public override async Task Stop() {
            var wv = App.webView;
            var tsk = wv?.EnsureCoreWebView2Async();
            await Task.Run(() => {
                try { tsk?.Wait(100); } catch { return; }
                App.Current.Dispatcher.Invoke(() => {
                    try {
                        wv?.NavigateToString("<html></html>");
                        wv?.CoreWebView2.TrySuspendAsync();
                    }
                    catch { }
                });
            });
        }
    }
}
