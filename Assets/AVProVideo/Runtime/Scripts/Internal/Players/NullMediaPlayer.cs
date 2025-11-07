using System;
using System.Text;
using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// This media player fakes video playback for platforms that aren't supported
	/// </summary>
	public sealed partial class NullMediaPlayer : BaseMediaPlayer
	{
		private bool		_isPlaying = false;
		private bool		_isPaused = false;
		private double		_currentTime = 0.0;
//		private bool		_audioMuted = false;
		private float		_volume = 0.0f;
		private float		_playbackRate = 1.0f;
		private bool		_bLoop;

		private int			_Width = 256;
		private int			_height = 256;
		private Texture2D	_texture;
		private Texture2D	_texture_AVPro;
		private Texture2D	_texture_AVPro1;
		private float		_fakeFlipTime;
		private int			_frameCount;

		private const float FrameRate = 10f;

		/// <inheritdoc/>
		public override string GetVersion()
		{
			return "0.0.0";
		}

		/// <inheritdoc/>
		public override string GetExpectedVersion()
		{
			return GetVersion();
		}

		/// <inheritdoc/>
		public override bool OpenMedia(string path, long offset, string httpHeader, MediaHints mediaHints, int forceFileFormat = 0, bool startWithHighestBitrate = false)
		{
			_texture_AVPro = (Texture2D)Resources.Load("Textures/AVProVideo-NullPlayer-Frame0");
			_texture_AVPro1 = (Texture2D)Resources.Load("Textures/AVProVideo-NullPlayer-Frame1");

			if( _texture_AVPro )
			{
				_Width = _texture_AVPro.width;
				_height = _texture_AVPro.height;
			}

			_texture = _texture_AVPro;

			_fakeFlipTime = 0.0f;
			_frameCount = 0;

			return true;
		}

		/// <inheritdoc/>
        public override void CloseMedia()
        {
			_frameCount = 0;
			Resources.UnloadAsset(_texture_AVPro);
			Resources.UnloadAsset(_texture_AVPro1);

			base.CloseMedia();
        }

		/// <inheritdoc/>
        public override void SetLooping( bool bLooping )
		{
			_bLoop = bLooping;
		}

		/// <inheritdoc/>
		public override bool IsLooping()
		{
			return _bLoop;
		}

		/// <inheritdoc/>
		public override bool HasMetaData()
		{
			return true;
		}

		/// <inheritdoc/>
		public override bool CanPlay()
		{
			return true;
		}

		/// <inheritdoc/>
		public override bool HasAudio()
		{
			return false;
		}

		/// <inheritdoc/>
		public override bool HasVideo()
		{
			return false;
		}

		/// <inheritdoc/>
		public override void Play()
		{
			_isPlaying = true;
			_isPaused = false;
			_fakeFlipTime = 0.0f;
		}

		/// <inheritdoc/>
		public override void Pause()
		{
			_isPlaying = false;
			_isPaused = true;
		}

		/// <inheritdoc/>
		public override void Stop()
		{
			_isPlaying = false;
			_isPaused = false;
		}

		/// <inheritdoc/>
		public override bool IsSeeking()
		{
			return false;
		}
		/// <inheritdoc/>
		public override bool IsPlaying()
		{
			return _isPlaying;
		}
		/// <inheritdoc/>
		public override bool IsPaused()
		{
			return _isPaused;
		}
		/// <inheritdoc/>
		public override bool IsFinished()
		{
			return _isPlaying && (_currentTime >= GetDuration());
		}

		/// <inheritdoc/>
		public override bool IsBuffering()
		{
			return false;
		}

		/// <inheritdoc/>
		public override double GetDuration()
		{
			return 10.0;
		}

		/// <inheritdoc/>
		public override int GetVideoWidth()
		{
			return _Width;
		}

		/// <inheritdoc/>
		public override int GetVideoHeight()
		{
			return _height;
		}

		/// <inheritdoc/>
		public override float GetVideoDisplayRate()
		{
			return FrameRate;
		}

		/// <inheritdoc/>
		public override Texture GetTexture( int index )
		{
//			return _texture ? _texture : Texture2D.whiteTexture;
			return _texture;
		}

		/// <inheritdoc/>
		public override int GetTextureFrameCount()
		{
			return _frameCount;
		}

		internal override StereoPacking InternalGetTextureStereoPacking()
		{
			return StereoPacking.Unknown;
		}		

		/// <inheritdoc/>
		public override bool RequiresVerticalFlip()
		{
			return false;
		}

		/// <inheritdoc/>
		public override void Seek(double time)
		{
			_currentTime = time;
		}

		/// <inheritdoc/>
		public override void SeekFast(double time)
		{
			_currentTime = time;
		}

		/// <inheritdoc/>
		public override double GetCurrentTime()
		{
			return _currentTime;
		}

		/// <inheritdoc/>
		public override void SetPlaybackRate(float rate)
		{
			_playbackRate = rate;
		}

		/// <inheritdoc/>
		public override float GetPlaybackRate()
		{
			return _playbackRate;
		}

		/// <inheritdoc/>
		public override void MuteAudio(bool bMuted)
		{
//			_audioMuted = bMuted;
		}

		/// <inheritdoc/>
		public override bool IsMuted()
		{
			return true;
		}

		/// <inheritdoc/>
		public override void SetVolume(float volume)
		{
			_volume = volume;
		}

		/// <inheritdoc/>
		public override float GetVolume()
		{
			return _volume;
		}

		/// <inheritdoc/>
		public override float GetVideoFrameRate()
		{
			return 0.0f;
		}

		/// <inheritdoc/>
		public override void Update()
		{
			UpdateSubtitles();

			if (_isPlaying)
			{
				_currentTime += Time.deltaTime;
				if (_currentTime >= GetDuration())
				{
					_currentTime = GetDuration();
					if( _bLoop )
					{
						Rewind();
					}
				}

				//

				_fakeFlipTime += Time.deltaTime;
				if( _fakeFlipTime >= (1.0 / FrameRate))
				{
					_fakeFlipTime = 0.0f;
					_texture = ( _texture == _texture_AVPro ) ? _texture_AVPro1 : _texture_AVPro;
					_frameCount++;
				}
			}
		}

		/// <inheritdoc/>
		public override void Render()
		{
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
		}
	}

	public sealed partial class NullMediaPlayer : BaseMediaPlayer
	{
		internal override bool InternalSetActiveTrack(TrackType trackType, int trackUid)
		{
			// Set the active text track using the unique identifier
			// Or disable all text tracks if < 0
			return false;
		}

		internal override bool InternalIsChangedTracks(TrackType trackType)
		{
			// Has the tracks changed since the last frame 'tick'
			return false;
		}

		internal override int InternalGetTrackCount(TrackType trackType)
		{
			// Return number of text tracks
			return 0;
		}

		internal override TrackBase InternalGetTrackInfo(TrackType trackType, int index, ref bool isActiveTrack)
		{
			// Get information about the specific track at index, range is [0...InternalGetTextTrackCount)
			return null;
		}

		internal override bool InternalIsChangedTextCue()
		{
			// Has the text cue changed since the last frame 'tick'
			return false;
		}

		internal override string InternalGetCurrentTextCue()
		{
			return null;
		}
	}
}