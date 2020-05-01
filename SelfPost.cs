using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Scarlett.RedditAssets
{
    public class SelfPost : RedditAsset
    {
        [JsonPropertyName("selftext")]
        public String self_text { get; set; }
        [JsonPropertyName("selftext_html")]
        public String html_body { get; set; }
    }
}
