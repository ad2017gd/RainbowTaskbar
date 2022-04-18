using System.Runtime.Serialization;
using RainbowTaskbar.Configuration;

namespace RainbowTaskbar.API.HTTP.Responses;

[DataContract]
public class InstructionResponse : HTTPAPIResponse {
    public InstructionResponse(Instruction instruction) : base(true) {
        Data = instruction;
    }

    public override Instruction Data { get; }
}