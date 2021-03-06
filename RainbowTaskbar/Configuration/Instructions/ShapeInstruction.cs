using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using RainbowTaskbar.Interpolation;

namespace RainbowTaskbar.Configuration.Instructions;

[DataContract]
public class ShapeInstruction : Instruction {

    public bool drawn = false;

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

    public override string Name {
        get {
            return $"Shape - {Shape.ToString()}";
        }
    }

    public override bool Execute(Taskbar window, CancellationToken token) {

        if(DrawOnce && drawn) {
            return false;
        }

        switch(Shape) {
            case ShapeInstructionShapes.Line: {
                    var geometry = new LineGeometry(new Point(X,Y), new Point(X2,Y2));
                    geometry.Freeze();
                    window.Dispatcher.Invoke(() =>
                        window.canvasManager.layers.DrawShape(Layer, geometry, null, new Pen(new SolidColorBrush(Line.ToMediaColor()), LineSize))
                    , System.Windows.Threading.DispatcherPriority.Normal, token);
                    break;
                }
            case ShapeInstructionShapes.Rectangle: {
                    var geometry = new RectangleGeometry(new Rect(new Point(X, Y), new Point(X2, Y2)));
                    geometry.Freeze();
                    window.Dispatcher.Invoke(() =>
                        window.canvasManager.layers.DrawShape(Layer, geometry, new SolidColorBrush(Fill.ToMediaColor()), new Pen(new SolidColorBrush(Line.ToMediaColor()), LineSize))
                    , System.Windows.Threading.DispatcherPriority.Normal, token);
                    break;
                }
            case ShapeInstructionShapes.Ellipse: {
                    var geometry = new EllipseGeometry(new Rect(new Point(X, Y), new Point(X2, Y2)));
                    geometry.Freeze();
                    window.Dispatcher.Invoke(() =>
                        window.canvasManager.layers.DrawShape(Layer, geometry, new SolidColorBrush(Fill.ToMediaColor()), new Pen(new SolidColorBrush(Line.ToMediaColor()), LineSize))
                    , System.Windows.Threading.DispatcherPriority.Normal, token);
                    break;
                }
        }

        if(DrawOnce) {
            drawn = true;
        }

        return false;
    }

    public enum ShapeInstructionShapes {
        Line,
        Rectangle,
        Ellipse
    }

    public override JObject ToJSON() {
        dynamic data = new ExpandoObject();

        data.Name = GetType().Name;
        data.Layer = Layer;
        data.Shape = Shape;
        data.X = X;
        data.Y = Y;
        data.X2 = X2;
        data.Y2 = Y2;
        data.Fill = ColorExtension.HexConverter(Fill);
        data.Line = ColorExtension.HexConverter(Line);
        data.LineSize = LineSize;
        data.DrawOnce = DrawOnce;

        return JObject.FromObject(data);
    }
}