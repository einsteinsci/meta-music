using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

			Timer.Tick += (s, e) => {
				//Window.StatusTxt.Text = Player.LoadingText;
			};

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
			//Window.PlayBtn.Content = desc;
			Window.PlayThumbBtn.Description = desc;
			Window.Min_PlayBtn.ToolTip = desc;

			bool isPlay = (desc == "Play" || desc == "Resume");
			if (isPlay)
			{
				Window.PlayThumbBtn.ImageSource = Util.LoadImage("Assets/play.png");
			}
			else
			{
				Window.PlayThumbBtn.ImageSource = Util.LoadImage("Assets/pause.png");
			}

			if (isPlay)
			{
				Window.Min_PlayBtn.Content = MainWindow.PLAY;
				Window.Min_PlayBtn.FontSize = 16;
				Window.Min_PlayBtn.Margin = new Thickness { Bottom = 3, Left = 5, Right = 5 };
			}
			else
			{
				Window.Min_PlayBtn.Content = MainWindow.PAUSE;
				Window.Min_PlayBtn.FontSize = 20;
				Window.Min_PlayBtn.Margin = new Thickness { Bottom = 5, Left = 2, Right = 1.5 };
			}
		}

		public void TogglePlay()
		{
			if (Player.Source == null)
			{
				string url = "";//Window.MusicPathBox.Text;

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