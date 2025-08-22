using PropertyChanged;
using RainbowTaskbar.Configuration;
using RainbowTaskbar.Configuration.Instruction;
using RainbowTaskbar.Editor.Pages.Controls.InstructionControls;
using RainbowTaskbar.Languages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Text.Json;
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
using Xceed.Wpf.AvalonDock.Controls;

namespace RainbowTaskbar.Editor.Pages.Edit {
    /// <summary>
    /// Interaction logic for InstructionEditPage.xaml
    /// </summary>
    public partial class InstructionEditPage : EditPage, INotifyPropertyChanged {

        public InstructionConfig IConfig { get => Config as  InstructionConfig; set => Config = value; }

        public InstructionConfig Current { get; set; }
        public Instruction SelectedInstruction { get; set; }

        public string[] InstalledFonts { get; set; } =
            new InstalledFontCollection().Families.Select(font => font.Name).ToArray();

        [DependsOn("SelectedInstruction")]
        public UserControl SelectedInstructionControl {
            get {
                if (SelectedInstruction is null) return null;
                switch (SelectedInstruction.GetType().Name) {
                    case "DelayInstruction": return new DelayInstructionControl();
                    case "TransparencyInstruction": return new TransparencyInstructionControl();
                    case "ColorInstruction": return new ColorInstructionControl();
                    case "BorderRadiusInstruction": return new BorderRadiusInstructionControl();
                    case "ClearLayerInstruction": return new ClearLayerInstructionControl();
                    case "ImageInstruction": return new ImageInstructionControl();
                    case "TextInstruction": return new TextInstructionControl();
                    case "ShapeInstruction": return new ShapeInstructionControl();
                }

                return null;
            }
        }

        public void OnSelectedInstructionChanged() {
            ;
        }
        public InstructionEditPage(InstructionConfig config) {
            InitializeComponent();
            DataContext = this;
            ApplicationThemeManager.ApplySystemTheme(true);
            App.localization.Enable(Resources.MergedDictionaries);

            Config = config;
            Current = config.Copy() as InstructionConfig;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count == 0) return;
            var others = this.FindVisualChildren<ListBox>().Where(x => x != (sender as ListBox)).ToList() ;
            others.ForEach(x=>x.SelectedItem = null);

            SelectedInstruction = (sender as ListBox).SelectedItem as Instruction;
        }

        private void AddGroup(object sender, RoutedEventArgs e) {
            Current.Data.LoopGroups.Add(new());
        }

        private void AddInstruction(object sender, RoutedEventArgs e) {
            var btn = sender as Wpf.Ui.Controls.Button;
            btn.ContextMenu = new();
            Instruction.DisplayableInstructionTypes.ToList().ForEach(x => {
                var v = new Wpf.Ui.Controls.MenuItem() { Header = App.localization.Get(x.Name.ToLower()) };
                v.Click += (_, _) => {
                    (btn.DataContext as InstructionGroup).Instructions.Add(x.GetConstructor(Array.Empty<Type>()).Invoke(null) as Instruction);
                };
                btn.ContextMenu.Items.Add(v);
            });
            btn.ContextMenu.IsOpen = true;
            ;
        }

        private void DeleteGroup(object sender, RoutedEventArgs e) {
            var btn = sender as Wpf.Ui.Controls.Button;
            Current.Data.LoopGroups.Remove(btn.DataContext as InstructionGroup);
        }
        
        private void RunConfig(object sender, RoutedEventArgs e) {
            if(App.Settings.SelectedConfig is not null) App.Settings.SelectedConfig.Stop();
            App.ReloadTaskbars(false);
            Taskbar.SoftReset(true, Current);
        }

        private void SaveConfig(object sender, RoutedEventArgs e) {
            Current.Updated = DateTime.Now;
            Current.ToFile();
            var idx = App.Configs.IndexOf(Config);
            App.Configs.RemoveAt(idx);
            App.Configs.Insert(idx, Current);
            Config = Current;
            Current = Current.Copy() as InstructionConfig;
        }
    }
}
