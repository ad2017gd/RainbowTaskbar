using System.Windows;

namespace RainbowTaskbar.Drawing
{
    public class Layers
    {
        public System.Windows.Shapes.Rectangle[] MainDrawRectangles;
        public Window window;

        public Layers(Window window)
        {
            this.window = window;
        }
    }
}
