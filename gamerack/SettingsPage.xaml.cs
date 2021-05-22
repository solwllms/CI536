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
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            InitializeComponent();

            ListPlugins.ItemsSource = PluginManager.Registered.Values;
            ListPlugins.Items.Refresh();
        }

        private void Plugin_Refresh(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            _ = ((Plugin)fe.DataContext).Refresh();
        }
    }
}
