using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection.Metadata;
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
            //Reddit ad = new Reddit("Sai22", "password", "client id", "secret", "To help develop a reddit apps in c++ or c# and save posts from my saved ( by /u/Sai22 )");
            Reddit rai = new Reddit(new User { Username="Sai22", Password="kkkkk" }, new AppData { ClientId="kdfgdfgdfgd", Secret="uwu", UserAgent="lmao" });
            rai.AuthorizeWithReddit();
            var saved = rai.GetSaved(new Dictionary<String, String>{
                { "limit", "5"}
            });
            Console.WriteLine($"Total number of saved items: {saved.Count}");
            using (StreamWriter stream = new StreamWriter("saved.json"))
            {
                foreach (var post in saved)
                {
                    stream.WriteLine(post.SerializeToJson());
                }
            }
     
        }
    }
}
