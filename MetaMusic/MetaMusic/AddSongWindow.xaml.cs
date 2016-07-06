using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

using MahApps.Metro.Controls;

using Ookii.Dialogs.Wpf;

using UltimateUtil;

namespace MetaMusic
{
	/// <summary>
	/// Interaction logic for AddSongWindow.xaml
	/// </summary>
	public partial class AddSongWindow : MetroWindow
	{
		public List<string> AddedSongs
		{ get; private set; }

		public static int LastFilterIndex
		{ get; private set; }

		public AddSongWindow()
		{
			InitializeComponent();

			AddedSongs = new List<string>();
		}

		public void UpdateSongsList()
		{
			string selected = SongsList.SelectedItem as string;

			SongsList.Items.Clear();
			foreach (string song in AddedSongs)
			{
				SongsList.Items.Add(song);
			}

			int idx = SongsList.Items.IndexOf(obj => obj as string == selected);
			SongsList.SelectedIndex = idx;
		}

		private void AddFileBtn_OnClick(object sender, RoutedEventArgs e)
		{
			VistaOpenFileDialog ofd = new VistaOpenFileDialog {
				Multiselect = true,
				Title = "Select File(s)",
				DefaultExt = ".mp3",
				Filter = "All Supported Files|*.mp3;*.brstm|MPEG-3 (.mp3)|*.mp3|Brawl Looped Stream (.brstm)|*.brstm",
				FilterIndex = LastFilterIndex
			};

			if (ofd.ShowDialog() == true)
			{
				foreach (string fileName in ofd.FileNames)
				{
					if (!AddedSongs.Contains(fileName))
					{
						AddedSongs.Add(fileName);
					}
				}

				UpdateSongsList();
			}

			LastFilterIndex = ofd.FilterIndex;
		}

		private void OKBtn_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				DialogResult = true;
			}
			catch (InvalidOperationException)
			{ }

			Close();
		}

		private void CancelBtn_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				DialogResult = false;
			}
			catch (InvalidOperationException)
			{ }

			Close();
		}

		private void AddSoundcloudBtn_OnClick(object sender, RoutedEventArgs e)
		{
			GetTextWindow gtw = new GetTextWindow("Add SoundCloud Song", "SoundCloud URL:", url =>
				url.ToLower().StartsWithAny("http://soundcloud.com", "https://soundcloud.com"));

			if (gtw.ShowDialog() == true)
			{
				if (!AddedSongs.Contains(gtw.ResultText))
				{
					AddedSongs.Add(gtw.ResultText);
				}

				UpdateSongsList();
			}
		}

		private void RemoveBtn_OnClick(object sender, RoutedEventArgs e)
		{
			string sel = SongsList.SelectedItem as string;
			if (sel != null)
			{
				SongsList.Items.Remove(sel);
				AddedSongs.Remove(sel);
			}
		}

		private void SongsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (RemoveBtn != null)
			{
				RemoveBtn.IsEnabled = SongsList.SelectedItem != null;
			}
		}
	}
}
