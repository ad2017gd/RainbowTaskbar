using Microsoft.Win32;
using PropertyChanged;
using RainbowTaskbar.Configuration;
using RainbowTaskbar.Editor;
using RainbowTaskbar.HTTPAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Dynamic;
using RainbowTaskbar.Configuration.Web;
using RainbowTaskbar.Configuration.Instruction.Instructions;
using System.Windows;
#if MSIX_BUILD
using Windows.ApplicationModel;
#endif

namespace RainbowTaskbar.Preferences {
    public class Settings : INotifyPropertyChanged {
        [OnChangedMethod(nameof(SaveChanged))]
        public bool CheckUpdate { get; set; } = true;

        
        public int InterpolationQuality { get; set; } = 25;
        [OnChangedMethod(nameof(SaveChanged))]
        [OnChangedMethod(nameof(OnTaskbarBehaviourChanged))]
        public bool GraphicsRepeat { get; set; } = false;
        [OnChangedMethod(nameof(SaveChanged))]
        [OnChangedMethod(nameof(OnTaskbarBehaviourChanged))]
        public bool SameRadiusOnEach { get; set; } = false;
        [OnChangedMethod(nameof(SaveChanged))]
        [OnChangedMethod(nameof(OnLanguageChanged))]
        public string Language { get; set; } = "SystemDefault";
        [OnChangedMethod(nameof(SaveChanged))]
        [OnChangedMethod(nameof(OnTrayIconVisibilityChanged))]
        public bool ShowTrayIcon { get; set; } = true;

        [OnChangedMethod(nameof(OnConfigChanged))]
        public string SelectedConfigID { get; set; }

        [OnChangedMethod(nameof(SaveChanged))]
        [OnChangedMethod(nameof(OnTaskbarBehaviourChanged))]
        public int MaxWebFPS { get; set; } = 40;

        [JsonIgnore]
        public string AccountUsername { get; set; } = "";
        [JsonIgnore]
        public bool LoggedIn { get; set; } = false;
        public string LoginKey { get; set; }
        public WorkshopAPI workshopAPI = null;

        [OnChangedMethod(nameof(SaveChanged))]
        [OnChangedMethod(nameof(OnWebTouchThroughChanged))]
        public bool WebTouchThrough { get; set; } = true;
        [OnChangedMethod(nameof(SaveChanged))]
        [OnChangedMethod(nameof(OnWebScriptEnabledChanged))]
        public bool WebScriptEnabled { get; set; } = true;
        public void OnWebTouchThroughChanged() {
            if (WebTouchThrough) App.StartHook();
            else App.StopHook();
        }
        [OnChangedMethod(nameof(SaveChanged))]
        [OnChangedMethod(nameof(OnTaskbarBehaviourChanged))]
        [OnChangedMethod(nameof(OnGlobalOpacityChanged))]
        public double GlobalOpacity { get; set; } = -1;

        public Version Version { get; set; } = new Version("1.0");

        public void OnGlobalOpacityChanged() {
            if(GlobalOpacity != -1) App.taskbars.ForEach(x => {
                new TransparencyInstruction() {
                    Type = TransparencyInstruction.TransparencyInstructionType.Taskbar,
                    Opacity = GlobalOpacity
                }.Execute(x);
            });
        }

        public void OnWebScriptEnabledChanged() {
            App.taskbars.ForEach(x => {
                try {
                    x.webView.CoreWebView2.Settings.IsScriptEnabled = WebScriptEnabled;
                }
                catch {

                }
            });
            if (App.Settings.SelectedConfig is WebConfig) Taskbar.SoftReset(true);
        }
        private Timer TryLoginTimer = null;
        public async void OnLoginKeyChanged() {
            if (App.Settings is null) return;
            try {
                using var http = new HttpClient();
                using var web = new WebClient();
                var content = await http.PostAsJsonAsync("https://rnbsrv.ad2017.dev/user/me/info", new { key = LoginKey });
                dynamic json = JsonSerializer.Deserialize<ExpandoObject>(content.Content.ReadAsStringAsync().Result);
                if (json.result.GetBoolean()) {
                    dynamic user = JsonSerializer.Deserialize<ExpandoObject>(json.user);
                    LoggedIn = true;
                    if (TryLoginTimer is not null) {
                        TryLoginTimer.Dispose();
                        TryLoginTimer = null;
                    }
                    AccountUsername = user.username.ToString();
                    workshopAPI = new WorkshopAPI() { LoginKey = LoginKey };
                    ToFile();
                }
                else {
                    throw new Exception();
                }
            } catch {
                LoggedIn = false;
                AccountUsername = "";
                if(TryLoginTimer is null) {
                    TryLoginTimer = new Timer((_) => {
                        OnLoginKeyChanged();
                    }, null, 15000, 15000);
                }
            }
        }

