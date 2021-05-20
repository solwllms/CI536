﻿using System;
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
        GameEntry entry;

        public GameDetails(GameEntry entry)
        {
            InitializeComponent();
            this.entry = entry;
            lblTitle.Content = entry.Title;
            if(entry.Media.Count > 0)
                imgHeader.Source = WPFUtil.GetImageFromURL(entry.Media[0]);

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

            pnlPlaytime.Visibility = entry.PlaytimeTotalMins == -1 ? Visibility.Collapsed : Visibility.Visible;
            lblTotalPlaytime.Content = ((float)entry.PlaytimeTotalMins / 60).ToString("0.0") + " hrs";
            lblRecentPlaytime.Content = ((float)entry.PlaytimeFortnightMins / 60).ToString("0.0") + " hrs";
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            entry.Launch();
        }
    }
}
