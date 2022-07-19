using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RainbowTaskbar.Drawing
{
    public class LayerManager
    {
        public int width = 0;
        public int height = 0;
        public Taskbar window = null;
        public List<RenderTargetBitmap> renderTargets = new List<RenderTargetBitmap>();
        public List<System.Windows.Media.Brush> brushes = new List<System.Windows.Media.Brush>();

        public LayerManager() {
            for (int i = 0; i < 16; i++) brushes.Add(new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)));
        }

        public LayerManager(Taskbar window) {
            this.window = window;
            width = (int) window.Width;
            height = (int) window.Height;
            for (int i = 0; i < 16; i++) brushes.Add(new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)));
        }

        public void MakeIfNeeded(int layer) {
            if (renderTargets.Count - 1 < layer && layer < 16) {
                for (int i = renderTargets.Count; i <= layer; i++) {
                    var target = new RenderTargetBitmap((int) width, (int) height, 96, 96, PixelFormats.Pbgra32);
                    renderTargets.Add(target);
                    if(window is not null) window.canvasManager.SetImage(i, target);
                    else {
                        App.taskbars.ForEach(b => {
                            b.canvasManager.SetImage(i, target);
                        });
                    }
                }

            }
        }

        public void DrawRect(int layer, System.Windows.Media.Brush fill, Rect? rect = null) {
            MakeIfNeeded(layer);

            var visual = new DrawingVisual();
            var ctx = visual.RenderOpen();
            ctx.DrawRectangle(fill, null, rect ?? new Rect(0, 0, width, height));
            brushes[layer] = fill;
            ctx.Close();
            renderTargets[layer].Render(visual);
        }
        public void DrawImage(int layer, Rect rect, ImageSource imageSource) {
            MakeIfNeeded(layer);

            var visual = new DrawingVisual();
            var ctx = visual.RenderOpen();
            ctx.DrawImage(imageSource, rect);
            ctx.Close();
            renderTargets[layer].Render(visual);
        }

        public void DrawShape(int layer, Geometry shape, System.Windows.Media.Brush brush = null, System.Windows.Media.Pen pen = null) {
            MakeIfNeeded(layer);

            var visual = new DrawingVisual();
            var ctx = visual.RenderOpen();
            ctx.DrawGeometry(brush, pen, shape);
            ctx.Close();
            renderTargets[layer].Render(visual);
        }
        public void DrawText(int layer, string content, int x = 0, int y = 0, int size = 32, string font = "Arial", System.Windows.Media.Brush fill = null) {
            MakeIfNeeded(layer);

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
}
