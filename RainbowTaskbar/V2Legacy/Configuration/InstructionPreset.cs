using System.ComponentModel;
using System.Runtime.Serialization;

namespace RainbowTaskbar.V2Legacy.Configuration;

[DataContract]
public class InstructionPreset {
    [field: DataMember] public string Name { get; set; }
    [field: DataMember] public BindingList<Instruction> Instructions { get; set; }
}