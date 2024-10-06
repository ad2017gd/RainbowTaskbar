using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace RainbowTaskbar.V2Legacy.Configuration.Instructions;

[DataContract]
internal class ImageInstruction : Instruction {

    [field: DataMember] public int Layer { get; set; } = 0;

    [field: DataMember] public string Path { get; set; } = "";

    [field: DataMember] public int X { get; set; } = 0;

    [field: DataMember] public int Y { get; set; } = 0;

    [field: DataMember] public int Width { get; set; } = 0;

    [field: DataMember] public int Height { get; set; } = 0;

    [field: DataMember] public double Opacity { get; set; } = 1;

    [field: DataMember] public bool DrawOnce { get; set; } = false;

    public override string Description {
        get {
            return "";
        }
    }


    public override bool Execute(Taskbar window, CancellationToken token) {
        
        return false;
    }

}