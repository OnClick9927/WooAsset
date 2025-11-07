// NOTE: We only allow this script to compile in editor so we can easily check for compilation issues
#if (UNITY_EDITOR || (UNITY_STANDALONE_WIN || UNITY_WSA_10_0))

#if UNITY_WSA_10 || ENABLE_IL2CPP
	#define AVPROVIDEO_MARSHAL_RETURN_BOOL
#endif
#if UNITY_5_4_OR_NEWER && !UNITY_2019_3_OR_NEWER
	#define AVPROVIDEO_FIXREGRESSION_TEXTUREQUALITY_UNITY542
#endif
#if UNITY_2019_3_OR_NEWER && !UNITY_2020_1_OR_NEWER
	#define AVPROVIDEO_FIX_UPDATEEXTERNALTEXTURE_LEAK
#endif

using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;
using System.Text;

#if NETFX_CORE
using Windows.Storage.Streams;
#endif

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Windows desktop and UWP implementation of BaseMediaPlayer
	/// </summary>
	public /*sealed*/ partial class WindowsMediaPlayer : BaseMediaPlayer
	{
		private Windows.AudioOutput _audioOutput = Windows.AudioOutput.System;
		private string			_audioDeviceOutputName = string.Empty;
		private List<string>	_preferredFilters = new List<string>();
		private Audio360ChannelMode _audio360ChannelMode = Audio360ChannelMode.TBE_8_2;
		private bool			_useCustomMovParser = false;
		private bool			_useStereoDetection = true;
		private bool			_useHapNotchLC = true;
		private bool			_useTextTrackSupport = true;
		private bool			_useFacebookAudio360Support = true;
		private bool			_useAudioDelay = false;
		private int 			_decoderParallelFrameCount = 3;
		private int				_decodePrerollFrameCount = 5;

		private bool			_isPlaying = false;
		private bool			_isPaused = false;
		private bool			_audioMuted = false;
		private float			_volume = 1.0f;
		private float			_balance = 0.0f;
		private bool			_isLooping = false;
		private bool			_canPlay = false;
		private bool			_hasMetaData = false;
		private int				_width = 0;
		private int				_height = 0;
		private float			_frameRate = 0f;
		private bool			_hasAudio = false;
		private bool			_hasVideo = false;
		private bool			_isTextureTopDown = true;
		private System.IntPtr 	_nativeTexture = System.IntPtr.Zero;
		private Texture2D		_texture;
		private RenderTexture 	_resolvedTexture;
		private System.IntPtr 	_instance = System.IntPtr.Zero;
		private Windows.VideoApi	_videoApi = Windows.VideoApi.MediaFoundation;
		private bool			_useHardwareDecoding = true;
		private bool			_useTextureMips = false;
		private bool			_use10BitTextures = false;
		private bool			_hintAlphaChannel = false;
		private bool			_useLowLatency = false;
		private bool			_supportsLinearColorSpace = true;
		private TextureFrame	_textureFrame;
#if AVPROVIDEO_FIX_UPDATEEXTERNALTEXTURE_LEAK
		private TextureFrame	_textureFramePrev;
#endif

		private static bool 	_isInitialised = false;
		private static string 	_version = "Plug-in not yet initialised";

		private static System.IntPtr _nativeFunction_UpdateAllTextures;
		private static System.IntPtr _nativeFunction_FreeTextures;
		private static System.IntPtr _nativeFunction_ExtractFrame;

#if AVPROVIDEO_FIXREGRESSION_TEXTUREQUALITY_UNITY542
		private int _textureQuality = QualitySettings.masterTextureLimit;
#endif

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
						SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore ||
#if !UNITY_2017_2_OR_NEWER
						SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D9 ||
#endif
						SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D11 ||
						SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D12)
					{
						if (!Native.Init(QualitySettings.activeColorSpace == ColorSpace.Linear, true))
						{
							Debug.LogError("[AVProVideo] Failing to initialise platform");
						}
						else
						{
							_isInitialised = true;
							_version = GetPluginVersion();
							_nativeFunction_UpdateAllTextures = Native.GetRenderEventFunc_UpdateAllTextures();
							_nativeFunction_FreeTextures = Native.GetRenderEventFunc_FreeTextures();
							_nativeFunction_ExtractFrame = Native.GetRenderEventFunc_WaitForNewFrame();
							if (_nativeFunction_UpdateAllTextures != IntPtr.Zero &&
								_nativeFunction_FreeTextures != IntPtr.Zero &&
								_nativeFunction_ExtractFrame != IntPtr.Zero)
							{
								_isInitialised = true;
							}
						}
					}
					else
					{
						Debug.LogError("[AVProVideo] graphicsDeviceType not supported: " + SystemInfo.graphicsDeviceType);
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
			Native.Deinit();
			_isInitialised = false;
		}

		public override int GetAudioChannelCount()
		{
			return Native.GetAudioChannelCount(_instance);
		}

		public override AudioChannelMaskFlags GetAudioChannelMask()
		{
			return (AudioChannelMaskFlags)Native.GetAudioChannelMask(_instance);
		}

		public WindowsMediaPlayer(MediaPlayer.OptionsWindows options) : base()
		{
			SetOptions(options.videoApi, options.audioOutput, options.useHardwareDecoding, options.useTextureMips, options.use10BitTextures, options.hintAlphaChannel, options.useLowLatency, options.forceAudioOutputDeviceName, options.preferredFilters, options.useCustomMovParser, options.parallelFrameCount, options.prerollFrameCount, options.useHapNotchLC, options.useStereoDetection, options.useTextTrackSupport, options.useFacebookAudio360Support, options.bufferedFrameSelection, options.pauseOnPrerollComplete, options.useAudioDelay);
		}

		public WindowsMediaPlayer(MediaPlayer.OptionsWindowsUWP options) : base()
		{
			Windows.VideoApi api = (options.videoApi == WindowsUWP.VideoApi.MediaFoundation)?Windows.VideoApi.MediaFoundation:Windows.VideoApi.WinRT;
			Windows.AudioOutput audioOutput = (Windows.AudioOutput)(int)options.audioOutput;
			SetOptions(api, audioOutput, options.useHardwareDecoding, options.useTextureMips, options.use10BitTextures, false, options.useLowLatency, string.Empty, null, false, 1, 0, false, true, false, true, BufferedFrameSelectionMode.None, false, false);
		}

		public void SetOptions(Windows.VideoApi videoApi, Windows.AudioOutput audioOutput, bool useHardwareDecoding, bool useTextureMips, bool use10BitTextures, bool hintAlphaChannel, 
								bool useLowLatency, string audioDeviceOutputName, List<string> preferredFilters, bool useCustomMovParser, int parallelFrameCount, int prerollFrameCount, 
								bool useHapNotchLC, bool useStereoDetection, bool useTextTrackSupport, bool useFacebookAudio360Support,
								BufferedFrameSelectionMode bufferedFrameSelection, bool pauseOnPrerollComplete, bool useAudioDelay)
		{
			_videoApi = videoApi;
			_audioOutput = audioOutput;
			_useHardwareDecoding = useHardwareDecoding;
			_useTextureMips = useTextureMips;
			_use10BitTextures = use10BitTextures;
			_hintAlphaChannel = hintAlphaChannel;
			_useLowLatency = useLowLatency;
			_useStereoDetection = useStereoDetection;
			_useTextTrackSupport = useTextTrackSupport;
			_useFacebookAudio360Support = useFacebookAudio360Support;
			_frameSelectionMode = bufferedFrameSelection;
			_pauseOnPrerollComplete = pauseOnPrerollComplete;
			_useHapNotchLC = useHapNotchLC;
			_useCustomMovParser = useCustomMovParser;
			_decoderParallelFrameCount = parallelFrameCount;
			_decodePrerollFrameCount = prerollFrameCount;
			_useAudioDelay = useAudioDelay;
			_audioDeviceOutputName = audioDeviceOutputName;
			if (!string.IsNullOrEmpty(_audioDeviceOutputName))
			{
				_audioDeviceOutputName = _audioDeviceOutputName.Trim();
			}
			_preferredFilters = preferredFilters;
			if (_preferredFilters != null)
			{
				for (int i = 0; i < _preferredFilters.Count; ++i)
				{
					if (!string.IsNullOrEmpty(_preferredFilters[i]))
					{
						_preferredFilters[i] = _preferredFilters[i].Trim();
					}
				}
			}
		}

		public override string GetVersion()
		{
			return _version;
		}

		public override string GetExpectedVersion()
		{
			return Helper.ExpectedPluginVersion.Windows;
		}

		private bool UseNativeMips()
		{
			// RJT TODO: Heuristic(s) to decide whether mipping should be performed at native level or here
			// - Query native first to see whether it can handle?
			//   - E.g. D3D11/12 hardware path should be able to handle but software not
			//   - May become moot if we're going to create a resolved texture at this level anyhow (makes sense to generate mips as final step?)
			return _useTextureMips;//true;// false;// _useTextureMips;
		}

		public override bool OpenMedia(string path, long offset, string httpHeader, MediaHints mediaHints, int forceFileFormat = 0, bool startWithHighestBitrate = false)
		{
			CloseMedia();

			uint filterCount = 0U;
			IntPtr[] filters = null;

			if (_preferredFilters != null && _preferredFilters.Count > 0)
			{
				filterCount = (uint)_preferredFilters.Count;
				filters = new IntPtr[_preferredFilters.Count];

				for (int i = 0; i < filters.Length; ++i)
				{
					filters[i] = Marshal.StringToHGlobalUni(_preferredFilters[i]);
				}
			}

			_instance = Native.BeginOpenSource(_instance, _videoApi, _audioOutput, _useHardwareDecoding, UseNativeMips(), mediaHints.transparency == TransparencyMode.Transparent, _useLowLatency, _use10BitTextures, _audioDeviceOutputName, _audioOutput == Windows.AudioOutput.Unity?Helper.GetUnityAudioSampleRate():0, filters, filterCount, (int)_audio360ChannelMode);
			if (_instance != System.IntPtr.Zero)
			{
				Native.SetCustomMovParserEnabled(_instance, _useCustomMovParser);
				Native.SetHapNotchLCEnabled(_instance, _useHapNotchLC);
				Native.SetFrameBufferingEnabled(_instance, (_frameSelectionMode != BufferedFrameSelectionMode.None), _pauseOnPrerollComplete);
				Native.SetStereoDetectEnabled(_instance, _useStereoDetection);
				Native.SetTextTrackSupportEnabled(_instance, _useTextTrackSupport);
				Native.SetAudioDelayEnabled(_instance, _useAudioDelay, true, 0.0);
				Native.SetFacebookAudio360SupportEnabled(_instance, _useFacebookAudio360Support);
				Native.SetDecoderHints(_instance, _decoderParallelFrameCount, _decodePrerollFrameCount);
				_instance = Native.EndOpenSource(_instance, path);
			}

			if (filters != null)
			{
				for (int i = 0; i < filters.Length; ++i)
				{
					Marshal.FreeHGlobal(filters[i]);
				}
			}

			if (_instance == System.IntPtr.Zero)
			{
				DisplayLoadFailureSuggestion(path);
				return false;
			}

			_mediaHints = mediaHints;

			return true;
		}

		public override bool OpenMediaFromBuffer(byte[] buffer)
		{
			CloseMedia();

			IntPtr[] filters;
			if (_preferredFilters.Count == 0)
			{
				filters = null;
			}
			else
			{
				filters = new IntPtr[_preferredFilters.Count];

				for (int i = 0; i < filters.Length; ++i)
				{
					filters[i] = Marshal.StringToHGlobalUni(_preferredFilters[i]);
				}
			}

			_instance = Native.OpenSourceFromBuffer(_instance, buffer, (ulong)buffer.Length, _videoApi, _audioOutput, _useHardwareDecoding, UseNativeMips(), _mediaHints.transparency == TransparencyMode.Transparent, _useLowLatency, _use10BitTextures, _audioDeviceOutputName, _audioOutput == Windows.AudioOutput.Unity?Helper.GetUnityAudioSampleRate():0, filters, (uint)_preferredFilters.Count);

			if (filters != null)
			{
				for (int i = 0; i < filters.Length; ++i)
				{
					Marshal.FreeHGlobal(filters[i]);
				}
			}

			if (_instance == System.IntPtr.Zero)
			{
				return false;
			}

			return true;
		}

		public override bool StartOpenMediaFromBuffer(ulong length)
		{
			CloseMedia();

			_instance = Native.StartOpenSourceFromBuffer(_instance, _videoApi, length);

			return _instance != IntPtr.Zero;
		}

		public override bool AddChunkToMediaBuffer(byte[] chunk, ulong offset, ulong length)
		{
			return Native.AddChunkToSourceBuffer(_instance, chunk, offset, length);
		}

		public override bool EndOpenMediaFromBuffer()
		{
			IntPtr[] filters;
			if (_preferredFilters.Count == 0)
			{
				filters = null;
			}
			else
			{
				filters = new IntPtr[_preferredFilters.Count];

				for (int i = 0; i < filters.Length; ++i)
				{
					filters[i] = Marshal.StringToHGlobalUni(_preferredFilters[i]);
				}
			}

			_instance = Native.EndOpenSourceFromBuffer(_instance, _audioOutput, _useHardwareDecoding, UseNativeMips(), _hintAlphaChannel, _useLowLatency, _use10BitTextures, _audioDeviceOutputName, _audioOutput == Windows.AudioOutput.Unity?Helper.GetUnityAudioSampleRate():0, filters, (uint)_preferredFilters.Count);

			if (filters != null)
			{
				for (int i = 0; i < filters.Length; ++i)
				{
					Marshal.FreeHGlobal(filters[i]);
				}
			}

			if (_instance == System.IntPtr.Zero)
			{
				return false;
			}

			return true;
		}

