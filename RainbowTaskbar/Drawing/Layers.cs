using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Media.Imaging;

namespace RainbowTaskbar.Drawing;

public class Layers {
    public List<Canvas> canvases;
    public List<RenderTargetBitmap> renderTargets = new List<RenderTargetBitmap>();
    public List<System.Windows.Media.Brush> brushes = new List<System.Windows.Media.Brush>();
    public Window window;

    public Layers(Window window, Canvas[] canvases) {
        this.window = window;
        this.canvases = canvases.ToList();

        canvases.ToList().ForEach((c) => {
            c.Width = window.Width;
            c.Height = window.Height;

            var img = new System.Windows.Controls.Image();
            img.Name = $"{c.Name}Image";
            img.Width = c.Width;
            img.Height = c.Height;
            img.SnapsToDevicePixels = true;

            var target = new RenderTargetBitmap((int) c.Width, (int) c.Height, 96, 96, PixelFormats.Pbgra32);
            renderTargets.Add(target);
            img.Source = target;

            brushes.Add(new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)));

            c.Children.Add(img);
            Canvas.SetTop(img, 0);
            Canvas.SetLeft(img, 0);
        });
    }

    public void DrawRect(int layer, System.Windows.Media.Brush fill, Rect? rect = null) {
        var visual = new DrawingVisual();
        var ctx = visual.RenderOpen();
        ctx.DrawRectangle(fill, null, rect ?? new Rect(0,0,window.Width,window.Height));
        brushes[layer] = fill;
        ctx.Close();
        renderTargets[layer].Render(visual);
    }
    public void DrawRect(int layer, Rect rect, System.Windows.Media.Brush fill) {
        var visual = new DrawingVisual();
        var ctx = visual.RenderOpen();
        ctx.DrawRectangle(fill, null, rect);
        ctx.Close();
        renderTargets[layer].Render(visual);
    }
    public void DrawImage(int layer, Rect rect, ImageSource imageSource) {
        var visual = new DrawingVisual();
        var ctx = visual.RenderOpen();
        ctx.DrawImage(imageSource, rect);
        ctx.Close();
        renderTargets[layer].Render(visual);
    }

    public void DrawShape(int layer, Geometry shape, System.Windows.Media.Brush brush = null, System.Windows.Media.Pen pen = null) {
        var visual = new DrawingVisual();
        var ctx = visual.RenderOpen();
        ctx.DrawGeometry(brush, pen, shape);
        ctx.Close();
        renderTargets[layer].Render(visual);
    }
    public void DrawText(int layer, string content, int x = 0, int y = 0, int size = 32, string font = "Arial", System.Windows.Media.Brush fill = null) {
        var visual = new DrawingVisual();
        var ctx = visual.RenderOpen();
        var ftext = new FormattedText(
            content ?? "",
            CultureInfo.GetCultureInfo("en-us"),
            FlowDirection.LeftToRight,
            new Typeface(font),
            size,
            fill
            );
        ctx.DrawText(ftext, new System.Windows.Point(x, y));
        ctx.Close();
        renderTargets[layer].Render(visual);
    }

}