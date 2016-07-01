using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaMusic.Players
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class MusicPlayerAttribute : Attribute
	{ }
}