#if NETFX_CORE
		public override bool OpenMedia(IRandomAccessStream ras, string path, long offset, string httpHeader)
		{
			CloseMedia();

			_instance = Native.OpenSourceFromStream(_instance, ras, path, _videoApi, _audioOutput, _useHardwareDecoding, UseNativeMips(), _hintAlphaChannel, _useLowLatency, _use10BitTextures, _audioDeviceOutputName, _audioOutput == Windows.AudioOutput.Unity?Helper.GetUnityAudioSampleRate():0);

			if (_instance == System.IntPtr.Zero)
			{
				DisplayLoadFailureSuggestion(path);
				return false;
			}

			return true;
		}
#endif

		private void DisplayLoadFailureSuggestion(string path)
		{
			bool usingDirectShow = (_videoApi == Windows.VideoApi.DirectShow) || SystemInfo.operatingSystem.Contains("Windows 7") || SystemInfo.operatingSystem.Contains("Windows Vista") || SystemInfo.operatingSystem.Contains("Windows XP");
			if (usingDirectShow && path.Contains(".mp4"))
			{
				Debug.LogWarning("[AVProVideo] The native Windows DirectShow H.264 decoder doesn't support videos with resolution above 1920x1080. You may need to reduce your video resolution, switch to another codec (such as DivX or Hap), or install 3rd party DirectShow codec (eg LAV Filters).  This shouldn't be a problem for Windows 8 and above as it has a native limitation of 3840x2160.");
			}
		}

		public override void CloseMedia()
		{
			_width = 0;
			_height = 0;
			_frameRate = 0f;
			_hasAudio = _hasVideo = false;
			_hasMetaData = false;
			_canPlay = false;
			_isPaused = true;
			_isPlaying = false;
			_isLooping = false;
			_audioMuted = false;
			_volume = 1f;
			_balance = 0f;
			_supportsLinearColorSpace = true;
			_displayClockTime = 0.0;
			_timeAccumulation = 0.0;
			FlushFrameBuffering(true);
			ReleaseTexture();
			
			if (_instance != System.IntPtr.Zero)
			{
				Native.CloseSource(_instance);
				_instance = System.IntPtr.Zero;
			}

			// Issue thread event to free the texture on the GPU
			IssueRenderThreadEvent(Native.RenderThreadEvent.FreeTextures);

			base.CloseMedia();
		}

		public override void SetLooping(bool looping)
		{
			_isLooping = looping;
			Native.SetLooping(_instance, looping);
		}

		public override bool IsLooping()
		{
			return _isLooping;
		}

		public override bool HasMetaData()
		{
			return _hasMetaData;
		}

		public override bool HasAudio()
		{
			return _hasAudio;
		}

		public override bool HasVideo()
		{
			return _hasVideo;
		}

		public override bool CanPlay()
		{
			return _canPlay;
		}

		public override void Play()
		{
			_isPlaying = true;
			_isPaused = false;
			Native.Play(_instance);
		}

		public override void Pause()
		{
			_isPlaying = false;
			_isPaused = true;
			Native.Pause(_instance);
		}

		public override void Stop()
		{
			_isPlaying = false;
			_isPaused = true;
			Native.Pause(_instance);
		}

		public override bool IsSeeking()
		{
			return Native.IsSeeking(_instance);
		}
		public override bool IsPlaying()
		{
			if (_isPlaying && _frameSelectionMode != BufferedFrameSelectionMode.None)
			{
				// In case we're still playing the buffered frames at the end of the video
				// We want to return true, even though internally it has stopping playing
				if (Native.IsFinished(_instance) && !IsFinished())
				{
					return true;
				}
				// In this case internal state can change so we need to check that too
				if (_pauseOnPrerollComplete)
				{
					return Native.IsPlaying(_instance);
				}
			}
			return _isPlaying;
		}
		public override bool IsPaused()
		{
			if (_pauseOnPrerollComplete)
			{
				// In this case internal state can change so we need to check that too
				return _isPaused || !Native.IsPlaying(_instance);
			}
			return _isPaused;
		}
		public override bool IsFinished()
		{
			bool result = false;

			if (!IsLooping())
			{
				result = Native.IsFinished(_instance);

				if (!result)
				{
					// This fixes a bug in Media Foundation where in some rare cases Native.IsFinished() returns false
					result = (GetCurrentTime() > GetDuration());
				}

				// During buffered playback we need to wait until all frames have been displayed
				if (result && _frameSelectionMode != BufferedFrameSelectionMode.None)
				{
					BufferedFramesState state = GetBufferedFramesState();
					if (state.bufferedFrameCount != 0)
					{
						result = false;
					}
				}
			}

			return result;
		}

		public override bool IsBuffering()
		{
			return Native.IsBuffering(_instance);
		}

		public override double GetDuration()
		{
			return Native.GetDuration(_instance);
		}

		public override int GetVideoWidth()
		{
			return _width;
		}
			
		public override int GetVideoHeight()
		{
			return _height;
		}

		public override float GetVideoFrameRate()
		{
			return _frameRate;
		}

		public override Texture GetTexture(int index)
		{
			Texture result = null;
			if (GetTextureFrameCount() > 0)
			{
				if (_resolvedTexture) { result = _resolvedTexture; }
				else { result = _texture; }
			}
			return result;
		}

		public override int GetTextureFrameCount()
		{
#if AVPROVIDEO_SUPPORT_BUFFERED_DISPLAY
			if (_frameSelectionMode != BufferedFrameSelectionMode.None)
			{
				return (int)_textureFrame.frameCounter;
			}
#endif
			return Native.GetTextureFrameCount(_instance);
		}

		public override long GetTextureTimeStamp()
		{
#if AVPROVIDEO_SUPPORT_BUFFERED_DISPLAY
			if (_frameSelectionMode != BufferedFrameSelectionMode.None)
			{
				return _textureFrame.timeStamp;
			}
#endif
			return Native.GetTextureTimeStamp(_instance);
		}

		public override bool RequiresVerticalFlip()
		{
			return _isTextureTopDown;
		}

		internal override StereoPacking InternalGetTextureStereoPacking()
		{
			return Native.GetStereoPacking(_instance);
		}

		public override void Seek(double time)
		{
			Native.SetCurrentTime(_instance, time, false);
			FlushFrameBuffering(false);
		}

		public override void SeekFast(double time)
		{
			Native.SetCurrentTime(_instance, time, true);
			FlushFrameBuffering(false);
		}

		public override double GetCurrentTime()
		{
			return Native.GetCurrentTime(_instance);
		}

		public override void SetPlaybackRate(float rate)
		{
			Native.SetPlaybackRate(_instance, rate);
		}

		public override float GetPlaybackRate()
		{
			return Native.GetPlaybackRate(_instance);
		}

		public override void MuteAudio(bool bMuted)
		{
			_audioMuted = bMuted;
			Native.SetMuted(_instance, _audioMuted);
		}

		public override bool IsMuted()
		{
			return _audioMuted;
		}

		public override void SetVolume(float volume)
		{
			_volume = volume;
			Native.SetVolume(_instance, volume);
		}

		public override float GetVolume()
		{
			return _volume;
		}

		public override void SetBalance(float balance)
		{
			_balance = balance;
			Native.SetBalance(_instance, balance);
		}

		public override float GetBalance()
		{
			return _balance;
		}

		public override bool IsPlaybackStalled()
		{
			bool result = Native.IsPlaybackStalled(_instance);
			if (!result)
			{
				result = base.IsPlaybackStalled();
			}
			return result;
		}

		public override bool WaitForNextFrame(Camera dummyCamera, int previousFrameCount)
		{
			// Mark as extracting
			Native.StartExtractFrame(_instance);

			// Queue up render thread event to wait for the new frame
			IssueRenderThreadEvent(Native.RenderThreadEvent.WaitForNewFrame);

			// Force render thread to run
			dummyCamera.Render();

			// Wait for the frame to change
			Native.WaitForExtract(_instance);

			// Return whether the frame changed
			return (previousFrameCount != Native.GetTextureFrameCount(_instance));
		}

		public override void SetAudioChannelMode(Audio360ChannelMode channelMode)
		{
			_audio360ChannelMode = channelMode;
			Native.SetAudioChannelMode(_instance, (int)channelMode);
		}

		public override void SetAudioHeadRotation(Quaternion q)
		{
			Native.SetHeadOrientation(_instance, q.x, q.y, q.z, q.w);
		}

		public override void ResetAudioHeadRotation()
		{
			Native.SetHeadOrientation(_instance, Quaternion.identity.x, Quaternion.identity.y, Quaternion.identity.z, Quaternion.identity.w);
		}

		public override void SetAudioFocusEnabled(bool enabled)
		{
			Native.SetAudioFocusEnabled(_instance, enabled);
		}

		public override void SetAudioFocusProperties(float offFocusLevel, float widthDegrees)
		{
			Native.SetAudioFocusProps(_instance, offFocusLevel, widthDegrees);
		}

		public override void SetAudioFocusRotation(Quaternion q)
		{
			Native.SetAudioFocusRotation(_instance, q.x, q.y, q.z, q.w);
		}

		public override void ResetAudioFocus()
		{
			Native.SetAudioFocusEnabled(_instance, false);
			Native.SetAudioFocusProps(_instance, 0f, 90f);
			Native.SetAudioFocusRotation(_instance, 0f, 0f, 0f, 1f);
		}

		//public override void SetAudioDeviceName(string name)
		//{
		//}

		//double timeOfDesiredFrameToDisplay = 0.0;
		//int frameFrames = 0;

		public override void Update()
		{
			Native.Update(_instance);
			/*Native.GetPlayerState(_instance, ref _playerState);
			if (_playerState.status.HasFlag(Native.Status.UpdatedAssetInfo))
			{
				Native.GetAssetInfo(_instance, ref _assetInfo);
			}*/

			UpdateTracks();
			UpdateTextCue();

			_lastError = (ErrorCode)Native.GetLastErrorCode(_instance);

			UpdateTimeRanges();
			UpdateSubtitles();

			if (!_canPlay)
			{
				if (!_hasMetaData)
				{
					if (Native.HasMetaData(_instance))
					{
						if (Native.HasVideo(_instance))
						{
							_width = Native.GetWidth(_instance);
							_height = Native.GetHeight(_instance);
							_frameRate = Native.GetFrameRate(_instance);

							// Sometimes the dimensions aren't available yet, in which case fail and poll them again next loop
							if (_width > 0 && _height > 0)
							{
								_hasVideo = true;

								// Note: If the Unity editor Build platform isn't set to Windows then maxTextureSize will not be correct
								if (Mathf.Max(_width, _height) > SystemInfo.maxTextureSize

								// If we're running in the editor it may be emulating another platform
								// in which case maxTextureSize won't be correct, so ignore it.
#if UNITY_EDITOR
								&& !SystemInfo.graphicsDeviceName.ToLower().Contains("emulated")
#endif
								)
								{
									Debug.LogError(string.Format("[AVProVideo] Video dimensions ({0}x{1}) larger than maxTextureSize ({2} for current build target)", _width, _height, SystemInfo.maxTextureSize));
									_width = _height = 0;
									_hasVideo = false;
								}
							}

							if (_hasVideo)
							{
								if (Native.HasAudio(_instance))
								{
									_hasAudio = true;
								}
							}
						}
						else
						{
							if (Native.HasAudio(_instance))
							{
								_hasAudio = true;
							}
						}

						if (_hasVideo || _hasAudio)
						{
							_hasMetaData = true;
						}

						_playerDescription = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(Native.GetPlayerDescription(_instance));
						_supportsLinearColorSpace = Native.IsTextureSampleLinear(_instance);
						Helper.LogInfo("Using playback path: " + _playerDescription + " (" + _width + "x" + _height + "@" + GetVideoFrameRate().ToString("F2") + ")");
						if (_hasVideo)
						{
							OnTextureSizeChanged();
						}
					}
				}
				if (_hasMetaData)
				{
					_canPlay = Native.CanPlay(_instance);
				}
			}

#if UNITY_WSA
			// NOTE: I think this issue has been resolved now as of version 1.5.24.  
			// The issue was caused by functions returning booleans incorrectly (4 bytes vs 1)
			// and as been resolved by specifying the return type during marshalling..
			// Still we'll keep this code here until after more testing.

			// WSA has an issue where it can load the audio track first and the video track later
			// Here we try to handle this case and get the video track information when it arrives
			if (_hasAudio && !_hasVideo)
			{
				_width = Native.GetWidth(_instance);
				_height = Native.GetHeight(_instance);
				_frameRate = Native.GetFrameRate(_instance);

				if (_width > 0 && _height > 0)
				{
					_hasVideo = true;
					OnTextureSizeChanged();
				}
			}
#endif

			// Handle texture creation, resizing, selection
			if (_hasVideo)
			{
				System.IntPtr newTexturePtr = System.IntPtr.Zero;
#if AVPROVIDEO_SUPPORT_BUFFERED_DISPLAY
				if (_frameSelectionMode != BufferedFrameSelectionMode.None)
				{
					UpdateBufferedDisplay();
					newTexturePtr = _textureFrame.texturePointer;
				}
				else
#endif
				{
					newTexturePtr = Native.GetTexturePointer(_instance);
				}

				UpdateTexture(newTexturePtr);
			}

			_playbackQualityStats.Update();
		}

		private void ReleaseTexture()
		{
			_nativeTexture = System.IntPtr.Zero;
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (_resolvedTexture) RenderTexture.DestroyImmediate(_resolvedTexture);
				if (_texture) Texture2D.DestroyImmediate(_texture);
			}
			else
