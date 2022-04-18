using System.Runtime.Serialization;

namespace RainbowTaskbar.API.HTTP.Responses;

[DataContract]
public class ErrorResponse : HTTPAPIResponse {
    public ErrorResponse(string error) : base(false) {
        Data = error;
    }

    public override string Data { get; }
}