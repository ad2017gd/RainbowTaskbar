using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using FastMember;
using Newtonsoft.Json.Linq;

namespace RainbowTaskbar.Configuration;

[DataContract]
[KnownType("GetKnownInstructionTypes")]
public abstract class Instruction : INotifyPropertyChanged {
    public static IEnumerable<Type> InstructionTypes { get; set; } = GetKnownInstructionTypes();

    public static IEnumerable<Type> DisplayableInstructionTypes {
        get => InstructionTypes.Skip(1);
    }

    public abstract string Name { get; }

    public event PropertyChangedEventHandler PropertyChanged;


    public static IEnumerable<Type> GetKnownInstructionTypes() {
        if (InstructionTypes == null)
            InstructionTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(Instruction).IsAssignableFrom(type)).ToList();

        return InstructionTypes;
    }

    public bool Execute(Taskbar window) => Execute(window, CancellationToken.None);

    public abstract bool Execute(Taskbar window, CancellationToken token);

    public abstract JObject ToJSON();

    public static Instruction FromJSON(Type type, JObject json) {
        dynamic inst = type.GetConstructor(Array.Empty<Type>()).Invoke(null) as Instruction;


        foreach (var prop in json.Properties())
            if (prop.Name != "Name" && prop.Name != "Position") {
                var wrapped = ObjectAccessor.Create(inst);
                if (prop.Name.StartsWith("Color"))
                    wrapped[prop.Name] = ColorTranslator.FromHtml(prop.Value.Value<string>());
                else
                    if(wrapped[prop.Name] is not null)
                        wrapped[prop.Name] = Convert.ChangeType(prop.Value, wrapped[prop.Name].GetType());
                    else
                        wrapped[prop.Name] = prop.Value;
            }

        return inst;
    }
}