#endif
			{
				if (_resolvedTexture) RenderTexture.Destroy(_resolvedTexture);
				if (_texture) Texture2D.Destroy(_texture);
			}
			_resolvedTexture = null;
			_texture = null;
			_textureFrame = new TextureFrame();
#if AVPROVIDEO_FIX_UPDATEEXTERNALTEXTURE_LEAK
			_textureFramePrev = new TextureFrame();
#endif
		}

		private void UpdateTexture(System.IntPtr newPtr)
		{ 
			// Check for texture recreation (due to device loss or change in texture size)
			if (_texture != null && _nativeTexture != System.IntPtr.Zero && _nativeTexture != newPtr)
			{
				_width = Native.GetWidth(_instance);
				_height = Native.GetHeight(_instance);

				if (newPtr == System.IntPtr.Zero)
				{
					ReleaseTexture();
				}
				else if (_width != _texture.width || _height != _texture.height)
				{
					Helper.LogInfo("Texture size changed: " + _width + " X " + _height);
					OnTextureSizeChanged();
					ReleaseTexture();
				}
				else if (_nativeTexture != newPtr)
				{
					if (newPtr != System.IntPtr.Zero)
					{
#if AVPROVIDEO_FIX_UPDATEEXTERNALTEXTURE_LEAK
						Native.ReleaseTextureFrame(_instance, ref _textureFramePrev);
#endif
						_texture.UpdateExternalTexture(newPtr);
#if AVPROVIDEO_FIX_UPDATEEXTERNALTEXTURE_LEAK
						_textureFramePrev = _textureFrame;
#endif
					}
					_nativeTexture = newPtr;
				}
			}

#if AVPROVIDEO_FIXREGRESSION_TEXTUREQUALITY_UNITY542
			// In Unity 5.4.2 and above the video texture turns black when changing the TextureQuality in the Quality Settings
			// The code below gets around this issue.  A bug report has been sent to Unity.  So far we have tested and replicated the
			// "bug" in Windows only, but a user has reported it in Android too.  
			// Texture.GetNativeTexturePtr() must sync with the rendering thread, so this is a large performance hit!
			if(_textureQuality != QualitySettings.masterTextureLimit)
			{
				ApplyTextureQualityChangeFix();
			}
#endif

			// Check if a new texture has to be created
			if (_texture == null && _width > 0 && _height > 0 && newPtr != System.IntPtr.Zero)
			{
				_isTextureTopDown = Native.IsTextureTopDown(_instance);
				bool isLinear = (!_supportsLinearColorSpace && QualitySettings.activeColorSpace == ColorSpace.Linear);

				_texture = Texture2D.CreateExternalTexture(_width, _height, TextureFormat.RGBA32, UseNativeMips(), isLinear, newPtr);
				if (_texture != null)
				{
#if AVPROVIDEO_FIX_UPDATEEXTERNALTEXTURE_LEAK
					_textureFramePrev = _textureFrame;
#endif
					_texture.name = "AVProVideo";
					_nativeTexture = newPtr;
					_playbackQualityStats.Start(this);
					ApplyTextureProperties(_texture);

					// Use an intermediate resolved texture?
					// RJT NOTE: Currently based on if/how mips are generated but may evolve (see 'Blit()' notes below)
					// RJT TODO: Appears support for '_useTextureMips' is not dynamic during a single run? Would be nice to address..
					if (_useTextureMips && !UseNativeMips())
					{
						// RJT TODO: Support 'isLinear'?
						_resolvedTexture = new RenderTexture(_width, _height, 0);// RenderTextureFormat.ARGB32);
						_resolvedTexture.useMipMap = _resolvedTexture.autoGenerateMips = false;
					}
				}
				else
				{
					Debug.LogError("[AVProVideo] Failed to create texture");
				}
			}

			// RJT TODO: If we have a resolved texture (Render Target) then blit into it, which will also generate mips if necessary
			// - 1. For certain paths (i.e. D3D11/12 hardware) our 'Native.GetTexturePointer()' texture is already an RT
			//      so is it possible to directly wrap that instead of creating a duplicate RT at this level?
			// - 2. There's probably a Unity version regression point at which this fails?
			//   - I.e. do we still need lower level support? (Ignoring standalone version of AVP etc..)
			// - 3. Possible to move this to a higher level for full cross-platform support?
			//      - This could also become the start of the resolved render discussed, where as well as mip generation we also resolve to a final output texture?
			//      - I.e. move into caller of above 'GetTexture()' function?
			//        - 'ApplyMapping()' functions could apply to an internal intermediate texture (/material?) that _then_ gets applied as if it were the
			//          original texture making sure that rendering performance isn't unecessarily comprimised (i.e. may have to defer/change location)
			// - Also, better location than 'Update()'? (I.e. 'Render()'?)
			if (_texture && _resolvedTexture)
			{
				_resolvedTexture.useMipMap = _resolvedTexture.autoGenerateMips = _useTextureMips;
				Graphics.Blit(_texture, _resolvedTexture);
			}
		}

		public override void EndUpdate()
		{
			Native.EndUpdate(_instance);
		}

		public override long GetLastExtendedErrorCode()
		{
			return Native.GetLastExtendedErrorCode(_instance);
		}

		private void OnTextureSizeChanged()
		{
		}

		public override void Render()
		{
			UpdateDisplayFrameRate();

			IssueRenderThreadEvent(Native.RenderThreadEvent.UpdateAllTextures);
		}

		public override void Dispose()
		{
			CloseMedia();
		}

		public override int GrabAudio(float[] buffer, int sampleCount, int channelCount)
		{
			return Native.GrabAudio(_instance, buffer, sampleCount, channelCount);
		}

		public override int GetAudioBufferedSampleCount()
		{
			return Native.GetAudioBufferedSampleCount(_instance);
		}

		public override bool PlayerSupportsLinearColorSpace()
		{
			return _supportsLinearColorSpace;
		}

		
		public override bool GetDecoderPerformance(ref int activeDecodeThreadCount, ref int decodedFrameCount, ref int droppedFrameCount)
		{
			return Native.GetDecoderPerformance(_instance, ref activeDecodeThreadCount, ref decodedFrameCount, ref droppedFrameCount);
		}

		private static int _lastUpdateAllTexturesFrame = -1;
		//private static int _lastFreeUnusedTexturesFrame = -1;

		private static void IssueRenderThreadEvent(Native.RenderThreadEvent renderEvent)
		{
			if (renderEvent == Native.RenderThreadEvent.UpdateAllTextures)
			{
				// We only want to update all textures once per Unity frame
				if (_lastUpdateAllTexturesFrame == Time.frameCount)
					return;

				_lastUpdateAllTexturesFrame = Time.frameCount;
			}
			/*else if (renderEvent == Native.RenderThreadEvent.FreeTextures)
			{
				// We only want to free unused textures once per Unity frame
				if (_lastFreeUnusedTexturesFrame == Time.frameCount)
					return;

				_lastFreeUnusedTexturesFrame = Time.frameCount;
			}*/

			if (renderEvent == Native.RenderThreadEvent.UpdateAllTextures)
			{
				GL.IssuePluginEvent(_nativeFunction_UpdateAllTextures, 0);
			}
			else if (renderEvent == Native.RenderThreadEvent.FreeTextures)
			{
				GL.IssuePluginEvent(_nativeFunction_FreeTextures, 0);
			}
			else if (renderEvent == Native.RenderThreadEvent.WaitForNewFrame)
			{
				GL.IssuePluginEvent(_nativeFunction_ExtractFrame, 0);
			}
		}

		private static string GetPluginVersion()
		{
			return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(Native.GetPluginVersion());
		}

