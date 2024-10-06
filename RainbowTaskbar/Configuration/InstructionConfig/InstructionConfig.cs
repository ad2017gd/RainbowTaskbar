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
using RainbowTaskbar.WebSocketServices;
using System.Diagnostics;
using RainbowTaskbar.Configuration.InstructionConfig.Instructions;

namespace RainbowTaskbar.Configuration.InstructionConfig;


public class InstructionConfig : Config {

    private static readonly int SupportedConfigVersion = 0;
    public CancellationTokenSource cts = new CancellationTokenSource();

    // PropertyChanged bullshit
    private void SetupPropertyChanged() {
        LoopGroups.ListChanged += (_, _) => OnPropertyChanged(nameof(Instructions));
    }
    // ------------------------

    public int ConfigFileVersion { get; set; } = SupportedConfigVersion;
    public InstructionGroup RunOnceGroup { get; set; } = null;
    [OnChangedMethod(nameof(SetupPropertyChanged))]
    public BindingList<InstructionGroup> LoopGroups { get; set; } = new BindingList<InstructionGroup>();



    public InstructionConfig() {
        SetupPropertyChanged();
        RunOnceGroup = new InstructionGroup();
    }
    

    public void StopGroupTasks() {
        if(cts is not null) {
            cts.Cancel();
            cts = null;
        }
        RunOnceGroup.Task.Wait();
        foreach(var group in LoopGroups) {
            group.Task.Wait();
        }
    }

    public void StartGroupTasks() {

        StopGroupTasks();
        cts = new CancellationTokenSource();
        RunOnceGroup.StartTask(cts.Token);
        RunOnceGroup.Task.Wait(cts.Token);
        foreach (var group in LoopGroups) {
            group.StartTask(cts.Token);
        }


    }
}