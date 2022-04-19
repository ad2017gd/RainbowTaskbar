using System.Runtime.Serialization;
using RainbowTaskbar.Configuration;

namespace RainbowTaskbar.API.WebSocket.Events;

[DataContract]
public class InstructionExecutedEvent : WebSocketAPIEvent {
    public InstructionExecutedEvent(Instruction instruction, int index) : base("InstructionExecuted") {
        Instruction = instruction;
        Index = index;
    }

     public int Index { get; }

     public Instruction Instruction { get; }
}