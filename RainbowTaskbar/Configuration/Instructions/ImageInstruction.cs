﻿using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace RainbowTaskbar.Configuration.Instructions;

[DataContract]
internal class ImageInstruction : Instruction {
    public bool drawn;

    [field: DataMember] public int Layer { get; set; } = 0;

    [field: DataMember] public string Path { get; set; }

    [field: DataMember] public int X { get; set; } = 0;

    [field: DataMember] public int Y { get; set; } = 0;

    [field: DataMember] public int Width { get; set; } = 0;

    [field: DataMember] public int Height { get; set; } = 0;

    [field: DataMember] public double Opacity { get; set; } = 1;

    [field: DataMember] public bool DrawOnce { get; set; } = false;

    public override bool Execute(Taskbar window, CancellationToken _) {
        if (Path is null) return false;

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

            window.layers.DrawImage(Layer,
                new Rect(X, Y, Width == 0 ? bmp.Width : Width, Height == 0 ? bmp.Height : Height), image);
        });

        if (DrawOnce) drawn = true;

        return false;
    }
}