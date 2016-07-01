﻿using System;
using System.Collections.Generic;
using System.IO;
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

		public string Title => Path.GetFileName(FilePath);

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
		}

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