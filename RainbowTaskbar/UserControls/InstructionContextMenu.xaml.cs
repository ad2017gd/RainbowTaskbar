﻿using RainbowTaskbar.Configuration.Instructions;
using RainbowTaskbar.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace RainbowTaskbar.UserControls {
    /// <summary>
    /// Interaction logic for InstructionContextMenu.xaml
    /// </summary>
    public partial class InstructionContextMenu : ContextMenu {
        public InstructionContextMenu() {
            InitializeComponent();
        }


        private void Duplicate_Click(object sender, RoutedEventArgs e) {
            App.Config.Instructions.Insert((App.editorViewModel.SelectedInstructionIndex ?? 0)+1, ObjectCopier.DeepClone(App.editorViewModel.SelectedInstruction));
        }

        private void Delete_Click(object sender, RoutedEventArgs e) {
            App.Config.Instructions.RemoveAt(App.editorViewModel.SelectedInstructionIndex ?? 0);
        }

        private void DuplicateRevColor_Click(object sender, RoutedEventArgs e) {
            var obj = ObjectCopier.DeepClone(App.editorViewModel.SelectedInstruction) as ColorInstruction;
            var tmp = obj.Color1;
            obj.Color1 = obj.Color2;
            obj.Color2 = tmp;
            App.Config.Instructions.Insert((App.editorViewModel.SelectedInstructionIndex ?? 0)+1, obj);
        }
    }
}
