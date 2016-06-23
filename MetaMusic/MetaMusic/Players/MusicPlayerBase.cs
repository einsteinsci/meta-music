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

		public float Volume
		{ get; set; }

		protected MusicPlayerBase()
		{
			Volume = 1.0f;
		}

		public virtual void Play(T source)
		{
			Source = source;
			Resume();
		}

		void IMusicPlayer.PlaySrc(IMusicSource src)
		{
			T t = src as T;
			if (t != null)
			{
				Play(t);
			}
		}

		public virtual void Resume()
		{
			Source.Play();
		}

		public virtual void Pause()
		{
			Source.Pause();
		}

		public virtual void TogglePause()
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

		public virtual void Stop()
		{
			if (Source != null)
			{
				Source.Stop();
				Source = null;
			}
		}

		IMusicSource IMusicPlayer.Source => Source;
	}
}