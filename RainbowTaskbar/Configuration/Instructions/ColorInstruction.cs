using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Media;
using RainbowTaskbar.Interpolation;
using Color = System.Drawing.Color;

namespace RainbowTaskbar.Configuration.Instructions;

[DataContract]
internal class ColorInstruction : Instruction {
    public enum ColorInstructionEffect {
        Solid,
        Fade,
        Gradient,
        FadeGradient
    }

    public enum ColorInstructionTransition {
        Linear,
        Sine,
        Cubic,
        Exponential,
        Back
    }

    [field: DataMember] public int Time { get; set; } = 1;

    [field: DataMember] public Color Color1 { get; set; }

    [field: DataMember] public ColorInstructionEffect Effect { get; set; }

    [field: DataMember] public Color Color2 { get; set; }

    [field: DataMember] public int Time2 { get; set; } = 1;

    [field: DataMember] public ColorInstructionTransition Transition { get; set; }

    [field: DataMember] public double Angle { get; set; } = 0;

    [field: DataMember] public int Layer { get; set; } = 0;

    [field: DataMember] public bool Randomize { get; set; } = false;

    public override bool Execute(Taskbar window, CancellationToken token) {
        var OldBrush = window.layers.brushes[Layer];
        if (Randomize) {
            Color1 = Color.FromArgb(255, App.rnd.Next(0, 255), App.rnd.Next(0, 255), App.rnd.Next(0, 255));
            Color2 = Color.FromArgb(255, App.rnd.Next(0, 255), App.rnd.Next(0, 255), App.rnd.Next(0, 255));
        }

        switch (Effect) {
            case ColorInstructionEffect.Solid:
                window.Dispatcher.Invoke(() => {
                    var Brush = new SolidColorBrush(Color1.ToMediaColor());
                    window.layers.DrawRect(Layer, Brush);
                });
                token.WaitHandle.WaitOne(Time);
                break;
            case ColorInstructionEffect.Gradient:
                window.Dispatcher.Invoke(() => {
                    var Brush = new LinearGradientBrush(Color1.ToMediaColor(), Color2.ToMediaColor(), Angle);
                    window.layers.DrawRect(Layer, Brush);
                });
                token.WaitHandle.WaitOne(Time);
                break;

            case ColorInstructionEffect.FadeGradient:

                if (OldBrush is SolidColorBrush) {
                    var Brush = (SolidColorBrush) OldBrush;

                    System.Windows.Media.Color OColor;
                    window.Dispatcher.Invoke(() => { OColor = Brush.Color; });

                    var j = 1;

                    while (j++ < Time2 / 25) {
                        var Color1Interpolated = ColorInterpolation.Interpolate(OColor.ToDrawingColor(), Color1,
                            (ColorInterpolation.INTERPOLATE_FUNCTION) Transition, (double) j / (Time2 / 25));
                        var Color2Interpolated = ColorInterpolation.Interpolate(OColor.ToDrawingColor(), Color2,
                            (ColorInterpolation.INTERPOLATE_FUNCTION) Transition, (double) j / (Time2 / 25));


                        window.Dispatcher.Invoke(() => {
                            var Brush = new LinearGradientBrush(Color1Interpolated.ToMediaColor(),
                                Color2Interpolated.ToMediaColor(), Angle);
                            window.layers.DrawRect(Layer, Brush);
                        });

                        token.WaitHandle.WaitOne(Time2 / (Time2 / 25));
                    }
                }
                else {
                    var Brush = (LinearGradientBrush) OldBrush;

                    System.Windows.Media.Color OColor1;
                    System.Windows.Media.Color OColor2;
                    window.Dispatcher.Invoke(() => {
                        OColor1 = Brush.GradientStops[0].Color;
                        OColor2 = Brush.GradientStops[1].Color;
                    });

                    var j = 1;

                    while (j++ < Time2 / 25) {
                        var Color1Interpolated = ColorInterpolation.Interpolate(OColor1.ToDrawingColor(), Color1,
                            (ColorInterpolation.INTERPOLATE_FUNCTION) Transition, (double) j / (Time2 / 25));
                        var Color2Interpolated = ColorInterpolation.Interpolate(OColor2.ToDrawingColor(), Color2,
                            (ColorInterpolation.INTERPOLATE_FUNCTION) Transition, (double) j / (Time2 / 25));


                        window.Dispatcher.Invoke(() => {
                            var Brush = new LinearGradientBrush(Color1Interpolated.ToMediaColor(),
                                Color2Interpolated.ToMediaColor(), Angle);
                            window.layers.DrawRect(Layer, Brush);
                        });


                        token.WaitHandle.WaitOne(Time2 / (Time2 / 25));
                    }
                }

                token.WaitHandle.WaitOne(Time);
                break;

            case ColorInstructionEffect.Fade:

                if (OldBrush is SolidColorBrush) {
                    var Brush = (SolidColorBrush) OldBrush;

                    System.Windows.Media.Color OColor;
                    window.Dispatcher.Invoke(() => { OColor = Brush.Color; });

                    var j = 1;

                    while (j++ < Time2 / 25) {
                        var Color1Interpolated = ColorInterpolation.Interpolate(OColor.ToDrawingColor(), Color1,
                            (ColorInterpolation.INTERPOLATE_FUNCTION) Transition, (double) j / (Time2 / 25));

                        window.Dispatcher.Invoke(() => {
                            var Brush = new SolidColorBrush(Color1Interpolated.ToMediaColor());
                            window.layers.DrawRect(Layer, Brush);
                        });


                        token.WaitHandle.WaitOne(Time2 / (Time2 / 25));
                    }
                }
                else {
                    var Brush = (LinearGradientBrush) OldBrush;

                    System.Windows.Media.Color OColor;
                    window.Dispatcher.Invoke(() => {
                        OColor = System.Windows.Media.Color.FromRgb(
                            (byte) ((Brush.GradientStops[0].Color.R + Brush.GradientStops[1].Color.R) / 2),
                            (byte) (Brush.GradientStops[0].Color.G + Brush.GradientStops[1].Color.G / 2),
                            (byte) (Brush.GradientStops[0].Color.B + Brush.GradientStops[1].Color.B / 2));
                    });

                    var j = 1;

                    while (j++ < Time2 / 25) {
                        var Color1Interpolated = ColorInterpolation.Interpolate(OColor.ToDrawingColor(), Color1,
                            (ColorInterpolation.INTERPOLATE_FUNCTION) Transition, (double) j / (Time2 / 25));

                        window.Dispatcher.Invoke(() => {
                            var Brush = new SolidColorBrush(Color1Interpolated.ToMediaColor());
                            window.layers.DrawRect(Layer, Brush);
                        });


                        token.WaitHandle.WaitOne(Time2 / (Time2 / 25));
                    }
                }

                token.WaitHandle.WaitOne(Time);
                break;
        }

        return true;
    }
}