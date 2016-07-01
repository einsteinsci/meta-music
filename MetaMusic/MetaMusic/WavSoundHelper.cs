using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NAudio.Wave;

namespace MetaMusic
{
	public class WavSoundHelper : ILastException
	{
		public TimeSpan Progress
		{ get; private set; }

		public TimeSpan? Duration
		{ get; private set; }

		public string FilePath
		{ get; private set; }

		public bool IsLoaded
		{ get; private set; }

		public PlaybackState PlaybackState
		{
			get
			{
				if (!IsLoaded)
				{
					return PlaybackState.Stopped;
				}

				return IsPaused ? PlaybackState.Paused : PlaybackState.Playing;
			}
		}

		public bool IsPaused
		{ get; private set; }

		public Exception LastException
		{ get; private set; }

		public bool HasThrown
		{ get; private set; }

		public float CurrentVolume
		{ get; set; }

		public TimeSpan? SeekPosition
		{ get; set; }

		private bool _stopped = true;

		private BackgroundWorker _worker;

		private Action _onPlayStart;

		public WavSoundHelper()
		{
			CurrentVolume = 1.0f;
		}

		public void LoadFromPath(string path)
		{
			FilePath = path;

			_worker = new BackgroundWorker
			{
				WorkerReportsProgress = true,
				WorkerSupportsCancellation = true
			};

			Progress = TimeSpan.Zero;
			_worker.DoWork += (s, e) => { _worker_DoWork(); };
			_worker.ProgressChanged += (s, e) => { Progress += (TimeSpan)e.UserState; };

			IsLoaded = true;
			HasThrown = false;
			_stopped = false;
		}

		public void Play(Action onPlayStart)
		{
			if (IsLoaded)
			{
				_worker.RunWorkerAsync();
			}

			_onPlayStart = onPlayStart;
		}

		public void Pause()
		{
			IsPaused = true;
		}

		public void Resume()
		{
			IsPaused = false;
		}

		public void TogglePause()
		{
			IsPaused = !IsPaused;
		}

		public void Stop()
		{
			_stopped = true;
			_worker?.CancelAsync();

			_worker = null;
			FilePath = null;
			IsLoaded = false;

			IsPaused = false;
		}

		private void _worker_DoWork()
		{
			if (!IsLoaded)
			{
				return;
			}
			
			_onPlayStart();
			
			using (WaveFileReader reader = new WaveFileReader(FilePath))
			{
				Duration = reader.TotalTime;

				using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
				{
					_playSoundStream(waveOut, reader);
				}
			}
		}

		private void _playSoundStream(WaveOut waveOut, WaveStream blockAlignedStream)
		{
			if (blockAlignedStream == null)
			{
				throw new ArgumentNullException(nameof(blockAlignedStream));
			}

			Stopwatch timer = new Stopwatch();
			timer.Start();

			waveOut.Init(blockAlignedStream);
			waveOut.Volume = CurrentVolume;
			waveOut.Play();
			
			while (waveOut.PlaybackState == PlaybackState.Playing || waveOut.PlaybackState == PlaybackState.Paused)
			{
				if (IsPaused)
				{
					waveOut.Pause();
					timer.Stop();
				}
				else if (waveOut.PlaybackState == PlaybackState.Paused)
				{
					waveOut.Resume();
					timer.Start();
				}

				if (_stopped || _worker.CancellationPending)
				{
					waveOut.Stop();
				}

				if (SeekPosition != null)
				{
					long newPos = (long)(blockAlignedStream.WaveFormat.AverageBytesPerSecond * SeekPosition.Value.TotalSeconds);

					// Force it to align to a block boundary
					if (newPos % blockAlignedStream.WaveFormat.BlockAlign != 0)
					{
						newPos -= newPos % blockAlignedStream.WaveFormat.BlockAlign;
					}
					// Force new position into valid range
					newPos = Math.Max(0, Math.Min(blockAlignedStream.Length, newPos));

					blockAlignedStream.Position = newPos;

					TimeSpan diff = SeekPosition.Value - Progress;
					_worker?.ReportProgress(0, diff);

					SeekPosition = null;
				}
				else
				{
					_worker?.ReportProgress(0, timer.Elapsed);
				}

				timer.Restart();

				Thread.Sleep(100);
			}

			timer.Stop();
		}
	}
}