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
    /// Interaction logic for GameDetails.xaml
    /// </summary>
    public partial class GameDetails : UserControl
    {

        List<string> media;
        string slug;

        GameEntry entry => Library.GetGameEntry(slug);

        public GameDetails(string slug)
        {
            InitializeComponent();
            this.slug = slug;
            Refresh();
        }

        public void Refresh()
        {
            lblTitle.Content = entry.Title;
            if (entry.Media.Count > 0)
                imgHeader.Source = WPFUtil.GetImageFromURL(entry.Media[0], 1280, 720, false);

            lblSummary.Text = entry.Summary;
            if (entry.Developers.Count == 0)
                lblDev.Content = "Unknown";
            else
                lblDev.Content = string.Join("\n", entry.Developers);

            if (entry.Publishers.Count == 0)
                lblPub.Content = "Unknown";
            else
                lblPub.Content = string.Join("\n", entry.Publishers);

            if (entry.ReleaseYear < 0)
                lblRel.Content = "Unknown";
            else
                lblRel.Content = entry.ReleaseYear;

            // stats
            pnlPlaytime.Visibility = entry.PlaytimeTotalMins == -1 ? Visibility.Collapsed : Visibility.Visible;
            lblTotalPlaytime.Content = ((float)entry.PlaytimeTotalMins / 60).ToString("0.0") + " hrs";
            lblRecentPlaytime.Content = ((float)entry.PlaytimeFortnightMins / 60).ToString("0.0") + " hrs";

            // media
            media = new List<string>();
            for (int i = 1; i < entry.Media.Count; i++)
            {
                WPFUtil.GetImageFromURL(entry.Media[i], 1280, 720, false);
            }
            MediaList.ItemsSource = media;
            MediaList.Items.Refresh();
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            if(!entry.Launch())
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Launch failed",
                    Content = "Failed to launch game with the current configuration.",
                    IsPrimaryButtonEnabled = false,
                    CloseButtonText = "Ok"
                };
                _ = dialog.ShowAsync();
            }
        }

        private void EditInfo_Click(object sender, RoutedEventArgs e)
        {
            EditGameInfo.EditInfo(slug);
        }

        private void EditConfig_Click(object sender, RoutedEventArgs e)
        {
            EditGameConfig.EditConfig(slug);
        }
    }
}
