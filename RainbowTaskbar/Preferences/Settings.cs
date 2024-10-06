using Microsoft.Win32;
using PropertyChanged;
using RainbowTaskbar.HTTPAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace RainbowTaskbar.Preferences {
    public class Settings : INotifyPropertyChanged {
        public bool CheckUpdate { get; set; } = true;
        public long StartedCount { get; set; } = 0;

        
        [OnChangedMethod(nameof(OnIsAPIEnabledChanged))]
        public bool IsAPIEnabled { get; set; } = false;

        [OnChangedMethod(nameof(OnAPIPortChanged))]
        public int APIPort { get; set; } = 9093;

        public int InterpolationQuality { get; set; } = 25;

        [OnChangedMethod(nameof(OnTaskbarBehaviourChanged))]
        public bool GraphicsRepeat { get; set; } = true;

        [OnChangedMethod(nameof(OnTaskbarBehaviourChanged))]
        public bool SameRadiusOnEach { get; set; } = false;

        [OnChangedMethod(nameof(OnLanguageChanged))]
        public string Language { get; set; } = "SystemDefault";

        [OnChangedMethod(nameof(OnTrayIconVisibilityChanged))]
        public bool ShowTrayIcon { get; set; } = true;

        public bool FirstStart { get; set; } = true;

        [JsonIgnore]
        public bool RunAtStartup {
            get => (string?) Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run")
                .GetValue("RainbowTaskbar") == Process.GetCurrentProcess().MainModule.FileName;

            set {
                var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (value)
                    key.SetValue("RainbowTaskbar", Process.GetCurrentProcess().MainModule.FileName);
                else
                    key.DeleteValue("RainbowTaskbar");
            }
        }

        public string language {
            get {
                return Language == "SystemDefault" ? CultureInfo.CurrentUICulture.Name : Language;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnTaskbarBehaviourChanged() {
            // TODO: Add this to this config-specific
            //App.ReloadTaskbars();
        }

        public void OnLanguageChanged() {
            App.localization.Switch(language);
            if (App.editor is not null) {
                App.editor.Close();
                App.editor = new EditorWindow();
            }
        }

        public void OnTrayIconVisibilityChanged() {
            if(App.trayWindow is not null) {
                App.trayWindow.Close();
                App.trayWindow = null;
            } else {
                App.trayWindow = new();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void OnIsAPIEnabledChanged() {
            if (API.http is not null) {
                if (IsAPIEnabled) API.Start();
                else
                    API.Stop();
            }
        }

        public void OnAPIPortChanged() {
            API.Start();
        }

    }
}
