using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows;

using RainbowTaskbar.V2Legacy.Configuration;
using WebSocketSharp.Server;

namespace RainbowTaskbar.HTTPAPI;

public static class HTTPAPIServer {
    // TODO: config-specific AND get rid of newtonsoft-json
    public static void Get(object sender, HttpRequestEventArgs args) {
        /*var req = args.Request;
        var res = args.Response;

        var uri = new Uri("http://localhost" + req.RawUrl);
        var query = HttpUtility.ParseQueryString(uri.Query);

        res.ContentType = "application/json; charset=utf-8";
        res.AddHeader("Access-Control-Allow-Origin", "*");
        res.StatusCode = 200;

        try {
            switch (uri.AbsolutePath) {
                case "/config": {
                    var json = new JObject();
                    json.Add("data", App.Config.ToJSON());
                    json.Add("success", JToken.FromObject(true));

                    var data = Encoding.UTF8.GetBytes(json.ToString());
                    res.ContentLength64 = data.Length;
                    res.OutputStream.Write(data);
                    break;
                }

                case "/instruction": {
                    byte[] data;
                    int position;

                    if (!int.TryParse(query["position"], out position) || position < 0 ||
                        position >= App.Config.Instructions.Count) throw new Exception("Position out of bounds");

                    var json = new JObject();
                    json.Add("data", App.Config.Instructions[position].ToJSON());
                    json.Add("success", JToken.FromObject("true"));

                    data = Encoding.UTF8.GetBytes(json.ToString());
                    res.ContentLength64 = data.Length;
                    res.OutputStream.Write(data);
                    break;
                }

                case "/instructions": {
                    var json = new JObject();
                    json.Add("data", JArray.FromObject(App.Config.Instructions.Select(i => i.ToJSON())));
                    json.Add("success", JToken.FromObject("true"));

                    var data = Encoding.UTF8.GetBytes(json.ToString());
                    res.ContentLength64 = data.Length;
                    res.OutputStream.Write(data);
                    break;
                }

                default:
                    res.StatusCode = 404;
                    break;
            }
        }
        catch (Exception e) {
            res.StatusCode = 500;

            var data = Encoding.UTF8.GetBytes("{\"success\": \"false\", \"error\": \"" + e.Message.Replace("\"", "'") +
                                              "\"}");
            res.ContentLength64 = data.Length;
            res.OutputStream.Write(data);
        }
        res.Close();
        
        */
    }

    public static void Options(object sender, HttpRequestEventArgs args) {
        var res = args.Response;
        res.AddHeader("Access-Control-Allow-Origin", "*");
        res.AddHeader("Access-Control-Allow-Headers", "*");
        res.AddHeader("Allow", "GET, POST");
        res.StatusCode = 200;
        res.Close();
    }

    public static void Post(object sender, HttpRequestEventArgs args) {
        /*
        var http = (HttpServer) sender;
        var req = args.Request;
        var res = args.Response;

        var uri = new Uri("http://localhost" + req.RawUrl);
        var query = HttpUtility.ParseQueryString(uri.Query);

        var bodybuf = new byte[short.MaxValue];
        req.InputStream.Read(bodybuf, 0, short.MaxValue);


        res.ContentType = "application/json; charset=utf-8";
        res.AddHeader("Access-Control-Allow-Origin", "*");
        res.AddHeader("Access-Control-Allow-Headers", "*");
        res.StatusCode = 200;

        try {
            var body = JObject.Parse(Encoding.UTF8.GetString(bodybuf).TrimEnd('\0'));
            switch (uri.AbsolutePath) {
                case "/clearinstruction": {
                    byte[] data;
                    int position;

                    if (!int.TryParse((string) body["Position"], out position) || position < 0 ||
                        position >= App.Config.Instructions.Count) throw new Exception("Position out of bounds");

                    App.Config.Instructions.RemoveAt(position);

                    data = Encoding.UTF8.GetBytes("{\"success\": \"true\"}");
                    res.ContentLength64 = data.Length;
                    res.OutputStream.Write(data);
                    break;
                }
                case "/clearinstructions": {
                    App.Config.Instructions = new BindingList<Instruction>();

                    var data = Encoding.UTF8.GetBytes("{\"success\": \"true\"}");
                    res.ContentLength64 = data.Length;
                    res.OutputStream.Write(data);
                    break;
                }
                case "/addinstruction": {
                    var position = 0;

                    if (body["Position"] is not null && (!int.TryParse((string) body["Position"], out position) ||
                                                         position < 0 || position >= App.Config.Instructions.Count)) {
                        throw new Exception("Position out of bounds");
                    }

                    if (Type.GetType("RainbowTaskbar.Configuration.Instructions." + body["Name"]) is null)
                        throw new Exception("Unknown instruction class name.");

                    var instruction =
                        Instruction.FromJSON(Type.GetType("RainbowTaskbar.Configuration.Instructions." + body["Name"]),
                            body);

                    Application.Current.Dispatcher.Invoke(() =>
                        App.Config.Instructions.Insert(position, instruction)
                    );
                    break;
                }
                case "/executeinstruction": {
                    byte[] data;

                    if (Type.GetType("RainbowTaskbar.Configuration.Instructions." + body["Name"]) is null)
                        throw new Exception("Unknown instruction class name.");

                    var instruction =
                        Instruction.FromJSON(Type.GetType("RainbowTaskbar.Configuration.Instructions." + body["Name"]),
                            body);

                    App.taskbars.ForEach(taskbar => instruction.Execute(taskbar));

                    data = Encoding.UTF8.GetBytes("{\"success\": \"true\"}");
                    res.ContentLength64 = data.Length;
                    res.OutputStream.Write(data);
                    break;
                }
                case "/goto": {
                    byte[] data;

                    var position = 0;

                    if (body["Position"] is not null && (!int.TryParse((string) body["Position"], out position) ||
                                                         position < 0 ||
                                                         position >= App.Config.Instructions.Count))
                        throw new Exception("Position out of bounds");

                    App.taskbars.ForEach(taskbar => App.Config.configStep = position);

                    data = Encoding.UTF8.GetBytes("{\"success\": \"true\"}");
                    res.ContentLength64 = data.Length;
                    res.OutputStream.Write(data);
                    break;
                }
                case "/saveconfig": {
                    App.Config.ToFile();

                    var data = Encoding.UTF8.GetBytes("{\"success\": \"true\"}");
                    res.ContentLength64 = data.Length;
                    res.OutputStream.Write(data);
                    break;
                }
                case "/loadconfig": {
                    App.Config = Config.FromFile();

                    var data = Encoding.UTF8.GetBytes("{\"success\": \"true\"}");
                    res.ContentLength64 = data.Length;
                    res.OutputStream.Write(data);
                    break;
                }
            }
        }

        catch (Exception e) {
            res.StatusCode = 500;

            var data = Encoding.UTF8.GetBytes("{\"success\": \"false\", \"error\": \"" + e.Message.Replace("\"", "'") +
                                              "\"}");
            res.ContentLength64 = data.Length;
            res.OutputStream.Write(data);
        }
        */
    }
}