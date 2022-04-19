﻿using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using O9d.Json.Formatting;
using RainbowTaskbar.Configuration;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace RainbowTaskbar.API.WebSocket;

internal class WebSocketAPIServer : WebSocketBehavior {
    private static readonly JsonSerializerOptions JsonOptions = new() {
        Converters = { new TypeDiscriminatorConverter<WebSocketAPIEvent>(), new TypeDiscriminatorConverter<Instruction>() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    public static void SendToSubscribed(string msg) {
        foreach (var id in API.APISubscribed) API.http.WebSocketServices["/rnbws"].Sessions.SendTo(msg, id);
    }

    public static void SendToSubscribed(WebSocketAPIEvent @event) {
        SendToSubscribed(JsonSerializer.Serialize(@event, JsonOptions));
    }

    protected override void OnMessage(MessageEventArgs e) {
        base.OnMessage(e);

        // Command message format:
        // { command: CommandName, [...args] }

        if (e.IsText) {
            var match = Regex.Match(Encoding.UTF8.GetString(e.RawData), @"({.+})");
            var jsonData = match.Groups.Count > 1 ? match.Groups.Values.ElementAt(1).Value : null;

            try {
                var json = JsonNode.Parse(jsonData!);

                if (json?["command"] != null)
                    switch (json["command"].ToString().ToLower()) {
                        case "subscribe":
                            if (json["value"] is null || bool.Parse(json["value"].ToString()))
                                API.APISubscribed.Add(ID);
                            else
                                API.APISubscribed.Remove(ID);

                            break;
                    }
            }
            catch {
                // ignored
            }
        }
    }

    protected override void OnClose(CloseEventArgs e) => API.APISubscribed.Remove(ID);
}