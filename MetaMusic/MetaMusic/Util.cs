using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
		public static string GetDurationString(this IMusicSource source)
		{
			if (source == null)
			{
				return "No file playing.";
			}

			return "{0:mm\\:ss} / {1}".Fmt(source.Position, source.Duration?.ToString("mm\\:ss") ?? "-:--");
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
			Win32Util.Win32Point w32Mouse = new Win32Util.Win32Point();
			Win32Util.GetCursorPos(ref w32Mouse);
			return new Point(w32Mouse.X, w32Mouse.Y);
		}

		public static Visibility ToVis(this bool b, Visibility onFalse = Visibility.Collapsed)
		{
			return b ? Visibility.Visible : onFalse;
		}

		public static bool StartsWithAny(this string str, params string[] starters)
		{
			return starters.Any(str.StartsWith);
		}

		public static bool MatchesSignature(this MethodInfo method, Type ret, params Type[] paramTypes)
		{
			if (method.ReturnType != ret)
			{
				return false;
			}

			ParameterInfo[] pars = method.GetParameters();

			if (pars.Length != paramTypes.Length)
			{
				return false;
			}

			for (int i = 0; i < paramTypes.Length; i++)
			{
				if (pars[i].ParameterType != paramTypes[i])
				{
					return false;
				}
			}

			return true;
		}

		public static T PeekOrDefault<T>(this Queue<T> queue)
		{
			if (queue.IsEmpty())
			{
				return default(T);
			}

			return queue.Peek();
		}

		public static ImageSource LoadImageFromBytes(byte[] data)
		{
			MemoryStream byteStream = new MemoryStream(data);
			BitmapImage image = new BitmapImage();
			image.BeginInit();
			image.StreamSource = byteStream;
			image.EndInit();

			return image;
		}

		public static int IndexOf(this ItemCollection coll, Predicate<object> pred)
		{
			for (int i = 0; i < coll.Count; i++)
			{
				if (pred(coll[i]))
				{
					return i;
				}
			}

			return -1;
		}
	}
}
