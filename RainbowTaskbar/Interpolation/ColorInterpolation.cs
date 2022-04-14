using System;
using System.Globalization;
using System.Windows.Data;

namespace RainbowTaskbar.Interpolation
{
    class ColorInterpolation
    {
        static double Linear(double x)
        {
            return x;
        }
        static double Cubic(double x)
        {
            return x < 0.5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2;
        }
        static double Back(double x)
        {
            const double c1 = 1.70158;
            const double c2 = c1 * 1.525;

            return x < 0.5
                ? (Math.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
                : (Math.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
        }
        static double Sine(double x)
        {
            return -(Math.Cos(3.14159 /*PI*/ * x) - 1) / 2;
        }
        static double Exponential(double x)
        {
            return x == 0
                ? 0
                : x == 1
                ? 1
                : x < 0.5 ? Math.Pow(2, 20 * x - 10) / 2
                : (2 - Math.Pow(2, -20 * x + 10)) / 2;
        }

        static double clamp(double d, double min, double max)
        {
            double t = d < min ? min : d;
            return t > max ? max : t;
        }


        static System.Drawing.Color interp(System.Drawing.Color color1, System.Drawing.Color color2, double fraction)
        {

            byte r = (byte)clamp(((color2.R - color1.R) * fraction + color1.R), 0, 255);
            byte g = (byte)clamp(((color2.G - color1.G) * fraction + color1.G), 0, 255);
            byte b = (byte)clamp(((color2.B - color1.B) * fraction + color1.B), 0, 255);

            return System.Drawing.Color.FromArgb(255, r, g, b);
        }

        public enum INTERPOLATE_FUNCTION
        {
            Linear,
            Sine,
            Cubic,
            Exponential,
            Back
        }

        public static System.Drawing.Color Interpolate(System.Drawing.Color color1, System.Drawing.Color color2, INTERPOLATE_FUNCTION which, double fraction)
        {
            switch (which)
            {
                case INTERPOLATE_FUNCTION.Linear:
                    {
                        return interp(color1, color2, fraction);
                    }
                case INTERPOLATE_FUNCTION.Sine:
                    {
                        return interp(color1, color2, Sine(fraction));
                    }
                case INTERPOLATE_FUNCTION.Cubic:
                    {
                        return interp(color1, color2, Cubic(fraction));
                    }
                case INTERPOLATE_FUNCTION.Exponential:
                    {
                        return interp(color1, color2, Exponential(fraction));
                    }
                case INTERPOLATE_FUNCTION.Back:
                    {
                        return interp(color1, color2, Back(fraction));
                    }
                default:
                    {
                        return interp(color1, color2, fraction);
                    }
            }
        }

    }
    static class ColorExtension
    {
        public static System.Drawing.Color ToDrawingColor(this System.Windows.Media.Color color, byte Alpha = 255)
        {
            return System.Drawing.Color.FromArgb(Alpha, color.R, color.G, color.B);
        }

        public static System.Windows.Media.Color ToMediaColor(this System.Drawing.Color color)
        {
            return System.Windows.Media.Color.FromRgb(color.R, color.G, color.B);
        }

        public static string HexConverter(System.Drawing.Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }
    }

    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((System.Drawing.Color)value).ToMediaColor();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((System.Windows.Media.Color)value).ToDrawingColor();
        }
    }
}
