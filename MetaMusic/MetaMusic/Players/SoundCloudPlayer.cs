using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetaMusic.Sources;

namespace MetaMusic.Players
{
	public class SoundCloudPlayer : MusicPlayerBase<SoundCloudMusic>, ILastException, ILoadingText
	{
		internal const string __CLIENTID__ = "32c6d127f78f111e92213bd0ba364199";
		private const string __SECRET__ = "7b23cc35985b69a911ad865c86bdc58a";

		public WebMusicHelper WebHelper
		{ get; private set; }

		public Exception LastException => Source.LastException;

		public bool HasThrown => Source.HasThrown;

		public string LoadingText => Source.LoadingText;

		public SoundCloudPlayer(WebMusicHelper helper)
		{
			WebHelper = helper;
			Volume = 0.3f;
		}

		public void Play(string url)
		{
			Source = new SoundCloudMusic(url, WebHelper);
			Source.Load();

			Source.Play();
		}

		public override void Play(SoundCloudMusic source)
		{
			base.Play(source);

			WebHelper.CurrentVolume = Volume;
		}
	}
}