using System.Collections.Generic;
using RainbowTaskbar.WebSocketServices;
using WebSocketSharp.Server;

namespace RainbowTaskbar.HTTPAPI;

public static class API {
    public static List<string> APISubscribed = new();
    public static HttpServer http;

    public static void Start() {
        Stop();

        http = App.Config.APIPort is > 0 and < 65536 ? new HttpServer(App.Config.APIPort) : new HttpServer(9093);

        http.AddWebSocketService<WebSocketAPIServer>("/rnbws");
        http.OnGet += HTTPAPIServer.Get;
        http.OnPost += HTTPAPIServer.Post;
        http.OnOptions += HTTPAPIServer.Options;
        http.KeepClean = false;

        if (App.Config.IsAPIEnabled) http.Start();
    }

    public static void Stop() => http?.Stop();
}