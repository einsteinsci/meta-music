using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Newtonsoft.Json;

namespace MetaMusic
{
	[JsonObject(MemberSerialization.OptIn)]
	public class PlayerSettings
	{
		public static readonly string SAVE_FOLDER = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "MetaMusic");

		public static readonly string SAVE_PATH = Path.Combine(SAVE_FOLDER, "settings.json");

		[JsonProperty]
		public bool Muted
		{ get; set; }

		[JsonProperty]
		public double VolumeFile
		{ get; set; }

		[JsonProperty]
		public double VolumeSoundCloud
		{ get; set; }

		[JsonProperty]
		public double VolumeBrstm
		{ get; set; }

		[JsonProperty]
		public double[] GridSplitterWidths
		{ get; private set; }

		[JsonProperty]
		public double WindowWidth
		{ get; set; }

		[JsonProperty]
		public double WindowHeight
		{ get; set; }

		[JsonProperty]
		public Point WindowPosition
		{ get; set; }

		[JsonProperty]
		public string ThemeAccent
		{ get; set; }

		[JsonProperty]
		public bool Minimalist
		{ get; set; }

		[JsonProperty]
		public string LastPlayedPlaylist
		{ get; set; }

		[JsonProperty]
		public string LastPlayedSong
		{ get; set; }

		public PlayerSettings()
		{
			GridSplitterWidths = new double[] { 2, 5, 3 };
			WindowWidth = 1000;
			WindowHeight = 560;

			WindowPosition = new Point(200, 200);

			VolumeFile = 1.0;
			VolumeSoundCloud = 0.3;
			VolumeBrstm = 0.4;

			ThemeAccent = "Amber";
		}

		public static PlayerSettings Load()
		{
			if (!File.Exists(SAVE_PATH))
			{
				return new PlayerSettings();
			}

			try
			{
				string json = File.ReadAllText(SAVE_PATH);
				return JsonConvert.DeserializeObject<PlayerSettings>(json);
			}
			catch (Exception)
			{
				return new PlayerSettings();
			}
		}

		public void Save()
		{
			Directory.CreateDirectory(SAVE_FOLDER);

			string json = JsonConvert.SerializeObject(this, Formatting.Indented);
			File.WriteAllText(SAVE_PATH, json);
		}
	}
}