using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

using MetaMusic.Players;
using MetaMusic.Sources;

using UltimateUtil;

namespace MetaMusic
{
	public class MetaMusicLogic
	{
		public static MetaMusicLogic Instance
		{ get; private set; }

		public MainWindow UI
		{ get; private set; }

		public Dispatcher Dispatcher => UI.Dispatcher;

		public readonly MediaPlayer FileMediaPlayer = new MediaPlayer();
		public readonly WebMusicHelper WebHelper = new WebMusicHelper();
		public readonly WavSoundHelper WavHelper = new WavSoundHelper();

		public readonly DispatcherTimer Timer = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(0.2)
		};

		public readonly List<Playlist> Playlists = new List<Playlist>();

		public readonly Queue<PlaylistItem> SongQueue = new Queue<PlaylistItem>();

		public int Rating
		{
			get
			{
				return NowPlaying?.Song.Rating ?? 0;
			}
			set
			{
				if (NowPlaying != null)
				{
					NowPlaying.Song.Rating = value;
				}
			}
		}

		public PlayerSettings Settings
		{ get; private set; }

		public SongLibrary Library
		{ get; private set; }

		public Playlist CurrentPlaylist
		{ get; private set; }

		public bool AdvancingSliders
		{ get; private set; }

		public readonly VersatilePlayer Player;

		public PlaylistItem NowPlaying => SongQueue.PeekOrDefault();

		public bool _previewingRating;

		public MetaMusicLogic(MainWindow ui)
		{
			UI = ui;

			Settings = PlayerSettings.Load();
			Library = SongLibrary.Load();
			LoadPlaylists();

			Player = new VersatilePlayer(FileMediaPlayer, WebHelper, WavHelper);
			Player.TitleChanged += (s, e) => { UpdateTitleBar(); };

			Timer.Tick += Timer_OnTick;
			Timer.Start();

			if (Instance == null)
			{
				Instance = this;
			}
		}

		private void Timer_OnTick(object sender, EventArgs e)
		{
			if (NowPlaying != null && Player.Source?.Duration != null)
			{
				NowPlaying.Song.LengthTime = Player.Source.Duration.Value;
			}

			UpdateSongUI();
			UpdateVolume();
		}

		public void LoadUISettings()
		{
			Settings = PlayerSettings.Load();

			LoadColumnWidths();

			DoThemeChange(new MenuItem { Header = Settings.ThemeAccent });

			if (UI.IsMinimalist != Settings.Minimalist)
			{
				UI.ToggleMinimalist();
			}

			UI.Width = Settings.WindowWidth;
			UI.Height = Settings.WindowHeight;

			UI.Left = Settings.WindowPosition.X;
			UI.Top = Settings.WindowPosition.Y;

			UI.LibraryTree.Items.Clear();
			UI.LibraryTree.Items.Add(MakeLibraryNode());
		}

		public void LoadPlaylists()
		{
			Playlists.Clear();

			if (!Directory.Exists(Playlist.PLAYLIST_FOLDER))
			{
				Directory.CreateDirectory(Playlist.PLAYLIST_FOLDER);
				return;
			}

			IEnumerable<string> files = from filepath in Directory.EnumerateFiles(Playlist.PLAYLIST_FOLDER)
				where filepath.ToLower().EndsWith(".json") select filepath;

			foreach (string f in files)
			{
				Playlist list = Playlist.Load(f);
				if (list != null)
				{
					Playlists.Add(list);
				}
			}
		}

		public void UpdateTitleBar()
		{
			Dispatcher.Invoke(() => {
				UI.Title = NowPlaying?.Song.DisplayName ?? "Meta Music Player";
			});
		}

		public void UpdatePlayBtn(string desc)
		{
			//Window.PlayBtn.Content = desc;
			UI.PlayThumbBtn.Description = desc;
			UI.Min_PlayBtn.ToolTip = desc;

			if (desc == "Play" || desc == "Resume")
			{
				UI.PlayThumbBtn.ImageSource = Util.LoadImage("Assets/tb-play.png");

				UI.Min_PlayBtn.Content = MainWindow.PLAY;
				UI.Min_PlayBtn.FontSize = 16;
				UI.Min_PlayBtn.Margin = new Thickness { Bottom = 3, Left = 5, Right = 5 };

				UI.PlayBtn.Content = MainWindow.PLAY;
				UI.PlayBtn.FontSize = 12;
				UI.PlayBtn.Padding = new Thickness { Bottom = 2, Left = 1 };
			}
			else
			{
				UI.PlayThumbBtn.ImageSource = Util.LoadImage("Assets/tb-pause.png");

				UI.Min_PlayBtn.Content = MainWindow.PAUSE;
				UI.Min_PlayBtn.FontSize = 20;
				UI.Min_PlayBtn.Margin = new Thickness { Bottom = 5, Left = 2, Right = 1.5 };

				UI.PlayBtn.Content = MainWindow.PAUSE;
				UI.PlayBtn.FontSize = 14;
				UI.PlayBtn.Padding = new Thickness { Bottom = 3 };
			}
		}

