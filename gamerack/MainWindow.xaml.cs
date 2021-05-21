using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CI536
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow instance;
        List<GameListEntry> gamesEntries;

        private string contextMenuSlug;
        private string currentGameSlug;

        public MainWindow()
        {
            InitializeComponent();
            gamesEntries = new List<GameListEntry>();

            instance = this;

            ShowHome();
            RefreshGamesList(Library.GetAllEntires());
        }

        public void RefreshContent()
        {
            RefreshGamesList(Library.GetAllEntires());

            if(ContentFrame.CurrentSourcePageType == typeof(GameDetails))
            {
                ShowGameDetails(currentGameSlug);
            }
            else ContentFrame.Refresh();
        }

        void RefreshGamesList(Dictionary<string, GameEntry> games)
        {
            gamesEntries.Clear();
            if (games == null) return;
            foreach (var item in games.OrderBy(entry => entry.Value.GetSortingTitle()))
            {
                gamesEntries.Add(new GameListEntry() { Name = item.Value.Title, Slug = item.Key });
            }
            lvGames.ItemsSource = gamesEntries;
            lvGames.Items.Refresh();
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            Navigate(Activator.CreateInstance((Type)args.InvokedItemContainer.Tag));
        }

        private void GamesList_LeftButtonDown(object sender, RoutedEventArgs e)
        {
            var item = sender as System.Windows.Controls.ListViewItem;
            if (item != null && item.IsSelected)
            {
                GameListEntry entry = (GameListEntry)item.Content;
                ShowGameDetails(entry.Slug);
            }
        }

        void ShowHome()
        {
            Navigate(new HomePage());
        }

        public void ShowGameDetails(string slug)
        {
            if (!Library.HasGameEntry(slug)) return;
            NavView.SelectedItem = null;
            currentGameSlug = slug;
            Navigate(new GameDetails(slug), true);
        }

        void Navigate(object page, bool bypass = false)
        {
            if (ContentFrame.CurrentSourcePageType != page.GetType() || bypass)
            {
                ContentFrame.Navigate(page);
            }
        }

        private void Search_GamesList(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (string.IsNullOrEmpty(sender.Text))
                RefreshGamesList(Library.GetAllEntires());
            else
                RefreshGamesList(Library.GetEntiresSearch(sender.Text));
        }

        private void Search_GamesList_Reset(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(sender.Text))
                RefreshGamesList(Library.GetAllEntires());
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            ShowHome();
        }

        public void ShowGameContextMenu(string slug, UIElement element)
        {
            ContextMenu cm = FindResource("cmButton") as ContextMenu;
            contextMenuSlug = slug;
            cm.PlacementTarget = element;
            cm.IsOpen = true;
        }

        private void GamesList_RightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = sender as System.Windows.Controls.ListViewItem;
            if (item != null && item.IsSelected)
            {
                GameListEntry entry = (GameListEntry)item.Content;
                ShowGameContextMenu(entry.Slug, sender as UIElement);
            }
        }

        private void GameContextMenu_Play(object sender, RoutedEventArgs e)
        {
            GameEntry entry = Library.GetGameEntry(contextMenuSlug);
            entry.Launch();
        }

        private void GameContextMenu_Edit(object sender, RoutedEventArgs e)
        {
            GameEntry entry = Library.GetGameEntry(contextMenuSlug);
            entry.EditInfo();
        }

        private void GameContextMenu_Hide(object sender, RoutedEventArgs e)
        {
            GameEntry entry = Library.GetGameEntry(contextMenuSlug);
            entry.Hide();
        }
    }

    struct GameListEntry
    {
        public string Name;
        public string Slug;

        public override string ToString()
        {
            return this.Name;
        }
    }
}
