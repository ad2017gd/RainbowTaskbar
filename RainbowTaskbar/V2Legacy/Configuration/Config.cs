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
using RainbowTaskbar.V2Legacy.Configuration.Instructions;

namespace RainbowTaskbar.V2Legacy.Configuration;

[DataContract]
public class Config : INotifyPropertyChanged {
    public Config() {
    }

    [field: DataMember] public int ConfigFileVersion { get; set; } = 0;

    [field: DataMember] public bool CheckUpdate { get; set; } = true;
    [field: DataMember] public long MagicCookie { get; set; } = 0;

    [field: DataMember]
    public BindingList<Instruction> Instructions { get; set; } = new BindingList<Instruction>();
    [field: DataMember]
    public BindingList<InstructionPreset> Presets { get; set; } = new BindingList<InstructionPreset>();

    [field: DataMember]
    public bool IsAPIEnabled { get; set; } = false;

    [field: DataMember]
    public int APIPort { get; set; } = 9093;

    [field: DataMember]
    public int InterpolationQuality { get; set; } = 25;

    [field: DataMember]
    public bool GraphicsRepeat { get; set; } = true;

    [field: DataMember]
    public bool SameRadiusOnEach { get; set; } = false;

    [field: DataMember]
    public string Language { get; set; } = "SystemDefault";

    [field: DataMember]
    public bool FirstStart { get; set; } = true;

    public event PropertyChangedEventHandler PropertyChanged;
}