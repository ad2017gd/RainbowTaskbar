using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace RainbowTaskbar.API.WebSocket;

[DataContract]
public abstract class WebSocketAPIEvent {
    protected WebSocketAPIEvent(string name) {
        Name = name;
    }

     public string Name { get; }

    public static IEnumerable<Type> GetKnownEventTypes() {
        return Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(WebSocketAPIEvent).IsAssignableFrom(type)).ToList();
    }
}