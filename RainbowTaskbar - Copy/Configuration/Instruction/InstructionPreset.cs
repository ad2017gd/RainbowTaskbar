using System.ComponentModel;
using System.Runtime.Serialization;

namespace RainbowTaskbar.Configuration.Instruction;


public class InstructionPreset {
    public string Name { get; set; }
    public InstructionGroup RunOnceGroup { get; set; } = new InstructionGroup();
    public BindingList<InstructionGroup> LoopGroups { get; set; } = new BindingList<InstructionGroup>();
}