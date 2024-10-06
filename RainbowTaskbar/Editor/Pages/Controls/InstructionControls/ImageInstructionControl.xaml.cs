using Microsoft.Win32;
using RainbowTaskbar.Configuration.Instruction.Instructions;
using RainbowTaskbar.Editor.Pages.Edit;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
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

namespace RainbowTaskbar.Editor.Pages.Controls.InstructionControls {
    /// <summary>
    /// Interaction logic for ImageInstructionControl.xaml
    /// </summary>
    public partial class ImageInstructionControl : UserControl {
        public ImageInstructionControl() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new OpenFileDialog {
                Filter = GetImageFilter(),
                CheckFileExists = true
            };
            if (openFileDialog.ShowDialog() == true) {
                ImagePathTextBox.Text = openFileDialog.FileName;
                ((DataContext as InstructionEditPage).SelectedInstruction as ImageInstruction).Path = openFileDialog.FileName;
            }
        }

        public string GetImageFilter() {
            var allImageExtensions = new StringBuilder();
            var separator = "";
            var codecs = ImageCodecInfo.GetImageEncoders();
            var images = new Dictionary<string, string>();
            foreach (var codec in codecs) {
                allImageExtensions.Append(separator);
                allImageExtensions.Append(codec.FilenameExtension);
                separator = ";";
                images.Add($"{codec.FormatDescription} Files: ({codec.FilenameExtension})",
                    codec.FilenameExtension);
            }

            var sb = new StringBuilder();
            if (allImageExtensions.Length > 0) sb.Append($"All Images|{allImageExtensions.ToString()}");
            images.Add("All Files", "*.*");
            foreach (var image in images) sb.Append($"|{image.Key}|{image.Value}");
            return sb.ToString();
        }
    }
}
