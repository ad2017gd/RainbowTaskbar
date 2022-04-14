namespace RainbowTaskbar.Configuration
{
    class Presets
    {
        public static Instruction[] Rainbow()
        {
            return new Instruction[] {
            new Instructions.BorderRadiusInstruction()
            {
                Radius = 20
            },
            new Instructions.TransparencyInstruction()
            {
                Type = Instructions.TransparencyInstruction.TransparencyInstructionType.Style,
                Style = Instructions.TransparencyInstruction.TransparencyInstructionStyle.Transparent,
            },
            new Instructions.TransparencyInstruction()
            {
                Type = Instructions.TransparencyInstruction.TransparencyInstructionType.RainbowTaskbar,
                Opacity = 0.8
            },
            new Instructions.ColorInstruction()
            {
                Time = 1,
                Color1 = System.Drawing.Color.FromArgb(255, 255, 0, 0),
                Color2 = System.Drawing.Color.FromArgb(255, 255, 154, 0),
                Effect = Instructions.ColorInstruction.ColorInstructionEffect.FadeGradient,
                Time2 = 500,
            },
            new Instructions.ColorInstruction()
            {
                Time = 1,
                Color1 = System.Drawing.Color.FromArgb(255, 255, 154, 0),
                Color2 = System.Drawing.Color.FromArgb(255, 208, 222, 33),
                Effect = Instructions.ColorInstruction.ColorInstructionEffect.FadeGradient,
                Time2 = 500,
            },
            new Instructions.ColorInstruction()
            {
                Time = 1,
                Color1 = System.Drawing.Color.FromArgb(255, 208, 222, 33),
                Color2 = System.Drawing.Color.FromArgb(255, 79, 220, 74),
                Effect = Instructions.ColorInstruction.ColorInstructionEffect.FadeGradient,
                Time2 = 500,
            },
            new Instructions.ColorInstruction()
            {
                Time = 1,
                Color1 = System.Drawing.Color.FromArgb(255, 79, 220, 74),
                Color2 = System.Drawing.Color.FromArgb(255, 63, 218, 216),
                Effect = Instructions.ColorInstruction.ColorInstructionEffect.FadeGradient,
                Time2 = 500,
            },
            new Instructions.ColorInstruction()
            {
                Time = 1,
                Color1 = System.Drawing.Color.FromArgb(255, 63, 218, 216),
                Color2 = System.Drawing.Color.FromArgb(255, 47, 201, 226),
                Effect = Instructions.ColorInstruction.ColorInstructionEffect.FadeGradient,
                Time2 = 500,
            },
            new Instructions.ColorInstruction()
            {
                Time = 1,
                Color1 = System.Drawing.Color.FromArgb(255, 47, 201, 226),
                Color2 = System.Drawing.Color.FromArgb(255, 28, 127, 238),
                Effect = Instructions.ColorInstruction.ColorInstructionEffect.FadeGradient,
                Time2 = 500,
            },
            new Instructions.ColorInstruction()
            {
                Time = 1,
                Color1 = System.Drawing.Color.FromArgb(255, 28, 127, 238),
                Color2 = System.Drawing.Color.FromArgb(255, 95, 21, 242),
                Effect = Instructions.ColorInstruction.ColorInstructionEffect.FadeGradient,
                Time2 = 500,
            },
            new Instructions.ColorInstruction()
            {
                Time = 1,
                Color1 = System.Drawing.Color.FromArgb(255, 95, 21, 242),
                Color2 = System.Drawing.Color.FromArgb(255, 186, 12, 248),
                Effect = Instructions.ColorInstruction.ColorInstructionEffect.FadeGradient,
                Time2 = 500,
            },
            new Instructions.ColorInstruction()
            {
                Time = 1,
                Color1 = System.Drawing.Color.FromArgb(255, 186, 12, 248),
                Color2 = System.Drawing.Color.FromArgb(255, 251, 7, 217),
                Effect = Instructions.ColorInstruction.ColorInstructionEffect.FadeGradient,
                Time2 = 500,
            },
            new Instructions.ColorInstruction()
            {
                Time = 1,
                Color1 = System.Drawing.Color.FromArgb(255, 251, 7, 217),
                Color2 = System.Drawing.Color.FromArgb(255, 255, 0, 0),
                Effect = Instructions.ColorInstruction.ColorInstructionEffect.FadeGradient,
                Time2 = 500,
            }

            };
        }


        public static Instruction[] Chill()
        {
            return new Instruction[] {
            new Instructions.TransparencyInstruction()
            {
                Type = Instructions.TransparencyInstruction.TransparencyInstructionType.Style,
                Style = Instructions.TransparencyInstruction.TransparencyInstructionStyle.Blur,
            },
            new Instructions.TransparencyInstruction()
            {
                Type = Instructions.TransparencyInstruction.TransparencyInstructionType.RainbowTaskbar,
                Opacity = 0.9
            },
            new Instructions.ColorInstruction()
            {
                Time = 5000,
                Color1 = System.Drawing.Color.RoyalBlue,
                Color2 = System.Drawing.Color.DarkBlue,
                Effect = Instructions.ColorInstruction.ColorInstructionEffect.FadeGradient,
                Transition = Instructions.ColorInstruction.ColorInstructionTransition.Cubic,
                Time2 = 3000,
            },
            new Instructions.ColorInstruction()
            {
                Time = 5000,
                Color1 = System.Drawing.Color.DarkBlue,
                Color2 = System.Drawing.Color.RoyalBlue,
                Effect = Instructions.ColorInstruction.ColorInstructionEffect.FadeGradient,
                Transition = Instructions.ColorInstruction.ColorInstructionTransition.Cubic,
                Time2 = 3000,
            }

            };
        }

        public static Instruction[] Unknown()
        {
            return new Instruction[] {
            new Instructions.TransparencyInstruction()
            {
                Type = Instructions.TransparencyInstruction.TransparencyInstructionType.Style,
                Style = Instructions.TransparencyInstruction.TransparencyInstructionStyle.Transparent,
            },
            new Instructions.TransparencyInstruction()
            {
                Type = Instructions.TransparencyInstruction.TransparencyInstructionType.RainbowTaskbar,
                Opacity = 0.9
            },
            new Instructions.ColorInstruction()
            {
                Time = 1001,
                Randomize = true,
                Effect = Instructions.ColorInstruction.ColorInstructionEffect.FadeGradient,
                Transition = Instructions.ColorInstruction.ColorInstructionTransition.Linear,
                Time2 = 1000,
            }

            };
        }
    }
}
