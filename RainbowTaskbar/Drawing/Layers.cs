using System.Windows;
using System.Windows.Shapes;

namespace RainbowTaskbar.Drawing;

public class Layers {
    public Rectangle[] MainDrawRectangles;
    public Window window;

    public Layers(Window window) {
        this.window = window;
    }
}