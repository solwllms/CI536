using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private const int MAX_RECENT = 16;

        private static string defaultPath;

        private static Dictionary<string, GameEntry> collection;
        private static FixedList<string> recent;

        public static event LibraryEvent OnSaveChanges;

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

                if (collection == null) collection = new Dictionary<string, GameEntry>();
            }

            // load recent
            recent = new FixedList<string>(MAX_RECENT);
            if (UserConfig.HasProperty(CONFIG_FILE, "recent"))
            {
                foreach (var entry in UserConfig.GetValue<JArray>(CONFIG_FILE, "recent"))
                {
                    recent.Enqueue(entry.ToString());
                }
            }
            else {
                UserConfig.SetValue(CONFIG_FILE, "recent", recent.ToArray());
            }
        }

        public static void SaveChanges()
        {
            if (collection == null) return;

            OnSaveChanges?.Invoke();

            string libraryPath = UserConfig.GetValue(CONFIG_FILE, "libary_path", defaultPath);

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            string json = JsonConvert.SerializeObject(collection, Formatting.Indented, settings);
            File.WriteAllText(libraryPath, json);

            UserConfig.SetValue(CONFIG_FILE, "recent", recent.ToArray());
        }

        public static Dictionary<string, GameEntry> GetRecentGames()
        {
            Dictionary<string, GameEntry> dict = new Dictionary<string, GameEntry>();
            foreach (var key in recent.ToArray())
            {
                GameEntry entry = GetGameEntry(key);
                if (entry == null) continue;
                dict.Add(key, entry);
            }
            return dict;
        }

        public static Dictionary<string, GameEntry> GetAllEntires()
        {
            if (collection == null) return null;
            return collection;
        }
        public static Dictionary<string, GameEntry> GetEntiresSearch(string search)
        {
            if (collection == null || search == null || String.IsNullOrEmpty(search)) return null;
            return collection.Where(e => e.Value.Title.ToLower().Contains(search.Trim().ToLower()))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        public static string GetEntrySlug(GameEntry entry)
        {
            if (collection == null || !collection.ContainsValue(entry)) return null;
            return collection.KeyByValue(entry);
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
            AddToRecent(title);
            return collection[title];
        }

        public static void AddToRecent(string title)
        {
            title = title.Trim().ToLower().ToSlug();
            recent.Enqueue(title);
            UserConfig.SetValue(CONFIG_FILE, "recent", recent.ToArray());
        }
    }

    public delegate void LibraryEvent();
}
