using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using RainbowTaskbar.Configuration;
using System.Net.Http.Json;
using RainbowTaskbar.Configuration.Web;
using System.IO;
using System.Text.Json;
using System.Dynamic;
using RainbowTaskbar.Configuration.Instruction;
using RainbowTaskbar.Editor.Pages;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RainbowTaskbar.HTTPAPI {
    public class AuthenticatedRequest {
        [JsonPropertyName("key")]
        public string LoginKey { get; set; }
    }
    public class ResultResponse {
        [JsonPropertyName("result")]
        public bool Result { get; set; }
    }
    public class PublishConfigRequest : AuthenticatedRequest {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("configData")]
        public string Data { get; set; }
        [JsonPropertyName("id")]
        public string? PublishedID { get; set; }
        [JsonPropertyName("prevId")]
        public string? PreviousPublishedId { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
    public class ConfigData {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("data")]
        public string Data { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("id")]
        public string PublishedID { get; set; }
        [JsonPropertyName("previousId")]
        public string PreviousPublishedID { get; set; }
        [JsonPropertyName("datePublished")]
        public long Published { get; set; }
        [JsonPropertyName("dateUpdated")]
        public long Updated { get; set; }
        [JsonPropertyName("author")]
        public string AuthorUsername { get; set; }
        [JsonPropertyName("likes")]
        public int LikeCount { get; set; }
        [JsonPropertyName("comments")]
        public int CommentCount { get; set; }
    }
    public class PublishConfigResponse : ResultResponse {
        [JsonPropertyName("config")]
        public ConfigData Data { get; set; }
        
    }

    public class CommentData {
        [JsonPropertyName("author")]
        public string AuthorUsername { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; }
        [JsonPropertyName("datePublished")]
        public long Published { get; set; }
        [JsonPropertyName("id")]
        public string ID { get; set; }

        [JsonIgnore]
        public bool OwnComment { get => AuthorUsername == App.Settings.AccountUsername || App.Settings.AccountUsername == "ad2017gd"; }
    }

    public class ConfigCommentsResponse : ResultResponse {
        [JsonPropertyName("comments")]
        public List<CommentData> Comments { get; set; }
    }
    public class ConfigCommentRequest : AuthenticatedRequest {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
    public class ConfigCommentResponse : ResultResponse {
        [JsonPropertyName("id")]
        public string ID { get; set; }
    }

    public class SearchConfigRequest {
        [JsonPropertyName("search")]
        public string Search { get; set; }
        [JsonPropertyName("page")]
        public int Page { get; set; }
        [JsonPropertyName("sort")]
        public int Sort { get; set; }

    }

    public class LikedConfigsResponse {
        [JsonPropertyName("search")]
        public List<string> Search { get; set; }

    }
    public class SearchConfigResponse : ResultResponse {
        [JsonPropertyName("search")]
        public List<ConfigData> Results { get; set; }

        public IEnumerable<Config?> Parse() {
            return Results.Select(x => {
                try {
                    Config cfg = x.Type == "web" ? new WebConfig() : new InstructionConfig();
                    cfg.InitNew();
                    cfg.Name = x.Title;
                    cfg.Description = x.Description;
                    cfg.Updated = new DateTime(1970, 1, 1).AddMilliseconds(x.Updated);
                    cfg.PublishedID = x.PublishedID;
                    cfg.PreviousPublishedID = x.PreviousPublishedID;
                    cfg.CachedPublisherUsername = x.AuthorUsername;
                    cfg.ConfigData = JsonSerializer.Deserialize<Configuration.ConfigData>(x.Data, Config.SerializerOptions);
                    cfg.CachedLikeCount = x.LikeCount;
                    cfg.CachedCommentCount = x.CommentCount;

                    return cfg;
                } catch {
                    return null;
                }
            }).Where(x=>x is not null);
        }
    }
    public class ThumbnailConfigRequest : AuthenticatedRequest {
        [JsonPropertyName("thumbnail")]
        public string Image { get; set; }

    }

    public class IssueRequest {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; }
        [JsonPropertyName("contact")]
        public string Contact { get; set; }

    }
    public class ExceptionRequest {
        [JsonPropertyName("message")]
        public string Message { get; set; }

    }
    public class WorkshopAPI {
        public string LoginKey { get; set; }
        public async Task<PublishConfigResponse?> PublishConfigAsync(Config cfg) {
            try {
                using var http = new HttpClient();

                var content = await http.PostAsJsonAsync("https://rnbsrv.ad2017.dev/user/me/publish", new PublishConfigRequest { 
                    LoginKey = LoginKey,
                    Title = cfg.Name,
                    Data = JsonSerializer.Serialize(cfg.ConfigData, Config.SerializerOptions),
                    PublishedID = cfg.PublishedID,
                    PreviousPublishedId = cfg.PreviousPublishedID,
                    Description = cfg.Description,
                    Type = cfg is WebConfig ? "web" : "instruction"
                });
                var result = await content.Content.ReadFromJsonAsync<PublishConfigResponse>();

                return result;
            } catch (Exception e) {
                return null;
            }
        }
        public async Task<SearchConfigResponse?> SearchConfigsAsync(string search, SortBy sort, int page = 0) {
            try {
                using var http = new HttpClient();

                var content = await http.PostAsJsonAsync("https://rnbsrv.ad2017.dev/config", new SearchConfigRequest {
                    Sort = (int)sort,
                    Search = search,
                    Page = page
                });
                var result = await content.Content.ReadFromJsonAsync<SearchConfigResponse>();

                return result;
            }
            catch (Exception e) {
                return null;
            }
        }
        public async Task<ResultResponse?> DeleteConfigAsync(Config config) {
            try {
                using var http = new HttpClient();

                var content = await http.PostAsJsonAsync($"https://rnbsrv.ad2017.dev/user/me/config/{config.PublishedID}/delete", new AuthenticatedRequest {
                    LoginKey = LoginKey
                });
                var result = await content.Content.ReadFromJsonAsync<ResultResponse>();

                return result;
            }
            catch (Exception e) {
                return null;
            }
        }

        public async Task<string?> DownloadThumbnailBase64(Config config, bool unverified = false) {
            try {
                using var http = new HttpClient();

                var res = await http.PostAsJsonAsync($"https://rnbsrv.ad2017.dev/{(unverified ? "u/" : "")}img/{config.PublishedID}.jpg", new AuthenticatedRequest {
                    LoginKey = LoginKey
                });
                if (res.StatusCode != HttpStatusCode.OK) return null;
                var bytes = await res.Content.ReadAsByteArrayAsync();

                return Convert.ToBase64String(bytes);
            } catch {
                return null;
            }
        }

        public async Task<ResultResponse> VerifyThumbnail(Config config) {
            try {
                using var http = new HttpClient();

                var content = await http.PostAsJsonAsync($"https://rnbsrv.ad2017.dev/thumb/{config.PublishedID}/verify", new AuthenticatedRequest {
                    LoginKey = LoginKey
                });
                var result = await content.Content.ReadFromJsonAsync<ResultResponse>();

                return result;
            }
            catch {
                return null;
            }
        }

        public async Task<LikedConfigsResponse> GetLikedConfigs() {
            try {
                using var http = new HttpClient();

                var content = await http.PostAsJsonAsync($"https://rnbsrv.ad2017.dev/user/me/liked", new AuthenticatedRequest {
                    LoginKey = LoginKey
                });
                var result = await content.Content.ReadFromJsonAsync<LikedConfigsResponse>();

                return result;
            }
            catch {
                return null;
            }
        }

        public async Task<ConfigCommentsResponse> GetConfigComments(Config config) {
            try {
                using var http = new HttpClient();

                var content = await http.PostAsJsonAsync($"https://rnbsrv.ad2017.dev/config/{config.PublishedID}/comments", new AuthenticatedRequest {
                    LoginKey = LoginKey
                });
                var result = await content.Content.ReadFromJsonAsync<ConfigCommentsResponse>();

                return result;
            }
            catch {
                return null;
            }
        }
        public async Task<ConfigCommentResponse> AddConfigComment(Config config, string comment) {
            try {
                using var http = new HttpClient();

                var content = await http.PostAsJsonAsync($"https://rnbsrv.ad2017.dev/config/{config.PublishedID}/comment", new ConfigCommentRequest {
                    Content = comment,
                    LoginKey = LoginKey
                });
                var result = await content.Content.ReadFromJsonAsync<ConfigCommentResponse>();

                return result;
            }
            catch {
                return null;
            }
        }

        public async Task<ResultResponse> DeleteConfigComment(Config config, string ID) {
            try {
                using var http = new HttpClient();

                var content = await http.PostAsJsonAsync($"https://rnbsrv.ad2017.dev/config/{config.PublishedID}/comment/{ID}/delete", new AuthenticatedRequest {
                    LoginKey = LoginKey
                });
                var result = await content.Content.ReadFromJsonAsync<ResultResponse>();

                return result;
            }
            catch {
                return null;
            }
        }

        public async Task<ResultResponse> LikeConfig(Config cfg, bool like = true) {
            try {
                using var http = new HttpClient();

                var content = await http.PostAsJsonAsync($"https://rnbsrv.ad2017.dev/config/{cfg.PublishedID}/{(like ? "" : "un")}like", new AuthenticatedRequest {
                    LoginKey = LoginKey
                });
                var result = await content.Content.ReadFromJsonAsync<ResultResponse>();

                return result;
            }
            catch {
                return null;
            }
        }

        public async Task<ResultResponse> SubmitIssue(string title, string desc, string contact) {
            try {
                using var http = new HttpClient();

                var content = await http.PostAsJsonAsync($"https://rnbsrv.ad2017.dev/issue", new IssueRequest {
                    Title = title,
                    Contact = contact == "" || contact == string.Empty ? "Not provided" : contact,
                    Content = desc
                });
                var result = await content.Content.ReadFromJsonAsync<ResultResponse>();

                return result;
            }
            catch {
                return null;
            }
        }
        public async Task<ResultResponse> ReportException(Exception e) {
            try {
                using var http = new HttpClient();

                var content = await http.PostAsJsonAsync($"https://rnbsrv.ad2017.dev/exception", new ExceptionRequest {
                    Message = e.Message + "\n" + e.StackTrace,
                });
                var result = await content.Content.ReadFromJsonAsync<ResultResponse>();

                return result;
            }
            catch {
                return null;
            }
        }


        public async Task<ResultResponse?> SetConfigThumbnail(Config config, Image img) {
            try {
                using var http = new HttpClient();

                string base64img = "";

                img.Dispatcher.Invoke(() => {
                    using (MemoryStream m = new MemoryStream()) {
                        var bitmap = new TransformedBitmap((BitmapSource) img.Source,
                            new ScaleTransform(
                            720.0 / img.Source.Height,
                            720.0 / img.Source.Height));

                        var bmpEncoder = new JpegBitmapEncoder();
                        bmpEncoder.Frames.Add(BitmapFrame.Create(bitmap));
                        bmpEncoder.Save(m);

                        byte[] imageBytes = m.ToArray();

                        base64img = Convert.ToBase64String(imageBytes);
                    }
                });
                

                var content = await http.PostAsJsonAsync($"https://rnbsrv.ad2017.dev/user/me/config/{config.PublishedID}/thumbnail", new ThumbnailConfigRequest {
                    LoginKey = LoginKey,
                    Image = base64img
                });
                var result = await content.Content.ReadFromJsonAsync<ResultResponse>();

                return result;
            }
            catch (Exception e) {
                return null;
            }
        }
    }
}
