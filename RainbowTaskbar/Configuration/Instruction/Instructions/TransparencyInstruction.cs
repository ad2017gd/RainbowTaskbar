using System;
using System.Dynamic;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Controls;
using RainbowTaskbar.Helpers;
using RainbowTaskbar.Languages;

namespace RainbowTaskbar.Configuration.Instruction.Instructions;


public class TransparencyInstruction : Instruction {
    public enum TransparencyInstructionStyle {
        Default,
        Blur,
        Transparent
    }

    public enum TransparencyInstructionType {
        Taskbar,
        RainbowTaskbar,
        All,
        Style,
        Layer
    }

    public TransparencyInstructionType Type { get; set; }

    public double Opacity { get; set; } = 1;

    public double TaskbarOpacity { get => App.Settings.GlobalOpacity == -1 ? Opacity : App.Settings.GlobalOpacity; }

    public TransparencyInstructionStyle Style { get; set; }

    public int Layer { get; set; }

    public override string Description {
        get {
            /*return Type == TransparencyInstructionType.Style ? $"Taskbar style - {Style.ToString()}" :
                (Type == TransparencyInstructionType.Layer ? $"Layer {Layer} - {Math.Round(Opacity * 100)}% opacity" :
                $"{Type.ToString()} - {Math.Round(Opacity * 100)}% opacity");*/
            if (Type == TransparencyInstructionType.Style) {
                return App.localization.InstructionFormatSuffix(this, "style", Style.ToStringLocalized());
            }
            else if (Type == TransparencyInstructionType.Layer) {
                return App.localization.InstructionFormatSuffix(this, "layer", Layer, Math.Round(Opacity * 100));
            }
            else {
                return App.localization.InstructionFormatSuffix(this, "opacity", Type.ToStringLocalized(), Math.Round(Opacity * 100));
            }

        }
    }
    /*
    public override JObject ToJSON() {
        dynamic data = new ExpandoObject();
        data.Name = GetType().Name;
        data.Opacity = Opacity;
        data.Layer = Layer;
        data.Style = Style;
        data.Type = Type;

        return JObject.FromObject(data);
    }
    */

    public override bool Execute(Taskbar window, CancellationToken token) {
        switch (Type) {
            case TransparencyInstructionType.Taskbar:

                window.taskbarHelper.SetAlpha(TaskbarOpacity);
                break;

            case TransparencyInstructionType.RainbowTaskbar:
                window.Dispatcher.Invoke(() => {
                    if (window.Opacity != Opacity) window.Opacity = Opacity;
                    
                }, System.Windows.Threading.DispatcherPriority.Normal, token);
                break;

            case TransparencyInstructionType.All:
                window.Dispatcher.Invoke(() => {
                    if (window.Opacity != Opacity) window.Opacity = Opacity;
                }, System.Windows.Threading.DispatcherPriority.Normal, token);
                window.taskbarHelper.SetAlpha(TaskbarOpacity);
                break;

            case TransparencyInstructionType.Style:

                switch (Style) {
                    case TransparencyInstructionStyle.Default:
                        if (window.taskbarHelper.Style != TaskbarHelper.TaskbarStyle.Default)
                            window.taskbarHelper.Style = TaskbarHelper.TaskbarStyle.ForceDefault;
                        break;
                    case TransparencyInstructionStyle.Blur:
                        window.taskbarHelper.Style = TaskbarHelper.TaskbarStyle.Blur;
                        break;
                    case TransparencyInstructionStyle.Transparent:
                        window.taskbarHelper.Style = TaskbarHelper.TaskbarStyle.Transparent;
                        break;
                }

                ExplorerTAP.ExplorerTAP.SetAppearanceType(Style);

                break;

            case TransparencyInstructionType.Layer:
                
                
                window.Dispatcher.Invoke(() =>
                    window.canvasManager.canvases[Layer].Opacity = Opacity
                    , System.Windows.Threading.DispatcherPriority.Normal, token);
                break;
        }

        return false;
    }
}