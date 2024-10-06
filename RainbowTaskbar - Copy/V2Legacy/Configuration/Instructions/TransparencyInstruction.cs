using System;
using System.Dynamic;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Controls;
using RainbowTaskbar.Helpers;
using RainbowTaskbar.Languages;

namespace RainbowTaskbar.V2Legacy.Configuration.Instructions;

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

    [field: DataMember] public TransparencyInstructionType Type { get; set; }

    [field: DataMember] public double Opacity { get; set; } = 1;

    [field: DataMember] public TransparencyInstructionStyle Style { get; set; }

    [field: DataMember] public int Layer { get; set; }

    public override string Description {
        get {
            return "";

        }
    }

    public override bool Execute(Taskbar window, CancellationToken token) {
        return false;
    }
}