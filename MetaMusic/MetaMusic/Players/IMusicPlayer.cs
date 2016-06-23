using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetaMusic.Sources;

namespace MetaMusic.Players
{
	public interface IMusicPlayer<T> : IMusicPlayer
		where T : IMusicSource
	{
		new T Source
		{ get; }

		void Play(T source);
	}

	public interface IMusicPlayer
	{
		IMusicSource Source
		{ get; }

		void PlaySrc(IMusicSource source);

		void Resume();

		void Pause();

		void TogglePause();

		void Stop();
	}
}