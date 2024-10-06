using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RainbowTaskbar.Editor.Pages.Controls.InstructionControls.Converters {
    public class FileExists : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) => File.Exists(value?.ToString())
            ? ValidationResult.ValidResult
            : new ValidationResult(false, "File must exist");
    }
}
