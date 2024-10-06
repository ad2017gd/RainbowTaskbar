using RainbowTaskbar.Interpolation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RainbowTaskbar.Editor.Pages.Controls.InstructionControls.Converters {
    public class ColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is not null ? ((Color) value).ToMediaColor() : null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is not null ? ((System.Windows.Media.Color) value).ToDrawingColor() : null;
    }
}
