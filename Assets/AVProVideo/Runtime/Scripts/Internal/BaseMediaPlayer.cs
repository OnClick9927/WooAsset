#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_IOS || UNITY_ANDROID
	#define UNITY_PLATFORM_SUPPORTS_LINEAR
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Base class for all platform specific MediaPlayers
	/// </summary>
	public abstract partial class BaseMediaPlayer : IMediaPlayer, IMediaControl, IMediaInfo, IMediaCache, ITextureProducer, IMediaSubtitles, IVideoTracks, IAudioTracks, ITextTracks, IBufferedDisplay, System.IDisposable
	{
		public BaseMediaPlayer()
		{
			InitTracks();
		}

		public abstract string		GetVersion();
		public abstract string		GetExpectedVersion();

		/// <inheritdoc/>
		public abstract bool		OpenMedia(string path, long offset, string customHttpHeaders, MediaHints mediaHints, int forceFileFormat = 0, bool startWithHighestBitrate = false);

#if NETFX_CORE
		/// <inheritdoc/>
		public virtual bool			OpenMedia(Windows.Storage.Streams.IRandomAccessStream ras, string path, long offset, string customHttpHeaders) { return false; }
#endif

		/// <inheritdoc/>
		public virtual bool			OpenMediaFromBuffer(byte[] buffer) { return false; }
		/// <inheritdoc/>
		public virtual bool			StartOpenMediaFromBuffer(ulong length) { return false; }
		/// <inheritdoc/>
		public virtual bool			AddChunkToMediaBuffer(byte[] chunk, ulong offset, ulong length) { return false; }
		/// <inheritdoc/>
		public virtual bool			EndOpenMediaFromBuffer() { return false; }

		/// <inheritdoc/>
		public virtual void			CloseMedia()
		{
			#if UNITY_EDITOR
			_displayRateLastRealTime = 0f;
			#endif
			_displayRateTimer = 0f;
			_displayRateLastFrameCount = 0;
			_displayRate = 0f;

			_stallDetectionTimer = 0f;
			_stallDetectionFrame = 0;
			_lastError = ErrorCode.None;

			_textTracks.Clear();
			_audioTracks.Clear();
			_videoTracks.Clear();
			_currentTextCue = null;
			_mediaHints = new MediaHints();
		}

		/// <inheritdoc/>
		public abstract void		SetLooping(bool looping);
		/// <inheritdoc/>
		public abstract bool		IsLooping();

		/// <inheritdoc/>
		public abstract bool		HasMetaData();
		/// <inheritdoc/>
		public abstract bool		CanPlay();
		/// <inheritdoc/>
		public abstract void		Play();
		/// <inheritdoc/>
		public abstract void		Pause();
		/// <inheritdoc/>
		public abstract void		Stop();
		/// <inheritdoc/>
		public virtual void			Rewind() { SeekFast(0.0);  }

		/// <inheritdoc/>
		public abstract void		Seek(double time);
		/// <inheritdoc/>
		public abstract void		SeekFast(double time);
		/// <inheritdoc/>
		public virtual void			SeekWithTolerance(double time, double timeDeltaBefore, double timeDeltaAfter) { Seek(time); }
		/// <inheritdoc/>
		public abstract double		GetCurrentTime();
		/// <inheritdoc/>
		public virtual DateTime		GetProgramDateTime() { return DateTime.MinValue; }
		/// <inheritdoc/>
		public abstract float		GetPlaybackRate();
		/// <inheritdoc/>
		public abstract void		SetPlaybackRate(float rate);

		// Basic Properties
		/// <inheritdoc/>
		public abstract double		GetDuration();
		/// <inheritdoc/>
		public abstract int			GetVideoWidth();
		/// <inheritdoc/>
		public abstract int			GetVideoHeight();
		/// <inheritdoc/>
		public abstract float		GetVideoFrameRate();
		/// <inheritdoc/>
		public virtual float		GetVideoDisplayRate() { return _displayRate; }
		/// <inheritdoc/>
		public abstract bool		HasAudio();
		/// <inheritdoc/>
		public abstract bool		HasVideo();
		/// <inheritdoc/>
		public bool 				IsVideoStereo() { return GetTextureStereoPacking() != StereoPacking.None; }

		// Basic State
		/// <inheritdoc/>
		public abstract bool		IsSeeking();
		/// <inheritdoc/>
		public abstract bool		IsPlaying();
		/// <inheritdoc/>
		public abstract bool		IsPaused();
		/// <inheritdoc/>
		public abstract bool		IsFinished();
		/// <inheritdoc/>
		public abstract bool		IsBuffering();
		/// <inheritdoc/>
		public virtual bool			WaitForNextFrame(Camera dummyCamera, int previousFrameCount) { return false; }

		// Textures
		/// <inheritdoc/>
		public virtual int			GetTextureCount() { return 1; }
		/// <inheritdoc/>
		public abstract Texture		GetTexture(int index = 0);
		/// <inheritdoc/>
		public abstract int			GetTextureFrameCount();
		/// <inheritdoc/>
		public virtual bool			SupportsTextureFrameCount() { return true; }
		/// <inheritdoc/>
		public virtual long			GetTextureTimeStamp() { return long.MinValue; }
		/// <inheritdoc/>
		public abstract bool		RequiresVerticalFlip();
		/// <inheritdoc/>
		public virtual float[]		GetTextureTransform() { return new float[] { 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f }; }
		/// <inheritdoc/>
		public virtual Matrix4x4	GetYpCbCrTransform() { return Matrix4x4.identity; }

		public StereoPacking GetTextureStereoPacking()
		{
			StereoPacking result = InternalGetTextureStereoPacking();
			if (result == StereoPacking.Unknown)
			{
				// If stereo is unknown, fall back to media hints or no packing
				result = _mediaHints.stereoPacking;
			}
			return result;
		}
		internal abstract StereoPacking InternalGetTextureStereoPacking();

		public virtual TransparencyMode GetTextureTransparency()
		{
			return _mediaHints.transparency;
		}

		public AlphaPacking GetTextureAlphaPacking()
		{
			if (GetTextureTransparency() == TransparencyMode.Transparent)
			{
				return _mediaHints.alphaPacking;
			}
			return AlphaPacking.None;
		}

		// Audio General
		/// <inheritdoc/>
		public abstract void		MuteAudio(bool bMuted);
		/// <inheritdoc/>
		public abstract bool		IsMuted();
		/// <inheritdoc/>
		public abstract void		SetVolume(float volume);
		/// <inheritdoc/>
		public virtual void			SetBalance(float balance) { }
		/// <inheritdoc/>
		public abstract float		GetVolume();
		/// <inheritdoc/>
		public virtual float		GetBalance() { return 0f; }

		// Audio Grabbing
		/// <inheritdoc/>
		public virtual int						GetAudioChannelCount() { return -1; }
		/// <inheritdoc/>
		public virtual AudioChannelMaskFlags	GetAudioChannelMask() { return 0; }
		/// <inheritdoc/>
		public virtual int	 					GrabAudio(float[] audioData, int audioDataFloatCount, int channelCount) { return 0; }
		/// <inheritdoc/>
		public virtual int	 					GetAudioBufferedSampleCount() { return 0; }

		// 360 Audio
		/// <inheritdoc/>
		public virtual void			SetAudioHeadRotation(Quaternion q) { }
		/// <inheritdoc/>
		public virtual void			ResetAudioHeadRotation() { }
		/// <inheritdoc/>
		public virtual void			SetAudioChannelMode(Audio360ChannelMode channelMode) { }
		/// <inheritdoc/>
		public virtual void			SetAudioFocusEnabled(bool enabled) { }
		/// <inheritdoc/>
		public virtual void			SetAudioFocusProperties(float offFocusLevel, float widthDegrees) { }
		/// <inheritdoc/>
		public virtual void			SetAudioFocusRotation(Quaternion q) { }
		/// <inheritdoc/>
		public virtual void			ResetAudioFocus() { }

		// Streaming
		/// <inheritdoc/>
		public virtual long			GetEstimatedTotalBandwidthUsed() { return -1; }
		/// <inheritdoc/>
		public virtual void			SetPlayWithoutBuffering(bool playWithoutBuffering) { }

		// Caching
		/// <inheritdoc/>
		public virtual bool					IsMediaCachingSupported() { return false; }
		/// <inheritdoc/>
		public virtual void					AddMediaToCache(string url, string headers, MediaCachingOptions options) { }
		/// <inheritdoc/>
		public virtual void					CancelDownloadOfMediaToCache(string url) { }
		/// <inheritdoc/>
		public virtual void					RemoveMediaFromCache(string url) { }
		/// <inheritdoc/>
		public virtual CachedMediaStatus	GetCachedMediaStatus(string url, ref float progress) { return CachedMediaStatus.NotCached; }
		/// <inheritdoc/>
		public virtual bool					IsMediaCached() { return false; }

		// External playback
		/// <inheritdoc/>
		public virtual bool			IsExternalPlaybackSupported() { return false; }
		/// <inheritdoc/>
		public virtual bool			IsExternalPlaybackActive() { return false; }
		/// <inheritdoc/>
		public virtual void			SetAllowsExternalPlayback(bool enable) { }
		/// <inheritdoc/>
		public virtual void			SetExternalPlaybackVideoGravity(ExternalPlaybackVideoGravity gravity) { }

		// Authentication
		//public virtual void			SetKeyServerURL(string url) { }
		/// <inheritdoc/>
		public virtual void			SetKeyServerAuthToken(string token) { }
		/// <inheritdoc/>
		public virtual void			SetOverrideDecryptionKey(byte[] key) { }

		// General
		/// <inheritdoc/>
		public abstract void		Update();
		/// <inheritdoc/>
		public abstract void		Render();
		/// <inheritdoc/>
		public abstract void		Dispose();

		// Internal method
		public virtual bool GetDecoderPerformance(ref int activeDecodeThreadCount, ref int decodedFrameCount, ref int droppedFrameCount) { return false; }

#if false
		public void Update()
		{
			Native.Update(_instance);
			if (UpdateTracks())
			{

			}
			if (UpdateTextCue())
			{

			}
		}
#endif

		public virtual void EndUpdate() { }

		public virtual IntPtr GetNativePlayerHandle() { return IntPtr.Zero; }

		public ErrorCode GetLastError()
		{
			ErrorCode errorCode = _lastError;
			_lastError = ErrorCode.None;
			return errorCode;
		}

		/// <inheritdoc/>
		public virtual long GetLastExtendedErrorCode()
		{
			return 0;
		}

		public string GetPlayerDescription()
		{
			return _playerDescription;
		}

		/// <inheritdoc/>
		public virtual bool PlayerSupportsLinearColorSpace()
		{
#if UNITY_PLATFORM_SUPPORTS_LINEAR
			return true;
#else
			return false;
#endif
		}

		protected string _playerDescription = string.Empty;
		protected ErrorCode _lastError = ErrorCode.None;
		protected FilterMode _defaultTextureFilterMode = FilterMode.Bilinear;
		protected TextureWrapMode _defaultTextureWrapMode = TextureWrapMode.Clamp;
		protected int _defaultTextureAnisoLevel = 1;
		protected MediaHints _mediaHints;
		protected TimeRanges _seekableTimes = new TimeRanges();
		protected TimeRanges _bufferedTimes = new TimeRanges();

		public TimeRanges GetSeekableTimes() { return _seekableTimes; }
		public TimeRanges GetBufferedTimes() { return _bufferedTimes; }

		public void SetTextureProperties(FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp, int anisoLevel = 0)
		{
			_defaultTextureFilterMode = filterMode;
			_defaultTextureWrapMode = wrapMode;
			_defaultTextureAnisoLevel = anisoLevel;
			for (int i = 0; i < GetTextureCount(); ++i)
			{
				ApplyTextureProperties(GetTexture(i));
			}
		}

		protected virtual void ApplyTextureProperties(Texture texture)
		{
			if (texture != null)
			{
				texture.filterMode = _defaultTextureFilterMode;
				texture.wrapMode = _defaultTextureWrapMode;
				texture.anisoLevel = _defaultTextureAnisoLevel;
			}
		}

#region Video Display Rate
#if UNITY_EDITOR
		private float 		_displayRateLastRealTime = 0f;
#endif
		private float		_displayRateTimer;
		private int			_displayRateLastFrameCount;
		private float		_displayRate = 1f;

		protected void UpdateDisplayFrameRate()
		{
			const float IntervalSeconds = 0.5f;
			if (_displayRateTimer >= IntervalSeconds)
			{
				int frameCount = GetTextureFrameCount();
				int frameDelta = (frameCount - _displayRateLastFrameCount);
				_displayRate = (float)frameDelta / _displayRateTimer;
				_displayRateTimer -= IntervalSeconds;
				if (_displayRateTimer >= IntervalSeconds) _displayRateTimer -= IntervalSeconds;
				if (_displayRateTimer >= IntervalSeconds) _displayRateTimer = 0f;
				_displayRateLastFrameCount = frameCount;
			}

			float deltaTime = Time.deltaTime;
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				// When not playing Time.deltaTime isn't valid so we have to derive it
				deltaTime = (Time.realtimeSinceStartup - _displayRateLastRealTime);
				_displayRateLastRealTime = Time.realtimeSinceStartup;
			}
#endif
			_displayRateTimer += deltaTime;
		}
