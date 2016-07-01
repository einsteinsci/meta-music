using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using MetaMusic.Players;

using UltimateUtil;

namespace MetaMusic
{
	public class PluginLoader
	{
		public Dictionary<string, object> Plugins
		{ get; private set; } = new Dictionary<string, object>();

		public PlayerRegistry Players
		{ get; private set; }

		public PluginLoader(PlayerRegistry players)
		{
			Players = players;
		}

		public void LoadPlugin(string path)
		{
			try
			{
				Assembly assembly = Assembly.LoadFile(path);
				
				foreach (Type t in assembly.ExportedTypes)
				{
					if (t.HasAttribute<PluginAttribute>())
					{
						LoadPlugin(t);
					}
				}
			}
			catch (IOException ex)
			{
				Debugger.Log(3, "PLUGINS", "IOException thrown during plugin loading: " + ex.Message);
			}
			catch (InvalidOperationException ex)
			{
				Debugger.Log(3, "PLUGINS", "InvalidOperationException during plugin loading: " + ex.Message);
			}
		}

		public void LoadPlugin(Type plugin)
		{
			PluginAttribute att = plugin.GetAttribute<PluginAttribute>();
			if (Plugins.ContainsKey(att.PluginID))
			{
				throw new InvalidOperationException($"Plugin ID '{att.PluginID}' already registered.");
			}

			object pluginInst = Activator.CreateInstance(plugin);
			Plugins.Add(att.PluginID, pluginInst);

			foreach (MethodInfo m in plugin.GetMethods())
			{
				if (m.MatchesSignature(typeof(void), typeof(PlayerRegistry)))
				{
					m.Invoke(pluginInst, new object[] { Players });
				}
			}
		}
	}
}