        [JsonIgnore]
        public Config SelectedConfig { get => App.Configs.FirstOrDefault(x=>x.LocalID == SelectedConfigID, null); set => SelectedConfigID = value.LocalID; }

        public bool FirstStart { get; set; } = true;

        [JsonIgnore]
        public bool RunAtStartup {
            get {
                if (App.IsMicrosoftStore()) {
#if MSIX_BUILD
                    StartupTask t = StartupTask.GetAsync("RnbTsk_Startup").AsTask().Result;
                    if (t is null) return false;
                    
                    return t.State == StartupTaskState.Enabled || t.State == StartupTaskState.EnabledByPolicy;
#else
                    return false;
#endif
                } else {
                    return (string?) Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run")
                .GetValue("RainbowTaskbar") == Environment.ProcessPath;
                }
            }

            set {
                if(App.IsMicrosoftStore()) {
#if MSIX_BUILD
                    StartupTask t = StartupTask.GetAsync("RnbTsk_Startup").AsTask().Result;
                    if (t is null) return;
                    if (value)
                        t.RequestEnableAsync();
                    else
                        t.Disable();
#endif
                } else {
                        var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    if (value)
                        key.SetValue("RainbowTaskbar", Environment.ProcessPath);
                    else
                        key.DeleteValue("RainbowTaskbar");
                    key.Close();
                }

                string prot = "rnbtsk";
                RegistryKey reg = Registry.CurrentUser.OpenSubKey($"Software\\Classes\\{prot}", true);
                if (reg != null) {
                    reg = reg.OpenSubKey(@"shell\open\command", true);
                    reg.SetValue(string.Empty, Environment.ProcessPath + " " + "shell \"%1\"");
                    reg.Close();
                } else {
                    reg = Registry.CurrentUser.CreateSubKey($"Software\\Classes\\{prot}", true);
                    reg.SetValue(string.Empty, "URL: " + prot);
                    reg.SetValue("URL Protocol", string.Empty);

                    reg = reg.CreateSubKey(@"shell\open\command");
                    reg.SetValue(string.Empty, Environment.ProcessPath + " " + "shell \"%1\"");
                    reg.Close();
                }
            }
        }

        public string language {
            get {
                return Language == "SystemDefault" ? CultureInfo.CurrentUICulture.Name : Language;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnTaskbarBehaviourChanged() {
            if (App.Settings is null) return;
            App.ReloadTaskbars();
        }

        public void OnLanguageChanged() {
            if (App.Settings is null) return;
            App.localization.Switch(language);
            if (App.editor is not null) {
                App.editor.Close();
                App.editor = new EditorWindow();
                App.LaunchEditor();
            }
        }

        public void OnTrayIconVisibilityChanged() {
            if (App.Settings is null) return;
            if (App.trayWindow is not null) {
                App.trayWindow.TrayIcon.Dispose();
                App.trayWindow.Close();
                App.trayWindow = null;
            } else {
                App.trayWindow = new();
            }
        }
        public void OnConfigChanged() {
            if (App.Settings is null) return;
            App.ReloadTaskbars();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public static Settings FromFile(string file = null) {
            if (file is null) file = Path.Join(App.rainbowTaskbarDir, "settings.json");

            if (!File.Exists(file)) return new();
            var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(file));
            settings.loaded = true;
            return settings;
        }
        public void ToFile(string file = null) {
            if (file is null) file = Path.Join(App.rainbowTaskbarDir, "settings.json");

            File.WriteAllText(file, JsonSerializer.Serialize(this));
        }
        public bool loaded = false;
        public void SaveChanged() {
            if (!loaded) return;
            ToFile();
        }

    }
}
