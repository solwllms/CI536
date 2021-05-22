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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CI536;
using System.Windows.Input;

namespace CI536
{
    /// <summary>
    /// Interaction logic for ImportWizard.xaml
    /// </summary>
    public partial class ImportWizard : UserControl
    {
        private List<ImportMethod> inputMethods;

        public ImportWizard()
        {
            InitializeComponent();

            inputMethods = new List<ImportMethod>();
            foreach (var plugin in PluginManager.Registered)
            {
                inputMethods.AddRange(plugin.Value.ImportMethods);
            }

            ListImportMethods.ItemsSource = inputMethods;
            ListImportMethods.Items.Refresh();
        }

        private void ListImportMethods_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListBoxItem;
            if (item != null && item.IsSelected)
            {
                ImportMethod import = item.Content as ImportMethod;
                import.handler.Invoke();
            }
        }
    }
}
