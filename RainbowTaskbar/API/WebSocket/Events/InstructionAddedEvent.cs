using System.Runtime.Serialization;
using RainbowTaskbar.Configuration;

namespace RainbowTaskbar.API.WebSocket.Events;

[DataContract]
public class InstructionAddedEvent : WebSocketAPIEvent {
    public InstructionAddedEvent(Instruction instruction, int index) : base("InstructionAdded") {
        Instruction = instruction;
        Index = index;
    }

    [DataMember(Name = "index")] public int Index { get; }

    [DataMember(Name = "instruction")] public Instruction Instruction { get; }
}