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
using System.Windows.Shapes;

namespace CI536
{
    /// <summary>
    /// Interaction logic for EditGameInfo.xaml
    /// </summary>
    public partial class EditGameInfo : Window
    {
        GameEntry entry;

        public EditGameInfo(GameEntry entry)
        {
            this.entry = entry;
            InitializeComponent();

            Refresh();
        }

        void Refresh()
        {
            Title = "Edit - " + entry.Title;

            txtTitle.Text = entry.Title;
            txtSortingTitle.Text = entry.GetSortingTitle();
            txtSummary.Text = entry.Summary;
            txtYear.Text = entry.ReleaseYear.ToString();
            if (txtYear.Text == "-1") txtYear.Text = "";
        }

        // https://stackoverflow.com/questions/4085471/allow-only-numeric-entry-in-wpf-text-box/4085607
        private void NumericOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = IsTextNumeric(e.Text) && int.Parse(e.Text) >= 0;
        }

        private static bool IsTextNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9]");
            return reg.IsMatch(str);
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            UpdateEntry();
            Library.SaveChanges();
            Close();
        }

        void UpdateEntry()
        {
            entry.Title = txtTitle.Text;

            if (entry.SortingTitle != entry.Title && string.IsNullOrEmpty(entry.SortingTitle))
                entry.SortingTitle = txtSortingTitle.Text;
            else entry.SortingTitle = entry.Title;

            entry.Summary = txtSummary.Text;

            int y;
            if (int.TryParse(txtYear.Text, out y) || y > 0) entry.ReleaseYear = y;
            else entry.ReleaseYear = -1;
        }

        private void FetchInfo(object sender, RoutedEventArgs e)
        {
            UpdateEntry();
            _ = AutoPopulate(entry);
        }

        async Task AutoPopulate(GameEntry entry)
        {
            IsEnabled = false;
            await Metadata.PopulateGame(entry);
            Refresh();
            IsEnabled = true;
        }
    }
}
