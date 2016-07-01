using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using UltimateUtil;

namespace MetaMusic
{
	[JsonObject(MemberSerialization.OptIn)]
	public class LibraryItem
	{
		[JsonProperty]
		public string Source
		{ get; set; }

		[JsonProperty]
		public string CustomName
		{ get; set; }

		public string ShortName
		{
			get
			{
				int index = Math.Max(Source.LastIndexOf('\\'), Source.LastIndexOf('/'));
				return Source.Substring(index + 1);
			}
		}

		[JsonProperty]
		public int Rating
		{ get; set; }

		[JsonProperty]
		public double Length
		{ get; set; }

		public string DisplayName => CustomName.IsNullOrEmpty() ? ShortName : CustomName;

		public TimeSpan LengthTime
		{
			get
			{
				return TimeSpan.FromSeconds(Length);
			}
			set
			{
				Length = value.TotalSeconds;
			}
		}

		public static LibraryItem FromSourceUri(string uri)
		{
			return new LibraryItem { Source = uri };
		}

		public PlaylistItem MakePlaylistItem()
		{
			return new PlaylistItem { Song = this, Source = Source };
		}
	}
}