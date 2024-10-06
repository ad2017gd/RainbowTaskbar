
using System.Drawing;
using System.Dynamic;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Media;
using RainbowTaskbar.Interpolation;
using Color = System.Drawing.Color;
using Brush = System.Windows.Media.Brush;
using RainbowTaskbar.Languages;

namespace RainbowTaskbar.Configuration.Instruction.Instructions;


internal class ColorInstruction : Instruction {

    public override string Description {
        get {
            if (Randomize) {
                return App.localization.InstructionFormatSuffix(this, "randomized", Effect.ToStringLocalized());
            }
            else if (Effect == ColorInstructionEffect.Gradient || Effect == ColorInstructionEffect.FadeGradient) {
                return App.localization.InstructionFormatSuffix(this, "gradient", Effect.ToStringLocalized(), ColorTranslator.ToHtml(Color1), ColorTranslator.ToHtml(Color2));
            }
            else {
                return App.localization.InstructionFormatSuffix(this, "solid", Effect.ToStringLocalized(), ColorTranslator.ToHtml(Color1));
            }
        }
    }
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

    public int Time { get; set; } = 1;

    public Color Color1 { get; set; } = Color.FromArgb(0, 0, 0);

    public ColorInstructionEffect Effect { get; set; }

    public Color Color2 { get; set; } = Color.FromArgb(0, 0, 0);

    public int Time2 { get; set; } = 1;

    public ColorInstructionTransition Transition { get; set; }

    public double Angle { get; set; } = 0;

    public int Layer { get; set; } = 0;

    public bool Randomize { get; set; } = false;

    public bool Has2Colors { get => Effect == ColorInstructionEffect.FadeGradient || Effect == ColorInstructionEffect.Gradient; }

    /*
    public override JObject ToJSON() {
        dynamic data = new ExpandoObject();
        data.Name = GetType().Name;
        data.Time = Time;
        data.Effect = Effect;
        data.Transition = Transition;
        data.Time2 = Time2;
        data.Angle = Angle;
        data.Layer = Layer;
        data.Randomize = Randomize;
        data.Color1 = ColorExtension.HexConverter(Color1);
        data.Color2 = ColorExtension.HexConverter(Color2);


        return JObject.FromObject(data);
    }
    */

