using System.Runtime.Serialization;
using RainbowTaskbar.Configuration;

namespace RainbowTaskbar.API.WebSocket.Events;

[DataContract]
public class InstructionExecutedEvent : WebSocketAPIEvent {
    public InstructionExecutedEvent(Instruction instruction, int index) : base("InstructionExecuted") {
        Instruction = instruction;
        Index = index;
    }

    [DataMember(Name = "index")] public int Index { get; }

    [DataMember(Name = "instruction")] public Instruction Instruction { get; }
}