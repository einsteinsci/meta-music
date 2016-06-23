using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace MetaMusic
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Playlist
	{
		[JsonProperty]
		public string Name
		{ get; set; }

		[JsonProperty]
		public List<PlaylistItem> Songs
		{ get; set; }
	}
}