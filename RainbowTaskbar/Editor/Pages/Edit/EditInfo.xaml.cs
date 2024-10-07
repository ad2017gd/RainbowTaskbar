using Microsoft.Win32;
using PropertyChanged;
using RainbowTaskbar.Configuration;
using RainbowTaskbar.Configuration.Instruction;
using RainbowTaskbar.Configuration.Web;
using RainbowTaskbar.Editor.Pages.Controls.WebControls;
using RainbowTaskbar.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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
using System.Xml.Linq;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Button = Wpf.Ui.Controls.Button;

namespace RainbowTaskbar.Editor.Pages.Edit {
    /// <summary>
    /// Interaction logic for EditInfo.xaml
    /// </summary>
    public partial class EditInfo : Page {
        public Config Config { get; set; }
        public Config New { get; set; }
        public bool IsClassic { get => Config is InstructionConfig; }

        public EditInfo(Config config) {
            InitializeComponent();
            this.DataContext = this;
            App.localization.Enable(Resources.MergedDictionaries);

            Config = config;
            New = Config.Copy();

            var content = new TextRange(richDescription.Document.ContentStart, richDescription.Document.ContentEnd);
            if (content.CanLoad(DataFormats.Rtf)) {
                try {
                    using (var stream = new MemoryStream(Encoding.Default.GetBytes(Config.Description)))
                        content.Load(stream, DataFormats.Rtf);
                }
                catch { }
            }
            thumbPreview.Source = Config.LoadImage();
        }

        private void Save(object sender, RoutedEventArgs e) {
            string newDesc = "";
            var textRange = new TextRange(richDescription.Document.ContentStart, richDescription.Document.ContentEnd);
            using (var stream = new MemoryStream()) {
                textRange.Save(stream, DataFormats.Rtf);
                stream.Position = 0;
                newDesc = Encoding.Default.GetString(stream.ToArray());
            }

            Config.Description = newDesc;
            Config.Name = New.Name;
            Config.Updated = DateTime.Now;
            if(Config is WebConfig) {
                (Config.ConfigData as WebConfigData).UserSettings = (New.ConfigData as WebConfigData).UserSettings;
            }
            Config.ToFile();
        }

        private void Edit(object sender, RoutedEventArgs e) {
            if(Config is WebConfig) {
                App.editorViewModel.EditPage = new WebEditPage();
                App.editorViewModel.EditPage.Config = Config;
            } else if (Config is InstructionConfig) {
                App.editorViewModel.EditPage = new InstructionEditPage(Config as InstructionConfig);
            }
            App.editor.nav.ReplaceContent(App.editorViewModel.EditPage);
        }

        private void Preset(object sender, RoutedEventArgs e) {
            var b = (Wpf.Ui.Controls.Button) sender;
            b.ContextMenu = new();
            DefaultPresets.Presets.ForEach(x => {
                var v = new Wpf.Ui.Controls.MenuItem() { Header = x.Name };
                v.Click += (_,_) => {
                    var cfg = Config as InstructionConfig;
                    cfg.Data.RunOnceGroup = x.RunOnceGroup;
                    cfg.Data.LoopGroups.Clear();
                    foreach(var group in x.LoopGroups) {
                        cfg.Data.LoopGroups.Add(group);
                    }
                    cfg.ToFile();
                };
                b.ContextMenu.Items.Add(v);
                
            });
            b.ContextMenu.IsOpen = true;
            
        }

        private void Publish(object sender, RoutedEventArgs e) {
            Save(null, null);
            if(!App.Settings.LoggedIn) {
                App.editor.contentDialogService.ShowSimpleDialogAsync(new() {
                    Content = App.localization.Get("msgbox_notloggedin"),
                    Title = App.localization.Get("msgbox_notloggedin_title"),
                    CloseButtonText = "OK"
                });
                return;
            }
            Task.Run(() => {
                var res = App.Settings.workshopAPI.PublishConfigAsync(Config).Result;
                if (res is null || !res.Result) {
                    Dispatcher.Invoke(() => App.editor.contentDialogService.ShowSimpleDialogAsync(new() {
                        Content = App.localization.Get("msgbox_fail"),
                        Title = App.localization.Get("msgbox_fail_title"),
                        CloseButtonText = "OK"
                    }));
                    return;
                }
                Dispatcher.Invoke(() => {
                    Task.Run(() => {
                        var res2 = App.Settings.workshopAPI.SetConfigThumbnail(Config, thumbPreview).Result;
                        if (res2 is null || !res2.Result) {
                            Dispatcher.Invoke(() => App.editor.contentDialogService.ShowSimpleDialogAsync(new() {
                                Content = App.localization.Get("msgbox_thumbnail"),
                                Title = App.localization.Get("msgbox_fail_title"),
                                CloseButtonText = "OK"
                            }));
                        }
                    });
                    

                    Config.PublishedID = res.Data.PublishedID;
                    Config.CachedPublisherUsername = App.Settings.AccountUsername;
                    Config.Published = DateTime.Now;
                    Config.ToFile();
                });
            });
        }

        private void richDescription_MouseWheel(object sender, MouseWheelEventArgs e) {
            var fontSizes = new List<double>() { 8, 9, 10, 11, 12, 13, 14, 16, 18, 20, 24, 30, 36, 42, 48, 60, 72 };
            if (Keyboard.Modifiers == ModifierKeys.Control) {
                var current = ((double) (richDescription.Selection.GetPropertyValue(FontSizeProperty) == DependencyProperty.UnsetValue ? 12.0 : richDescription.Selection.GetPropertyValue(FontSizeProperty)));
                var nearest = fontSizes.MinBy(x => Math.Abs(x - current));
                var nextIdx = Math.Min(Math.Max(0,fontSizes.IndexOf(nearest) + Math.Sign(e.Delta)), fontSizes.Count-1);

                richDescription.Selection.ApplyPropertyValue(FontSizeProperty, fontSizes[nextIdx]);
            }
        }

        

        private void picture_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog =
            new() {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Filter = "Image files (*.bmp;*.jpg;*.jpeg;*.png;*.webp)|*.bmp;*.jpg;*.jpeg;*.png;*.webp|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() ?? false) {
                if(openFileDialog.CheckFileExists) {
                    Config.CachedBase64Thumbnail = Convert.ToBase64String(File.ReadAllBytes(openFileDialog.FileName));
                    Config.Updated = DateTime.Now;
                }
            }
            thumbPreview.Source = Config.LoadImage();
        }

        private void Delete(object sender, RoutedEventArgs e) {
            var btn = sender as Button;

            (New.ConfigData as WebConfigData).UserSettings.Remove(btn.Tag as WebConfigUserSetting);
            
        }

        private void AddProperty(object sender, RoutedEventArgs e) {

            var ctrl = new AddPropertyDialogControl();
            Task.Run(() => {
                Task<ContentDialogResult> result = null;
                Dispatcher.Invoke(() => result = App.editor.contentDialogService.ShowSimpleDialogAsync(new() {
                    Content = ctrl,
                    Title = App.localization.Get("msgbox_property_title"),
                    CloseButtonText = "OK"
                }));
                result.Wait();
                Dispatcher.Invoke(() => {
                    (New.ConfigData as WebConfigData).UserSettings.Add(ctrl.Setting);
                    ;
                });
            });
        }
    }
}
