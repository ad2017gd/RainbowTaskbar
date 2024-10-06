using RainbowTaskbar.Configuration.Instruction;
using RainbowTaskbar.Editor.Pages.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace RainbowTaskbar.Editor.Pages.Controls
{
    /// <summary>
    /// Interaction logic for InstructionContextMenu.xaml
    /// </summary>
    public partial class InstructionContextMenu : ContextMenu
    {
        public static readonly DependencyProperty ListBoxProperty =
                   DependencyProperty.Register(
                         "ParentListBox",
                          typeof(ListBox),
                          typeof(InstructionContextMenu));

        public ListBox ParentListBox {
            get {
                return (ListBox) GetValue(ListBoxProperty);
            }
            set {
                SetValue(ListBoxProperty, value);
            }
        }
        public InstructionContextMenu()
        {
            InitializeComponent();
        }

        private void Duplicate_Click(object sender, RoutedEventArgs e) {
            var list = (ParentListBox.ItemsSource as BindingList<Instruction>);
            list.Insert(ParentListBox.SelectedIndex+1, JsonSerializer.Deserialize<Instruction>(JsonSerializer.Serialize(ParentListBox.SelectedItem as Instruction)));
        }

        private void Delete_Click(object sender, RoutedEventArgs e) {
            (ParentListBox.ItemsSource as BindingList<Instruction>).Remove(ParentListBox.SelectedItem as Instruction);
        }
    }
}
