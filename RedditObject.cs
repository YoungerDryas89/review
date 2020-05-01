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
        public LinkPost lnk;
        public SelfPost sp;
        public String SerializeToJson(Boolean indent = true)
        {
            if (IsSelf)
            {
                return JsonSerializer.Serialize<SelfPost>(sp, new JsonSerializerOptions
                {
                    WriteIndented = indent
                });
            } else {
                return JsonSerializer.Serialize<LinkPost>(lnk, new JsonSerializerOptions
                {
                    WriteIndented = indent
                });
            }

        }

    }
}
