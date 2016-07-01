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
	public sealed class PlaylistItem
	{
		[JsonProperty]
		public string Source
		{ get; set; }

		public LibraryItem Song
		{ get; set; }

		public void LoadFromLibrary(SongLibrary library)
		{
			Song = library.Retrieve(Source);
		}
	}
}