#endregion	// Video Display Rate

#region Stall Detection
		protected bool IsExpectingNewVideoFrame()
		{
			if (HasVideo())
			{
				// If we're playing then we expect a new frame
				if (!IsFinished() && (!IsPaused() && IsPlaying() && GetPlaybackRate() != 0.0f))
				{
					// Check that the video is not a single frame and therefore there is no other frame to display
					bool isSingleFrame = (GetTextureFrameCount() > 0 && GetDurationFrames() == 1);
					if (!isSingleFrame)
					{
						// NOTE: if a new frame isn't available then we could either be seeking or stalled
						return true;
					}
				}
			}
			return false;
		}

		/// <inheritdoc/>
		public virtual bool IsPlaybackStalled()
		{
			const float StallDetectionDuration = 0.5f;

			// Manually detect stalled video if the platform doesn't have native support to detect it
			if (SupportsTextureFrameCount() && IsExpectingNewVideoFrame())
			{
				// Detect a new video frame
				int frameCount = GetTextureFrameCount();
				if (frameCount != _stallDetectionFrame)
				{
					_stallDetectionTimer = 0f;
					_stallDetectionFrame = frameCount;
				}
				else
				{
					// Update the detection timer, but never more than once a Unity frame
					if (_stallDetectionGuard != Time.frameCount)
					{
						_stallDetectionTimer += Time.deltaTime;
					}
				}
				_stallDetectionGuard = Time.frameCount;

				float thresholdDuration = StallDetectionDuration;

				// Scale by the playback rate, but should be at least StallDetectionDuration
				thresholdDuration = Mathf.Max(thresholdDuration / Mathf.Abs(GetPlaybackRate()), StallDetectionDuration);

				// If a valid FPS is available then make sure the thresholdDuration
				// is at least double that.  This is mainly for very low FPS
				// content (eg 1 or 2 FPS)
				float fps = GetVideoFrameRate();
				if (fps > 0f && !float.IsNaN(fps))
				{
					thresholdDuration = Mathf.Max(thresholdDuration, 2f / fps);
				}

				return (_stallDetectionTimer > thresholdDuration);
			}
			else
			{
				_stallDetectionTimer = 0f;
			}
			return false;
		}

		private float _stallDetectionTimer;
		private int _stallDetectionFrame;
		private int _stallDetectionGuard;
