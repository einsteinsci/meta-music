using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using MetaMusic.Players;
using MetaMusic.Sources;

using UltimateUtil;

namespace MetaMusic
{
	public static class Util
	{
		public static string GetDurationString(this IMusicSource source)
		{
			if (source == null)
			{
				return "No file playing.";
			}

			return "{0:mm\\:ss} / {1}".Fmt(source.Position, source.Duration?.ToString("mm\\:ss") ?? "???");
		}

		public static ImageSource LoadImage(string path)
		{
			return new BitmapImage(new Uri("pack://application:,,,/MetaMusic;component/{0}".Fmt(path)));
		}

		public static string GetResolveUrl(string permanentUrl)
		{
			return "https://api.soundcloud.com/resolve?url=" + permanentUrl + "&client_id=" + SoundCloudPlayer.__CLIENTID__;
		}
	}
}
