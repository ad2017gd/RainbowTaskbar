using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using RainbowTaskbar.Configuration.Instructions;
using RainbowTaskbar.HTTPAPI;
using RainbowTaskbar.WebSocketServices;

namespace RainbowTaskbar.Configuration;

[DataContract]
public class Config : INotifyPropertyChanged {
    public static readonly string ConfigPath = Environment.ExpandEnvironmentVariables("%appdata%/rnbconf.xml");
    public static readonly string LegacyConfigPath = Environment.ExpandEnvironmentVariables("%appdata%/rnbconf.txt");

    private static readonly int SupportedConfigVersion = 3;

    public Config() {
        SetupPropertyChanged();
    }

    [field: DataMember] public int ConfigFileVersion { get; set; } = SupportedConfigVersion;

    [field: DataMember] public bool CheckUpdate { get; set; } = true;

    [OnChangedMethod(nameof(SetupPropertyChanged))]
    [field: DataMember]
    public BindingList<Instruction> Instructions { get; set; } = new BindingList<Instruction>();

    [OnChangedMethod(nameof(SetupPropertyChanged))]
    [field: DataMember]
    public BindingList<InstructionPreset> Presets { get; set; } = new BindingList<InstructionPreset>
            {DefaultPresets.Rainbow, DefaultPresets.Chill, DefaultPresets.Unknown};

    [field: DataMember]
    [OnChangedMethod(nameof(OnIsAPIEnabledChanged))]
    public bool IsAPIEnabled { get; set; } = false;

    [field: DataMember]
    [OnChangedMethod(nameof(OnAPIPortChanged))]
    public int APIPort { get; set; } = 9093;

    [field: DataMember]
    public int InterpolationQuality { get; set; } = 25;

    [field: DataMember]
    [OnChangedMethod(nameof(OnTaskbarBehaviourChanged))]
    public bool GraphicsRepeat { get; set; } = true;

    [field: DataMember]
    [OnChangedMethod(nameof(OnTaskbarBehaviourChanged))]
    public bool SameRadiusOnEach { get; set; } = false;

    public event PropertyChangedEventHandler PropertyChanged;

    public void SetupPropertyChanged() {
        Instructions.ListChanged += (_, _) => OnPropertyChanged(nameof(Instructions));
        Presets.ListChanged += (_, _) => OnPropertyChanged(nameof(Presets));
    }

    public void OnTaskbarBehaviourChanged() {
        App.ReloadTaskbars();
    }

    public CancellationTokenSource cts = new CancellationTokenSource();
    public Thread thread = null;
    public int configStep = 0;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public void OnIsAPIEnabledChanged() {
        if (API.http is not null && App.Config is not null) {
            if (IsAPIEnabled) API.Start();
            else
                API.Stop();
        }
    }

    public void OnAPIPortChanged() {
        if(App.Config is not null) 
            API.Start();
    }

    public JObject ToJSON() {
        dynamic data = new ExpandoObject();
        data.ConfigFileVersion = ConfigFileVersion;
        data.IsAPIEnabled = IsAPIEnabled;
        data.APIPort = APIPort;
        Application.Current.Dispatcher.Invoke(() => {
            data.RunAtStartup = App.editorViewModel.RunAtStartup;
        });

        data.Instructions = Instructions.Select(i => i.ToJSON());

        return JObject.FromObject(data);
    }

    public void ConfigThread(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            var slept = false;

            for (App.Config.configStep = 0;
                 App.Config.configStep < App.Config.Instructions.Count && !token.IsCancellationRequested;
                 App.Config.configStep++) {
                if (API.APISubscribed.Count > 0) {
                    var data = new JObject();
                    data.Add("type", "InstructionStep");
                    data.Add("index", App.Config.configStep);
                    data.Add("instruction", App.Config.Instructions[App.Config.configStep].ToJSON());
                    WebSocketAPIServer.SendToSubscribed(data.ToString());
                }

                try {


                    IntPtr pHandle = App.GetCurrentProcess();
                    App.SetProcessWorkingSetSize(pHandle, -1, -1);


                    var tasks = new List<Task>();
                    App.taskbars.ForEach(taskbar => {
                        tasks.Add(Task.Run(() => {
                            if (App.Config.Instructions[App.Config.configStep].Execute(taskbar, token)) slept = true; 
                        }));
                    });
                    Task.WaitAll(tasks.ToArray(), token);
                    
                }
                catch (Exception e) {
                    if (e.GetType() == typeof(System.OperationCanceledException) || (e.InnerException is not null && e.InnerException.GetType() == typeof(System.Threading.Tasks.TaskCanceledException))) {
                        return;
                    }
                    MessageBox.Show(
                        $"The \"{App.Config.Instructions[App.Config.configStep].Name}\" instruction at index {App.Config.configStep} (starting from 0) threw an exception, it will be removed from the config.\n${e.Message}",
                        "RainbowTaskbar", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Dispatcher.Invoke(() => {
                        App.Config.Instructions.RemoveAt(App.Config.configStep);
                        App.Config.ToFile();
                        App.ReloadTaskbars();
                    });
                    return;
                }
            }

            if (!slept) break;
        }
    }

    public void StopThread() {
        cts.Cancel();
        thread.Join();
        cts.Dispose();
        thread = null;
    }

    public void StartThread() {
        if (thread != null && thread.ThreadState != ThreadState.Stopped) {
            cts.Cancel();
            thread.Join();
            cts.Dispose();

        }
        
        cts = new CancellationTokenSource();
        
        thread = new Thread(() => { ConfigThread(cts.Token); });
        thread.Start();
    }

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
                                    : TransparencyInstruction.TransparencyInstructionStyle.Transparent;
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
            using var fileStream = new FileStream(ConfigPath, FileMode.OpenOrCreate);
            using var reader = XmlDictionaryReader.CreateTextReader(fileStream, new XmlDictionaryReaderQuotas());

            var serializerSettings = new DataContractSerializerSettings {
                PreserveObjectReferences = true
            };

            var serializer = new DataContractSerializer(typeof(Config), serializerSettings);
            var cfg = serializer.ReadObject(reader) as Config;
            if (cfg.ConfigFileVersion != SupportedConfigVersion) {
                switch (cfg.ConfigFileVersion) {
                    case 1:
                        cfg.Presets = new BindingList<InstructionPreset>
                            {DefaultPresets.Rainbow, DefaultPresets.Chill, DefaultPresets.Unknown};
                        cfg.ConfigFileVersion = SupportedConfigVersion;
                        goto case 2;
                    case 2:
                        cfg.InterpolationQuality = 25;
                        //cfg.SeparateTaskbarGraphics = false;
                        cfg.SameRadiusOnEach = false;
                        cfg.ConfigFileVersion = SupportedConfigVersion;
                        break;
                }
                cfg.ToFile();
            }

            cfg.SetupPropertyChanged();
            return cfg;
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

        var serializerSettings = new DataContractSerializerSettings {
            PreserveObjectReferences = true
        };

        var serializer = new DataContractSerializer(typeof(Config), serializerSettings);
        serializer.WriteObject(fileStream, this);
    }
}