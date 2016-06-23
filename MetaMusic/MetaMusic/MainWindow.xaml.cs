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

using MahApps.Metro.Controls;

namespace MetaMusic
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	
	// ReSharper disable once RedundantExtendsListEntry
	public partial class MainWindow : MetroWindow
	{
		//private FlashWindowHelper _flashHelper = new FlashWindowHelper();

		public MetaMusicLogic Logic
		{ get; private set; }

		public MainWindow()
		{
			InitializeComponent();

			Logic = new MetaMusicLogic(this);
		}

		private void PlayBtn_OnClick(object sender, RoutedEventArgs e)
		{
			Logic.TogglePlay();
		}

		private void StopBtn_OnClick(object sender, RoutedEventArgs e)
		{
			Logic.Stop();
		}

		private void PlayThumbBtn_OnClick(object sender, EventArgs e)
		{
			PlayBtn_OnClick(sender, null);
		}

		private void StopThumbBtn_OnClick(object sender, EventArgs e)
		{
			StopBtn_OnClick(sender, null);
		}

		private void MainWindow_OnClosed(object sender, EventArgs e)
		{
			Logic.ShutDown();
		}
	}
}
