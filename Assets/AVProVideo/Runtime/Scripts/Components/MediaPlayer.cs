//#define AVPROVIDEO_BETA_SUPPORT_TIMESCALE		// BETA FEATURE: comment this in if you want to support frame stepping based on changes in Time.timeScale or Time.captureFramerate
//#define AVPROVIDEO_FORCE_NULL_MEDIAPLAYER		// DEV FEATURE: comment this out to make all mediaplayers use the null mediaplayer
//#define AVPROVIDEO_DISABLE_LOGGING			// DEV FEATURE: disables Debug.Log from AVPro Video
#define AVPROVIDEO_SUPPORT_LIVEEDITMODE
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// This is the primary AVPro Video component and handles all media loading,
	/// seeking, information retrieving etc.  This component does not do any display
	/// of the video.  Instead this is handled by other components such as
	/// ApplyToMesh, ApplyToMaterial, DisplayIMGUI, DisplayUGUI.
	/// </summary>
#if AVPROVIDEO_SUPPORT_LIVEEDITMODE
	[ExecuteInEditMode]
#endif
	[AddComponentMenu("AVPro Video/Media Player", -100)]
	[HelpURL("http://renderheads.com/products/avpro-video/")]
	public partial class MediaPlayer : MonoBehaviour
	{
		// These fields are just used to setup the default properties for a new video that is about to be loaded
		// Once a video has been loaded you should use the interfaces exposed in the properties to
		// change playback properties (eg volume, looping, mute)

		// Media source

		[SerializeField] MediaSource _mediaSource = MediaSource.Reference;
		public MediaSource MediaSource { get { return _mediaSource; } internal set { _mediaSource = value; } }

		[SerializeField] MediaReference _mediaReference = null;
		public MediaReference MediaReference { get { return _mediaReference; } internal set { _mediaReference = value; } }

		[SerializeField] MediaPath _mediaPath = new MediaPath();
		public MediaPath MediaPath { get { return _mediaPath; } internal set { _mediaPath = value; } }

		[SerializeField] MediaHints _fallbackMediaHints = MediaHints.Default;
		public MediaHints FallbackMediaHints { get { return _fallbackMediaHints; } set { _fallbackMediaHints = value; } }

		[FormerlySerializedAs("m_AutoOpen")]
		[SerializeField] bool _autoOpen = true;
		public bool AutoOpen { get { return _autoOpen; } set { _autoOpen = value; } }

		[FormerlySerializedAs("m_AutoStart")]
		[SerializeField] bool _autoPlayOnStart = true;
		public bool AutoStart { get { return _autoPlayOnStart; } set { _autoPlayOnStart = value; } }

		// Basic controls

		[FormerlySerializedAs("m_Loop")]
		[SerializeField] bool _loop = false;
		public bool Loop { get { return _loop; } set { _loop = value; if (_controlInterface != null) _controlInterface.SetLooping(_loop); } }

		[FormerlySerializedAs("m_Volume")]
		[Range(0.0f, 1.0f)]
		[SerializeField] float _audioVolume = 1.0f;
		public virtual float AudioVolume { get { return _audioVolume; } set { _audioVolume = Mathf.Clamp01(value); if (_controlInterface != null) _controlInterface.SetVolume(_audioVolume); } }

		[FormerlySerializedAs("m_Balance")]
		[Range(-1.0f, 1.0f)]
		[SerializeField] float _audioBalance = 0.0f;
		public float AudioBalance { get { return _audioBalance; } set { _audioBalance = Mathf.Clamp(value, -1f, 1f); if (_controlInterface != null) _controlInterface.SetBalance(_audioBalance); } }

		[FormerlySerializedAs("m_Muted")]
		[SerializeField] bool _audioMuted = false;
		public virtual bool AudioMuted { get { return _audioMuted; } set { _audioMuted = value; if (_controlInterface != null) _controlInterface.MuteAudio(_audioMuted); } }

		private AudioSource _audioSource = null;
		public AudioSource AudioSource { get { return _audioSource; } internal set { _audioSource = value; } }

		[FormerlySerializedAs("m_PlaybackRate")]
		[Range(-4.0f, 4.0f)]
		[SerializeField] float _playbackRate = 1.0f;
		public float PlaybackRate { get { return _playbackRate; } set { _playbackRate = value; if (_controlInterface != null) _controlInterface.SetPlaybackRate(_playbackRate); } }

		// Resampler

		[FormerlySerializedAs("m_Resample")]
		[SerializeField] bool _useResampler = false;
		public bool UseResampler { get { return _useResampler; } set { _useResampler = value; } }

		[FormerlySerializedAs("m_ResampleMode")]
		[SerializeField] Resampler.ResampleMode _resampleMode = Resampler.ResampleMode.POINT;
		public Resampler.ResampleMode ResampleMode { get { return _resampleMode; } set { _resampleMode = value; } }
		
		[FormerlySerializedAs("m_ResampleBufferSize")]
		[Range(3, 10)]
		[SerializeField] int _resampleBufferSize = 5;
		public int ResampleBufferSize { get { return _resampleBufferSize; } set { _resampleBufferSize = value; } }

		private Resampler _resampler = null;
		public Resampler FrameResampler	{ get { return _resampler; } }

		// Visual options

		[FormerlySerializedAs("m_videoMapping")]
		[SerializeField] VideoMapping _videoMapping = VideoMapping.Unknown;
		public VideoMapping VideoLayoutMapping { get { return _videoMapping; } set { _videoMapping = value; } }

		[FormerlySerializedAs("m_FilterMode")]
		[SerializeField] FilterMode _textureFilterMode = FilterMode.Bilinear;
		public FilterMode TextureFilterMode { get { return _textureFilterMode; } set { _textureFilterMode = value; if (_controlInterface != null) _controlInterface.SetTextureProperties(_textureFilterMode, _textureWrapMode, _textureAnisoLevel); } }

		[FormerlySerializedAs("m_WrapMode")]
		[SerializeField] TextureWrapMode _textureWrapMode = TextureWrapMode.Clamp;
		public TextureWrapMode TextureWrapMode { get { return _textureWrapMode; } set { _textureWrapMode = value; if (_controlInterface != null) _controlInterface.SetTextureProperties(_textureFilterMode, _textureWrapMode, _textureAnisoLevel); } }

		[FormerlySerializedAs("m_AnisoLevel")]
		[Range(0, 16)]
		[SerializeField] int _textureAnisoLevel = 0;
		public int TextureAnisoLevel { get { return _textureAnisoLevel; } set { _textureAnisoLevel = value; if (_controlInterface != null) _controlInterface.SetTextureProperties(_textureFilterMode, _textureWrapMode, _textureAnisoLevel); } }

		[SerializeField] bool _useVideoResolve = false;
		public bool UseVideoResolve { get { return _useVideoResolve; } set { _useVideoResolve = value; } }

		[SerializeField] VideoResolveOptions _videoResolveOptions = VideoResolveOptions.Create();
		public VideoResolveOptions VideoResolveOptions { get { return _videoResolveOptions; } set { _videoResolveOptions = value; } }

#if AVPRO_FEATURE_VIDEORESOLVE
		[SerializeField] VideoResolve _videoResolve = new VideoResolve();
#endif
		// Sideloaded subtitles

		[FormerlySerializedAs("m_LoadSubtitles")]
		[SerializeField] bool _sideloadSubtitles;
		public bool SideloadSubtitles { get { return _sideloadSubtitles; } set { _sideloadSubtitles = value; } }

		[SerializeField] MediaPath _subtitlePath;
		public MediaPath SubtitlePath { get { return _subtitlePath; } set { _subtitlePath = value; } }

		// Audio 360

		[FormerlySerializedAs("m_AudioHeadTransform")]
		[SerializeField] Transform _audioHeadTransform;
		public Transform AudioHeadTransform { set { _audioHeadTransform = value; }	get { return _audioHeadTransform; } }

		[FormerlySerializedAs("m_AudioFocusEnabled")]
		[SerializeField] bool _audioFocusEnabled;
		public bool AudioFocusEnabled { get { return _audioFocusEnabled; } set { _audioFocusEnabled = value; } }

		[FormerlySerializedAs("m_AudioFocusTransform")]
		[SerializeField] Transform _audioFocusTransform;
		public Transform AudioFocusTransform { get { return _audioFocusTransform; } set { _audioFocusTransform = value; } }

		[FormerlySerializedAs("m_AudioFocusWidthDegrees")]
		[SerializeField, Range(40f, 120f)] float _audioFocusWidthDegrees = 90f;
		public float AudioFocusWidthDegrees { get { return _audioFocusWidthDegrees; } set { _audioFocusWidthDegrees = value; } }

		[FormerlySerializedAs("m_AudioFocusOffLevelDB")]
		[SerializeField, Range(-24f, 0f)] float _audioFocusOffLevelDB = 0f;
		public float AudioFocusOffLevelDB { get { return _audioFocusOffLevelDB; } set { _audioFocusOffLevelDB = value; } }

		// Network

		[SerializeField] HttpHeaderData _httpHeaders = new HttpHeaderData();
		public HttpHeaderData HttpHeaders { get { return _httpHeaders; } set { _httpHeaders = value; } }

		[SerializeField] KeyAuthData _keyAuth = new KeyAuthData();
		public KeyAuthData KeyAuth { get { return _keyAuth; } set { _keyAuth = value; } }

		// Events

		[FormerlySerializedAs("m_events")]
		[SerializeField] MediaPlayerEvent _events = null;
		public MediaPlayerEvent Events
		{
			get
			{
				if (_events == null)
				{
					_events = new MediaPlayerEvent();
				}
				return _events;
			}
		}

		[FormerlySerializedAs("m_eventMask")]
		[SerializeField] int _eventMask = -1;
		public int EventMask { get { return _eventMask; } set { _eventMask = value; } }

		[SerializeField] bool _pauseMediaOnAppPause = true;
		public bool PauseMediaOnAppPause { get { return _pauseMediaOnAppPause; } set { _pauseMediaOnAppPause = value; }	}

		[SerializeField] bool _playMediaOnAppUnpause = true;
		public bool PlayMediaOnAppUnpause { get { return _playMediaOnAppUnpause; } set { _playMediaOnAppUnpause = value; } }

		// Misc options

		[FormerlySerializedAs("m_Persistent")]
		[SerializeField] bool _persistent = false;
		public bool Persistent { get { return _persistent; } set { _persistent = value; } }

		[FormerlySerializedAs("m_forceFileFormat")]
		[SerializeField] FileFormat _forceFileFormat = FileFormat.Unknown;
		public FileFormat ForceFileFormat { get { return _forceFileFormat; } set { _forceFileFormat = value; } }

		// Interfaces

		private BaseMediaPlayer _baseMediaPlayer;
		private IMediaControl _controlInterface;
		private ITextureProducer _textureInterface;
		private IMediaInfo _infoInterface;
		private IMediaPlayer _playerInterface;
		private IMediaSubtitles _subtitlesInterface;
		private IMediaCache _cacheInterface;
		private IBufferedDisplay _bufferedDisplayInterface;
		private IVideoTracks _videoTracksInterface;
		private IAudioTracks _audioTracksInterface;
		private ITextTracks _textTracksInterface;
		private System.IDisposable _disposeInterface;

		public virtual IMediaInfo Info { get { return _infoInterface; } }
		public virtual IMediaControl Control { get { return _controlInterface; } }
		public virtual IMediaPlayer Player { get { return _playerInterface; } }
		public virtual ITextureProducer TextureProducer	{ get { return _textureInterface; }	}
		public virtual IMediaSubtitles Subtitles {	get { return _subtitlesInterface; }	}
		public virtual IVideoTracks VideoTracks {	get { return _videoTracksInterface; } }
		public virtual IAudioTracks AudioTracks {	get { return _audioTracksInterface; } }
		public virtual ITextTracks TextTracks {	get { return _textTracksInterface; } }
		public virtual IMediaCache Cache { get { return _cacheInterface; } }
		public virtual IBufferedDisplay BufferedDisplay { get { return _bufferedDisplayInterface; } }

		// State
		private bool _isMediaOpened = false;
		public bool MediaOpened	{ get { return _isMediaOpened; }	}
		private bool _autoPlayOnStartTriggered = false;
		private bool _wasPlayingOnPause = false;
		private Coroutine _renderingCoroutine = null;

		// Global init
		private static bool s_GlobalStartup = false;

		// Subtitle state
		private MediaPath _queueSubtitlePath;
		private Coroutine _loadSubtitlesRoutine;

		// Extract frame
		private static Camera _dummyCamera = null;
		public delegate void ProcessExtractedFrame(Texture2D extractedFrame);

		/// <summary>
		/// Methods
		/// </summary>

		#if UNITY_EDITOR
		static MediaPlayer()
		{
			SetupEditorPlayPauseSupport();
		}
		#endif

		void Awake()
		{
			if (_persistent)
			{
				// TODO: set "this.transform.root.gameObject" to also DontDestroyOnLoad?
				DontDestroyOnLoad(this.gameObject);
			}
		}

		protected void Initialise()
		{
			BaseMediaPlayer mediaPlayer = CreateMediaPlayer();
			if (mediaPlayer != null)
			{
				// Set-up interface
				_baseMediaPlayer = mediaPlayer;
				_controlInterface = mediaPlayer;
				_textureInterface = mediaPlayer;
				_infoInterface = mediaPlayer;
				_playerInterface = mediaPlayer;
				_subtitlesInterface = mediaPlayer;
				_videoTracksInterface = mediaPlayer;
				_audioTracksInterface = mediaPlayer;
				_textTracksInterface = mediaPlayer;
				_disposeInterface = mediaPlayer;
				_cacheInterface = mediaPlayer;
				_bufferedDisplayInterface = mediaPlayer;

				string nativePluginVersion = mediaPlayer.GetVersion();
				string expectedNativePluginVersion = mediaPlayer.GetExpectedVersion();

				// Check that the plugin version number is not too old
				if (!nativePluginVersion.StartsWith(expectedNativePluginVersion))
				{
					Debug.LogError("[AVProVideo] Plugin version number " + nativePluginVersion + " doesn't match the expected version number " + expectedNativePluginVersion + ".  It looks like the plugin didn't upgrade correctly.  To resolve this please restart Unity and try to upgrade the package again.");
				}

				if (!s_GlobalStartup)
				{
					Helper.LogInfo(string.Format("Initialising AVPro Video v{0} (native plugin v{1}) on {2}/{3} (MT {4}) on {5}", Helper.AVProVideoVersion, nativePluginVersion, SystemInfo.graphicsDeviceName, SystemInfo.graphicsDeviceVersion, SystemInfo.graphicsMultiThreaded, Application.platform));

					#if AVPROVIDEO_BETA_SUPPORT_TIMESCALE
					Debug.LogWarning("[AVProVideo] TimeScale support used.  This could affect performance when changing Time.timeScale or Time.captureFramerate.  This feature is useful for supporting video capture system that adjust time scale during capturing.");
					#endif

					s_GlobalStartup = true;
				}
			}
		}

		void Start()
		{
#if UNITY_WEBGL
			_useResampler = false;
#endif
			if (_controlInterface == null)
			{
				if (Application.isPlaying)
				{
					Initialise();
					if (_controlInterface != null)
					{
						if (_autoOpen)
						{
							OpenMedia(_autoPlayOnStart);

							if (_sideloadSubtitles && _subtitlesInterface != null && _subtitlePath != null && !string.IsNullOrEmpty(_subtitlePath.Path))
							{
								EnableSubtitles(_subtitlePath);
							}
						}

						StartRenderCoroutine();
					}
				}
			}
		}

		public bool OpenMedia(MediaPath path, bool autoPlay = true)
		{
			return OpenMedia(path.PathType, path.Path, autoPlay);
		}

		public bool OpenMedia(MediaPathType pathType, string path, bool autoPlay = true)
		{
			_mediaSource = MediaSource.Path;
			_mediaPath.Path = path;
			_mediaPath.PathType = pathType;
			
			return OpenMedia(autoPlay);
		}

		public bool OpenMedia(MediaReference mediaReference, bool autoPlay = true)
		{
			_mediaSource = MediaSource.Reference;
			_mediaReference = mediaReference;

			return OpenMedia(autoPlay);
		}

		public bool OpenMedia(bool autoPlay = true)
		{
			_autoPlayOnStart = autoPlay;

			if (_controlInterface == null)
			{
				//_autoOpen = false;		 // If OpenVideoFromFile() is called before Start() then set _autoOpen to false so that it doesn't load the video a second time during Start()
				Initialise();
			}

			return InternalOpenMedia();
		}

		private bool InternalOpenMedia()
		{
			bool result = false;
			// Open the video file
			if (_controlInterface != null)
			{
				CloseMedia();

				_isMediaOpened = true;
				_autoPlayOnStartTriggered = !_autoPlayOnStart;
				_finishedFrameOpenCheck = true;
				long fileOffset = GetPlatformFileOffset();	// TODO: replace this with MediaReference

				MediaPath mediaPath = null;
				MediaHints mediaHints = _fallbackMediaHints;

				if (_mediaSource == MediaSource.Reference)
				{
					if (_mediaReference != null)
					{
						mediaPath = _mediaReference.GetCurrentPlatformMediaReference().MediaPath;
						mediaHints = _mediaReference.GetCurrentPlatformMediaReference().Hints;
						if (string.IsNullOrEmpty(mediaPath.Path))
						{
							mediaPath = null;
						}
					}
					else
					{
						Debug.LogError("[AVProVideo] No MediaReference specified", this);
					}
				}
				else if (_mediaSource == MediaSource.Path)
				{
					if (!string.IsNullOrEmpty(_mediaPath.Path))
					{
						mediaPath = _mediaPath;
					}
					else
					{
						Debug.LogError("[AVProVideo] No file path specified", this);
					}
				}
				
				if (null != mediaPath)
				{
					string fullPath = mediaPath.GetResolvedFullPath();
					string customHttpHeaders = null;

					bool checkForFileExist = true;
					bool isURL = fullPath.Contains("://");
					if (isURL)
					{
						checkForFileExist = false;
						customHttpHeaders = GetPlatformHttpHeadersAsString();
					}
#if (!UNITY_EDITOR && UNITY_ANDROID)
					checkForFileExist = false;
#endif
					if (checkForFileExist && !System.IO.File.Exists(fullPath))
					{
						Debug.LogError("[AVProVideo] File not found: " + fullPath, this);
					}
					else
					{
						Helper.LogInfo(string.Format("Opening {0} (offset {1}) with API {2}", fullPath, fileOffset, GetPlatformVideoApiString()), this);

#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
						// NOTE: We don't need to call SetAudioChannelMode on Android,
						// as it's set when the AndroidMediaPlayer object is created
						if (_optionsWindows.audioOutput == Windows.AudioOutput.FacebookAudio360)
						{
							_controlInterface.SetAudioChannelMode(_optionsWindows.audio360ChannelMode);
						}
						else
						{
							_controlInterface.SetAudioChannelMode(Audio360ChannelMode.INVALID);
						}
#elif (!UNITY_EDITOR && UNITY_WSA_10_0)
						if (_optionsWindowsUWP.audioOutput == WindowsUWP.AudioOutput.FacebookAudio360)
						{
							_controlInterface.SetAudioChannelMode(_optionsWindowsUWP.audio360ChannelMode);
						}
						else
						{
							_controlInterface.SetAudioChannelMode(Audio360ChannelMode.INVALID);
						}
#endif
						PlatformOptions options = GetCurrentPlatformOptions();
						bool startWithHighestBitrate = false;
						if (options != null)
						{
							startWithHighestBitrate = options.StartWithHighestBandwidth();
						}

						SetLoadOptions();

						if (!_controlInterface.OpenMedia(fullPath, fileOffset, customHttpHeaders, mediaHints, (int)_forceFileFormat, startWithHighestBitrate))
						{
							Debug.LogError("[AVProVideo] Failed to open " + fullPath, this);
						}
						else
						{
							SetPlaybackOptions();
							result = true;
							StartRenderCoroutine();
						}
					}
				}
				else
				{
					Debug.LogError("[AVProVideo] No file path specified", this);
				}
			}
			return result;
		}

		private void SetLoadOptions()
		{
			// On some platforms we can update the loading options without having to recreate the player
	#if !AVPROVIDEO_FORCE_NULL_MEDIAPLAYER
		#if (UNITY_EDITOR_OSX && UNITY_IOS) || (!UNITY_EDITOR && UNITY_IOS)
		#elif (UNITY_EDITOR_OSX && UNITY_TVOS) || (!UNITY_EDITOR && UNITY_TVOS)
		#elif (UNITY_EDITOR_OSX || (!UNITY_EDITOR && UNITY_STANDALONE_OSX))
		#elif (UNITY_EDITOR_WIN) || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
		#elif (!UNITY_EDITOR && UNITY_WSA_10_0)
		#elif (!UNITY_EDITOR && UNITY_ANDROID)
		#elif (!UNITY_EDITOR && UNITY_WEBGL)
			((WebGLMediaPlayer)_baseMediaPlayer).SetOptions(_optionsWebGL);
		#endif
	#endif

			// Encryption support
			PlatformOptions options = GetCurrentPlatformOptions();
			if (options != null)
			{
				_controlInterface.SetKeyServerAuthToken(options.GetKeyServerAuthToken());
				//_controlInterface.SetKeyServerURL(options.GetKeyServerURL());
				_controlInterface.SetOverrideDecryptionKey(options.GetOverrideDecryptionKey());
			}
		}

		private void SetPlaybackOptions()
		{
			// Set playback options
			if (_controlInterface != null)
			{
				_controlInterface.SetLooping(_loop);
				_controlInterface.SetPlaybackRate(_playbackRate);
				_controlInterface.SetVolume(_audioVolume);
				_controlInterface.SetBalance(_audioBalance);
				_controlInterface.MuteAudio(_audioMuted);
				_controlInterface.SetTextureProperties(_textureFilterMode, _textureWrapMode, _textureAnisoLevel);
			}
		}

		public void CloseMedia()
		{
			// Close the media file
			if (_controlInterface != null)
			{
				if (_events != null && _isMediaOpened && _events.HasListeners() && IsHandleEvent(MediaPlayerEvent.EventType.Closing))
				{
					_events.Invoke(this, MediaPlayerEvent.EventType.Closing, ErrorCode.None);
				}

				_autoPlayOnStartTriggered = false;
				_isMediaOpened = false;
				ResetEvents();

				if (_loadSubtitlesRoutine != null)
				{
					StopCoroutine(_loadSubtitlesRoutine);
					_loadSubtitlesRoutine = null;
				}

				_controlInterface.CloseMedia();
			}

			if (_resampler != null)
			{
				_resampler.Reset();
			}

			StopRenderCoroutine();
		}

		public void RewindPrerollPause()
		{
			PlatformOptionsWindows.pauseOnPrerollComplete = true;
			if (BufferedDisplay != null)
			{
				BufferedDisplay.SetBufferedDisplayOptions(true);
			}
			Rewind(false);
			Play();
		}

		public virtual void Play()
		{
			if (_controlInterface != null && _controlInterface.CanPlay())
			{
				_controlInterface.Play();

				// Mark this event as done because it's irrelevant once playback starts
				_eventFired_ReadyToPlay = true;
			}
			else
			{
				// Can't play, perhaps it's still loading?  Queuing play using _autoPlayOnStart to play after loading
				_autoPlayOnStart = true;
				_autoPlayOnStartTriggered = false;
			}
		}

		public virtual void Pause()
		{
			if (_controlInterface != null && _controlInterface.IsPlaying())
			{
				_controlInterface.Pause();
			}
			_wasPlayingOnPause = false;
#if AVPROVIDEO_BETA_SUPPORT_TIMESCALE
			_timeScaleIsControlling = false;
#endif
		}

		public void Stop()
		{
			if (_controlInterface != null)
			{
				_controlInterface.Stop();
			}
#if AVPROVIDEO_BETA_SUPPORT_TIMESCALE
			_timeScaleIsControlling = false;
#endif
		}

		public void Rewind(bool pause)
		{
			if (_controlInterface != null)
			{
				if (pause)
				{
					Pause();
				}
				_controlInterface.Rewind();
			}
		}

		public void SeekToLiveTime(double offset = 0.0)
		{
			if (_controlInterface != null)
			{
				double liveTime = _controlInterface.GetBufferedTimes().MaxTime;
				if (liveTime > 0.0)
				{
					_controlInterface.Seek(liveTime - offset);
				}
			}
		}

#if UNITY_EDITOR && AVPROVIDEO_SUPPORT_LIVEEDITMODE
		public bool EditorUpdate()
		{
			if (_playerInterface != null)
			{
				Update();
				_playerInterface.Render();
				return true;
			}
			return false;
		}
#endif
		protected virtual void Update()
		{
			if (_controlInterface != null)
			{
				// Auto start the playback
				if (_isMediaOpened && _autoPlayOnStart && !_autoPlayOnStartTriggered && _controlInterface.CanPlay())
				{
					_autoPlayOnStartTriggered = true;
					Play();
				}

				if (Application.isPlaying)
				{
					if (_renderingCoroutine == null && _controlInterface.CanPlay())
					{
						StartRenderCoroutine();
					}
				}

				if (_subtitlesInterface != null && _queueSubtitlePath != null && !string.IsNullOrEmpty(_queueSubtitlePath.Path))
				{
					EnableSubtitles(_queueSubtitlePath);
					_queueSubtitlePath = null;
				}

#if AVPROVIDEO_BETA_SUPPORT_TIMESCALE
				UpdateTimeScale();
#endif

				UpdateAudioHeadTransform();
				UpdateAudioFocus();
				
				_playerInterface.Update();

				// Render (done in co-routine)
				//_playerInterface.Render();

				UpdateErrors();
				UpdateEvents();

				_playerInterface.EndUpdate();
			}

#if UNITY_EDITOR
			CheckEditorAudioMute();
#endif
		}

		private void LateUpdate()
		{
			UpdateResampler();
		}

		private void UpdateResampler()
		{
#if !UNITY_WEBGL
			if (_useResampler)
			{
				if (_resampler == null)
				{
					_resampler = new Resampler(this, gameObject.name, _resampleBufferSize, _resampleMode);
				}
			}
#else
			_useResampler = false;
#endif

			if (_resampler != null)
			{
				_resampler.Update();
				_resampler.UpdateTimestamp();
			}
		}

		void OnEnable()
		{
			if (_controlInterface != null && _wasPlayingOnPause)
			{
				_autoPlayOnStart = true;
				_autoPlayOnStartTriggered = false;
				_wasPlayingOnPause = false;
			}

			if(_playerInterface != null)
			{
				_playerInterface.OnEnable();
				StartRenderCoroutine();
			}
		}

		void OnDisable()
		{
			if (_controlInterface != null)
			{
				if (_controlInterface.IsPlaying())
				{
					Pause();
					// Force an update to ensure the player state is synchronised with the plugin
					Update();
					// Needs to follow Pause() otherwise it will be reset.
					_wasPlayingOnPause = true;
				}
			}

			StopRenderCoroutine();
		}

		protected virtual void OnDestroy()
		{
			CloseMedia();

			_baseMediaPlayer = null;
			_controlInterface = null;
			_textureInterface = null;
			_infoInterface = null;
			_playerInterface = null;
			_subtitlesInterface = null;
			_cacheInterface = null;
			_bufferedDisplayInterface = null;
			_videoTracksInterface = null;
			_audioTracksInterface = null;
			_textTracksInterface = null;

			if (_disposeInterface != null)
			{
				_disposeInterface.Dispose();
				_disposeInterface = null;
			}

			if (_resampler != null)
			{
				_resampler.Release();
				_resampler = null;
			}

			// TODO: possible bug if MediaPlayers are created and destroyed manually (instantiated), OnApplicationQuit won't be called!
		}

		public void ForceDispose()
		{
			OnDisable();
			OnDestroy();
		}

#if UNITY_EDITOR
		public static void EditorAllPlayersDispose()
		{
			AllPlayersDispose();
		}
#endif

		private static void AllPlayersDispose()
		{
			// Clean up any open media players
			MediaPlayer[] players = Resources.FindObjectsOfTypeAll<MediaPlayer>();
			if (players != null && players.Length > 0)
			{
				for (int i = 0; i < players.Length; i++)
				{
					players[i].ForceDispose();
				}
			}
		}

		void OnApplicationQuit()
		{
			if (s_GlobalStartup)
			{
				Helper.LogInfo("Shutdown");

				AllPlayersDispose();

#if UNITY_EDITOR
	#if UNITY_EDITOR_WIN
				WindowsMediaPlayer.DeinitPlatform();
				WindowsRtMediaPlayer.DeinitPlatform();
	#endif
#else
	#if (UNITY_STANDALONE_WIN)
				WindowsMediaPlayer.DeinitPlatform();
				WindowsRtMediaPlayer.DeinitPlatform();
	#elif (UNITY_ANDROID)
				AndroidMediaPlayer.DeinitPlatform();
	#endif
#endif
				s_GlobalStartup = false;
			}
		}

#region Rendering Coroutine

		private void StartRenderCoroutine()
		{
			if (_renderingCoroutine == null)
			{
				// Use the method instead of the method name string to prevent garbage
				_renderingCoroutine = StartCoroutine(FinalRenderCapture());
			}
		}

		private void StopRenderCoroutine()
		{
			if (_renderingCoroutine != null)
			{
				StopCoroutine(_renderingCoroutine);
				_renderingCoroutine = null;
			}
		}

		private IEnumerator FinalRenderCapture()
		{
			// Preallocate the YieldInstruction to prevent garbage
			YieldInstruction wait = new WaitForEndOfFrame();
			while (Application.isPlaying)
			{
				// NOTE: in editor, if the game view isn't visible then WaitForEndOfFrame will never complete
				yield return wait;

				if (this.enabled)
				{
					if (_playerInterface != null)
					{
						_playerInterface.Render();
					}
				}
			}
		}
#endregion // Rendering Coroutine

#region Platform and Path
		public static Platform GetPlatform()
		{
			Platform result = Platform.Unknown;

			// Setup for running in the editor (Either OSX, Windows or Linux)
#if UNITY_EDITOR
#if (UNITY_EDITOR_OSX && UNITY_EDITOR_64)
			result = Platform.MacOSX;
#elif UNITY_EDITOR_WIN
			result = Platform.Windows;
#endif
#else
			// Setup for running builds
#if (UNITY_STANDALONE_WIN)
			result = Platform.Windows;
#elif (UNITY_STANDALONE_OSX)
			result = Platform.MacOSX;
#elif (UNITY_IPHONE || UNITY_IOS)
			result = Platform.iOS;
#elif (UNITY_TVOS)
			result = Platform.tvOS;
#elif (UNITY_ANDROID)
			result = Platform.Android;
#elif (UNITY_WSA_10_0)
			result = Platform.WindowsUWP;
#elif (UNITY_WEBGL)
			result = Platform.WebGL;
#endif

#endif
			return result;
		}

		public PlatformOptions GetCurrentPlatformOptions()
		{
			PlatformOptions result = null;

#if UNITY_EDITOR
#if (UNITY_EDITOR_OSX && UNITY_EDITOR_64)
			result = _optionsMacOSX;
#elif UNITY_EDITOR_WIN
			result = _optionsWindows;
#endif
#else
	// Setup for running builds

#if (UNITY_STANDALONE_WIN)
			result = _optionsWindows;
#elif (UNITY_STANDALONE_OSX)
			result = _optionsMacOSX;
#elif (UNITY_IPHONE || UNITY_IOS)
			result = _optionsIOS;
#elif (UNITY_TVOS)
			result = _optionsTVOS;
#elif (UNITY_ANDROID)
			result = _optionsAndroid;
#elif (UNITY_WSA_10_0)
			result = _optionsWindowsUWP;
#elif (UNITY_WEBGL)
			result = _optionsWebGL;
#endif

#endif
			return result;
		}

#if UNITY_EDITOR
		public PlatformOptions GetPlatformOptions(Platform platform)
		{
			PlatformOptions result = null;

			switch (platform)
			{
				case Platform.Windows:
					result = _optionsWindows;
					break;
				case Platform.MacOSX:
					result = _optionsMacOSX;
					break;
				case Platform.Android:
					result = _optionsAndroid;
					break;
				case Platform.iOS:
					result = _optionsIOS;
					break;
				case Platform.tvOS:
					result = _optionsTVOS;
					break;
				case Platform.WindowsUWP:
					result = _optionsWindowsUWP;
					break;
				case Platform.WebGL:
					result = _optionsWebGL;
					break;
			}

			return result;
		}

		public static string GetPlatformOptionsVariable(Platform platform)
		{
			string result = string.Empty;

			switch (platform)
			{
				case Platform.Windows:
					result = "_optionsWindows";
					break;
				case Platform.MacOSX:
					result = "_optionsMacOSX";
					break;
				case Platform.iOS:
					result = "_optionsIOS";
					break;
				case Platform.tvOS:
					result = "_optionsTVOS";
					break;
				case Platform.Android:
					result = "_optionsAndroid";
					break;
				case Platform.WindowsUWP:
					result = "_optionsWindowsUWP";
					break;
				case Platform.WebGL:
					result = "_optionsWebGL";
					break;
			}

			return result;
		}
#endif

		private string GetPlatformVideoApiString()
		{
			string result = string.Empty;
#if UNITY_EDITOR
	#if UNITY_EDITOR_OSX
	#elif UNITY_EDITOR_WIN
			result = _optionsWindows.videoApi.ToString();
	#elif UNITY_EDITOR_LINUX
	#endif
#else
	#if UNITY_STANDALONE_WIN
			result = _optionsWindows.videoApi.ToString();
	#elif UNITY_WSA_10_0
			result = _optionsWindowsUWP.videoApi.ToString();
	#elif UNITY_ANDROID
			result = _optionsAndroid.videoApi.ToString();
	#endif
#endif
			return result;
		}

		private long GetPlatformFileOffset()
		{
			long result = 0;
#if UNITY_EDITOR
	#if UNITY_EDITOR_OSX
	#elif UNITY_EDITOR_WIN
	#elif UNITY_EDITOR_LINUX
	#endif
#else
	#if UNITY_ANDROID
			result = _optionsAndroid.fileOffset;
	#endif
#endif
			return result;
		}

		private string GetPlatformHttpHeadersAsString()
		{
			string result = null;

#if UNITY_EDITOR
	#if UNITY_EDITOR_OSX
			result = _optionsMacOSX.httpHeaders.ToValidatedString();
	#elif UNITY_EDITOR_WIN
			result = _optionsWindows.httpHeaders.ToValidatedString();
	#elif UNITY_EDITOR_LINUX
	#endif
#else
	#if UNITY_STANDALONE_OSX
			result = _optionsMacOSX.httpHeaders.ToValidatedString();
	#elif UNITY_STANDALONE_WIN
			result = _optionsWindows.httpHeaders.ToValidatedString();
	#elif UNITY_WSA_10_0
			result = _optionsWindowsUWP.httpHeaders.ToValidatedString();
	#elif UNITY_IOS || UNITY_IPHONE
			result = _optionsIOS.httpHeaders.ToValidatedString();
	#elif UNITY_TVOS
			result = _optionsTVOS.httpHeaders.ToValidatedString();
	#elif UNITY_ANDROID
			result = _optionsAndroid.httpHeaders.ToValidatedString();
	#elif UNITY_WEBGL
	#endif
#endif

			if (!string.IsNullOrEmpty(result))
			{
				result = result.Trim();
			}
			
			string globalHeaders = _httpHeaders.ToValidatedString();
			if (!string.IsNullOrEmpty(globalHeaders))
			{
				result += globalHeaders;
				result = result.Trim();
			}

			return result;
		}

		private string GetResolvedFilePath(string filePath, MediaPathType fileLocation)
		{
			string result = string.Empty;

			result = Helper.GetFilePath(filePath, fileLocation);

			#if (UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN))
			if (result.Length > 200 && !result.Contains("://"))
			{
				result = Helper.ConvertLongPathToShortDOS83Path(result);
			}
			#endif

			return result;
		}
#endregion // Platform and Path

#region Create MediaPlayers
		#if (UNITY_EDITOR_WIN) || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
		private static BaseMediaPlayer CreateMediaPlayer(OptionsWindows options)
		{
			BaseMediaPlayer result = null;
			if (options.videoApi == Windows.VideoApi.WinRT)
			{
				if (WindowsRtMediaPlayer.InitialisePlatform())
				{
					result = new WindowsRtMediaPlayer(options);
				}
				else
				{
					Debug.LogWarning(string.Format("[AVProVideo] Failed to initialise WinRT API - platform {0} may not support it.  Trying another video API...", SystemInfo.operatingSystem));
				}
			}

			if (result == null)
			{
				if (WindowsMediaPlayer.InitialisePlatform())
				{
					result = new WindowsMediaPlayer(options);
				}
			}
			return result;
		}
		#endif

		#if (!UNITY_EDITOR && UNITY_WSA_10_0)
		private static BaseMediaPlayer CreateMediaPlayer(OptionsWindowsUWP options)
		{
			BaseMediaPlayer result = null;
			if (options.videoApi == WindowsUWP.VideoApi.WinRT)
			{
				if (WindowsRtMediaPlayer.InitialisePlatform())
				{
					result = new WindowsRtMediaPlayer(options);
				}
				else
				{
					Debug.LogWarning(string.Format("[AVProVideo] Failed to initialise WinRT API - platform {0} may not support it.  Trying another video API...", SystemInfo.operatingSystem));
				}
			}

			if (result == null)
			{
				if (WindowsMediaPlayer.InitialisePlatform())
				{
					result = new WindowsMediaPlayer(options);
				}
			}
			return result;
		}
		#endif

		#if (!UNITY_EDITOR && UNITY_ANDROID)
		private static BaseMediaPlayer CreateMediaPlayer(OptionsAndroid options)
		{
			BaseMediaPlayer result = null;
			// Initialise platform (also unpacks videos from StreamingAsset folder (inside a jar), to the persistent data path)
			if (AndroidMediaPlayer.InitialisePlatform())
			{
				result = new AndroidMediaPlayer(options);
			}
			return result;
		}
		#endif

		#if (UNITY_EDITOR_OSX) || (!UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_IPHONE || UNITY_IOS || UNITY_TVOS))
		private static BaseMediaPlayer CreateMediaPlayer(OptionsApple options)
		{
			AppleMediaPlayer mediaPlayer = new AppleMediaPlayer(options);
			return mediaPlayer;
		}
		#endif

		#if (!UNITY_EDITOR && UNITY_WEBGL)
		private static BaseMediaPlayer CreateMediaPlayer(OptionsWebGL options)
		{
			BaseMediaPlayer result = null;
			if (WebGLMediaPlayer.InitialisePlatform())
			{
				result = new WebGLMediaPlayer(options);
			}
			return result;
		}
		#endif

		private static BaseMediaPlayer CreateMediaPlayerNull()
		{
			return new NullMediaPlayer();
		}

		public virtual BaseMediaPlayer CreateMediaPlayer()
		{
			BaseMediaPlayer mediaPlayer = null;

	#if !AVPROVIDEO_FORCE_NULL_MEDIAPLAYER
		#if (UNITY_EDITOR_OSX && UNITY_IOS) || (!UNITY_EDITOR && UNITY_IOS)
			mediaPlayer = CreateMediaPlayer(_optionsIOS);
		#elif (UNITY_EDITOR_OSX && UNITY_TVOS) || (!UNITY_EDITOR && UNITY_TVOS)
			mediaPlayer = CreateMediaPlayer(_optionsTVOS);
		#elif (UNITY_EDITOR_OSX || (!UNITY_EDITOR && UNITY_STANDALONE_OSX))
			mediaPlayer = CreateMediaPlayer(_optionsMacOSX);
		#elif (UNITY_EDITOR_WIN) || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
			mediaPlayer = CreateMediaPlayer(_optionsWindows);
		#elif (!UNITY_EDITOR && UNITY_WSA_10_0)
			mediaPlayer = CreateMediaPlayer(_optionsWindowsUWP);
		#elif (!UNITY_EDITOR && UNITY_ANDROID)
			mediaPlayer = CreateMediaPlayer(_optionsAndroid);
		#elif (!UNITY_EDITOR && UNITY_WEBGL)
			mediaPlayer = CreateMediaPlayer(_optionsWebGL);
		#endif
	#endif
			// Fallback
			if (mediaPlayer == null)
			{
				Debug.LogError(string.Format("[AVProVideo] Not supported on this platform {0} {1} {2} {3}.  Using null media player!", Application.platform, SystemInfo.deviceModel, SystemInfo.processorType, SystemInfo.operatingSystem));
				mediaPlayer = CreateMediaPlayerNull();
			}

			return mediaPlayer;
		}
#endregion // Create MediaPlayers

		private void UpdateAudioFocus()
		{
			// TODO: we could use gizmos to draw the focus area
			_controlInterface.SetAudioFocusEnabled(_audioFocusEnabled);
			_controlInterface.SetAudioFocusProperties(_audioFocusOffLevelDB, _audioFocusWidthDegrees);
			_controlInterface.SetAudioFocusRotation(_audioFocusTransform == null ? Quaternion.identity : _audioFocusTransform.rotation);
		}

		private void UpdateAudioHeadTransform()
		{
			if (_audioHeadTransform != null)
			{
				_controlInterface.SetAudioHeadRotation(_audioHeadTransform.rotation);
			}
			else
			{
				_controlInterface.ResetAudioHeadRotation();
			}
		}

		private void UpdateErrors()
		{
			ErrorCode errorCode = _controlInterface.GetLastError();
			if (ErrorCode.None != errorCode)
			{
				Debug.LogError("[AVProVideo] Error: " + Helper.GetErrorMessage(errorCode));

				// Display additional information for load failures
				if (ErrorCode.LoadFailed == errorCode)
				{
					#if !UNITY_EDITOR && UNITY_ANDROID
					// TODO: Update this to handle case where media is MediaReference
					if (_mediaPath.Path.ToLower().Contains("http://"))
					{
						Debug.LogError("Android 8 and above require HTTPS by default, change to HTTPS or enable ClearText in the AndroidManifest.xml");
					}
					#endif
				}

				if (_events != null && _events.HasListeners() && IsHandleEvent(MediaPlayerEvent.EventType.Error))
				{
					_events.Invoke(this, MediaPlayerEvent.EventType.Error, errorCode);
				}
			}
		}


#region Save Frame To PNG
#if UNITY_EDITOR || (!UNITY_EDITOR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX))
		[ContextMenu("Save Frame To PNG")]
		public void SaveFrameToPng()
		{
			Texture2D frame = ExtractFrame(null);
			if (frame != null)
			{
				byte[] imageBytes = frame.EncodeToPNG();
				if (imageBytes != null)
				{
					string timecode = Mathf.FloorToInt((float)(Control.GetCurrentTime() * 1000.0)).ToString("D8");
					System.IO.File.WriteAllBytes("frame-" + timecode + ".png", imageBytes);
				}

				Destroy(frame);
			}
		}
		[ContextMenu("Save Frame To EXR")]
		public void SaveFrameToExr()
		{
			Texture frame = (Texture)TextureProducer.GetTexture(0);
			if (frame != null)
			{
				RenderTexture rt = new RenderTexture(frame.width, frame.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
				rt.Create();
				Graphics.Blit(frame, rt);
				Texture2D frameRead = new Texture2D(frame.width, frame.height, TextureFormat.RGBAFloat, false, true);
				RenderTexture.active = rt;
				frameRead.ReadPixels(new Rect(0f, 0f, frame.width, frame.height), 0, 0, false);
				frameRead.Apply(false, false);
				RenderTexture.active = null;
				byte[] imageBytes = frameRead.EncodeToEXR();
				if (imageBytes != null)
				{
					string timecode = Mathf.FloorToInt((float)(Control.GetCurrentTime() * 1000.0)).ToString("D8");
					System.IO.File.WriteAllBytes("frame-" + timecode + ".exr", imageBytes);
				}

				Destroy(frame);
				Texture2D.Destroy(frameRead);
				RenderTexture.Destroy(rt);
			}
		}
#endif
#endregion // Save Frame To PNG
	}
}
