using UnityEngine;
#if NETFX_CORE
using Windows.Storage.Streams;
#endif

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	public partial class MediaPlayer : MonoBehaviour
	{

#if NETFX_CORE
		public bool OpenVideoFromStream(IRandomAccessStream ras, string path, bool autoPlay = true)
		{
			_videoLocation = FileLocation.AbsolutePathOrURL;
			_videoPath = path;
			_autoPlayOnStart = autoPlay;

			if (_controlInterface == null)
			{
				Initialise();
			}

			return OpenVideoFromStream(ras);
		}

		private bool OpenVideoFromStream(IRandomAccessStream ras)
		{
			bool result = false;
			// Open the video file
			if (_controlInterface != null)
			{
				CloseVideo();

				_isVideoOpened = true;
				_autoPlayOnStartTriggered = !_autoPlayOnStart;

				// Potentially override the file location
				long fileOffset = GetPlatformFileOffset();

				if (!Control.OpenVideoFromFile(ras, _videoPath, fileOffset, null, _manuallySetAudioSourceProperties ? _sourceAudioSampleRate : 0,
					_manuallySetAudioSourceProperties ? _sourceAudioChannels : 0))
				{
					Debug.LogError("[AVProVideo] Failed to open " + _videoPath, this);
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
#endif
	}
}