using CI536;
using Newtonsoft.Json.Linq;
using SteamKit2;
using SteamKit2.Internal;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Windows;
using ModernWpf.Controls;

namespace plugin
{
    public enum AUTH_MODE
    {
        NONE = 0,
        APP = 1,
        EMAIL = 2
    }

    enum  LOGIN_STATUS {
        AWAITING = -1,
        FAIL = 0,
        SUCCESS = 1,
    }

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

        private LOGIN_STATUS loginStatus;

        private bool background;
        private string username = null;
        private string password = null;
        private string authCode = null;
        private string emailCode = null;

        private string savedLogin;

        private SteamID steamID;

        private Login loginWindow;

        private JobID gameRequest = JobID.Invalid;
        CancellationTokenSource cancelCallbackTok;

        private static ContentDialog dialog;

        public async Task<bool> LoginPrompt(bool background = false, string username = null, string password = null, string authCode = null, string emailCode = null)
        {
            UserConfig.RegisterConfig("steam");

            this.background = background;
            this.username = username;
            this.password = password;
            this.authCode = authCode;
            this.emailCode = emailCode;

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

            loginStatus = LOGIN_STATUS.AWAITING;
            Debug.WriteLine("Connecting to steam..");
            client.Connect();

            cancelCallbackTok = new CancellationTokenSource();
            Task callbackTask = new Task(() =>
            {
                while (!cancelCallbackTok.IsCancellationRequested)
                {
                    try
                    {
                        manager.RunCallbacks();
                    }
                    catch(Exception e) {
                        Console.WriteLine("Error! : " + e.Message);
                    }
                }
            }, cancelCallbackTok.Token);
            callbackTask.Start();

            return await Task<bool>.Run(() =>
            {
                while (loginStatus == LOGIN_STATUS.AWAITING) { }
                return loginStatus == LOGIN_STATUS.SUCCESS;
            });
        }

        private void OnFriendList(SteamFriends.FriendsListCallback obj)
        {
            // sometimes the info hasn't been cached yet, so we must wait for that
            while (friends.GetFriendPersonaName(obj.FriendList[0].SteamID) == null) { }

            Console.BackgroundColor = ConsoleColor.Cyan;
            Console.ForegroundColor = ConsoleColor.Black;
            Debug.WriteLine($"You have {obj.FriendList.Count} friends!");
            Console.ResetColor();
            Debug.WriteLine("Press any key to list them.");
            Console.ReadKey();
            foreach (var friend in obj.FriendList)
            {
                string name = friends.GetFriendPersonaName(friend.SteamID);
                if (name == null) name = friend.SteamID.Render();
                Debug.WriteLine($"{name} ({friends.GetFriendPersonaState(friend.SteamID)})");
            }
        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback obj)
        {
            Debug.WriteLine($"Logged out of steam {obj.Result}");
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
                    Debug.WriteLine($"Email authentication code required.");
                    ShowLogin(AUTH_MODE.EMAIL, "Enter the code sent to your email.");
                }
                else {
                    Debug.WriteLine($"App authentication code required.");
                    ShowLogin(AUTH_MODE.APP, "Enter SteamGuard code from your app.");
                }

