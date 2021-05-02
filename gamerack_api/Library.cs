using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CI536
{
    public class Library
    {
        private const string CONFIG_FILE = "library";

        private static string defaultPath;

        private static Dictionary<string, GameEntry> collection;

        public static void Init()
        {
            UserConfig.RegisterConfig(CONFIG_FILE);

            collection = new Dictionary<string, GameEntry>();
            defaultPath = Directory.GetCurrentDirectory() + "/data/library.json";
            LoadLibrary();
        }

        public static void LoadLibrary()
        {
            // make sure the file tree exists
            string libraryPath = UserConfig.GetValue(CONFIG_FILE, "libary_path", defaultPath);
            Directory.CreateDirectory(Path.GetDirectoryName(libraryPath));

            using (StreamReader reader = new StreamReader(File.Open(libraryPath, FileMode.OpenOrCreate)))
            {
                string content = reader.ReadToEnd();
                collection = JsonConvert.DeserializeObject<Dictionary<string, GameEntry>>(content);

                if(collection == null) collection = new Dictionary<string, GameEntry>();
            }
        }

        public static void SaveChanges()
        {
            if (collection == null) return;

            string libraryPath = UserConfig.GetValue(CONFIG_FILE, "libary_path", defaultPath);

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            string json = JsonConvert.SerializeObject(collection, Formatting.Indented, settings);
            File.WriteAllText(libraryPath, json);
        }

        public static IEnumerable<KeyValuePair<string, GameEntry>> GetAllEntires()
        {
            if (collection == null) return null;
            return collection;
        }
        public static IEnumerable<KeyValuePair<string, GameEntry>> GetEntiresSearch(string search)
        {
            if (collection == null || search == null || String.IsNullOrEmpty(search)) return null;
            return collection.Where(e => e.Value.Title.ToLower().Contains(search.Trim().ToLower()));
        }

        public static GameEntry GetGameEntry(string title)
        {
            title = title.Trim().ToLower().ToSlug();

            if (collection == null || !collection.ContainsKey(title)) return null;
            return collection[title];
        }

        public static bool HasGameEntry(string title)
        {
            title = title.Trim().ToLower().ToSlug();

            if (collection == null || !collection.ContainsKey(title)) return false;
            return true;
        }

        public static bool RemoveGameEntry(string title)
        {
            title = title.Trim().ToLower().ToSlug();

            if (collection == null || !collection.ContainsKey(title)) return false;
            collection.Remove(title);
            return true;
        }

        public static GameEntry AddGameEntry(string title, GameEntry entry)
        {
            title = title.Trim().ToLower().ToSlug();

            if (collection == null || collection.ContainsKey(title)) return collection[title];
            collection.Add(title, entry);
            return collection[title];
        }
    }
}
