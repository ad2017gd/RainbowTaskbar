using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;

namespace RainbowTaskbar.Interpolation;

internal class ColorInterpolation {
    public enum INTERPOLATE_FUNCTION {
        Linear,
        Sine,
        Cubic,
        Exponential,
        Back
    }

    private static double Cubic(double x) => x < 0.5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2;

    private static double Back(double x) {
        const double c1 = 1.70158;
        const double c2 = c1 * 1.525;

        return x < 0.5
            ? Math.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2) / 2
            : (Math.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
    }

    private static double Sine(double x) => -(Math.Cos(3.14159 /*PI*/ * x) - 1) / 2;

    private static double Exponential(double x) =>
        x == 0
            ? 0
            : x == 1
                ? 1
                : x < 0.5
                    ? Math.Pow(2, 20 * x - 10) / 2
                    : (2 - Math.Pow(2, -20 * x + 10)) / 2;

    private static double clamp(double d, double min, double max) {
        var t = d < min ? min : d;
        return t > max ? max : t;
    }


    private static Color interp(Color color1, Color color2, double fraction) {
        var r = (byte) clamp((color2.R - color1.R) * fraction + color1.R, 0, 255);
        var g = (byte) clamp((color2.G - color1.G) * fraction + color1.G, 0, 255);
        var b = (byte) clamp((color2.B - color1.B) * fraction + color1.B, 0, 255);

        return Color.FromArgb(255, r, g, b);
    }

    public static Color Interpolate(Color color1, Color color2, INTERPOLATE_FUNCTION which, double fraction) {
        switch (which) {
            case INTERPOLATE_FUNCTION.Linear: {
                return interp(color1, color2, fraction);
            }
            case INTERPOLATE_FUNCTION.Sine: {
                return interp(color1, color2, Sine(fraction));
            }
            case INTERPOLATE_FUNCTION.Cubic: {
                return interp(color1, color2, Cubic(fraction));
            }
            case INTERPOLATE_FUNCTION.Exponential: {
                return interp(color1, color2, Exponential(fraction));
            }
            case INTERPOLATE_FUNCTION.Back: {
                return interp(color1, color2, Back(fraction));
            }
            default: {
                return interp(color1, color2, fraction);
            }
        }
    }
}

internal static class ColorExtension {
    public static Color ToDrawingColor(this System.Windows.Media.Color color) =>
        Color.FromArgb(color.A, color.R, color.G, color.B);

    public static System.Windows.Media.Color ToMediaColor(this Color color) =>
        System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);

    public static string HexConverter(Color c) => "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
}

public class ColorConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is not null ? ((Color) value).ToMediaColor() : null;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is not null ? ((System.Windows.Media.Color) value).ToDrawingColor() : null;
}