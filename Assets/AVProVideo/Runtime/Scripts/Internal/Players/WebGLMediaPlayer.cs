//#define AVPRO_WEBGL_USE_RENDERTEXTURE
// NOTE: We only allow this script to compile in editor so we can easily check for compilation issues
#if (UNITY_EDITOR || UNITY_WEBGL)
using UnityEngine;
using System;
using System.Text;
using System.Runtime.InteropServices;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// WebGL implementation of BaseMediaPlayer
	/// </summary>
	public sealed class WebGLMediaPlayer : BaseMediaPlayer
	{
		//private enum AVPPlayerStatus
		//{
		//    Unknown,
		//    ReadyToPlay,
		//    Playing,
		//    Finished,
		//    Seeking,
		//    Failed
		//}

		[DllImport("__Internal")]
		private static extern bool AVPPlayerInsertVideoElement(string path, int[] idValues, int externalLibrary);

		[DllImport("__Internal")]
		private static extern int AVPPlayerWidth(int player);

		[DllImport("__Internal")]
		private static extern int AVPPlayerHeight(int player);

		[DllImport("__Internal")]
		private static extern int AVPPlayerGetLastError(int player);

		[DllImport("__Internal")]
		private static extern int AVPPlayerGetVideoTrackCount(int player);

		[DllImport("__Internal")]
		private static extern int AVPPlayerGetAudioTrackCount(int player);

		[DllImport("__Internal")]
		private static extern int AVPPlayerGetTextTrackCount(int player);

		[DllImport("__Internal")]
		private static extern bool AVPPlayerSetActiveVideoTrack(int player, int trackId);

		[DllImport("__Internal")]
		private static extern bool AVPPlayerSetActiveAudioTrack(int player, int trackId);

		[DllImport("__Internal")]
		private static extern bool AVPPlayerSetActiveTextTrack(int player, int trackId);

		[DllImport("__Internal")]
		private static extern void AVPPlayerClose(int player);

		[DllImport("__Internal")]
		private static extern bool AVPPlayerReady(int player);

		[DllImport("__Internal")]
		private static extern void AVPPlayerSetLooping(int player, bool loop);

		[DllImport("__Internal")]
		private static extern bool AVPPlayerIsLooping(int player);

		[DllImport("__Internal")]
		private static extern bool AVPPlayerIsSeeking(int player);

		[DllImport("__Internal")]
		private static extern bool AVPPlayerIsPlaying(int player); 

		[DllImport("__Internal")]
		private static extern bool AVPPlayerIsPaused(int player);

		[DllImport("__Internal")]
		private static extern bool AVPPlayerIsFinished(int player);

		[DllImport("__Internal")]
		private static extern bool AVPPlayerIsBuffering(int player);

		[DllImport("__Internal")]
		private static extern bool AVPPlayerIsPlaybackStalled(int player);

		[DllImport("__Internal")]
		private static extern bool AVPPlayerPlay(int player);

		[DllImport("__Internal")]
		private static extern void AVPPlayerPause(int player);

		[DllImport("__Internal")]
		private static extern void AVPPlayerSeekToTime(int player, double time, bool fast);

		[DllImport("__Internal")]
		private static extern double AVPPlayerGetCurrentTime(int player);

		[DllImport("__Internal")]
		private static extern float AVPPlayerGetDuration(int player);

		[DllImport("__Internal")]
		private static extern float AVPPlayerGetPlaybackRate(int player);

		[DllImport("__Internal")]
		private static extern void AVPPlayerSetPlaybackRate(int player, float rate);

		[DllImport("__Internal")]
		private static extern void AVPPlayerSetMuted(int player, bool muted);

		[DllImport("__Internal")]
		private static extern bool AVPPlayerIsMuted(int player);

		[DllImport("__Internal")]
		private static extern float AVPPlayerGetVolume(int player);

		[DllImport("__Internal")]
		private static extern void AVPPlayerSetVolume(int player, float volume);

		[DllImport("__Internal")]
		private static extern bool AVPPlayerHasVideo(int player);

		[DllImport("__Internal")]
		private static extern bool AVPPlayerHasAudio(int player);

		[DllImport("__Internal")]
		private static extern void AVPPlayerCreateVideoTexture(int textureId);

		[DllImport("__Internal")]
		private static extern void AVPPlayerDestroyVideoTexture(int textureId);

		[DllImport("__Internal")]
		private static extern void AVPPlayerFetchVideoTexture(int player, IntPtr texture, bool init);

		[DllImport("__Internal")]
		private static extern int AVPPlayerGetDecodedFrameCount(int player);

		[DllImport("__Internal")]
		private static extern bool AVPPlayerSupportedDecodedFrameCount(int player);

		[DllImport("__Internal")]
		private static extern bool AVPPlayerHasMetadata(int player);

		[DllImport("__Internal")]
		private static extern int AVPPlayerUpdatePlayerIndex(int id);

		[DllImport("__Internal")]
		private static extern int AVPPlayerGetNumBufferedTimeRanges(int id);

		[DllImport("__Internal")]
		private static extern double AVPPlayerGetTimeRangeStart(int id, int timeRangeIndex);
		[DllImport("__Internal")]
		private static extern double AVPPlayerGetTimeRangeEnd(int id, int timeRangeIndex);

		[DllImport("__Internal")]
		private static extern string AVPPlayerGetVideoTrackName(int player, int trackIndex);

		[DllImport("__Internal")]
		private static extern string AVPPlayerGetAudioTrackName(int player, int trackIndex);

		[DllImport("__Internal")]
		private static extern string AVPPlayerGetTextTrackName(int player, int trackIndex);

		[DllImport("__Internal")]
		private static extern string AVPPlayerGetVideoTrackLanguage(int player, int trackIndex);

		[DllImport("__Internal")]
		private static extern string AVPPlayerGetAudioTrackLanguage(int player, int trackIndex);

		[DllImport("__Internal")]
		private static extern string AVPPlayerGetTextTrackLanguage(int player, int trackIndex);

		[DllImport("__Internal")]
		private static extern bool AVPPlayerIsVideoTrackActive(int player, int trackIndex);

		[DllImport("__Internal")]
		private static extern bool AVPPlayerIsAudioTrackActive(int player, int trackIndex);

		[DllImport("__Internal")]
		private static extern bool AVPPlayerIsTextTrackActive(int player, int trackIndex);

		private WebGL.ExternalLibrary _externalLibrary = WebGL.ExternalLibrary.None;
		private int _playerIndex = -1;
		private int _playerID = -1;

		#if AVPRO_WEBGL_USE_RENDERTEXTURE
		private RenderTexture _texture = null;
		#else
		private Texture2D _texture = null;
		#endif

		private int _width = 0;
		private int _height = 0;
		private int _cachedVideoTrackCount = 0;
		private int _cachedAudioTrackCount = 0;
		private int _cachedTextTrackCount = 0;
		private bool _isDirtyVideoTracks = false;
		private bool _isDirtyAudioTracks = false;
		private bool _isDirtyTextTracks = false;
		private bool _useTextureMips = false;
		private System.IntPtr _cachedTextureNativePtr = System.IntPtr.Zero;

		private static bool _isWebGL1 = false;

		public static bool InitialisePlatform()
		{
			_isWebGL1 = (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2);
			return true;
		}

		public WebGLMediaPlayer(MediaPlayer.OptionsWebGL options)
		{
			SetOptions(options);
		}

		public void SetOptions(MediaPlayer.OptionsWebGL options)
		{
			_externalLibrary = options.externalLibrary;
			_useTextureMips = options.useTextureMips;
		}

		public override string GetVersion()
		{
			return "2.1.6";
		}

		public override string GetExpectedVersion()
		{
			return GetVersion();
		}

		public override bool OpenMedia(string path, long offset, string httpHeader, MediaHints mediaHints, int forceFileFormat = 0, bool startWithHighestBitrate = false)
		{
			bool result = false;

			if (path.StartsWith("http://") || 
				path.StartsWith("https://") ||
				path.StartsWith("file://") ||
				path.StartsWith("blob:") ||
				path.StartsWith("chrome-extension://"))
			{
				int[] idValues = new int[2];
				idValues[0] = -1;
				AVPPlayerInsertVideoElement(path, idValues, (int)_externalLibrary);
				{
					int playerIndex = idValues[0];
					_playerID = idValues[1];

					if (playerIndex > -1)
					{
						_playerIndex = playerIndex;
						_mediaHints = mediaHints;
						result = true;
					}
				}
			}
			else
			{
				Debug.LogError("[AVProVideo] Unknown URL protocol");
			}

			return result;
		}

		public override void CloseMedia()
		{
			if (_playerIndex != -1)
			{
				Pause();

				_width = 0;
				_height = 0;
				_cachedVideoTrackCount = 0;
				_cachedAudioTrackCount = 0;
				_cachedTextTrackCount = 0;
				_isDirtyVideoTracks = false;
				_isDirtyAudioTracks = false;
				_isDirtyTextTracks = false;

				AVPPlayerClose(_playerIndex);

				if (_texture != null)
				{
					DestroyTexture();
				}

				_playerIndex = -1;
				_playerID = -1;

				base.CloseMedia();
			}
		}

		public override bool IsLooping()
		{
			//Debug.Assert(_player != -1, "no player IsLooping");
			bool result = false;

			if (_playerIndex != -1)
			{
				result = AVPPlayerIsLooping(_playerIndex);
			}

			return result;
		}

		public override void SetLooping(bool looping)
		{
			//Debug.Assert(_playerIndex != -1, "no player SetLooping");

			AVPPlayerSetLooping(_playerIndex, looping);
		}

		public override bool HasAudio()
		{
			//Debug.Assert(_player != -1, "no player HasAudio");
			bool result = false;

			if (_playerIndex != -1)
			{
				result = AVPPlayerHasAudio(_playerIndex);
			}

			return result;
		}

		public override bool HasVideo()
		{
			//Debug.Assert(_player != -1, "no player HasVideo");
			bool result = false;

			if (_playerIndex != -1)
			{
				result = AVPPlayerHasVideo(_playerIndex);
			}

			return result;
		}

		public override bool HasMetaData()
		{
			//Debug.Assert(_player != -1, "no player HasMetaData");
			bool result = false;

			if (_playerIndex != -1)
			{
				result = AVPPlayerHasMetadata(_playerIndex);
			}

			return result;
		}

		public override bool CanPlay()
		{
			//Debug.Assert(_player != -1, "no player CanPlay");
			bool result = false;

			if (_playerIndex != -1)
			{
				result = AVPPlayerReady(_playerIndex);
			}

			return result;
		}

		public override void Play()
		{
			Debug.Assert(_playerIndex != -1, "no player Play");

			if (!AVPPlayerPlay(_playerIndex))
			{
				Debug.LogWarning("[AVProVideo] Browser permission prevented video playback");
			}
		}

		public override void Pause()
		{
			Debug.Assert(_playerIndex != -1, "no player Pause");

			AVPPlayerPause(_playerIndex);
		}

		public override void Stop()
		{
			Debug.Assert(_playerIndex != -1, "no player Stop");

			AVPPlayerPause(_playerIndex);
		}

		public override void Seek(double time)
		{
			Debug.Assert(_playerIndex != -1, "no player Seek");
			AVPPlayerSeekToTime(_playerIndex, time, false);
		}

		public override void SeekFast(double time)
		{
			Debug.Assert(_playerIndex != -1, "no player SeekFast");
			AVPPlayerSeekToTime(_playerIndex, time, true);
		}

		public override double GetCurrentTime()
		{
			//Debug.Assert(_player != -1, "no player GetCurrentTime");
			double result = 0.0;
			if (_playerIndex != -1)
			{
				result = AVPPlayerGetCurrentTime(_playerIndex);
			}
			return result;
		}

		public override void SetPlaybackRate(float rate)
		{
			Debug.Assert(_playerIndex != -1, "no player SetPlaybackRate");

			// No HTML implementations allow negative rate yet
			rate = Mathf.Clamp(rate, 0.25f, 8f);

			AVPPlayerSetPlaybackRate(_playerIndex, rate);
		}

		public override float GetPlaybackRate()
		{
			//Debug.Assert(_player != -1, "no player GetPlaybackRate");
			float result = 0.0f;

			if (_playerIndex != -1)
			{
				result = AVPPlayerGetPlaybackRate(_playerIndex);
			}

			return result;
		}

		public override double GetDuration()
		{
			//Debug.Assert(_player != -1, "no player GetDuration");
			double result = 0.0;
			if (_playerIndex != -1)
			{
				result = AVPPlayerGetDuration(_playerIndex);
			}
			return result;
		}

		public override int GetVideoWidth()
		{
			if (_width == 0)
			{
				_width = AVPPlayerWidth(_playerIndex);
			}
			return _width;
		}

		public override int GetVideoHeight()
		{
			if (_height == 0)
			{
				_height = AVPPlayerHeight(_playerIndex);
			}
			return _height;
		}

		public override float GetVideoFrameRate()
		{
			// There is no way in HTML5 yet to get the frame rate of the video
			return 0f;
		}

		public override bool IsSeeking()
		{
			//Debug.Assert(_player != -1, "no player IsSeeking");
			bool result = false;

			if (_playerIndex != -1)
			{
				result = AVPPlayerIsSeeking(_playerIndex);
			}

			return result;
		}

		public override bool IsPlaying()
		{
			//Debug.Assert(_player != -1, "no player IsPlaying");
			bool result = false;

			if (_playerIndex != -1)
			{
				result = AVPPlayerIsPlaying(_playerIndex);
			}

			return result;
		}

		public override bool IsPaused()
		{
			//Debug.Assert(_player != -1, "no player IsPaused");
			bool result = false;

			if (_playerIndex != -1)
			{
				result = AVPPlayerIsPaused(_playerIndex);
			}

			return result;
		}

		public override bool IsFinished()
		{
			//Debug.Assert(_player != -1, "no player IsFinished");
			bool result = false;

			if (_playerIndex != -1)
			{
				result = AVPPlayerIsFinished(_playerIndex);
			}

			return result;
		}

		public override bool IsBuffering()
		{
			//Debug.Assert(_player != -1, "no player IsBuffering");
			bool result = false;

			if (_playerIndex != -1)
			{
				result = AVPPlayerIsBuffering(_playerIndex);
			}

			return result;
		}

		public override Texture GetTexture( int index )
		{
			return _texture;
		}

		public override int GetTextureFrameCount()
		{
			//Debug.Assert(_player != -1, "no player GetTextureFrameCount");
			int result = 0;

			if (_playerIndex != -1)
			{
				result = AVPPlayerGetDecodedFrameCount(_playerIndex);
			}

			return result;
		}

		internal override StereoPacking InternalGetTextureStereoPacking()
		{
			return StereoPacking.Unknown;
		}

		public override bool SupportsTextureFrameCount()
		{
			bool result = false;

			if (_playerIndex != -1)
			{
				result = AVPPlayerSupportedDecodedFrameCount(_playerIndex);
			}

			return result;
		}

		public override bool RequiresVerticalFlip()
		{
			return true;
		}

		public override bool IsMuted()
		{
			//Debug.Assert(_player != -1, "no player IsMuted");
			bool result = false;

			if (_playerIndex != -1)
			{
				result = AVPPlayerIsMuted(_playerIndex);
			}

			return result;
		}

		public override void MuteAudio(bool bMute)
		{
			Debug.Assert(_playerIndex != -1, "no player MuteAudio");

			AVPPlayerSetMuted(_playerIndex, bMute);
		}

		public override void SetVolume(float volume)
		{
			Debug.Assert(_playerIndex != -1, "no player SetVolume");

			AVPPlayerSetVolume(_playerIndex, volume);
		}

		public override float GetVolume()
		{
			//Debug.Assert(_player != -1, "no player GetVolume");
			float result = 0.0f;

			if (_playerIndex != -1)
			{
				result = AVPPlayerGetVolume(_playerIndex);
			}

			return result;
		}

		public override void Render()
		{
			
		}

		private void UpdateLastErrorCode()
		{
			var code = AVPPlayerGetLastError(_playerIndex);

			switch(code){
				case 0:
					_lastError = ErrorCode.None;
					break;
				case 1:
					_lastError = ErrorCode.LoadFailed;
					break;
				case 2:
					_lastError = ErrorCode.LoadFailed;
					break;
				case 3:
					_lastError = ErrorCode.DecodeFailed;
					break;
				case 4:
					_lastError = ErrorCode.LoadFailed;
					break;
				default:
					break;
			}
		}

		private bool IsMipMapGenerationSupported(int videoWidth, int videoHeight)
		{
			if (!_isWebGL1 || (Mathf.IsPowerOfTwo(videoWidth) && Mathf.IsPowerOfTwo(videoHeight)))
			{
				// Mip generation only supported in WebGL 2.0, or WebGL 1.0 when using power-of-two textures
				return true;
			}
			return false;
		}

		private void CreateTexture()
		{
			//Debug.Log("creating texture " + _width + " X " + _height);

#if AVPRO_WEBGL_USE_RENDERTEXTURE
			_texture = new RenderTexture(_width, _height, 0, RenderTextureFormat.Default);
			_texture.autoGenerateMips = false;
			_texture.useMipMap = (_useTextureMips && IsMipMapGenerationSupported(_width, _height));
			_texture.Create();
			_cachedTextureNativePtr = _texture.GetNativeTexturePtr();
#else
			int textureId = 80000 + _playerIndex;
			_cachedTextureNativePtr = new System.IntPtr(textureId);
			AVPPlayerCreateVideoTexture(textureId);
			// TODO: add support for mip generation
			_texture = Texture2D.CreateExternalTexture(_width, _height, TextureFormat.RGBA32, false, false, _cachedTextureNativePtr);
			if (_useTextureMips)
			{
				Debug.LogWarning("[AVProVideo] Texture Mips not yet implemented in this WebGL rendering path");
			}
			//Debug.Log("created texture1 " + _texture);
			//Debug.Log("created texture2 " + _texture.GetNativeTexturePtr().ToInt32());
#endif

			ApplyTextureProperties(_texture);

			bool initTexture = true;
			#if AVPRO_WEBGL_USE_RENDERTEXTURE
			// Textures in WebGL 2.0 don't require texImage2D as they are already recreated with texStorage2D
			initTexture = _isWebGL1;
			#endif
			AVPPlayerFetchVideoTexture(_playerIndex, _cachedTextureNativePtr, initTexture);
		}

		private void DestroyTexture()
		{
			// Have to update with zero to release Metal textures!
			//_texture.UpdateExternalTexture(0);
			if (_texture != null)
			{
			#if AVPRO_WEBGL_USE_RENDERTEXTURE
				RenderTexture.Destroy(_texture);
			#else
				Texture2D.Destroy(_texture);
				AVPPlayerDestroyVideoTexture(_cachedTextureNativePtr.ToInt32());
			#endif
				_texture = null;
			}
			_cachedTextureNativePtr = System.IntPtr.Zero;
		}

		public override void Update()
		{
			if(_playerID >= 0) // CheckPlayer's index and update it
			{
				_playerIndex = AVPPlayerUpdatePlayerIndex(_playerID);
			}

			if(_playerIndex >= 0)
			{
				CheckTracksDirty();
				UpdateTracks();
				UpdateTextCue();
				UpdateSubtitles();
				UpdateLastErrorCode();

				if (AVPPlayerReady(_playerIndex))
				{
					UpdateTimeRanges();
					if (AVPPlayerHasVideo(_playerIndex))
					{
						_width = AVPPlayerWidth(_playerIndex);
						_height = AVPPlayerHeight(_playerIndex);

						if (_texture != null && (_texture.width != _width || _texture.height != _height))
						{
							DestroyTexture();
						}

						if (_texture == null && _width > 0 && _height > 0)
						{
							CreateTexture();
						}

						// Update the texture
						if (_cachedTextureNativePtr != System.IntPtr.Zero)
						{
							// TODO: only update the texture when the frame count changes
							// (actually this will break the update for certain browsers such as edge and possibly safari - Sunrise)
							AVPPlayerFetchVideoTexture(_playerIndex, _cachedTextureNativePtr, false);

							#if AVPRO_WEBGL_USE_RENDERTEXTURE
							if (_texture.useMipMap)
							{
								_texture.GenerateMips();
							}
							#endif
						}

						UpdateDisplayFrameRate();
					}
				}
			}
		}

		private void CheckTracksDirty()
		{
			_isDirtyVideoTracks = false;
			_isDirtyAudioTracks = false;
			_isDirtyTextTracks = false;

			// TODO: replace this crude polling check with events, or only do it once metadataReady
			// Need to add event support as tracks can be added via HTML (especially text)
			int videoTrackCount = AVPPlayerGetVideoTrackCount(_playerIndex);
			int audioTrackCount = AVPPlayerGetAudioTrackCount(_playerIndex);
			int textTrackCount = AVPPlayerGetTextTrackCount(_playerIndex);

			_isDirtyVideoTracks = (_cachedVideoTrackCount != videoTrackCount);
			_isDirtyAudioTracks = (_cachedAudioTrackCount != audioTrackCount);
			_isDirtyTextTracks = ( _cachedTextTrackCount != textTrackCount);

			_cachedVideoTrackCount = videoTrackCount;
			_cachedAudioTrackCount = audioTrackCount;
			_cachedTextTrackCount = textTrackCount;
		}

		private void UpdateTimeRanges()
		{
			{
				int rangeCount = AVPPlayerGetNumBufferedTimeRanges(_playerIndex);
				if (rangeCount != _bufferedTimes.Count)
				{
					_bufferedTimes._ranges = new TimeRange[rangeCount];
				}
				for (int i = 0; i < rangeCount; i++)
				{
					double startTime = AVPPlayerGetTimeRangeStart(_playerIndex, i);
					double endTime = AVPPlayerGetTimeRangeEnd(_playerIndex, i);
					_bufferedTimes._ranges[i] = new TimeRange(startTime, endTime - startTime);
				}
				_bufferedTimes.CalculateRange();
			}

			{
				double duration = GetDuration();
				if (duration > 0.0)
				{
					_seekableTimes._ranges = new TimeRange[1];
					_seekableTimes._ranges[0] = new TimeRange(0.0, duration);
				}
				else
				{
					_seekableTimes._ranges = new TimeRange[0];
				}
				_seekableTimes.CalculateRange();
			}
		}

		public override void Dispose()
		{
			CloseMedia();
		}

		public override bool IsPlaybackStalled()
		{
			bool result = false;
			if (_playerIndex > -1)
			{
				result = AVPPlayerIsPlaybackStalled(_playerIndex) && IsPlaying();
			}
			return result;
		}

		// Tracks
		internal override int InternalGetTrackCount(TrackType trackType)
		{ 
			int result = 0;
			switch (trackType)
			{
				case TrackType.Video:
					result = AVPPlayerGetVideoTrackCount(_playerIndex);
					break;
				case TrackType.Audio:
					result = AVPPlayerGetAudioTrackCount(_playerIndex);
					break;
				case TrackType.Text:
					result = AVPPlayerGetTextTrackCount(_playerIndex);
					break;
			}
			return result;
		}

		internal override bool InternalIsChangedTracks(TrackType trackType)
		{ 
			bool result = false;
			switch (trackType)
			{
				case TrackType.Video:
					result = _isDirtyVideoTracks;
					break;
				case TrackType.Audio:
					result = _isDirtyAudioTracks;
					break;
				case TrackType.Text:
					result = _isDirtyTextTracks;
					break;
			}
			return result;
		}

		internal override bool InternalSetActiveTrack(TrackType trackType, int trackId)
		{
			bool result = false;
			switch (trackType)
			{
				case TrackType.Video:
					result = AVPPlayerSetActiveVideoTrack(_playerIndex, trackId);
					break;
				case TrackType.Audio:
					result = AVPPlayerSetActiveAudioTrack(_playerIndex, trackId);
					break;
				case TrackType.Text:
					result = AVPPlayerSetActiveTextTrack(_playerIndex, trackId);
					break;
			}
			return result;
		}

		internal override TrackBase InternalGetTrackInfo(TrackType trackType, int trackIndex, ref bool isActiveTrack)
		{
			TrackBase result = null;

			switch (trackType)
			{
				case TrackType.Video:
				{
					string trackName = AVPPlayerGetVideoTrackName(_playerIndex, trackIndex);
					string trackLanguage = AVPPlayerGetVideoTrackLanguage(_playerIndex, trackIndex);
					bool isActive = AVPPlayerIsVideoTrackActive(_playerIndex, trackIndex);
					result = new VideoTrack(trackIndex, trackName, trackLanguage, isActive);
					break;
				}
				case TrackType.Audio:
				{
					string trackName = AVPPlayerGetAudioTrackName(_playerIndex, trackIndex);
					string trackLanguage = AVPPlayerGetAudioTrackLanguage(_playerIndex, trackIndex);
					bool isActive = AVPPlayerIsAudioTrackActive(_playerIndex, trackIndex);
					result = new AudioTrack(trackIndex, trackName, trackLanguage, isActive);
					break;
				}
				case TrackType.Text:
				{
					string trackName = AVPPlayerGetTextTrackName(_playerIndex, trackIndex);
					string trackLanguage = AVPPlayerGetTextTrackLanguage(_playerIndex, trackIndex);
					bool isActive = AVPPlayerIsTextTrackActive(_playerIndex, trackIndex);
					result = new TextTrack(trackIndex, trackName, trackLanguage, isActive);
					break;
				}
			}
			return result;
		}

		// Text Cue stub methods
		internal override bool InternalIsChangedTextCue() { return false; }
		internal override string InternalGetCurrentTextCue() { return string.Empty; }
	}
}
#endif