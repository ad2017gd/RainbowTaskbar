
using System;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using RainbowTaskbar.Interpolation;
using RainbowTaskbar.Languages;

namespace RainbowTaskbar.V2Legacy.Configuration.Instructions;

[DataContract]
public class ShapeInstruction : Instruction {

    [field: DataMember]
    public int Layer { get; set; } = 0;

    [field: DataMember]
    public ShapeInstructionShapes Shape { get; set; } = ShapeInstructionShapes.Line;

    [field: DataMember] public int X { get; set; } = 0;

    [field: DataMember] public int Y { get; set; } = 0;

    [field: DataMember] public int X2 { get; set; } = 0;

    [field: DataMember] public int Y2 { get; set; } = 0;

    [field: DataMember] public bool DrawOnce { get; set; } = false;

    [field: DataMember]
    public System.Drawing.Color Fill { get; set; } = System.Drawing.Color.FromArgb(255, 0, 0, 0);

    [field: DataMember]
    public System.Drawing.Color Line { get; set; } = System.Drawing.Color.FromArgb(255, 0, 0, 0);

    [field: DataMember]
    public int LineSize { get; set; } = 1;

    [field: DataMember]
    public int Radius { get; set; } = 0;

    [field: DataMember]
    public bool FitTaskbars { get; set; } = false;

    public override string Description {
        get {
            return "";
        }
    }

    public override bool Execute(Taskbar window, CancellationToken token) {
        

        return false;
    }

    public enum ShapeInstructionShapes {
        Line,
        Rectangle,
        Ellipse
    }

}
