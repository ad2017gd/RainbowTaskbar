using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Xml;
using Dahomey.Json;
using Dahomey.Json.Serialization.Conventions;
using PropertyChanged;
using RainbowTaskbar.API.WebSocket;
using RainbowTaskbar.Configuration.Instructions;

namespace RainbowTaskbar.Configuration;

[DataContract]
public class Config : INotifyPropertyChanged {
    public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions() {

    }.SetupExtensions();
    
    public static readonly string ConfigPath = Environment.ExpandEnvironmentVariables("%appdata%/rnbconf.json");
    public static readonly string OldConfigPath = Environment.ExpandEnvironmentVariables("%appdata%/rnbconf.xml");
    public static readonly string LegacyConfigPath = Environment.ExpandEnvironmentVariables("%appdata%/rnbconf.txt");

    private static readonly int SupportedConfigVersion = 2;

    public Config() {


        SetupPropertyChanged();
    }

    [DataMember] public int ConfigFileVersion { get; set; } = SupportedConfigVersion;

    [DataMember] public bool CheckUpdate { get; set; } = true;

    [OnChangedMethod(nameof(SetupPropertyChanged))]
    [DataMember]
    public BindingList<Instruction> Instructions { get; set; } = new BindingList<Instruction>();

    [OnChangedMethod(nameof(SetupPropertyChanged))]
    [DataMember]
    public BindingList<InstructionPreset> Presets { get; set; } = new BindingList<InstructionPreset>
            {DefaultPresets.Rainbow, DefaultPresets.Chill, DefaultPresets.Unknown};

    [DataMember]
    [OnChangedMethod(nameof(OnIsAPIEnabledChanged))]
    public bool IsAPIEnabled { get; set; } = false;

    [DataMember]
    [OnChangedMethod(nameof(OnAPIPortChanged))]
    public int APIPort { get; set; } = 9093;

    public event PropertyChangedEventHandler PropertyChanged;