		public void UpdateVolume()
		{
			UI.Min_VolumeSlider.Value = Player.GetVolume(Settings);
			UI.VolumeSlider.Value = Player.GetVolume(Settings);
		}

		public void UpdateSongUI()
		{
			SongState state = SongState.Load(Player.Source, NowPlaying);

			UI.SongNameTxt.Text = state.TitleText;
			UI.Title = state.TitleTextTitleBar;
			UI.TimeTxt.Text = state.TimeText;
			UI.SongCoverImg.Source = state.Artwork;

			UI.ProgressSlider.IsEnabled = state.ProgressMeaningful;
			UI.Min_ProgressSlider.IsEnabled = state.ProgressMeaningful;

			AdvancingSliders = true;
			UI.ProgressSlider.Value = state.ProgressValue;
			UI.Min_ProgressSlider.Value = state.ProgressValue;
			AdvancingSliders = false;

			UI.SoundCloudLogoBtn.Tag = state.SoundCloudUrl;
			UI.SoundCloudLogoBtn.Visibility = state.IsSoundCloud.ToVis();
			UI.LoadingMessageTxt.Text = state.LoadingText;
			UI.LoadingMessageTxt.Visibility = state.ShowLoadingMessage.ToVis();

			if (!_previewingRating)
			{
				UpdateRatingUI(Rating);
			}
		}

		public void SetSongProgress(double value)
		{
			if (Player.Source?.Duration == null)
			{
				return;
			}

			TimeSpan progress = TimeSpan.FromSeconds(Player.Source.Duration.Value.TotalSeconds * value);
			Player.Source.Position = progress;
		}

		public void DoThemeChange(MenuItem sender)
		{
			string theme = sender.Header as string;

			UI.ChangeAppTheme(theme);
			Settings.ThemeAccent = theme;

			foreach (object child in UI.ThemeMenuRoot.Items)
			{
				MenuItem colorItem = child as MenuItem;

				if (colorItem != null && colorItem.Header != sender.Header)
				{
					colorItem.IsChecked = false;
				}
			}
		}

		public TreeViewItem MakeLibraryNode()
		{
			TreeViewItem lib = new TreeViewItem {
				Header = "Library",
				FontSize = 14,
				IsExpanded = true,
				FontWeight = FontWeights.SemiBold
			};

			TreeViewItem playlistsNode = new TreeViewItem {
				Header = "Playlists",
				FontSize = 12,
				FontWeight = FontWeights.Normal
			};

			foreach (Playlist playlist in Playlists)
			{
				TreeViewItem plNode = new TreeViewItem {
					Header = playlist.Name,
					Tag = playlist
				};
				playlistsNode.Items.Add(plNode);
			}
			lib.Items.Add(playlistsNode);

			TreeViewItem all = new TreeViewItem {
				Header = "All",
				FontSize = 12,
				FontWeight = FontWeights.Normal,
				IsExpanded = true
			};

			foreach (LibraryItem song in Library)
			{
				MakeSongNode(all, song);
			}
			lib.Items.Add(all);

			return lib;
		}

		public void MakeSongNode(TreeViewItem all, LibraryItem song)
		{
			TreeViewItem songNode = new TreeViewItem
			{
				Header = song.DisplayName,
				ToolTip = song.Source,
				Tag = song
			};

			songNode.MouseDoubleClick += (s, e) => {
				PlaySongClearQueue(song.MakePlaylistItem());
			};

			all.Items.Add(songNode);
		}

		public void PlaySongClearQueue(PlaylistItem song)
		{
			if (Player.Source != null)
			{
				Player.Stop();
			}
			
			SongQueue.Clear();
			SongQueue.Enqueue(song);
			Player.LoadAndPlay(NowPlaying.Song.Source);
			UpdatePlayBtn("Pause");
			UpdateSongUI();
		}

