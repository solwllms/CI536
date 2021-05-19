using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CI536
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			Splash splash = new Splash();
			splash.Show();

			UserConfig.Init();
			Library.Init();
			Metadata.Init();

			Plugins.LoadPlugins();

			MainWindow wnd = new MainWindow();
			splash.Hide();
			wnd.Show();
		}
	}
}