#if AVPROVIDEO_FIXREGRESSION_TEXTUREQUALITY_UNITY542
		private void ApplyTextureQualityChangeFix()
		{
			if (_texture != null && _nativeTexture != System.IntPtr.Zero && _texture.GetNativeTexturePtr() == System.IntPtr.Zero)
			{
				Debug.LogWarning("[AVProVideo] Applying Texture Quality/Lost Fix");
				_texture.UpdateExternalTexture(_nativeTexture);
			}
			_textureQuality = QualitySettings.masterTextureLimit;
		}

		public override void OnEnable()
		{
			base.OnEnable();
			ApplyTextureQualityChangeFix();
		}
#endif

		internal override bool InternalSetActiveTrack(TrackType trackType, int trackUid)
		{
			bool result = false;
			switch (trackType)
			{
				case TrackType.Video:
				case TrackType.Audio:
				case TrackType.Text:
				{
					result = Native.SetActiveTrack(_instance, trackType, trackUid);
					break;
				}
			}
			return result;
		}

		// Has it changed since the last frame 'tick'
		internal override bool InternalIsChangedTextCue()
		{
			return Native.IsChangedTextCue(_instance);
		}

		internal override string InternalGetCurrentTextCue()
		{
			string result = null;
			System.IntPtr ptr = Native.GetCurrentTextCue(_instance);
			if (ptr != System.IntPtr.Zero)
			{
				result = System.Runtime.InteropServices.Marshal.PtrToStringUni(ptr);
			}
			return result;
		}

		// Has it changed since the last frame 'tick'
		internal override bool InternalIsChangedTracks(TrackType trackType)
		{
			bool result = false;
			switch (trackType)
			{
				case TrackType.Video:
				case TrackType.Audio:
				case TrackType.Text:
				{
					result = Native.IsChangedTracks(_instance, trackType);
					break;
				}
			}
			return result;
		}

		internal override int InternalGetTrackCount(TrackType trackType)
		{
			int result = 0;
			switch (trackType)
			{
				case TrackType.Video:
				case TrackType.Audio:
				case TrackType.Text:
				{
					result = Native.GetTrackCount(_instance, trackType);
					break;
				}
			}
			return result;
		}

		internal override TrackBase InternalGetTrackInfo(TrackType trackType, int trackIndex, ref bool isActiveTrack)
		{
			TrackBase result = null;
			switch (trackType)
			{
				case TrackType.Video:
				case TrackType.Audio:
				case TrackType.Text:
				{
					StringBuilder name = new StringBuilder(128);
					StringBuilder language = new StringBuilder(16);
					int uid = -1;
					if (Native.GetTrackInfo(_instance, trackType, trackIndex, ref uid, ref isActiveTrack, name, name.Capacity, language, language.Capacity))
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
					break;
				}
			}
			return result;
		}

		/*private Native.PlayerState _playerState = new Native.PlayerState();
		private Native.AssetInfo _assetInfo = new Native.AssetInfo();*/

		private partial struct Native
		{
			/*[Flags]
			internal enum Status : long
			{
				Unknown                   = 0,
				UpdatedAssetInfo          = 1 <<  8,
				UpdatedBufferedTimeRanges = 1 << 10,
				UpdatedSeekableTimeRanges = 1 << 11,
				UpdatedTextCue            = 1 << 12,
				UpdatedVideoTracks        = 1 << 16,
				UpdatedAudioTracks        = 1 << 17,
				UpdatedTextTracks         = 1 << 18,
				Failed                    = 1 << 63
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct PlayerState
			{
				internal Status status;
				internal double currentTime;
				internal double currentDate;
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct AssetInfo
			{
				internal double duration;
				internal int width;						// current video track
				internal int height;					// current video track
				internal float frameRate;				// current video track
				internal int videoTrackCount;
				internal int audioTrackCount;
				internal int textTrackCount;
				internal int metadataTrackCount;
				internal int bufferedTimeRangesCount;
				internal int seekableTimeRangesCount;
			}

			[DllImport("AVProVideo")]
			public static extern void GetPlayerState(System.IntPtr instance, ref PlayerState playerState);

			[DllImport("AVProVideo")]
			public static extern void GetAssetInfo(System.IntPtr instance, ref AssetInfo assetInfo);*/

			[DllImport("AVProVideo")]
			[return: MarshalAs(UnmanagedType.I1)]
			public static extern bool IsChangedTracks(System.IntPtr instance, TrackType trackType);

			[DllImport("AVProVideo")]
			public static extern int GetTrackCount(System.IntPtr instance, TrackType trackType);

			[DllImport("AVProVideo")]
			[return: MarshalAs(UnmanagedType.I1)]
			public static extern bool GetTrackInfo(System.IntPtr instance, TrackType trackType, int index, ref int uid, 
										ref bool isActive,
										[MarshalAs(UnmanagedType.LPWStr)] StringBuilder name, int maxNameLength,
										[MarshalAs(UnmanagedType.LPWStr)] StringBuilder language, int maxLanguageLength);

			[DllImport("AVProVideo")]
			[return: MarshalAs(UnmanagedType.I1)]
			public static extern bool SetActiveTrack(System.IntPtr instance, TrackType trackType, int trackUid);

			[DllImport("AVProVideo")]
			[return: MarshalAs(UnmanagedType.I1)]
			public static extern bool IsChangedTextCue(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern System.IntPtr GetCurrentTextCue(System.IntPtr instance);
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
			int newCount = Native.GetTimeRanges(_instance, range, range.Length, timeRangeType);
			if (newCount != range.Length)
			{
				range = new TimeRange[newCount];
				Native.GetTimeRanges(_instance, range, range.Length, timeRangeType);
			}
		}

		private partial struct Native
		{
			internal enum TimeRangeTypes
			{
				Seekable = 0,
				Buffered = 1,
			}

			[DllImport("AVProVideo")]
			public static extern int GetTimeRanges(System.IntPtr playerInstance, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] TimeRange[] ranges, int rangeCount, TimeRangeTypes timeRangeType);
		}

		private partial struct Native
		{
			public enum RenderThreadEvent
			{
				UpdateAllTextures,
				FreeTextures,
				WaitForNewFrame,
			}

			// Global

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool Init(bool linearColorSpace, bool isD3D11NoSingleThreaded);

			[DllImport("AVProVideo")]
			public static extern void Deinit();

			[DllImport("AVProVideo")]
			public static extern System.IntPtr GetPluginVersion();

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool IsTrialVersion();

			// Open and Close

			[DllImport("AVProVideo")]
			public static extern System.IntPtr BeginOpenSource(System.IntPtr instance, Windows.VideoApi videoApi, Windows.AudioOutput audioOutput, bool useHardwareDecoding, 
				bool generateTextureMips, bool hintAlphaChannel, bool useLowLatency, bool use10BitTextures, [MarshalAs(UnmanagedType.LPWStr)]string forceAudioOutputDeviceName,
				 int unitySampleRate, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)]IntPtr[] preferredFilter, uint numFilters,
				 int audio360ChannelMode);

			[DllImport("AVProVideo")]
			public static extern System.IntPtr EndOpenSource(System.IntPtr instance, [MarshalAs(UnmanagedType.LPWStr)]string path);

			[DllImport("AVProVideo")]
			public static extern System.IntPtr OpenSourceFromBuffer(System.IntPtr instance, byte[] buffer, ulong bufferLength, Windows.VideoApi videoApi, Windows.AudioOutput audioOutput, bool useHardwareDecoding,
				bool generateTextureMips, bool hintAlphaChannel, bool useLowLatency, bool use10BitTextures, [MarshalAs(UnmanagedType.LPWStr)]string forceAudioOutputDeviceName, 
				int unitySampleRate, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)]IntPtr[] preferredFilter, uint numFilters);

			[DllImport("AVProVideo")]
			public static extern System.IntPtr StartOpenSourceFromBuffer(System.IntPtr instance, Windows.VideoApi videoApi, ulong bufferLength);

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool AddChunkToSourceBuffer(System.IntPtr instance, byte[] buffer, ulong offset, ulong chunkLength);

			[DllImport("AVProVideo")]
			public static extern System.IntPtr EndOpenSourceFromBuffer(System.IntPtr instance, Windows.AudioOutput audioOutput, bool useHardwareDecoding, bool generateTextureMips, bool hintAlphaChannel, 
				bool useLowLatency, bool use10BitTextures, [MarshalAs(UnmanagedType.LPWStr)]string forceAudioOutputDeviceName, int unitySampleRate,
				[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)]IntPtr[] preferredFilter, uint numFilters);

