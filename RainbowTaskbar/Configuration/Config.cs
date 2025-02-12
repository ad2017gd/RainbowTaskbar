using RainbowTaskbar.Configuration.Instruction;
using RainbowTaskbar.Configuration.Instruction.Instructions;
using RainbowTaskbar.Configuration.Web;
using RainbowTaskbar.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RainbowTaskbar.Configuration {
    public enum ConfigPublishStatus {
        NotPublished,
        PreviouslyPublished,
        Published
    }

    [JsonDerivedType(typeof(InstructionConfigData), typeDiscriminator: "c")]
    [JsonDerivedType(typeof(WebConfigData), typeDiscriminator: "w")]
    [Serializable]
    public abstract class  ConfigData {
        
    }

    [JsonDerivedType(typeof(InstructionConfig), typeDiscriminator: "c")]
    [JsonDerivedType(typeof(WebConfig), typeDiscriminator: "w")]
    [Serializable]
    public abstract class Config : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public string Name { get; set; } = "Untitled";
        public string Description { get; set; } = "";
        public string? PublishedID { get; set; }
        public string? PreviousPublishedID { get; set; }
        public string? CachedPublisherUsername { get; set; }
        public string? CachedBase64Thumbnail { get; set; }
        [JsonIgnore]
        public int? CachedLikeCount { get; set; }
        public string LocalID { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Updated { get; set; } = DateTime.Now;
        public DateTime Published { get; set; } = DateTime.MinValue;
        [JsonIgnore]
        public bool ChangedSincePublish { get => Updated > Published; }
        [JsonIgnore]
        public ConfigPublishStatus PublishStatus { get => PublishedID is not null ? (ChangedSincePublish ? ConfigPublishStatus.PreviouslyPublished : ConfigPublishStatus.Published) : ConfigPublishStatus.NotPublished; }

        public static Config currentlyRunning;
        public ConfigData ConfigData { get; set; }
        [JsonIgnore]
        public static JsonSerializerOptions SerializerOptions { get; set; } = new JsonSerializerOptions { Converters = { new JsonColorConverter() } };
        [JsonIgnore]
        public string ConfigType { get => this.GetType().Name; }

        [JsonIgnore]
        public string fileName = null;

        public void InitNew() {
            LocalID = Guid.NewGuid().ToString();
        }
        public static Config FromFile(string file) {
            if (!File.Exists(file)) return null;
            Config cfg = JsonSerializer.Deserialize<Config>(File.ReadAllText(file), SerializerOptions);
            cfg.fileName = file;
            return cfg;
        }
        public void ToFile() {
            if (fileName is null)
                fileName = Path.Join(App.configDir, LocalID + ".json");
            File.WriteAllText(fileName,
                JsonSerializer.Serialize(this, SerializerOptions));
        }
        public Config Copy() {
            return JsonSerializer.Deserialize<Config>(JsonSerializer.Serialize(this, SerializerOptions), SerializerOptions);
        }
        public void DeleteFile() {
            if (File.Exists(fileName)) File.Delete(fileName);
        }

        public virtual void Start() {
            App.taskbars.ForEach(x => {
                new TransparencyInstruction() {
                    Type = TransparencyInstruction.TransparencyInstructionType.All,
                    Opacity = 1
                }.Execute(x);
            });
            App.taskbars.ForEach(x => {
                new TransparencyInstruction() {
                    Type = TransparencyInstruction.TransparencyInstructionType.Style,
                    Style = TransparencyInstruction.TransparencyInstructionStyle.Default,
                }.Execute(x);
            });
            if (currentlyRunning is not null) currentlyRunning.Stop().Wait();
            currentlyRunning = this;
           
            



        }
        public abstract Task Stop();


        public BitmapImage? LoadImage() {
            if (CachedBase64Thumbnail is null) return null;

            try {
                byte[] imageBytes = Convert.FromBase64String(CachedBase64Thumbnail);

                BitmapImage bi = new BitmapImage();

                bi.BeginInit();
                bi.StreamSource = new MemoryStream(imageBytes, 0, imageBytes.Length);
                bi.EndInit();

                return bi;

            }
            catch { return null; }

        }


    }


}
