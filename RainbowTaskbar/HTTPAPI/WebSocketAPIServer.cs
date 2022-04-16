using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace RainbowTaskbar.WebSocketServices;

internal class WebSocketAPIServer : WebSocketBehavior {
    public static void SendToSubscribed(string msg) {
        foreach (var ID in App.APISubscribed) App.http.WebSocketServices["/rnbws"].Sessions.SendTo(msg, ID);
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

            string CommandName = parsed.command;

            if (CommandName is not null)
                switch (CommandName.ToLower()) {
                    case "subscribe":
                        if (parsed.value is null || (bool) parsed.value)
                            App.APISubscribed.Add(ID);
                        else
                            App.APISubscribed.Remove(ID);

                        break;
                }
        }
    }
}