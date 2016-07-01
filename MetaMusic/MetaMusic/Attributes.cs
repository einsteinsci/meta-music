using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaMusic
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class PluginAttribute : Attribute
	{
		public string PluginID
		{ get; private set; }

		public string Version
		{ get; private set; }

		public PluginAttribute(string pluginID)
		{
			PluginID = pluginID;
		}
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class PluginInitAttribute : Attribute
	{ }
}