using CI536;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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

            Image img = new Image();
            img.Source = GetSourceForOnRender("plugin.icon.png");
            ImportMethod main = new ImportMethod("Steam", "Import Steam library games, and launch these games through Steam.", ImportGames, img);
            RegisterImportMethod(main);
        }

        public BitmapSource GetSourceForOnRender(string file)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var bitmap = new BitmapImage();

            using (var stream =
                assembly.GetManifestResourceStream(file))
            {
                if (stream == null) return null;

                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }

            return bitmap;
        }

        void ImportGames()
        {
            _ = importGames();
        }

        async Task importGames()
        {
            await Authenticate();
            if (!api.RequestGames())
            {
                SteamAPI.ShowDialogNotice("Failed to sync. You can retry sync from the plugins menu.");
                Debug.WriteLine("Failed to sync!");
            }
        }

        public override async Task<bool> Load()
        {
            api = new SteamAPI();            
            return await api.LoginPrompt(true, null, null, null, null);
            if (!api.RequestGames())
            {
                SteamAPI.ShowDialogNotice("Failed to sync. You can retry sync from the plugins menu.");
                Debug.WriteLine("Failed to sync!");
            }
        }

        public async Task Authenticate()
        {
            await api.LoginPrompt(false, null, null, null, null);
        }

        // called by our login window to use our API.
        public static void Login(string username, string password, string authCode, string emailCode)
        {
            _ = instance.api.LoginPrompt(false, username, password, authCode, emailCode);
        }

        public override async Task Refresh()
        {
            _ = importGames();
        }

        public override string getName() { return "Steam"; }
        public override string getAuthor() { return "Sol Williams"; }
        public override string getVersion() { return "v1.0"; }
        public override string getSummary() { return "Sync and import your game library and statistics from Steam."; }
    }
}
