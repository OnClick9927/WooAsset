using System.Collections.Generic;
using UnityEngine;
using System;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// A singleton to handle multiple instances of the AudioOutput component
	/// </summary>
	public class AudioOutputManager
	{
		private static AudioOutputManager _instance = null;

		public static AudioOutputManager Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new AudioOutputManager();
				}
				return _instance;
			}
		}

		protected class PlayerInstance
		{
			public HashSet<AudioOutput> outputs;
			public float[] pcmData;
			public bool isPcmDataReady;
		}

		private Dictionary<MediaPlayer, PlayerInstance> _instances;

		private AudioOutputManager()
		{
			_instances = new Dictionary<MediaPlayer, PlayerInstance>();
		}

		public void RequestAudio(AudioOutput outputComponent, MediaPlayer mediaPlayer, float[] audioData, int audioChannelCount, int channelMask, AudioOutput.AudioOutputMode audioOutputMode, bool supportPositionalAudio)
		{
			if (mediaPlayer == null || mediaPlayer.Control == null || !mediaPlayer.Control.IsPlaying())
			{
				if (supportPositionalAudio)
				{
					ZeroAudio(audioData, 0);
				}
				return;
			}

			int channels = mediaPlayer.Control.GetAudioChannelCount();
			if (channels <= 0)
			{
				if (supportPositionalAudio)
				{
					ZeroAudio(audioData, 0);
				}
				return;
			}

			// total samples requested should be multiple of channels
			Debug.Assert(audioData.Length % audioChannelCount == 0);

			// Find or create an instance
			PlayerInstance instance = null;
			if (!_instances.TryGetValue(mediaPlayer, out instance))
			{
				instance = _instances[mediaPlayer] = new PlayerInstance()
				{
					outputs = new HashSet<AudioOutput>(),
					pcmData = null
				};
			}

			// requests data if it hasn't been requested yet for the current cycle
			if (instance.outputs.Count == 0 || instance.outputs.Contains(outputComponent) || instance.pcmData == null)
			{
				instance.outputs.Clear();

				int actualDataRequired = (audioData.Length * channels) / audioChannelCount;
				if (instance.pcmData == null || actualDataRequired != instance.pcmData.Length)
				{
					instance.pcmData = new float[actualDataRequired];
				}

				instance.isPcmDataReady = GrabAudio(mediaPlayer, instance.pcmData, channels);

				instance.outputs.Add(outputComponent);
			}

			if (instance.isPcmDataReady)
			{
				// calculate how many samples and what channels are needed and then copy over the data
				int samples = Math.Min(audioData.Length / audioChannelCount, instance.pcmData.Length / channels);
				int storedPos = 0;
				int requestedPos = 0;

				// multiple mode, copies over audio from desired channels into the same channels on the audiosource
				if (audioOutputMode == AudioOutput.AudioOutputMode.MultipleChannels)
				{
					int lesserChannels = Math.Min(channels, audioChannelCount);

					if (!supportPositionalAudio)
					{
						for (int i = 0; i < samples; ++i)
						{
							for (int j = 0; j < lesserChannels; ++j)
							{
								if ((1 << j & channelMask) > 0)
								{
									audioData[requestedPos + j] = instance.pcmData[storedPos + j];
								}
							}

							storedPos += channels;
							requestedPos += audioChannelCount;
						}
					}
					else
					{
						for (int i = 0; i < samples; ++i)
						{
							for (int j = 0; j < lesserChannels; ++j)
							{
								if ((1 << j & channelMask) > 0)
								{
									audioData[requestedPos + j] *= instance.pcmData[storedPos + j];
								}
							}

							storedPos += channels;
							requestedPos += audioChannelCount;
						}
					}
				}
				//Mono mode, copies over single channel to all output channels
				else if (audioOutputMode == AudioOutput.AudioOutputMode.OneToAllChannels)
				{
					int desiredChannel = 0;

					for (int i = 0; i < 8; ++i)
					{
						if ((channelMask & (1 << i)) > 0)
						{
							desiredChannel = i;
							break;
						}
					}

					if (desiredChannel < channels)
					{
						if (!supportPositionalAudio)
						{
							for (int i = 0; i < samples; ++i)
							{
								for (int j = 0; j < audioChannelCount; ++j)
								{
									audioData[requestedPos + j] = instance.pcmData[storedPos + desiredChannel];
								}

								storedPos += channels;
								requestedPos += audioChannelCount;
							}
						}
						else
						{
							for (int i = 0; i < samples; ++i)
							{
								for (int j = 0; j < audioChannelCount; ++j)
								{
									audioData[requestedPos + j] *= instance.pcmData[storedPos + desiredChannel];
								}

								storedPos += channels;
								requestedPos += audioChannelCount;
							}
						}
					}
				}

				// If there is left over audio
				if (supportPositionalAudio && requestedPos != audioData.Length)
				{
					// Zero the remaining audio data otherwise there are pops
					ZeroAudio(audioData, requestedPos);
				}
			}
			else
			{
				if (supportPositionalAudio)
				{
					// Zero the remaining audio data otherwise there are pops
					ZeroAudio(audioData, 0);
				}
			}
		}

		private void ZeroAudio(float[] audioData, int startPosition)
		{
			for (int i = startPosition; i < audioData.Length; i++)
			{
				audioData[i] = 0f;
			}
		}

		private bool GrabAudio(MediaPlayer player, float[] audioData, int channelCount)
		{
			return (0 != player.Control.GrabAudio(audioData, audioData.Length, channelCount));
		}
	}
}