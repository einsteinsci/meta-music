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

		public void Save()
		{
			Directory.CreateDirectory(PLAYLIST_FOLDER);

			string path = Path.Combine(PLAYLIST_FOLDER, Name);
			string json = JsonConvert.SerializeObject(this, Formatting.Indented);
			File.WriteAllText(path, json);
		}
	}
}