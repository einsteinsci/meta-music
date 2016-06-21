using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using MetaMusic.Sources;

namespace MetaMusic.Players
{
	public class FilePlayer : MusicPlayerBase<FileMusic>
	{
		private readonly MediaPlayer _player;

		public FilePlayer(MediaPlayer player)
		{
			_player = player;
		}

		public void Play(string path)
		{
			Source = new FileMusic(path, _player);
			Source.Play();
		}
	}
}