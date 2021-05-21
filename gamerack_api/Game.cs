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
        public int PlaytimeTotalMins { get; set; }
        public int PlaytimeFortnightMins { get; set; }
        public string BoxArt { get; set; }
        public List<string> Media { get; set; }
        public Dictionary<string, LaunchConfig> Configs { get; set; }
        public string SelectedConfig { get; set; }

        public GameEntry()
        {
            Developers = new List<string>();
            Publishers = new List<string>();
            Media = new List<string>();
            Configs = new Dictionary<string, LaunchConfig>();
            PlaytimeTotalMins = -1;
            PlaytimeFortnightMins = -1;
            ReleaseYear = -1;
        }

        public void AddConfig(LaunchConfig config)
        {
            string uuid = Guid.NewGuid().ToString();
            Configs.Add(uuid, config);

            if (SelectedConfig == null)
                SelectedConfig = uuid;
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

        public bool Launch(string configUUID = null)
        {
            if (configUUID == null)
                configUUID = SelectedConfig;

            Library.AddToRecent(Title);

            if (configUUID == null) return false;
            if (Configs.ContainsKey(configUUID))
            {
                System.Diagnostics.Process.Start(Configs[configUUID].LaunchCommand, Configs[configUUID].LaunchArguments);
                return true;
            }
            return false;
        }
    }

    public class LaunchConfig
    {
        public string Type { get; set; }
        public string LaunchCommand { get; set; }
        public string LaunchArguments { get; set; }
    }
}
