using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Scarlett.RedditAssets
{
    public struct RedditObject
    {
        public String Kind;
        public Boolean IsSelf;
        public LinkPost Link;
        public SelfPost Self;
        public String SerializeToJson(Boolean indent = true)
        {
            if (IsSelf)
            {
                return JsonSerializer.Serialize<SelfPost>(Self, new JsonSerializerOptions
                {
                    WriteIndented = indent
                });
            } else {
                return JsonSerializer.Serialize<LinkPost>(Link, new JsonSerializerOptions
                {
                    WriteIndented = indent
                });
            }

        }

    }
}
