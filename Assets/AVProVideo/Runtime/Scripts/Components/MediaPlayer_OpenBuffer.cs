using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	public partial class MediaPlayer : MonoBehaviour
	{
		public bool OpenMediaFromBuffer(byte[] buffer, bool autoPlay = true)
		{
			_mediaPath = new MediaPath("buffer", MediaPathType.AbsolutePathOrURL);
			_autoPlayOnStart = autoPlay;

			if (_controlInterface == null)
			{
				Initialise();
			}

			return OpenMediaFromBufferInternal(buffer);
		}

		public bool StartOpenChunkedMediaFromBuffer(ulong length, bool autoPlay = true)
		{
			_mediaPath = new MediaPath("buffer", MediaPathType.AbsolutePathOrURL);
			_autoPlayOnStart = autoPlay;

			if (_controlInterface == null)
			{
				Initialise();
			}

			return StartOpenMediaFromBufferInternal(length);
		}

		public bool AddChunkToVideoBuffer(byte[] chunk, ulong offset, ulong chunkSize)
		{
			return AddChunkToBufferInternal(chunk, offset, chunkSize);
		}

		public bool EndOpenChunkedVideoFromBuffer()
		{
			return EndOpenMediaFromBufferInternal();
		}

		private bool OpenMediaFromBufferInternal(byte[] buffer)
		{
			bool result = false;
			// Open the video file
			if (_controlInterface != null)
			{
				CloseMedia();

				_isMediaOpened = true;
				_autoPlayOnStartTriggered = !_autoPlayOnStart;

				Helper.LogInfo("Opening buffer of length " + buffer.Length, this);

				if (!_controlInterface.OpenMediaFromBuffer(buffer))
				{
					Debug.LogError("[AVProVideo] Failed to open buffer", this);
					if (GetCurrentPlatformOptions() != PlatformOptionsWindows || PlatformOptionsWindows.videoApi != Windows.VideoApi.DirectShow)
					{
						Debug.LogError("[AVProVideo] Loading from buffer is currently only supported in Windows when using the DirectShow API");
					}
				}
				else
				{
					SetPlaybackOptions();
					result = true;
					StartRenderCoroutine();
				}
			}
			return result;
		}

		private bool StartOpenMediaFromBufferInternal(ulong length)
		{
			bool result = false;
			// Open the video file
			if (_controlInterface != null)
			{
				CloseMedia();

				_isMediaOpened = true;
				_autoPlayOnStartTriggered = !_autoPlayOnStart;

				Helper.LogInfo("Starting Opening buffer of length " + length, this);

				if (!_controlInterface.StartOpenMediaFromBuffer(length))
				{
					Debug.LogError("[AVProVideo] Failed to start open video from buffer", this);
					if (GetCurrentPlatformOptions() != PlatformOptionsWindows || PlatformOptionsWindows.videoApi != Windows.VideoApi.DirectShow)
					{
						Debug.LogError("[AVProVideo] Loading from buffer is currently only supported in Windows when using the DirectShow API");
					}
				}
				else
				{
					SetPlaybackOptions();
					result = true;
					StartRenderCoroutine();
				}
			}
			return result;
		}

		private bool AddChunkToBufferInternal(byte[] chunk, ulong offset, ulong chunkSize)
		{
			if (Control != null)
			{
				return Control.AddChunkToMediaBuffer(chunk, offset, chunkSize);
			}

			return false;
		}

		private bool EndOpenMediaFromBufferInternal()
		{
			if (Control != null)
			{
				return Control.EndOpenMediaFromBuffer();
			}

			return false;
		}
	}
}