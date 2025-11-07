using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Audio is grabbed from the MediaPlayer and rendered via Unity AudioSource
	/// This allows audio to have 3D spatial control, effects applied and to be spatialised for VR
	/// Currently supported on Windows and UWP (Media Foundation API only), macOS, iOS, tvOS and Android (ExoPlayer API only)
	/// </summary>
	[RequireComponent(typeof(AudioSource))]
	[AddComponentMenu("AVPro Video/Audio Output", 400)]
	[HelpURL("http://renderheads.com/products/avpro-video/")]
	public class AudioOutput : MonoBehaviour
	{
		public enum AudioOutputMode
		{
			OneToAllChannels,
			MultipleChannels
		}

		[SerializeField] MediaPlayer _mediaPlayer = null;
		[SerializeField] AudioOutputMode _audioOutputMode = AudioOutputMode.MultipleChannels;
		[HideInInspector, SerializeField] int _channelMask = 0xffff;
		[SerializeField] bool _supportPositionalAudio = false;

		public MediaPlayer Player
		{
			get { return _mediaPlayer; }
			set { ChangeMediaPlayer(value); }
		}

		public AudioOutputMode OutputMode
		{
			get { return _audioOutputMode; }
			set { _audioOutputMode = value; }
		}

		public int ChannelMask
		{
			get { return _channelMask; }
			set { _channelMask = value; }
		}

		private AudioSource _audioSource;

		void Awake()
		{
			_audioSource = this.GetComponent<AudioSource>();
			Debug.Assert(_audioSource != null);
		}

		void Start()
		{
			ChangeMediaPlayer(_mediaPlayer);
		}

		void OnDestroy()
		{
			ChangeMediaPlayer(null);
		}

		void Update()
		{
			if (_mediaPlayer != null && _mediaPlayer.Control != null && _mediaPlayer.Control.IsPlaying())
			{
				ApplyAudioSettings(_mediaPlayer, _audioSource);
			}
		}

		public AudioSource GetAudioSource()
		{
			return _audioSource;
		}

		public void ChangeMediaPlayer(MediaPlayer newPlayer)
		{
			// When changing the media player, handle event subscriptions
			if (_mediaPlayer != null)
			{
				_mediaPlayer.AudioSource = null;
				_mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
				_mediaPlayer = null;
			}

			_mediaPlayer = newPlayer;
			if (_mediaPlayer != null)
			{
				_mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
				_mediaPlayer.AudioSource = _audioSource;
			}

			if (_supportPositionalAudio)
			{
				if (_audioSource.clip == null)
				{
					// Position audio is implemented from hints found on this thread:
					// https://forum.unity.com/threads/onaudiofilterread-sound-spatialisation.362782/
					int frameCount = 2048 * 10;
					int sampleCount = frameCount * Helper.GetUnityAudioSpeakerCount();
					AudioClip clip = AudioClip.Create("dummy", frameCount, Helper.GetUnityAudioSpeakerCount(), Helper.GetUnityAudioSampleRate(), false);
					float[] samples = new float[sampleCount];
					for (int i = 0; i < samples.Length; i++) { samples[i] = 1f; }
					clip.SetData(samples, 0);
					_audioSource.clip = clip;
					_audioSource.loop = true;
				}
			}
			else if (_audioSource.clip != null)
			{
				_audioSource.clip = null;
			}
		}

		// Callback function to handle events
		private void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			switch (et)
			{
				case MediaPlayerEvent.EventType.Closing:
					_audioSource.Stop();
					break;
				case MediaPlayerEvent.EventType.Started:
					ApplyAudioSettings(_mediaPlayer, _audioSource);
					_audioSource.Play();
					break;
			}
		}

		private static void ApplyAudioSettings(MediaPlayer player, AudioSource audioSource)
		{
			// Apply volume and mute from the MediaPlayer to the AudioSource
			if (audioSource != null && player != null && player.Control != null)
			{
				float volume = player.Control.GetVolume();
				bool isMuted = player.Control.IsMuted();
				float rate = player.Control.GetPlaybackRate();
				audioSource.volume = volume;
				audioSource.mute = isMuted;
				audioSource.pitch = rate;
			}
		}

#if (UNITY_EDITOR_WIN || UNITY_EDITOR_OSX) || (!UNITY_EDITOR && (UNITY_STANDALONE_WIN || UNITY_WSA_10_0 || UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_TVOS || UNITY_ANDROID))
		void OnAudioFilterRead(float[] audioData, int channelCount)
		{
			AudioOutputManager.Instance.RequestAudio(this, _mediaPlayer, audioData, channelCount, _channelMask, _audioOutputMode, _supportPositionalAudio);
		}
#endif
	}
}