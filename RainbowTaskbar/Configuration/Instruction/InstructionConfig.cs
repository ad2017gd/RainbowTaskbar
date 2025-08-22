using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Xml;

using PropertyChanged;
using RainbowTaskbar.HTTPAPI;
using System.Diagnostics;
using RainbowTaskbar.Configuration.Instruction.Instructions;
using System.Text.Json.Serialization;

namespace RainbowTaskbar.Configuration.Instruction;
[Serializable]
public class InstructionConfigData : ConfigData, INotifyPropertyChanged {

    private static readonly int SupportedConfigVersion = 0;
    public int ConfigFileVersion { get; set; } = SupportedConfigVersion;
    public InstructionGroup RunOnceGroup { get; set; } = new();
    [OnChangedMethod(nameof(SetupPropertyChanged))]
    public BindingList<InstructionGroup> LoopGroups { get; set; } = new BindingList<InstructionGroup>();

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // PropertyChanged bullshit
    private void SetupPropertyChanged() {
        LoopGroups.ListChanged += (_, _) => OnPropertyChanged(nameof(Instructions));
    }
    // ------------------------
    public InstructionConfigData() {
        SetupPropertyChanged();
    }
}

[Serializable]
public class InstructionConfig : Config {

    public CancellationTokenSource cts = new CancellationTokenSource();

    public InstructionConfigData Data { get => ConfigData as InstructionConfigData; set => ConfigData = value; }

    public InstructionConfig() {
        ConfigData = new InstructionConfigData();

        Data.RunOnceGroup = new InstructionGroup();
    }
    public override async Task<bool> Start() {
        if(!(await base.Start())) return false;
        App.taskbars.ForEach(x => {
            x.ClassicGrid.Visibility = System.Windows.Visibility.Visible;
            x.WebGrid.Visibility = System.Windows.Visibility.Collapsed;
        });
        Task.Run(() => StartGroupTasks());
        return true;
    }
    public override Task Stop() {
        return Task.WhenAll(StopGroupTasks());
    }

    public List<Task> StopGroupTasks() {
        var tasks = new List<Task>();
        if(cts is not null) {
            cts.Cancel();
            cts = null;
        }
        if(Data.RunOnceGroup.Task is not null) tasks.Add(Data.RunOnceGroup.Task);
        foreach(var group in Data.LoopGroups) {
            if(group.Task is not null) tasks.Add(group.Task);
        }

        return tasks;
    }

    public void StartGroupTasks() {

        Stop().Wait();
        cts = new CancellationTokenSource();
        Data.RunOnceGroup.StartOnceTask(cts.Token);
        Data.RunOnceGroup.Task.Wait(cts.Token);
        foreach (var group in Data.LoopGroups) {
            group.StartLoopTask(cts.Token);
        }


    }

    public static BindingList<Instruction> LegacyInstructionsToV2(BindingList<V2Legacy.Configuration.Instruction> instructions) {
        var cfg = new BindingList<Instruction>();


        foreach (var inst in instructions) {
            if (inst is V2Legacy.Configuration.Instructions.BorderRadiusInstruction binst) {
                cfg.Add(new BorderRadiusInstruction() {
                    Radius = binst.Radius,
                });
            }
            if (inst is V2Legacy.Configuration.Instructions.ClearLayerInstruction cinst) {
                cfg.Add(new ClearLayerInstruction() {
                    Layer = cinst.Layer,
                });
            }
            if (inst is V2Legacy.Configuration.Instructions.ColorInstruction clinst) {
                cfg.Add(new ColorInstruction() {
                    Angle = clinst.Angle,
                    Color1 = clinst.Color1,
                    Color2 = clinst.Color2,
                    Effect = (ColorInstruction.ColorInstructionEffect) clinst.Effect,
                    Randomize = clinst.Randomize,
                    Layer = clinst.Layer,
                    Time = clinst.Time,
                    Time2 = clinst.Time2,
                    Transition = (ColorInstruction.ColorInstructionTransition) clinst.Transition
                });
            }
            if (inst is V2Legacy.Configuration.Instructions.DelayInstruction dinst) {
                cfg.Add(new DelayInstruction() {
                    Time = dinst.Time,
                });
            }
            if (inst is V2Legacy.Configuration.Instructions.ImageInstruction iinst) {
                cfg.Add(new ImageInstruction() {
                    DrawOnce = iinst.DrawOnce,
                    Height = iinst.Height,
                    Layer = iinst.Layer,
                    Width = iinst.Width,
                    Opacity = iinst.Opacity,
                    Path = iinst.Path,
                    X = iinst.X,
                    Y = iinst.Y
                });
            }
            if (inst is V2Legacy.Configuration.Instructions.ShapeInstruction sinst) {
                cfg.Add(new ShapeInstruction() {
                    DrawOnce = sinst.DrawOnce,
                    Shape = (ShapeInstruction.ShapeInstructionShapes) sinst.Shape,
                    Fill = sinst.Fill,
                    FitTaskbars = sinst.FitTaskbars,
                    Layer = sinst.Layer,
                    Line = sinst.Line,
                    LineSize = sinst.LineSize,
                    Radius = sinst.Radius,
                    X2 = sinst.X2,
                    Y2 = sinst.Y2,
                    X = sinst.X,
                    Y = sinst.Y
                });
            }
            if (inst is V2Legacy.Configuration.Instructions.TextInstruction tinst) {
                cfg.Add(new TextInstruction() {
                    Size = tinst.Size,
                    Center = tinst.Center,
                    Color = tinst.Color,
                    DrawOnce = tinst.DrawOnce,
                    Font = tinst.Font,
                    Layer = tinst.Layer,
                    Text = tinst.Text,
                    X = tinst.X,
                    Y = tinst.Y,
                });
            }
            if (inst is V2Legacy.Configuration.Instructions.TransparencyInstruction trinst) {
                cfg.Add(new TransparencyInstruction() {
                    Layer = trinst.Layer,
                    Style = (TransparencyInstruction.TransparencyInstructionStyle) trinst.Style,
                    Opacity = trinst.Opacity,
                    Type = (TransparencyInstruction.TransparencyInstructionType) trinst.Type
                });
            }
        }

        return cfg;
    }


    public static InstructionConfig FromLegacyConfig(V2Legacy.Configuration.Config config) {
        var cfg = new InstructionConfig();
        cfg.InitNew();
        var cfgData = (InstructionConfigData)cfg.ConfigData;
        cfgData.LoopGroups.Add(new());
        cfgData.LoopGroups[0].Instructions = LegacyInstructionsToV2(config.Instructions);
        cfg.Name = "(MIGRATED) Current";
        return cfg;
    }
    public static InstructionConfig FromLegacyPreset(V2Legacy.Configuration.InstructionPreset preset) {
        var cfg = new InstructionConfig();
        cfg.InitNew();
        var cfgData = (InstructionConfigData) cfg.ConfigData;
        cfgData.LoopGroups.Add(new());
        cfgData.LoopGroups[0].Instructions = LegacyInstructionsToV2(preset.Instructions);
        cfg.Name = "(MIGRATED) " + preset.Name;
        

        return cfg;
    }
}