		public void ToggleMute()
		{
			Settings.Muted = !Settings.Muted;
			Player.Muted = Settings.Muted;

			UI.Min_VolumeBtn.Content = Settings.Muted ? MainWindow.SPEAKER_OFF : MainWindow.SPEAKER_ON;
			UI.Min_VolumeBtn.Margin = new Thickness { Right = Settings.Muted ? 5 : 16, Left = Settings.Muted ? 0 : 2, Bottom = 3 };

			UI.VolumeBtn.Content = Settings.Muted ? MainWindow.SPEAKER_OFF : MainWindow.SPEAKER_ON;
			UI.VolumeBtn.Padding = new Thickness { Right = Settings.Muted ? 2 : 8, Bottom = 1 };
		}

		public void SetRating(int rating)
		{
			Rating = rating;
			UpdateRatingUI(Rating);
		}

		public void SetRatingPreview(int rating)
		{
			UpdateRatingUI(rating);
		}

		private void UpdateRatingUI(int rating)
		{
			UI.Rating1Btn.Content = MainWindow.STAR_OFF;
			UI.Rating2Btn.Content = MainWindow.STAR_OFF;
			UI.Rating3Btn.Content = MainWindow.STAR_OFF;
			UI.Rating4Btn.Content = MainWindow.STAR_OFF;
			UI.Rating5Btn.Content = MainWindow.STAR_OFF;

			if (rating >= 1)
			{
				UI.Rating1Btn.Content = MainWindow.STAR_ON;
			}

			if (rating >= 2)
			{
				UI.Rating2Btn.Content = MainWindow.STAR_ON;
			}

			if (rating >= 3)
			{
				UI.Rating3Btn.Content = MainWindow.STAR_ON;
			}

			if (rating >= 4)
			{
				UI.Rating4Btn.Content = MainWindow.STAR_ON;
			}

			if (rating >= 5)
			{
				UI.Rating5Btn.Content = MainWindow.STAR_ON;
			}
		}

		public void PreviewRating(int rating)
		{
			Brush white = UI.Foreground;
			Brush dark = new SolidColorBrush(Colors.LightGray);

			UI.Rating1Btn.Foreground = white;
			UI.Rating2Btn.Foreground = white;
			UI.Rating3Btn.Foreground = white;
			UI.Rating4Btn.Foreground = white;
			UI.Rating5Btn.Foreground = white;

			if (rating == 0 || rating == Rating)
			{
				SetRating(Rating); // end preview
				_previewingRating = false;
				return;
			}

			_previewingRating = true;

			int temp = Rating;
			SetRatingPreview(rating);
			Rating = temp;

			if (rating >= 1)
			{
				UI.Rating1Btn.Foreground = dark;
			}

			if (rating >= 2)
			{
				UI.Rating2Btn.Foreground = dark;
			}

			if (rating >= 3)
			{
				UI.Rating3Btn.Foreground = dark;
			}

			if (rating >= 4)
			{
				UI.Rating4Btn.Foreground = dark;
			}

			if (rating >= 5)
			{
				UI.Rating5Btn.Foreground = dark;
			}
		}

		public void PlayQueueIfNotPlaying()
		{
			if (Player.Source == null)
			{
				Player.LoadAndPlay(NowPlaying.Song.Source);
				UpdatePlayBtn("Pause");
			}
		}

		public void TogglePlay()
		{
			if (Player.Source == null)
			{
				Player.LoadAndPlay(NowPlaying.Song.Source);
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

			SaveAllSettings();

			Library.Save();

			Timer.Stop();
			WebHelper.Stop();
			WavHelper.Stop();
		}

		public void SaveAllSettings()
		{
			Settings.GridSplitterWidths[0] = UI.LibraryColumn.Width.Value;
			Settings.GridSplitterWidths[1] = UI.QueueColumn.Width.Value;
			Settings.GridSplitterWidths[2] = UI.PlaylistColumn.Width.Value;
			
			Settings.WindowWidth = UI.Width;
			Settings.WindowHeight = UI.Height;

			Settings.WindowPosition = new Point(UI.Left, UI.Top);

			Settings.Save();
		}

		public void LoadColumnWidths()
		{
			UI.LibraryColumn.Width = new GridLength(Settings.GridSplitterWidths[0], GridUnitType.Star);
			UI.QueueColumn.Width = new GridLength(Settings.GridSplitterWidths[1], GridUnitType.Star);
			UI.PlaylistColumn.Width = new GridLength(Settings.GridSplitterWidths[2], GridUnitType.Star);
		}
	}
}