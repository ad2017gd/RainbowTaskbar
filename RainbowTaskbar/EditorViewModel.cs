using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using RainbowTaskbar.Configuration;
using RainbowTaskbar.Helpers;
using RainbowTaskbar.UserControls;
using RainbowTaskbar.WebSocketServices;

namespace RainbowTaskbar;

public class EditorViewModel : INotifyPropertyChanged {
    public EditorViewModel() {
        AddInstructionCommandImpl = new AddInstructionCommand(this);
        RemoveInstructionCommandImpl = new RemoveInstructionCommand(this);

        SetPresetCommandImpl = new SetPresetCommand(this);
        DeletePresetCommandImpl = new DeletePresetCommand(this);
        InstructionsToPresetCommandImpl = new InstructionsToPresetCommand(this);
    }

    public string[] InstalledFonts { get; set; } =
        new InstalledFontCollection().Families.Select(font => font.Name).ToArray();

    public Config Config {
        get => App.Config;
        set => App.Config = value;
    }

    public ICommand AddInstructionCommandImpl { get; set; }
    public ICommand RemoveInstructionCommandImpl { get; set; }

    public ICommand SetPresetCommandImpl { get; set; }
    public ICommand DeletePresetCommandImpl { get; set; }
    public ICommand InstructionsToPresetCommandImpl { get; set; }

    public Instruction SelectedInstruction { get; set; }
    public int? SelectedInstructionIndex { get; set; }

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

    public bool RunAtStartup {
        get => (string?) Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run")
            .GetValue("RainbowTaskbar") == Process.GetCurrentProcess().MainModule.FileName;

        set {
            var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (value)
                key.SetValue("RainbowTaskbar", Process.GetCurrentProcess().MainModule.FileName);
            else
                key.DeleteValue("RainbowTaskbar");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
}

public class FormatInstructionNameValueConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Regex
        .Replace(value.ToString().Replace("Instruction", ""), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0")
        .TrimStart();

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class TransparencyStyleNameValueConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (value.ToString() != "Style" ? 
        (value.ToString() == "All" ? "Both transparencies" : value.ToString() + " transparency") : 
        "Taskbar style");

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class FloatToPercentageValueConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        try {
            return (int) ((double) value * 100);
        }
        catch {
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        try {
            return double.Parse(value.ToString()) / 100;
        }
        catch {
            return null;
        }
    }
}

public class DivideHalf : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (int) value / 2;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        (double) value * 2;
}

public class NumericValidationRule : ValidationRule {
    public Type ValidationType { get; set; }

    public double? NumMinValue { get; set; }

    public double? NumMaxValue { get; set; }


    public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
        var strValue = Convert.ToString(value);

        if (string.IsNullOrEmpty(strValue))
            return new ValidationResult(false, "Value cannot be coverted to string.");
        var canConvert = false;
        switch (ValidationType.Name) {
            case "Boolean":
                var boolVal = false;
                canConvert = bool.TryParse(strValue, out boolVal);
                return canConvert
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, "Input should be type of boolean");
            case "Int32":
                var intVal = 0;
                canConvert = int.TryParse(strValue, out intVal);
                if (canConvert && NumMinValue is not null && intVal < NumMinValue)
                    return new ValidationResult(false, "Input value is less than minimum value allowed");
                if (canConvert && NumMaxValue is not null && intVal > NumMaxValue)
                    return new ValidationResult(false, "Input value is higher than maximum value allowed");
                return canConvert
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, "Input should be type of Int32");
            case "Double":
                double doubleVal = 0;
                if (strValue.EndsWith(".")) return new ValidationResult(false, "Input value is invalid");
                canConvert = double.TryParse(strValue, out doubleVal);
                if (canConvert && NumMinValue is not null && doubleVal < NumMinValue)
                    return new ValidationResult(false, "Input value is less than minimum value allowed");
                if (canConvert && NumMaxValue is not null && doubleVal > NumMaxValue)
                    return new ValidationResult(false, "Input value is higher than maximum value allowed");
                return canConvert
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, "Input should be type of Double");
            case "Int64":
                long longVal = 0;
                canConvert = long.TryParse(strValue, out longVal);
                return canConvert
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, "Input should be type of Int64");
            default:
                throw new InvalidCastException($"{ValidationType.Name} is not supported");
        }
    }
}

