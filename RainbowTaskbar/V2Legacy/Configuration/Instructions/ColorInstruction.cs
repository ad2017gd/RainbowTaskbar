
using System.Drawing;
using System.Dynamic;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Media;
using RainbowTaskbar.Interpolation;
using Color = System.Drawing.Color;
using Brush = System.Windows.Media.Brush;
using RainbowTaskbar.Languages;
using RainbowTaskbar.Configuration.InstructionConfig;

namespace RainbowTaskbar.Configuration.Instructions;

[DataContract]
internal class ColorInstruction : Instruction {

    public override string Description {
        get {
            return "";
        }
    }
    public enum ColorInstructionEffect {
        Solid,
        Fade,
        Gradient,
        FadeGradient
    }

    public enum ColorInstructionTransition {
        Linear,
        Sine,
        Cubic,
        Exponential,
        Back
    }

    [field: DataMember] public int Time { get; set; } = 1;

    [field: DataMember] public Color Color1 { get; set; } = Color.FromArgb(0, 0, 0);

    [field: DataMember] public ColorInstructionEffect Effect { get; set; }

    [field: DataMember] public Color Color2 { get; set; } = Color.FromArgb(0, 0, 0);

    [field: DataMember] public int Time2 { get; set; } = 1;

    [field: DataMember] public ColorInstructionTransition Transition { get; set; }

    [field: DataMember] public double Angle { get; set; } = 0;

    [field: DataMember] public int Layer { get; set; } = 0;

    [field: DataMember] public bool Randomize { get; set; } = false;

    public bool Has2Colors { get => Effect == ColorInstructionEffect.FadeGradient || Effect == ColorInstructionEffect.Gradient; }

    public override bool Execute(Taskbar window, CancellationToken token) {
        return true;
    }
}