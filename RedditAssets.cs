using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Scarlett.RedditAssets
{
    public class IRedditAsset
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

    public class SelfPost : IRedditAsset
    {
        [JsonPropertyName("selftext")]
        public String self_text { get; set; }
        [JsonPropertyName("selftext_html")]
        public String html_body { get; set; }

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

    public class LinkPost : IRedditAsset
    {
        [JsonPropertyName("url")]
        public String url { get; set; }

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

    public struct RedditObject
    {
        public String Kind;
        public Boolean is_self;
        public LinkPost lnk;
        public SelfPost sp;

    }

}
