using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MetaMusic.BrstmConvert;
using MetaMusic.Players;

using NAudio.Wave;

using Newtonsoft.Json.Linq;

using UltimateUtil;

namespace MetaMusic.Sources
{
	public class BrstmMusic : IMusicSource, ILoadingText
	{
		public WavSoundHelper WavHelper
		{ get; private set; }

		public string FilePath
		{ get; private set; }

		public string ConvertedFilePath
		{ get; private set; }

		public TimeSpan? Duration => WavHelper.Duration;

		public string Title => Path.GetFileName(FilePath);

		public TimeSpan Position
		{
			get
			{
				return WavHelper.Progress;
			}
			set
			{
				WavHelper.SeekPosition = value;
			}
		}

		public bool IsPlaying
		{ get; private set; }

		public bool HasLoaded
		{ get; set; }

		public Exception LastException
		{ get; private set; }

		public bool HasThrown
		{ get; private set; }

		public string LoadingText
		{ get; private set; }

		private bool _hasHelperLoaded;

		public BrstmMusic(string permUrl, WavSoundHelper helper)
		{
			FilePath = permUrl;
			WavHelper = helper;
		}

		public void Load()
		{
			LoadingText = "Converting from BRSTM...";

			try
			{
				ConvertedFilePath = VgmstreamConverter.ConvertToWAV(FilePath);
			}
			catch (Win32Exception ex)
			{
				HasThrown = true;
				LoadingText = "Conversion Failed: " + ex.Message;
				LastException = ex;
				return;
			}

			WavHelper.LoadFromPath(ConvertedFilePath);

			LoadingText = "Decompressing song '{0}'...".Fmt(Title);

			HasLoaded = true;
		}

		public void Play()
		{
			if (!_hasHelperLoaded)
			{
				Thread loader = new Thread(() => {
					if (!HasLoaded)
					{
						Load();
					}

					_hasHelperLoaded = true;

					WavHelper.Play(() => { LoadingText = null; });
				});

				loader.Start();
			}
			else
			{
				WavHelper.Resume();
			}

			IsPlaying = true;
		}

		public void Pause()
		{
			WavHelper.Pause();

			IsPlaying = false;
		}

		public void Stop()
		{
			WavHelper.Stop();
			IsPlaying = false;
		}
	}
}