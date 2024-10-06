using RainbowTaskbar.Configuration.InstructionConfig.Instructions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace RainbowTaskbar.Configuration.InstructionConfig;

public static class DefaultPresets {
    public static readonly InstructionPreset Rainbow = new() {
        Name = "Rainbow",
        RunOnceGroup = new InstructionGroup() {
            Instructions = {
                new BorderRadiusInstruction {
                     Radius = 16
                },
                new TransparencyInstruction {
                    Type = TransparencyInstruction.TransparencyInstructionType.Style,
                    Style = TransparencyInstruction.TransparencyInstructionStyle.Transparent
                },
                new TransparencyInstruction {
                    Type = TransparencyInstruction.TransparencyInstructionType.RainbowTaskbar,
                    Opacity = 0.8
                },
            }
        },
        LoopGroups = new BindingList<InstructionGroup>(new InstructionGroup[] {
            new InstructionGroup {
                Instructions = new BindingList<Instruction> {
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
                }
            }
        })
    };


    public static readonly InstructionPreset Chill = new() {
        Name = "Chill",
        RunOnceGroup = new InstructionGroup() {
            Instructions = {
                new TransparencyInstruction {
                    Type = TransparencyInstruction.TransparencyInstructionType.Style,
                    Style = TransparencyInstruction.TransparencyInstructionStyle.Transparent
                },
                new TransparencyInstruction {
                    Type = TransparencyInstruction.TransparencyInstructionType.RainbowTaskbar,
                    Opacity = 0.9
                },
            }
        },
        LoopGroups = new BindingList<InstructionGroup>(new InstructionGroup[] {
            new InstructionGroup {
                Instructions = new BindingList<Instruction> {
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
                } 
            }
        })
    };

    public static readonly InstructionPreset Unknown = new() {
        Name = "Unknown",
        RunOnceGroup = new InstructionGroup() {
            Instructions = {
                new TransparencyInstruction {
                    Type = TransparencyInstruction.TransparencyInstructionType.Style,
                    Style = TransparencyInstruction.TransparencyInstructionStyle.Transparent
                },
                new TransparencyInstruction {
                    Type = TransparencyInstruction.TransparencyInstructionType.RainbowTaskbar,
                    Opacity = 0.9
                },
            }
        },
        LoopGroups = new BindingList<InstructionGroup>(new InstructionGroup[] {
            new InstructionGroup {
                Instructions = new BindingList<Instruction> {
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
                } 
            }
        })
    };

    public static readonly InstructionPreset HighContrast = new() {
        Name = "High contrast",
        RunOnceGroup = new InstructionGroup() {
            Instructions = {
                new TransparencyInstruction {
                    Type = TransparencyInstruction.TransparencyInstructionType.Style,
                    Style = TransparencyInstruction.TransparencyInstructionStyle.Transparent
                },
                new ShapeInstruction {
                    Shape = ShapeInstruction.ShapeInstructionShapes.Rectangle,
                    Fill = Color.Transparent,
                    Line = Color.FromArgb(0, 255, 0),
                    Layer = 1,
                    LineSize = 4,
                    FitTaskbars = true
                },
                new ColorInstruction {
                    Time = 1001,
                    Color1 = Color.Black,
                    Effect = ColorInstruction.ColorInstructionEffect.Solid,
                }
            }
        }
    };

    public static readonly InstructionPreset ModernChill = new() {
        Name = "Modern Blue Chill",
        RunOnceGroup = new InstructionGroup() {
            Instructions = {
                 new BorderRadiusInstruction {
                     Radius = 16
                 },
                 new TransparencyInstruction {
                     Type = TransparencyInstruction.TransparencyInstructionType.Style,
                     Style = TransparencyInstruction.TransparencyInstructionStyle.Transparent
                 },
                 new TransparencyInstruction {
                     Type = TransparencyInstruction.TransparencyInstructionType.RainbowTaskbar,
                     Opacity = 0.9
                 },
                 new TransparencyInstruction {
                     Type = TransparencyInstruction.TransparencyInstructionType.Layer,
                     Opacity = 0.8,
                     Layer = 0,
                 },
            }
        },
        LoopGroups = new BindingList<InstructionGroup>(new InstructionGroup[] {
            new InstructionGroup {
                Instructions = new BindingList<Instruction> {
                    new ShapeInstruction {
                        FitTaskbars = true,
                        DrawOnce = true,
                        Shape = ShapeInstruction.ShapeInstructionShapes.Rectangle,
                        LineSize = 2,
                        Line = Color.FromArgb(255,0, 200, 255),
                        Fill = Color.Transparent,
                        Layer = 1
                    },
                    new ColorInstruction {
                        Effect = ColorInstruction.ColorInstructionEffect.FadeGradient,
                        Transition = ColorInstruction.ColorInstructionTransition.Sine,
                        Time = 3000,
                        Color1 = Color.FromArgb(255, 0, 136, 196),
                        Color2 = Color.Black,
                        Angle = 0,
                        Time2 = 5000,
                        Layer = 0,
                        Randomize = false
                    },
                    new ColorInstruction {
                        Effect = ColorInstruction.ColorInstructionEffect.FadeGradient,
                        Transition = ColorInstruction.ColorInstructionTransition.Sine,
                        Time = 3000,
                        Color1 = Color.Black,
                        Color2 = Color.FromArgb(255, 0, 136, 196),
                        Angle = 0,
                        Time2 = 5000,
                        Layer = 0,
                        Randomize = false
                    }
                } 
            }
        })
    };

