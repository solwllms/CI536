using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CI536
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : UserControl
    {
        List<GameTileEntry> gamesEntriesRecent;
        List<GameTileEntry> gamesEntries;

        public HomePage()
        {
            InitializeComponent();
            gamesEntries = new List<GameTileEntry>();
            gamesEntriesRecent = new List<GameTileEntry>();

            RefreshGamesList(Library.GetAllEntires());
            RefreshRecentGames(Library.GetRecentGames());
        }

        void RefreshRecentGames(Dictionary<string, GameEntry> games)
        {
            gamesEntriesRecent.Clear();
            if (games == null) return;
            foreach (var item in games.OrderBy(entry => entry.Value.Title))
            {
                gamesEntriesRecent.Add(new GameTileEntry() { Title = item.Value.Title, Cover = item.Value.BoxArt == null ? null : WPFUtil.GetImageFromURL(item.Value.BoxArt), Slug = item.Key });
            }
            RecentList.ItemsSource = gamesEntriesRecent;
            RecentList.Items.Refresh();
            pnlRecent.Visibility = gamesEntriesRecent.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        void RefreshGamesList(Dictionary<string, GameEntry> games)
        {
            gamesEntries.Clear();
            if (games == null) return;
            foreach (var item in games.OrderBy(entry => entry.Value.Title))
            {
                gamesEntries.Add(new GameTileEntry() { Title = item.Value.Title, Cover = item.Value.BoxArt == null ? null : WPFUtil.GetImageFromURL(item.Value.BoxArt), Slug = item.Key });
            }
            GamesListTile.ItemsSource = gamesEntries;
            GamesListTile.Items.Refresh();
            AllGamesTitle.Content = $"All Games ({gamesEntries.Count})";
        }

        private void GameTile_LeftButtonDown(object sender, RoutedEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null && item.IsSelected)
            {
                GameTileEntry entry = (GameTileEntry)item.Content;
                MainWindow.instance.ShowGameDetails(entry.Slug);
            }
        }
    }

    public class GameTileEntry
    {
        public string Title { get; set; }
        public BitmapImage Cover { get; set; }
        public string Slug { get; set; }
    }
}
