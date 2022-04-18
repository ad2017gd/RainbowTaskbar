using System.Collections.Generic;
using System.Runtime.Serialization;
using RainbowTaskbar.Configuration;

namespace RainbowTaskbar.API.HTTP.Responses;

[DataContract]
public class InstructionsResponse : HTTPAPIResponse {
    public InstructionsResponse(List<Instruction> instructions) : base(true) {
        Data = instructions;
    }

    public override List<Instruction> Data { get; }
}