using CI536;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace plugin
{
    public class PluginDLL : Plugin
    {
        public const string CONFIG_FILE = "steam";

        private static PluginDLL instance;

        public static string errorMessage;

        SteamAPI api;

        public PluginDLL()
        {
            instance = this;
        }

        public override async Task<bool> Load()
        {
            api = new SteamAPI();            
            return await api.LoginPrompt(true, null, null, null, null);
        }

        public override async Task Authenticate()
        {
            await api.LoginPrompt(false, null, null, null, null);
        }

        // called by our login window to use our API.
        public static void Login(string username, string password, string authCode, string emailCode)
        {
            _ = instance.api.LoginPrompt(false, username, password, authCode, emailCode);
        }

        public override async Task Sync()
        {
            if(!api.RequestGames())
                Debug.WriteLine("Failed to sync!");
        }

        public override string getName()
        {
            return "Steam";
        }
    }
}
