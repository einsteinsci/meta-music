using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using MahApps.Metro;

namespace MetaMusic
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			
		}

		private void _loadAccent(string name)
		{
			ThemeManager.AddAccent(name, new Uri($"pack://application:,,,/MahAppsMetroThemesSample;component/{name}.xaml"));
		}
	}
}
