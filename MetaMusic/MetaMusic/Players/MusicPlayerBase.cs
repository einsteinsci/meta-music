using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetaMusic.Sources;

namespace MetaMusic.Players
{
	public abstract class MusicPlayerBase<T> : IMusicPlayer<T> 
		where T : class, IMusicSource
	{
		public T Source
		{ get; protected set; }

		public void Play(T source)
		{
			Source = source;
			Resume();
		}

		public void Resume()
		{
			Source.Play();
		}

		public void Pause()
		{
			Source.Pause();
		}

		public void TogglePause()
		{
			if (Source.IsPlaying)
			{
				Source.Pause();
			}
			else
			{
				Source.Play();
			}
		}

		public void Stop()
		{
			Source.Stop();
			Source = null;
		}
	}
}