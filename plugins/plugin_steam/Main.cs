using CI536;

namespace plugin
{
    public class PluginDLL : Plugin
    {
        public const string CONFIG_FILE = "steam";


        public override void Init()
        {
            SteamAPI api = new SteamAPI();
            api.LoginPrompt();
        }


        public override string getName()
        {
            return "Steam";
        }
    }
}
