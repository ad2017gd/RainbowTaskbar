using System.Dynamic;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RainbowTaskbar.Interpolation;
using Color = System.Drawing.Color;

namespace RainbowTaskbar.V2Legacy.Configuration.Instructions;

[DataContract]
internal class TextInstruction : Instruction {


    [field: DataMember] public int Layer { get; set; } = 1;

    [field: DataMember] public string Text { get; set; } = "";

    [field: DataMember] public int X { get; set; } = 0;

    [field: DataMember] public int Y { get; set; } = 0;

    [field: DataMember] public string Font { get; set; } = "Arial";

    [field: DataMember] public int Size { get; set; } = 32;

    [field: DataMember] public bool DrawOnce { get; set; } = false;

    [field: DataMember] public Color Color { get; set; } = Color.Black;

    [field: DataMember] public bool Center { get; set; } = false;

    public override string Description {
        get {
            return "";
        }
    }


    public override bool Execute(Taskbar window, CancellationToken token) {
        
        return false;
    }
}