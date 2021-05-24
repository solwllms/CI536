using CI536;
using Microsoft.Win32;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace plugin
{
    class PluginDefault : Plugin
    {

        ContentDialog dialog;

        public PluginDefault()
        {
            isRefreshEnabled = false;

            ImportMethod main = new ImportMethod("Local Disk", "Import game executables from local disk.", ImportGames, new SymbolIcon(Symbol.NewFolder));
            RegisterImportMethod(main);

            LaunchConfigType mainType = new LaunchConfigType("Local App");
            RegisterLaunchConfigType("", mainType);
        }

        public void ImportGames()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Games and Apps (*.exe, *.url)|*.exe;*.url|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileNames.Length > 1)
                    AddMultipleGames(openFileDialog.FileNames);
                else if (openFileDialog.FileNames.Length == 1)
                    AddSingleGame(openFileDialog.FileNames[0]);
            }
        }

        void AddSingleGame(string filename)
        {
            GameEntry entry = AddGame(filename);
            EditGameInfo.EditInfo(Library.GetEntrySlug(entry));
        }

        GameEntry AddGame(string filename)
        {
            GameEntry entry = new GameEntry();
            entry.Title = Path.GetFileNameWithoutExtension(filename);
            entry.AddConfig(new LaunchConfig()
            {
                LaunchCommand = filename
            });

            Library.AddGameEntry(entry.Title, entry);
            return entry;
        }

        void AddMultipleGames(string[] files)
        {
            List<string> titles = new List<string>();

            foreach (string filename in files)
            {
                GameEntry entry = AddGame(filename);
                titles.Add(entry.Title);
            }

            Application.Current.Dispatcher.Invoke(new Action(async () =>
            {
                int num = titles.Count;

                Debug.WriteLine($"Added { num } games to your library.");
                Console.ResetColor();

                Library.SaveChanges();

                if (num > 0)
                {


                    ShowDialogNotice($"We'll now add these { num } games to your collection. We'll also grab the information for these titles from the internet.\n\nThis might take a little while! Please do not interrupt this process.", "Working..");
                    dialog.IsEnabled = false;
                    await Metadata.PopulateGames(titles);
                    dialog.IsEnabled = true;
                    ShowDialogNotice($"Finished populating your library. Have fun!", "All done");
                }
            }));
        }

        void ShowDialogNotice(string message, string title = "Import games")
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                dialog?.Hide();

                dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    IsPrimaryButtonEnabled = false,
                    CloseButtonText = "Ok"
                };
                _ = dialog.ShowAsync();
            }));
        }

        public override async Task<bool> Load()
        {
            return true;
        }

        public override async Task Refresh()
        {

        }

        public override string getName() { return "Gamerack"; }
        public override string getAuthor() { return "Group B - CI536"; }
        public override string getVersion() { return "v1.0"; }
        public override string getSummary() { return "Default Gamerack import options."; }
    }
}