                return;
            }

            // something went wrong
            if (obj.Result != EResult.OK)
            {
                AUTH_MODE auth = AUTH_MODE.NONE;
                if(authCode != null && emailCode == null) auth = AUTH_MODE.APP;
                else if (authCode == null && emailCode != null) auth = AUTH_MODE.EMAIL;
                ShowLogin(auth, $"Unable to login ({obj.Result})");

                Debug.WriteLine($"Unable to logon to Steam: {obj.Result} / {obj.ExtendedResult}");
                loginStatus = LOGIN_STATUS.FAIL;
                return;
            }

            while(friends.GetPersonaName() == null) { }
            CloseLogin();
            Debug.WriteLine("Successfully logged into steam! Welcome, " + friends.GetPersonaName());

            if (!background)
                ShowDialogNotice($"Successfully logged into steam! Welcome, {friends.GetPersonaName()}.");

            loginStatus = LOGIN_STATUS.SUCCESS;

            steamID = obj.ClientSteamID;
        }

        public bool RequestGames()
        {
            if (steamID == null) return false;

            CPlayer_GetOwnedGames_Request req = new CPlayer_GetOwnedGames_Request()
            {
                steamid = steamID,
                include_free_sub = false,
                include_appinfo = true,
            };
            gameRequest = servicePlayer.SendMessage(x => x.GetOwnedGames(req));

            return true;
        }

        void OnMethodResponse(SteamUnifiedMessages.ServiceMethodResponse callback)
        {
            if (callback.JobID != gameRequest) return;
                        
            if (callback.Result != EResult.OK)
            {
                Debug.WriteLine($"Request failed with {callback.Result}");
                return;
            }

            cancelCallbackTok.Cancel();

            var resp = callback.GetDeserializedResponse<CPlayer_GetOwnedGames_Response>();

            Console.BackgroundColor = ConsoleColor.Cyan;
            Console.ForegroundColor = ConsoleColor.Black;

            List<string> titles = new List<string>();

            int existing = 0;
            foreach (var game in resp.games)
            {
                GameEntry entry;
                if (Library.HasGameEntry(game.name))
                {
                    // update a existing entry
                    entry = Library.GetGameEntry(game.name);
                    entry.PlaytimeTotalMins = game.playtime_forever;
                    entry.PlaytimeFortnightMins = game.playtime_2weeks;

                    //TODO: we should add a config here too tho!
                    existing++;
                }
                else
                {
                    // add a new entry
                    entry = new GameEntry();
                    entry.Title = game.name;
                    entry.PlaytimeTotalMins = game.playtime_forever;
                    entry.PlaytimeFortnightMins = game.playtime_2weeks;
                    entry.AddConfig(new LaunchConfig()
                    {
                        Type = "steam",
                        LaunchCommand = $"steam://rungameid/{game.appid}"
                    });

                    titles.Add(game.name);
                    Library.AddGameEntry(game.name, entry);
                    //Debug.WriteLine($"{game.name} ({game.appid}) Playtime: {game.playtime_forever / 60}hrs");
                }
            }

            gameRequest = JobID.Invalid;

            Application.Current.Dispatcher.Invoke(new Action(async () =>
            {
                int num = (int) resp.game_count - existing;

                Debug.WriteLine($"Added { num } games to your library.");
                Console.ResetColor();

                Library.SaveChanges();

                if (num > 0) {
                    ShowDialogNotice($"We'll now add the { num } games from your Steam library to your collection. We'll also grab the information for these titles from the internet.\n\nThis might take a little while! Please do not interrupt this process.", "Working..");
                    dialog.IsEnabled = false;
                    await Metadata.PopulateGames(titles);
                    dialog.IsEnabled = true;
                    ShowDialogNotice($"Finished populating your library. Have fun!", "All done");
                }
            }));
        }

        public static void ShowDialogNotice(string message, string title = "Steam Plugin")
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                dialog?.Hide();

                dialog = new ContentDialog {
                    Title = title,
                    Content = message,
                    IsPrimaryButtonEnabled = false,
                    CloseButtonText = "Ok"
                };
                _ = dialog.ShowAsync();
            }));
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
                Debug.WriteLine($"{name} ({appid}) Playtime: {playtime_forever / 60}hrs");
            }
        }

        private void OnDisconnected(SteamClient.DisconnectedCallback obj)
        {
            Debug.WriteLine("Disconnected from Steam");
        }

        private void OnConnected(SteamClient.ConnectedCallback obj)
        {
            if (UserConfig.HasProperty(PluginDLL.CONFIG_FILE, "username") && UserConfig.HasProperty(PluginDLL.CONFIG_FILE, "saved_login"))
            {
                username = UserConfig.GetValue<string>(PluginDLL.CONFIG_FILE, "username");
                savedLogin = UserConfig.GetValue<string>(PluginDLL.CONFIG_FILE, "saved_login");
            }
            else if(username == null)
            {
                Debug.WriteLine($"No saved login. Manual login required.");
                ShowLogin(AUTH_MODE.NONE, null);
                return;
            }

            Debug.WriteLine($"Connected to Steam! Logging in as '{username}'...");
            user.LogOn(new SteamUser.LogOnDetails
            {
                Username = username,
                Password = password,

                AuthCode = emailCode,
                TwoFactorCode = authCode,

                LoginKey = savedLogin,
                ShouldRememberPassword = true,
            });
        }

        public void ShowLogin(AUTH_MODE auth, string error)
        {
            if (background) return;

            Application.Current.Dispatcher.Invoke((Action)delegate {
                if (loginWindow == null) loginWindow = new Login();
                loginWindow.UpdateStage(auth, error);
                loginWindow.ShowAsync();
            });
        }

        public void CloseLogin()
        {
            if (background) return;

            Application.Current.Dispatcher.Invoke((Action)delegate {
                if (loginWindow != null)
                    loginWindow.Hide();
            });
        }
    }
}
