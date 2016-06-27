using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

using MetaMusic.Players;
using MetaMusic.Sources;

namespace MetaMusic
{
	public class MetaMusicLogic
	{
		public MainWindow Window
		{ get; private set; }

		public Dispatcher Dispatcher => Window.Dispatcher;

		public readonly MediaPlayer FileMediaPlayer = new MediaPlayer();
		public readonly WebMusicHelper WebHelper = new WebMusicHelper();
		public readonly WavSoundHelper WavHelper = new WavSoundHelper();

		public readonly DispatcherTimer Timer = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(0.5)
		};

		public readonly VersatilePlayer Player;

		private IMusicSource _currentTrack;

		public MetaMusicLogic(MainWindow window)
		{
			Window = window;

			Player = new VersatilePlayer(FileMediaPlayer, WebHelper, WavHelper);

			Timer.Tick += (s, e) => { Window.StatusTxt.Text = Player.LoadingText; };

			Timer.Start();

			Player.TitleChanged += (s, e) => { UpdateTitleBar(); };
		}

		public void UpdateTitleBar()
		{
			Dispatcher.Invoke(() => {
				if (Player.Source != null)
				{
					Window.Title = "MM | " + Path.GetFileName(Player.Source.Title);
				}
				else
				{
					Window.Title = "Meta Music Player";
				}
			});
		}

		public void UpdatePlayBtn(string desc)
		{
			Window.PlayBtn.Content = desc;
			Window.PlayThumbBtn.Description = desc;

			if (desc == "Play" || desc == "Resume")
			{
				Window.PlayThumbBtn.ImageSource = Util.LoadImage("Assets/play.png");
			}
			else
			{
				Window.PlayThumbBtn.ImageSource = Util.LoadImage("Assets/pause.png");
			}
		}

		public void TogglePlay()
		{
			if (Player.Source == null)
			{
				string url = Window.MusicPathBox.Text;

				if (url.StartsWith("https://soundcloud.com") || url.StartsWith("http://soundcloud.com"))
				{
					_currentTrack = new SoundCloudMusic(url, WebHelper);
				}
				else if (url.EndsWith(".mp3"))
				{
					_currentTrack = new FileMusic(url, FileMediaPlayer);
				}
				else if (url.EndsWith(".brstm"))
				{
					_currentTrack = new BrstmMusic(url, WavHelper);
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

		public void Stop()
		{
			Player.Stop();
			UpdateTitleBar();
			UpdatePlayBtn("Play");
		}

		public void ShutDown()
		{
			Stop();

			Timer.Stop();
			WebHelper.Stop();
			WavHelper.Stop();
		}
	}
}