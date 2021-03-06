﻿using System;

namespace MetaMusic.Sources
{
	public interface IMusicSource
	{
		TimeSpan? Duration
		{ get; }

		TimeSpan Position
		{ get; set; }

		string Title
		{ get; }

		string Author
		{ get; }

		string Album
		{ get; }

		byte[] CoverArtData
		{ get; }

		bool IsPlaying
		{ get; }

		void Play();

		void Pause();

		void Stop();
	}
}