    public override bool Execute(Taskbar window, CancellationToken token) {
        
        var OldBrush = window.canvasManager.layers.brushes[Layer];
        if (Randomize) {
            Color1 = Color.FromArgb(255, App.rnd.Next(0, 255), App.rnd.Next(0, 255), App.rnd.Next(0, 255));
            Color2 = Color.FromArgb(255, App.rnd.Next(0, 255), App.rnd.Next(0, 255), App.rnd.Next(0, 255));
        }

        switch (Effect) {
            case ColorInstructionEffect.Solid:
                window.Dispatcher.Invoke(() => {
                    var Brush = new SolidColorBrush(Color1.ToMediaColor());
                    window.canvasManager.layers.DrawRect(Layer, Brush);
                }, System.Windows.Threading.DispatcherPriority.Normal, token);
                token.WaitHandle.WaitOne(Time);
                break;
            case ColorInstructionEffect.Gradient:
                window.Dispatcher.Invoke(() => {
                    var Brush = new LinearGradientBrush(Color1.ToMediaColor(), Color2.ToMediaColor(), Angle);
                    window.canvasManager.layers.DrawRect(Layer, Brush);
                }, System.Windows.Threading.DispatcherPriority.Normal, token);
                token.WaitHandle.WaitOne(Time);
                break;

            case ColorInstructionEffect.FadeGradient:

                if (OldBrush is SolidColorBrush) {
                    var Brush = (SolidColorBrush) OldBrush;

                    System.Windows.Media.Color OColor;
                    window.Dispatcher.Invoke(() => { OColor = Brush.Color; }, System.Windows.Threading.DispatcherPriority.Normal, token);

                    var j = 1;

                    while (j++ < Time2 / App.Settings.InterpolationQuality) {
                        var Color1Interpolated = ColorInterpolation.Interpolate(OColor.ToDrawingColor(), Color1,
                            (ColorInterpolation.INTERPOLATE_FUNCTION) Transition, (double) j / (Time2 / App.Settings.InterpolationQuality));
                        var Color2Interpolated = ColorInterpolation.Interpolate(OColor.ToDrawingColor(), Color2,
                            (ColorInterpolation.INTERPOLATE_FUNCTION) Transition, (double) j / (Time2 / App.Settings.InterpolationQuality));


                        window.Dispatcher.Invoke(() => {
                            var Brush = new LinearGradientBrush(Color1Interpolated.ToMediaColor(),
                                Color2Interpolated.ToMediaColor(), Angle);
                            window.canvasManager.layers.DrawRect(Layer, Brush);
                        }, System.Windows.Threading.DispatcherPriority.Normal, token);

                        token.WaitHandle.WaitOne(Time2 / (Time2 / App.Settings.InterpolationQuality));
                    }
                }
                else {
                    var Brush = (LinearGradientBrush) OldBrush;

                    System.Windows.Media.Color OColor1;
                    System.Windows.Media.Color OColor2;
                    window.Dispatcher.Invoke(() => {
                        OColor1 = Brush.GradientStops[0].Color;
                        OColor2 = Brush.GradientStops[1].Color;
                    }, System.Windows.Threading.DispatcherPriority.Normal, token);

                    var j = 1;

                    while (j++ < Time2 / App.Settings.InterpolationQuality) {
                        var Color1Interpolated = ColorInterpolation.Interpolate(OColor1.ToDrawingColor(), Color1,
                            (ColorInterpolation.INTERPOLATE_FUNCTION) Transition, (double) j / (Time2 / App.Settings.InterpolationQuality));
                        var Color2Interpolated = ColorInterpolation.Interpolate(OColor2.ToDrawingColor(), Color2,
                            (ColorInterpolation.INTERPOLATE_FUNCTION) Transition, (double) j / (Time2 / App.Settings.InterpolationQuality));


                        window.Dispatcher.Invoke(() => {
                            var Brush = new LinearGradientBrush(Color1Interpolated.ToMediaColor(),
                                Color2Interpolated.ToMediaColor(), Angle);
                            window.canvasManager.layers.DrawRect(Layer, Brush);
                        }, System.Windows.Threading.DispatcherPriority.Normal, token);


                        token.WaitHandle.WaitOne(Time2 / (Time2 / App.Settings.InterpolationQuality));
                    }
                }

                token.WaitHandle.WaitOne(Time);
                break;

            case ColorInstructionEffect.Fade:

                if (OldBrush is SolidColorBrush) {
                    var Brush = (SolidColorBrush) OldBrush;

                    System.Windows.Media.Color OColor;
                    window.Dispatcher.Invoke(() => { OColor = Brush.Color; }, System.Windows.Threading.DispatcherPriority.Normal, token);

                    var j = 1;

                    while (j++ < Time2 / App.Settings.InterpolationQuality) {
                        var Color1Interpolated = ColorInterpolation.Interpolate(OColor.ToDrawingColor(), Color1,
                            (ColorInterpolation.INTERPOLATE_FUNCTION) Transition, (double) j / (Time2 / App.Settings.InterpolationQuality));

                        window.Dispatcher.Invoke(() => {
                            var Brush = new SolidColorBrush(Color1Interpolated.ToMediaColor());
                            window.canvasManager.layers.DrawRect(Layer, Brush);
                        }, System.Windows.Threading.DispatcherPriority.Normal, token);


                        token.WaitHandle.WaitOne(Time2 / (Time2 / App.Settings.InterpolationQuality));
                    }
                }
                else {
                    var Brush = (LinearGradientBrush) OldBrush;

                    System.Windows.Media.Color OColor1;
                    System.Windows.Media.Color OColor2;
                    window.Dispatcher.Invoke(() => {
                        OColor1 = Brush.GradientStops[0].Color;
                        OColor2 = Brush.GradientStops[1].Color;
                    }, System.Windows.Threading.DispatcherPriority.Normal, token);

                    var j = 1;

                    while (j++ < Time2 / App.Settings.InterpolationQuality) {
                        var Color1Interpolated = ColorInterpolation.Interpolate(OColor1.ToDrawingColor(), Color1,
                            (ColorInterpolation.INTERPOLATE_FUNCTION) Transition, (double) j / (Time2 / App.Settings.InterpolationQuality));
                        var Color2Interpolated = ColorInterpolation.Interpolate(OColor2.ToDrawingColor(), Color1,
                            (ColorInterpolation.INTERPOLATE_FUNCTION) Transition, (double) j / (Time2 / App.Settings.InterpolationQuality));


                        window.Dispatcher.Invoke(() => {
                            var Brush = new LinearGradientBrush(Color1Interpolated.ToMediaColor(),
                                Color2Interpolated.ToMediaColor(), Angle);
                            window.canvasManager.layers.DrawRect(Layer, Brush);
                        }, System.Windows.Threading.DispatcherPriority.Normal, token);


                        token.WaitHandle.WaitOne(Time2 / (Time2 / App.Settings.InterpolationQuality));
                    }
                }

                token.WaitHandle.WaitOne(Time);
                break;
        }
        
        return true;

    }
}