using CI536;
using Newtonsoft.Json.Linq;
using SteamKit2;
using SteamKit2.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace plugin
{
    /*
        Implementation uses some elements and guidance from the SteamKit examples.
        https://github.com/SteamRE/SteamKit/tree/master/Samples
    */
    class SteamAPI
    {                      
        private SteamClient client;
        private CallbackManager manager;
        private SteamUser user;
        private SteamFriends friends;

        private SteamUnifiedMessages unifiedMessages;
        private SteamUnifiedMessages.UnifiedService<IPlayer> servicePlayer;

        private bool isRunning;

        private string username = null;
        private string password = null;
        private string emailAuth;
        private string appAuth;
        private string savedLogin;

        private JobID gameRequest = JobID.Invalid;

        public void LoginPrompt()
        {
            UserConfig.RegisterConfig("steam");

            Steam_login();
        }

        private void Steam_login()
        {
            client = new SteamClient();
            manager = new CallbackManager(client);
            user = client.GetHandler<SteamUser>();
            friends = client.GetHandler<SteamFriends>();
            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
            manager.Subscribe<SteamUser.LoginKeyCallback>(OnLoginKey);

            //manager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendList);
            manager.Subscribe<SteamUnifiedMessages.ServiceMethodResponse>(OnMethodResponse);

            unifiedMessages = client.GetHandler<SteamUnifiedMessages>();
            servicePlayer = unifiedMessages.CreateService<IPlayer>();

            Console.WriteLine("Connecting to steam..");
            client.Connect();

            isRunning = true;
            while (isRunning)
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
        }

        private void OnFriendList(SteamFriends.FriendsListCallback obj)
        {
            // sometimes the info hasn't been cached yet, so we must wait for that
            while (friends.GetFriendPersonaName(obj.FriendList[0].SteamID) == null) { }

            Console.BackgroundColor = ConsoleColor.Cyan;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine($"You have {obj.FriendList.Count} friends!");
            Console.ResetColor();
            Console.WriteLine("Press any key to list them.");
            Console.ReadKey();
            foreach (var friend in obj.FriendList)
            {
                string name = friends.GetFriendPersonaName(friend.SteamID);
                if (name == null) name = friend.SteamID.Render();
                Console.WriteLine($"{name} ({friends.GetFriendPersonaState(friend.SteamID)})");
            }
        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback obj)
        {
            Console.WriteLine("Logged out of steam {0}", obj.Result);
        }

        private void OnLoginKey(SteamUser.LoginKeyCallback obj)
        {
            UserConfig.SetValue(PluginDLL.CONFIG_FILE, "username", username);
            UserConfig.SetValue(PluginDLL.CONFIG_FILE, "saved_login", obj.LoginKey);
        }

        private void OnLoggedOn(SteamUser.LoggedOnCallback obj)
        {
            // we might need to sort out authentication/2fa codes
            var isAuthApp = obj.Result == EResult.AccountLoginDeniedNeedTwoFactor;
            var isAuthEmail = obj.Result == EResult.AccountLogonDenied;
            if (isAuthApp || isAuthEmail)
            {
                if (isAuthEmail)
                {
                    Console.Write("Please enter your authentication code, sent to {0}: ", obj.EmailDomain);
                    emailAuth = Console.ReadLine();
                }
                else {
                    Console.Write("Please enter your app authentication code: ");
                    appAuth = Console.ReadLine();
                }

                return;
            }

            // something went wrong
            if (obj.Result != EResult.OK)
            {
                Console.WriteLine("Unable to logon to Steam: {0} / {1}", obj.Result, obj.ExtendedResult);
                isRunning = false;
                return;
            }

            while(friends.GetPersonaName() == null) { }
            Console.WriteLine("Successfully logged into steam! Welcome, " + friends.GetPersonaName());

            CPlayer_GetOwnedGames_Request req = new CPlayer_GetOwnedGames_Request()
            {
                steamid = obj.ClientSteamID,
                include_free_sub = false,
                include_appinfo = true,
            };
            gameRequest = servicePlayer.SendMessage(x => x.GetOwnedGames(req));
        }

        void OnMethodResponse(SteamUnifiedMessages.ServiceMethodResponse callback)
        {
            if (callback.JobID != gameRequest) return;
                        
            if (callback.Result != EResult.OK)
            {
                Console.WriteLine($"Request failed with {callback.Result}");
                return;
            }

            var resp = callback.GetDeserializedResponse<CPlayer_GetOwnedGames_Response>();

            Console.BackgroundColor = ConsoleColor.Cyan;
            Console.ForegroundColor = ConsoleColor.Black;
            /*
            Console.WriteLine($"You own {resp.game_count} games!");
            Console.ResetColor();
            Console.WriteLine("Press any key to list them.");
            Console.ReadKey();*/
            int existing = 0;
            foreach (var game in resp.games)
            {
                if (Library.HasGameEntry(game.name))
                {
                    //TODO: we should add a config here too tho!
                    existing++;
                    continue;
                }
                GameEntry entry = new GameEntry();
                entry.Title = game.name;
                entry.AddConfig(new LaunchConfig(){
                    Type = "steam",
                    LaunchCommand = $"steam://rungameid/{game.appid}"
                });

                Library.AddGameEntry(game.name, entry);
                //Console.WriteLine($"{game.name} ({game.appid}) Playtime: {game.playtime_forever / 60}hrs");
            }

            Library.SaveChanges();
            Console.WriteLine($"Added { resp.game_count - existing } games to your library.");

            gameRequest = JobID.Invalid;
        }

        private void PrintAppInfo(int appid, int playtime_forever)
        {
            // note: the API seems to be limited! making many requests will result in a time-ban
            using (var wb = new WebClient())
            {
                var response = wb.DownloadString($"https://store.steampowered.com/api/appdetails/?appids={appid}");

                JObject jsonObj = JObject.Parse(response);
                JToken app = jsonObj[$"{appid}"];
                if (app.Value<bool>("success"))
                {
                    app = app.Value<JToken>("data");
                }
                else return;

                string type = app.Value<string>("type");
                if (type != "game" && type != "demo") return;

                string name = app.Value<string>("name");
                Console.WriteLine($"{name} ({appid}) Playtime: {playtime_forever / 60}hrs");
            }
        }

        private void OnDisconnected(SteamClient.DisconnectedCallback obj)
        {
            Console.WriteLine("Disconnected. Reconnecting... ");
            client.Connect();
        }

        private void OnConnected(SteamClient.ConnectedCallback obj)
        {
            if (UserConfig.HasProperty(PluginDLL.CONFIG_FILE, "username") && UserConfig.HasProperty(PluginDLL.CONFIG_FILE, "saved_login"))
            {
                username = UserConfig.GetValue<string>(PluginDLL.CONFIG_FILE, "username");
                savedLogin = UserConfig.GetValue<string>(PluginDLL.CONFIG_FILE, "saved_login");
            }
            else if(username == null || savedLogin == null)
            {
                Console.Write("Username: ");
                username = Console.ReadLine();
                Console.Write("Password: ");
                password = Console.ReadLine();
            }

            Console.WriteLine("Connected to Steam! Logging in as '{0}'...", username);
            user.LogOn(new SteamUser.LogOnDetails
            {
                Username = username,
                Password = password,

                AuthCode = emailAuth,
                TwoFactorCode = appAuth,

                LoginKey = savedLogin,
                ShouldRememberPassword = true,
            });
        }
    }
}
