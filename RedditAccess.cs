using Scarlett.RedditAssets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RedditAccess
{
    public class Reddit
    {
        protected String AuthToken => $"{RedditAppData.ClientId}:{RedditAppData.Secret}";
        public String Username => RedditUser.Username;
        public String UserAgent => RedditAppData.UserAgent;
        protected User RedditUser;
        protected AppData RedditAppData;
        protected Dictionary<String, String> GrantParameters;
        protected TokenData tokendata;

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

        protected Reddit() { }

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
                    // Generate the rest of the URL
                    String URLTail = $"/user/{Username}/saved?";
                    for(int elem = 0; elem < args.Count; elem++)
                    {
                        String key = args.ElementAt(elem).Key;
                        String value = args.ElementAt(elem).Value;

                        URLTail += $"{key}={value}";

                        // If this is the last element don't append a & at the end to it
                        if(elem != args.Count-1)
                        {
                            URLTail += "&";
                        }
                    }
                    var req = new HttpRequestMessage(HttpMethod.Get, URLTail);
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
        public async Task AsyncGetSaved(RedditListing SavedListing, Dictionary<String, String>? args = null)
        {
            RedditListing TemporaryListing = await AsyncGetSaved(args);
            // After retrieving a new RedditListing instance, copy it to SavedListing
            SavedListing.Listing = SavedListing.Listing.Concat(TemporaryListing.Listing).ToList();
            SavedListing.before = TemporaryListing.before;
            SavedListing.after = TemporaryListing.after;

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
                                if (js.GetProperty("data").TryGetProperty("is_self", out JsonElement s).Equals(true))
                                {
                                    entity.IsSelf = s.GetBoolean();
                                    entity.Self = DeserializeRedditObject<SelfPost>(js);
                                }
                                else
                                {
                                    entity.Link = DeserializeRedditObject<LinkPost>(js);
                                }
                                break;

                            case "t1":
                                continue;

                            default:
                                continue;

                        }
                        saved.Listing.Add(entity);
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
