using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using RedditAccess;
using Scarlett.RedditAssets;

namespace Scarlett
{
    class Program
    {
        static async Task Main(string[] args)
        {
            AccessData ad = new AccessData("Sai22", "password", "client id", "secret", "To help develop a reddit apps in c++ or c# and save posts from my saved ( by /u/Sai22 )");
            ad.AuthorizeWithReddit();
            var r = await ad.AsyncGetSaved(10);
            using (StreamWriter sw = File.CreateText("saved.json"))
            {
                foreach (var i in r)
                {
                    String j;
                    if (i.is_self)
                    {
                        j = JsonSerializer.Serialize<SelfPost>(i.sp);
                    }
                    else
                    {
                        j = JsonSerializer.Serialize<LinkPost>(i.lnk);
                    }
                    sw.Write(j);
                }
            }
        }
    }
}
