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

		public readonly MediaPlayer InternalFilePlayer = new MediaPlayer();
		public readonly WebMusicHelper InternalWebPlayer = new WebMusicHelper();

		public readonly DispatcherTimer Timer = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(0.5)
		};

		public readonly VersatilePlayer Player;

		private IMusicSource _currentTrack;

		public MetaMusicLogic(MainWindow window)
		{
			Window = window;

			Player = new VersatilePlayer(InternalFilePlayer, InternalWebPlayer);

			Timer.Tick += (s, e) => { Window.StatusTxt.Text = Player.StatusText; };

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
				if (Window.MusicPathBox.Text.StartsWith("http"))
				{
					_currentTrack = new SoundCloudMusic(Window.MusicPathBox.Text, InternalWebPlayer);
				}
				else
				{
					_currentTrack = new FileMusic(Window.MusicPathBox.Text, InternalFilePlayer);
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
			InternalWebPlayer.Stop();
		}
	}
}