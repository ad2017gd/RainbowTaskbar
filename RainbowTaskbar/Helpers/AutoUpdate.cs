using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using RainbowTaskbar.Configuration;

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
                var res = MessageBox.Show(App.localization.Get("msgbox_update"),
                    "RainbowTaskbar", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (res == MessageBoxResult.Yes) {
                    var asset = response.Assets.First(asset => asset.Name.StartsWith($"rnbtsk-{System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString().ToLower()}"));

                    var uri = new Uri(asset.BrowserDownloadUrl);
                    var oldfile = Environment.ProcessPath;
                    var newfile = $"{Environment.ProcessPath}_new.exe";

                    var programfilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                    var verb = "";
                    var programfiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    if (Environment.ProcessPath.StartsWith(programfilesX86, StringComparison.OrdinalIgnoreCase) || Environment.ProcessPath.StartsWith(programfiles, StringComparison.OrdinalIgnoreCase)) {
                        verb = "runas";
                        newfile = Environment.ExpandEnvironmentVariables("%temp%\\rnbtsk_newver_.exe");
                    }

                    await web.DownloadFileTaskAsync(uri, newfile);




                    var data = false;
                    var proc = Process.Start(
                        new ProcessStartInfo {
                            UseShellExecute = verb == "runas",
                            Verb = verb,
                            Arguments =
                                $"/C echo p && timeout /t 1 /nobreak > nul && taskkill /f /im \"{Process.GetCurrentProcess().MainModule.ModuleName}\" > nul && timeout /t 1 /nobreak > nul && move /y \"{newfile}\" \"{oldfile}\" > nul",
                            FileName = "cmd",
                            WindowStyle = ProcessWindowStyle.Hidden
                        }
                    );
                    var procStart = Process.Start(
                        new ProcessStartInfo {
                            Arguments =
                                $"/C timeout /t 10 /nobreak > nul && start \"\" \"{oldfile}\"",
                            FileName = "cmd",
                            WindowStyle = ProcessWindowStyle.Hidden
                        }
                    );
                }
            }
        });
    public static async Task<string> GetLatestBody() {
        using var http = new HttpClient();
        using var web = new WebClient();
        http.DefaultRequestHeaders.Add("User-Agent", "RainbowTaskbar");
        var content =
            await http.GetStreamAsync("https://api.github.com/repos/ad2017gd/RainbowTaskbar/releases/latest");

        var ser = new DataContractJsonSerializer(typeof(GitHubAPIResponse));
        var response = ser.ReadObject(content) as GitHubAPIResponse;
        return response.Body;
    }
        

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

        [DataMember(Name = "body")] public string Body { get; set; }

        [DataMember(Name = "assets")] public List<Asset> Assets { get; set; }
    }
}