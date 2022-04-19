using System.Runtime.Serialization;
using RainbowTaskbar.Configuration;

namespace RainbowTaskbar.API.WebSocket.Events;

[DataContract]
public class InstructionAddedEvent : WebSocketAPIEvent {
    public InstructionAddedEvent(Instruction instruction, int index) : base("InstructionAdded") {
        Instruction = instruction;
        Index = index;
    }

     public int Index { get; }

     public Instruction Instruction { get; }
}