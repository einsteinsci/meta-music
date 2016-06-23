using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using MetaMusic.Sources;

namespace MetaMusic.Players
{
	public class VersatilePlayer : IMusicPlayer<IMusicSource>
	{
		public FilePlayer StandardFilePlayer
		{ get; private set; }

		public SoundCloudPlayer SoundCloudPlayer
		{ get; private set; }

		public IMusicPlayer ActivePlayer
		{ get; private set; }

		public IMusicSource Source
		{ get; private set; }

		public event EventHandler TitleChanged;

		public string StatusText
		{
			get
			{
				if (ActivePlayer == null)
				{
					return "Enter a URL to play.";
				}

				if (ActivePlayer is SoundCloudPlayer && SoundCloudPlayer.LoadingText != null)
				{
					return SoundCloudPlayer.LoadingText;
				}

				return Source.GetDurationString();
			}
		}

		public VersatilePlayer(MediaPlayer mediaPlayer, WebMusicHelper webHelper)
		{
			StandardFilePlayer = new FilePlayer(mediaPlayer);
			SoundCloudPlayer = new SoundCloudPlayer(webHelper);
		}

		protected IMusicPlayer getMatchingPlayer(Type sourceType)
		{
			if (sourceType == typeof(FileMusic))
			{
				return StandardFilePlayer;
			}

			if (sourceType == typeof(SoundCloudMusic))
			{
				return SoundCloudPlayer;
			}

			return null;
		}

		public void Play(IMusicSource source)
		{
			if (ActivePlayer == null)
			{
				Source = source;

				ActivePlayer = getMatchingPlayer(Source.GetType());
				ActivePlayer.PlaySrc(Source);

				if (ActivePlayer == SoundCloudPlayer)
				{
					SoundCloudPlayer.Source.TitleChanged += (s, e) => { TitleChanged?.Invoke(s, e); };
				}
				else if (ActivePlayer == StandardFilePlayer)
				{
					TitleChanged?.Invoke(this, new EventArgs());
				}
			}
			else
			{
				ActivePlayer.Resume();
			}
		}

		void IMusicPlayer.PlaySrc(IMusicSource src)
		{
			Play(src);
		}

		public void Resume()
		{
			ActivePlayer?.Resume();
		}

		public void Pause()
		{
			ActivePlayer?.Pause();
		}

		public void TogglePause()
		{
			ActivePlayer?.TogglePause();
		}

		public void Stop()
		{
			ActivePlayer?.Stop();

			ActivePlayer = null;
			Source = null;
		}
	}
}