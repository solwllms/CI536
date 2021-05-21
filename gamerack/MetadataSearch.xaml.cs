using ModernWpf.Controls;
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
    /// Interaction logic for MetadataSearch.xaml
    /// </summary>
    public partial class MetadataSearch : ContentDialog
    {
        ContentDialog original;
        List<GameEntry> results;

        string slug;

        public MetadataSearch(ContentDialog original, string slug, string search)
        {
            InitializeComponent();
            this.original = original;
            this.slug = slug;

            SearchBox.Text = search;
        }

        async Task RefreshResults(string search)
        {
            IsEnabled = false;
            results = await Metadata.PopulateResults(search);
            ListResults.ItemsSource = results;
            ListResults.Items.Refresh();
            IsEnabled = true;
        }

        private void ResultsList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as System.Windows.Controls.ListBoxItem;
            if (item != null && item.IsSelected)
            {
                GameEntry entry = item.Content as GameEntry;
                Library.ReplaceGameEntry(slug, entry);
                Hide();
                original.ShowAsync();

                if(original.GetType() == typeof(EditGameInfo))
                    ((EditGameInfo)original).Refresh();
            }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Hide();
            original.ShowAsync();
        }

        private void controlsSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            _ = RefreshResults(sender.Text);
        }
    }
}
