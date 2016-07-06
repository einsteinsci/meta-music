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
	public sealed class SoundCloudMusic : IMusicSource, ILastException, ILoadingText
	{
		public WebMusicHelper WebHelper
		{ get; private set; }

		public string PermanentURL
		{ get; private set; }

		public string StreamURL
		{ get; private set; }

		public string TemporaryFileURL
		{ get; private set; }

		public TimeSpan? Duration
		{ get; private set; }

		public string Title
		{ get; private set; }

		public string Author
		{ get; private set; }

		public byte[] CoverArtData
		{ get; private set; }
		private string _coverArtUrl;

		public TimeSpan Position
		{
			get
			{
				return WebHelper.Progress;
			}
			set
			{
				WebHelper.SeekPosition = value;
			}
		}

		public bool IsPlaying
		{ get; private set; }

		public bool HasLoaded
		{ get; private set; }

		public Exception LastException
		{ get; private set; }

		public bool HasThrown
		{ get; private set; }

		public string LoadingText
		{ get; private set; }

		public string Album => null;

		public event EventHandler TitleChanged;

		private bool _hasHelperLoaded;

		private Thread _artworkThread;

		public SoundCloudMusic(string permUrl, WebMusicHelper helper)
		{
			PermanentURL = permUrl;
			WebHelper = helper;
		}

		public void ResetException()
		{
			LastException = null;
			HasThrown = false;
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
			Author = root.user?.username ?? "UNKNOWN";
			TitleChanged?.Invoke(this, new EventArgs());

			_coverArtUrl = root.artwork_url;
			if (_coverArtUrl.IsNullOrEmpty())
			{
				_coverArtUrl = root.user.avatar_url;
			}

			int durationMS = root.duration;
			Duration = TimeSpan.FromMilliseconds(durationMS);

			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(StreamURL);
			req.AllowAutoRedirect = false;
			HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
			TemporaryFileURL = resp.GetResponseHeader("Location");
			
			WebHelper.LoadFromUrl(TemporaryFileURL);

			HasLoaded = true;

			LoadingText = "Decompressing song '{0}'...".Fmt(Title);
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
				loader.Name = "SoundCloud Song Loader";
				loader.Start();

				StartArtworkLoader();
			}
			else
			{
				WebHelper.Resume();
			}

			IsPlaying = true;
		}

		private void StartArtworkLoader()
		{
			if (_artworkThread != null && _artworkThread.IsAlive)
			{
				_artworkThread.Abort();
			}

			_artworkThread = new Thread(() => {
				while (_coverArtUrl.IsNullOrEmpty())
				{
					Thread.Sleep(100);
				}

				using (WebClient client = new WebClient())
				{
					CoverArtData = client.DownloadData(_coverArtUrl);
				}
			});
			_artworkThread.Name = "SoundCloud Song Artwork Loader";
			_artworkThread.Start();
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

			_artworkThread.Abort();
		}
	}
}