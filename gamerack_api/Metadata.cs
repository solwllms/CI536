using Newtonsoft.Json.Linq;
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

            Library.SaveChanges();
        }

        public static async Task<List<GameEntry>> PopulateResults(string search)
        {
            return await populateResults(search);
        }

        public static async Task PopulateGame(GameEntry entry)
        {
            await populateGame(entry);
        }

        private static async Task populateGames(string[] titles, string slug = null)
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
                    if (name == null) continue;

                    if (slug != null) name = slug;

                    GameEntry entry = Library.GetGameEntry(name);
                    PopulateEntry(ref entry, game);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static async Task<List<GameEntry>> populateResults(string search)
        {
            await getAccessToken();

            string request = $"search \"" + search + "\";fields name, summary, cover.url, screenshots.url, involved_companies.*, involved_companies.company.name, release_dates.y; where platforms !=n & platforms = [6];";

            string body = await postRequest($"https://api.igdb.com/v4/games/", request);
            if (body == null)
                return null;

            JArray array = JArray.Parse(body);
            List<GameEntry> entries = new List<GameEntry>();
            foreach (JObject game in array)
            {
                try
                {
                    string name = (string)game["name"];
                    if (name == null) continue;

                    GameEntry entry = new GameEntry();
                    PopulateEntry(ref entry, game);
                    entries.Add(entry);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return entries;
        }

        private static async Task populateGame(GameEntry entry)
        {
            await getAccessToken();

            string request = $"search \"" + entry.Title + "\";fields name, summary, cover.url, screenshots.url, involved_companies.*, involved_companies.company.name, release_dates.y; where platforms !=n & platforms = [6];";

            string body = await postRequest($"https://api.igdb.com/v4/games/", request);
            if (body == null)
                return;

            JArray array = JArray.Parse(body);
            if (array.Count < 1) return;
            JObject game = (JObject)array.First;

            try
            {
                PopulateEntry(ref entry, game);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Library.SaveChanges();
        }

        static void PopulateEntry(ref GameEntry entry, JObject json)
        {
            string name = (string)json["name"];
            //if (!titles.Contains(name)) continue;
            if (name == null) return;

            string summary = (string)json["summary"];
            string coverurl = null;
            if (json.ContainsKey("cover"))
                coverurl = "http:" + ((string)json["cover"]["url"]).Replace("t_thumb", "t_cover_big_2x");

            List<string> developers = new List<string>();
            List<string> publishers = new List<string>();

            if (json.ContainsKey("involved_companies"))
            {
                foreach (var company in json["involved_companies"])
                {
                    string company_name = (string)company["company"]["name"];
                    if ((bool)company["developer"] && !(bool)company["porting"] && !developers.Contains(company_name))
                        developers.Add(company_name);
                    if ((bool)company["publisher"] && !(bool)company["porting"] && !publishers.Contains(company_name))
                        publishers.Add(company_name);
                }
            }

            int earliest_release = int.MaxValue;
            if (json.ContainsKey("release_dates"))
            {
                foreach (JObject release in json["release_dates"])
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
            if (json.ContainsKey("screenshots"))
            {
                foreach (JObject screenshot in json["screenshots"])
                {
                    if (screenshot.ContainsKey("url"))
                        media.Add("http:" + ((string)screenshot["url"]).Replace("t_thumb", "t_1080p_2x"));
                }
            }

            entry.Title = name;
            entry.SortingTitle = null;
            entry.Developers = developers;
            entry.Publishers = publishers;
            entry.Summary = summary;
            entry.ReleaseYear = earliest_release;
            entry.BoxArt = coverurl;
            entry.Media = media;
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
