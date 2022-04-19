using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using O9d.Json.Formatting;

namespace RainbowTaskbar.Helpers;

internal static class AutoUpdate {
    private static readonly JsonSerializerOptions SnakeCase = new() {
        PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy()
    };
    
    public static void CheckForUpdate() =>
        Task.Run(async () => {
            using var http = new HttpClient();
            using var web = new WebClient();
            http.DefaultRequestHeaders.Add("User-Agent", "RainbowTaskbar");
            var content =
                await http.GetStreamAsync("https://api.github.com/repos/ad2017gd/RainbowTaskbar/releases/latest");
            
            var response = JsonSerializer.Deserialize<GitHubAPIResponse>(content, SnakeCase);
            if (Assembly.GetExecutingAssembly().GetName().Version.CompareTo(Version.Parse(response.TagName)) < 0) {
                // Outdated version
                var res = MessageBox.Show("A new RainbowTaskbar update has released. Would you like to update?",
                    "RainbowTaskbar", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (res == MessageBoxResult.Yes) {
                    var asset = response.Assets.First(asset =>
                        Environment.Is64BitProcess ? asset.Name.Contains("x64") : !asset.Name.Contains("x64"));

                    var uri = new Uri(asset.BrowserDownloadUrl);
                    var oldfile = Process.GetCurrentProcess().MainModule.ModuleName;
                    var newfile = $"{Process.GetCurrentProcess().MainModule.ModuleName}_new.exe";
                    await web.DownloadFileTaskAsync(uri, newfile);

                    var proc = Process.Start(
                        new ProcessStartInfo {
                            Arguments =
                                $"/C taskkill /f /im {oldfile} > nul && timeout /t 1 /nobreak > nul && move /y {newfile} {oldfile} > nul && start {oldfile}",
                            FileName = "cmd",
                            WindowStyle = ProcessWindowStyle.Hidden
                        }
                    );
                }
            }
        });

    public class Asset {
        public string Name { get; set; }

        public string Label { get; set; }
        
        public string BrowserDownloadUrl { get; set; }
    }
    
    public class GitHubAPIResponse {
        public string TagName { get; set; }

        public string Name { get; set; }

        public List<Asset> Assets { get; set; }
    }
}