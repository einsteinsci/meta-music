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
	public sealed class PlaylistItem : IEquatable<PlaylistItem>
	{
		[JsonProperty]
		public string Source
		{ get; set; }

		public LibraryItem LibraryData
		{ get; set; }

		public void LoadFromLibrary(SongLibrary library)
		{
			LibraryData = library.Retrieve(Source);
		}

		public bool Equals(PlaylistItem pli)
		{
			if ((object)pli == null)
			{
				return false;
			}

			return pli.Source == Source;
		}

		public override bool Equals(object obj)
		{
			PlaylistItem pli = obj as PlaylistItem;
			if (pli != null)
			{
				return pli.Equals(this);
			}

			return false;
		}

		public override int GetHashCode()
		{
			// ReSharper disable once NonReadonlyMemberInGetHashCode
			return Source.GetHashCode();
		}

		public override string ToString()
		{
			return "[PlaylistItem] " + (LibraryData?.DisplayName ?? Source);
		}

		public static bool operator ==(PlaylistItem a, PlaylistItem b)
		{
			if ((object)a != null)
			{
				return a.Equals(b);
			}

			return (object)b == null;
		}

		public static bool operator !=(PlaylistItem a, PlaylistItem b)
		{
			if ((object)a != null)
			{
				return !a.Equals(b);
			}

			return (object)b != null;
		}
	}
}