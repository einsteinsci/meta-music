using System;

namespace MetaMusic.Sources
{
	public interface IMusicSource
	{
		TimeSpan? Duration
		{ get; }

		TimeSpan Position
		{ get; }

		bool IsPlaying
		{ get; }

		void Play();

		void Pause();

		void Stop();
	}
}