using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using static RainbowTaskbar.Configuration.Instruction.Instructions.ColorInstruction;

namespace RainbowTaskbar.Editor.Pages.Controls.InstructionControls {
    /// <summary>
    /// Interaction logic for ColorInstructionControl.xaml
    /// </summary>
    public partial class ColorInstructionControl : UserControl {
        public ColorInstructionControl() {
            InitializeComponent();
            Task.Run(() => {
                Thread.Sleep(100);
                Dispatcher.Invoke(() => {
                    ColorPicker1_SelectedColorChanged(null, null);
                });
            });
        }

        private void ColorPicker1_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (SelectedEffect.SelectedItem is ColorInstructionEffect instructionEffect) {
                if (instructionEffect == ColorInstructionEffect.Gradient ||
                    instructionEffect == ColorInstructionEffect.FadeGradient)
                    if (ColorPicker1.SelectedColor.HasValue && ColorPicker2.SelectedColor.HasValue)
                        PreviewRectangle.Fill = new LinearGradientBrush(ColorPicker1.SelectedColor.Value,
                            ColorPicker2.SelectedColor.Value, AnglePicker.Value ?? 0);
                if (instructionEffect == ColorInstructionEffect.Fade || instructionEffect == ColorInstructionEffect.Solid)
                    if (ColorPicker1.SelectedColor.HasValue)
                        PreviewRectangle.Fill = new SolidColorBrush(ColorPicker1.SelectedColor.Value);
            }
        }

        private void Randomize_Checked(object sender, RoutedEventArgs e) {
            if ((bool) Randomize.IsChecked) {
                var rnd = new Random();
                var Color1 = Color.FromRgb((byte) rnd.Next(0, 255), (byte) rnd.Next(0, 255), (byte) rnd.Next(0, 255));
                var Color2 = Color.FromRgb((byte) rnd.Next(0, 255), (byte) rnd.Next(0, 255), (byte) rnd.Next(0, 255));
                if (SelectedEffect.SelectedItem is ColorInstructionEffect instructionEffect) {
                    if (instructionEffect == ColorInstructionEffect.Gradient ||
                        instructionEffect == ColorInstructionEffect.FadeGradient)
                        PreviewRectangle.Fill = new LinearGradientBrush(Color1, Color2,  AnglePicker.Value ?? 0);
                    if (instructionEffect == ColorInstructionEffect.Fade ||
                        instructionEffect == ColorInstructionEffect.Solid)
                        PreviewRectangle.Fill = new SolidColorBrush(Color1);
                }
            }
            else {
                ColorPicker1_SelectedColorChanged(null, null);
            }
        }

        public void UpdateColors() {

            ColorPicker1_SelectedColorChanged(null, null);
        }
    }
}
