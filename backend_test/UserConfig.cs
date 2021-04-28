using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CI536
{
    public class UserConfig
    {
        private static Dictionary<string, JObject> configFiles;

        private static string configPath;

        public static void Init()
        {
            configFiles = new Dictionary<string, JObject>();
            configPath = Directory.GetCurrentDirectory() + "/config/";
            LoadAllConfigs();
        }

        static void LoadAllConfigs()
        {
            if (configFiles == null) return;

            if (!Directory.Exists(configPath))
                Directory.CreateDirectory(configPath);

            string[] configs = System.IO.Directory.GetFiles(configPath, "*.json");
            foreach (string file in configs)
            {
                if (!File.Exists(file)) return;

                string contents = File.ReadAllText(file);
                contents = contents.Replace("\r\n", "\n");
                JObject json = JObject.Parse(contents);
                configFiles.Add(Path.GetFileNameWithoutExtension(file),json);
            }
        }

        public static void SaveAllConfigs()
        {
            foreach (var entry in configFiles)
            {
                File.WriteAllText(configPath + entry.Key + ".json", entry.Value.ToString());
            }
        }

        static void SaveConfig(string file)
        {
            if (!configFiles.ContainsKey(file)) return;

            JsonSerializerSettings settings = new JsonSerializerSettings();

            string json = JsonConvert.SerializeObject(configFiles[file], Formatting.Indented);
            json = json.Replace("\r\n", "\n");
            File.WriteAllText(configPath + file + ".json", json);
        }

        public static void RegisterConfig(string file)
        {
            // we've already got it
            if (configFiles.ContainsKey(file)) return;

            JObject obj = new JObject();
            configFiles.Add(file, obj);
            SaveConfig(file);
        }

        public static void SetValue(string file, string property, object value)
        {
            if (!configFiles.ContainsKey(file)) return;

            if (configFiles[file].ContainsKey(property))
            {
                configFiles[file][property] = JToken.FromObject(value);
            }
            else configFiles[file].Add(property, JToken.FromObject(value));

            SaveConfig(file);
        }

        public static T GetValue<T>(string file, string property, T fallback = default(T))
        {
            if (!configFiles.ContainsKey(file))
            {
                Console.WriteLine($"Error: User config file '{file}' not initialised.");
                return fallback;
            }

            if (configFiles[file].ContainsKey(property))
            {
                return configFiles[file].Value<T>(property);
            }
            else
            {
                SetValue(file, property, fallback);
                return fallback;
            }
        }

        public static bool HasProperty(string file, string property)
        {
            if (!configFiles.ContainsKey(file)) return false;

            return configFiles[file].ContainsKey(property);
        }
    }
}
