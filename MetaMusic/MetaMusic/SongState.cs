using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using MetaMusic.Sources;

using UltimateUtil;

namespace MetaMusic
{
	public class SongState
	{
		public string TitleText
		{ get; set; }

		public string TitleTextTitleBar
		{
			get
			{
				if (TitleText.IsNullOrEmpty() || TitleText == "-")
				{
					return "Meta Music Player";
				}

				return TitleText;
			}
		}

		public string TimeText
		{ get; set; }

		public double ProgressValue
		{ get; set; }

		public bool ProgressMeaningful
		{ get; set; }

		public string SoundCloudUrl
		{ get; set; }

		public string LoadingText
		{ get; set; }

		public ImageSource Artwork
		{ get; set; }

		public bool ShowLoadingMessage => !LoadingText.IsNullOrEmpty();

		public bool IsSoundCloud => SoundCloudUrl != null;

		public SongState()
		{
			TitleText = "-";
			TimeText = "-:-- / -:--";
		}

		public static SongState Load(IMusicSource song, PlaylistItem nowPlaying)
		{
			SongState res = new SongState();

			if (song == null && nowPlaying != null)
			{
				res.TitleText = nowPlaying.Song.DisplayName;
				return res;
			}

			if (song != null)
			{
				res.TitleText = song.Title ?? "";
				res.TimeText = song.GetDurationString();
				
				SoundCloudMusic sc = song as SoundCloudMusic;
				if (sc != null)
				{
					res.SoundCloudUrl = sc.PermanentURL;
				}

				if (song.Duration != null)
				{
					res.ProgressValue = song.Position.TotalSeconds / song.Duration.Value.TotalSeconds;
				}

				res.ProgressMeaningful = song.Duration != null;

				if (!song.Author.IsNullOrEmpty())
				{
					res.TitleText = song.Author + " - " + res.TitleText;
				}

				if (song.CoverArtData != null)
				{
					res.Artwork = Util.LoadImageFromBytes(song.CoverArtData);
				}
			}

			ILoadingText ilt = song as ILoadingText;
			if (ilt != null)
			{
				res.LoadingText = ilt.LoadingText;
			}

			return res;
		}
	}
}