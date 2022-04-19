using System.Runtime.Serialization;
using RainbowTaskbar.Configuration;

namespace RainbowTaskbar.API.WebSocket.Events;

[DataContract]
public class InstructionRemovedEvent : WebSocketAPIEvent {
    public InstructionRemovedEvent(Instruction instruction, int index) : base("InstructionRemoved") {
        Instruction = instruction;
        Index = index;
    }

    [DataMember(Name = "index")] public int Index { get; }

    [DataMember(Name = "instruction")] public Instruction Instruction { get; }
}