#endregion // Stall Detection

		protected List<Subtitle> _subtitles;
		protected Subtitle _currentSubtitle;

		/// <inheritdoc/>
		public bool LoadSubtitlesSRT(string data)
		{
			if (string.IsNullOrEmpty(data))
			{
				// Disable subtitles
				_subtitles = null;
				_currentSubtitle = null;
			}
			else
			{
				_subtitles = SubtitleUtils.ParseSubtitlesSRT(data);
				_currentSubtitle = null;
			}
			return (_subtitles != null);
		}

		/// <inheritdoc/>
		public virtual void UpdateSubtitles()
		{
			if (_subtitles != null)
			{
				double time = GetCurrentTime();

				// TODO: implement a more efficient subtitle index searcher
				int searchIndex = 0;
				if (_currentSubtitle != null)
				{
					if (!_currentSubtitle.IsTime(time))
					{
						if (time > _currentSubtitle.timeEnd)
						{
							searchIndex = _currentSubtitle.index + 1;
						}
						_currentSubtitle = null;
					}
				}

				if (_currentSubtitle == null)
				{
					for (int i = searchIndex; i < _subtitles.Count; i++)
					{
						if (_subtitles[i].IsTime(time))
						{
							_currentSubtitle = _subtitles[i];
							break;
						}
					}
				}
			}
		}

		/// <inheritdoc/>
		public virtual int GetSubtitleIndex()
		{
			int result = -1;
			if (_currentSubtitle != null)
			{
				result = _currentSubtitle.index;
			}
			return result;
		}

		/// <inheritdoc/>
		public virtual string GetSubtitleText()
		{
			string result = string.Empty;
			if (_currentSubtitle != null)
			{
				result = _currentSubtitle.text;
			}
			else if (_currentTextCue != null)
			{
				result = _currentTextCue.Text;
			}
			return result;
		}

		public virtual void OnEnable()
		{
		}

		/// <inheritdoc/>
		public int GetCurrentTimeFrames(float overrideFrameRate = 0f)
		{
			int result = 0;
			float frameRate = (overrideFrameRate > 0f)?overrideFrameRate:GetVideoFrameRate();
			if (frameRate > 0f)
			{
				result = Helper.ConvertTimeSecondsToFrame(GetCurrentTime(), frameRate);
				result = Mathf.Min(result, GetMaxFrameNumber());
			}
			return result;
		}

		/// <inheritdoc/>
		public int GetDurationFrames(float overrideFrameRate = 0f)
		{
			int result = 0;
			float frameRate = (overrideFrameRate > 0f)?overrideFrameRate:GetVideoFrameRate();
			if (frameRate > 0f)
			{
				result = Helper.ConvertTimeSecondsToFrame(GetDuration(), frameRate);
			}
			return result;
		}

		/// <inheritdoc/>
		public int GetMaxFrameNumber(float overrideFrameRate = 0f)
		{
			int result = GetDurationFrames();
			result = Mathf.Max(0, result - 1);
			return result;
		}

		/// <inheritdoc/>
		public void SeekToFrameRelative(int frameOffset, float overrideFrameRate = 0f)
		{
			float frameRate = (overrideFrameRate > 0f)?overrideFrameRate:GetVideoFrameRate();
			if (frameRate > 0f)
			{
				int frame = Helper.ConvertTimeSecondsToFrame(GetCurrentTime(), frameRate);
				frame += frameOffset;
				frame = Mathf.Clamp(frame, 0, GetMaxFrameNumber(frameRate));
				double time = Helper.ConvertFrameToTimeSeconds(frame, frameRate);
				Seek(time);
			}
		}

		/// <inheritdoc/>
		public void SeekToFrame(int frame, float overrideFrameRate = 0f)
		{
			float frameRate = (overrideFrameRate > 0f)?overrideFrameRate:GetVideoFrameRate();
			if (frameRate > 0f)
			{
				frame = Mathf.Clamp(frame, 0, GetMaxFrameNumber(frameRate));
				double time = Helper.ConvertFrameToTimeSeconds(frame, frameRate);
				Seek(time);
			}
		}

		#region IBufferedDisplay Implementation

		private int _unityFrameCountBufferedDisplayGuard = -1;

		/// <inheritdoc/>
		public long UpdateBufferedDisplay()
		{
			// Guard to make sure we're only updating the buffered frame once per Unity frame
			if (Time.frameCount == _unityFrameCountBufferedDisplayGuard) return GetTextureTimeStamp();

			_unityFrameCountBufferedDisplayGuard = Time.frameCount;

			return InternalUpdateBufferedDisplay();
		}

		internal virtual long InternalUpdateBufferedDisplay() { return 0; }

		/// <inheritdoc/>
		public virtual BufferedFramesState GetBufferedFramesState()
		{
			return new BufferedFramesState();
		}

		/// <inheritdoc/>
		public virtual void SetSlaves(IBufferedDisplay[] slaves) { }

		/// <inheritdoc/>
		public virtual void SetBufferedDisplayMode(BufferedFrameSelectionMode mode, IBufferedDisplay master = null) { }

		/// <inheritdoc/>
		public virtual void SetBufferedDisplayOptions(bool pauseOnPrerollComplete) { }

		#endregion // IBufferedDisplay Implementation

		protected PlaybackQualityStats _playbackQualityStats = new PlaybackQualityStats();

		public PlaybackQualityStats GetPlaybackQualityStats()
		{
			return _playbackQualityStats;
		}
	}
}