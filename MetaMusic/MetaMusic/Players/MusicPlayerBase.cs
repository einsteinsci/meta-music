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

		public abstract string RegistryName
		{ get; }

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

		public abstract bool CanPlay(string sourceUri);

		public abstract IMusicSource MakeSource(string sourceUri);

		public virtual double GetVolume(PlayerSettings settings)
		{
			return settings.VolumeFile;
		}

		public virtual void SetVolume(PlayerSettings settings, double volume)
		{
			settings.VolumeFile = volume;
		}

		IMusicSource IMusicPlayer.Source => Source;
	}
}