﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CI536
{
    public class Metadata
    {
        // TODO: move these to env variables at some point!
        private const string client_id = "vzzhw04og482t4cftxngi003tcqs0g";
        private const string client_secret = "l57t9qfnkfxz9ipo4k8ewe031faj06";

        private static HttpClient client;

        public static void Init()
        {
            client = new HttpClient();

            //PopulateGames(new string[] { "Call of Duty 4: Modern Warfare", "Quake" });
        }

        public static async Task PopulateGames(List<string> titles)
        {
            if (titles.Count == 0)
            {
                Console.WriteLine("No metadata to fetch!");
                return;
            }

            Console.WriteLine("Doing metadata fetch.. 0%");
            float pc = ((float)titles.Count() / 10) / 100;
            int i = 1;
            var chunks = titles.Split(10);
            foreach (var chunk in chunks)
            {
                await populateGames(chunk.ToArray());
                Console.WriteLine($"{Math.Floor(i * pc)}%");
                i++;
                Thread.Sleep(1500);
            }
            Console.WriteLine("100% - got all metadata!");
        }

        private static async Task populateGames(string[] titles)
        {
            await getAccessToken();

            string title_search = string.Join("\",\"", titles).ToSafeString();
            string request = $"fields name, summary, cover.url, screenshots.url, involved_companies.*, involved_companies.company.name, release_dates.y; where platforms !=n & platforms = [6] & name = (\"{title_search}\");";

            string body = await postRequest($"https://api.igdb.com/v4/games/", request);
            if (body == null)
                return;

            JArray array = JArray.Parse(body);
            foreach (JObject game in array)
            {
                try
                {
                    string name = (string)game["name"];
                    //if (!titles.Contains(name)) continue;
                    if (name == null) continue;

                    string summary = (string)game["summary"];
                    string coverurl = null;
                    if (game.ContainsKey("cover"))
                        coverurl = "http:" + ((string)game["cover"]["url"]).Replace("t_thumb", "t_cover_big");

                    List<string> developers = new List<string>();
                    List<string> publishers = new List<string>();

                    if (game.ContainsKey("involved_companies"))
                    {
                        foreach (var company in game["involved_companies"])
                        {
                            string company_name = (string)company["company"]["name"];
                            if ((bool)company["developer"] && !(bool)company["porting"] && !developers.Contains(company_name))
                                developers.Add(company_name);
                            if ((bool)company["publisher"] && !(bool)company["porting"] && !publishers.Contains(company_name))
                                publishers.Add(company_name);
                        }
                    }

                    int earliest_release = int.MaxValue;
                    if (game.ContainsKey("release_dates"))
                    {
                        foreach (JObject release in game["release_dates"])
                        {
                            if (release.ContainsKey("y"))
                            {
                                int year = (int)release["y"];
                                if (year < earliest_release) earliest_release = year;
                            }
                        }
                    }
                    else earliest_release = -1;

                    List<string> media = new List<string>();
                    if (game.ContainsKey("screenshots"))
                    {
                        foreach (JObject screenshot in game["screenshots"])
                        {
                            if (screenshot.ContainsKey("url"))
                                media.Add("http:" + ((string)screenshot["url"]).Replace("t_thumb", "t_1080p"));
                        }
                    }

                    /*
                    Console.WriteLine($"Title: {name} ({earliest_release})");
                    Console.WriteLine("Developers: " + string.Join(", ", developers));
                    Console.WriteLine("Publishers: " + string.Join(", ", publishers));
                    Console.WriteLine("Desc: " + summary);
                    Console.WriteLine("Cover: " + coverurl);*/

                    GameEntry entry = Library.GetGameEntry(name);
                    entry.Developers = developers;
                    entry.Publishers = publishers;
                    entry.Summary = summary;
                    entry.ReleaseYear = earliest_release;
                    entry.BoxArt = coverurl;
                    entry.Media = media;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            Library.SaveChanges();
        }

        public static async Task getAccessToken()
        {
            string body = await postRequest($"https://id.twitch.tv/oauth2/token?client_id={client_id}&client_secret={client_secret}&grant_type=client_credentials", "", false);
            JObject json = JObject.Parse(body);

            if (json.ContainsKey("access_token"))
            {
                string token = json["access_token"].Value<string>();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        static async Task<string> postRequest(string url, string request, bool includeHeader = true)
        {
            StringContent content = new StringContent(request);
            if (includeHeader)
            {
                content.Headers.Add("Client-ID", client_id);
            }

            var result = await client.PostAsync(url, content);
            //Console.WriteLine(result);
            //if(!result.IsSuccessStatusCode) return null;

            string body = await result.Content.ReadAsStringAsync();
            return body;
        }
    }
}
