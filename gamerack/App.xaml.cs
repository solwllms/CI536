using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
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
			splash.ShowInTaskbar = false;
			splash.Show();

			UserConfig.Init();
			Library.Init();
			Metadata.Init();

			PluginManager.LoadPlugins();

			MainWindow wnd = new MainWindow();
			wnd.IsEnabled = false;
			wnd.Show();

			// hide splash when we've loaded everything
			wnd.ContentRendered += (s, x) => {
				wnd.IsEnabled = true;
				splash.Hide();
			};

			// exit app on window close
			wnd.Closed += (s, x) => {
				Environment.Exit(0);
			};

			// refresh app content when our library changes
			Library.OnSaveChanges += () => wnd.RefreshContent();
		}
	}
}
