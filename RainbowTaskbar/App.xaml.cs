using Newtonsoft.Json.Linq;
using RainbowTaskbar.Configuration;
using RainbowTaskbar.WebSocketServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using WebSocketSharp.Server;
using PropertyChanged;
using System.Reflection;
using RainbowTaskbar.Helpers;

namespace RainbowTaskbar
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static WebSocketServer ws;
        public static Random rnd = new Random();
        public static HttpServer http;
        public static List<Taskbar> taskbars = new List<Taskbar>();
        public static List<string> APISubscribed = new List<string>();

        [OnChangedMethod(nameof(ReloadTaskbars))]
        public static Config Config { get; set; }

        public static void ReloadTaskbars()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                taskbars = taskbars.Select(taskbar => { taskbar.Close(); return new Taskbar(taskbar.Secondary); }).ToList();
                taskbars.ForEach(taskbar => taskbar.Show());
            });
        }

        public App()
        {
            
            Config = Config.FromFile();
            if(Config.CheckUpdate) AutoUpdate.CheckForUpdate();
            ConfigureServer();
            if (Helpers.TaskbarHelper.FindWindow("Shell_SecondaryTrayWnd", null) != (IntPtr)0)
            {
                var newWindow = new Taskbar(true);
                newWindow.Show();

                App.taskbars.Add(newWindow);
            }

        }

        public static void ConfigureServer()
        {
            if (http is not null)
            {
                http.Stop();
            }

            if (Config.APIPort > 0 && Config.APIPort < 65536) http = new HttpServer(Config.APIPort);
            else http = new HttpServer(9093);

            http.AddWebSocketService<WebSocketAPIServer>("/rnbws");
            http.OnGet += HTTPAPI.HTTPAPIServer.Get;
            http.OnPost += HTTPAPI.HTTPAPIServer.Post;
            http.OnOptions += HTTPAPI.HTTPAPIServer.Options;
            http.KeepClean = false;

            if (Config.IsAPIEnabled) http.Start();
            
        }

        
    }
}
