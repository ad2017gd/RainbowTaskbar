using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using RainbowTaskbar.HTTPAPI;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace RainbowTaskbar.WebSocketServices;

internal class WebSocketAPIServer : WebSocketBehavior {
    public static void SendToSubscribed(string msg) {
        foreach (var id in API.APISubscribed) API.http.WebSocketServices["/rnbws"].Sessions.SendTo(msg, id);
    }

    protected override void OnMessage(MessageEventArgs e) {
        base.OnMessage(e);

        // Command message format:
        // { command: CommandName, [...args] }

        if (e.IsText) {
            var match = Regex.Match(Encoding.UTF8.GetString(e.RawData), @"({.+})");
            var JSONData = match.Groups.Count > 1 ? match.Groups.Values.ElementAt(1).Value : null;

            dynamic parsed = null;
            try {
                parsed = JObject.Parse(JSONData);
            }
            catch {
                return;
            }

            string commandName = parsed.command;

            if (commandName is not null)
                switch (commandName.ToLower()) {
                    case "subscribe":
                        if (parsed.value is null || (bool) parsed.value)
                            API.APISubscribed.Add(ID);
                        else
                            API.APISubscribed.Remove(ID);

                        break;
                }
        }
    }

    protected override void OnClose(CloseEventArgs e) => API.APISubscribed.Remove(ID);
}