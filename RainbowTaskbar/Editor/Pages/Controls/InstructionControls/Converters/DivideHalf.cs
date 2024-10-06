using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RainbowTaskbar.Editor.Pages.Controls.InstructionControls.Converters {
    public class DivideHalf : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            double p;
            if (double.TryParse(value.ToString(), out p)) return p / 2.0;
            else return 0;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            double p;
            if (double.TryParse(value.ToString(), out p)) return p * 2.0;
            else return 0;
        }
    }
}
