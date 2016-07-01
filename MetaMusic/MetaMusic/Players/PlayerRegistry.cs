using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetaMusic.Sources;

using UltimateUtil.Registries;

namespace MetaMusic.Players
{
	public class PlayerRegistry : DynamicRegistry<IMusicPlayer>
	{
		public VersatilePlayer VersatilePlayer
		{ get; private set; }

		public PlayerRegistry(VersatilePlayer versatile)
		{
			VersatilePlayer = versatile;

			Register(VersatilePlayer.StandardFilePlayer);
			Register(VersatilePlayer.SoundCloudPlayer);
			Register(VersatilePlayer.BrstmPlayer);
		}

		public void Register(IMusicPlayer player)
		{
			Register(player.RegistryName, player);
		}

		public IMusicSource MakeSource(string sourceUri)
		{
			foreach (IMusicPlayer plr in Items)
			{
				if (plr.CanPlay(sourceUri) && !(plr is VersatilePlayer))
				{
					return plr.MakeSource(sourceUri);
				}
			}

			return null;
		}
	}
}