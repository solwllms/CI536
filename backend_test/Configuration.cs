using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CI536
{
    public class Configuration
    {
        private static Dictionary<string, JObject> configFiles;

        private static string configPath;

        public static void Init()
        {
            configFiles = new Dictionary<string, JObject>();
            configPath = Directory.GetCurrentDirectory() + "/config/";
            LoadFiles();
        }

        static void LoadFiles()
        {
            if (configFiles == null) return;

            if (!Directory.Exists(configPath))
                Directory.CreateDirectory(configPath);

            string[] configs = System.IO.Directory.GetFiles(configPath, "*.json");
            foreach (string file in configs)
            {
                if (!File.Exists(file)) return;

                string contents = File.ReadAllText(file);
                JObject json = JObject.Parse(contents);
                configFiles.Add(Path.GetFileNameWithoutExtension(file),json);
            }
        }

        public static void SaveAllFiles()
        {
            foreach (var entry in configFiles)
            {
                File.WriteAllText(configPath + entry.Key + ".json", entry.Value.ToString());
            }
        }

        static void SaveFile(string file)
        {
            if (!configFiles.ContainsKey(file)) return;

            File.WriteAllText(configPath + file + ".json", configFiles[file].ToString());
        }

        public static void SetupFile(string file)
        {
            // we've already got it
            if (configFiles.ContainsKey(file)) return;

            JObject obj = new JObject();
            configFiles.Add(file, obj);
            SaveFile(file);
        }

        public static void SetValue(string file, string property, object value)
        {
            if (!configFiles.ContainsKey(file)) return;

            if (configFiles[file].ContainsKey(property))
            {
                configFiles[file][property] = JToken.FromObject(value);
            }
            else configFiles[file].Add(property, JToken.FromObject(value));

            SaveFile(file);
        }

        public static T GetValue<T>(string file, string property)
        {
            if (!configFiles.ContainsKey(file)) return default(T);

            if (configFiles[file].ContainsKey(property))
            {
                return configFiles[file].Value<T>(property);
            }
            else return default(T);
        }

        public static bool HasProperty(string file, string property)
        {
            if (!configFiles.ContainsKey(file)) return false;

            return configFiles[file].ContainsKey(property);
        }
    }
}
