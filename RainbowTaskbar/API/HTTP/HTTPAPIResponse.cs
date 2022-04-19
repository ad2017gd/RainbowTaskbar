using System.Runtime.Serialization;

namespace RainbowTaskbar.API.HTTP;

[DataContract]
public abstract class HTTPAPIResponse {
    protected HTTPAPIResponse(bool success) {
        Success = success;
    }

     public bool Success { get; }

     public abstract object Data { get; }
}