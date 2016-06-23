using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetaMusic.Sources;

namespace MetaMusic.Players
{
	public class BrstmPlayer : MusicPlayerBase<BrstmMusic>, ILoadingText
	{
		public WavSoundHelper WavHelper
		{ get; private set; }

		public Exception LastException => Source.LastException;

		public bool HasThrown => Source.HasThrown;

		public string LoadingText => Source.LoadingText;

		public BrstmPlayer(WavSoundHelper helper)
		{
			WavHelper = helper;
			Volume = 0.3f;
		}

		public void Play(string path)
		{
			Source = new BrstmMusic(path, WavHelper);
			Source.Load();

			Source.Play();
		}

		public override void Play(BrstmMusic source)
		{
			base.Play(source);

			WavHelper.CurrentVolume = Volume;
		}
	}
}