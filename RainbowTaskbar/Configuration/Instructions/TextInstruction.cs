using System.Dynamic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RainbowTaskbar.Interpolation;
using Color = System.Drawing.Color;

namespace RainbowTaskbar.Configuration.Instructions;

[DataContract]
internal class TextInstruction : Instruction {

    [JsonIgnore]
    public bool drawn = false;

    [DataMember] public int Layer { get; set; } = 1;

    [DataMember] public string Text { get; set; }

    [DataMember] public int X { get; set; } = 0;

    [DataMember] public int Y { get; set; } = 0;

    [DataMember] public string Font { get; set; } = "Arial";

    [DataMember] public int Size { get; set; } = 32;

    [DataMember] public bool DrawOnce { get; set; } = false;

    [DataMember] public Color Color { get; set; } = Color.Black;
    
    public override bool Execute(Taskbar window, CancellationToken _) {
        if(drawn && DrawOnce) {
            return false;
        }

        window.Dispatcher.Invoke( () => 
            window.layers.DrawText(Layer, Text, X, Y, Size, "Arial", new SolidColorBrush(Color.ToMediaColor()))
        );

        if (DrawOnce) drawn = true;

        return false;
    }
}