using System.Runtime.Serialization;
using System.Threading;
using RainbowTaskbar.Helpers;

namespace RainbowTaskbar.Configuration.Instructions;

[DataContract]
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

    [DataMember] public TransparencyInstructionType Type { get; set; }

    [DataMember] public double Opacity { get; set; } = 1;

    [DataMember] public TransparencyInstructionStyle Style { get; set; }

    [DataMember] public int Layer { get; set; }

    public override bool Execute(Taskbar window, CancellationToken _) {
        switch (Type) {
            case TransparencyInstructionType.Taskbar:

                window.taskbarHelper.SetAlpha(Opacity);
                break;

            case TransparencyInstructionType.RainbowTaskbar:
                window.Dispatcher.Invoke(() => {
                    if (window.Opacity != Opacity) window.Opacity = Opacity;
                });
                break;

            case TransparencyInstructionType.All:
                window.Dispatcher.Invoke(() => {
                    if (window.Opacity != Opacity) window.Opacity = Opacity;
                });
                window.taskbarHelper.SetAlpha(Opacity);
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

                break;

            case TransparencyInstructionType.Layer:
                window.Dispatcher.Invoke(() =>
                    window.layers.canvases[Layer].Opacity = Opacity
                );
                break;
        }

        return false;
    }
}