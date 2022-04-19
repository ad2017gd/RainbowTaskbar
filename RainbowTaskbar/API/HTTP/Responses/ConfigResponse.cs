using System.Runtime.Serialization;
using RainbowTaskbar.Configuration;

namespace RainbowTaskbar.API.HTTP.Responses;

[DataContract]
public class ConfigResponse : HTTPAPIResponse {
    public ConfigResponse(Config config) : base(true) {
        Data = config;
    }

    public override Config Data { get; }
}