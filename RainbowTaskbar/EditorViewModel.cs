using GongSolutions.Wpf.DragDrop;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using RainbowTaskbar.Configuration;
using RainbowTaskbar.UserControls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace RainbowTaskbar
{
    public class EditorViewModel : INotifyPropertyChanged
    {
        public string[] InstalledFonts { get; set; } = new InstalledFontCollection().Families.Select(font => font.Name).ToArray();
        public Config Config
        {
            get => App.Config;
            set => App.Config = value;
        }

        public ICommand AddInstructionCommandImpl { get; set; }
        public ICommand RemoveInstructionCommandImpl { get; set; }

        public Instruction SelectedInstruction { get; set; }
        public int? SelectedInstructionIndex { get; set; }

        [DependsOn("SelectedInstruction")]
        public object SelectedInstructionControl
        {
            get
            {
                if (SelectedInstruction is null) return null;
                switch (SelectedInstruction.Name)
                {
                    case "Delay": return new DelayInstructionControl();
                    case "Transparency": return new TransparencyInstructionControl();
                    case "Color": return new ColorInstructionControl();
                    case "Border Radius": return new BorderRadiusInstructionControl();
                    case "Clear Layer": return new ClearLayerInstructionControl();
                    case "Image": return new ImageInstructionControl();
                    case "Text": return new TextInstructionControl();
                    default:
                        break;
                }
                return null;
            }
        }

        public bool RunAtStartup
        {
            get => Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run").GetValue("RainbowTaskbar") is not null;

            set
            {
                

                var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (value)
                    key.SetValue("RainbowTaskbar", Process.GetCurrentProcess().MainModule.FileName);
                else
                    key.DeleteValue("RainbowTaskbar");

            }
        }

        public bool EnableAPI
        {
            get => App.Config.IsAPIEnabled;

            set => App.Config.IsAPIEnabled = value;

        }

        public int APIPort
        {
            get => App.Config.APIPort;

            set => App.Config.APIPort = value;

        }

        public event PropertyChangedEventHandler PropertyChanged;

        public EditorViewModel()
        {
            AddInstructionCommandImpl = new AddInstructionCommand(this);
            RemoveInstructionCommandImpl = new RemoveInstructionCommand(this);
        }

    }

    public class FormatInstructionNameValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Regex.Replace((value.ToString()).Replace("Instruction", ""), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0").TrimStart();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FloatToPercentageValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return (int)((double)value * 100);
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return double.Parse(value.ToString()) / 100;
            }
            catch
            {
                return null;
            }
        }
    }

    public class NumericValidationRule : ValidationRule
    {
        public Type ValidationType { get; set; }

        public double? NumMinValue { get; set; }

        public double? NumMaxValue { get; set; }


        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string strValue = Convert.ToString(value);

            if (string.IsNullOrEmpty(strValue))
                return new ValidationResult(false, $"Value cannot be coverted to string.");
            bool canConvert = false;
            switch (ValidationType.Name)
            {
                case "Boolean":
                    bool boolVal = false;
                    canConvert = bool.TryParse(strValue, out boolVal);
                    return canConvert ? new ValidationResult(true, null) : new ValidationResult(false, $"Input should be type of boolean");
                case "Int32":
                    int intVal = 0;
                    canConvert = int.TryParse(strValue, out intVal);
                    if (canConvert && NumMinValue is not null && intVal < NumMinValue) return new ValidationResult(false, $"Input value is less than minimum value allowed");
                    if (canConvert && NumMaxValue is not null && intVal > NumMaxValue) return new ValidationResult(false, $"Input value is higher than maximum value allowed");
                    return canConvert ? new ValidationResult(true, null) : new ValidationResult(false, $"Input should be type of Int32");
                case "Double":
                    double doubleVal = 0;
                    if (strValue.EndsWith(".")) return new ValidationResult(false, $"Input value is invalid");
                    canConvert = double.TryParse(strValue, out doubleVal);
                    if (canConvert && NumMinValue is not null && doubleVal < NumMinValue) return new ValidationResult(false, $"Input value is less than minimum value allowed");
                    if (canConvert && NumMaxValue is not null && doubleVal > NumMaxValue) return new ValidationResult(false, $"Input value is higher than maximum value allowed");
                    return canConvert ? new ValidationResult(true, null) : new ValidationResult(false, $"Input should be type of Double");
                case "Int64":
                    long longVal = 0;
                    canConvert = long.TryParse(strValue, out longVal);
                    return canConvert ? new ValidationResult(true, null) : new ValidationResult(false, $"Input should be type of Int64");
                default:
                    throw new InvalidCastException($"{ValidationType.Name} is not supported");
            }
        }
    }

    public class AddInstructionCommand : ICommand
    {
        EditorViewModel vm;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public AddInstructionCommand(EditorViewModel vm)
        {
            this.vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return parameter as Type is not null;
        }

        public void Execute(object parameter)
        {
            var index = vm.SelectedInstructionIndex + 1 ?? 0;
            var instruction = (parameter as Type).GetConstructor(Array.Empty<Type>()).Invoke(null) as Instruction;

            App.Config.Instructions.Insert(index, instruction);
            
            JObject data = new JObject();
            data.Add("type", "InstructionAdded");
            data.Add("index", index);
            data.Add("instruction", instruction.ToJSON());
            WebSocketServices.WebSocketAPIServer.SendToSubscribed(data.ToString());
        }
    }

    public class RemoveInstructionCommand : ICommand
    {
        EditorViewModel vm;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RemoveInstructionCommand(EditorViewModel vm)
        {
            this.vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return vm.SelectedInstructionIndex > -1;
        }

        public void Execute(object parameter)
        {
            if (vm.SelectedInstructionIndex is int SelectedInstructionIndex && SelectedInstructionIndex != -1)
            {

                var index = SelectedInstructionIndex;
                var instruction = App.Config.Instructions[index];

                JObject data = new JObject();
                data.Add("type", "InstructionRemoved");
                data.Add("index", index);
                data.Add("instruction", instruction.ToJSON());
                WebSocketServices.WebSocketAPIServer.SendToSubscribed(data.ToString());

                App.Config.Instructions.RemoveAt(index);

            }
        }
    }

    public class DivideHalf : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value * 2;
        }
    }
}
