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
	public class LibraryItem : IEquatable<LibraryItem>
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
			return new PlaylistItem { LibraryData = this, Source = Source };
		}

		public bool Equals(LibraryItem lbi)
		{
			if ((object)lbi == null)
			{
				return false;
			}

			return lbi.Source == Source;
		}

		public override bool Equals(object obj)
		{
			LibraryItem lbi = obj as LibraryItem;
			if (lbi != null)
			{
				return lbi.Equals(this);
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
			return DisplayName;
		}

		public static bool operator==(LibraryItem a, LibraryItem b)
		{
			if ((object)a != null)
			{
				return a.Equals(b);
			}

			return (object)b == null;
		}

		public static bool operator!=(LibraryItem a, LibraryItem b)
		{
			if ((object)a != null)
			{
				return !a.Equals(b);
			}

			return (object)b != null;
		}
	}
}