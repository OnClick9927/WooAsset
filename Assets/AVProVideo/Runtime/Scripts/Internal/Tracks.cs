using System.Collections;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	public enum TrackType
	{
		Video,
		Audio,
		Text,
	}

	public class TrackBase
	{
		protected TrackBase() { }

		internal TrackBase(TrackType trackType, int uid, string name, string language, bool isDefault)
		{
			TrackType = trackType;
			Uid = uid;
			Name = name;
			Language = language;
			IsDefault = isDefault;
			DisplayName = CreateDisplayName();
		}

		// The UID is unique to the media
		internal int Uid { get; private set; }

		public TrackType TrackType { get; private set; }

		public string DisplayName { get; private set; }

		// Optional
		public string Name { get; private set; }

		// Optional
		public string Language { get; private set; }

		// Optional
		public bool IsDefault { get; private set; }

		protected string CreateDisplayName()
		{
			string result;
			if (!string.IsNullOrEmpty(Name))
			{
				result = Name;
			}
			else
			{
				result = "Track " + Uid.ToString();
			}
			if (!string.IsNullOrEmpty(Language))
			{
				result = string.Format("{0} ({1})", result, Language);
			}
			return result;
		}
	}

	public abstract class TrackCollection : IEnumerable
	{
		public virtual TrackType TrackType { get; private set; }
		public abstract int Count { get; }
		public abstract IEnumerator GetEnumerator();

		internal abstract void Clear();
		internal abstract void Add(TrackBase track);
		internal abstract bool HasActiveTrack();
		internal abstract bool IsActiveTrack(TrackBase track);
		internal abstract void SetActiveTrack(TrackBase track);
		internal abstract void SetFirstTrackActive();
	}

	public class TrackCollection<T> : TrackCollection where T : TrackBase
	{
		internal TrackCollection() {}

		public override IEnumerator GetEnumerator()
		{
			return _tracks.GetEnumerator();
		}

		public T this[int index]
		{
			get
			{
				return _tracks[index];
			}
		}

		internal T ActiveTrack { get; set; }

		internal override bool HasActiveTrack() { return ActiveTrack != null; }

		internal override bool IsActiveTrack(TrackBase track)
		{
			return (ActiveTrack == track);
		}

		internal override void Clear()
		{
			_tracks.Clear();
			ActiveTrack = null;
		}

		internal override void Add(TrackBase track)
		{
			_tracks.Add(track as T);
		}

		internal override void SetActiveTrack(TrackBase track)
		{
			ActiveTrack = track as T;
		}

		internal override void SetFirstTrackActive()
		{
			if (_tracks.Count > 0)
			{
				ActiveTrack = _tracks[0];
			}
		}

		public override int Count { get{ return _tracks.Count; } }

		internal List<T> _tracks = new List<T>(4);
	}

	public class VideoTracks : TrackCollection<VideoTrack>
	{
		public override TrackType TrackType { get { return TrackType.Video; } }
	}

	public class AudioTracks : TrackCollection<AudioTrack>
	{
		public override TrackType TrackType { get { return TrackType.Audio; } }
	}

	public class TextTracks : TrackCollection<TextTrack>
	{
		public override TrackType TrackType { get { return TrackType.Text; } }
	}

	public class VideoTrack : TrackBase
	{
		private VideoTrack() { }

		internal VideoTrack(int uid, string name, string language, bool isDefault)
			: base(TrackType.Video, uid, name, language, isDefault) { }
	
		// Optional
		public int Bitrate { get; private set; }
	}

	public class AudioTrack : TrackBase
	{
		private AudioTrack() { }

		internal AudioTrack(int uid, string name, string language, bool isDefault)
			: base(TrackType.Audio, uid, name, language, isDefault) { }

		// Optional
		public int Bitrate { get; private set; }

		// Optional
		public int ChannelCount { get; private set; }
	}

	public class TextTrack : TrackBase
	{
		private TextTrack() { }

		internal TextTrack(int uid, string name, string language, bool isDefault)
			: base(TrackType.Text, uid, name, language, isDefault) { }
	}

	public interface IVideoTracks
	{
		VideoTracks			GetVideoTracks();
		VideoTrack			GetActiveVideoTrack();
		void 				SetActiveVideoTrack(VideoTrack track);
	}

	public interface IAudioTracks
	{
		AudioTracks			GetAudioTracks();
		AudioTrack			GetActiveAudioTrack();
		void 				SetActiveAudioTrack(AudioTrack track);
	}

	public interface ITextTracks
	{
		TextTracks			GetTextTracks();
		TextTrack			GetActiveTextTrack();
		void 				SetActiveTextTrack(TextTrack track);
		TextCue				GetCurrentTextCue();
	}

	public partial class BaseMediaPlayer : IVideoTracks, IAudioTracks, ITextTracks
	{
		protected VideoTracks _videoTracks = new VideoTracks();
		protected AudioTracks _audioTracks = new AudioTracks();
		protected TextTracks _textTracks = new TextTracks();
		protected TrackCollection[] _trackCollections;

		public VideoTracks			GetVideoTracks() { return _videoTracks; }
		public AudioTracks			GetAudioTracks() { return _audioTracks; }
		public TextTracks			GetTextTracks() { return _textTracks; }
		public VideoTrack			GetActiveVideoTrack() { return _videoTracks.ActiveTrack; }
		public AudioTrack			GetActiveAudioTrack() { return _audioTracks.ActiveTrack; }
		public TextTrack			GetActiveTextTrack() { return _textTracks.ActiveTrack; }
		public void 				SetActiveVideoTrack(VideoTrack track) { if (track != null) SetActiveTrack(_videoTracks, track); }
		public void 				SetActiveAudioTrack(AudioTrack track) { if (track != null) SetActiveTrack(_audioTracks, track); }
		public void 				SetActiveTextTrack(TextTrack track) { SetActiveTrack(_textTracks, track); }

		internal abstract bool 		InternalIsChangedTracks(TrackType trackType);
		internal abstract int 		InternalGetTrackCount(TrackType trackType);
		internal abstract bool 		InternalSetActiveTrack(TrackType trackType, int trackUid);
		internal abstract TrackBase InternalGetTrackInfo(TrackType trackType, int trackIndex, ref bool isActiveTrack);

		private void InitTracks()
		{
			_trackCollections = new TrackCollection[3] { _videoTracks, _audioTracks, _textTracks };
		}

		protected void UpdateTracks()
		{
			foreach (TrackCollection trackCollection in _trackCollections)
			{
				if (InternalIsChangedTracks(trackCollection.TrackType))
				{
					PopulateTrackCollection(trackCollection);
				}
			}
		}

		private void PopulateTrackCollection(TrackCollection collection)
		{
			collection.Clear();
			int trackCount = InternalGetTrackCount(collection.TrackType);
			for (int i = 0; i < trackCount; i++)
			{
				bool isActiveTrack = false;
				TrackBase track = InternalGetTrackInfo(collection.TrackType, i, ref isActiveTrack);
				if (track != null)
				{
					collection.Add(track);
					if (isActiveTrack)
					{
						collection.SetActiveTrack(track);
					}
				}
				else
				{
					UnityEngine.Debug.LogWarning(string.Format("[AVProVideo] Failed to enumerate {0} track {1} ", collection.TrackType, i));
				}
			}
		}

		private void SetActiveTrack(TrackCollection collection, TrackBase track)
		{
			// Check if this is already the active track
			if (collection.IsActiveTrack(track)) return;

			// Convert from TextTrack to uid
			int trackUid = -1;
			if (track != null)
			{
				trackUid = track.Uid;
			}

			// Set track based on uid (-1 is no active track)
			// NOTE: TrackType is pulled from collection as track may be null
			if (InternalSetActiveTrack(collection.TrackType, trackUid))
			{
				collection.SetActiveTrack(track);
				switch (collection.TrackType)
				{
					case TrackType.Text:
						UpdateTextCue(force:true);
						break;
				}
			}
		}
	}
}