using Microsoft.Win32;
using RainbowTaskbar.Configuration.Instructions;
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

namespace RainbowTaskbar.UserControls
{
    /// <summary>
    /// Interaction logic for ImageInstructionControl.xaml
    /// </summary>
    public partial class ImageInstructionControl : UserControl
    {
        public ImageInstructionControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() {
                Filter = "Image Files|*.BMP;*.JPG;*.PNG"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                ImagePathTextBox.Text = openFileDialog.FileName;
                ((DataContext as EditorViewModel).SelectedInstruction as ImageInstruction).Path = openFileDialog.FileName;
            }

        }
    }
}
