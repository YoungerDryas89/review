using RedditAccess;
using Scarlett.RedditAssets;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Scarlett
{
    public enum SortBy
    {
        Unsorted = 0,
        Subreddit,
        NSFW
    }
    
    public class Scarlett : Reddit
    {
        SortBy Sort;
        RedditListing Posts;
        ArgParser ArgsParser;
        
        public void Run(String[] args)
        {
            try
            {
                ArgsParser.Parse(args);
            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            // Start off by loading credentials and if they don't exist, ask for them directly
            if(!LoadCredentials())
            { 
                if(AskForCredentials())
                {
                    GetCredentials();
                }
            }

            Reddit rai = new Reddit(new User { Username = "pogchamp42069696969", Password = "null" }, new AppData { ClientId = "", Secret = "", UserAgent = "To help develop a reddit apps in c++ or c# and save posts from my saved" });
            rai.AuthorizeWithReddit();
            RedditListing saved = new RedditListing();
            do
            {
                rai.AsyncGetSaved(saved, new Dictionary<string, string>
                {
                    {"after", saved.after},
                    {"limit", "100" }
                }).GetAwaiter().GetResult();
            } while (!String.IsNullOrEmpty(saved.after));
            Console.WriteLine($"Total number of saved items: {saved.Listing.Count}");
            /*using (StreamWriter stream = new StreamWriter("saved.json"))
            {
                foreach (var post in saved.Listing)
                {
                    stream.WriteLine(post.SerializeToJson());
                }
            }*/


            // Download images and whatnot
            var downloadables =
                from downloadable in saved.Listing
                where downloadable.IsSelf != false && downloadable.Link != null
                select downloadable;
            foreach (var link in downloadables)
            {
                try
                {
                    var image = DownloadImage(link).GetAwaiter().GetResult();
                    StoreData(link, $"./images/{link.Link.Subreddit}/{link.Link.id}.{image.Item2}", image.Item1).GetAwaiter().GetResult();
                } catch (Exception e)
                {
                    Console.WriteLine($"Error, ${e.Message}");
                }
            }
        }

        private async Task<(Stream, String)> DownloadImage(RedditObject Post)
        {
            String extension;
            Stream data;
            using (HttpClient client = new HttpClient())
            {
                if (!String.IsNullOrEmpty(Post.Link.url))
                {
                    var response = await client.GetAsync(Post.Link.url);
                    if (response.IsSuccessStatusCode && response.Content.Headers.ContentType.MediaType.StartsWith("image"))
                    {
                        extension = response.Content.Headers.ContentType.MediaType.Split('/')[1];
                        data = await response.Content.ReadAsStreamAsync();
                    }
                    else
                    {
                        throw new HttpRequestException($"Failed to retrieve image ${Post.Link.url}");
                    }
                }
                else
                {
                    throw new NoNullAllowedException("URL is a null");
                }
                return (data, extension);
            }
        }

        private async Task StoreData(RedditObject Post, String path, Stream buffer)
        {
            Directory.CreateDirectory(path);

            using (FileStream stream = File.Create(path))
            {
                await buffer.CopyToAsync(stream);
            }
        }
        private void SortPosts(String? compared = null)
        {
            RedditListing SortedPosts = new RedditListing();
            switch(Sort)
            {
                case SortBy.Subreddit:
                    
                    SortedPosts.Listing = Posts.Listing.Where(p =>
                    {
                        if (p.Link != null && compared != null)
                        {
                            return p.Link.Subreddit == compared;
                        } else
                        {
                            return p.Self.Subreddit == compared;
                        }
                    }).ToList();
                    break;

                case SortBy.NSFW:
                    SortedPosts.Listing = Posts.Listing.Where(p => 
                    {
                        if (p.Link != null)
                        {
                            return p.Link.over_18 == true;
                        }
                        else if(p.Self != null)
                        { 
                            return p.Self.over_18 == true;
                        } else
                        {
                            return false;
                        }
                    }).ToList();
                    break;

                default:
                    break;
            }
        }
        private Boolean LoadCredentials()
        {
            return false;
        }

        private Boolean AskForCredentials()
        {
            Console.WriteLine("Enter Reddit app details (client id,secret and etc)? (Y/N):");
            int AnswerInput = Console.Read();
            Boolean AnsweredYes = false;
            switch(Char.ToUpper(Convert.ToChar(AnswerInput)))
            {
                case 'y':
                case 'Y':
                    AnsweredYes = true;
                    break;
                case 'n':
                case 'N':
                    break;

                default:
                    Console.WriteLine("Invalid input.");
                    AnsweredYes = AskForCredentials();
                    break;
            }
            return AnsweredYes;
        }
        private void GetCredentials()
        {
            Console.WriteLine("Enter Client ID: ");
            RedditAppData.ClientId = Console.ReadLine();
            Console.WriteLine("Enter Secret: ");
            RedditAppData.Secret = Console.ReadLine();
            Console.WriteLine("Enter Username: ");
            RedditUser.Username = Console.ReadLine();
            Console.WriteLine("Enter Password: ");
            RedditUser.Password = Console.ReadLine();
            Console.WriteLine("Enter User Agent: ");
            RedditAppData.UserAgent = Console.ReadLine();
        }
    }
}
