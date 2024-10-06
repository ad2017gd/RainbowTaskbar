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
    public override void Start() {
        base.Start();
        App.taskbars.ForEach(x => {
            x.ClassicGrid.Visibility = System.Windows.Visibility.Visible;
            x.WebGrid.Visibility = System.Windows.Visibility.Collapsed;
        });
        Task.Run(() => StartGroupTasks());
    }
    public override async Task Stop() {
        StopGroupTasks();
    }

    public void StopGroupTasks() {
        if(cts is not null) {
            cts.Cancel();
            cts = null;
        }
        if(Data.RunOnceGroup.Task is not null) Data.RunOnceGroup.Task.Wait();
        foreach(var group in Data.LoopGroups) {
            if(group.Task is not null) group.Task.Wait();
        }
    }

    public void StartGroupTasks() {

        StopGroupTasks();
        cts = new CancellationTokenSource();
        Data.RunOnceGroup.StartOnceTask(cts.Token);
        Data.RunOnceGroup.Task.Wait(cts.Token);
        foreach (var group in Data.LoopGroups) {
            group.StartLoopTask(cts.Token);
        }


    }
}