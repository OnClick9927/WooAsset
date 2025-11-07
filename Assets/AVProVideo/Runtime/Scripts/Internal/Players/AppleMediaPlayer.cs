//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

#if UNITY_2017_2_OR_NEWER && (UNITY_EDITOR_OSX || (!UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_TVOS)))

using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RenderHeads.Media.AVProVideo
{
	public sealed partial class AppleMediaPlayer: BaseMediaPlayer
	{
		internal partial struct Native
		{
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
			private const string PluginName = "AVProVideo";
#elif UNITY_IOS || UNITY_TVOS
			private const string PluginName = "__Internal";
#endif
		};

		private static Regex RxSupportedSchema = new Regex("^(https?|file)://", RegexOptions.None);
		private static DateTime Epoch = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		static AppleMediaPlayer()
		{
			#if (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR_OSX
				Native.AVPPluginBootstrap();
			#endif
		}

		private IntPtr _player;
		Native.AVPPlayerSettings _playerSettings;

		private MediaPlayer.OptionsApple _options;

		public AppleMediaPlayer(MediaPlayer.OptionsApple options)
		{
			// Keep a handle on the options
			_options = options;

			// Alert the user to OpenGL renderer being used
			if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore)
			{
				Debug.LogWarning("[AVProVideo] OpenGL is not supported.");
				Debug.Log("[AVProVideo] The video will play but no video frames will be displayed. Please switch to using the Metal rendering API.");
			}

			// Configure the video output settings
			_playerSettings = new Native.AVPPlayerSettings();

			switch (options.textureFormat)
			{
				case MediaPlayer.OptionsApple.TextureFormat.BGRA:
				default:
					_playerSettings.pixelFormat = Native.AVPPlayerVideoPixelFormat.Bgra;
					break;
				case MediaPlayer.OptionsApple.TextureFormat.YCbCr420:
					_playerSettings.pixelFormat = Native.AVPPlayerVideoPixelFormat.YCbCr420;
					break;
			}

			if (options.flags.GenerateMipmaps())
				_playerSettings.videoFlags |= Native.AVPPlayerVideoOutputSettingsFlags.GenerateMipmaps;
			if (QualitySettings.activeColorSpace == ColorSpace.Linear)
				_playerSettings.videoFlags |= Native.AVPPlayerVideoOutputSettingsFlags.LinearColorSpace;

			GetWidthHeightFromResolution(
				options.preferredMaximumResolution,
				options.customPreferredMaximumResolution,
				out _playerSettings.preferredMaximumResolution_width,
				out _playerSettings.preferredMaximumResolution_height
			);

			_playerSettings.maximumPlaybackRate = options.maximumPlaybackRate;

			// Configure the audio output settings
			if (options.audioMode == MediaPlayer.OptionsApple.AudioMode.Unity)
			{
				_playerSettings.audioOutputMode = Native.AVPPlayerAudioOutputMode.Unity;
				_playerSettings.sampleRate = AudioSettings.outputSampleRate;
				int numBuffers;
				AudioSettings.GetDSPBufferSize(out _playerSettings.bufferLength, out numBuffers);
			}

			// Configure any network settings
			_playerSettings.preferredPeakBitRate = options.GetPreferredPeakBitRateInBitsPerSecond();
			_playerSettings.preferredForwardBufferDuration = options.preferredForwardBufferDuration;
			if (options.flags.PlayWithoutBuffering())
				_playerSettings.networkFlags |= Native.AVPPlayerNetworkSettingsFlags.PlayWithoutBuffering;
			if (options.flags.UseSinglePlayerItem())
				_playerSettings.networkFlags |= Native.AVPPlayerNetworkSettingsFlags.UseSinglePlayerItem;

			// Make the player
			_player = Native.AVPPluginMakePlayer(_playerSettings);

			// Setup any other flags from the options
			_flags = _flags.SetAllowExternalPlayback(options.flags.AllowExternalPlayback());
			_flags = _flags.SetResumePlayback(options.flags.ResumePlaybackAfterAudioSessionRouteChange());

			// Force an update to get our state in sync with the native
			Update();
		}

		private static void GetWidthHeightFromResolution(MediaPlayer.OptionsApple.Resolution resolution, Vector2Int custom, out float width, out float height)
		{
			switch (resolution)
			{
				case MediaPlayer.OptionsApple.Resolution.NoPreference:
				default:
					width = 0;
					height = 0;
					break;
				case MediaPlayer.OptionsApple.Resolution._480p:
					width = 640;
					height = 480;
					break;
				case MediaPlayer.OptionsApple.Resolution._720p:
					width = 1280;
					height = 720;
					break;
				case MediaPlayer.OptionsApple.Resolution._1080p:
					width = 1920;
					height = 1080;
					break;
				case MediaPlayer.OptionsApple.Resolution._1440p:
					width = 2560;
					height = 1440;
					break;
				case MediaPlayer.OptionsApple.Resolution._2160p:
					width = 3840;
					height = 2160;
					break;
				case MediaPlayer.OptionsApple.Resolution.Custom:
					width = custom.x;
					height = custom.y;
					break;
			}
		}

		internal partial struct Native
		{
			// Video settings

			internal enum AVPPlayerVideoPixelFormat: int
			{
				Invalid,
				Bgra,
				YCbCr420
			}

			[Flags]
			internal enum AVPPlayerVideoOutputSettingsFlags: int
			{
				None             = 0,
				LinearColorSpace = 1 << 0,
				GenerateMipmaps  = 1 << 1,
			}

			// Audio settings

			internal enum AVPPlayerAudioOutputMode : int
			{
				SystemDirect,
				Unity,
			}

			// Network settings

			[Flags]
			internal enum AVPPlayerNetworkSettingsFlags: int
			{
				None                 = 0,
				PlayWithoutBuffering = 1 << 0,
				UseSinglePlayerItem  = 1 << 1,
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerSettings
			{
				// Video
				internal AVPPlayerVideoPixelFormat pixelFormat;
				internal AVPPlayerVideoOutputSettingsFlags videoFlags;
				internal float preferredMaximumResolution_width;
				internal float preferredMaximumResolution_height;
				internal float maximumPlaybackRate;

				// Audio
				internal AVPPlayerAudioOutputMode audioOutputMode;
				internal int sampleRate;
				internal int bufferLength;
				internal int audioFlags;

				// Network
				internal double preferredPeakBitRate;
				internal double preferredForwardBufferDuration;
				internal AVPPlayerNetworkSettingsFlags networkFlags;
			}

#if UNITY_IOS || UNITY_TVOS
			[DllImport(PluginName)]
			internal static extern void AVPPluginBootstrap();
#endif

			[DllImport(PluginName)]
			internal static extern IntPtr AVPPluginMakePlayer(Native.AVPPlayerSettings settings);
		}
	}

	// IMediaPlayer
	public sealed partial class AppleMediaPlayer
	{
		private const int MaxTexturePlanes = 2;
		private Native.AVPPlayerState _state = new Native.AVPPlayerState();
		private Native.AVPPlayerFlags _flags = Native.AVPPlayerFlags.None;
		private Native.AVPPlayerAssetInfo _assetInfo = new Native.AVPPlayerAssetInfo();
		private Native.AVPPlayerVideoTrackInfo[] _videoTrackInfo = new Native.AVPPlayerVideoTrackInfo[0];
		private Native.AVPPlayerAudioTrackInfo[] _audioTrackInfo = new Native.AVPPlayerAudioTrackInfo[0];
		private Native.AVPPlayerTextTrackInfo[] _textTrackInfo = new Native.AVPPlayerTextTrackInfo[0];
		private Native.AVPPlayerTexture _playerTexture;
		private Native.AVPPlayerText _playerText;
		private Texture2D[] _texturePlanes = new Texture2D[MaxTexturePlanes];
		private float _volume = 1.0f;
		private float _rate = 1.0f;

		public override void OnEnable()
		{

		}

		public override void Update()
		{
			Native.AVPPlayerStatus prevStatus = _state.status;
			Native.AVPPlayerGetState(_player, ref _state);

			Native.AVPPlayerStatus changedStatus = prevStatus ^ _state.status;

			// Need to make sure that lastError is set when status is failed so that the Error event is triggered
			if (/*BaseMediaPlayer.*/_lastError == ErrorCode.None && changedStatus.HasFailed() && _state.status.HasFailed())
			{
				/*BaseMediaPlayer.*/_lastError = ErrorCode.LoadFailed;
			}

			if (_state.status.HasUpdatedAssetInfo())
			{
				Native.AVPPlayerGetAssetInfo(_player, ref _assetInfo);

				if (_state.status.HasVideo())
				{
					_videoTrackInfo = new Native.AVPPlayerVideoTrackInfo[_assetInfo.videoTrackCount];
					for (int i = 0; i < _assetInfo.videoTrackCount; ++i)
					{
						_videoTrackInfo[i] = new Native.AVPPlayerVideoTrackInfo();
						Native.AVPPlayerGetVideoTrackInfo(_player, i, ref _videoTrackInfo[i]);
					}
				}

				if (_state.status.HasAudio())
				{
					_audioTrackInfo = new Native.AVPPlayerAudioTrackInfo[_assetInfo.audioTrackCount];
					for (int i = 0; i < _assetInfo.audioTrackCount; ++i)
					{
						_audioTrackInfo[i] = new Native.AVPPlayerAudioTrackInfo();
						Native.AVPPlayerGetAudioTrackInfo(_player, i, ref _audioTrackInfo[i]);
					}
				}

				if (_state.status.HasText())
				{
					_textTrackInfo = new Native.AVPPlayerTextTrackInfo[_assetInfo.textTrackCount];
					for (int i = 0; i < _assetInfo.textTrackCount; ++i)
					{
						_textTrackInfo[i] = new Native.AVPPlayerTextTrackInfo();
						Native.AVPPlayerGetTextTrackInfo(_player, i, ref _textTrackInfo[i]);
					}
				}

				/*BaseMediaPlayer.*/UpdateTracks();
			}

			if (_state.status.HasUpdatedBufferedTimeRanges())
			{
				if (_state.bufferedTimeRangesCount > 0)
				{
					Native.AVPPlayerTimeRange[] timeRanges = new Native.AVPPlayerTimeRange[_state.bufferedTimeRangesCount];
					Native.AVPPlayerGetBufferedTimeRanges(_player, timeRanges, timeRanges.Length);
					_bufferedTimes = ConvertNativeTimeRangesToTimeRanges(timeRanges);
				}
				else
				{
					_bufferedTimes = new TimeRanges();
				}
			}

			if (_state.status.HasUpdatedSeekableTimeRanges())
			{
				if (_state.seekableTimeRangesCount > 0)
				{
					Native.AVPPlayerTimeRange[] timeRanges = new Native.AVPPlayerTimeRange[_state.seekableTimeRangesCount];
					Native.AVPPlayerGetSeekableTimeRanges(_player, timeRanges, timeRanges.Length);
					_seekableTimes = ConvertNativeTimeRangesToTimeRanges(timeRanges);
				}
				else
				{
					_seekableTimes = new TimeRanges();
				}
			}

			if (_state.status.HasUpdatedTexture())
			{
				Native.AVPPlayerGetTexture(_player, ref _playerTexture);
				for (int i = 0; i < _playerTexture.planeCount; ++i)
				{
					TextureFormat textureFormat = TextureFormat.BGRA32;
					switch (_playerTexture.planes[i].textureFormat)
					{
						case Native.AVPPlayerTextureFormat.R8:
							textureFormat = TextureFormat.R8;
							break;
						case Native.AVPPlayerTextureFormat.RG8:
							textureFormat = TextureFormat.RG16;
							break;
						case Native.AVPPlayerTextureFormat.BC1:
							textureFormat = TextureFormat.DXT1;
							break;
						case Native.AVPPlayerTextureFormat.BC3:
							textureFormat = TextureFormat.DXT5;
							break;
						case Native.AVPPlayerTextureFormat.BC4:
							textureFormat = TextureFormat.BC4;
							break;
						case Native.AVPPlayerTextureFormat.BC5:
							textureFormat = TextureFormat.BC5;
							break;
						case Native.AVPPlayerTextureFormat.BC7:
							textureFormat = TextureFormat.BC7;
							break;
						case Native.AVPPlayerTextureFormat.BGRA8:
						default:
							break;
					}

					if (_texturePlanes[i] == null ||
						_texturePlanes[i].width != _playerTexture.planes[i].width ||
						_texturePlanes[i].height != _playerTexture.planes[i].height ||
						_texturePlanes[i].format != textureFormat)
					{
						// Ensure any existing texture is released.
						if (_texturePlanes[i] != null)
						{
							_texturePlanes[i].UpdateExternalTexture(IntPtr.Zero);
							_texturePlanes[i] = null;
						}
						_texturePlanes[i] = Texture2D.CreateExternalTexture(
							_playerTexture.planes[i].width,
							_playerTexture.planes[i].height,
							textureFormat,
							_playerTexture.flags.IsMipmapped(),
							_playerTexture.flags.IsLinear(),
							_playerTexture.planes[i].plane
						);
						base.ApplyTextureProperties(_texturePlanes[i]);
					}
					else
					{
						_texturePlanes[i].UpdateExternalTexture(_playerTexture.planes[i].plane);
					}
				}
			}

			if (_state.status.HasUpdatedText())
			{
				Native.AVPPlayerGetText(_player, ref _playerText);
				/*BaseMediaPlayer.*/UpdateTextCue();
			}

			if (_flags.IsDirty())
			{
				_flags = _flags.SetDirty(false);
				Native.AVPPlayerSetFlags(_player, (int)_flags);
			}

			if (_options.HasChanged())
			{
				if (_options.HasChanged(MediaPlayer.OptionsApple.ChangeFlags.PreferredPeakBitRate))
				{
					_playerSettings.preferredPeakBitRate = _options.GetPreferredPeakBitRateInBitsPerSecond();
				}
				if (_options.HasChanged(MediaPlayer.OptionsApple.ChangeFlags.PreferredForwardBufferDuration))
				{
					_playerSettings.preferredForwardBufferDuration = _options.preferredForwardBufferDuration;
				}
				if (_options.HasChanged(MediaPlayer.OptionsApple.ChangeFlags.PlayWithoutBuffering))
				{
					bool enabled = (_options.flags & MediaPlayer.OptionsApple.Flags.PlayWithoutBuffering) == MediaPlayer.OptionsApple.Flags.PlayWithoutBuffering;
					_playerSettings.networkFlags = enabled ? _playerSettings.networkFlags | Native.AVPPlayerNetworkSettingsFlags.PlayWithoutBuffering
														   : _playerSettings.networkFlags & ~Native.AVPPlayerNetworkSettingsFlags.PlayWithoutBuffering;
				}
				if (_options.HasChanged(MediaPlayer.OptionsApple.ChangeFlags.PreferredMaximumResolution))
				{
					GetWidthHeightFromResolution(
						_options.preferredMaximumResolution,
						_options.customPreferredMaximumResolution,
						out _playerSettings.preferredMaximumResolution_width,
						out _playerSettings.preferredMaximumResolution_height);
				}

				Native.AVPPlayerSetPlayerSettings(_player, _playerSettings);
			}

			/*BaseMediaPlayer.*/UpdateDisplayFrameRate();
			/*BaseMediaPlayer.*/UpdateSubtitles();
		}

		public override void Render()
		{

		}

		public override IntPtr GetNativePlayerHandle()
		{
			return _player;
		}

		private static TimeRanges ConvertNativeTimeRangesToTimeRanges(Native.AVPPlayerTimeRange[] ranges)
		{
			TimeRange[] targetRanges = new TimeRange[ranges.Length];
			for (int i = 0; i < ranges.Length; i++)
			{
				targetRanges[i].startTime = ranges[i].start;
				targetRanges[i].duration = ranges[i].duration;
			}
			return new TimeRanges(targetRanges);
		}

		internal partial struct Native
		{
			[Flags]
			internal enum AVPPlayerStatus : int
			{
				Unknown                   = 0,
				ReadyToPlay               = 1 <<  0,
				Playing                   = 1 <<  1,
				Paused                    = 1 <<  2,
				Finished                  = 1 <<  3,
				Seeking                   = 1 <<  4,
				Buffering                 = 1 <<  5,
				Stalled                   = 1 <<  6,
				ExternalPlaybackActive    = 1 <<  7,
				Cached                    = 1 <<  8,
				FinishedSeeking           = 1 <<  9,

				UpdatedAssetInfo          = 1 << 16,
				UpdatedTexture            = 1 << 17,
				UpdatedBufferedTimeRanges = 1 << 18,
				UpdatedSeekableTimeRanges = 1 << 19,
				UpdatedText               = 1 << 20,

				HasVideo                  = 1 << 24,
				HasAudio                  = 1 << 25,
				HasText                   = 1 << 26,
				HasMetadata               = 1 << 27,

				Failed                    = 1 << 31
			}

			[Flags]
			internal enum AVPPlayerFlags : int
			{
				None                  = 0,
				Looping               = 1 <<  0,
				Muted                 = 1 <<  1,
				AllowExternalPlayback = 1 <<  2,
				ResumePlayback        = 1 << 16,	// iOS only, resumes playback after audio session route change
				Dirty                 = 1 << 31
			}

			internal enum AVPPlayerExternalPlaybackVideoGravity : int
			{
				Resize,
				ResizeAspect,
				ResizeAspectFill
			};

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerSize
			{
				internal float width;
				internal float height;
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPAffineTransform
			{
				internal float a;
				internal float b;
				internal float c;
				internal float d;
				internal float tx;
				internal float ty;
			}

			[Flags]
			internal enum AVPPlayerAssetFlags : int
			{
				None                  = 0,
				CompatibleWithAirPlay = 1 << 0,
			};

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerAssetInfo
			{
				internal double duration;
				internal AVPPlayerSize dimensions;
				internal float frameRate;
				internal int videoTrackCount;
				internal int audioTrackCount;
				internal int textTrackCount;
				internal AVPPlayerAssetFlags flags;
			}

			[Flags]
			internal enum AVPPlayerTrackFlags: int
			{
				Default = 1 << 0,
			}

			internal enum AVPPlayerVideoTrackStereoMode: int
			{
				Unknown,
				Monoscopic,
				StereoscopicTopBottom,
				StereoscopicLeftRight,
				StereoscopicCustom,
				StereoscopicRightLeft,
			}

			[Flags]
			internal enum AVPPlayerVideoTrackFlags: int
			{
				HasAlpha = 1 << 0,
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerVideoTrackInfo
			{
				[MarshalAs(UnmanagedType.LPWStr)] internal string name;
				[MarshalAs(UnmanagedType.LPWStr)] internal string language;
				internal int trackId;
				internal float estimatedDataRate;
				internal uint codecSubtype;
				internal AVPPlayerTrackFlags flags;

				internal AVPPlayerSize dimensions;
				internal float frameRate;
				internal AVPAffineTransform transform;
				internal AVPPlayerVideoTrackStereoMode stereoMode;
				internal int bitsPerComponent;
				internal AVPPlayerVideoTrackFlags videoTrackFlags;

				internal Matrix4x4 yCbCrTransform;
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerAudioTrackInfo
			{
				[MarshalAs(UnmanagedType.LPWStr)] internal string name;
				[MarshalAs(UnmanagedType.LPWStr)] internal string language;
				internal int trackId;
				internal float estimatedDataRate;
				internal uint codecSubtype;
				internal AVPPlayerTrackFlags flags;

				internal double sampleRate;
				internal uint channelCount;
				internal uint channelLayoutTag;
				internal AudioChannelMaskFlags channelBitmap;
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerTextTrackInfo
			{
				[MarshalAs(UnmanagedType.LPWStr)] internal string name;
				[MarshalAs(UnmanagedType.LPWStr)] internal string language;
				internal int trackId;
				internal float estimatedDataRate;
				internal uint codecSubtype;
				internal AVPPlayerTrackFlags flags;
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerTimeRange
			{
				internal double start;
				internal double duration;
			};

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerState
			{
				internal AVPPlayerStatus status;
				internal double currentTime;
				internal double currentDate;
				internal int selectedVideoTrack;
				internal int selectedAudioTrack;
				internal int selectedTextTrack;
				internal int bufferedTimeRangesCount;
				internal int seekableTimeRangesCount;
				internal int audioCaptureBufferedSamplesCount;
			}

			internal enum AVPPlayerTextureFormat: int
			{
				Unknown,
				BGRA8,
				R8,
				RG8,
				BC1,
				BC3,
				BC4,
				BC5,
				BC7,
				BGR10A2,
				R16,
				RG16,
				BGR10XR,
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerTexturePlane
			{
				internal IntPtr plane;
				internal int width;
				internal int height;
				internal AVPPlayerTextureFormat textureFormat;
			}

			[Flags]
			internal enum AVPPlayerTextureFlags: int
			{
				None      = 0,
				Flipped   = 1 << 0,
				Linear    = 1 << 1,
				Mipmapped = 1 << 2,
			}

			internal enum AVPPlayerTextureYCbCrMatrix: int
			{
				Identity,
				ITU_R_601,
				ITU_R_709,
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerTexture
			{
				[MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
				internal AVPPlayerTexturePlane[] planes;
				internal long itemTime;
				internal int frameCount;
				internal int planeCount;
				internal AVPPlayerTextureFlags flags;
				internal AVPPlayerTextureYCbCrMatrix YCbCrMatrix;
			};

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerText
			{
				internal IntPtr buffer;
				internal long itemTime;
				internal int length;
				internal int sequence;
			};

			[DllImport(PluginName)]
			internal static extern void AVPPlayerGetState(IntPtr player, ref AVPPlayerState state);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerSetFlags(IntPtr player, int flags);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerGetAssetInfo(IntPtr player, ref AVPPlayerAssetInfo info);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerGetVideoTrackInfo(IntPtr player, int index, ref AVPPlayerVideoTrackInfo info);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerGetAudioTrackInfo(IntPtr player, int index, ref AVPPlayerAudioTrackInfo info);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerGetTextTrackInfo(IntPtr player, int index, ref AVPPlayerTextTrackInfo info);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerGetBufferedTimeRanges(IntPtr player, AVPPlayerTimeRange[] ranges, int count);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerGetSeekableTimeRanges(IntPtr player, AVPPlayerTimeRange[] ranges, int count);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerGetTexture(IntPtr player, ref AVPPlayerTexture texture);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerGetText(IntPtr player, ref AVPPlayerText text);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerSetPlayerSettings(IntPtr player, AVPPlayerSettings settings);
		}
	}

	internal static class AppleMediaPlayerExtensions
	{
		// AVPPlayerStatus

		internal static bool IsReadyToPlay(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.ReadyToPlay) == AppleMediaPlayer.Native.AVPPlayerStatus.ReadyToPlay;
		}

		internal static bool IsPlaying(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.Playing) == AppleMediaPlayer.Native.AVPPlayerStatus.Playing;
		}

		internal static bool IsPaused(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.Paused) == AppleMediaPlayer.Native.AVPPlayerStatus.Paused;
		}

		internal static bool IsFinished(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.Finished) == AppleMediaPlayer.Native.AVPPlayerStatus.Finished;
		}

		internal static bool IsSeeking(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.Seeking) == AppleMediaPlayer.Native.AVPPlayerStatus.Seeking;
		}

		internal static bool IsBuffering(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.Buffering) == AppleMediaPlayer.Native.AVPPlayerStatus.Buffering;
		}

		internal static bool IsStalled(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.Stalled) == AppleMediaPlayer.Native.AVPPlayerStatus.Stalled;
		}

		internal static bool IsExternalPlaybackActive(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.ExternalPlaybackActive) == AppleMediaPlayer.Native.AVPPlayerStatus.ExternalPlaybackActive;
		}

		internal static bool IsCached(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.Cached) == AppleMediaPlayer.Native.AVPPlayerStatus.Cached;
		}

		internal static bool HasFinishedSeeking(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.FinishedSeeking) == AppleMediaPlayer.Native.AVPPlayerStatus.FinishedSeeking;
		}

		internal static bool HasUpdatedAssetInfo(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.UpdatedAssetInfo) == AppleMediaPlayer.Native.AVPPlayerStatus.UpdatedAssetInfo;
		}

		internal static bool HasUpdatedTexture(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.UpdatedTexture) == AppleMediaPlayer.Native.AVPPlayerStatus.UpdatedTexture;
		}

		internal static bool HasUpdatedBufferedTimeRanges(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.UpdatedBufferedTimeRanges) == AppleMediaPlayer.Native.AVPPlayerStatus.UpdatedBufferedTimeRanges;
		}

		internal static bool HasUpdatedSeekableTimeRanges(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.UpdatedSeekableTimeRanges) == AppleMediaPlayer.Native.AVPPlayerStatus.UpdatedSeekableTimeRanges;
		}

		internal static bool HasUpdatedText(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.UpdatedText) == AppleMediaPlayer.Native.AVPPlayerStatus.UpdatedText;
		}

		internal static bool HasVideo(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.HasVideo) == AppleMediaPlayer.Native.AVPPlayerStatus.HasVideo;
		}

		internal static bool HasAudio(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.HasAudio) == AppleMediaPlayer.Native.AVPPlayerStatus.HasAudio;
		}

		internal static bool HasText(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.HasText) == AppleMediaPlayer.Native.AVPPlayerStatus.HasText;
		}

		internal static bool HasMetadata(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.HasMetadata) == AppleMediaPlayer.Native.AVPPlayerStatus.HasMetadata;
		}

		internal static bool HasFailed(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.Failed) == AppleMediaPlayer.Native.AVPPlayerStatus.Failed;
		}

		// AVPPlayerFlags

		internal static bool IsLooping(this AppleMediaPlayer.Native.AVPPlayerFlags flags)
		{
			return (flags & AppleMediaPlayer.Native.AVPPlayerFlags.Looping) == AppleMediaPlayer.Native.AVPPlayerFlags.Looping;
		}

		internal static AppleMediaPlayer.Native.AVPPlayerFlags SetLooping(this AppleMediaPlayer.Native.AVPPlayerFlags flags, bool b)
		{
			if (flags.IsLooping() ^ b)
			{
				flags = (b ? flags | AppleMediaPlayer.Native.AVPPlayerFlags.Looping
				           : flags & ~AppleMediaPlayer.Native.AVPPlayerFlags.Looping) | AppleMediaPlayer.Native.AVPPlayerFlags.Dirty;
			}
			return flags;
		}

		internal static bool IsMuted(this AppleMediaPlayer.Native.AVPPlayerFlags flags)
		{
			return (flags & AppleMediaPlayer.Native.AVPPlayerFlags.Muted) == AppleMediaPlayer.Native.AVPPlayerFlags.Muted;
		}

		internal static AppleMediaPlayer.Native.AVPPlayerFlags SetMuted(this AppleMediaPlayer.Native.AVPPlayerFlags flags, bool b)
		{
			if (flags.IsMuted() ^ b)
			{
				flags = (b ? flags | AppleMediaPlayer.Native.AVPPlayerFlags.Muted
				           : flags & ~AppleMediaPlayer.Native.AVPPlayerFlags.Muted) | AppleMediaPlayer.Native.AVPPlayerFlags.Dirty;
			}
			return flags;
		}

		internal static bool IsExternalPlaybackAllowed(this AppleMediaPlayer.Native.AVPPlayerFlags flags)
		{
			return (flags & AppleMediaPlayer.Native.AVPPlayerFlags.AllowExternalPlayback) == AppleMediaPlayer.Native.AVPPlayerFlags.AllowExternalPlayback;
		}

		internal static AppleMediaPlayer.Native.AVPPlayerFlags SetAllowExternalPlayback(this AppleMediaPlayer.Native.AVPPlayerFlags flags, bool b)
		{
			if (flags.IsExternalPlaybackAllowed() ^ b)
			{
				flags = (b ? flags |  AppleMediaPlayer.Native.AVPPlayerFlags.AllowExternalPlayback
				           : flags & ~AppleMediaPlayer.Native.AVPPlayerFlags.AllowExternalPlayback) | AppleMediaPlayer.Native.AVPPlayerFlags.Dirty;
			}
			return flags;
		}

		internal static bool ResumePlayback(this AppleMediaPlayer.Native.AVPPlayerFlags flags)
		{
			return (flags & AppleMediaPlayer.Native.AVPPlayerFlags.ResumePlayback) == AppleMediaPlayer.Native.AVPPlayerFlags.ResumePlayback;
		}

		internal static AppleMediaPlayer.Native.AVPPlayerFlags SetResumePlayback(this AppleMediaPlayer.Native.AVPPlayerFlags flags, bool b)
		{
			if (flags.ResumePlayback() ^ b)
			{
				flags = (b ? flags | AppleMediaPlayer.Native.AVPPlayerFlags.ResumePlayback
				           : flags & ~AppleMediaPlayer.Native.AVPPlayerFlags.ResumePlayback) | AppleMediaPlayer.Native.AVPPlayerFlags.Dirty;
			}
			return flags;
		}

		internal static bool IsDirty(this AppleMediaPlayer.Native.AVPPlayerFlags flags)
		{
			return (flags & AppleMediaPlayer.Native.AVPPlayerFlags.Dirty) == AppleMediaPlayer.Native.AVPPlayerFlags.Dirty;
		}

		internal static AppleMediaPlayer.Native.AVPPlayerFlags SetDirty(this AppleMediaPlayer.Native.AVPPlayerFlags flags, bool b)
		{
			if (flags.IsDirty() ^ b)
			{
				flags = b ? flags | AppleMediaPlayer.Native.AVPPlayerFlags.Dirty : flags & ~AppleMediaPlayer.Native.AVPPlayerFlags.Dirty;
			}
			return flags;
		}

		// MARK: AVPPlayerAssetFlags

		internal static bool IsCompatibleWithAirPlay(this AppleMediaPlayer.Native.AVPPlayerAssetFlags flags)
		{
			return (flags & AppleMediaPlayer.Native.AVPPlayerAssetFlags.CompatibleWithAirPlay) == AppleMediaPlayer.Native.AVPPlayerAssetFlags.CompatibleWithAirPlay;
		}

		// MARK: AVPPlayerTrackFlags

		internal static bool IsDefault(this AppleMediaPlayer.Native.AVPPlayerTrackFlags flags)
		{
			return (flags & AppleMediaPlayer.Native.AVPPlayerTrackFlags.Default) == AppleMediaPlayer.Native.AVPPlayerTrackFlags.Default;
		}

		// AVPPlayerTextureFlags

		internal static bool IsFlipped(this AppleMediaPlayer.Native.AVPPlayerTextureFlags flags)
		{
			return (flags & AppleMediaPlayer.Native.AVPPlayerTextureFlags.Flipped) == AppleMediaPlayer.Native.AVPPlayerTextureFlags.Flipped;
		}

		internal static bool IsLinear(this AppleMediaPlayer.Native.AVPPlayerTextureFlags flags)
		{
			return (flags & AppleMediaPlayer.Native.AVPPlayerTextureFlags.Linear) == AppleMediaPlayer.Native.AVPPlayerTextureFlags.Linear;
		}

		internal static bool IsMipmapped(this AppleMediaPlayer.Native.AVPPlayerTextureFlags flags)
		{
			return (flags & AppleMediaPlayer.Native.AVPPlayerTextureFlags.Mipmapped) == AppleMediaPlayer.Native.AVPPlayerTextureFlags.Mipmapped;
		}
	}

	// IMediaControl
	public sealed partial class AppleMediaPlayer
	{
		public override bool OpenMedia(string path, long offset, string headers, MediaHints mediaHints, int forceFileFormat, bool startWithHighestBitrate)
		{
			_mediaHints = mediaHints;

			bool b = false;
			Match match = RxSupportedSchema.Match(path);
			if (match.Success)
			{
				string schema = match.Value;
				if (schema == "http://" || schema == "https://")
				{
					b = Native.AVPPlayerOpenURL(_player, path, headers);
				}
				else if (schema == "file://")
				{
					b = Native.AVPPlayerOpenURL(_player, path, null);
				}
				else
				{
					Debug.LogWarningFormat("[AVProVideo] Unsupported schema '{0}'", schema);
				}
			}
			else if (path.StartsWith("/"))
			{
				b = Native.AVPPlayerOpenURL(_player, "file://" + path, null);
			}
			else
			{
				Debug.LogWarning("[AVProVideo] Path is not a URL nor is it absolute.");
			}

			if (b)
			{
				Update();
			}

			return b;
		}

		public override bool OpenMediaFromBuffer(byte[] buffer)
		{
			// Unsupported
			return false;
		}

		public override bool StartOpenMediaFromBuffer(ulong length)
		{
			// Unsupported
			return false;
		}

		public override bool AddChunkToMediaBuffer(byte[] chunk, ulong offset, ulong length)
		{
			// Unsupported
			return false;
		}

		public override bool EndOpenMediaFromBuffer()
		{
			// Unsupported
			return false;
		}

		public override void CloseMedia()
		{
			Native.AVPPlayerClose(_player);
			Update();
			_playerTexture.frameCount = 0;
		}

		public override void SetLooping(bool b)
		{
			_flags = _flags.SetLooping(b);
		}

		public override bool IsLooping()
		{
			return _flags.IsLooping();
		}

		public override bool HasMetaData()
		{
			return _state.status.HasMetadata();
		}

		public override bool CanPlay()
		{
			return _state.status.IsReadyToPlay();
		}

		public override bool IsPlaying()
		{
			return _state.status.IsPlaying();
		}

		public override bool IsSeeking()
		{
			return _state.status.IsSeeking() || _state.status.HasFinishedSeeking();
		}

		public override bool IsPaused()
		{
			return _state.status.IsPaused();
		}

		public override bool IsFinished()
		{
			return _state.status.IsFinished();
		}

		public override bool IsBuffering()
		{
			return _state.status.IsBuffering();
		}

		public override void Play()
		{
			Native.AVPPlayerSetRate(_player, _rate);
			Update();
		}

		public override void Pause()
		{
			Native.AVPPlayerSetRate(_player, 0.0f);
			Update();
		}

		public override void Stop()
		{
			Pause();
		}

		public override void Rewind()
		{
			SeekWithTolerance(0.0, 0.0, 0.0);
		}

		public override void Seek(double toTime)
		{
			SeekWithTolerance(toTime, 0.0, 0.0);
		}

		public override void SeekFast(double toTime)
		{
			SeekWithTolerance(toTime, double.PositiveInfinity, double.PositiveInfinity);
		}

		public override void SeekWithTolerance(double toTime, double toleranceBefore, double toleranceAfter)
		{
			Native.AVPPlayerSeek(_player, toTime, toleranceBefore, toleranceAfter);
			Update();
		}

		public override double GetCurrentTime()
		{
			return _state.currentTime;
		}

		public override DateTime GetProgramDateTime()
		{
			return Epoch.AddSeconds(_state.currentDate);
		}

		public override float GetPlaybackRate()
		{
			return _rate;
		}

		public override void SetPlaybackRate(float rate)
		{
			if (rate != _rate) {
				_rate = rate;
				Native.AVPPlayerSetRate(_player, rate);
				Update();
			}
		}

		public override void MuteAudio(bool mute)
		{
			_flags = _flags.SetMuted(mute);
		}

		public override bool IsMuted()
		{
			return _flags.IsMuted();
		}

		public override void SetVolume(float volume)
		{
			if (volume != _volume) {
				_volume = volume;
				Native.AVPPlayerSetVolume(_player, volume);
			}
		}

		public override void SetBalance(float balance)
		{
			// Unsupported
		}

		public override float GetVolume()
		{
			return _volume;
		}

		public override float GetBalance()
		{
			// Unsupported
			return 0.0f;
		}

		public override long GetLastExtendedErrorCode()
		{
			return 0;
		}

		public override int GetAudioChannelCount()
		{
			int channelCount = -1;
			if (_state.selectedAudioTrack > -1 && _state.selectedAudioTrack < _audioTrackInfo.Length)
			{
				channelCount = (int)_audioTrackInfo[_state.selectedAudioTrack].channelCount;
#if UNITY_IOS && !UNITY_EDITOR
					if (_options.audioMode == MediaPlayer.OptionsApple.AudioMode.Unity)
					{
						// iOS audio capture will convert down to two channel stereo
						channelCount = Math.Min(channelCount, 2);
					}
#endif
			}
			return channelCount;
		}

		public override AudioChannelMaskFlags GetAudioChannelMask()
		{
			if (_state.selectedAudioTrack != -1 && _state.selectedAudioTrack < _audioTrackInfo.Length)
			{
				return _audioTrackInfo[_state.selectedAudioTrack].channelBitmap;
			}
			return AudioChannelMaskFlags.Unspecified;
		}

		public override int GrabAudio(float[] buffer, int sampleCount, int channelCount)
		{
			return Native.AVPPlayerGetAudio(_player, buffer, buffer.Length);
		}

		public override int GetAudioBufferedSampleCount()
		{
			return _state.audioCaptureBufferedSamplesCount;
		}

		public override void SetAudioHeadRotation(Quaternion q)
		{
			// Unsupported
		}

		public override void ResetAudioHeadRotation()
		{
			// Unsupported
		}

		public override void SetAudioChannelMode(Audio360ChannelMode channelMode)
		{
			// Unsupported
		}

		public override void SetAudioFocusEnabled(bool enabled)
		{
			// Unsupported
		}

		public override void SetAudioFocusProperties(float offFocusLevel, float widthDegrees)
		{
			// Unsupported
		}

		public override void SetAudioFocusRotation(Quaternion q)
		{
			// Unsupported
		}

		public override void ResetAudioFocus()
		{
			// Unsupported
		}

		public override bool WaitForNextFrame(Camera camera, int previousFrameCount)
		{
			return false;
		}

		public override void SetKeyServerAuthToken(string token)
		{
			Native.AVPPlayerSetKeyServerAuthToken(_player, token);
		}

		public override void SetOverrideDecryptionKey(byte[] key)
		{
			int length = key != null ? key.Length : 0;
			Native.AVPPlayerSetDecryptionKey(_player, key, length);
		}

		public override bool IsExternalPlaybackActive()
		{
			return _state.status.IsExternalPlaybackActive();
		}

		public override void SetAllowsExternalPlayback(bool enable)
		{
			_flags.SetAllowExternalPlayback(enable);
		}

		public override void SetExternalPlaybackVideoGravity(ExternalPlaybackVideoGravity gravity_)
		{
			Native.AVPPlayerExternalPlaybackVideoGravity gravity;
			switch (gravity_)
			{
				case ExternalPlaybackVideoGravity.Resize:
				default:
					gravity = Native.AVPPlayerExternalPlaybackVideoGravity.Resize;
					break;
				case ExternalPlaybackVideoGravity.ResizeAspect:
					gravity = Native.AVPPlayerExternalPlaybackVideoGravity.ResizeAspect;
					break;
				case ExternalPlaybackVideoGravity.ResizeAspectFill:
					gravity = Native.AVPPlayerExternalPlaybackVideoGravity.ResizeAspectFill;
					break;
			}
			Native.AVPPlayerSetExternalPlaybackVideoGravity(_player, gravity);
		}

		internal partial struct Native
		{
			[DllImport(PluginName)]
			[return: MarshalAs(UnmanagedType.U1)]
			internal static extern bool AVPPlayerOpenURL(IntPtr player, string url, string headers);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerClose(IntPtr player);

			[DllImport(PluginName)]
			internal static extern int AVPPlayerGetAudio(IntPtr player, float[] buffer, int length);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerSetRate(IntPtr player, float rate);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerSetVolume(IntPtr player, float volume);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerSetExternalPlaybackVideoGravity(IntPtr player, AVPPlayerExternalPlaybackVideoGravity gravity);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerSeek(IntPtr player, double toTime, double toleranceBefore, double toleranceAfter);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerSetKeyServerAuthToken(IntPtr player, string token);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerSetKeyServerURL(IntPtr player, string url);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerSetDecryptionKey(IntPtr player, byte[] key, int length);
		}
	}

	// IMediaInfo
	public sealed partial class AppleMediaPlayer
	{
		public override double GetDuration()
		{
			return _assetInfo.duration;
		}

		public override int GetVideoWidth()
		{
			int width = 0;
			if (_state.selectedVideoTrack >= 0) {
				width = (int)_videoTrackInfo[_state.selectedVideoTrack].dimensions.width;
			}
			return width;
		}

		public override int GetVideoHeight()
		{
			int height = 0;
			if (_state.selectedVideoTrack >= 0) {
				height = (int)_videoTrackInfo[_state.selectedVideoTrack].dimensions.height;
			}
			return height;
		}

		public override float GetVideoFrameRate()
		{
			float framerate = 0.0f;
			if (_state.selectedVideoTrack >= 0) {
				framerate = _videoTrackInfo[_state.selectedVideoTrack].frameRate;
			}
			return framerate;
		}

		public override bool HasVideo()
		{
			return _state.status.HasVideo();
		}

		public override bool HasAudio()
		{
			return _state.status.HasAudio();
		}

		public override bool PlayerSupportsLinearColorSpace()
		{
			return _playerTexture.flags.IsLinear();
		}

		public override bool IsPlaybackStalled()
		{
			return _state.status.IsStalled();
		}

		public override float[] GetTextureTransform()
		{
			if (_state.selectedVideoTrack >= 0)
			{
				Native.AVPAffineTransform transform = _videoTrackInfo[_state.selectedVideoTrack].transform;
				return new float[] { transform.a, transform.b, transform.c, transform.d, transform.tx, transform.ty };
			}
			else
			{
				return new float[] { 1.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f };
			}
		}

		public override long GetEstimatedTotalBandwidthUsed()
		{
			return 0;
		}

		public override bool IsExternalPlaybackSupported()
		{
			return _assetInfo.flags.IsCompatibleWithAirPlay();
		}
	}

	// IMediaProducer
	public sealed partial class AppleMediaPlayer
	{
		public override int GetTextureCount()
		{
			return _playerTexture.planeCount;
		}

		public override Texture GetTexture(int index)
		{
			return _texturePlanes[index];
		}

		public override int GetTextureFrameCount()
		{
			return _playerTexture.frameCount;
		}

		public override bool SupportsTextureFrameCount()
		{
			return true;
		}

		public override long GetTextureTimeStamp()
		{
			return _playerTexture.itemTime;
		}

		public override bool RequiresVerticalFlip()
		{
			return _playerTexture.flags.IsFlipped();
		}

		public override TransparencyMode GetTextureTransparency()
		{
			if (_state.selectedVideoTrack >= 0)
			{
				Native.AVPPlayerVideoTrackInfo info = _videoTrackInfo[_state.selectedVideoTrack];
				if ((info.videoTrackFlags & Native.AVPPlayerVideoTrackFlags.HasAlpha) == Native.AVPPlayerVideoTrackFlags.HasAlpha)
				{
					return TransparencyMode.Transparent;
				}
			}
			return base.GetTextureTransparency();
		}

		public override Matrix4x4 GetYpCbCrTransform()
		{
			if (_videoTrackInfo.Length > 0 && _state.selectedVideoTrack >= 0)
				return _videoTrackInfo[_state.selectedVideoTrack].yCbCrTransform;
			else
				return Matrix4x4.identity;
		}

		internal override StereoPacking InternalGetTextureStereoPacking()
		{
			if (_state.selectedVideoTrack >= 0)
			{
				switch (_videoTrackInfo[_state.selectedVideoTrack].stereoMode)
				{
					case Native.AVPPlayerVideoTrackStereoMode.Unknown:
						return StereoPacking.Unknown;
					case Native.AVPPlayerVideoTrackStereoMode.Monoscopic:
						return StereoPacking.None;
					case Native.AVPPlayerVideoTrackStereoMode.StereoscopicLeftRight:
						return StereoPacking.LeftRight;
					case Native.AVPPlayerVideoTrackStereoMode.StereoscopicTopBottom:
						return StereoPacking.TopBottom;
					case Native.AVPPlayerVideoTrackStereoMode.StereoscopicRightLeft:
						return StereoPacking.Unknown;
					case Native.AVPPlayerVideoTrackStereoMode.StereoscopicCustom:
						return StereoPacking.CustomUV;
				}
			}
			return StereoPacking.Unknown;
		}
	}

	// IDispose
	public sealed partial class AppleMediaPlayer
	{
		public override void Dispose()
		{
			Native.AVPPlayerRelease(_player);
		}
	}

	// Version
	public sealed partial class AppleMediaPlayer
	{
		public override string GetVersion()
		{
			return Native.GetPluginVersion();
		}

		public override string GetExpectedVersion()
		{
			return Helper.ExpectedPluginVersion.Apple;
		}

		internal partial struct Native
		{
			[DllImport(PluginName)]
			private static extern IntPtr AVPPluginGetVersionStringPointer();

			internal static string GetPluginVersion()
			{
				return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(AVPPluginGetVersionStringPointer());
			}
		}
	}

	// Media selection
	public sealed partial class AppleMediaPlayer
	{
		internal override bool InternalIsChangedTracks(TrackType trackType)
		{
			return _state.status.HasUpdatedAssetInfo();
		}

		internal override int InternalGetTrackCount(TrackType trackType)
		{
			switch (trackType)
			{
				case TrackType.Video:
					return _videoTrackInfo.Length;
				case TrackType.Audio:
					return _audioTrackInfo.Length;
				case TrackType.Text:
					return _textTrackInfo.Length;
				default:
					return 0;
			}
		}

		internal override bool InternalSetActiveTrack(TrackType trackType, int index)
		{
			switch (trackType)
			{
				case TrackType.Video:
					return Native.AVPPlayerSetTrack(_player, Native.AVPPlayerTrackType.Video, index);

				case TrackType.Audio:
					return Native.AVPPlayerSetTrack(_player, Native.AVPPlayerTrackType.Audio, index);

				case TrackType.Text:
					return Native.AVPPlayerSetTrack(_player, Native.AVPPlayerTrackType.Text, index);

				default:
					return false;
			}
		}

		internal override TrackBase InternalGetTrackInfo(TrackType type, int index, ref bool isActiveTrack)
		{
			TrackBase track = null;
			switch (type)
			{
				case TrackType.Video:
					if (index >= 0 && index < _videoTrackInfo.Length)
					{
						Native.AVPPlayerVideoTrackInfo trackInfo = _videoTrackInfo[index];
						track = new VideoTrack(index, trackInfo.name, trackInfo.language, trackInfo.flags.IsDefault());
						isActiveTrack = _state.selectedVideoTrack == index;
					}
					break;

				case TrackType.Audio:
					if (index >= 0 && index < _audioTrackInfo.Length)
					{
						Native.AVPPlayerAudioTrackInfo trackInfo = _audioTrackInfo[index];
						track = new AudioTrack(index, trackInfo.name, trackInfo.language, trackInfo.flags.IsDefault());
						isActiveTrack = _state.selectedAudioTrack == index;
					}
					break;

				case TrackType.Text:
					if (index >= 0 && index < _textTrackInfo.Length)
					{
						Native.AVPPlayerTextTrackInfo trackInfo = _textTrackInfo[index];
						track = new TextTrack(index, trackInfo.name, trackInfo.language, trackInfo.flags.IsDefault());
						isActiveTrack = _state.selectedTextTrack == index;
					}
					break;

				default:
					break;
			}
			return track;
		}

		internal override bool InternalIsChangedTextCue()
		{
			return _state.status.HasUpdatedText();
		}

		internal override string InternalGetCurrentTextCue()
		{
			if (_playerText.buffer != IntPtr.Zero)
				return Marshal.PtrToStringUni(_playerText.buffer, _playerText.length);
			else
				return null;
		}

		internal partial struct Native
		{
			internal enum AVPPlayerTrackType: int
			{
				Video,
				Audio,
				Text
			};

			[DllImport(PluginName)]
			[return: MarshalAs(UnmanagedType.I1)]
			internal static extern bool AVPPlayerSetTrack(IntPtr player, AVPPlayerTrackType type, int index);
		}
	}