public class FileExistsValidationRule : ValidationRule {
    public override ValidationResult Validate(object value, CultureInfo cultureInfo) => File.Exists(value?.ToString())
        ? ValidationResult.ValidResult
        : new ValidationResult(false, "File must exist");
}

public class AddInstructionCommand : ICommand {
    private readonly EditorViewModel vm;

    public AddInstructionCommand(EditorViewModel vm) {
        this.vm = vm;
    }

    public event EventHandler CanExecuteChanged {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object parameter) => parameter is Type;

    public void Execute(object parameter) {
        var index = vm.SelectedInstructionIndex + 1 ?? 0;
        var instruction = (parameter as Type).GetConstructor(Array.Empty<Type>()).Invoke(null) as Instruction;

        App.Config.Instructions.Insert(index, instruction);

        var data = new JObject {
            { "type", "InstructionAdded" },
            { "index", index },
            { "instruction", instruction.ToJSON() }
        };
        WebSocketAPIServer.SendToSubscribed(data.ToString());
    }
}

public class RemoveInstructionCommand : ICommand {
    private readonly EditorViewModel vm;

    public RemoveInstructionCommand(EditorViewModel vm) {
        this.vm = vm;
    }

    public event EventHandler CanExecuteChanged {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object parameter) => vm.SelectedInstructionIndex > -1;

    public void Execute(object parameter) {
        if (vm.SelectedInstructionIndex is { } selectedInstructionIndex && selectedInstructionIndex != -1) {
            var instruction = App.Config.Instructions[selectedInstructionIndex];

            var data = new JObject {
                {"type", "InstructionRemoved"},
                {"index", selectedInstructionIndex},
                {"instruction", instruction.ToJSON()}
            };
            WebSocketAPIServer.SendToSubscribed(data.ToString());

            App.Config.Instructions.RemoveAt(selectedInstructionIndex);
        }
    }
}

public class SetPresetCommand : ICommand {
    private readonly EditorViewModel vm;

    public SetPresetCommand(EditorViewModel vm) {
        this.vm = vm;
    }

    public bool CanExecute(object parameter) => parameter is InstructionPreset;

    public void Execute(object parameter) {
        App.Config.Instructions = (parameter as InstructionPreset)!.Instructions.DeepClone();
        App.Config.ToFile();
        App.ReloadTaskbars();
    }

    public event EventHandler CanExecuteChanged {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}

public class DeletePresetCommand : ICommand {
    private readonly EditorViewModel vm;

    public DeletePresetCommand(EditorViewModel vm) {
        this.vm = vm;
    }

    public bool CanExecute(object parameter) => parameter is InstructionPreset;

    public void Execute(object parameter) {
        App.Config.Presets.Remove(parameter as InstructionPreset);
        
        App.Config.ToFile();
    }

    public event EventHandler CanExecuteChanged {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}

public class InstructionsToPresetCommand : ICommand {
    private readonly EditorViewModel vm;

    public InstructionsToPresetCommand(EditorViewModel vm) {
        this.vm = vm;
    }

    public bool CanExecute(object parameter) => App.Config.Instructions.Count > 0;

    public void Execute(object parameter) {
        var dialog = new PresetNameDialog();
        if (dialog.ShowDialog() is true)
            App.Config.Presets.Add(new InstructionPreset {
                Name = dialog.NameTextBox.Text,
                Instructions = App.Config.Instructions.DeepClone()
            });
        App.Config.ToFile();
    }

    public event EventHandler CanExecuteChanged {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}