using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2015-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	public static class Helper
	{
		public const string AVProVideoVersion = "2.5.0";
		public sealed class ExpectedPluginVersion
		{
			public const string Windows      = "2.5.0";
			public const string WinRT        = "2.5.0";
			public const string Android      = "2.5.0";
			public const string Apple        = "2.5.0";
		}

		public const string UnityBaseTextureName = "_MainTex";
		public const string UnityBaseTextureName_URP = "_BaseMap";
		public const string UnityBaseTextureName_HDRP = "_BaseColorMap";

		public static string GetPath(MediaPathType location)
		{
			string result = string.Empty;
			switch (location)
			{
				case MediaPathType.AbsolutePathOrURL:
					break;
				case MediaPathType.RelativeToDataFolder:
					result = Application.dataPath;
					break;
				case MediaPathType.RelativeToPersistentDataFolder:
					result = Application.persistentDataPath;
					break;
				case MediaPathType.RelativeToProjectFolder:
#if !UNITY_WINRT_8_1
					string path = "..";
#if UNITY_STANDALONE_OSX && !UNITY_EDITOR_OSX
						path += "/..";
#endif
					result = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, path));
					result = result.Replace('\\', '/');
#endif
					break;
				case MediaPathType.RelativeToStreamingAssetsFolder:
					result = Application.streamingAssetsPath;
					break;
			}
			return result;
		}

		public static string GetFilePath(string path, MediaPathType location)
		{
			string result = string.Empty;
			if (!string.IsNullOrEmpty(path))
			{
				switch (location)
				{
					case MediaPathType.AbsolutePathOrURL:
						result = path;
						break;
					case MediaPathType.RelativeToDataFolder:
					case MediaPathType.RelativeToPersistentDataFolder:
					case MediaPathType.RelativeToProjectFolder:
					case MediaPathType.RelativeToStreamingAssetsFolder:
						result = System.IO.Path.Combine(GetPath(location), path);
						break;
				}
			}
			return result;
		}

		public static string GetFriendlyResolutionName(int width, int height, float fps)
		{
			// List of common 16:9 resolutions
			int[] areas = { 0, 7680 * 4320, 3840 * 2160, 2560 * 1440, 1920 * 1080, 1280 * 720, 853 * 480, 640 * 360, 426 * 240, 256 * 144 };
			string[] names = { "Unknown", "8K", "4K", "1440p", "1080p", "720p", "480p", "360p", "240p", "144p" };

			Debug.Assert(areas.Length == names.Length);

			// Find the closest resolution
			int closestAreaIndex = 0;
			int area = width * height;
			int minDelta = int.MaxValue;
			for (int i = 0; i < areas.Length; i++)
			{
				int d = Mathf.Abs(areas[i] - area);
				// TODO: add a maximum threshold to ignore differences that are too high
				if (d < minDelta)
				{
					closestAreaIndex = i;
					minDelta = d;
					// If the exact mode is found, early out
					if (d == 0)
					{
						break;
					}
				}
			}

			string result = names[closestAreaIndex];

			// Append frame rate if valid
			if (fps > 0f && !float.IsNaN(fps))
			{
				result += fps.ToString("0.##");
			}

			return result;
		}

		public static string GetErrorMessage(ErrorCode code)
		{
			string result = string.Empty;
			switch (code)
			{
				case ErrorCode.None:
					result = "No Error";
					break;
				case ErrorCode.LoadFailed:
					result = "Loading failed.  File not found, codec not supported, video resolution too high or insufficient system resources.";
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
					// Add extra information for older Windows versions that don't have support for modern codecs
					if (SystemInfo.operatingSystem.StartsWith("Windows XP") ||
						SystemInfo.operatingSystem.StartsWith("Windows Vista"))
					{
						result += " NOTE: Windows XP and Vista don't have native support for H.264 codec.  Consider using an older codec such as DivX or installing 3rd party codecs such as LAV Filters.";
					}
#endif
					break;
				case ErrorCode.DecodeFailed:
					result = "Decode failed.  Possible codec not supported, video resolution/bit-depth too high, or insufficient system resources.";
#if UNITY_ANDROID
					result += " On Android this is generally due to the hardware not having enough resources to decode the video. Most Android devices can only handle a maximum of one 4K video at once.";
#endif
					break;
			}
			return result;
		}

		public static string GetPlatformName(Platform platform)
		{
			string result = "Unknown";
			switch (platform)
			{
				case Platform.WindowsUWP:
					result = "Windows UWP";
					break;
				case Platform.MacOSX:
					result = "macOS";
					break;
				default:
					result = platform.ToString();
				break;
			}
			return result;
		}

		public static string[] GetPlatformNames()
		{
			return new string[] {
				GetPlatformName(Platform.Windows),
				GetPlatformName(Platform.MacOSX),
				GetPlatformName(Platform.iOS),
				GetPlatformName(Platform.tvOS),
				GetPlatformName(Platform.Android),
				GetPlatformName(Platform.WindowsUWP),
				GetPlatformName(Platform.WebGL),
			};
		}

