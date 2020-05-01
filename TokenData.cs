using System;
using System.Text.Json.Serialization;

namespace Scarlett.RedditAssets
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

}