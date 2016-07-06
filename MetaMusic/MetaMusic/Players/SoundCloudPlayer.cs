using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetaMusic.Sources;

namespace MetaMusic.Players
{
	[MusicPlayer]
	public class SoundCloudPlayer : MusicPlayerBase<SoundCloudMusic>, ILastException, ILoadingText
	{
		internal const string __CLIENTID__ = "32c6d127f78f111e92213bd0ba364199";
		private const string __SECRET__ = "7b23cc35985b69a911ad865c86bdc58a";

		public WebMusicHelper WebHelper
		{ get; private set; }
		
		public override bool Muted
		{
			get
			{
				return WebHelper.Muted;
			}
			set
			{
				WebHelper.Muted = value;
			}
		}

		public Exception LastException => Source.LastException;

		public bool HasThrown => Source.HasThrown;

		public string LoadingText => Source.LoadingText;

		public bool HasLoaded => Source.HasLoaded;

		public override string RegistryName => nameof(SoundCloudPlayer);

		public SoundCloudPlayer(WebMusicHelper helper)
		{
			WebHelper = helper;

			WebHelper.OnPlayFinished += runOnPlayFinished;
		}

		public void ResetException()
		{
			Source.ResetException();
		}

		public void Play(string url)
		{
			Source = new SoundCloudMusic(url, WebHelper);
			Source.Load();

			Source.Play();
		}

		public override bool CanPlay(string sourceUri)
		{
			return sourceUri.ToLower().StartsWithAny("http://soundcloud.com", "https://soundcloud.com");
		}

		public override IMusicSource MakeSource(string source)
		{
			return new SoundCloudMusic(source, WebHelper);
		}

		public override double GetVolume(PlayerSettings settings)
		{
			return settings.VolumeSoundCloud;
		}

		public override void SetVolume(PlayerSettings settings, double volume)
		{
			settings.VolumeSoundCloud = volume;
			WebHelper.CurrentVolume = (float)volume;
		}
	}
}