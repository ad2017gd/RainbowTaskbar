using System.Dynamic;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RainbowTaskbar.Interpolation;
using Color = System.Drawing.Color;

namespace RainbowTaskbar.Configuration.Instruction.Instructions;


internal class TextInstruction : Instruction {

    public bool drawn = false;

    public int Layer { get; set; } = 1;

    public string Text { get; set; } = "";

    public int X { get; set; } = 0;

    public int Y { get; set; } = 0;

    public string Font { get; set; } = "Arial";

    public int Size { get; set; } = 32;

    public bool DrawOnce { get; set; } = false;

    public Color Color { get; set; } = Color.Black;

    public bool Center { get; set; } = false;

    public override string Description {
        get {
            return App.localization.InstructionFormat(this, Text);
        }
    }
    /*
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
        data.Center = Center;

        return JObject.FromObject(data);
    }*/

    public override bool Execute(Taskbar window, CancellationToken token) {
        if (drawn && DrawOnce) {
            return false;
        }


        window.Dispatcher.Invoke(() =>
            window.canvasManager.layers.DrawText(Layer, Text, X, Y, Size, Font, new SolidColorBrush(Color.ToMediaColor()), Center)
        , System.Windows.Threading.DispatcherPriority.Normal, token);

        if (DrawOnce) drawn = true;

        return false;
    }
}