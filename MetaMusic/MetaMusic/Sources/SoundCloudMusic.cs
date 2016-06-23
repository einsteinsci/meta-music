using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MetaMusic.Players;

using Newtonsoft.Json.Linq;

using UltimateUtil;

namespace MetaMusic.Sources
{
	public class SoundCloudMusic : IMusicSource, ILastException
	{
		public WebMusicHelper WebHelper
		{ get; private set; }

		public string PermanentURL
		{ get; private set; }

		public string StreamURL
		{ get; set; }

		public string TemporaryFileURL
		{ get; set; }

		public TimeSpan? Duration
		{ get; set; }

		public string Title
		{ get; set; }

		public TimeSpan Position => WebHelper.Progress;

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

		public event EventHandler TitleChanged;

		private bool _hasHelperLoaded;

		public SoundCloudMusic(string permUrl, WebMusicHelper helper)
		{
			PermanentURL = permUrl;
			WebHelper = helper;
		}

		public void Load()
		{
			LoadingText = "Loading from SoundCloud...";

			LastException = null;
			HasThrown = false;

			string songDataJson;
			try
			{
				WebClient client = new WebClient();
				songDataJson = client.DownloadString(Util.GetResolveUrl(PermanentURL));
			}
			catch (WebException ex)
			{
				LastException = ex;
				HasThrown = true;
				LoadingText = "Failed to connect: " + LastException.GetType().Name + " - " + LastException.Message;
				return;
			}

			dynamic root = JObject.Parse(songDataJson);

			if (!_isValidTrack(root))
			{
				return;
			}

			StreamURL = root.stream_url + "?client_id=" + SoundCloudPlayer.__CLIENTID__;

			Title = root.title;
			TitleChanged?.Invoke(this, new EventArgs());

			int durationMS = root.duration;
			Duration = TimeSpan.FromMilliseconds(durationMS);

			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(StreamURL);
			req.AllowAutoRedirect = false;
			HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
			TemporaryFileURL = resp.GetResponseHeader("Location");
			
			WebHelper.LoadFromUrl(TemporaryFileURL);

			HasLoaded = true;

			LoadingText = "Starting song '{0}'...".Fmt(Title);
		}

		private bool _isValidTrack(dynamic root)
		{
			if (root.errors != null)
			{
				dynamic errObj = root.errors[0];
				JToken msg = errObj["error_message"];

				LoadingText = "Song retrieval failed: " + msg;
				LastException = new Exception(LoadingText);
				HasThrown = true;
				return false;
			}

			if (root.kind != "track")
			{
				LoadingText = "Wrong data type: " + root.kind;
				LastException = new Exception(LoadingText);
				HasThrown = true;
				return false;
			}

			return true;
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

					WebHelper.Play(() => { LoadingText = null; });
				});

				loader.Start();
			}
			else
			{
				WebHelper.Resume();
			}

			IsPlaying = true;
		}

		public void Pause()
		{
			WebHelper.Pause();

			IsPlaying = false;
		}

		public void Stop()
		{
			WebHelper.Stop();
			IsPlaying = false;
		}
	}
}