#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_TVOS
	#define UNITY_PLATFORM_SUPPORTS_YPCBCR
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Base class to apply texture from MediaPlayer
	/// </summary>
	public abstract class ApplyToBase : MonoBehaviour
	{
		[Header("Media Source")]
		[Space(8f)]

		[SerializeField] protected MediaPlayer _media = null;

		public MediaPlayer Player
		{
			get { return _media; }
			set { ChangeMediaPlayer(value); }
		}

		[Space(8f)]
		[Header("Display")]

		[SerializeField] bool _automaticStereoPacking = true;
		public bool AutomaticStereoPacking
		{
			get { return _automaticStereoPacking; }
			set { if (_automaticStereoPacking != value) { _automaticStereoPacking = value; _isDirty = true; } }
		}

		[SerializeField] StereoPacking _overrideStereoPacking = StereoPacking.None;
		public StereoPacking OverrideStereoPacking
		{
			get { return _overrideStereoPacking; }
			set { if (_overrideStereoPacking != value) { _overrideStereoPacking = value; _isDirty = true; } }
		}

		[SerializeField] bool _stereoRedGreenTint = false;
		public bool StereoRedGreenTint { get { return _stereoRedGreenTint; } set { if (_stereoRedGreenTint != value) { _stereoRedGreenTint = value; _isDirty = true; } } }

		protected bool _isDirty = false;

		void Awake()
		{
			ChangeMediaPlayer(_media, force:true);
		}

		private void ChangeMediaPlayer(MediaPlayer player, bool force = false)
		{
			if (_media != player || force)
			{
				if (_media != null)
				{
					_media.Events.RemoveListener(OnMediaPlayerEvent);
				}
				_media = player;
				if (_media != null)
				{
					_media.Events.AddListener(OnMediaPlayerEvent);
				}
				_isDirty = true;
			}
		}

		// Callback function to handle events
		private void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			switch (et)
			{
				case MediaPlayerEvent.EventType.FirstFrameReady:
				case MediaPlayerEvent.EventType.PropertiesChanged:
					ForceUpdate();
					break;
			}
		}

		public void ForceUpdate()
		{
			_isDirty = true;
			if (this.isActiveAndEnabled)
			{
				Apply();
			}
		}

		private void Start()
		{
			SaveProperties();
			Apply();
		}

		protected virtual void OnEnable()
		{
			SaveProperties();
			ForceUpdate();
		}

		protected virtual void OnDisable()
		{
			RestoreProperties();
		}

		private void OnDestroy()
		{
			ChangeMediaPlayer(null);
		}

		protected virtual void SaveProperties()
		{
		}

		protected virtual void RestoreProperties()
		{
		}

		public abstract void Apply();
	}
}