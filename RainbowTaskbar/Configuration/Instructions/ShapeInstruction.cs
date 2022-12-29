
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
using Newtonsoft.Json.Linq;
using RainbowTaskbar.Interpolation;
using RainbowTaskbar.Languages;

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

    private Taskbar _tsk;
    private int x2 { get {
            if(FitTaskbars) {
                return App.Config.GraphicsRepeat ? _tsk.canvasManager.layers.width : App.layers.width;
            } else {
                return X2;
            }
        } 
    }

    private int y2 {
        get {
            if (FitTaskbars) {
                return App.Config.GraphicsRepeat ? _tsk.canvasManager.layers.height : App.layers.height;
            }
            else {
                return X2;
            }
        }
    }

    [field: DataMember] public bool DrawOnce { get; set; } = false;

    [field: DataMember]
    public System.Drawing.Color Fill { get; set; } = System.Drawing.Color.FromArgb(255, 0, 0, 0);

    [field: DataMember]
    public System.Drawing.Color Line { get; set; } = System.Drawing.Color.FromArgb(255, 0, 0, 0);

    [field: DataMember]
    public int LineSize { get; set; } = 1;

    [field: DataMember]
    public int Radius { get; set; } = 0;

    private int radius { get {
            if(FitTaskbars) {
                return _tsk.taskbarHelper.Radius+1;
            } else {
                return Radius;
            }
        } }

    [field: DataMember]
    public bool FitTaskbars { get; set; } = false;

    public override string Description {
        get {
            var name = Shape == ShapeInstructionShapes.Rectangle && Radius > 0 ? App.localization.Get("enum_roundedrect") : Shape.ToStringLocalized();
            return App.localization.Format(this, name);
        }
    }

    public override bool Execute(Taskbar window, CancellationToken token) {
        _tsk = window;
        if(DrawOnce && drawn) {
            return false;
        }

        switch(Shape) {
            case ShapeInstructionShapes.Line: {
                    var geometry = new LineGeometry(new Point(X,Y), new Point(x2,y2));
                    geometry.Freeze();
                    window.Dispatcher.Invoke(() =>
                        window.canvasManager.layers.DrawShape(Layer, geometry, null, new Pen(new SolidColorBrush(Line.ToMediaColor()), LineSize))
                    , System.Windows.Threading.DispatcherPriority.Normal, token);
                    break;
                }
            case ShapeInstructionShapes.Rectangle: {
                    var geometry = new RectangleGeometry(new Rect(new Point(X, Y), new Point(x2, y2)));
                    geometry.RadiusX = radius/2.0; geometry.RadiusY = radius/2.0;
                    geometry.Freeze();
                    window.Dispatcher.Invoke(() =>
                        window.canvasManager.layers.DrawShape(Layer, geometry, new SolidColorBrush(Fill.ToMediaColor()), new Pen(new SolidColorBrush(Line.ToMediaColor()), LineSize))
                    , System.Windows.Threading.DispatcherPriority.Normal, token);
                    break;
                }
            case ShapeInstructionShapes.Ellipse: {
                    var geometry = new EllipseGeometry(new Rect(new Point(X, Y), new Point(x2, y2)));
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
        data.FitTaskbars = FitTaskbars;
        data.Fill = ColorExtension.HexConverter(Fill);
        data.Line = ColorExtension.HexConverter(Line);
        data.LineSize = LineSize;
        data.DrawOnce = DrawOnce;

        return JObject.FromObject(data);
    }
}