    public static readonly InstructionPreset ModernChill2 = new() {
        Name = "Modern Red Chill",
        RunOnceGroup = new InstructionGroup() {
            Instructions = {
                new BorderRadiusInstruction {
                    Radius = 16
                },
                new TransparencyInstruction {
                    Type = TransparencyInstruction.TransparencyInstructionType.Style,
                    Style = TransparencyInstruction.TransparencyInstructionStyle.Transparent
                },
                new TransparencyInstruction {
                    Type = TransparencyInstruction.TransparencyInstructionType.RainbowTaskbar,
                    Opacity = 0.9
                },
                new TransparencyInstruction {
                    Type = TransparencyInstruction.TransparencyInstructionType.Layer,
                    Opacity = 0.8,
                    Layer = 0,
                },
            }
        },
        LoopGroups = new BindingList<InstructionGroup>(new InstructionGroup[] {
            new InstructionGroup {
                Instructions = new BindingList<Instruction> {
                    new ShapeInstruction {
                        FitTaskbars = true,
                        DrawOnce = true,
                        Shape = ShapeInstruction.ShapeInstructionShapes.Rectangle,
                        LineSize = 2,
                        Line = Color.FromArgb(255,255, 61, 0),
                        Fill = Color.Transparent,
                        Layer = 1
                    },
                    new ColorInstruction {
                        Effect = ColorInstruction.ColorInstructionEffect.FadeGradient,
                        Transition = ColorInstruction.ColorInstructionTransition.Sine,
                        Time = 3000,
                        Color1 = Color.FromArgb(255, 176, 0, 0),
                        Color2 = Color.Black,
                        Angle = 0,
                        Time2 = 5000,
                        Layer = 0,
                        Randomize = false
                    },
                    new ColorInstruction {
                        Effect = ColorInstruction.ColorInstructionEffect.FadeGradient,
                        Transition = ColorInstruction.ColorInstructionTransition.Sine,
                        Time = 3000,
                        Color1 = Color.Black,
                        Color2 = Color.FromArgb(255, 176, 0, 0),
                        Angle = 0,
                        Time2 = 5000,
                        Layer = 0,
                        Randomize = false
                    }
                } 
            }
        })
    };

    public static readonly InstructionPreset Translucent = new() {
        Name = "Translucent",
        RunOnceGroup = new InstructionGroup() {
            Instructions = {
                new TransparencyInstruction {
                    Type = TransparencyInstruction.TransparencyInstructionType.Style,
                    Style = TransparencyInstruction.TransparencyInstructionStyle.Transparent
                }
            }
        },
    };

    public static readonly InstructionPreset Blurred = new() {
        Name = "Blurred",
        RunOnceGroup = new InstructionGroup() {
            Instructions = {
                new TransparencyInstruction {
                    Type = TransparencyInstruction.TransparencyInstructionType.Style,
                    Style = TransparencyInstruction.TransparencyInstructionStyle.Blur
                }
            }
        },
    };

