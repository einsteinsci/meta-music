using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetaMusic.Sources;

using UltimateUtil.Registries;

namespace MetaMusic.Players
{
	public interface IMusicPlayer<T> : IMusicPlayer
		where T : IMusicSource
	{
		new T Source
		{ get; }

		void Play(T source);
	}

	public interface IMusicPlayer : IRegisterable
	{
		IMusicSource Source
		{ get; }

		void PlaySrc(IMusicSource source);

		void Resume();

		void Pause();

		void TogglePause();

		void Stop();

		bool CanPlay(string sourceUri);

		IMusicSource MakeSource(string sourceUri);

		double GetVolume(PlayerSettings settings);

		void SetVolume(PlayerSettings settings, double volume);
	}
}