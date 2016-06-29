using System;
using System.Collections.Generic;
using System.Diagnostics;
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

using MahApps.Metro;
using MahApps.Metro.Controls;

namespace MetaMusic
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	
	// ReSharper disable once RedundantExtendsListEntry
	public partial class MainWindow : MetroWindow
	{
		public const string MINIMALIST = "\u25be";
		public const string NORMAL_WINDOW = "\u25c6";
		public const string PREV = "\u00ab";
		public const string NEXT = "\u00bb";
		public const string PAUSE = "\u23f8";
		public const string PLAY = "\u25b6";
		public const string STOP = "\u25a0";
		public const string SPEAKER_ON = "\ud83d\udd08";
		public const string SPEAKER_OFF = "\ud83d\udd07";
		public const string STAR_ON = "\u2605";
		public const string STAR_OFF = "\u2606";
		public const string SHUFFLE = "\ud83d\udd00";
		public const string LOOP = "\u221e";
		public const string OPEN = "\u23cf";

		public const int REDUCE_PROGRESS = 850;
		public const int HIDE_PROGRESS = 700;
		public const int HIDE_VOLUME = 550;
		public const int HIDE_SONGNAME = 450;
		public const int HIDE_CONTROLS = 285;

		public const int PROGRESS_LARGE = 200;
		public const int PROGRESS_SMALL = 100;

		public const int MIN_MINWIDTH = 185;
		public const int FULL_MINWIDTH = 710;

		//private FlashWindowHelper _flashHelper = new FlashWindowHelper();

		public MetaMusicLogic Logic
		{ get; private set; }

		public readonly DispatcherTimer DebugTimer;

		public bool IsMinimalist
		{ get; private set; }

		public double LastHeight
		{ get; private set; }

		//public static readonly DependencyProperty ColorsDepProperty = DependencyProperty.Register("Colors",
		//	typeof(List<KeyValuePair<string, Color>>), typeof(MainWindow),
		//	new PropertyMetadata(new List<KeyValuePair<string, Color>>()));
		//
		//public List<KeyValuePair<string, Color>> AccentColors
		//{
		//	get
		//	{
		//		return (List<KeyValuePair<string, Color>>)GetValue(ColorsDepProperty);
		//	}
		//	set
		//	{
		//		SetValue(ColorsDepProperty, value);
		//	}
		//}

		private readonly List<string> debugLines = new List<string>();

		private bool _playMode;

		private bool _windowLoaded;

		public MainWindow()
		{
			InitializeComponent();

			RenderOptions.SetBitmapScalingMode(SoundCloudLogoBtn, BitmapScalingMode.NearestNeighbor);

			Logic = new MetaMusicLogic(this);

			DebugTimer = new DispatcherTimer {
				Interval = TimeSpan.FromSeconds(0.02)
			};
			DebugTimer.Tick += (s, e) => {
				getDebugLines();
				TitleBtn.ToolTip = string.Join("\n", debugLines);
			};

			//AccentColors = (from prop in typeof(Colors).GetProperties()
			//	where typeof(Color).IsAssignableFrom(prop.PropertyType)
			//	select new KeyValuePair<string, Color>(prop.Name, (Color)prop.GetValue(null))).ToList();
			

			Tuple<AppTheme, Accent> theme = ThemeManager.DetectAppStyle(Application.Current);
			ThemeManager.ChangeAppStyle(this, theme.Item2, theme.Item1);
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

			MinimalistMenuItem.IsChecked = IsMinimalist;

			MaxHeight = IsMinimalist ? 39.0 : double.PositiveInfinity;
			Height = IsMinimalist ? 39.0 : LastHeight;
			MinimalistBtn.Content = IsMinimalist ? NORMAL_WINDOW : MINIMALIST;
			IsMaxRestoreButtonEnabled = !IsMinimalist;
			Topmost = IsMinimalist;
			MinWidth = IsMinimalist ? MIN_MINWIDTH : FULL_MINWIDTH;

			MainGrid.Visibility = (!IsMinimalist).ToVis();
			Min_FlowControlPanel.Visibility = IsMinimalist.ToVis();
			Min_VolumeGrid.Visibility = IsMinimalist.ToVis();
			Min_ProgressSlider.Visibility = IsMinimalist.ToVis();
			Min_ProgressSeparator.Visibility = IsMinimalist.ToVis();

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
				Min_ProgressSeparator.Visibility = (width > HIDE_SONGNAME).ToVis();
				Min_FlowControlPanel.Visibility = (width > HIDE_CONTROLS).ToVis();
			}
			else
			{
				Title = "Captain-Cool.mp3";
			}
		}

		public static void ChangeAppTheme(string theme)
		{
			ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(theme),
				ThemeManager.DetectAppStyle().Item1);
		}

		private void MainWindow_OnClosed(object sender, EventArgs e)
		{
			Logic.ShutDown();
			DebugTimer.Stop();
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			_windowLoaded = true;

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

		private void TitleBtn_OnClick(object sender, RoutedEventArgs e)
		{
			TitleBtn.ContextMenu.IsOpen = true;
		}

		private void PrevThumbBtn_OnClick(object sender, EventArgs e)
		{
			PrevTrackBtn_OnClick(sender, null);
		}

		private void PlayThumbBtn_OnClick(object sender, EventArgs e)
		{
			PlayBtn_OnClick(sender, null);
		}

		private void StopThumbBtn_OnClick(object sender, EventArgs e)
		{
			StopBtn_OnClick(sender, null);
		}

		private void NextThumbBtn_OnClick(object sender, EventArgs e)
		{
			NextTrackBtn_OnClick(sender, null);
		}

		private void MinThumbBtn_OnClick(object sender, EventArgs e)
		{
			MinimalistBtn_OnClick(sender, null);
		}

		private void MinimalistBtn_OnClick(object sender, RoutedEventArgs e)
		{
			ToggleMinimalist();
		}

		private void PrevTrackBtn_OnClick(object sender, RoutedEventArgs e)
		{ }

		private void PlayBtn_OnClick(object sender, RoutedEventArgs e)
		{
			//if (!Logic.Timer.IsEnabled)
			//{
			//	Logic.Timer.Start();
			//}
			//
			//Logic.TogglePlay();

			_playMode = !_playMode;
			Logic.UpdatePlayBtn(_playMode ? "Pause" : "Play");
		}

		private void StopBtn_OnClick(object sender, RoutedEventArgs e)
		{
			Logic.Stop();
		}

		private void NextTrackBtn_OnClick(object sender, RoutedEventArgs e)
		{ }

		private void VolumeBtn_OnClick(object sender, RoutedEventArgs e)
		{
			Logic.ToggleMute();

		}

		private void Rating1Btn_OnClick(object sender, RoutedEventArgs e)
		{
			Logic.SetRating(1);
			Logic.PreviewRating(0);
		}

		private void Rating2Btn_OnClick(object sender, RoutedEventArgs e)
		{
			Logic.SetRating(2);
			Logic.PreviewRating(0);
		}

		private void Rating3Btn_OnClick(object sender, RoutedEventArgs e)
		{
			Logic.SetRating(3);
			Logic.PreviewRating(0);
		}

		private void Rating4Btn_OnClick(object sender, RoutedEventArgs e)
		{
			Logic.SetRating(4);
			Logic.PreviewRating(0);
		}

		private void Rating5Btn_OnClick(object sender, RoutedEventArgs e)
		{
			Logic.SetRating(5);
			Logic.PreviewRating(0);
		}

		private void Rating1Btn_OnMouseEnter(object sender, MouseEventArgs e)
		{
			Logic.PreviewRating(1);
		}

		private void Rating2Btn_OnMouseEnter(object sender, MouseEventArgs e)
		{
			Logic.PreviewRating(2);
		}

		private void Rating3Btn_OnMouseEnter(object sender, MouseEventArgs e)
		{
			Logic.PreviewRating(3);
		}

		private void Rating4Btn_OnMouseEnter(object sender, MouseEventArgs e)
		{
			Logic.PreviewRating(4);
		}

		private void Rating5Btn_OnMouseEnter(object sender, MouseEventArgs e)
		{
			Logic.PreviewRating(5);
		}

		private void RatingBtns_OnMouseLeave(object sender, MouseEventArgs e)
		{
			Logic.PreviewRating(0);
		}

		private void VolumeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_windowLoaded)
			{
				return;
			}

			Logic.Volume = (float)e.NewValue;
			Logic.UpdateVolume();
		}

		private void SoundCloudLogoBtn_OnMouseEnter(object sender, MouseEventArgs e)
		{
			SoundCloudLogoBtn.Opacity = 1.0;
		}

		private void SoundCloudLogoBtn_OnMouseLeave(object sender, MouseEventArgs e)
		{
			SoundCloudLogoBtn.Opacity = 0.7;
		}

		private void SoundCloudLogoBtn_OnClick(object sender, RoutedEventArgs e)
		{
			Process.Start("https://soundcloud.com");
		}

		private void AccentColorMenu_OnClick(object senderObj, RoutedEventArgs e)
		{
			MenuItem sender = senderObj as MenuItem;
			if (senderObj == null)
			{
				return;
			}

			string theme = sender.Header as string;
			ChangeAppTheme(theme);
			
			foreach (object child in ThemeMenuRoot.Items)
			{
				MenuItem colorItem = child as MenuItem;

				if (colorItem != null && colorItem.Header != sender.Header)
				{
					colorItem.IsChecked = false;
				}
			}
			
		}
	}
}
