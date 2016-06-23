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
using System.Windows.Threading;

using MahApps.Metro.Controls;

using MetaMusic.Players;
using MetaMusic.Sources;

using UltimateUtil;
using IOPath = System.IO.Path;

namespace MetaMusic
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	
	// ReSharper disable once RedundantExtendsListEntry
	public partial class MainWindow : MetroWindow
	{
		public readonly MediaPlayer InternalFilePlayer = new MediaPlayer();
		public readonly WebMusicHelper InternalWebPlayer = new WebMusicHelper();

		public readonly DispatcherTimer Timer = new DispatcherTimer {
			Interval = TimeSpan.FromSeconds(0.5)
		};

		public readonly VersatilePlayer Player;

		private IMusicSource _currentTrack;

		//private FlashWindowHelper _flashHelper = new FlashWindowHelper();

		public MainWindow()
		{
			InitializeComponent();

			Player = new VersatilePlayer(InternalFilePlayer, InternalWebPlayer);

			Timer.Tick += (s, e) => { StatusTxt.Text = Player.StatusText; };

			Timer.Start();

			Player.TitleChanged += (s, e) => { UpdateTitleBar(); };
		}

		public void UpdateTitleBar()
		{
			Dispatcher.Invoke(() => {
				if (Player.Source != null)
				{
					Title = "MM | " + IOPath.GetFileName(Player.Source.Title);
				}
				else
				{
					Title = "Meta Music Player";
				}
			});
		}

		public void UpdatePlayBtn(string desc)
		{
			PlayBtn.Content = desc;
			PlayThumbBtn.Description = desc;

			if (desc == "Play" || desc == "Resume")
			{
				PlayThumbBtn.ImageSource = Util.LoadImage("Assets/play.png");
			}
			else
			{
				PlayThumbBtn.ImageSource = Util.LoadImage("Assets/pause.png");
			}
		}

		private void PlayBtn_OnClick(object sender, RoutedEventArgs e)
		{
			if (Player.Source == null)
			{
				if (MusicPathBox.Text.StartsWith("http"))
				{
					_currentTrack = new SoundCloudMusic(MusicPathBox.Text, InternalWebPlayer);
				}
				else
				{
					_currentTrack = new FileMusic(MusicPathBox.Text, InternalFilePlayer);
				}

				Player.Play(_currentTrack);
				UpdatePlayBtn("Pause");
			}
			else
			{
				Player.TogglePause();
				UpdatePlayBtn(Player.Source.IsPlaying ? "Pause" : "Resume");
			}
		}

		private void StopBtn_OnClick(object sender, RoutedEventArgs e)
		{
			//_scStop = true;
			//InternalWebPlayer.Stop();

			Player.Stop();
			UpdateTitleBar();
			UpdatePlayBtn("Play");
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
			//if (StreamPlayer != null && StreamPlayer.IsBusy)
			//{
			//	StreamPlayer.CancelAsync();
			//}

			InternalWebPlayer.Stop();
		}
	}
}
