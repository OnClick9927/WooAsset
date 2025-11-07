// You need to define AVPRO_PACKAGE_VFXGRAPH manually to use this script
// We could set up the asmdef to reference the package, but the package doesn't 
// existing in Unity 2017 etc, and it throws an error due to missing reference
//#define AVPRO_PACKAGE_VFXGRAPH
#if (UNITY_2018_3_OR_NEWER && AVPRO_PACKAGE_VFXGRAPH)
using UnityEngine;
using UnityEngine.VFX;
using RenderHeads.Media.AVProVideo;

//-----------------------------------------------------------------------------
// Copyright 2019-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Sets the texture from the MediaPlayer to a texture parameter in a VFX Graph
	/// </summary>
	[AddComponentMenu("AVPro Video/Apply To VFX Graph", 300)]
	[HelpURL("http://renderheads.com/products/avpro-video/")]
	public class ApplyToVfxGraph : MonoBehaviour
	{
		[Header("Media Source")]

		[SerializeField] MediaPlayer _mediaPlayer = null;

		[Tooltip("Default texture to display when the video texture is preparing")]
		[SerializeField] Texture2D _defaultTexture = null;

		[Space(8f)]
		[Header("VFX Target")]

		[SerializeField] VisualEffect _visualEffect = null;
		[SerializeField] string _texturePropertyName = string.Empty;

		public Texture2D DefaultTexture
		{
			get { return _defaultTexture; }
			set { if (_defaultTexture != value) { _defaultTexture = value; _isDirty = true; } }
		}

		public VisualEffect VisualEffect
		{
			get { return _visualEffect; }
			set { _visualEffect = value; _isDirty = true; }
		}

		public string TexturePropertyName
		{
			get { return _texturePropertyName; }
			set { _texturePropertyName = value; _textureProp = Shader.PropertyToID(_texturePropertyName); _isDirty = true; }
		}

		public MediaPlayer MediaPlayer
		{
			get { return _mediaPlayer; }
			set { ChangeMediaPlayer(value); }
		}

		private bool _isDirty = true;
		private int _textureProp = 0;

		void OnEnable()
		{
			// Force evaluations
			TexturePropertyName = _texturePropertyName;
			if (_mediaPlayer != null)
			{
				_mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
			}
		}

#if UNITY_EDITOR
		void OnValidate()
		{
			_isDirty = true;
		}
#endif

		void OnDisable()
		{
			if (_mediaPlayer != null)
			{
				_mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
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
				case MediaPlayerEvent.EventType.Closing:
					_isDirty = true;	// Allow the update to happen on the next frame, as the video is still closing
					break;
			}
		}

		private void ChangeMediaPlayer(MediaPlayer player)
		{
			if (_mediaPlayer != player)
			{
				if (_mediaPlayer != null)
				{
					_mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
				}
				_mediaPlayer = player;
				if (_mediaPlayer != null)
				{
					_mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
				}
				_isDirty = true;
			}
		}

		public void ForceUpdate()
		{
			_isDirty = true;
			LateUpdate();
		}

		void LateUpdate()
		{
			if (_isDirty)
			{
				ApplyMapping();
			}
		}

		private void ApplyMapping()
		{
			if (_visualEffect != null && !string.IsNullOrEmpty(_texturePropertyName))
			{
				if (_mediaPlayer != null && _mediaPlayer.TextureProducer != null && _mediaPlayer.TextureProducer.GetTexture() != null)
				{
					_visualEffect.SetTexture(_textureProp, _mediaPlayer.TextureProducer.GetTexture());
					_isDirty = false;
				}
				else if (_defaultTexture != null)
				{
					_visualEffect.SetTexture(_textureProp, _defaultTexture);
					_isDirty = false;
				}
			}
		}
	}
}
#endif