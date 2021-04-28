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
        public string Developer { get; set; }
        public Dictionary<string, LaunchConfig> Configs { get; set; }

        public GameEntry()
        {
            Configs = new Dictionary<string, LaunchConfig>();
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
