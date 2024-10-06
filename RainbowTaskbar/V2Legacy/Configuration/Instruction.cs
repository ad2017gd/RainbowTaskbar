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


namespace RainbowTaskbar.V2Legacy.Configuration;

[DataContract]
[KnownType("GetKnownInstructionTypes")]
public abstract class Instruction : INotifyPropertyChanged {
    public static IEnumerable<Type> InstructionTypes { get; set; } = GetKnownInstructionTypes();

    public abstract string Description { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    public static IEnumerable<Type> GetKnownInstructionTypes() {
        if (InstructionTypes == null)
            InstructionTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(Instruction).IsAssignableFrom(type)).ToList();

        return InstructionTypes;
    }

    public bool Execute(Taskbar window) => Execute(window, CancellationToken.None);

    public abstract bool Execute(Taskbar window, CancellationToken token);
}