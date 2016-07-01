using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using MetaMusic.Sources;

namespace MetaMusic.Players
{
	[MusicPlayer]
	public class FilePlayer : MusicPlayerBase<FileMusic>
	{
		private readonly MediaPlayer _player;

		public override bool Muted
		{
			get
			{
				return _muted;
			}
			set
			{
				_muted = value;
				_player.Volume = _muted ? 0 : _bufferVolume;
			}
		}
		private bool _muted;

		private double _bufferVolume;

		public override string RegistryName => nameof(FilePlayer);

		public FilePlayer(MediaPlayer player)
		{
			_player = player;
			_bufferVolume = 1.0;
		}

		public void Play(string path)
		{
			Source = new FileMusic(path, _player);
			Source.Play();
		}

		public override bool CanPlay(string sourceUri)
		{
			return sourceUri.ToLower().EndsWith("mp3");
		}

		public override IMusicSource MakeSource(string source)
		{
			return new FileMusic(source, _player);
		}

		public override double GetVolume(PlayerSettings settings)
		{
			return settings.VolumeFile;
		}

		public override void SetVolume(PlayerSettings settings, double volume)
		{
			settings.VolumeFile = volume;

			_player.Volume = volume;
			_bufferVolume = volume;
		}
	}
}