#if AVPROVIDEO_DISABLE_LOGGING
		[System.Diagnostics.Conditional("ALWAYS_FALSE")]
#endif
		public static void LogInfo(string message, Object context = null)
		{
			if (context == null)
			{
				Debug.Log("[AVProVideo] " + message);
			}
			else
			{
				Debug.Log("[AVProVideo] " + message, context);
			}
		}

		public static int GetUnityAudioSampleRate()
		{
			// For standalone builds (not in the editor):
			// In Unity 4.6, 5.0, 5.1 when audio is disabled there is no indication from the API.
			// But in 5.2.0 and above, it logs an error when trying to call
			// AudioSettings.GetDSPBufferSize() or AudioSettings.outputSampleRate
			// So to prevent the error, check if AudioSettings.GetConfiguration().sampleRate == 0
			return (AudioSettings.GetConfiguration().sampleRate == 0) ? 0 : AudioSettings.outputSampleRate;
		}

		public static int GetUnityAudioSpeakerCount()
		{
			switch (AudioSettings.GetConfiguration().speakerMode)
			{
				case AudioSpeakerMode.Mono: return 1;
				case AudioSpeakerMode.Stereo: return 2;
				case AudioSpeakerMode.Quad: return 4;
				case AudioSpeakerMode.Surround: return 5;
				case AudioSpeakerMode.Mode5point1: return 6;
				case AudioSpeakerMode.Mode7point1: return 8;
				case AudioSpeakerMode.Prologic: return 2;
			}
			return 0;
		}

		// Returns a valid range to use for a timeline display
		// Either it will return the range 0..duration, or
		// for live streams it will return first seekable..last seekable time
		public static TimeRange GetTimelineRange(double duration, TimeRanges seekable)
		{
			TimeRange result = new TimeRange();
			if (duration >= 0.0 && duration < 2e10)
			{
				// Duration is valid
				result.startTime = 0f;
				result.duration = duration;
			}
			else
			{
				// Duration is invalid, so it could be a live stream, so derive from seekable range
				result.startTime = seekable.MinTime;
				result.duration = seekable.Duration;
			}
			return result;
		}

		public const double SecondsToHNS = 10000000.0;
		public const double MilliSecondsToHNS = 10000.0;

		public static string GetTimeString(double timeSeconds, bool showMilliseconds = false)
		{
			float totalSeconds = (float)timeSeconds;
			int hours = Mathf.FloorToInt(totalSeconds / (60f * 60f));
			float usedSeconds = hours * 60f * 60f;

			int minutes = Mathf.FloorToInt((totalSeconds - usedSeconds) / 60f);
			usedSeconds += minutes * 60f;

			int seconds = Mathf.FloorToInt(totalSeconds - usedSeconds);

			string result;
			if (hours <= 0)
			{
				if (showMilliseconds)
				{
					int milliSeconds = (int)((totalSeconds - Mathf.Floor(totalSeconds)) * 1000f);
					result = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliSeconds);
				}
				else
				{
					result = string.Format("{0:00}:{1:00}", minutes, seconds);
				}
			}
			else
			{
				if (showMilliseconds)
				{
					int milliSeconds = (int)((totalSeconds - Mathf.Floor(totalSeconds)) * 1000f);
					result = string.Format("{2}:{0:00}:{1:00}:{3:000}", minutes, seconds, hours, milliSeconds);
				}
				else
				{
					result = string.Format("{2}:{0:00}:{1:00}", minutes, seconds, hours);
				}
			}

			return result;
		}

		/// <summary>
		/// Convert texture transform matrix to an enum of orientation types
		/// </summary>
		public static Orientation GetOrientation(float[] t)
		{
			Orientation result = Orientation.Landscape;
			if (t != null)
			{
				// TODO: check that the Portrait and PortraitFlipped are the right way around
				if (t[0] == 0f && t[1]== 1f && t[2] == -1f && t[3] == 0f)
				{
					result = Orientation.Portrait;
				} else
				if (t[0] == 0f && t[1] == -1f && t[2] == 1f && t[3] == 0f)
				{
					result = Orientation.PortraitFlipped;
				} else
				if (t[0]== 1f && t[1] == 0f && t[2] == 0f && t[3] == 1f)
				{
					result = Orientation.Landscape;
				} else
				if (t[0] == -1f && t[1] == 0f && t[2] == 0f && t[3] == -1f)
				{
					result = Orientation.LandscapeFlipped;
				}
				else
				if (t[0] == 0f && t[1] == 1f && t[2] == 1f && t[3] == 0f)
				{
					result = Orientation.PortraitHorizontalMirror;
				}
			}
			return result;
		}

		private static Matrix4x4 PortraitMatrix         = Matrix4x4.TRS(new Vector3(0f, 1f, 0f), Quaternion.Euler(0f, 0f, -90f), Vector3.one);
		private static Matrix4x4 PortraitFlippedMatrix  = Matrix4x4.TRS(new Vector3(1f, 0f, 0f), Quaternion.Euler(0f, 0f, 90f), Vector3.one);
		private static Matrix4x4 LandscapeFlippedMatrix = Matrix4x4.TRS(new Vector3(0f, 1f, 0f), Quaternion.Euler(0f, 0f, -90f), Vector3.one);

		public static Matrix4x4 GetMatrixForOrientation(Orientation ori)
		{
			Matrix4x4 result;
			switch (ori)
			{
				case Orientation.Landscape:
					result = Matrix4x4.identity;
					break;
				case Orientation.LandscapeFlipped:
					result = LandscapeFlippedMatrix;
					break;
				case Orientation.Portrait:
					result = PortraitMatrix;
					break;
				case Orientation.PortraitFlipped:
					result = PortraitFlippedMatrix;
					break;
				case Orientation.PortraitHorizontalMirror:
					result = new Matrix4x4();
					result.SetColumn(0, new Vector4(0f, 1f, 0f, 0f));
					result.SetColumn(1, new Vector4(1f, 0f, 0f, 0f));
					result.SetColumn(2, new Vector4(0f, 0f, 1f, 0f));
					result.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));
					break;
				default:
					throw new System.Exception("Unknown Orientation type");
			}
			return result;
		}

		public static int ConvertTimeSecondsToFrame(double seconds, float frameRate)
		{
			// NOTE: Generally you should use RountToInt when converting from time to frame number
			// but because we're adding a half frame offset (which seems to be the safer thing to do) we need to FloorToInt
			seconds = System.Math.Max(0.0, seconds);
			frameRate = Mathf.Max(0f, frameRate);
			return (int)System.Math.Floor(frameRate * seconds);
		}

		public static double ConvertFrameToTimeSeconds(int frame, float frameRate)
		{
			frame = Mathf.Max(0, frame);
			frameRate = Mathf.Max(0f, frameRate);
			double frameDurationSeconds = 1.0 / frameRate;
			return ((double)frame * frameDurationSeconds) + (frameDurationSeconds * 0.5);		// Add half a frame we that the time lands in the middle of the frame range and not at the edges
		}

		public static double FindNextKeyFrameTimeSeconds(double seconds, float frameRate, int keyFrameInterval)
		{
			seconds = System.Math.Max(0.0, seconds);
			frameRate = Mathf.Max(0f, frameRate);
			keyFrameInterval = Mathf.Max(0, keyFrameInterval);
			int currentFrame = Helper.ConvertTimeSecondsToFrame(seconds, frameRate);
			// TODO: allow specifying a minimum number of frames so that if currentFrame is too close to nextKeyFrame, it will calculate the next-next keyframe
			int nextKeyFrame = keyFrameInterval * Mathf.CeilToInt((float)(currentFrame + 1) / (float)keyFrameInterval);
			return Helper.ConvertFrameToTimeSeconds(nextKeyFrame, frameRate);
		}

		public static System.DateTime ConvertSecondsSince1970ToDateTime(double secondsSince1970)
		{
			System.TimeSpan time = System.TimeSpan.FromSeconds(secondsSince1970);
			return new System.DateTime(1970, 1, 1).Add(time);
		}

	#if (UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN))
		[System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, EntryPoint = "GetShortPathNameW", SetLastError=true)]
		private static extern int GetShortPathName([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pathName,
													[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] System.Text.StringBuilder shortName,
													int cbShortName);

		// Handle very long file paths by converting to DOS 8.3 format
		internal static string ConvertLongPathToShortDOS83Path(string path)
		{
			const string pathToken = @"\\?\";
			string result = pathToken + path.Replace("/","\\");
			int length = GetShortPathName(result, null, 0);
			if (length > 0)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder(length);
				if (0 != GetShortPathName(result, sb, length))
				{
					result = sb.ToString().Replace(pathToken, "");
					Debug.LogWarning("[AVProVideo] Long path detected. Changing to DOS 8.3 format");
				}
			}
			return result;
		}
	#endif

		// Converts a non-readable texture to a readable Texture2D.
		// "targetTexture" can be null or you can pass in an existing texture.
		// Remember to Destroy() the returned texture after finished with it
		public static Texture2D GetReadableTexture(Texture inputTexture, bool requiresVerticalFlip, Orientation ori, Texture2D targetTexture = null)
		{
			Texture2D resultTexture = targetTexture;

			RenderTexture prevRT = RenderTexture.active;

			int textureWidth = inputTexture.width;
			int textureHeight = inputTexture.height;
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IPHONE || UNITY_IOS || UNITY_TVOS
			if (ori == Orientation.Portrait || ori == Orientation.PortraitFlipped)
			{
				textureWidth = inputTexture.height;
				textureHeight = inputTexture.width;
			}
#endif

			// Blit the texture to a temporary RenderTexture
			// This handles any format conversion that is required and allows us to use ReadPixels to copy texture from RT to readable texture
			RenderTexture tempRT = RenderTexture.GetTemporary(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32);

			if (ori == Orientation.Landscape)
			{
				if (!requiresVerticalFlip)
				{
					Graphics.Blit(inputTexture, tempRT);
				}
				else
				{
					// The above Blit can't flip unless using a material, so we use Graphics.DrawTexture instead
					GL.PushMatrix();
					RenderTexture.active = tempRT;
					GL.LoadPixelMatrix(0f, tempRT.width, 0f, tempRT.height);
					Rect sourceRect = new Rect(0f, 0f, 1f, 1f);
					// NOTE: not sure why we need to set y to -1, without this there is a 1px gap at the bottom
					Rect destRect = new Rect(0f, -1f, tempRT.width, tempRT.height);

					Graphics.DrawTexture(destRect, inputTexture, sourceRect, 0, 0, 0, 0);
					GL.PopMatrix();
					GL.InvalidateState();
				}
			}
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IPHONE || UNITY_IOS || UNITY_TVOS
			else
			{
				Matrix4x4 m = Matrix4x4.identity;
				switch (ori)
				{
					case Orientation.Portrait:
						m = Matrix4x4.TRS(new Vector3(0f, inputTexture.width, 0f), Quaternion.Euler(0f, 0f, -90f), Vector3.one);
						break;
					case Orientation.PortraitFlipped:
						m = Matrix4x4.TRS(new Vector3(inputTexture.height, 0f, 0f), Quaternion.Euler(0f, 0f, 90f), Vector3.one);
						break;
					case Orientation.LandscapeFlipped:
						m = Matrix4x4.TRS(new Vector3(inputTexture.width, inputTexture.height, 0f), Quaternion.identity, new Vector3(-1f, -1f, 1f));
						break;
				}

				// The above Blit can't flip unless using a material, so we use Graphics.DrawTexture instead
				GL.InvalidateState();
				GL.PushMatrix();
				GL.Clear(false, true, Color.red);
				RenderTexture.active = tempRT;
				GL.LoadPixelMatrix(0f, tempRT.width, 0f, tempRT.height);
				Rect sourceRect = new Rect(0f, 0f, 1f, 1f);
				// NOTE: not sure why we need to set y to -1, without this there is a 1px gap at the bottom
				Rect destRect = new Rect(0f, -1f, inputTexture.width, inputTexture.height);
				GL.MultMatrix(m);

				Graphics.DrawTexture(destRect, inputTexture, sourceRect, 0, 0, 0, 0);
				GL.PopMatrix();
				GL.InvalidateState();
			}
#endif

			if (resultTexture == null)
			{
				resultTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
			}

			RenderTexture.active = tempRT;
			resultTexture.ReadPixels(new Rect(0f, 0f, textureWidth, textureHeight), 0, 0, false);
			resultTexture.Apply(false, false);
			RenderTexture.ReleaseTemporary(tempRT);

			RenderTexture.active = prevRT;

			return resultTexture;
		}

		// Converts a non-readable texture to a readable Texture2D.
		// "targetTexture" can be null or you can pass in an existing texture.
		// Remember to Destroy() the returned texture after finished with it
		public static Texture2D GetReadableTexture(RenderTexture inputTexture, Texture2D targetTexture = null)
		{
			if (targetTexture == null)
			{
				targetTexture = new Texture2D(inputTexture.width, inputTexture.height, TextureFormat.ARGB32, false);
			}

			RenderTexture prevRT = RenderTexture.active;
			RenderTexture.active = inputTexture;
			targetTexture.ReadPixels(new Rect(0f, 0f, inputTexture.width, inputTexture.height), 0, 0, false);
			targetTexture.Apply(false, false);
			RenderTexture.active = prevRT;

			return targetTexture;
		}
	}
}
