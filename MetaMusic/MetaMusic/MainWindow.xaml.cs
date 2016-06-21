using System;
using System.Collections.Generic;
using System.IO;
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

using MetaMusic.Players;

using UltimateUtil;
using IOPath = System.IO.Path;

namespace MetaMusic
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	
	// ReSharper disable once RedundantExtendsListEntry
	public partial class MainWindow : Window
	{
		public readonly MediaPlayer Player = new MediaPlayer();

		public readonly DispatcherTimer Timer = new DispatcherTimer() {
			Interval = TimeSpan.FromSeconds(0.5)
		};

		public readonly FilePlayer FilePlayer;

		private FlashWindowHelper _flashHelper;

		public MainWindow()
		{
			InitializeComponent();

			FilePlayer = new FilePlayer(Player);

			Timer.Tick += (s, e) => {
				if (!File.Exists(MusicPathBox.Text) && FilePlayer.Source == null)
				{
					StatusTxt.Text = "File does not exist.";
				}
				else
				{
					StatusTxt.Text = FilePlayer.Source.GetDurationString();
				}
			};

			Timer.Start();
		}

		public void UpdateTitleBar()
		{
			if (FilePlayer.Source != null)
			{
				Title = "MM | " + IOPath.GetFileName(FilePlayer.Source.FilePath);
			}
			else
			{
				Title = "Meta Music Player";
			}
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
			if (FilePlayer.Source == null)
			{
				if (File.Exists(MusicPathBox.Text))
				{
					FilePlayer.Play(MusicPathBox.Text);
					UpdateTitleBar();
					UpdatePlayBtn("Pause");
				}
			}
			else
			{
				FilePlayer.TogglePause();
				UpdatePlayBtn(FilePlayer.Source.IsPlaying ? "Pause" : "Resume");
			}

		}

		private void StopBtn_OnClick(object sender, RoutedEventArgs e)
		{
			FilePlayer.Stop();
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
	}
}
