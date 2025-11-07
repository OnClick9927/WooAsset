using UnityEngine.Events;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	[System.Serializable]
	public class MediaPlayerLoadEvent : UnityEvent<string> {}

	[System.Serializable]
	public class MediaPlayerEvent : UnityEvent<MediaPlayer, MediaPlayerEvent.EventType, ErrorCode>
	{
		public enum EventType
		{
			MetaDataReady,		// Triggered when meta data(width, duration etc) is available
			ReadyToPlay,		// Triggered when the video is loaded and ready to play
			Started,			// Triggered when the playback starts
			FirstFrameReady,	// Triggered when the first frame has been rendered
			FinishedPlaying,	// Triggered when a non-looping video has finished playing
			Closing,			// Triggered when the media is closed
			Error,				// Triggered when an error occurs
			SubtitleChange,		// Triggered when the subtitles change
			Stalled,			// Triggered when media is stalled (eg. when lost connection to media stream)
			Unstalled,			// Triggered when media is resumed form a stalled state (eg. when lost connection is re-established)
			ResolutionChanged,	// Triggered when the resolution of the video has changed (including the load) Useful for adaptive streams
			StartedSeeking,		// Triggered when seeking begins
			FinishedSeeking,    // Triggered when seeking has finished
			StartedBuffering,	// Triggered when buffering begins
			FinishedBuffering,	// Triggered when buffering has finished
			PropertiesChanged,	// Triggered when any properties (eg stereo packing are changed) - this has to be triggered manually
			PlaylistItemChanged,// Triggered when the new item is played in the playlist
			PlaylistFinished,	// Triggered when the playlist reaches the end

			TextTracksChanged,	// Triggered when the text tracks are added or removed
			TextCueChanged = SubtitleChange,	// Triggered when the text to display changes

			// TODO: 
			//StartLoop,		// Triggered when the video starts and is in loop mode
			//EndLoop,			// Triggered when the video ends and is in loop mode
			//NewFrame			// Trigger when a new video frame is available
		}

		private List<UnityAction<MediaPlayer, MediaPlayerEvent.EventType, ErrorCode>> _listeners = new List<UnityAction<MediaPlayer, EventType, ErrorCode>>(4);

		public bool HasListeners()
		{
			return (_listeners.Count > 0) || (GetPersistentEventCount() > 0);
		}

		new public void AddListener(UnityAction<MediaPlayer, MediaPlayerEvent.EventType, ErrorCode> call)
		{
			if (!_listeners.Contains(call))
			{
				_listeners.Add(call);
				base.AddListener(call);
			}
		}

		new public void RemoveListener(UnityAction<MediaPlayer, MediaPlayerEvent.EventType, ErrorCode> call)
		{
			int index = _listeners.IndexOf(call);
			if (index >= 0)
			{
				_listeners.RemoveAt(index);
				base.RemoveListener(call);
			}
		}

		new public void RemoveAllListeners()
		{
			_listeners.Clear();
			base.RemoveAllListeners();
		}
	}

#if false
	public interface IMediaEvents
	{
		void				AddEventListener(UnityAction<MediaPlayer, MediaPlayerEvent.EventType, ErrorCode> call);
		void				RemoveListener(UnityAction<MediaPlayer, MediaPlayerEvent.EventType, ErrorCode> call);
		void				RemoveAllEventListeners();
	}

	public partial class BaseMediaPlayer
	{
		void AddEventListener(UnityAction<MediaPlayer, MediaPlayerEvent.EventType, ErrorCode> call)
		{

		}
		void RemoveListener(UnityAction<MediaPlayer, MediaPlayerEvent.EventType, ErrorCode> call)
		{

		}
		void RemoveAllEventListeners()
		{

		}

		private MediaPlayerEvent _eventHandler;

	}
#endif
}
