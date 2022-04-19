using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Web;
using System.Windows;
using RainbowTaskbar.API.HTTP.Responses;
using RainbowTaskbar.Configuration;
using WebSocketSharp.Server;

namespace RainbowTaskbar.API.HTTP;

public static class HTTPAPIServer {
    private static Instruction instruction;

    public static void Get(object sender, HttpRequestEventArgs args) {
        var req = args.Request;
        var res = args.Response;

        var uri = new Uri("http://localhost" + req.RawUrl);
        var query = HttpUtility.ParseQueryString(uri.Query);

        res.ContentType = "application/json; charset=utf-8";
        res.AddHeader("Access-Control-Allow-Origin", "*");
        res.StatusCode = 200;

        HTTPAPIResponse response;
        try {
            switch (uri.AbsolutePath) {
                case "/config": {
                    response = new ConfigResponse(App.Config);
                    break;
                }

                case "/instruction": {
                    if (!int.TryParse(query["position"], out var position) || position < 0 ||
                        position >= App.Config.Instructions.Count) {
                        response = new ErrorResponse("Position invalid or out of bounds");
                        break;
                    }

                    response = new InstructionResponse(App.Config.Instructions[position]);
                    break;
                }

                case "/instructions": {
                    response = new InstructionsResponse(App.Config.Instructions.ToList());
                    break;
                }

                default:
                    response = new ErrorResponse("Unknown route");
                    break;
            }
        }
        catch (Exception e) {
            response = new ErrorResponse(e.Message.Replace("\"", "'"));
        }

        var ser = new DataContractJsonSerializer(typeof(HTTPAPIResponse));
        ser.WriteObject(res.OutputStream, response);
        res.Close();
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
        var http = (HttpServer) sender;
        var req = args.Request;
        var res = args.Response;

        var uri = new Uri("http://localhost" + req.RawUrl);
        var query = HttpUtility.ParseQueryString(uri.Query);

        var bodybuf = new byte[short.MaxValue];
        req.InputStream.Read(bodybuf, 0, short.MaxValue);
        req.InputStream.Seek(0, SeekOrigin.Begin);


        res.ContentType = "application/json; charset=utf-8";
        res.AddHeader("Access-Control-Allow-Origin", "*");
        res.AddHeader("Access-Control-Allow-Headers", "*");
        res.StatusCode = 200;

        HTTPAPIResponse response;
        try {
            var body = JsonNode.Parse(Encoding.UTF8.GetString(bodybuf).TrimEnd('\0'));
            switch (uri.AbsolutePath) {
                case "/clearinstruction": {
                    if (!int.TryParse((string) body["Position"], out var position) || position < 0 ||
                        position >= App.Config.Instructions.Count) {
                        response = new ErrorResponse("Position invalid or out of bounds");
                        break;
                    }

                    App.Config.Instructions.RemoveAt(position);

                    response = new SuccessResponse();
                    break;
                }
                case "/clearinstructions": {
                    App.Config.Instructions = new BindingList<Instruction>();

                    response = new SuccessResponse();
                    break;
                }
                case "/addinstruction": {
                    if (!int.TryParse((string) body["Position"], out var position) ||
                        position < 0 || position >= App.Config.Instructions.Count) {
                        response = new ErrorResponse("Position invalid or out of bounds");
                        break;
                    }

                    var instructionType =
                        Instruction.DisplayableInstructionTypes.FirstOrDefault(type =>
                            type.Name == body["Name"].ToString());

                    if (instructionType is null) {
                        response = new ErrorResponse("Unknown instruction class name");
                        break;
                    }

                    instruction =
                        new DataContractJsonSerializer(instructionType).ReadObject(req.InputStream) as Instruction;
                    Application.Current.Dispatcher.Invoke(() =>
                        App.Config.Instructions.Insert(position, instruction)
                    );

                    response = new SuccessResponse();
                    break;
                }
                case "/executeinstruction": {
                    var instructionType =
                        Instruction.DisplayableInstructionTypes.FirstOrDefault(type =>
                            type.Name == body["Name"].ToString());

                    if (instructionType is null) {
                        response = new ErrorResponse("Unknown instruction class name");
                        break;
                    }

                    instruction =
                        new DataContractJsonSerializer(instructionType).ReadObject(req.InputStream) as Instruction;
                    App.taskbars.ForEach(taskbar => { instruction.Execute(taskbar); });

                    response = new SuccessResponse();
                    break;
                }
                case "/goto": {
                    var position = 0;

                    if (body["Position"] is not null && (!int.TryParse((string) body["Position"], out position) ||
                                                         position < 0 ||
                                                         position >= App.Config.Instructions.Count)) {
                        response = new ErrorResponse("Position invalid or out of bounds");
                        break;
                    }

                    App.taskbars.ForEach(taskbar => taskbar.viewModel.ConfigStep = position);

                    response = new SuccessResponse();
                    break;
                }
                case "/saveconfig": {
                    App.Config.ToFile();

                    response = new SuccessResponse();
                    break;
                }
                case "/loadconfig": {
                    App.Config = Config.FromFile();

                    response = new SuccessResponse();
                    break;
                }

                default: {
                    response = new ErrorResponse("Unknown route");
                    break;
                }
            }
        }
        catch (Exception e) {
            res.StatusCode = 500;
            response = new ErrorResponse(e.Message.Replace("\"", "'"));
        }

        var ser = new DataContractJsonSerializer(typeof(HTTPAPIResponse));
        ser.WriteObject(res.OutputStream, response);
    }
}