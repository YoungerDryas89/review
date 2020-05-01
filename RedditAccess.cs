using Scarlett.RedditAssets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RedditAccess
{
    public class Reddit
    {
        private String AuthToken => $"{RedditAppData.ClientId}:{RedditAppData.Secret}";
        public String Username => RedditUser.Username;
        public String UserAgent => RedditAppData.UserAgent;

        private User RedditUser;
        private AppData RedditAppData;
        private Dictionary<String, String> GrantParameters;
        private TokenData tokendata;

        public Reddit(User user, AppData appdata)
        {
            this.RedditUser = user;
            this.RedditAppData = appdata;
            this.GrantParameters = new Dictionary<String, String> { { "grant_type", "password" }, { "username", user.Username }, { "password", user.Password } };
        }

        private void ExtractTokenData(String json)
        {
            try
            {
                tokendata = JsonSerializer.Deserialize<TokenData>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public void AuthorizeWithReddit()
        {
            async_authorize_with_reddit().GetAwaiter().GetResult();
        }

        private async Task async_authorize_with_reddit()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var encoded_token = Encoding.ASCII.GetBytes(AuthToken);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(encoded_token));
                    client.DefaultRequestHeaders.UserAgent.TryParseAdd(RedditAppData.UserAgent);
                    String URL = "https://www.reddit.com/api/v1/access_token";
                    var gparams = new FormUrlEncodedContent(GrantParameters);

                    var result = await client.PostAsync(URL, gparams);
                    var content = await result.Content.ReadAsStringAsync();

                    ExtractTokenData(content);

                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    throw;
                }
            }
        }
        public RedditListing GetSaved(Dictionary<String, String>? args = null)
        {
            return AsyncGetSaved(args).GetAwaiter().GetResult();
        }

        public async Task<RedditListing> AsyncGetSaved(Dictionary<String, String>? args = null)
        {
            if(args == null)
            {
                args = new Dictionary<string, string>
                {
                    {"limit","0" },
                    {"context","1000" }
                };
            }
            using (HttpClient t = new HttpClient())
            {
                try
                {
                    t.BaseAddress = new Uri("https://oauth.reddit.com/");
                    t.DefaultRequestHeaders.UserAgent.TryParseAdd(UserAgent);
                    t.DefaultRequestHeaders.Add("Authorization", $"{tokendata.token_type} {tokendata.access_token}");
                    var req = new HttpRequestMessage(HttpMethod.Get, $"/user/{Username}/saved");
                    req.Content = new FormUrlEncodedContent(args);

                    var result = await t.SendAsync(req);
                    var content = await result.Content.ReadAsStringAsync();
                    return ExtractRedditForumPosts(content);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    throw;
                }
            }
        }

        private RedditListing ExtractRedditForumPosts(string content)
        {
            RedditListing saved = new RedditListing();
            using (JsonDocument doc = JsonDocument.Parse(content))
            {
                var data_exists = doc.RootElement.TryGetProperty("data", out JsonElement d);
                var error = doc.RootElement.TryGetProperty("error", out JsonElement e);
                if (data_exists)
                {
                    saved.before = d.GetProperty("before").ToString();
                    saved.after = d.GetProperty("after").ToString();
                    foreach (var js in d.GetProperty("children").EnumerateArray())
                    {
                        RedditObject entity = new RedditObject();
                        entity.Kind = js.GetProperty("kind").ToString();
                        switch (entity.Kind)
                        {
                            case "t3":
                                if (js.TryGetProperty("IsSelf", out JsonElement s))
                                {
                                    entity.IsSelf = s.GetBoolean();
                                    entity.sp = DeserializeRedditObject<SelfPost>(js);
                                }
                                else
                                {
                                    entity.lnk = DeserializeRedditObject<LinkPost>(js);
                                }
                                break;

                            case "t1":
                                continue;

                            default:
                                continue;

                        }
                        saved.Add(entity);
                    }
                }
                else if (!data_exists && error)
                {
                    throw new SystemException($"Failed to obtain JSON, error code: {error.ToString()}, message:{doc.RootElement.GetProperty("message")}");
                }
                else
                {
                    throw new SystemException("An unknown error occured. Please try again");
                }
            }

            return saved;
        }

        public T DeserializeRedditObject<T>(JsonElement js)
        {
            return JsonSerializer.Deserialize<T>(js.GetProperty("data").ToString());
        }
    }

}
