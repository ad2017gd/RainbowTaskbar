using System;
using System.ComponentModel;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Xml;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using RainbowTaskbar.Configuration.Instructions;

namespace RainbowTaskbar.Configuration;

[DataContract]
public class Config : INotifyPropertyChanged {
    public static readonly string CONFIG_PATH = Environment.ExpandEnvironmentVariables("%appdata%/rnbconf.xml");
    public static readonly string OLD_CONFIG_PATH = Environment.ExpandEnvironmentVariables("%appdata%/rnbconf.txt");

    [field: DataMember] public int ConfigFileVersion { get; set; } = 1;

    [field: DataMember] public bool CheckUpdate { get; set; } = true;

    [field: DataMember] public BindingList<Instruction> Instructions { get; set; } = new();

    [field: DataMember]
    [OnChangedMethod(nameof(OnIsAPIEnabledChanged))]
    public bool IsAPIEnabled { get; set; } = false;

    [field: DataMember]
    [OnChangedMethod(nameof(OnAPIPortChanged))]
    public int APIPort { get; set; } = 9093;

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnIsAPIEnabledChanged() {
        if (App.http is not null) {
            if (IsAPIEnabled) App.ConfigureServer();
            else
                App.http.Stop();
        }
    }

    public void OnAPIPortChanged() => App.ConfigureServer();

    public JObject ToJSON() {
        dynamic data = new ExpandoObject();
        data.ConfigFileVersion = ConfigFileVersion;
        data.IsAPIEnabled = IsAPIEnabled;
        data.APIPort = APIPort;
        Application.Current.Dispatcher.Invoke(() => {
            data.RunAtStartup = ((Editor) Application.Current.MainWindow).viewModel.RunAtStartup;
        });

        data.Instructions = Instructions.Select(i => i.ToJSON());

        return JObject.FromObject(data);
    }

    public static Config FromFile() {
        if (File.Exists(OLD_CONFIG_PATH)) {
            var cfg = new Config();

            cfg.ToFile();

            var conf = File.ReadAllLines(OLD_CONFIG_PATH);
            foreach (var line in conf) {
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
                        var w = new DelayInstruction();
                        w.Time = time;
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
                        var bd = new BorderRadiusInstruction();
                        bd.Radius = time;
                        cfg.Instructions.Add(bd);
                        break;
                    case 'i':
                        var i = new ImageInstruction();
                        i.Path = effect;
                        i.X = time;
                        i.Y = r;
                        i.Width = g;
                        i.Height = b;
                        i.Opacity = ef1 / 255.0;
                        cfg.Instructions.Add(i);
                        break;
                    case 'c':
                        var c = new ColorInstruction();
                        c.Time = time;
                        c.Color1 = Color.FromArgb(255, r, g, b);

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

            File.Move(OLD_CONFIG_PATH, Environment.ExpandEnvironmentVariables("%appdata%/rnbconf.bak.txt"), true);
        }

        try {
            using var fileStream = new FileStream(CONFIG_PATH, FileMode.OpenOrCreate);
            using var reader = XmlDictionaryReader.CreateTextReader(fileStream, new XmlDictionaryReaderQuotas());

            var serializerSettings = new DataContractSerializerSettings {
                PreserveObjectReferences = true
            };

            var serializer = new DataContractSerializer(typeof(Config), serializerSettings);
            return serializer.ReadObject(reader) as Config;
        }
        catch {
            var cfg = new Config();
            cfg.Instructions = new BindingList<Instruction>(Presets.Rainbow());

            cfg.ToFile();
            return cfg;
        }
    }

    public void ToFile() {
        using var fileStream = new FileStream(CONFIG_PATH, FileMode.Create);

        var serializerSettings = new DataContractSerializerSettings {
            PreserveObjectReferences = true
        };

        var serializer = new DataContractSerializer(typeof(Config), serializerSettings);
        serializer.WriteObject(fileStream, this);
    }
}