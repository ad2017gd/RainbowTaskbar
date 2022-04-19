using System.ComponentModel;
using System.Runtime.Serialization;

namespace RainbowTaskbar.Configuration;

[DataContract]
public class InstructionPreset {
    [DataMember] public string Name { get; set; }

    [DataMember] public BindingList<Instruction> Instructions { get; set; }
}