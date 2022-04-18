using System.Runtime.Serialization;

namespace RainbowTaskbar.API.WebSocket;

[DataContract]
public abstract class WebSocketAPIEvent {
    protected WebSocketAPIEvent(string name) {
        Name = name;
    }

    [DataMember(Name = "name")] public string Name { get; }
}