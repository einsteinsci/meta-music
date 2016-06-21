using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetaMusic.Sources;

namespace MetaMusic.Players
{
	public interface IMusicPlayer<T> 
		where T : IMusicSource
	{
		T Source
		{ get; }

		void Play(T source);

		void Resume();

		void Pause();

		void TogglePause();

		void Stop();
	}
}