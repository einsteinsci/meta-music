using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace MetaMusic
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class SongLibrary : IList<LibraryItem>
	{
		public static readonly string LIBRARY_PATH = Path.Combine(PlayerSettings.SAVE_FOLDER, "library.json");

		public int Count => _data.Count;

		public bool IsReadOnly => false;

		public LibraryItem this[int index]
		{
			get
			{
				return _data[index];
			}
			set
			{
				_data[index] = value;
			}
		}

		[JsonProperty("Data")]
		private List<LibraryItem> _data = new List<LibraryItem>();

		private static string _makeSong(string filename)
		{
			return @"C:\Users\sealedinterface\OneDrive\Music\" + filename;
		}

		#region IList<LibraryItem>
		public IEnumerator<LibraryItem> GetEnumerator()
		{
			return _data.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(LibraryItem item)
		{
			if (!_data.Exists(li => li.Source == item.Source))
			{
				_data.Add(item);
			}
		}

		public void Clear()
		{
			_data.Clear();
		}

		public bool Contains(LibraryItem item)
		{
			return _data.Contains(item);
		}

		public void CopyTo(LibraryItem[] array, int arrayIndex)
		{
			_data.CopyTo(array, arrayIndex);
		}

		public bool Remove(LibraryItem item)
		{
			return _data.Remove(item);
		}
		public int IndexOf(LibraryItem item)
		{
			return _data.IndexOf(item);
		}

		public void Insert(int index, LibraryItem item)
		{
			_data.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			_data.RemoveAt(index);
		}
		#endregion IList<LibraryItem>

		public LibraryItem Retrieve(string src)
		{
			return _data.FirstOrDefault(li => li.Source == src);
		}

		public static SongLibrary Load()
		{
			if (!File.Exists(LIBRARY_PATH))
			{
				SongLibrary res = new SongLibrary();
				res.Add(LibraryItem.FromSourceUri(_makeSong("Captain-Cool.mp3")));
				res.Add(LibraryItem.FromSourceUri(_makeSong("Dare.mp3")));
				res.Add(LibraryItem.FromSourceUri(_makeSong("Active (Theof Remix).mp3")));
				res.Add(LibraryItem.FromSourceUri(_makeSong("Antipixel--paid.mp3")));
				res.Add(LibraryItem.FromSourceUri(_makeSong("Dr-Wilys-Castle-Arnmt.mp3")));
				res.Add(LibraryItem.FromSourceUri(_makeSong("Ichor.mp3")));
				res.Add(LibraryItem.FromSourceUri(_makeSong("Level-Four.mp3")));
				return res;
			}

			try
			{
				string json = File.ReadAllText(LIBRARY_PATH);
				return JsonConvert.DeserializeObject<SongLibrary>(json);
			}
			catch (Exception)
			{
				return new SongLibrary();
			}
		}

		public void Save()
		{
			Directory.CreateDirectory(PlayerSettings.SAVE_FOLDER);

			string json = JsonConvert.SerializeObject(this, Formatting.Indented);
			File.WriteAllText(LIBRARY_PATH, json);
		}
	}
}