using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace RainbowTaskbar.Configuration.Instruction.Instructions;


internal class ImageInstruction : Instruction {

    public bool drawn = false;

    public int Layer { get; set; } = 0;

    public string Path { get; set; } = "";

    public int X { get; set; } = 0;

    public int Y { get; set; } = 0;

    public int Width { get; set; } = 0;

    public int Height { get; set; } = 0;

    public double Opacity { get; set; } = 1;

    public bool DrawOnce { get; set; } = false;

    public override string Description {
        get {
            return App.localization.InstructionFormat(this, System.IO.Path.GetFileName(Path));
        }
    }
    /*
    public override JObject ToJSON() {
        dynamic data = new ExpandoObject();

        data.Name = GetType().Name;
        data.Layer = Layer;
        data.Path = Path;
        data.X = X;
        data.Y = Y;
        data.Width = Width;
        data.Height = Height;
        data.Opacity = Opacity;
        data.DrawOnce = DrawOnce;

        return JObject.FromObject(data);
    }
    */
    public override bool Execute(Taskbar window, CancellationToken token) {
        if (Path == "") return false;

        if (DrawOnce && drawn) return false;

        window.Dispatcher.Invoke(() => {
            var bmp = new Bitmap(Path);
            var ms = new MemoryStream();

            bmp.Save(ms, ImageFormat.Png);

            var image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();

            window.canvasManager.layers.DrawImage(Layer, new Rect(X, Y, Width == 0 ? bmp.Width : Width, Height == 0 ? bmp.Height : Height), image);
        }, System.Windows.Threading.DispatcherPriority.Normal, token);

        if (DrawOnce) {
            drawn = true;
        }

        return false;
    }

}