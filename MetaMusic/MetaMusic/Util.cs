using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using MetaMusic.Players;
using MetaMusic.Sources;

using NAudio.Wave;

using UltimateUtil;

namespace MetaMusic
{
	public static class Util
	{
		[StructLayout(LayoutKind.Sequential)]
		internal struct Win32Point
		{
			public int X;
			public int Y;
		};

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetCursorPos(ref Win32Point pt);

		public static string GetDurationString(this IMusicSource source)
		{
			if (source == null)
			{
				return "No file playing.";
			}

			return "{0:mm\\:ss} / {1}".Fmt(source.Position, source.Duration?.ToString("mm\\:ss") ?? "???");
		}

		public static ImageSource LoadImage(string path)
		{
			return new BitmapImage(new Uri("pack://application:,,,/MetaMusic;component/{0}".Fmt(path)));
		}

		public static string GetResolveUrl(string permanentUrl)
		{
			return "https://api.soundcloud.com/resolve?url=" + permanentUrl + "&client_id=" + SoundCloudPlayer.__CLIENTID__;
		}

		public static string Quote(this string str)
		{
			return "\"" + str + "\"";
		}

		public static TimeSpan GetWavDuration(string filePath)
		{
			WaveFileReader reader = new WaveFileReader(filePath);
			return reader.TotalTime;
		}

		public static Point GetMousePosition()
		{
			Win32Point w32Mouse = new Win32Point();
			GetCursorPos(ref w32Mouse);
			return new Point(w32Mouse.X, w32Mouse.Y);
		}

		public static Visibility ToVis(this bool b, Visibility onFalse = Visibility.Collapsed)
		{
			return b ? Visibility.Visible : onFalse;
		}
	}
}