#if NETFX_CORE
			[DllImport("AVProVideo")]
			public static extern System.IntPtr OpenSourceFromStream(System.IntPtr instance, IRandomAccessStream ras, 
				[MarshalAs(UnmanagedType.LPWStr)]string path, Windows.VideoApi videoApi, Windows.AudioOutput audioOutput, bool useHardwareDecoding, bool generateTextureMips, 
				bool hintAlphaChannel, bool useLowLatency, bool use10BitTextures, [MarshalAs(UnmanagedType.LPWStr)]string forceAudioOutputDeviceName, int unitySampleRate);
#endif

			[DllImport("AVProVideo")]
			public static extern void CloseSource(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern System.IntPtr GetPlayerDescription(System.IntPtr instance);

			// Custom Filters

			[DllImport("AVProVideo")]
			public static extern void SetCustomMovParserEnabled(System.IntPtr instance, bool enabled);

			[DllImport("AVProVideo")]
			public static extern void SetHapNotchLCEnabled(System.IntPtr instance, bool enabled);

			[DllImport("AVProVideo")]
			public static extern void SetFrameBufferingEnabled(System.IntPtr instance, bool enabled, bool pauseOnPrerollComplete);

			[DllImport("AVProVideo")]
			public static extern void SetStereoDetectEnabled(System.IntPtr instance, bool enabled);

			[DllImport("AVProVideo")]
			public static extern void SetTextTrackSupportEnabled(System.IntPtr instance, bool enabled);

			[DllImport("AVProVideo")]
			public static extern void SetAudioDelayEnabled(System.IntPtr instance, bool enabled, bool isAutomatic, double timeSeconds);

			[DllImport("AVProVideo")]
			public static extern void SetFacebookAudio360SupportEnabled(System.IntPtr instance, bool enabled);

			// Hap & NotchLC Decoder

			[DllImport("AVProVideo")]
			public static extern void SetDecoderHints(System.IntPtr instance, int parallelFrameCount, int prerollFrameCount);

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool GetDecoderPerformance(System.IntPtr instance, ref int activeDecodeThreadCount, ref int decodedFrameCount, ref int droppedFrameCount);

			// Errors

			[DllImport("AVProVideo")]
			public static extern int GetLastErrorCode(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern long GetLastExtendedErrorCode(System.IntPtr instance);

			// Controls

			[DllImport("AVProVideo")]
			public static extern void Play(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern void Pause(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern void SetMuted(System.IntPtr instance, bool muted);

			[DllImport("AVProVideo")]
			public static extern void SetVolume(System.IntPtr instance, float volume);

			[DllImport("AVProVideo")]
			public static extern void SetBalance(System.IntPtr instance, float volume);

			[DllImport("AVProVideo")]
			public static extern void SetLooping(System.IntPtr instance, bool looping);

			// Properties

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool HasVideo(System.IntPtr instance);

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool HasAudio(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern int GetWidth(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern int GetHeight(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern float GetFrameRate(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern StereoPacking GetStereoPacking(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern double GetDuration(System.IntPtr instance);

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool IsPlaybackStalled(System.IntPtr instance);

			// State

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool HasMetaData(System.IntPtr instance);

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool CanPlay(System.IntPtr instance);

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool IsSeeking(System.IntPtr instance);

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool IsPlaying(System.IntPtr instance);

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool IsFinished(System.IntPtr instance);

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool IsBuffering(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern double GetCurrentTime(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern void SetCurrentTime(System.IntPtr instance, double time, bool fast);

			[DllImport("AVProVideo")]
			public static extern float GetPlaybackRate(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern void SetPlaybackRate(System.IntPtr instance, float rate);

			[DllImport("AVProVideo")]
			public static extern void StartExtractFrame(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern void WaitForExtract(System.IntPtr instance);

			// Update and Rendering

			[DllImport("AVProVideo")]
			public static extern void Update(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern void EndUpdate(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern System.IntPtr GetTexturePointer(System.IntPtr instance);

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool IsTextureTopDown(System.IntPtr instance);

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool IsTextureSampleLinear(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern int GetTextureFrameCount(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern long GetTextureTimeStamp(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern System.IntPtr GetRenderEventFunc_UpdateAllTextures();

			[DllImport("AVProVideo")]
			public static extern System.IntPtr GetRenderEventFunc_FreeTextures();

			[DllImport("AVProVideo")]
			public static extern System.IntPtr GetRenderEventFunc_WaitForNewFrame();

			// Audio Grabbing

			[DllImport("AVProVideo")]
			public static extern int GrabAudio(System.IntPtr instance, float[] buffer, int sampleCount, int channelCount);

			[DllImport("AVProVideo")]
			public static extern int GetAudioBufferedSampleCount(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern int GetAudioChannelCount(System.IntPtr instance);

			[DllImport("AVProVideo")]
			public static extern int GetAudioChannelMask(System.IntPtr instance);

			// Audio 360

			[DllImport("AVProVideo")]
			public static extern int SetAudioChannelMode(System.IntPtr instance, int audio360ChannelMode);

			[DllImport("AVProVideo")]
			public static extern void SetHeadOrientation(System.IntPtr instance, float x, float y, float z, float w);

			[DllImport("AVProVideo")]
			public static extern void SetAudioFocusEnabled(System.IntPtr instance, bool enabled);

			[DllImport("AVProVideo")]
			public static extern void SetAudioFocusProps(System.IntPtr instance, float offFocusLevel, float widthDegrees);

			[DllImport("AVProVideo")]
			public static extern void SetAudioFocusRotation(System.IntPtr instance, float x, float y, float z, float w);
		}
	}
}
#endif