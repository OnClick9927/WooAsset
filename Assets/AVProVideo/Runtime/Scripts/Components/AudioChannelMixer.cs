using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2019-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// Allows per-channel volume control
	/// Currently supported on Windows and UWP (Media Foundation API only), macOS, iOS, tvOS and Android (ExoPlayer API only)
	[AddComponentMenu("AVPro Video/Audio Channel Mixer", 401)]
	[HelpURL("http://renderheads.com/products/avpro-video/")]
	public class AudioChannelMixer : MonoBehaviour
	{
		const int MaxChannels = 8;

		[Range(0f, 1f)]
		[SerializeField] float[] _channels = null;

		/// Range 0.0 to 1.0
		public float[] Channel
		{
			get { return _channels; }
			set { _channels = value; }
		}

		void Reset()
		{
			_channels = new float[MaxChannels];
			for (int i = 0; i < MaxChannels; i++)
			{
				_channels[i] = 1f;
			}
		}

		void ChangeChannelCount(int numChannels)
		{
			float[] channels = new float[numChannels];
			if (_channels != null && _channels.Length != 0)
			{
				for (int i = 0; i < channels.Length; i++)
				{
					if (i < _channels.Length)
					{
						channels[i] = _channels[i];
					}
					else
					{
						channels[i] = 1f;
					}
				}
			}
			else
			{
				for (int i = 0; i < numChannels; i++)
				{
					channels[i] = 1f;
				}
			}
			_channels = channels;
		}

		void OnAudioFilterRead(float[] data, int channels)
		{
			if (channels != _channels.Length)
			{
				ChangeChannelCount(channels);
			}
			int k = 0;
			int numSamples = data.Length / channels;
			for (int j = 0; j < numSamples; j++)
			{
				for (int i = 0; i < channels; i++)
				{
					data[k] *= _channels[i];
					k++;
				}
			}
		}
	}
}