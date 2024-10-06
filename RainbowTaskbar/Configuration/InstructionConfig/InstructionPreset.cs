using System.ComponentModel;
using System.Runtime.Serialization;

namespace RainbowTaskbar.Configuration.InstructionConfig;


public class InstructionPreset {
    public string Name { get; set; }
    public InstructionGroup RunOnceGroup { get; set; }
    public BindingList<InstructionGroup> LoopGroups { get; set; }
}