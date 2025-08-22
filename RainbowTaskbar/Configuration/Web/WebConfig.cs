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
        public string Name { get; set; } = App.localization?.Get("defpropname") ?? "New property";
        public string Key { get; set; } = App.localization?.Get("defpropkey") ?? "property_1";
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
        <style>
            body,html {
                width:100%;
                height:100%;
                margin:0;
                padding:0;
                overflow:hidden;
            }
        </style>
    </head>
    <body>

    </body>
    <script>
        rainbowTaskbar
            .setTaskbarOpacity(1)
            .setStyleTransparent();

        rainbowTaskbar.onUIChanged = (info) => {
            
        }
     </script>

</html>
""" };
        }

        private void _Start(WebView2 webView, Mutex webViewReady, Taskbar t) {
            if(!webViewReady.WaitOne(1500)) return;
            webViewReady.ReleaseMutex();
            App.Current.Dispatcher.Invoke(() => {
                webView?.CoreWebView2.Resume();
                webView.CoreWebView2.MemoryUsageTargetLevel = Microsoft.Web.WebView2.Core.CoreWebView2MemoryUsageTargetLevel.Low;

                File.WriteAllText(Path.Join(App.rainbowTaskbarDir, "current.html"), Data.WebContent);

                var code = $$"""
                        // requestAnimationFrame code adapted from https://github.com/PixelsCommander/fps-control-chrome-extension/blob/master/src/js/content.js

                        window.rtMaxFPS = {{App.Settings.MaxWebFPS}};
                        window.rtUserConfig = {};

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

                        window.rainbowTaskbar = {
                            setTaskbarOpacity: (v) => {window.chrome.webview.postMessage({m:"transparency",v,which:0}); return window.rainbowTaskbar},
                            setUnderlayOpacity: (v) => {window.chrome.webview.postMessage({m:"transparency",v,which:1}); return window.rainbowTaskbar},
                            setTaskbarElementsOpacity: (v) => {window.chrome.webview.postMessage({m:"transparency",v,which:5}); return window.rainbowTaskbar},
                            setStyleDefault: () => {window.chrome.webview.postMessage({m:"style",style:0}); return window.rainbowTaskbar},
                            setStyleBlur: () => {window.chrome.webview.postMessage({m:"style",style:1}); return window.rainbowTaskbar},
                            setStyleTransparent: () => {window.chrome.webview.postMessage({m:"style",style:2}); return window.rainbowTaskbar},
                            setTaskbarOffset: (offset) => {window.chrome.webview.postMessage({m:"offset",v:offset}); return window.rainbowTaskbar},
                            
                            uiInfo: {},
                            graphicsRepeat: {{(App.Settings.GraphicsRepeat ? "true" : "false")}},
                            maxFps: window.rtMaxFPS,
                            rtUserConfig: window.rtUserConfig,
                            taskbarIndex: {{(t is not null ? App.taskbars.IndexOf(t) : -1)}},

                            helpers: {
                                normalizeTaskbarCoords: (_ui, copy = true) => {
                                    let ui = copy ? JSON.parse(JSON.stringify(_ui)) : _ui;
                        
                                    let minX = _ui.sort((a,b)=>a.taskbar.X - b.taskbar.X)[0].taskbar.X;
                                    for(let e of ui) {
                                        e.taskbar.X -= minX;
                                    }
                        
                                    return ui;
                                },
                                normalizeAndScaleUICoords: (_ui, copy = true) => {
                                    let ui = rainbowTaskbar.helpers.normalizeTaskbarCoords(_ui, copy);

                                    for(let e of ui) {
                                        e.taskbar.X /= window.devicePixelRatio;
                                        e.taskbar.Y /= window.devicePixelRatio;
                                        e.taskbar.Width /= window.devicePixelRatio;
                                        e.taskbar.Height /= window.devicePixelRatio;
                        
                                        e.taskbarFrameRepeater.X /= window.devicePixelRatio;
                                        e.taskbarFrameRepeater.Y /= window.devicePixelRatio;
                                        e.taskbarFrameRepeater.Width /= window.devicePixelRatio;
                                        e.taskbarFrameRepeater.Height /= window.devicePixelRatio;
                        
                                        e.systemTrayFrame.X /= window.devicePixelRatio;
                                        e.systemTrayFrame.Y /= window.devicePixelRatio;
                                        e.systemTrayFrame.Width /= window.devicePixelRatio;
                                        e.systemTrayFrame.Height /= window.devicePixelRatio;
                                    }

                                    return ui;
                                },
                                generateCSSMaskForUI: (_ui, fadeTop = true) => {
                                    let ui = rainbowTaskbar.helpers.normalizeAndScaleUICoords(_ui);
                                    ui.sort((a,b) => a.taskbar.X - b.taskbar.X);

                                    let genVis = ``;

                                    if(rainbowTaskbar.graphicsRepeat) {
                                        let x = ui.find(x=>x.taskbarIndex == rainbowTaskbar.taskbarIndex);

                                        genVis = `rgba(255, 255, 255, 0) ${x.taskbarFrameRepeater.X - 24}px, rgba(255, 255, 255, 1) ${x.taskbarFrameRepeater.X + 64}px, rgba(255, 255, 255, 1) ${x.taskbarFrameRepeater.X + x.taskbarFrameRepeater.Width - 64}px, rgba(255, 255, 255, 0) ${x.taskbarFrameRepeater.X + x.taskbarFrameRepeater.Width + 24}px, rgba(255, 255, 255, 0) ${x.systemTrayFrame.X - 24}px, rgba(255, 255, 255, 1) ${x.systemTrayFrame.X + 64}px, rgba(255, 255, 255, 1) ${x.taskbar.Width - 8}px, rgba(255, 255, 255, 0) ${x.taskbar.Width}px,`
                                    } else {
                                        genVis = ui.map((x,i)=>
                                            `rgba(255, 255, 255, ${+(x.taskbarFrameRepeater.X == 0)}) ${x.taskbar.X + x.taskbarFrameRepeater.X - 24}px, rgba(255, 255, 255, 1) ${x.taskbar.X + x.taskbarFrameRepeater.X + 64}px, rgba(255, 255, 255, 1) ${x.taskbar.X + x.taskbarFrameRepeater.X + x.taskbarFrameRepeater.Width - 64}px, rgba(255, 255, 255, 0) ${x.taskbar.X + x.taskbarFrameRepeater.X + x.taskbarFrameRepeater.Width + 24}px, rgba(255, 255, 255, 0) ${x.taskbar.X + x.systemTrayFrame.X - 24}px, rgba(255, 255, 255, 1) ${x.taskbar.X + x.systemTrayFrame.X + 64}px, rgba(255, 255, 255, 1) ${x.taskbar.X + x.taskbar.Width - 8}px, rgba(255, 255, 255, ${+(x.taskbarFrameRepeater.X == 0)}) ${x.taskbar.X + x.taskbar.Width}px,`
                                        ).join("")
                                    }

                                   

                                    let mask = `linear-gradient(90deg,rgba(255, 255, 255, 0) 0%, ${genVis} rgba(255, 255, 255, 1) 100%) 0px 0px / 100% 100% no-repeat${fadeTop ? ", linear-gradient(180deg,rgba(255, 255, 255, 0) 0%, rgba(255, 255, 255, 0) 1px, rgba(255, 255, 255, 1) 40%, rgba(255, 255, 255, 1) 100%) 0px 0px / 100% 100% no-repeat" : ""}`;
                                    let maskComposite = 'intersect';
                                    return {
                                        mask, maskComposite, 
                                        apply: () => {
                                            document.documentElement.style.mask = mask;
                                            document.documentElement.style.maskComposite = maskComposite;
                                        }
                                    }
                                }
                            },

                            onUIChanged: (uiInfo) => {},

                            __internal: {
                                __onUIChanged: (data) => {window.rainbowTaskbar.uiInfo=data;window.rainbowTaskbar.onUIChanged(window.rainbowTaskbar.uiInfo);},
                            }
                            
                        }

                        window.chrome.webview.addEventListener("message", (m) => {
                            let message = JSON.parse(m.data);
                            if(message.message == "ui") {
                                window.rainbowTaskbar.__internal.__onUIChanged(message.data);
                            }
                        });

                        window.chrome.webview.postMessage({m:"ui"})
                        for(let i = 1; i <= 5; i++) setTimeout(() => window.chrome.webview.postMessage({m:"ui"}), i*500);


                        window.requestAnimationFrame = __mockedRaf;
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

        public override async Task<bool> Start() {
            if (!(await base.Start())) return false;

            App.taskbars.ForEach(x => {
                x.ClassicGrid.Visibility = System.Windows.Visibility.Collapsed;
                x.WebGrid.Visibility = System.Windows.Visibility.Visible;
            });

            if(App.Settings.GraphicsRepeat) {
                App.taskbars.ForEach(x => {
                    Task.Run(() => {
                        _Start(x.webView, x.webViewReady_, x);
                    });
                });
            } else {
                Task.Run(() => {
                    _Start(App.webView, App.webViewReady, null);
                });
            }

            return true;
        }

        public override Task Stop() {
            var wv = App.webView;
            return Task.Run(() => {
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
