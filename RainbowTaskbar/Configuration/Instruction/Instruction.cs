using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using FastMember;
using RainbowTaskbar.Configuration.Instruction.Instructions;
using RainbowTaskbar.Configuration.Web;


namespace RainbowTaskbar.Configuration.Instruction;

[JsonDerivedType(typeof(BorderRadiusInstruction), typeDiscriminator: "b")]
[JsonDerivedType(typeof(ClearLayerInstruction), typeDiscriminator: "c")]
[JsonDerivedType(typeof(ColorInstruction), typeDiscriminator: "cl")]
[JsonDerivedType(typeof(DelayInstruction), typeDiscriminator: "d")]
[JsonDerivedType(typeof(ImageInstruction), typeDiscriminator: "i")]
[JsonDerivedType(typeof(ShapeInstruction), typeDiscriminator: "s")]
[JsonDerivedType(typeof(TextInstruction), typeDiscriminator: "t")]
[JsonDerivedType(typeof(TransparencyInstruction), typeDiscriminator: "tr")]
public abstract class Instruction : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;


    public static IEnumerable<Type> InstructionTypes { get; set; } = GetKnownInstructionTypes();
    public static IEnumerable<Type> DisplayableInstructionTypes {
        get => InstructionTypes.Skip(1);
    }
    [JsonIgnore]
    public abstract string Description { get; }
    [JsonIgnore]
    public string Name {
        get {
            return App.localization.Name(GetType().Name.ToLower());
        }
    }
    [JsonIgnore]
    public string TypeName {
        get {
            return GetType().Name;
        }
    }


    public static IEnumerable<Type> GetKnownInstructionTypes() {
        if (InstructionTypes == null)
            InstructionTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(Instruction).IsAssignableFrom(type)).ToList();

        return InstructionTypes;
    }

    public bool Execute(Taskbar window) => Execute(window, CancellationToken.None);

    public abstract bool Execute(Taskbar window, CancellationToken token);

    // TODO: Json or remove this idk
    //public abstract JObject ToJSON();
    /*
    public static Instruction FromJSON(Type type, JObject json) {
        dynamic inst = type.GetConstructor(Array.Empty<Type>()).Invoke(null) as Instruction;


        foreach (var prop in json.Properties())
            if (prop.Name != "Name" && prop.Name != "Position") {
                var wrapped = ObjectAccessor.Create(inst);
                if (prop.Name.StartsWith("Color"))
                    wrapped[prop.Name] = ColorTranslator.FromHtml(prop.Value.Value<string>());
                else
                    if (wrapped[prop.Name] is not null)
                    wrapped[prop.Name] = Convert.ChangeType(prop.Value, wrapped[prop.Name].GetType());
                else
                    wrapped[prop.Name] = prop.Value;
            }

        return inst;
    }
    */
}