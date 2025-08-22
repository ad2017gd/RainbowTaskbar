using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
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
using System.Windows.Shapes;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace RainbowTaskbar.Editor
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : FluentWindow, INotifyPropertyChanged
    {
        public string DebugText { get; set; }
        private bool active = true;
        public DebugWindow()
        {

            DataContext = this;
            InitializeComponent();
            ApplicationThemeManager.ApplySystemTheme(true);
            Task.Run(() => {
                while (active) {
                    List<string> missingKeys = new List<string>();
                    foreach (DictionaryEntry key in App.localization.dictionary_en_US) {
                        if(!App.localization.dictionary.Contains(key.Key)) missingKeys.Add((string)key.Key);
                    }
                    DebugText = $"Missing translation keys: {string.Join(", ", missingKeys)}\n\n";
                    if (ExplorerTAP.ExplorerTAP.IsInjected) {
                        DebugText += $"UI Data: {ExplorerTAP.ExplorerTAP.GetUIDataStr(App.taskbars[0])}\n";
                    }
                    Thread.Sleep(100);
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void FluentWindow_Closing(object sender, CancelEventArgs e) {
            active = false;
        }
    }
}
