using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetaMusic.Sources;

namespace MetaMusic.Players
{
	[MusicPlayer]
	public class BrstmPlayer : MusicPlayerBase<BrstmMusic>, ILoadingText, ILastException
	{
		public WavSoundHelper WavHelper
		{ get; private set; }

		public override bool Muted
		{
			get
			{
				return WavHelper.Muted;
			}
			set
			{
				WavHelper.Muted = value;
			}
		}

		public Exception LastException => Source.LastException;

		public bool HasThrown => Source.HasThrown;

		public string LoadingText => Source.LoadingText;

		public bool HasLoaded => Source.HasLoaded;

		public override string RegistryName => nameof(BrstmPlayer);

		public BrstmPlayer(WavSoundHelper helper)
		{
			WavHelper = helper;

			WavHelper.OnPlayFinished += runOnPlayFinished;
		}

		public void ResetException()
		{
			Source.ResetException();
		}

		public void Play(string path)
		{
			Source = new BrstmMusic(path, WavHelper);
			Source.Load();

			Source.Play();
		}

		public override bool CanPlay(string sourceUri)
		{
			return sourceUri.ToLower().EndsWith(".brstm");
		}

		public override IMusicSource MakeSource(string source)
		{
			return new BrstmMusic(source, WavHelper);
		}

		public override double GetVolume(PlayerSettings settings)
		{
			return settings.VolumeBrstm;
		}

		public override void SetVolume(PlayerSettings settings, double volume)
		{
			settings.VolumeBrstm = volume;
			WavHelper.CurrentVolume = (float)volume;
		}
	}
}