using UnityEngine;
using System.Collections;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	public partial class MediaPlayer : MonoBehaviour
	{
#region Extract Frame

		private bool ForceWaitForNewFrame(int lastFrameCount, float timeoutMs)
		{
			bool result = false;
			// Wait for the frame to change, or timeout to happen (for the case that there is no new frame for this time)
			System.DateTime startTime = System.DateTime.Now;
			int iterationCount = 0;
			while (Control != null && (System.DateTime.Now - startTime).TotalMilliseconds < (double)timeoutMs)
			{
				_playerInterface.Update();

				// TODO: check if Seeking has completed!  Then we don't have to wait

				// If frame has changed we can continue
				// NOTE: this will never happen because GL.IssuePlugin.Event is never called in this loop
				if (lastFrameCount != TextureProducer.GetTextureFrameCount())
				{
					result = true;
					break;
				}

				iterationCount++;

				// NOTE: we tried to add Sleep for 1ms but it was very slow, so switched to this time based method which burns more CPU but about double the speed
				// NOTE: had to add the Sleep back in as after too many iterations (over 1000000) of GL.IssuePluginEvent Unity seems to lock up
				// NOTE: seems that GL.IssuePluginEvent can't be called if we're stuck in a while loop and they just stack up
				//System.Threading.Thread.Sleep(0);
			}

			_playerInterface.Render();

			return result;
		}
		
		/// <summary>
		/// Create or return (if cached) a camera that is inactive and renders nothing
		/// This camera is used to call .Render() on which causes the render thread to run
		/// This is useful for forcing GL.IssuePluginEvent() to run and is used for
		/// wait for frames to render for ExtractFrame() and UpdateTimeScale()
		/// </summary>
		private static Camera GetDummyCamera()
		{
			if (_dummyCamera == null)
			{
				const string goName = "AVPro Video Dummy Camera";
				GameObject go = GameObject.Find(goName);
				if (go == null)
				{
					go = new GameObject(goName);
					go.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
					go.SetActive(false);
					Object.DontDestroyOnLoad(go);

					_dummyCamera = go.AddComponent<Camera>();
					_dummyCamera.hideFlags = HideFlags.HideInInspector | HideFlags.DontSave;
					_dummyCamera.cullingMask = 0;
					_dummyCamera.clearFlags = CameraClearFlags.Nothing;
					_dummyCamera.enabled = false;
				}
				else
				{
					_dummyCamera = go.GetComponent<Camera>();
				}
			}
			//Debug.Assert(_dummyCamera != null);
			return _dummyCamera;
		}

		private IEnumerator ExtractFrameCoroutine(Texture2D target, ProcessExtractedFrame callback, double timeSeconds = -1.0, bool accurateSeek = true, int timeoutMs = 1000, int timeThresholdMs = 100)
		{
#if (!UNITY_EDITOR && UNITY_ANDROID) || UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_IOS || UNITY_TVOS
			Texture2D result = target;

			Texture frame = null;

			if (_controlInterface != null)
			{
				if (timeSeconds >= 0f)
				{
					Pause();

					// If the right frame is already available (or close enough) just grab it
					if (TextureProducer.GetTexture() != null && (System.Math.Abs(_controlInterface.GetCurrentTime() - timeSeconds) < (timeThresholdMs / 1000.0)))
					{
						frame = TextureProducer.GetTexture();
					}
					else
					{
						int preSeekFrameCount = _textureInterface.GetTextureFrameCount();

						// Seek to the frame
						if (accurateSeek)
						{
							_controlInterface.Seek(timeSeconds);
						}
						else
						{
							_controlInterface.SeekFast(timeSeconds);
						}

						// Wait for the new frame to arrive
						if (!_controlInterface.WaitForNextFrame(GetDummyCamera(), preSeekFrameCount))
						{
							// If WaitForNextFrame fails (e.g. in android single threaded), we run the below code to asynchronously wait for the frame
							int currFc = TextureProducer.GetTextureFrameCount();
							int iterations = 0;
							int maxIterations = 50;

							//+1 as often there will be an extra frame produced after pause (so we need to wait for the second frame instead)
							while((currFc + 1) >= TextureProducer.GetTextureFrameCount() && iterations++ < maxIterations)
							{
								yield return null;
							}
						}
						frame = TextureProducer.GetTexture();
					}
				}
				else
				{
					frame = TextureProducer.GetTexture();
				}
			}
			if (frame != null)
			{
				result = Helper.GetReadableTexture(frame, TextureProducer.RequiresVerticalFlip(), Helper.GetOrientation(Info.GetTextureTransform()), target);
			}
#else
			Texture2D result = ExtractFrame(target, timeSeconds, accurateSeek, timeoutMs, timeThresholdMs);
#endif
			callback(result);

			yield return null;
		}

		public void ExtractFrameAsync(Texture2D target, ProcessExtractedFrame callback, double timeSeconds = -1.0, bool accurateSeek = true, int timeoutMs = 1000, int timeThresholdMs = 100)
		{
			StartCoroutine(ExtractFrameCoroutine(target, callback, timeSeconds, accurateSeek, timeoutMs, timeThresholdMs));
		}

		// "target" can be null or you can pass in an existing texture.
		public Texture2D ExtractFrame(Texture2D target, double timeSeconds = -1.0, bool accurateSeek = true, int timeoutMs = 1000, int timeThresholdMs = 100)
		{
			Texture2D result = target;

			// Extract frames returns the internal frame of the video player
			Texture frame = ExtractFrame(timeSeconds, accurateSeek, timeoutMs, timeThresholdMs);
			if (frame != null)
			{
				result = Helper.GetReadableTexture(frame, TextureProducer.RequiresVerticalFlip(), Helper.GetOrientation(Info.GetTextureTransform()), target);
			}

			return result;
		}

		private Texture ExtractFrame(double timeSeconds = -1.0, bool accurateSeek = true, int timeoutMs = 1000, int timeThresholdMs = 100)
		{
			Texture result = null;

			if (_controlInterface != null)
			{
				if (timeSeconds >= 0f)
				{
					Pause();

					// If the right frame is already available (or close enough) just grab it
					if (TextureProducer.GetTexture() != null && (System.Math.Abs(_controlInterface.GetCurrentTime() - timeSeconds) < (timeThresholdMs / 1000.0)))
					{
						result = TextureProducer.GetTexture();
					}
					else
					{
						// Store frame count before seek
						int frameCount = TextureProducer.GetTextureFrameCount();

						// Seek to the frame
						if (accurateSeek)
						{
							_controlInterface.Seek(timeSeconds);
						}
						else
						{
							_controlInterface.SeekFast(timeSeconds);
						}

						// Wait for frame to change
						ForceWaitForNewFrame(frameCount, timeoutMs);
						result = TextureProducer.GetTexture();
					}
				}
				else
				{
					result = TextureProducer.GetTexture();
				}
			}
			return result;
		}
#endregion // Extract Frame
	}
}