using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CI536
{
    public class GameEntry
    {
        public string Title { get; set; }
        public List<string> Developers { get; set; }
        public List<string> Publishers { get; set; }
        public string Summary { get; set; }
        public int ReleaseYear { get; set; }
        public string BoxArt { get; set; }
        public List<string> Media { get; set; }
        public Dictionary<string, LaunchConfig> Configs { get; set; }

        public GameEntry()
        {
            Developers = new List<string>();
            Publishers = new List<string>();
            Media = new List<string>();
            Configs = new Dictionary<string, LaunchConfig>();
            ReleaseYear = -1;
        }

        public void AddConfig(LaunchConfig config)
        {
            Configs.Add(Guid.NewGuid().ToString(), config);
        }

        public bool RemoveConfig(string uuid)
        {
            if (Configs.ContainsKey(uuid))
            {
                Configs.Remove(uuid);
                return true;
            }
            return false;
        }

        public void RemoveAllConfigs()
        {
            Configs.Clear();
        }
    }

    public class LaunchConfig
    {
        public string Type { get; set; }
        public string LaunchCommand { get; set; }
        public string LaunchArguments { get; set; }
    }
}
