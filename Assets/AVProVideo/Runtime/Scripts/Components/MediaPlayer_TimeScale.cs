using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	public partial class MediaPlayer : MonoBehaviour
	{

#region Support for Time Scale
#if AVPROVIDEO_BETA_SUPPORT_TIMESCALE
		// Adjust this value to get faster performance but may drop frames.
		// Wait longer to ensure there is enough time for frames to process
		private const float TimeScaleTimeoutMs = 20f;
		private bool _timeScaleIsControlling;
		private double _timeScaleVideoTime;

		private void UpdateTimeScale()
		{
			if (Time.timeScale != 1f || Time.captureFramerate != 0)
			{
				if (_controlInterface.IsPlaying())
				{
					_controlInterface.Pause();
					_timeScaleIsControlling = true;
					_timeScaleVideoTime = _controlInterface.GetCurrentTime();
				}

				if (_timeScaleIsControlling)
				{
					// Progress time
					_timeScaleVideoTime += Time.deltaTime;

					// Handle looping
					if (_controlInterface.IsLooping() && _timeScaleVideoTime >= Info.GetDuration())
					{
						// TODO: really we should seek to (_timeScaleVideoTime % Info.GetDuration())
						_timeScaleVideoTime = 0.0;
					}

					int preSeekFrameCount = TextureProducer.GetTextureFrameCount();

					// Seek to the new time
					{
						double preSeekTime = Control.GetCurrentTime();

						// Seek
						_controlInterface.Seek(_timeScaleVideoTime);

						// Early out, if after the seek the time hasn't changed, the seek was probably too small to go to the next frame.
						// TODO: This behaviour may be different on other platforms (not Windows) and needs more testing.
						if (Mathf.Approximately((float)preSeekTime, (float)_controlInterface.GetCurrentTime()))
						{
							return;
						}
					}

					// Wait for the new frame to arrive
					if (!_controlInterface.WaitForNextFrame(GetDummyCamera(), preSeekFrameCount))
					{
						// If WaitForNextFrame fails (e.g. in android single threaded), we run the below code to asynchronously wait for the frame
						System.DateTime startTime = System.DateTime.Now;
						int lastFrameCount = TextureProducer.GetTextureFrameCount();

						while (_controlInterface != null && (System.DateTime.Now - startTime).TotalMilliseconds < (double)TimeScaleTimeoutMs)
						{
							_playerInterface.Update();
							_playerInterface.Render();
							GetDummyCamera().Render();
							if (lastFrameCount != TextureProducer.GetTextureFrameCount())
							{
								break;
							}
						}
					}
				}
			}
			else
			{
				// Restore playback when timeScale becomes 1
				if (_timeScaleIsControlling)
				{
					_controlInterface.Play();
					_timeScaleIsControlling = false;
				}
			}
		}
#endif
#endregion // Support for Time Scale
	}
}