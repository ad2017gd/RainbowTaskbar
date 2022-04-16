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
using Newtonsoft.Json.Linq;
using Image = System.Windows.Controls.Image;

namespace RainbowTaskbar.Configuration.Instructions;

[DataContract]
internal class ImageInstruction : Instruction {
    [field: DataMember] public int Layer { get; set; } = 0;

    [field: DataMember] public string Path { get; set; }

    [field: DataMember] public int X { get; set; } = 0;

    [field: DataMember] public int Y { get; set; } = 0;

    [field: DataMember] public int Width { get; set; } = 0;

    [field: DataMember] public int Height { get; set; } = 0;

    [field: DataMember] public double Opacity { get; set; } = 1;

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

        return JObject.FromObject(data);
    }

    public override bool Execute(Taskbar window, CancellationToken _) {
        if (Path is null) return false;

        var bmp = new Bitmap(Path);
        var resized = bmp;
        if (Width > 0 || Height > 0)
            resized = ResizeImage(bmp, Width == 0 ? bmp.Width : Width, Height == 0 ? bmp.Height : Height);

        var ms = new MemoryStream();
        resized.Save(ms, ImageFormat.Png);


        window.Dispatcher.Invoke(() => {
            var image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);


            image.StreamSource = ms;
            image.EndInit();

            var fix = new TransformedBitmap(image, new ScaleTransform(image.DpiX / 96, image.DpiY / 96));


            var cvs = (Canvas) window.layers.MainDrawRectangles[Layer].Parent;


            var img = new Image();
            img.Source = fix;
            img.Opacity = Opacity;
            img.Stretch = Stretch.Fill;
            img.Width = fix.Width;
            img.Height = fix.Height;
            img.SnapsToDevicePixels = true;
            img.UseLayoutRounding = true;

            foreach (var elem in cvs.Children.Cast<UIElement>().ToList())
                if (elem is Image) {
                    var timg = (Image) elem;
                    if ((img.Height == timg.Height || double.IsNaN(img.Height) == double.IsNaN(timg.Height)) &&
                        (img.Width == timg.Width || double.IsNaN(img.Width) == double.IsNaN(timg.Width)) &&
                        Y == Canvas.GetTop(elem) && X == Canvas.GetLeft(elem))
                        cvs.Children.Remove(elem);
                }

            cvs.Children.Add(img);
            Canvas.SetTop(img, Y);
            Canvas.SetLeft(img, X);
        });

        bmp.Dispose();
        resized.Dispose();
        return false;
    }


    public static Bitmap ResizeImage(Bitmap image, int width, int height) {
        var destRect = new Rectangle(0, 0, width, height);
        var destImage = new Bitmap(width, height);

        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        using (var graphics = Graphics.FromImage(destImage)) {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighSpeed;
            graphics.InterpolationMode = InterpolationMode.Bicubic;
            graphics.SmoothingMode = SmoothingMode.HighSpeed;
            graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;

            using (var wrapMode = new ImageAttributes()) {
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }
        }

        return destImage;
    }
}