    public static readonly InstructionPreset Vaporwave = new() {
        Name = "Vaporwave",
        RunOnceGroup = new InstructionGroup() {
            Instructions = {
                new TransparencyInstruction {
                    Type = TransparencyInstruction.TransparencyInstructionType.Style,
                    Style = TransparencyInstruction.TransparencyInstructionStyle.Transparent
                },
                new TransparencyInstruction {
                    Type = TransparencyInstruction.TransparencyInstructionType.Layer,
                    Opacity = 0.6,
                    Layer = 0,
                },
            }
        },
        LoopGroups = new BindingList<InstructionGroup>(new InstructionGroup[] {
            new InstructionGroup {
                Instructions = new BindingList<Instruction> {
                    new ShapeInstruction {
                        FitTaskbars = true,
                        DrawOnce = true,
                        Shape = ShapeInstruction.ShapeInstructionShapes.Rectangle,
                        LineSize = 2,
                        Line = Color.FromArgb(255, 159,198, 255),
                        Fill = Color.Transparent,
                        Layer = 1
                    },
                    new TextInstruction {
                        DrawOnce = true,
                        Text = " レーンボー・タスクバー  レーンボー・タスクバー  レーンボー・タスクバー  レーンボー・タスクバー  レーンボー・タスクバー  レーンボー・タスクバー  レーンボー・タスクバー  レーンボー・タスクバー  レーンボー・タスクバー  レーンボー・タスクバー ",
                        Center = true,
                        Y = 6,
                        Font = "Arial",
                        Size = 32,
                        Color = Color.FromArgb(63,220,247,255),
                        Layer = 1
                    },
                    new TextInstruction {
                        DrawOnce = true,
                        Text = "◓        ◓        ◓        ◓        ◓        ◓        ◓        ◓        ◓        ◓        ◓        ◓        ◓        ◓        ◓        ◓        ◓        ◓        ◓        ◓        ◓        ◓        ◓        ◓        ◓",
                        X = 4,
                        Y = 6,
                        Font = "Arial",
                        Size = 32,
                        Color = Color.FromArgb(127,255,110,172),
                        Layer = 1
                    },
                    new TextInstruction {
                        DrawOnce = true,
                        Text = "◓        ◒        ◓        ◒        ◓        ◒        ◓        ◒        ◓        ◒        ◓        ◒        ◓        ◒        ◓        ◒        ◓        ◒        ◓        ◒        ◓        ◒        ◓        ◒        ◓",
                        X = 6,
                        Y = 8,
                        Font = "Arial",
                        Size = 32,
                        Color = Color.FromArgb(127,255,214,110),
                        Layer = 1
                    },
                    new TextInstruction {
                        DrawOnce = true,
                        Text = "ツ                ツ                ツ                ツ                ツ                ツ                ツ                ツ                ツ                ツ                ツ                ツ                ツ                ツ                ツ                ツ                ツ                ツ                ツ                ツ                ツ                ツ                ツ                ツ                ツ                ツ                ",
                        Center = true,
                        Y = 29,
                        Font = "Arial",
                        Size = 16,
                        Color = Color.FromArgb(200,110,255,182),
                        Layer = 1
                    },
                    new ColorInstruction {
                        Effect = ColorInstruction.ColorInstructionEffect.FadeGradient,
                        Transition = ColorInstruction.ColorInstructionTransition.Linear,
                        Time = 0,
                        Color1 = Color.FromArgb(255, 100, 112, 216),
                        Color2 = Color.FromArgb(255, 0, 185, 255),
                        Angle = 0,
                        Time2 = 1500,
                        Layer = 0,
                        Randomize = false
                    },
                    new ColorInstruction {
                        Effect = ColorInstruction.ColorInstructionEffect.FadeGradient,
                        Transition = ColorInstruction.ColorInstructionTransition.Linear,
                        Time = 0,
                        Color1 = Color.FromArgb(255, 0, 185, 255),
                        Color2 = Color.FromArgb(255, 255, 0, 158),
                        Angle = 0,
                        Time2 = 1500,
                        Layer = 0,
                        Randomize = false
                    },
                    new ColorInstruction {
                        Effect = ColorInstruction.ColorInstructionEffect.FadeGradient,
                        Transition = ColorInstruction.ColorInstructionTransition.Linear,
                        Time = 0,
                        Color1 = Color.FromArgb(255, 255, 0, 158),
                        Color2 = Color.FromArgb(255, 100, 112, 216),
                        Angle = 0,
                        Time2 = 1500,
                        Layer = 0,
                        Randomize = false
                    }
                } 
            }
        })
    };
    public static readonly List<InstructionPreset> Presets = new List<InstructionPreset>() {
        Translucent, Blurred, Rainbow, Chill,
        HighContrast, ModernChill,ModernChill2,
        Vaporwave
    };
}