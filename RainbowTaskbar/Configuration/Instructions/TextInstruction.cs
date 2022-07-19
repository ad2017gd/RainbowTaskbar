using System.Dynamic;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using RainbowTaskbar.Interpolation;
using Color = System.Drawing.Color;

namespace RainbowTaskbar.Configuration.Instructions;

[DataContract]
internal class TextInstruction : Instruction {

    public bool drawn = false;

    [field: DataMember] public int Layer { get; set; } = 1;

    [field: DataMember] public string Text { get; set; } = "";

    [field: DataMember] public int X { get; set; } = 0;

    [field: DataMember] public int Y { get; set; } = 0;

    [field: DataMember] public string Font { get; set; } = "Arial";

    [field: DataMember] public int Size { get; set; } = 32;

    [field: DataMember] public bool DrawOnce { get; set; } = false;

    [field: DataMember] public Color Color { get; set; } = Color.Black;

    public override JObject ToJSON() {
        dynamic data = new ExpandoObject();
        data.Name = GetType().Name;
        data.Layer = Layer;
        data.Text = Text;
        data.X = X;
        data.Y = Y;
        data.Font = Font;
        data.Size = Size;
        data.Color = ColorExtension.HexConverter(Color);
        data.DrawOnce = DrawOnce;

        return JObject.FromObject(data);
    }

    public override bool Execute(Taskbar window, CancellationToken token) {
        if(drawn && DrawOnce) {
            return false;
        }

        window.Dispatcher.Invoke( () =>
            window.canvasManager.layers.DrawText(Layer, Text, X, Y, Size, "Arial", new SolidColorBrush(Color.ToMediaColor()))
        , System.Windows.Threading.DispatcherPriority.Normal, token);

        if (DrawOnce) drawn = true;

        return false;
    }
}