using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

using MetaMusic.Players;

using UltimateUtil;

namespace MetaMusic
{
	public class MetaMusicLogic
	{
		public const double SONG_HEIGHT = 20;

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

		public Playlist AllPlaylist
		{ get; private set; }

		public RingCollection<PlaylistItem> SongQueue
		{ get; private set; }

		public int Rating
		{
			get
			{
				return NowPlaying?.LibraryData.Rating ?? 0;
			}
			set
			{
				if (NowPlaying != null)
				{
					NowPlaying.LibraryData.Rating = value;
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

		public PlaylistItem NowPlaying => SongQueue.CurrentValue;

		private bool _previewingRating;

		private Thread _advanceOnFailureThread;

		public MetaMusicLogic(MainWindow ui)
		{
			UI = ui;

			SongQueue = new RingCollection<PlaylistItem>();

			Settings = PlayerSettings.Load();
			Library = SongLibrary.Load();

			Playlist.LoadAllInFolder(Playlist.PLAYLIST_FOLDER, Playlists);
			AllPlaylist = MakeAllPlaylist();
			Playlists.Add(AllPlaylist);
			CurrentPlaylist = Playlists.FirstOrDefault();

			foreach (Playlist pl in Playlists)
			{
				pl.LinkTo(Library);
			}

			Player = new VersatilePlayer(FileMediaPlayer, WebHelper, WavHelper);
			Player.TitleChanged += (s, e) => UpdateTitleBar();
			Player.OnPlayFinished += Player_OnPlayFinished;

			Timer.Tick += Timer_OnTick;
			Timer.Start();

			FillQueueWithPlaylist(CurrentPlaylist);
		}

		private void Player_OnPlayFinished(object sender, EventArgs e)
		{
			Thread thread = new Thread(() => {
				Thread.Sleep(100);
				Dispatcher.Invoke(StopAndAdvance);
				Thread.Sleep(100);
				PlayCurrentSongInQueue();
			});
			thread.Name = "Playback Finish Thread";
			thread.Start();
		}

		private void Timer_OnTick(object sender, EventArgs e)
		{
			if (NowPlaying != null && Player.Source?.Duration != null)
			{
				NowPlaying.LibraryData.LengthTime = Player.Source.Duration.Value;
			}

			UpdateSongUI();
			UpdateVolume();
			UpdateQueueUI();
			UpdatePlaylistUI();

			ILastException ile = Player.Source as ILastException;
			if (ile != null && ile.HasThrown && (_advanceOnFailureThread?.IsAlive ?? true))
			{
				_advanceOnFailureThread = new Thread(() => {
					ile.ResetException();
					Thread.Sleep(5000);
					try
					{
						Dispatcher.Invoke(NextAndPlay);
					}
					catch (TaskCanceledException)
					{ }
				});
				_advanceOnFailureThread.Start();
			}
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

		public void UpdateTitleBar()
		{
			Dispatcher.Invoke(() => {
				UI.TitleTxt.Text = NowPlaying?.LibraryData.DisplayName ?? "Meta Music Player";
			});
		}

		public void UpdatePlayBtn(string desc)
		{
			UI.PlayBtn.ToolTip = desc;
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
			UI.TitleTxt.Text = state.TitleTextTitleBar;
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

		public void UpdateQueueUI()
		{
			ListBoxItem selected = UI.QueueList.SelectedItem as ListBoxItem;

			UI.QueueList.Items.Clear();
			foreach (PlaylistItem pli in SongQueue)
			{
				UI.QueueList.Items.Add(MakeQueueListboxItem(pli));
			}

			if (selected != null)
			{
				UI.QueueList.SelectedIndex = UI.QueueList.Items.IndexOf((obj) => {
					ListBoxItem lbi = obj as ListBoxItem;
					if (lbi != null)
					{
						return lbi.Tag == selected.Tag;
					}

					return false;
				});
			}
		}

		public void UpdatePlaylistUI()
		{
			ListBoxItem selected = UI.PlaylistList.SelectedItem as ListBoxItem;

			UI.PlaylistList.Items.Clear();

			if (CurrentPlaylist == null)
			{
				return;
			}

			for (int i = 0; i < CurrentPlaylist.Songs.Count; i++)
			{
				ListBoxItem lbi = MakePlaylistListboxItem(CurrentPlaylist.Songs[i], i);

				UI.PlaylistList.Items.Add(lbi);
			}

			if (selected != null)
			{
				UI.PlaylistList.SelectedIndex = UI.PlaylistList.Items.IndexOf((obj) => {
					ListBoxItem lbi = obj as ListBoxItem;
					if (lbi != null)
					{
						return lbi.Tag == selected.Tag;
					}

					return false;
				});
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
				IsExpanded = true,
				FontWeight = FontWeights.Normal,
				ContextMenu = MakePlaylistsContextMenu()
			};

			foreach (Playlist playlist in Playlists)
			{
				if (playlist.Name != "All")
				{
					playlistsNode.Items.Add(MakePlaylistNode(playlist));
				}
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
				all.Items.Add(MakeSongNode(song));
			}
			lib.Items.Add(all);

			return lib;
		}

		public TreeViewItem MakePlaylistNode(Playlist playlist)
		{
			TreeViewItem plNode = new TreeViewItem
			{
				Header = playlist.Name,
				Tag = playlist,
				MinHeight = 0,
				ContextMenu = MakeLibraryContextMenu(playlist)
			};

			if (playlist.Name != "All")
			{
				foreach (PlaylistItem pli in playlist.Songs)
				{
					TreeViewItem node = MakeSongNode(pli.LibraryData);
					plNode.Items.Add(node);
				}
			}

			plNode.MouseDoubleClick += (s, e) => {
				PlayPlaylist(playlist);
				plNode.IsSelected = true;
			};
			plNode.ContextMenuOpening += (s, e) => plNode.IsSelected = true;
			return plNode;
		}

		public ListBoxItem MakePlaylistListboxItem(PlaylistItem pli, int idx)
		{
			ListBoxItem lbi = new ListBoxItem {
				Content = (idx + 1).ToString("D3") + ": " + pli.LibraryData.DisplayName,
				Tag = pli,
				Height = SONG_HEIGHT,
				MinHeight = 0,
				FontSize = 12
			};

			if (NowPlaying == pli)
			{
				lbi.FontWeight = FontWeights.SemiBold;
				lbi.FontStyle = FontStyles.Italic;
			}

			lbi.MouseDoubleClick += (s, e) => {
				PlaylistItem tag = (s as ListBoxItem)?.Tag as PlaylistItem;

				if (tag != null)
				{
					if (!SongQueue.Contains(tag))
					{
						SongQueue.Add(tag);
					}

					PlaySongInQueue(tag);
				}
				UpdateSongUI();
			};
			return lbi;
		}

		public ListBoxItem MakeQueueListboxItem(PlaylistItem pli)
		{
			ListBoxItem lbi = new ListBoxItem
			{
				Content = pli.LibraryData.DisplayName,
				Tag = pli,
				Height = SONG_HEIGHT,
				MinHeight = 0,
				FontSize = 12
			};

			if (NowPlaying == pli)
			{
				lbi.FontWeight = FontWeights.SemiBold;
				lbi.FontStyle = FontStyles.Italic;
			}

			lbi.MouseDoubleClick += (s, e) => {
				PlaylistItem tag = (s as ListBoxItem)?.Tag as PlaylistItem;

				if (tag != null)
				{
					PlaySongInQueue(tag);
				}
				UpdateSongUI();
			};
			return lbi;
		}

		public ContextMenu MakeLibraryContextMenu(Playlist list)
		{
			ContextMenu res = new ContextMenu();

			MenuItem playPlaylist = new MenuItem { Header = "Play Playlist" };
			playPlaylist.Click += (s, e) => PlayPlaylist(list);
			res.Items.Add(playPlaylist);

			MenuItem addSong = new MenuItem { Header = "Add Songs..." };
			addSong.Click += (s, e) => AddSongsToPlaylist(list);
			res.Items.Add(addSong);

			return res;
		}

		public TreeViewItem MakeSongNode(LibraryItem song)
		{
			PlaylistItem pli = song.MakePlaylistItem();

			TreeViewItem songNode = new TreeViewItem
			{
				Header = song.DisplayName,
				ToolTip = song.Source,
				Tag = song,
				ContextMenu = MakeLibraryContextMenu(pli),
				Height = SONG_HEIGHT,
				MinHeight = 0
			};
			songNode.ContextMenuOpening += (s, e) => songNode.IsSelected = true;

			songNode.MouseDoubleClick += (s, e) => {
				if (CurrentPlaylist != null && SongQueue.Contains(pli))
				{
					PlaySongInQueue(pli);
				}
				else
				{
					PlaySongClearQueue(pli);
				}
				songNode.IsSelected = true;
			};

			return songNode;
		}

		public ContextMenu MakeLibraryContextMenu(PlaylistItem pli)
		{
			ContextMenu res = new ContextMenu();

			MenuItem addToQueue = new MenuItem { Header = "Add to Queue" };
			addToQueue.Click += (s, e) => {
				SongQueue.Add(pli);
				if (Player.ActivePlayer == null)
				{
					PlayCurrentSongInQueue();
				}
			};
			res.Items.Add(addToQueue);

			MenuItem playNow = new MenuItem { Header = "Play Now" };
			playNow.Click += (s, e) => PlaySongClearQueue(pli);
			res.Items.Add(playNow);

			return res;
		}

		public ContextMenu MakePlaylistsContextMenu()
		{
			ContextMenu res = new ContextMenu();
			MenuItem create = new MenuItem { Header = "Create Playlist..." };
			create.Click += (s, e) => {
				GetTextWindow nameWin = new GetTextWindow("Create Playlist", "Playlist Name:", 
					_s => _s.ToLower() != "all");
				if (nameWin.ShowDialog() == true)
				{
					Playlist added = CreateNewPlaylist(nameWin.ResultText);
					AddSongsToPlaylist(added);
				}
			};
			res.Items.Add(create);

			return res;
		}

		public Playlist CreateNewPlaylist(string name)
		{
			Playlist added = new Playlist(name);
			Playlists.Add(added);

			UpdatePlaylistUI();

			return added;
		}

		public void AddSongsToPlaylist(Playlist list)
		{
			AddSongWindow addWin = new AddSongWindow();
			if (addWin.ShowDialog() == true)
			{
				foreach (string song in addWin.AddedSongs)
				{
					PlaylistItem pli = new PlaylistItem
					{
						Source = song,
						LibraryData = new LibraryItem { Source = song }
					};
					list.Songs.Add(pli);

					if (list == CurrentPlaylist)
					{
						SongQueue.Add(pli);
					}

					if (!Library.Contains(pli.LibraryData))
					{
						Library.Add(pli.LibraryData);
						AllPlaylist.Songs.Add(pli);
					}
				}

				list.Save();
				Library.Save();
			}
		}

		public Playlist MakeAllPlaylist()
		{
			Playlist all = new Playlist("All");
			foreach (LibraryItem lbi in Library)
			{
				all.Songs.Add(new PlaylistItem {
					LibraryData = lbi,
					Source = lbi.Source
				});
			}

			return all;
		}

		public void PlaySongClearQueue(PlaylistItem song)
		{
			if (Player.Source != null)
			{
				Player.Stop();
			}

			Thread thread = new Thread(() => {
				Thread.Sleep(100);
				Dispatcher.Invoke(() => {
					SongQueue.Clear();
					SongQueue.Add(song);
					Player.LoadAndPlay(NowPlaying.LibraryData.Source);
					UpdatePlayBtn("Pause");
					UpdateSongUI();
				});
			});
			thread.Name = "Advance Track Thread";
			thread.Start();
		}

		public void PlaySongInQueue(PlaylistItem song)
		{
			if (Player.Source != null)
			{
				Player.Stop();
			}

			Thread thread = new Thread(() => {
				Thread.Sleep(100);
				Dispatcher.Invoke(() => {
					bool found = SongQueue.MoveTo(song);
					if (found)
					{
						Player.LoadAndPlay(NowPlaying.LibraryData.Source);
						UpdatePlayBtn("Pause");
						UpdateSongUI();
					}
				});
			});
			thread.Name = "Advance Track Thread";
			thread.Start();
		}

		public void PlayPlaylist(Playlist playlist)
		{
			CurrentPlaylist = playlist;

			if (Player.Source != null)
			{
				Player.Stop();
			}

			Thread thread = new Thread(() => {
				Thread.Sleep(100);
				Dispatcher.Invoke(() => {
					SongQueue.Clear();
					SongQueue.AddRange(CurrentPlaylist.Songs);
					SongQueue.MoveTo(CurrentPlaylist.CurrentSong);
					Player.LoadAndPlay(NowPlaying.LibraryData.Source);
					UpdatePlayBtn("Pause");
					UpdateSongUI();
				});
			});
			thread.Name = "Start Playlist Thread";
			thread.Start();

			Settings.LastPlayedPlaylist = playlist.Name;
		}

		public void FillQueueWithPlaylist(Playlist playlist)
		{
			CurrentPlaylist = playlist;

			if (Player.Source != null)
			{
				Player.Stop();
			}

			Thread thread = new Thread(() => {
				Thread.Sleep(100);
				Dispatcher.Invoke(() => {
					SongQueue.Clear();
					SongQueue.AddRange(CurrentPlaylist.Songs);
					SongQueue.MoveTo(CurrentPlaylist.CurrentSong);
					UpdateSongUI();
				});
			});
			thread.Name = "Start Playlist Thread";
			thread.Start();

			Settings.LastPlayedPlaylist = playlist.Name;
		}

		public void NextAndPlay()
		{
			if (Player.Source != null)
			{
				Player.Stop();
			}

			Thread thread = new Thread(() => {
				Thread.Sleep(100);
				Dispatcher.Invoke(() => {
					SongQueue.MoveNext();
					if (SongQueue.CurrentValue != null)
					{
						Player.LoadAndPlay(NowPlaying.LibraryData.Source);
						UpdatePlayBtn("Pause");
						UpdateSongUI();

						if (CurrentPlaylist != null)
						{
							CurrentPlaylist.CurrentSongIndex = CurrentPlaylist.Songs.IndexOf(NowPlaying);
						}
					}
				});
			});
			thread.Name = "Advance Track Thread";
			thread.Start();
		}

		public void PrevAndPlay()
		{
			if (Player.Source != null)
			{
				Player.Stop();
			}

			Thread thread = new Thread(() => {
				Thread.Sleep(100);
				Dispatcher.Invoke(() => {
					SongQueue.MovePrevious();
					if (SongQueue.CurrentValue != null)
					{
						Player.LoadAndPlay(NowPlaying.LibraryData.Source);
						UpdatePlayBtn("Pause");
						UpdateSongUI();

						if (CurrentPlaylist != null)
						{
							CurrentPlaylist.CurrentSongIndex = CurrentPlaylist.Songs.IndexOf(NowPlaying);
						}
					}
				});
			});
			thread.Name = "Advance Track Thread";
			thread.Start();
		}

		public void PlayCurrentSongInQueue()
		{
			if (Player.Source != null)
			{
				Player.Stop();
			}

			if (NowPlaying?.LibraryData != null)
			{
				Thread thread = new Thread(() => {
					Thread.Sleep(100);
					Dispatcher.Invoke(() => {
						Player.LoadAndPlay(NowPlaying.LibraryData.Source);
						UpdatePlayBtn("Pause");
						UpdateSongUI();

						if (CurrentPlaylist != null)
						{
							CurrentPlaylist.CurrentSongIndex = CurrentPlaylist.Songs.IndexOf(NowPlaying);
						}
					});
				});
				thread.Name = "Advance Track Thread";
				thread.Start();
			}
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

		public void TogglePlay()
		{
			if (Player.Source == null)
			{
				Player.LoadAndPlay(NowPlaying.LibraryData.Source);
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

			if (SongQueue.Count == 1)
			{
				SongQueue.RemoveCurrent();
			}

			UpdateTitleBar();
			UpdatePlayBtn("Play");
		}

		public void StopAndAdvance()
		{
			Player.Stop();

			if (SongQueue.Count == 1)
			{
				SongQueue.RemoveCurrent();
			}
			else
			{
				SongQueue.MoveNext();

				if (CurrentPlaylist != null)
				{
					CurrentPlaylist.CurrentSongIndex = CurrentPlaylist.Songs.IndexOf(NowPlaying);
				}
			}

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