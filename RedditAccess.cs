using Scarlett.RedditAssets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedditAccess
{

    struct TokenData
    {
        [JsonPropertyName("access_token")]
        public String access_token { get; set; }
        [JsonPropertyName("token_type")]
        public String token_type { get; set; }
        [JsonPropertyName("expires_in")]
        public int expires_in { get; set; }
    }
    public class AccessData
    {
        public String authToken
        {
            get{
                return $"{this.client_id}:{this.secret}";
            }
            set{}
        }
        public String Username;
        public String UserAgent;


        private String client_id;
        private String secret;
        private String Password;
        public Dictionary<String, String> GrantParameters;
        private TokenData td;

        public AccessData(String Username, String Password, String Client_ID, String Secret, String UserAgent)
        {
            this.Username = Username;
            this.secret = Secret;
            this.Password = Password;
            this.client_id = Client_ID;
            this.UserAgent = UserAgent;
            this.GrantParameters = new Dictionary<String, String> { { "grant_type", "password" }, { "username", Username }, { "password", Password } };
        }

        private void DeserializeToken(String json)
        {
            try
            {
                td = JsonSerializer.Deserialize<TokenData>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private SelfPost DeserializeSelfPost(String json)
        {
            SelfPost p;
            try
            {
                p = JsonSerializer.Deserialize<SelfPost>(json);
                return p;
            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private LinkPost DeserializeLinkPost(String json)
        {
            LinkPost p;
            try
            {
                p = JsonSerializer.Deserialize<LinkPost>(json);
                return p;
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public void AuthorizeWithReddit()
        {
            async_authorize().GetAwaiter().GetResult();
        }
        public async Task AsyncAuthorizeWithReddit()
        {
            await async_authorize();
        }

        private async Task async_authorize()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var encoded_token = Encoding.ASCII.GetBytes(authToken);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(encoded_token));
                    client.DefaultRequestHeaders.UserAgent.TryParseAdd(UserAgent);
                    String URL = "https://www.reddit.com/api/v1/access_token";
                    var gparams = new FormUrlEncodedContent(GrantParameters);

                    var result = await client.PostAsync(URL, gparams);
                    var content = await result.Content.ReadAsStringAsync();

                    DeserializeToken(content);

                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    throw;
                }
            }
        }

        public async Task<List<RedditObject>> AsyncGetSaved(int limit = 0)
        {
            Dictionary<String, String> args = new Dictionary<string, string>
            {
                {"limit",  limit.ToString()}
            };

            using (HttpClient t = new HttpClient())
            {
                try
                {
                    t.BaseAddress = new Uri("https://oauth.reddit.com/");
                    t.DefaultRequestHeaders.UserAgent.TryParseAdd(UserAgent);
                    t.DefaultRequestHeaders.Add("Authorization", $"{td.token_type} {td.access_token}");
                    var req = new HttpRequestMessage(HttpMethod.Get, $"/user/{Username}/saved");
                    req.Content = new FormUrlEncodedContent(args);

                    var result = await t.SendAsync(req);
                    var content = await result.Content.ReadAsStringAsync();

                    List<RedditObject> saved = new List<RedditObject>();
                    using(JsonDocument doc = JsonDocument.Parse(content))
                    {
                        var data_exists = doc.RootElement.TryGetProperty("data", out JsonElement d);
                        var error = doc.RootElement.TryGetProperty("error", out JsonElement e);
                        if (data_exists)
                        {
                            foreach (var j in d.GetProperty("children").EnumerateArray())
                            {
                                RedditObject ro = new RedditObject();
                                ro.Kind = j.GetProperty("kind").ToString();
                                switch(ro.Kind)
                                {
                                    case "t3":
                                        if (j.TryGetProperty("is_self", out JsonElement s))
                                        {
                                            ro.is_self = s.GetBoolean();
                                            ro.sp = JsonSerializer.Deserialize<SelfPost>(j.GetProperty("data").ToString());
                                        }
                                        else
                                        {
                                            ro.lnk = JsonSerializer.Deserialize<LinkPost>(j.GetProperty("data").ToString());
                                        }
                                        break;

                                    case "t1":
                                        continue;

                                    default:
                                        continue;
                                        
                                }
                                saved.Add(ro);
                            }
                        } else if(!data_exists && error)
                        {
                            throw new SystemException($"Failed to obtain JSON, error code: {error.ToString()}, message:{doc.RootElement.GetProperty("message")}");
                        } else
                        {
                            throw new SystemException("An unknown error occured. Please try again");
                        }
                    }

                    return saved;
                } catch(Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    throw;
                }
            }
        }
    }

}