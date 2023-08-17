using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Windows;

namespace RainbowTaskbar.Helpers;

internal static class AutoUpdate {
    public static void CheckForUpdate() =>
        Task.Run(async () => {
            using var http = new HttpClient();
            using var web = new WebClient();
            http.DefaultRequestHeaders.Add("User-Agent", "RainbowTaskbar");
            var content =
                await http.GetStreamAsync("https://api.github.com/repos/ad2017gd/RainbowTaskbar/releases/latest");

            var ser = new DataContractJsonSerializer(typeof(GitHubAPIResponse));
            var response = ser.ReadObject(content) as GitHubAPIResponse;
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
                                $"/C taskkill /f /im \"{oldfile}\" > nul && timeout /t 1 /nobreak > nul && move /y \"{newfile}\" \"{oldfile}\" > nul && start \"{oldfile}\"",
                            FileName = "cmd",
                            WindowStyle = ProcessWindowStyle.Hidden
                        }
                    );
                }
            }
        });

    [DataContract]
    public class Asset {
        [DataMember(Name = "name")] public string Name { get; set; }

        [DataMember(Name = "label")] public string Label { get; set; }

        [DataMember(Name = "browser_download_url")]
        public string BrowserDownloadUrl { get; set; }
    }

    [DataContract]
    public class GitHubAPIResponse {
        [DataMember(Name = "tag_name")] public string TagName { get; set; }

        [DataMember(Name = "name")] public string Name { get; set; }

        [DataMember(Name = "assets")] public List<Asset> Assets { get; set; }
    }
}