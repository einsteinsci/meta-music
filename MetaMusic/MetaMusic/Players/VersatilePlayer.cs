using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using MetaMusic.Sources;

namespace MetaMusic.Players
{
	public class VersatilePlayer : IMusicPlayer<IMusicSource>, ILoadingText
	{
		public FilePlayer StandardFilePlayer
		{ get; private set; }

		public SoundCloudPlayer SoundCloudPlayer
		{ get; private set; }

		public BrstmPlayer BrstmPlayer
		{ get; private set; }

		public IMusicPlayer ActivePlayer
		{ get; private set; }

		public IMusicSource Source
		{ get; private set; }

		public event EventHandler TitleChanged;

		public string LoadingText
		{
			get
			{
				if (ActivePlayer == null)
				{
					return "Enter a URL to play.";
				}

				ILoadingText ilt = ActivePlayer as ILoadingText;
				if (ilt?.LoadingText != null)
				{
					return ilt.LoadingText;
				}

				return Source.GetDurationString();
			}
		}

		public VersatilePlayer(MediaPlayer mediaPlayer, WebMusicHelper webHelper, WavSoundHelper wavHelper)
		{
			StandardFilePlayer = new FilePlayer(mediaPlayer);
			SoundCloudPlayer = new SoundCloudPlayer(webHelper);
			BrstmPlayer = new BrstmPlayer(wavHelper);
		}

		protected IMusicPlayer getMatchingPlayer(Type sourceType)
		{
			IMusicPlayer res = null;

			if (sourceType == typeof(FileMusic))
			{
				res = StandardFilePlayer;
			}
			else if (sourceType == typeof(SoundCloudMusic))
			{
				res = SoundCloudPlayer;
			}
			else if (sourceType == typeof(BrstmMusic))
			{
				res = BrstmPlayer;
			}

			return res;
		}

		public void Play(IMusicSource source)
		{
			if (ActivePlayer == null)
			{
				Source = source;

				ActivePlayer = getMatchingPlayer(Source.GetType());
				ActivePlayer.PlaySrc(Source);

				if (ActivePlayer is SoundCloudPlayer)
				{
					SoundCloudPlayer.Source.TitleChanged += (s, e) => { TitleChanged?.Invoke(s, e); };
				}
				else // if (ActivePlayer is FilePlayer)
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