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

namespace MetaMusic
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	
	// ReSharper disable once RedundantExtendsListEntry
	public partial class MainWindow : MetroWindow
	{
		public const string MINIMALIST = "\x25be";
		public const string NORMAL_WINDOW = "\x25c6";
		public const string PREV = "\x00ab";
		public const string NEXT = "\x00bb";
		public const string PAUSE = "\x23f8";
		public const string PLAY = "\x25b6";
		public const string STOP = "\x25a0";
		public const string SPEAKER_ON = "\uD83D\uDD08";
		public const string SPEAKER_OFF = "\uD83D\uDD07";

		public const int REDUCE_PROGRESS = 850;
		public const int HIDE_PROGRESS = 700;
		public const int HIDE_VOLUME = 550;
		public const int HIDE_SONGNAME = 450;
		public const int HIDE_CONTROLS = 320;

		public const int PROGRESS_LARGE = 200;
		public const int PROGRESS_SMALL = 100;

		//private FlashWindowHelper _flashHelper = new FlashWindowHelper();

		public MetaMusicLogic Logic
		{ get; private set; }

		public readonly DispatcherTimer DebugTimer;

		public bool IsMinimalist
		{ get; private set; }

		public double LastHeight
		{ get; private set; }

		private readonly List<string> debugLines = new List<string>();

		private bool _playMode;

		private bool _muted;

		public MainWindow()
		{
			InitializeComponent();

			Logic = new MetaMusicLogic(this);

			DebugTimer = new DispatcherTimer {
				Interval = TimeSpan.FromSeconds(0.02)
			};
			DebugTimer.Tick += (s, e) => {
				getDebugLines();
				DebugTxt.Text = string.Join("\n", debugLines);
				MMTxt.ToolTip = string.Join("\n", debugLines);
			};
		}

		internal void getDebugLines()
		{
			debugLines.Clear();

			debugLines.Add("Volume: " + Min_VolumeSlider.Value.ToString("F2"));
			debugLines.Add("Width: " + Width);
		}

		public void ToggleMinimalist()
		{
			IsMinimalist = !IsMinimalist;

			if (IsMinimalist)
			{
				LastHeight = Height;
			}

			MaxHeight = IsMinimalist ? 39.0 : double.PositiveInfinity;
			Height = IsMinimalist ? 39.0 : LastHeight;
			MinimalistBtn.Content = IsMinimalist ? NORMAL_WINDOW : MINIMALIST;
			IsMaxRestoreButtonEnabled = !IsMinimalist;

			MainGrid.Visibility = (!IsMinimalist).ToVis();
			Min_FlowControlPanel.Visibility = IsMinimalist.ToVis();
			Min_VolumeGrid.Visibility = IsMinimalist.ToVis();
			Min_ProgressSlider.Visibility = IsMinimalist.ToVis();

			UpdateMinimalistControlsVisibility(Width);
		}

		public void UpdateMinimalistControlsVisibility(double width)
		{
			Min_ProgressSlider.Width = width > REDUCE_PROGRESS ? PROGRESS_LARGE : PROGRESS_SMALL;

			if (IsMinimalist)
			{
				Min_ProgressSlider.Visibility = (width > HIDE_PROGRESS).ToVis();
				Min_VolumeGrid.Visibility = (width > HIDE_VOLUME).ToVis();
				Title = width > HIDE_SONGNAME ? "Captain-Cool.mp3" : "";
				Min_FlowControlPanel.Visibility = (width > HIDE_CONTROLS).ToVis();
			}
		}

		private void PlayBtn_OnClick(object sender, RoutedEventArgs e)
		{
			if (!Logic.Timer.IsEnabled)
			{
				Logic.Timer.Start();
			}

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
			DebugTimer.Stop();
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			Logic.UpdatePlayBtn(_playMode ? "Pause" : "Play");
			DebugTimer.Start();
		}

		private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (!e.WidthChanged)
			{
				return;
			}

			double width = e.NewSize.Width;

			UpdateMinimalistControlsVisibility(width);
		}

		private void PrevThumbBtn_OnClick(object sender, EventArgs e)
		{ }

		private void NextThumbBtn_OnClick(object sender, EventArgs e)
		{ }

		private void MinimalistBtn_OnClick(object sender, RoutedEventArgs e)
		{
			ToggleMinimalist();
		}

		private void Min_PrevTrackBtn_OnClick(object sender, RoutedEventArgs e)
		{ }

		private void Min_PlayBtn_OnClick(object sender, RoutedEventArgs e)
		{
			_playMode = !_playMode;
			Logic.UpdatePlayBtn(_playMode ? "Pause" : "Play");
		}

		private void Min_StopBtn_OnClick(object sender, RoutedEventArgs e)
		{ }

		private void Min_NextTrackBtn_OnClick(object sender, RoutedEventArgs e)
		{ }

		private void Min_VolumeBtn_OnClick(object sender, RoutedEventArgs e)
		{
			_muted = !_muted;
			Min_VolumeBtn.Content = _muted ? SPEAKER_OFF : SPEAKER_ON;
			Min_VolumeBtn.Margin = new Thickness { Right = _muted ? 5 : 16, Left = _muted ? 0 : 2, Bottom = 3 };
		}
	}
}
