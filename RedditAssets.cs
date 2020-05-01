using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Scarlett.RedditAssets
{
    public class RedditAsset
    {
        [JsonPropertyName("author")]
        public String author { get; set; }
        [JsonPropertyName("name")]
        public String name { get; set; }
        [JsonPropertyName("id")]
        public String id { get; set; }
        [JsonPropertyName("created")]
        public double created { get; set; }
        [JsonPropertyName("created_utc")]
        public double created_utc { get; set; }
        [JsonPropertyName("subreddit")]
        public String Subreddit { get; set; }
        [JsonPropertyName("link_url")]
        public String link_url { get; set; }
    }

}
