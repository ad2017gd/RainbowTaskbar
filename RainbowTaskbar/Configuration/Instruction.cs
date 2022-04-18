using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;

namespace RainbowTaskbar.Configuration;

[DataContract]
[KnownType("GetKnownInstructionTypes")]
public abstract class Instruction : INotifyPropertyChanged {
    public static IEnumerable<Type> InstructionTypes { get; set; } = GetKnownInstructionTypes();

    public static IEnumerable<Type> DisplayableInstructionTypes {
        get => InstructionTypes.Skip(1);
    }

    public string Name {
        get => Regex.Replace(GetType().Name.Replace("Instruction", ""), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))",
            " $0").TrimStart();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    //todo: maybe skip 1 here as well? to test
    public static IEnumerable<Type> GetKnownInstructionTypes() {
        if (InstructionTypes == null)
            InstructionTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(Instruction).IsAssignableFrom(type)).ToList();

        return InstructionTypes;
    }

    public bool Execute(Taskbar window) => Execute(window, CancellationToken.None);

    public abstract bool Execute(Taskbar window, CancellationToken token);
}