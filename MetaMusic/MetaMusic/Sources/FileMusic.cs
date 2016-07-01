using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using Id3;
using Id3.Frames;

using MetaMusic.Players;

namespace MetaMusic.Sources
{
	public sealed class FileMusic : IMusicSource
	{
		public string FilePath
		{ get; private set; }

		public string Title => _title ?? TitlePath;

		public string TitlePath => Path.GetFileName(FilePath);

		public string Author
		{ get; private set; }

		public string Album
		{ get; private set; }

		public byte[] CoverArtData
		{ get; private set; }

		private string _title;

		public TimeSpan? Duration
		{
			get
			{
				if (_player.NaturalDuration.HasTimeSpan)
				{
					return _player.NaturalDuration.TimeSpan;
				}

				return null;
			}
		}

		public TimeSpan Position
		{
			get
			{
				return _player.Position;
			}
			set
			{
				_player.Position = value;
			}
		}	

		public bool IsPlaying
		{ get; set; }

		private readonly MediaPlayer _player;

		public FileMusic(string path, MediaPlayer player)
		{
			FilePath = path;
			_player = player;
			_player.Open(new Uri(FilePath));
			LoadMetadata();
		}

		public void Open(string path = null)
		{
			if (path != null)
			{
				FilePath = path;
			}

			if (FilePath != null) // still
			{
				_player.Open(new Uri(FilePath));
				LoadMetadata();
			}
		}

		public void LoadMetadata()
		{
			if (!File.Exists(FilePath))
			{
				Author = "???";
				return;
			}

			Mp3File file = new Mp3File(FilePath);
			if (file.HasTags)
			{
				Id3Tag tag = file.GetTag(Id3TagFamily.FileStartTag);
				Author = tag.Artists.Values.FirstOrDefault();
				Album = tag.Album.Value;
				_title = tag.Title;

				PictureFrame frame = tag.Pictures.FirstOrDefault();
				if (frame != null)
				{
					CoverArtData = frame.PictureData;
				}
			}
		}

		public void Play()
		{
			if (_player.Source == null)
			{
				throw new InvalidOperationException("MediaPlayer.Source is null.");
			}

			_player.Play();
			IsPlaying = true;
		}

		public void Pause()
		{
			_player.Pause();
			IsPlaying = false;
		}

		public void Stop()
		{
			_player.Stop();
			FilePath = null;
			IsPlaying = false;
		}
	}
}