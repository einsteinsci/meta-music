using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetaMusic.Sources;

using Newtonsoft.Json;

namespace MetaMusic
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class PlaylistItem
	{
		public enum PlayListItemType
		{
			File,
			BRSTM,
			SoundCloud
		}

		public IMusicSource SoundSource
		{ get; private set; }

		[JsonProperty]
		public string CustomName
		{ get; set; }

		[JsonProperty]
		public int? Rating
		{ get; set; }

		[JsonProperty]
		public PlayListItemType Type
		{ get; set; }

		[JsonProperty]
		public TimeSpan Duration
		{ get; set; }

		// For local types, this means the file path. For web types, this means the URL.
		public string Location
		{ get; set; }

		public PlaylistItem(IMusicSource source, string name)
		{
			
		}
		internal PlaylistItem()
		{ }
	}
}