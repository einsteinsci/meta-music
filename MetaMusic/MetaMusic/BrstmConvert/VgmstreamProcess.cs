using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaMusic.BrstmConvert
{
	internal class VgmstreamProcess : Process
	{
		public const string VGMSTREAM_DIR = "vgmstream";

		public VgmstreamProcess()
		{
			StartInfo.FileName = VGMSTREAM_DIR + Path.DirectorySeparatorChar + "test.exe";
		}
	}
}