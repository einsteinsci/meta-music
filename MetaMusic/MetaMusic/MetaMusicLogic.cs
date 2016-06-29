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

		public int Rating
		{ get; set; }

		public bool Muted
		{ get; set; }

		public float Volume
		{ get; set; }

		public readonly VersatilePlayer Player;

		private IMusicSource _currentTrack;

		public MetaMusicLogic(MainWindow window)
		{
			Window = window;

			Volume = 1.0f;

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

			if (desc == "Play" || desc == "Resume")
			{
				Window.PlayThumbBtn.ImageSource = Util.LoadImage("Assets/tb-play.png");

				Window.Min_PlayBtn.Content = MainWindow.PLAY;
				Window.Min_PlayBtn.FontSize = 16;
				Window.Min_PlayBtn.Margin = new Thickness { Bottom = 3, Left = 5, Right = 5 };

				Window.PlayBtn.Content = MainWindow.PLAY;
				Window.PlayBtn.FontSize = 12;
				Window.PlayBtn.Padding = new Thickness { Bottom = 2, Left = 1 };
			}
			else
			{
				Window.PlayThumbBtn.ImageSource = Util.LoadImage("Assets/tb-pause.png");

				Window.Min_PlayBtn.Content = MainWindow.PAUSE;
				Window.Min_PlayBtn.FontSize = 20;
				Window.Min_PlayBtn.Margin = new Thickness { Bottom = 5, Left = 2, Right = 1.5 };

				Window.PlayBtn.Content = MainWindow.PAUSE;
				Window.PlayBtn.FontSize = 14;
				Window.PlayBtn.Padding = new Thickness { Bottom = 3 };
			}
		}

		public void UpdateVolume()
		{
			Window.Min_VolumeSlider.Value = Volume;
			Window.VolumeSlider.Value = Volume;
		}

		public void ToggleMute()
		{
			Muted = !Muted;
			Window.Min_VolumeBtn.Content = Muted ? MainWindow.SPEAKER_OFF : MainWindow.SPEAKER_ON;
			Window.Min_VolumeBtn.Margin = new Thickness { Right = Muted ? 5 : 16, Left = Muted ? 0 : 2, Bottom = 3 };

			Window.VolumeBtn.Content = Muted ? MainWindow.SPEAKER_OFF : MainWindow.SPEAKER_ON;
			Window.VolumeBtn.Padding = new Thickness { Right = Muted ? 2 : 8, Bottom = 1 };
		}

		public void SetRating(int rating)
		{
			Rating = rating;

			Window.Rating1Btn.Content = MainWindow.STAR_OFF;
			Window.Rating2Btn.Content = MainWindow.STAR_OFF;
			Window.Rating3Btn.Content = MainWindow.STAR_OFF;
			Window.Rating4Btn.Content = MainWindow.STAR_OFF;
			Window.Rating5Btn.Content = MainWindow.STAR_OFF;

			if (rating >= 1)
			{
				Window.Rating1Btn.Content = MainWindow.STAR_ON;
			}

			if (rating >= 2)
			{
				Window.Rating2Btn.Content = MainWindow.STAR_ON;
			}

			if (rating >= 3)
			{
				Window.Rating3Btn.Content = MainWindow.STAR_ON;
			}

			if (rating >= 4)
			{
				Window.Rating4Btn.Content = MainWindow.STAR_ON;
			}

			if (rating >= 5)
			{
				Window.Rating5Btn.Content = MainWindow.STAR_ON;
			}
		}

		public void PreviewRating(int rating)
		{
			Brush white = Window.Foreground;
			Brush dark = new SolidColorBrush(Colors.LightGray);

			Window.Rating1Btn.Foreground = white;
			Window.Rating2Btn.Foreground = white;
			Window.Rating3Btn.Foreground = white;
			Window.Rating4Btn.Foreground = white;
			Window.Rating5Btn.Foreground = white;

			if (rating == 0 || rating == Rating)
			{
				SetRating(Rating); // end preview
				return;
			}

			int temp = Rating;
			SetRating(rating);
			Rating = temp;

			if (rating >= 1)
			{
				Window.Rating1Btn.Foreground = dark;
			}

			if (rating >= 2)
			{
				Window.Rating2Btn.Foreground = dark;
			}

			if (rating >= 3)
			{
				Window.Rating3Btn.Foreground = dark;
			}

			if (rating >= 4)
			{
				Window.Rating4Btn.Foreground = dark;
			}

			if (rating >= 5)
			{
				Window.Rating5Btn.Foreground = dark;
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