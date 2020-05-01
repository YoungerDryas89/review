using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Scarlett.RedditAssets
{
    public class LinkPost : RedditAsset
    {
        [JsonPropertyName("url")]
        public String url { get; set; }
    }
}
