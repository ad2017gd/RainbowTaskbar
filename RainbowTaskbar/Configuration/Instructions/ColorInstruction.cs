using Newtonsoft.Json.Linq;
using RainbowTaskbar.Interpolation;
using System;
using System.Dynamic;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Media;

namespace RainbowTaskbar.Configuration.Instructions
{
    [DataContract]
    class ColorInstruction : Instruction
    {

        [field: DataMember]
        public int Time { get; set; } = 1;

        [field: DataMember]
        public System.Drawing.Color Color1 { get; set; }

        [field: DataMember]
        public ColorInstructionEffect Effect { get; set; }

        [field: DataMember]
        public System.Drawing.Color Color2 { get; set; }

        [field: DataMember]
        public int Time2 { get; set; } = 1;

        [field: DataMember]
        public ColorInstructionTransition Transition { get; set; }

        [field: DataMember]
        public double Angle { get; set; } = 0;

        [field: DataMember]
        public int Layer { get; set; } = 0;

        [field: DataMember]
        public bool Randomize { get; set; } = false;

        public override JObject ToJSON()
        {
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
        public override bool Execute(Taskbar window, System.Threading.CancellationToken token)
        {
            if (Randomize)
            {
                Color1 = System.Drawing.Color.FromArgb(255, App.rnd.Next(0, 255), App.rnd.Next(0, 255), App.rnd.Next(0, 255));
                Color2 = System.Drawing.Color.FromArgb(255, App.rnd.Next(0, 255), App.rnd.Next(0, 255), App.rnd.Next(0, 255));
            }
            switch (Effect)
            {

                case ColorInstructionEffect.Solid:
                    window.Dispatcher.Invoke(() =>
                    {
                        window.layers.MainDrawRectangles[Layer].Fill = new SolidColorBrush(Color1.ToMediaColor());
                    });
                    token.WaitHandle.WaitOne(Time);
                    break;
                case ColorInstructionEffect.Gradient:
                    window.Dispatcher.Invoke(() =>
                    {
                        window.layers.MainDrawRectangles[Layer].Fill = new LinearGradientBrush(Color1.ToMediaColor(), Color2.ToMediaColor(), Angle);
                    });
                    token.WaitHandle.WaitOne(Time);
                    break;

                case ColorInstructionEffect.FadeGradient:


                    System.Windows.Media.Brush OldBrush = null;
                    window.Dispatcher.Invoke(() =>
                    {
                        OldBrush = window.layers.MainDrawRectangles[Layer].Fill;
                    });


                    if (OldBrush is SolidColorBrush)
                    {
                        SolidColorBrush Brush = (SolidColorBrush)OldBrush;

                        System.Windows.Media.Color OColor;
                        window.Dispatcher.Invoke(() =>
                        {
                            OColor = Brush.Color;
                        });

                        int j = 1;

                        while (j++ < Time2 / 25)
                        {
                            var Color1Interpolated = Interpolation.ColorInterpolation.Interpolate(OColor.ToDrawingColor(), Color1, (Interpolation.ColorInterpolation.INTERPOLATE_FUNCTION)Transition, (double)j / (Time2 / 25));
                            var Color2Interpolated = Interpolation.ColorInterpolation.Interpolate(OColor.ToDrawingColor(), Color2, (Interpolation.ColorInterpolation.INTERPOLATE_FUNCTION)Transition, (double)j / (Time2 / 25));


                            window.Dispatcher.Invoke(() =>
                            {
                                var brush = new LinearGradientBrush(Color1Interpolated.ToMediaColor(), Color2Interpolated.ToMediaColor(), Angle);
                                window.layers.MainDrawRectangles[Layer].Fill = brush;
                            });

                            token.WaitHandle.WaitOne(Time2 / (Time2 / 25));


                        }


                    }
                    else
                    {
                        LinearGradientBrush Brush = (LinearGradientBrush)OldBrush;

                        System.Windows.Media.Color OColor1;
                        System.Windows.Media.Color OColor2;
                        window.Dispatcher.Invoke(() =>
                        {

                            OColor1 = Brush.GradientStops[0].Color;
                            OColor2 = Brush.GradientStops[1].Color;
                        });

                        int j = 1;

                        while (j++ < Time2 / 25)
                        {
                            var Color1Interpolated = Interpolation.ColorInterpolation.Interpolate(OColor1.ToDrawingColor(), Color1, (Interpolation.ColorInterpolation.INTERPOLATE_FUNCTION)Transition, (double)j / (Time2 / 25));
                            var Color2Interpolated = Interpolation.ColorInterpolation.Interpolate(OColor2.ToDrawingColor(), Color2, (Interpolation.ColorInterpolation.INTERPOLATE_FUNCTION)Transition, (double)j / (Time2 / 25));


                            window.Dispatcher.Invoke(() =>
                            {
                                var brush = new LinearGradientBrush(Color1Interpolated.ToMediaColor(), Color2Interpolated.ToMediaColor(), Angle);
                                window.layers.MainDrawRectangles[Layer].Fill = brush;
                            });


                            token.WaitHandle.WaitOne(Time2 / (Time2 / 25));


                        }


                    }

                    token.WaitHandle.WaitOne(Time);
                    break;

                case ColorInstructionEffect.Fade:


                    OldBrush = null;
                    window.Dispatcher.Invoke(() =>
                    {
                        OldBrush = window.layers.MainDrawRectangles[Layer].Fill;
                    });



                    if (OldBrush is SolidColorBrush)
                    {
                        SolidColorBrush Brush = (SolidColorBrush)OldBrush;

                        System.Windows.Media.Color OColor;
                        window.Dispatcher.Invoke(() =>
                        {
                            OColor = Brush.Color;
                        });

                        int j = 1;

                        while (j++ < Time2 / 25)
                        {
                            var Color1Interpolated = Interpolation.ColorInterpolation.Interpolate(OColor.ToDrawingColor(), Color1, (Interpolation.ColorInterpolation.INTERPOLATE_FUNCTION)Transition, (double)j / (Time2 / 25));

                            window.Dispatcher.Invoke(() =>
                            {
                                var brush = new SolidColorBrush(Color1Interpolated.ToMediaColor());
                                window.layers.MainDrawRectangles[Layer].Fill = brush;
                            });


                            token.WaitHandle.WaitOne(Time2 / (Time2 / 25));


                        }


                    }
                    else
                    {
                        LinearGradientBrush Brush = (LinearGradientBrush)OldBrush;

                        System.Windows.Media.Color OColor;
                        window.Dispatcher.Invoke(() =>
                        {

                            OColor = System.Windows.Media.Color.FromRgb((byte)((Brush.GradientStops[0].Color.R + Brush.GradientStops[1].Color.R) / 2), (byte)(Brush.GradientStops[0].Color.G + Brush.GradientStops[1].Color.G / 2), (byte)(Brush.GradientStops[0].Color.B + Brush.GradientStops[1].Color.B / 2));
                        });

                        int j = 1;

                        while (j++ < Time2 / 25)
                        {
                            var Color1Interpolated = Interpolation.ColorInterpolation.Interpolate(OColor.ToDrawingColor(), Color1, (Interpolation.ColorInterpolation.INTERPOLATE_FUNCTION)Transition, (double)j / (Time2 / 25));

                            window.Dispatcher.Invoke(() =>
                            {
                                var brush = new SolidColorBrush(Color1Interpolated.ToMediaColor());
                                window.layers.MainDrawRectangles[Layer].Fill = brush;
                            });


                            token.WaitHandle.WaitOne(Time2 / (Time2 / 25));


                        }


                    }

                    token.WaitHandle.WaitOne(Time);
                    break;
            }
            return true;
        }

        public enum ColorInstructionEffect
        {
            Solid,
            Fade,
            Gradient,
            FadeGradient
        }

        public enum ColorInstructionTransition
        {
            Linear,
            Sine,
            Cubic,
            Exponential,
            Back
        }

    }
}
