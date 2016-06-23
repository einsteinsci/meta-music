using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UltimateUtil;

namespace MetaMusic.BrstmConvert
{
	public static class VgmstreamConverter
	{
		public const string TMP_DIR = "tmp";

		public static string ConvertToWAV(string input)
		{
			Directory.CreateDirectory(TMP_DIR);

			const string loops = "2.0"; // always use 2 loops in case the loop point is changed through sample rate conversion.
			Process proc = new VgmstreamProcess();
			
			proc.StartInfo.Arguments = "-o " + TMP_DIR + Path.DirectorySeparatorChar + "in.wav" + " -l " + loops +
				" -f 0.0 " + Path.GetFullPath(input).Quote();
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

			proc.Start();
			proc.WaitForExit();
			if (proc.ExitCode != 0)
			{
				throw new Win32Exception("Failed to create WAV files for " + Path.GetFullPath(input) + " (Exit code {0})".Fmt(proc.ExitCode));
			}

			return TMP_DIR + Path.DirectorySeparatorChar + "in.wav";
		}
	}
}