using System.Runtime.Serialization;

namespace RainbowTaskbar.API.HTTP.Responses;

[DataContract]
public class SuccessResponse : HTTPAPIResponse {
    public SuccessResponse() : base(true) { }

    public override object Data {
        get => null;
    }
}