using System.Runtime.Serialization;

namespace RainbowTaskbar.API.HTTP;

[DataContract]
public abstract class HTTPAPIResponse {
    protected HTTPAPIResponse(bool success) {
        Success = success;
    }

    [DataMember(Name = "success")] public bool Success { get; }

    [DataMember(Name = "data")] public abstract object Data { get; }
}