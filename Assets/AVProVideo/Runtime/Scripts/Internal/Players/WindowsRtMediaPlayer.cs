// NOTE: We only allow this script to compile in editor so we can easily check for compilation issues
#if (UNITY_EDITOR || (UNITY_STANDALONE_WIN || UNITY_WSA_10_0))

#if UNITY_WSA_10 || ENABLE_IL2CPP
	#define AVPROVIDEO_MARSHAL_RETURN_BOOL
#endif

using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;
using System.Text;

//-----------------------------------------------------------------------------
// Copyright 2018-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	public enum PlaybackState
	{
		None = 0,
		Opening = 1,
		Buffering = 2,		// Replace with Stalled and add Buffering as State 64??
		Playing = 3,
		Paused = 4,
		StateMask = 7,

		Seeking = 32,
	}

	public class AuthData
	{
		public string URL { get; set; }
		public string Token { get; set; }
		public byte[] KeyBytes { get; set; }

		public AuthData()
		{
			Clear();
		}

		public void Clear()
		{
			URL = string.Empty;
			Token = string.Empty;
			KeyBytes = null;
		}

		public string KeyBase64
		{
			get
			{
				if (KeyBytes != null)
				{
					return System.Convert.ToBase64String(KeyBytes);
				}
				else
				{
					return string.Empty;
				}
			}
			set
			{
				if (value != null)
				{
					KeyBytes = System.Convert.FromBase64String(value);
				}
				else
				{
					KeyBytes = null;
				}
			}
		}
	};

	public partial class WindowsRtMediaPlayer : BaseMediaPlayer
	{
		private bool _isMediaLoaded = false;
		private bool _use10BitTextures = false;
		private bool _useLowLiveLatency = false;

		public WindowsRtMediaPlayer(MediaPlayer.OptionsWindows options) : base()
		{
			_playerDescription = "WinRT";
			_use10BitTextures = options.use10BitTextures;
			_useLowLiveLatency = options.useLowLiveLatency;
			for (int i = 0; i < _eyeTextures.Length; i++)
			{
				_eyeTextures[i] = new EyeTexture();
			}
		}

		public WindowsRtMediaPlayer(MediaPlayer.OptionsWindowsUWP options) : base()
		{
			_playerDescription = "WinRT";
			_use10BitTextures = options.use10BitTextures;
			_useLowLiveLatency = options.useLowLiveLatency;
			for (int i = 0; i < _eyeTextures.Length; i++)
			{
				_eyeTextures[i] = new EyeTexture();
			}
		}

		public override bool CanPlay()
		{
			return HasMetaData();
		}

		public override void Dispose()
		{
			CloseMedia();
			if (_playerInstance != System.IntPtr.Zero)
			{
				Native.DestroyPlayer(_playerInstance); _playerInstance = System.IntPtr.Zero;
				Native.IssueRenderThreadEvent_FreeAllTextures();
			}
			for (int i = 0; i < _eyeTextures.Length; i++)
			{
				_eyeTextures[i].Dispose();
			}
		}

		public override bool PlayerSupportsLinearColorSpace()
		{
			// The current player doesn't support rendering to SRGB textures
			return false;
		}

		public override double GetCurrentTime()
		{
			return Native.GetCurrentPosition(_playerInstance);
		}

		public override double GetDuration()
		{
			return Native.GetDuration(_playerInstance);
		}

		public override float GetPlaybackRate()
		{
			return Native.GetPlaybackRate(_playerInstance);
		}

		public override Texture GetTexture(int index = 0)
		{
			Texture result = null;
			if (_frameTimeStamp > 0 && index < _eyeTextures.Length)
			{
				result = _eyeTextures[index].texture;
			}
			return result;
		}

		public override int	GetTextureCount()
		{
			if (_eyeTextures[1].texture != null)
			{
				return 2;
			}
			return 1;
		}

		public override int GetTextureFrameCount()
		{
			return (int)_frameTimeStamp;
		}

		internal override StereoPacking InternalGetTextureStereoPacking()
		{
			return Native.GetStereoPacking(_playerInstance);
		}

		public override string GetVersion()
		{
			return _version;
		}

		public override string GetExpectedVersion()
		{
			return Helper.ExpectedPluginVersion.WinRT;
		}

		public override float GetVideoFrameRate()
		{
			float result = 0f;
			Native.VideoTrack videoTrack;
			if (Native.GetActiveVideoTrackInfo(_playerInstance, out videoTrack))
			{
				result = videoTrack.frameRate;
			}
			return result;
		}

		public override int GetVideoWidth()
		{
			int result = 0;
			if (_eyeTextures[0].texture)
			{
				result = _eyeTextures[0].texture.width;
			}
			return result;
		}

		public override int GetVideoHeight()
		{
			int result = 0;
			if (_eyeTextures[0].texture)
			{
				result = _eyeTextures[0].texture.height;
			}
			return result;
		}

		public override float GetVolume()
		{
			return Native.GetAudioVolume(_playerInstance);
		}

		public override void SetBalance(float balance)
		{
			Native.SetAudioBalance(_playerInstance, balance);
		}

		public override float GetBalance()
		{
			return Native.GetAudioBalance(_playerInstance);
		}

		public override bool HasAudio()
		{
			return _audioTracks.Count > 0;
		}

		public override bool HasMetaData()
		{
			return Native.GetDuration(_playerInstance) > 0f;
		}

		public override bool HasVideo()
		{
			return _videoTracks.Count > 0;
		}

		public override bool IsBuffering()
		{
			return ((Native.GetPlaybackState(_playerInstance) & PlaybackState.StateMask) == PlaybackState.Buffering);
		}

		public override bool IsFinished()
		{
			bool result = false;
			if (IsPaused() && !IsSeeking() && GetCurrentTime() >= GetDuration())
			{
				result = true;
			}
			return result;
		}

		public override bool IsLooping()
		{
			return Native.IsLooping(_playerInstance);
		}

		public override bool IsMuted()
		{
			return Native.IsAudioMuted(_playerInstance);
		}

		public override bool IsPaused()
		{
			return ((Native.GetPlaybackState(_playerInstance) & PlaybackState.StateMask) == PlaybackState.Paused);
		}

		public override bool IsPlaying()
		{
			return ((Native.GetPlaybackState(_playerInstance) & PlaybackState.StateMask) == PlaybackState.Playing);
		}

		public override bool IsSeeking()
		{
			return ((Native.GetPlaybackState(_playerInstance) & PlaybackState.Seeking) != 0);
		}

		public override void MuteAudio(bool bMuted)
		{
			Native.SetAudioMuted(_playerInstance, bMuted);
		}
		
		// TODO: replace all these options with a structure
		public override bool OpenMedia(string path, long offset, string httpHeader, MediaHints mediaHints, int forceFileFormat = 0, bool startWithHighestBitrate = false)
		{
			bool result = false;

			CloseMedia();

			if (_playerInstance == System.IntPtr.Zero)
			{
				_playerInstance = Native.CreatePlayer();

				// Force setting any auth data as it wouldn't have been set without a _playerInstance
				AuthenticationData = _nextAuthData;
			}
			if (_playerInstance != System.IntPtr.Zero)
			{
				result = Native.OpenMedia(_playerInstance, path, httpHeader, (FileFormat)forceFileFormat, startWithHighestBitrate, _use10BitTextures);
				if (result)
				{
					if (_useLowLiveLatency)
					{
						Native.SetLiveOffset(_playerInstance, 0.0);
					}
				}
				_mediaHints = mediaHints;
			}
			
			return result;
		}

		public override void CloseMedia()
		{
			// NOTE: This unloads the current video, but the texture should remain
			_isMediaLoaded = false;
			Native.CloseMedia(_playerInstance);

			base.CloseMedia();
		}

		public override void Pause()
		{
			Native.Pause(_playerInstance);
		}

		public override void Play()
		{
			Native.Play(_playerInstance);
		}

		public override void Render()
		{
			Native.IssueRenderThreadEvent_UpdateAllTextures();
		}

		private void Update_Textures()
		{
			// See if there is a new frame ready
			{
				System.IntPtr texturePointerLeft = System.IntPtr.Zero;
				System.IntPtr texturePointerRight = System.IntPtr.Zero;
				ulong frameTimeStamp = 0;
				int width, height;
				if (Native.GetLatestFrame(_playerInstance, out texturePointerLeft, out texturePointerRight, out frameTimeStamp, out width, out height))
				{
					bool isFrameUpdated = false;
					bool isNewFrameTime = (frameTimeStamp > _frameTimeStamp) || (_frameTimeStamp == 0 && frameTimeStamp == 0);
					for (int i = 0; i < _eyeTextures.Length; i++)
					{
						EyeTexture eyeTexture = _eyeTextures[i];
						System.IntPtr texturePointer = texturePointerLeft;
						if (i == 1)
						{
							texturePointer = texturePointerRight;
						}
					
						bool isNewFrameSpecs = (eyeTexture.texture != null && (texturePointer == IntPtr.Zero || eyeTexture.texture.width != width || eyeTexture.texture.height != height));
						//Debug.Log("tex? " + i + " " + width + " " + height + " " + (eyeTexture.texture != null) + " " + texturePointer.ToString() + " " + frameTimeStamp);
					
						// Check whether the latest frame is newer than the one we got last time
						if (isNewFrameTime || isNewFrameSpecs)
						{
							if (isNewFrameSpecs)
							{
								eyeTexture.Dispose();
								// TODO: blit from the old texture to the new texture before destroying?
							}

							/// Switch to the latest texture pointer
							if (eyeTexture.texture != null)
							{
								// TODO: check whether UpdateExternalTexture resets the sampling filter to POINT - it seems to in Unity 5.6.6
								if (eyeTexture.nativePointer != texturePointer)
								{
									eyeTexture.texture.UpdateExternalTexture(texturePointer);
									eyeTexture.nativePointer = texturePointer;
								}
							}
							else
							{
								if (texturePointer != IntPtr.Zero)
								{
									eyeTexture.texture = Texture2D.CreateExternalTexture(width, height, TextureFormat.RGBA32, false, false, texturePointer);
									if (eyeTexture.texture != null)
									{
										eyeTexture.texture.name = "AVProVideo";
										eyeTexture.nativePointer = texturePointer;
										ApplyTextureProperties(eyeTexture.texture);
									}
									else
									{
										Debug.LogError("[AVProVideo] Failed to create texture");
									}
								}
							}

							isFrameUpdated = true;
						}
					}
					if (isFrameUpdated)
					{
						_frameTimeStamp = frameTimeStamp;
					}
				}
			}
		}

		private AuthData _nextAuthData = new AuthData();

		public AuthData AuthenticationData
		{ 
			get 
			{ 
				return _nextAuthData;
			}
			set
			{
				_nextAuthData = value;
				Native.SetNextAuthData(_playerInstance, _nextAuthData);
			}
		}

		public override bool RequiresVerticalFlip()
		{
			return true;
		}

		public override void Seek(double time)
		{
			Native.SeekParams seekParams = new Native.SeekParams();
			seekParams.timeSeconds = time;
			seekParams.mode = Native.SeekMode.Accurate;
			Native.Seek(_playerInstance, ref seekParams);
		}

		public override void SeekFast(double time)
		{
			// Keyframe seeking is not supported on this platform
			Seek(time);
		}

		public override void SetLooping(bool bLooping)
		{
			Native.SetLooping(_playerInstance, bLooping);
		}

		public override void SetPlaybackRate(float rate)
		{
			// Clamp rate as WinRT doesn't seem to be able to handle negative rate
			rate = Mathf.Max(0f, rate);
			Native.SetPlaybackRate(_playerInstance, rate);
		}

		public override void SetVolume(float volume)
		{
			Native.SetAudioVolume(_playerInstance, volume);
		}

		public override void Stop()
		{
			Pause();
		}

		private void UpdateTimeRanges()
		{	
			UpdateTimeRange(ref _seekableTimes._ranges, Native.TimeRangeTypes.Seekable);
			UpdateTimeRange(ref _bufferedTimes._ranges, Native.TimeRangeTypes.Buffered);
			_seekableTimes.CalculateRange();
			_bufferedTimes.CalculateRange();
		}

		private void UpdateTimeRange(ref TimeRange[] range, Native.TimeRangeTypes timeRangeType)
		{
			int newCount = Native.GetTimeRanges(_playerInstance, range, range.Length, timeRangeType);
			if (newCount != range.Length)
			{
				range = new TimeRange[newCount];
				Native.GetTimeRanges(_playerInstance, range, range.Length, timeRangeType);
			}
		}

		public override System.DateTime GetProgramDateTime()
		{
			double seconds = Native.GetCurrentDateTimeSecondsSince1970(_playerInstance);
			return Helper.ConvertSecondsSince1970ToDateTime(seconds);
		}

		public override void Update()
		{
			Native.Update(_playerInstance);
			UpdateTracks();
			UpdateTextCue();

			_lastError = (ErrorCode)Native.GetLastErrorCode(_playerInstance);

			UpdateTimeRanges();
			UpdateSubtitles();
			Update_Textures();
			UpdateDisplayFrameRate();

			if (!_isMediaLoaded)
			{
				if (HasVideo() && _eyeTextures[0].texture != null)
				{
					Native.VideoTrack videoTrack;
					if (Native.GetActiveVideoTrackInfo(_playerInstance, out videoTrack))
					{	
						Helper.LogInfo("Using playback path: " + _playerDescription + " (" + videoTrack.frameWidth + "x" + videoTrack.frameHeight + "@" + videoTrack.frameRate.ToString("F2") + ")");
						_isMediaLoaded = true;
					}
				}
				else if (HasAudio() && !HasVideo())
				{
					Helper.LogInfo("Using playback path: " + _playerDescription);
					_isMediaLoaded = true;
				}
			}
		}

		/*public override void SetKeyServerURL(string url)
		{
			_nextAuthData.URL = url;
			AuthenticationData = _nextAuthData;	
		}*/

		public override void SetKeyServerAuthToken(string token)
		{
			_nextAuthData.Token = token;
			AuthenticationData = _nextAuthData;	
		}

		public override void SetOverrideDecryptionKey(byte[] key)
		{
			_nextAuthData.KeyBytes = key;
			AuthenticationData = _nextAuthData;	
		}
	}

	// Tracks
	public sealed partial class WindowsRtMediaPlayer
	{	
		internal override bool InternalSetActiveTrack(TrackType trackType, int trackUid)
		{
			return Native.SetActiveTrack(_playerInstance, trackType, trackUid);
		}

		// Has it changed since the last frame 'tick'
		internal override bool InternalIsChangedTracks(TrackType trackType)
		{
			return Native.IsChangedTracks(_playerInstance, trackType);
		}

		internal override int InternalGetTrackCount(TrackType trackType)
		{
			return Native.GetTrackCount(_playerInstance, trackType);
		}

		internal override TrackBase InternalGetTrackInfo(TrackType trackType, int trackIndex, ref bool isActiveTrack)
		{
			TrackBase result = null;
			StringBuilder name = new StringBuilder(128);
			StringBuilder language = new StringBuilder(16);
			int uid = -1;
			if (Native.GetTrackInfo(_playerInstance, trackType, trackIndex, ref uid, ref isActiveTrack, name, name.Capacity, language, language.Capacity))
			{
				if (trackType == TrackType.Video)
				{
					result = new VideoTrack(uid, name.ToString(), language.ToString(), false);
				}
				else if (trackType == TrackType.Audio)
				{
					result = new AudioTrack(uid, name.ToString(), language.ToString(), false);
				}
				else if (trackType == TrackType.Text)
				{
					result = new TextTrack(uid, name.ToString(), language.ToString(), false);
				}
			}
			return result;
		}

		private partial struct Native
		{
			[DllImport("AVProVideoWinRT")]
			[return: MarshalAs(UnmanagedType.I1)]
			public static extern bool IsChangedTracks(System.IntPtr instance, TrackType trackType);

			[DllImport("AVProVideoWinRT")]
			public static extern int GetTrackCount(System.IntPtr instance, TrackType trackType);

			[DllImport("AVProVideoWinRT")]
			[return: MarshalAs(UnmanagedType.I1)]
			public static extern bool GetTrackInfo(System.IntPtr instance, TrackType trackType, int index, ref int uid, 
										ref bool isActive,
										[MarshalAs(UnmanagedType.LPWStr)] StringBuilder name, int maxNameLength,
										[MarshalAs(UnmanagedType.LPWStr)] StringBuilder language, int maxLanguageLength);

			[DllImport("AVProVideoWinRT")]
			[return: MarshalAs(UnmanagedType.I1)]
			public static extern bool SetActiveTrack(System.IntPtr instance, TrackType trackType, int trackUid);
		}
	}

	// Text Cue
	public sealed partial class WindowsRtMediaPlayer
	{	
		// Has it changed since the last frame 'tick'
		internal override bool InternalIsChangedTextCue()
		{
			return Native.IsChangedTextCue(_playerInstance);
		}

		internal override string InternalGetCurrentTextCue()
		{
			string result = null;
			System.IntPtr ptr = Native.GetCurrentTextCue(_playerInstance);
			if (ptr != System.IntPtr.Zero)
			{
				result = System.Runtime.InteropServices.Marshal.PtrToStringUni(ptr);
			}
			return result;
		}

		private partial struct Native
		{
			[DllImport("AVProVideoWinRT")]
			[return: MarshalAs(UnmanagedType.I1)]
			public static extern bool IsChangedTextCue(System.IntPtr instance);

			[DllImport("AVProVideoWinRT")]
			public static extern System.IntPtr GetCurrentTextCue(System.IntPtr instance);
		}
	}

	public sealed partial class WindowsRtMediaPlayer
	{	
		private partial struct Native
		{
			[DllImport("AVProVideoWinRT", EntryPoint = "GetPluginVersion")]
			private static extern System.IntPtr GetPluginVersionStringPointer();

			public static string GetPluginVersion()
			{
				return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(GetPluginVersionStringPointer());
			}

			[DllImport("AVProVideoWinRT")]
			public static extern System.IntPtr CreatePlayer();

			[DllImport("AVProVideoWinRT")]
			public static extern void DestroyPlayer(System.IntPtr playerInstance);

			[DllImport("AVProVideoWinRT")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool OpenMedia(System.IntPtr playerInstance, [MarshalAs(UnmanagedType.LPWStr)] string filePath, 
												[MarshalAs(UnmanagedType.LPWStr)] string httpHeader, FileFormat overrideFileFormat, 
												bool startWithHighestBitrate, bool use10BitTextures);

			[DllImport("AVProVideoWinRT")]
			public static extern void CloseMedia(System.IntPtr playerInstance);


			[DllImport("AVProVideoWinRT")]
			public static extern void Pause(System.IntPtr playerInstance);

			[DllImport("AVProVideoWinRT")]
			public static extern void Play(System.IntPtr playerInstance);

			[DllImport("AVProVideoWinRT")]
			public static extern void SetAudioVolume(System.IntPtr playerInstance, float volume);

			[DllImport("AVProVideoWinRT")]
			public static extern void SetAudioBalance(System.IntPtr playerInstance, float balance);

			[DllImport("AVProVideoWinRT")]
			public static extern void SetPlaybackRate(System.IntPtr playerInstance, float rate);

			[DllImport("AVProVideoWinRT")]
			public static extern void SetAudioMuted(System.IntPtr playerInstance, bool muted);

			[DllImport("AVProVideoWinRT")]
			public static extern float GetAudioVolume(System.IntPtr playerInstance);

			[DllImport("AVProVideoWinRT")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool IsAudioMuted(System.IntPtr playerInstance);

			[DllImport("AVProVideoWinRT")]
			public static extern float GetAudioBalance(System.IntPtr playerInstance);

			[DllImport("AVProVideoWinRT")]
			public static extern float GetPlaybackRate(System.IntPtr playerInstance);

			[DllImport("AVProVideoWinRT")]
			public static extern void SetLooping(System.IntPtr playerInstance, bool looping);

			[DllImport("AVProVideoWinRT")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool IsLooping(System.IntPtr playerInstance);

			[DllImport("AVProVideoWinRT")]
			public static extern int GetLastErrorCode(System.IntPtr playerInstance);


			[DllImport("AVProVideoWinRT")]
			public static extern void Update(System.IntPtr playerInstance);

			[DllImport("AVProVideoWinRT")]
			public static extern double GetDuration(System.IntPtr playerInstance);

			[DllImport("AVProVideoWinRT")]
			public static extern StereoPacking GetStereoPacking(System.IntPtr playerInstance);

			[DllImport("AVProVideoWinRT")]
			public static extern double GetCurrentPosition(System.IntPtr playerInstance);

			[DllImport("AVProVideoWinRT")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool GetLatestFrame(System.IntPtr playerInstance, out System.IntPtr leftEyeTexturePointer, out System.IntPtr rightEyeTexturePointer, out ulong frameTimeStamp, out int width, out int height);

			[DllImport("AVProVideoWinRT")]
			public static extern PlaybackState GetPlaybackState(System.IntPtr playerInstance);

			[DllImport("AVProVideoWinRT")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool GetActiveVideoTrackInfo(System.IntPtr playerInstance, out VideoTrack videoTrack);

			[DllImport("AVProVideoWinRT")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool GetActiveAudioTrackInfo(System.IntPtr playerInstance, out AudioTrack audioTrack);

			[DllImport("AVProVideoWinRT")]
			public static extern double GetCurrentDateTimeSecondsSince1970(System.IntPtr playerInstance);

			[DllImport("AVProVideoWinRT")]
			public static extern void SetLiveOffset(System.IntPtr playerInstance, double seconds);

			[DllImport("AVProVideoWinRT")]
			public static extern void DebugValues(System.IntPtr playerInstance, out int isD3D, out int isUnityD3D, out int isTexture, out int isSharedTexture, out int isSurface);

			public enum SeekMode
			{
				Fast = 0,
				Accurate = 1,
				// TODO: Add Fast_Before and Fast_After
			}

			[StructLayout(LayoutKind.Sequential, Pack = 1)]
			public struct VideoTrack
			{
				public int trackIndex;
				public int frameWidth;
				public int frameHeight;
				public float frameRate;
				public uint averageBitRate;
				//public string trackName;
				// TODO: add index, language, name, bitrate, codec etc
			}

			[StructLayout(LayoutKind.Sequential, Pack = 1)]
			public struct AudioTrack
			{
				public int trackIndex;
				public uint channelCount;
				public uint sampleRate;
				public uint bitsPerSample;
				public uint averageBitRate;
				//public string trackName;
				// TODO: add index, language, name, bitrate, codec etc
			}			

			[StructLayout(LayoutKind.Sequential, Pack = 1)]
			public struct SeekParams
			{
				public double timeSeconds;
				public SeekMode mode;
				// TODO: add min-max thresholds
			}

			[DllImport("AVProVideoWinRT")]
			public static extern void Seek(System.IntPtr playerInstance, ref SeekParams seekParams);

			public static void SetNextAuthData(System.IntPtr playerInstance, RenderHeads.Media.AVProVideo.AuthData srcAuthData)
			{		
				Native.AuthData ad = new Native.AuthData();
				ad.url = string.IsNullOrEmpty(srcAuthData.URL) ? null : srcAuthData.URL;
				ad.token = string.IsNullOrEmpty(srcAuthData.Token) ? null : srcAuthData.Token;
				if (srcAuthData.KeyBytes != null && srcAuthData.KeyBytes.Length > 0)
				{
					ad.keyBytes = Marshal.AllocHGlobal(srcAuthData.KeyBytes.Length);
					Marshal.Copy(srcAuthData.KeyBytes, 0, ad.keyBytes, srcAuthData.KeyBytes.Length);
					ad.keyBytesLength = srcAuthData.KeyBytes.Length;
				}
				else
				{
					ad.keyBytes = System.IntPtr.Zero;
					ad.keyBytesLength = 0;
				}

				SetNextAuthData(playerInstance, ref ad);

				if (ad.keyBytes != System.IntPtr.Zero)
				{
					Marshal.FreeHGlobal(ad.keyBytes);
				}
			}

			[StructLayout(LayoutKind.Sequential, Pack = 1)]
			public struct AuthData
			{
				[MarshalAs(UnmanagedType.LPWStr)]
				public string url;

				[MarshalAs(UnmanagedType.LPWStr)]
				public string token;

				public System.IntPtr keyBytes;
				public int keyBytesLength;
			};

			[DllImport("AVProVideoWinRT")]
			private static extern void SetNextAuthData(System.IntPtr playerInstance, ref AuthData authData);

			internal enum TimeRangeTypes
			{
				Seekable = 0,
				Buffered = 1,
			}

			[DllImport("AVProVideoWinRT")]
			public static extern int GetTimeRanges(System.IntPtr playerInstance, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] TimeRange[] ranges, int rangeCount, TimeRangeTypes timeRangeType);

			[DllImport("AVProVideoWinRT")]
			private static extern System.IntPtr GetRenderEventFunc_UpdateAllTextures();

			[DllImport("AVProVideoWinRT")]
			private static extern System.IntPtr GetRenderEventFunc_FreeTextures();

			private static System.IntPtr _nativeFunction_UpdateAllTextures;
			private static System.IntPtr _nativeFunction_FreeTextures;

			public static void IssueRenderThreadEvent_UpdateAllTextures()
			{
				if (System.IntPtr.Zero == _nativeFunction_UpdateAllTextures)
				{
					_nativeFunction_UpdateAllTextures = GetRenderEventFunc_UpdateAllTextures();
				}
				if (System.IntPtr.Zero != _nativeFunction_UpdateAllTextures)
				{
					UnityEngine.GL.IssuePluginEvent(_nativeFunction_UpdateAllTextures, 0);
				}
			}

			public static void IssueRenderThreadEvent_FreeAllTextures()
			{
				if (System.IntPtr.Zero == _nativeFunction_FreeTextures)
				{
					_nativeFunction_FreeTextures = GetRenderEventFunc_FreeTextures();
				}
				if (System.IntPtr.Zero != _nativeFunction_FreeTextures)
				{
					UnityEngine.GL.IssuePluginEvent(_nativeFunction_FreeTextures, 0);
				}
			}
		}
	}

	public sealed partial class WindowsRtMediaPlayer
	{
		private static bool _isInitialised = false;
		private static string _version = "Plug-in not yet initialised";

		private ulong _frameTimeStamp;
		private System.IntPtr _playerInstance;

		class EyeTexture
		{
			public Texture2D texture = null;
			public System.IntPtr nativePointer = System.IntPtr.Zero;

			public void Dispose()
			{
				if (texture)
				{
					if (Application.isPlaying) { Texture2D.Destroy(texture); }
						else { Texture2D.DestroyImmediate(texture); }
					texture = null;
				}
				nativePointer = System.IntPtr.Zero;
			}
		}

		private EyeTexture[] _eyeTextures = new EyeTexture[2];

		public static bool InitialisePlatform()
		{
			if (!_isInitialised)
			{
				try
				{
#if !UNITY_2019_3_OR_NEWER
					if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D12)
					{
						Debug.LogError("[AVProVideo] Direct3D 12 is not supported until Unity 2019.3");
						return false;
					}
#endif
					if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null ||
						SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D11 ||
						SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D12)
					{
						/*if (!Native.Init(QualitySettings.activeColorSpace == ColorSpace.Linear))
						{
							Debug.LogError("[AVProVideo] Failing to initialise platform");
						}
						else*/
						{
							_isInitialised = true;
							_version = Native.GetPluginVersion();
						}
					}
					else
					{
						Debug.LogError("[AVProVideo] Only Direct3D 11 and 12 are supported, graphicsDeviceType not supported: " + SystemInfo.graphicsDeviceType);
					}
				}
				catch (System.DllNotFoundException e)
				{
					Debug.LogError("[AVProVideo] Failed to load DLL. " + e.Message);
				}
			}

			return _isInitialised;
		}

		public static void DeinitPlatform()
		{
			//Native.Deinit();
			_isInitialised = false;
		}
	}
}

#endif