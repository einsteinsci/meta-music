using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using MetaMusic.Players;

namespace MetaMusic.Sources
{
	public sealed class FileMusic : IMusicSource
	{
		public string FilePath
		{ get; private set; }

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

		public TimeSpan Position => _player.Position;

		public bool IsPlaying
		{ get; set; }

		private readonly MediaPlayer _player;

		public FileMusic(string path, MediaPlayer player)
		{
			FilePath = path;
			_player = player;
			_player.Open(new Uri(FilePath));
		}
		internal FileMusic()
		{ }

		public void Open(string path = null)
		{
			if (path != null)
			{
				FilePath = path;
			}

			if (path != null) // still
			{
				_player.Open(new Uri(FilePath));
			}
		}

		public void Play()
		{
			if (FilePath == null)
			{
				throw new InvalidOperationException("FilePath is null.");
			}

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