#if UNITY_IOS
	// Media Caching
	public sealed partial class AppleMediaPlayer
	{
        public override bool IsMediaCachingSupported()
        {
            return true;
        }

		public override void AddMediaToCache(string url, string headers, MediaCachingOptions options)
		{
			Native.MediaCachingOptions nativeOptions = new Native.MediaCachingOptions();
			GCHandle artworkHandle = new GCHandle();

			if (options != null)
			{
				nativeOptions.minimumRequiredBitRate = options.minimumRequiredBitRate;
				nativeOptions.minimumRequiredResolution_width = options.minimumRequiredResolution.x;
				nativeOptions.minimumRequiredResolution_height = options.minimumRequiredResolution.y;
				nativeOptions.title = options.title;
				if (options.artwork != null && options.artwork.Length > 0)
				{
					artworkHandle = GCHandle.Alloc(options.artwork, GCHandleType.Pinned);
					nativeOptions.artwork = artworkHandle.AddrOfPinnedObject();
					nativeOptions.artworkLength = options.artwork.Length;
				}
			}

			Native.AVPPluginCacheMediaForURL(url, headers, nativeOptions);

			if (artworkHandle.IsAllocated)
			{
				artworkHandle.Free();
			}
		}

		public override void CancelDownloadOfMediaToCache(string url)
		{
			Native.AVPPluginCancelDownloadOfMediaForURL(url);
		}

		public override void RemoveMediaFromCache(string url)
		{
			Native.AVPPluginRemoveCachedMediaForURL(url);
		}

        public override CachedMediaStatus GetCachedMediaStatus(string url, ref float progress)
        {
			return (CachedMediaStatus)Native.AVPPluginGetCachedMediaStatusForURL(url, ref progress);
        }

		internal partial struct Native
		{
			public struct MediaCachingOptions
			{
				public double minimumRequiredBitRate;
				public float  minimumRequiredResolution_width;
				public float  minimumRequiredResolution_height;
				public string title;
				public IntPtr artwork;
				public int    artworkLength;
			}

			[DllImport(PluginName)]
			public static extern void AVPPluginCacheMediaForURL(string url, string headers, MediaCachingOptions options);

			[DllImport(PluginName)]
			public static extern void AVPPluginCancelDownloadOfMediaForURL(string url);

			[DllImport(PluginName)]
			public static extern void AVPPluginRemoveCachedMediaForURL(string url);

			[DllImport(PluginName)]
			public static extern int AVPPluginGetCachedMediaStatusForURL(string url, ref float progress);
		}
	}
#endif

	// Native interface
	public sealed partial class AppleMediaPlayer
	{
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
			private const string PluginName = "AVProVideo";
#elif UNITY_IOS || UNITY_TVOS
			private const string PluginName = "__Internal";
#endif

		internal partial struct Native
		{
			[DllImport(PluginName)]
			public static extern IntPtr AVPPlayerRelease(IntPtr player);
		}
	}
}

#endif