    public void SetupPropertyChanged() {
        Instructions.ListChanged += (_, _) => OnPropertyChanged(nameof(Instructions));
        Presets.ListChanged += (_, _) => OnPropertyChanged(nameof(Presets));
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public void OnIsAPIEnabledChanged() {
        if (API.API.http is not null) {
            if (IsAPIEnabled) API.API.Start();
            else
                API.API.Stop();
        }
    }

    public void OnAPIPortChanged() => API.API.Start();

    public static Config FromFile() {
        if (File.Exists(LegacyConfigPath)) {
            var cfg = new Config();

            cfg.ToFile();

            var legacyCfg = File.ReadAllLines(LegacyConfigPath);
            foreach (var line in legacyCfg) {
                if (line.Length == 0 || line[0] == '#' || line[0] == '\r' || line[0] == '\n') continue;
                // %c %i %i %i %i %s %i %i %i %i %i %i (%127s)

                int time = 0, r = 0, g = 0, b = 0, ef1 = 0, ef2 = 0, ef3 = 0, ef4 = 0;
                var effect = "";

                var parts = line.Split(" ");
                var prefix = Convert.ToChar(parts[0]);
                if (parts.Length > 1) time = Convert.ToInt32(parts[1]);

                if (parts.Length > 2) r = Convert.ToInt32(parts[2]);
                if (parts.Length > 3) g = Convert.ToInt32(parts[3]);
                if (parts.Length > 4) b = Convert.ToInt32(parts[4]);

                if (parts.Length > 5) effect = parts[5];

                if (parts.Length > 6) ef1 = Convert.ToInt32(parts[6]);
                if (parts.Length > 7) ef2 = Convert.ToInt32(parts[7]);
                if (parts.Length > 8) ef3 = Convert.ToInt32(parts[8]);
                if (parts.Length > 9) ef4 = Convert.ToInt32(parts[9]);

                switch (prefix) {
                    case 'w':
                        var w = new DelayInstruction {
                            Time = time
                        };
                        cfg.Instructions.Add(w);
                        break;
                    case 't':
                        var t = new TransparencyInstruction();
                        switch (time) {
                            case 1:
                                t.Type = TransparencyInstruction.TransparencyInstructionType.Taskbar;
                                break;
                            case 2:
                                t.Type = TransparencyInstruction.TransparencyInstructionType.RainbowTaskbar;
                                break;
                            case 3:
                                t.Type = TransparencyInstruction.TransparencyInstructionType.All;
                                break;
                            case 4:
                                t.Type = TransparencyInstruction.TransparencyInstructionType.Style;
                                t.Style = r > 0
                                    ? TransparencyInstruction.TransparencyInstructionStyle.Blur
                                    : TransparencyInstruction.TransparencyInstructionStyle.Default;
                                break;
                        }

                        t.Opacity = r / 255.0;
                        cfg.Instructions.Add(t);
                        break;
                    case 'b':
                        var bd = new BorderRadiusInstruction {
                            Radius = time
                        };
                        cfg.Instructions.Add(bd);
                        break;
                    case 'i':
                        var i = new ImageInstruction {
                            Path = effect,
                            X = time,
                            Y = r,
                            Width = g,
                            Height = b,
                            Opacity = ef1 / 255.0
                        };
                        cfg.Instructions.Add(i);
                        break;
                    case 'c':
                        var c = new ColorInstruction {
                            Time = time,
                            Color1 = Color.FromArgb(255, r, g, b)
                        };

                        switch (effect) {
                            case "none":
                                c.Effect = ColorInstruction.ColorInstructionEffect.Solid;
                                break;
                            case "fade":
                                c.Effect = ColorInstruction.ColorInstructionEffect.Fade;
                                c.Time2 = ef1;
                                switch (ef2) {
                                    case 0:
                                        c.Transition = ColorInstruction.ColorInstructionTransition.Linear;
                                        break;
                                    case 1:
                                        c.Transition = ColorInstruction.ColorInstructionTransition.Sine;
                                        break;
                                    case 2:
                                        c.Transition = ColorInstruction.ColorInstructionTransition.Cubic;
                                        break;
                                    case 3:
                                        c.Transition = ColorInstruction.ColorInstructionTransition.Exponential;
                                        break;
                                    case 4:
                                        c.Transition = ColorInstruction.ColorInstructionTransition.Back;
                                        break;
                                }

                                break;
                            case "fgrd":
                                c.Effect = ColorInstruction.ColorInstructionEffect.FadeGradient;
                                c.Color2 = Color.FromArgb(255, ef1, ef2, ef3);
                                c.Time2 = ef4;
                                switch (ef2) {
                                    case 0:
                                        c.Transition = ColorInstruction.ColorInstructionTransition.Linear;
                                        break;
                                    case 1:
                                        c.Transition = ColorInstruction.ColorInstructionTransition.Sine;
                                        break;
                                    case 2:
                                        c.Transition = ColorInstruction.ColorInstructionTransition.Cubic;
                                        break;
                                    case 3:
                                        c.Transition = ColorInstruction.ColorInstructionTransition.Exponential;
                                        break;
                                    case 4:
                                        c.Transition = ColorInstruction.ColorInstructionTransition.Back;
                                        break;
                                }

                                break;
                            case "grad":
                                c.Effect = ColorInstruction.ColorInstructionEffect.Gradient;
                                c.Color2 = Color.FromArgb(255, ef1, ef2, ef3);

                                break;
                        }

                        cfg.Instructions.Add(c);
                        break;
                }
            }

            cfg.ToFile();

            File.Move(LegacyConfigPath, Environment.ExpandEnvironmentVariables("%appdata%/rnbconf.bak.txt"), true);
        }

        try {
            if (File.Exists(OldConfigPath)) {
                using var oldFileStream = new FileStream(OldConfigPath, FileMode.OpenOrCreate);
                using var reader = XmlDictionaryReader.CreateTextReader(oldFileStream, new XmlDictionaryReaderQuotas());

                var serializerSettings = new DataContractSerializerSettings {
                    PreserveObjectReferences = true
                };

                var serializer = new DataContractSerializer(typeof(Config), serializerSettings);
                var cfg = serializer.ReadObject(reader) as Config;
                if (cfg.ConfigFileVersion != SupportedConfigVersion)
                    switch (cfg.ConfigFileVersion) {
                        case 1:
                            cfg.Presets = new BindingList<InstructionPreset>
                                {DefaultPresets.Rainbow, DefaultPresets.Chill, DefaultPresets.Unknown};
                            cfg.ConfigFileVersion = SupportedConfigVersion;
                            break;
                    }

                cfg.SetupPropertyChanged();
                return cfg;
            }
            
            using var fileStream = new FileStream(ConfigPath, FileMode.OpenOrCreate);
            return JsonSerializer.Deserialize<Config>(fileStream, JsonOptions);
        }
        catch {
            var cfg = new Config {
                Instructions = new BindingList<Instruction>(DefaultPresets.Rainbow.Instructions)
            };

            cfg.ToFile();
            return cfg;
        }
    }

    public void ToFile() {
        using var fileStream = new FileStream(ConfigPath, FileMode.Create);
        JsonSerializer.Serialize(fileStream, this, JsonOptions);
    }
}