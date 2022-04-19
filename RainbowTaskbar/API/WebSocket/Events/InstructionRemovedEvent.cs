using System.Runtime.Serialization;
using RainbowTaskbar.Configuration;

namespace RainbowTaskbar.API.WebSocket.Events;

[DataContract]
public class InstructionRemovedEvent : WebSocketAPIEvent {
    public InstructionRemovedEvent(Instruction instruction, int index) : base("InstructionRemoved") {
        Instruction = instruction;
        Index = index;
    }

     public int Index { get; }

     public Instruction Instruction { get; }
}