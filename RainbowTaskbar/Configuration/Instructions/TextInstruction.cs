using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Media;
using RainbowTaskbar.Interpolation;
using Color = System.Drawing.Color;

namespace RainbowTaskbar.Configuration.Instructions;

[DataContract]
internal class TextInstruction : Instruction {
    [field: DataMember] public int Layer { get; set; } = 1;

    [field: DataMember] public string Text { get; set; }

    [field: DataMember] public int X { get; set; } = 0;

    [field: DataMember] public int Y { get; set; } = 0;

    [field: DataMember] public string Font { get; set; } = "Arial";

    [field: DataMember] public int Size { get; set; } = 32;

    [field: DataMember] public Color Color { get; set; } = Color.Black;

    public override bool Execute(Taskbar window, CancellationToken _) {
        window.Dispatcher.Invoke(() =>
            window.layers.DrawText(Layer, Text, X, Y, Size, "Arial", new SolidColorBrush(Color.ToMediaColor()))
        );
        return false;
    }
}