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
        GameEntry entry;

        public GameDetails(GameEntry entry)
        {
            InitializeComponent();
            this.entry = entry;
            lblTitle.Content = entry.Title;
            if(entry.Media.Count > 0)
                imgHeader.Source = WPFUtil.GetImageFromURL(entry.Media[0]);
        }
    }
}
