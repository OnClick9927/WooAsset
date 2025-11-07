using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Attempts to give insight into video playback presentation smoothness quality
	/// Keeps track of skipped and duplicated frames and warns about suboptimal setup
	/// such as no vsync enabled or video frame rate not being a multiple of the display frame rate
	/// </summary>
	public class PlaybackQualityStats
	{
		public int SkippedFrames { get; private set; }
		public int DuplicateFrames { get; private set; }
		public int UnityDroppedFrames { get; private set; }
		public float PerfectFramesT { get; private set; }
		public string VSyncStatus { get; private set; }
		private int PerfectFrames { get; set; }
		private int TotalFrames { get; set; }

		public bool LogIssues { get; set; }

		private int _sameFrameCount;
		private long _lastTimeStamp;
		private BaseMediaPlayer _player;

		public void Reset()
		{
			_sameFrameCount = 0;
			if (_player != null)
			{
				_lastTimeStamp = _player.GetTextureTimeStamp();
			}

			SkippedFrames = 0;
			DuplicateFrames = 0;
			UnityDroppedFrames = 0;
			TotalFrames = 0;
			PerfectFrames = 0;
			PerfectFramesT = 0f;
		}

		internal void Start(BaseMediaPlayer player)
		{
			_player = player;
			Reset();

			bool vsyncEnabled = true;
			if (QualitySettings.vSyncCount == 0)
			{
				vsyncEnabled = false;
				if (LogIssues)
				{
					Debug.LogWarning("[AVProVideo][Quality] VSync is currently disabled in Quality Settings");
				}
			}
			if (!IsGameViewVSyncEnabled())
			{
				vsyncEnabled = false;
				if (LogIssues)
				{
					Debug.LogWarning("[AVProVideo][Quality] VSync is currently disabled in the Game View");
				}
			}

			float frameRate = _player.GetVideoFrameRate();
			float frameMs = (1000f / frameRate);
			if (LogIssues)
			{
				Debug.Log(string.Format("[AVProVideo][Quality] Video: {0}fps {1}ms", frameRate, frameMs));
			}

			if (vsyncEnabled)
			{
				float vsyncRate = (float)Screen.currentResolution.refreshRate / QualitySettings.vSyncCount;
				float vsyncMs = (1000f / vsyncRate);

				if (LogIssues)
				{
					Debug.Log(string.Format("[AVProVideo][Quality] VSync: {0}fps {1}ms", vsyncRate, vsyncMs));
				}

				float framesPerVSync = frameMs / vsyncMs;
				float fractionalframesPerVsync = framesPerVSync - Mathf.FloorToInt(framesPerVSync);
				if (fractionalframesPerVsync > 0.0001f && LogIssues)
				{
					Debug.LogWarning("[AVProVideo][Quality] Video is not a multiple of VSync so playback cannot be perfect");
				}
				VSyncStatus = "VSync " + framesPerVSync;
			}
			else
			{
				if (LogIssues)
				{
					Debug.LogWarning("[AVProVideo][Quality] Running without VSync enabled");
				}
				VSyncStatus = "No VSync";
			}
		}

		internal void Update()
		{
			if (_player == null) return;

			// Don't analyse stats unless real playback is happening
			if (_player.IsPaused() || _player.IsSeeking() || _player.IsFinished()) return;

			long timeStamp = _player.GetTextureTimeStamp();
			long frameDuration = (long)(Helper.SecondsToHNS / _player.GetVideoFrameRate());

			bool isPerfectFrame = true;

			// Check for skipped frames
			long d = (timeStamp - _lastTimeStamp);
			if (d > 0)
			{
				const long threshold = 10000;
				d -= frameDuration;
				if (d > threshold)
				{
					int skippedFrames = Mathf.FloorToInt((float)d / (float)frameDuration);
					if (LogIssues)
					{
						Debug.LogWarning("[AVProVideo][Quality] Possible frame skip, at " + timeStamp + " delta " + d + " = " + skippedFrames + " frames");
					}
					SkippedFrames += skippedFrames;
					isPerfectFrame = false;
				}
			}

			if (QualitySettings.vSyncCount != 0)
			{
				long vsyncDuration = (long)((QualitySettings.vSyncCount * Helper.SecondsToHNS) / (float)Screen.currentResolution.refreshRate);
				if (timeStamp != _lastTimeStamp)
				{
					float framesPerVSync = (float)frameDuration / (float)vsyncDuration;
					//Debug.Log((float)frameDuration + " " +  (float)vsyncDuration);
					float fractionalFramesPerVSync = framesPerVSync - Mathf.FloorToInt(framesPerVSync);

					//Debug.Log(framesPerVSync + " " + fractionalFramesPerVSync);
					// VSync rate is a multiple of the video rate so we should be able to get perfectly smooth playback
					if (fractionalFramesPerVSync <= 0.0001f)
					{
						// Check for duplicate frames
						if (!Mathf.Approximately(_sameFrameCount, (int)framesPerVSync))
						{
							if (LogIssues)
							{
								Debug.LogWarning("[AVProVideo][Quality] Frame " + timeStamp + " was shown for " + _sameFrameCount + " frames instead of expected " + framesPerVSync);
							}
							DuplicateFrames++;
							isPerfectFrame = false;
						}
					}

					_sameFrameCount = 1;
				}
				else
				{
					// Count the number of Unity-frames the video-frame is displayed for
					_sameFrameCount++;
				}

				// Check for Unity dropping frames
				{
					long frameTime = (long)(Time.deltaTime * Helper.SecondsToHNS);
					if (frameTime > (vsyncDuration + (vsyncDuration / 3)))
					{
						if (LogIssues)
						{
							Debug.LogWarning("[AVProVideo][Quality] Possible Unity dropped frame, delta time: " + (Time.deltaTime * 1000f) + "ms");
						}
						UnityDroppedFrames++;
						isPerfectFrame = false;
					}
				}
			}

			if (_lastTimeStamp != timeStamp)
			{
				if (isPerfectFrame)
				{
					PerfectFrames++;
				}
				TotalFrames++;
				PerfectFramesT = (float)PerfectFrames / (float)TotalFrames;
			}

			_lastTimeStamp = timeStamp;
		}

		private static bool IsGameViewVSyncEnabled()
		{
			bool result = true;
#if UNITY_EDITOR && UNITY_2019_1_OR_NEWER
			System.Reflection.Assembly assembly = typeof(UnityEditor.EditorWindow).Assembly;
			System.Type type = assembly.GetType("UnityEditor.GameView");
			UnityEditor.EditorWindow window = UnityEditor.EditorWindow.GetWindow(type);
			System.Reflection.PropertyInfo prop = type.GetProperty("vSyncEnabled");
			if (prop != null)
			{
				result = (bool)prop.GetValue(window);
			}
#endif
			return result;
		}
	}
}