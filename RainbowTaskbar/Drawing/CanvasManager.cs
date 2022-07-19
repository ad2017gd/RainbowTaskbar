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

public class CanvasManager {
    public List<Canvas> canvases;
    private LayerManager _layers = null;
    public LayerManager layers { 
        get {
            if (_layers == null) {
                return App.layers;
            }
            else return _layers;
        }
        set => _layers = value;
    }
    public Window window;

    public CanvasManager(Taskbar window, Canvas[] canvases) {
        this.window = window;
        this.canvases = canvases.ToList();

        this.canvases.ForEach((c) => {
            c.Width = window.Width;
            c.Height = window.Height;
        });
        if (App.Config.GraphicsRepeat) _layers = new LayerManager(window);
    }

    public void SetImage(int index, RenderTargetBitmap target) {
        var c = canvases[index];
        var img = new System.Windows.Controls.Image();
        img.Name = $"{c.Name}Image";
        img.Width = layers.width;
        img.Height = c.Height;
        img.SnapsToDevicePixels = true;

        img.Source = target;

        img.Stretch = Stretch.None;
        c.Children.Add(img);
        Canvas.SetTop(img, 0);
        if(!App.Config.GraphicsRepeat) {
            Canvas.SetLeft(img, -window.Left);
        }
    }

   

    

}