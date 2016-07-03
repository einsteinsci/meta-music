using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
// ReSharper disable All

namespace MetaMusic
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Playlist
	{
		public static readonly string PLAYLIST_FOLDER = Path.Combine(PlayerSettings.SAVE_FOLDER, "Playlists");

		[JsonProperty]
		public string Name
		{ get; set; }

		[JsonProperty]
		public List<PlaylistItem> Songs
		{ get; set; }

		[JsonProperty]
		public int CurrentSongIndex
		{ get; set; }

		[JsonProperty]
		public bool Shuffle
		{ get; set; }

		public PlaylistItem CurrentSong => Songs[CurrentSongIndex];

		public Playlist()
		{
			Songs = new List<PlaylistItem>();
		}

		public Playlist(string name) : this()
		{
			Name = name;
		}

		public static Playlist Load(string path)
		{
			if (!File.Exists(path))
			{
				return null;
			}

			try
			{
				string json = File.ReadAllText(path);
				return JsonConvert.DeserializeObject<Playlist>(json);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static void LoadAllInFolder(string folderPath, List<Playlist> res)
		{
			if (res == null)
			{
				throw new ArgumentNullException(nameof(res));
			}

			res.Clear();

			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
				return;
			}

			IEnumerable<string> files = from filepath in Directory.EnumerateFiles(folderPath)
										where filepath.ToLower().EndsWith(".json")
										select filepath;

			foreach (string f in files)
			{
				Playlist list = Load(f);
				if (list != null)
				{
					res.Add(list);
				}
			}
		}

		public void LinkTo(SongLibrary library)
		{
			foreach (PlaylistItem pli in Songs)
			{
				LibraryItem libi = library.FirstOrDefault(_libi => _libi.Source == pli.Source);
				if (libi == null)
				{
					libi = new LibraryItem { Source = pli.Source };
					library.Add(libi);
				}

				pli.Song = libi;
			}
		}

		public void Save()
		{
			Directory.CreateDirectory(PLAYLIST_FOLDER);

			string path = Path.Combine(PLAYLIST_FOLDER, Name + ".json");
			string json = JsonConvert.SerializeObject(this, Formatting.Indented);
			File.WriteAllText(path, json);
		}
	}
}