using RainbowTaskbar.Configuration;
using RainbowTaskbar.Editor.Pages;
using RainbowTaskbar.Editor.Pages.Controls;
using RainbowTaskbar.Editor.Pages.Edit;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using Xceed.Wpf.AvalonDock.Controls;

namespace RainbowTaskbar.Editor {
    /// <summary>
    /// Interaction logic for EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : FluentWindow {

        public IContentDialogService contentDialogService;
        public EditorWindow() {

            SystemThemeWatcher.Watch(this);
            InitializeComponent();

            nav.Loaded += (_, _) => {
                nav.Navigate(typeof(Home));
                ApplicationThemeManager.ApplySystemTheme(true);
            };

            this.DataContext = App.editorViewModel;

            this.contentDialogService = new ContentDialogService();
            contentDialogService.SetDialogHost(RootContentDialogPresenter);

            App.localization.Enable(Resources.MergedDictionaries);


            if (!App.monacoExtracted) {
                Stream stream = new MemoryStream(Properties.Resources.monaco);
                if (Directory.Exists(App.monacoDir))
                    Directory.Delete(App.monacoDir, true);
                ZipFile.ExtractToDirectory(stream, App.monacoDir);
            }

            
        }

        Page current = null;

        public void OpenConfig(Config config) {
            App.editor.nav.Navigate(typeof(EmptyPageBadFix));

            var page = new ViewInfo(config);
            Task.Run(() => {
                Thread.Sleep(50);
                Dispatcher.Invoke(() => App.editor.nav.Navigate(typeof(EmptyPageBadFix2)));
                Thread.Sleep(50);
                Dispatcher.Invoke(() => App.editor.nav.ReplaceContent(page));
            });
        }
        private void nav_Navigating(NavigationView sender, NavigatingCancelEventArgs args) {
            // this is totally disgusting but wpf ui is cooked so whatever


            if(args.Page is Browse b) {
                b.OnSortChanged(); 
            }

            current.FindVisualChildren<UnsafeImage>().ToList().ForEach(x => {
                x.Dispose();
            });

            if (current is not null && current.GetType() != args.Page.GetType() && (current is Browse || App.editorViewModel.EditPage is InstructionEditPage) && args.Page is not EmptyPageBadFix) {
                

                
                args.Cancel = true;
                nav.Navigate(typeof(EmptyPageBadFix));
                Type t = args.Page.GetType();
                Task.Run(() => {
                    Thread.Sleep(50);
                    Dispatcher.Invoke(() => nav.Navigate(t));
                });
            }
            current = args.Page as Page;
            

            
            if (App.editorViewModel.EditPage is not null && App.editorViewModel.EditPage is InstructionEditPage) {
                var page = App.editorViewModel.EditPage as InstructionEditPage;
                page.Current.Stop();
                if (App.Settings.SelectedConfig is not null) App.Settings.SelectedConfig.Start();
            }
            if (App.editorViewModel.EditPage is not null && App.editorViewModel.EditPage is WebEditPage) {
                if (App.Settings.SelectedConfig is not null) App.Settings.SelectedConfig.Start();
            }

            if (App.editorViewModel.EditPage is not null && App.editorViewModel.EditPage.Modified) {

                args.Cancel = true;
                
                if(App.editorViewModel.EditPage is WebEditPage) {
                    var page = App.editorViewModel.EditPage as WebEditPage;
                    page.webView.Visibility = Visibility.Hidden;
                }

                var task = contentDialogService.ShowSimpleDialogAsync(
                    new SimpleContentDialogCreateOptions() {
                        Title = App.localization.Get("msgbox_save_title"),
                        Content = App.localization.Get("msgbox_save"),
                        PrimaryButtonText = App.localization.Get("msgbox_save_b1"),
                        SecondaryButtonText = App.localization.Get("msgbox_save_b2"),
                        CloseButtonText = App.localization.Get("msgbox_button_cancel"),
                });

                Task.Run(() => {
                    var res = task.Result;
                    var saving = false;

                    switch (res) {
                        case ContentDialogResult.Primary:
                            if (App.editorViewModel.EditPage is WebEditPage) {
                                var page = App.editorViewModel.EditPage as WebEditPage;

                                // INSANE!
                                Dispatcher.Invoke(() => {
                                    saving = true;
                                    page.Save();
                                    Task.Run(() => {
                                        Thread.Sleep(1500);
                                        saving = false;
                                        Dispatcher.Invoke(() => {
                                            try {
                                                page.webView.Dispose();
                                            }
                                            catch { }
                                        });
                                    });
                                });
                            }
                            goto case ContentDialogResult.Secondary;
                        case ContentDialogResult.Secondary:
                            Dispatcher.Invoke(() => {
                                // weird COM error sometimes?
                                try {
                                    if(!saving) (App.editorViewModel.EditPage as WebEditPage).webView.Dispose();
                                }
                                catch { }
                            });
                            App.editorViewModel.EditPage = null;
                            Dispatcher.Invoke(() => sender.Navigate(args.Page.GetType()));
                            Dispatcher.Invoke(() => { if (App.Settings.SelectedConfig is not null) App.Settings.SelectedConfig.Start(); });
                            break;
                        case ContentDialogResult.None:
                            if (App.editorViewModel.EditPage is WebEditPage) {
                                var page = App.editorViewModel.EditPage as WebEditPage;
                                Dispatcher.Invoke(() => page.webView.Visibility = Visibility.Visible);
                            }
                            break;
                    }
                });
                return;
                
            }
            if (App.editorViewModel.EditPage is not null && App.editorViewModel.EditPage is WebEditPage) {
                (App.editorViewModel.EditPage as WebEditPage).webView.Dispose();
            }
            if(App.editorViewModel.EditPage is not null) {
                App.editorViewModel.EditPage = null;
            }

            ApplicationThemeManager.ApplySystemTheme(true);
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            App.editor = null;
            nav.FindVisualChildren<UnsafeImage>().ToList().ForEach(x => {
                x.Dispose();
            });
            //e.Cancel = true;
            //Hide();
        }
        DebugWindow curDeb = null;
        private void FluentWindow_KeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.F12) {
                if(curDeb is not null) {
                    curDeb.Close();
                }
                curDeb = new DebugWindow();
                curDeb.Show();
            }
        }
    }
}
