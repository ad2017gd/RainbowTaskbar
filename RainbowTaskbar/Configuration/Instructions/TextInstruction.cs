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
    [field: DataMember] public int Layer { get; set; } = 1;

    [field: DataMember] public string Text { get; set; }

    [field: DataMember] public int X { get; set; } = 0;

    [field: DataMember] public int Y { get; set; } = 0;

    [field: DataMember] public string Font { get; set; } = "Arial";

    [field: DataMember] public int Size { get; set; } = 32;

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

        return JObject.FromObject(data);
    }

    public override bool Execute(Taskbar window, CancellationToken _) {
        window.Dispatcher.Invoke(() => {
            var cvs = (Canvas) window.layers.MainDrawRectangles[Layer].Parent;

            var text = new Label();
            text.Content = Text;
            text.Foreground = new SolidColorBrush(Color.ToMediaColor());
            text.FontSize = Size;
            text.FontFamily = new FontFamily(Font);

            foreach (UIElement elem in cvs.Children)
                if (elem is Label) {
                    var tlab = (Label) elem;
                    if (Y == Canvas.GetTop(elem) && X == Canvas.GetLeft(elem) && (string) tlab.Content == Text)
                        return;
                }

            cvs.Children.Add(text);
            Canvas.SetTop(text, Y);
            Canvas.SetLeft(text, X);
        });
        return false;
    }
}