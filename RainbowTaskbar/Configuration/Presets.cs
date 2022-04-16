using System.Drawing;
using RainbowTaskbar.Configuration.Instructions;

namespace RainbowTaskbar.Configuration;

internal class Presets {
    public static Instruction[] Rainbow() =>
        new Instruction[] {
            new BorderRadiusInstruction {
                Radius = 20
            },
            new TransparencyInstruction {
                Type = TransparencyInstruction.TransparencyInstructionType.Style,
                Style = TransparencyInstruction.TransparencyInstructionStyle.Transparent
            },
            new TransparencyInstruction {
                Type = TransparencyInstruction.TransparencyInstructionType.RainbowTaskbar,
                Opacity = 0.8
            },
            new ColorInstruction {
                Time = 1,
                Color1 = Color.FromArgb(255, 255, 0, 0),
                Color2 = Color.FromArgb(255, 255, 154, 0),
                Effect = ColorInstruction.ColorInstructionEffect.FadeGradient,
                Time2 = 500
            },
            new ColorInstruction {
                Time = 1,
                Color1 = Color.FromArgb(255, 255, 154, 0),
                Color2 = Color.FromArgb(255, 208, 222, 33),
                Effect = ColorInstruction.ColorInstructionEffect.FadeGradient,
                Time2 = 500
            },
            new ColorInstruction {
                Time = 1,
                Color1 = Color.FromArgb(255, 208, 222, 33),
                Color2 = Color.FromArgb(255, 79, 220, 74),
                Effect = ColorInstruction.ColorInstructionEffect.FadeGradient,
                Time2 = 500
            },
            new ColorInstruction {
                Time = 1,
                Color1 = Color.FromArgb(255, 79, 220, 74),
                Color2 = Color.FromArgb(255, 63, 218, 216),
                Effect = ColorInstruction.ColorInstructionEffect.FadeGradient,
                Time2 = 500
            },
            new ColorInstruction {
                Time = 1,
                Color1 = Color.FromArgb(255, 63, 218, 216),
                Color2 = Color.FromArgb(255, 47, 201, 226),
                Effect = ColorInstruction.ColorInstructionEffect.FadeGradient,
                Time2 = 500
            },
            new ColorInstruction {
                Time = 1,
                Color1 = Color.FromArgb(255, 47, 201, 226),
                Color2 = Color.FromArgb(255, 28, 127, 238),
                Effect = ColorInstruction.ColorInstructionEffect.FadeGradient,
                Time2 = 500
            },
            new ColorInstruction {
                Time = 1,
                Color1 = Color.FromArgb(255, 28, 127, 238),
                Color2 = Color.FromArgb(255, 95, 21, 242),
                Effect = ColorInstruction.ColorInstructionEffect.FadeGradient,
                Time2 = 500
            },
            new ColorInstruction {
                Time = 1,
                Color1 = Color.FromArgb(255, 95, 21, 242),
                Color2 = Color.FromArgb(255, 186, 12, 248),
                Effect = ColorInstruction.ColorInstructionEffect.FadeGradient,
                Time2 = 500
            },
            new ColorInstruction {
                Time = 1,
                Color1 = Color.FromArgb(255, 186, 12, 248),
                Color2 = Color.FromArgb(255, 251, 7, 217),
                Effect = ColorInstruction.ColorInstructionEffect.FadeGradient,
                Time2 = 500
            },
            new ColorInstruction {
                Time = 1,
                Color1 = Color.FromArgb(255, 251, 7, 217),
                Color2 = Color.FromArgb(255, 255, 0, 0),
                Effect = ColorInstruction.ColorInstructionEffect.FadeGradient,
                Time2 = 500
            }
        };


    public static Instruction[] Chill() =>
        new Instruction[] {
            new TransparencyInstruction {
                Type = TransparencyInstruction.TransparencyInstructionType.Style,
                Style = TransparencyInstruction.TransparencyInstructionStyle.Blur
            },
            new TransparencyInstruction {
                Type = TransparencyInstruction.TransparencyInstructionType.RainbowTaskbar,
                Opacity = 0.9
            },
            new ColorInstruction {
                Time = 5000,
                Color1 = Color.RoyalBlue,
                Color2 = Color.DarkBlue,
                Effect = ColorInstruction.ColorInstructionEffect.FadeGradient,
                Transition = ColorInstruction.ColorInstructionTransition.Cubic,
                Time2 = 3000
            },
            new ColorInstruction {
                Time = 5000,
                Color1 = Color.DarkBlue,
                Color2 = Color.RoyalBlue,
                Effect = ColorInstruction.ColorInstructionEffect.FadeGradient,
                Transition = ColorInstruction.ColorInstructionTransition.Cubic,
                Time2 = 3000
            }
        };

    public static Instruction[] Unknown() =>
        new Instruction[] {
            new TransparencyInstruction {
                Type = TransparencyInstruction.TransparencyInstructionType.Style,
                Style = TransparencyInstruction.TransparencyInstructionStyle.Transparent
            },
            new TransparencyInstruction {
                Type = TransparencyInstruction.TransparencyInstructionType.RainbowTaskbar,
                Opacity = 0.9
            },
            new ColorInstruction {
                Time = 1001,
                Randomize = true,
                Effect = ColorInstruction.ColorInstructionEffect.FadeGradient,
                Transition = ColorInstruction.ColorInstructionTransition.Linear,
                Time2 = 1000
            }
        };
}