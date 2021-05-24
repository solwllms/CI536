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
    /// Interaction logic for EditGameConfig.xaml
    /// </summary>
    public partial class EditGameConfig : ContentDialog
    {
        string slug;
        GameEntry entry => Library.GetGameEntry(slug);
        List<LaunchConfigDisplay> configs;

        public EditGameConfig(string slug)
        {
            InitializeComponent();
            this.slug = slug;

            configs = new List<LaunchConfigDisplay>();
            Refresh();
        }

        void Refresh()
        {
            IsEnabled = false;
            configs.Clear();

            GameEntry entry = Library.GetGameEntry(slug);
            foreach (var config in entry.Configs)
            {
                configs.Add(new LaunchConfigDisplay()
                {
                    Entry = entry,
                    UUID = config.Key,
                    Type = PluginManager.GetLaunchConfigType(config.Value.Type),
                    LaunchCommand = config.Value.LaunchCommand,
                    LaunchArguments = config.Value.LaunchArguments
                }); ;
            }

            ListConfigs.ItemsSource = configs;
            ListConfigs.Items.Refresh();
            IsEnabled = true;
        }

        public static void EditConfig(string slug)
        {
            EditGameConfig edit = new EditGameConfig(slug);
            edit.ShowAsync();
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void AddConfig_Click(object sender, RoutedEventArgs e)
        {
            entry.AddConfig(new LaunchConfig());
            Refresh();
        }

        private void ConfigSelect_Checked(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            entry.SelectedConfig = ((LaunchConfigDisplay)fe.DataContext).UUID;
            Refresh();
        }

        private void ConfigRemove_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            string uuid = ((LaunchConfigDisplay)fe.DataContext).UUID;

            _ = RemoveConfigPrompt(uuid);
        }

        private async Task RemoveConfigPrompt(string uuid)
        {
            Hide();
            var dialog = new ContentDialog
            {
                Title = "Remove Config",
                Content = "Are you sure you want to remove this launch configuration?",
                IsPrimaryButtonEnabled = true,
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No"
            };
            var result = await dialog.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                entry.RemoveConfig(uuid);
                Refresh();
            }

            await ShowAsync();
        }
    }

    public class LaunchConfigDisplay
    {
        public GameEntry Entry { get; set; }
        public string UUID { get; set; }
        public LaunchConfigType Type;
        public string Name => Type == null ? "Unknown" : Type.Alias;
        public bool CanEditLaunchCommand => Type == null ? true : Type.CanEditLaunchCommand;
        public string LaunchCommand
        {
            get
            {
                return Entry.Configs[UUID].LaunchCommand;
            }
            set
            {
                Entry.Configs[UUID].LaunchCommand = value;
                Library.SaveChanges();
            }
        }
        public string LaunchArguments
        {
            get
            {
                return Entry.Configs[UUID].LaunchArguments;
            }
            set
            {
                Entry.Configs[UUID].LaunchArguments = value;
                Library.SaveChanges();
            }
        }

        public bool isSelected => Entry.SelectedConfig == UUID;
    }
}
