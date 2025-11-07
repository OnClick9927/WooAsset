// You need to define AVPRO_PACKAGE_TIMELINE manually to use this script
// We could set up the asmdef to reference the package, but the package doesn't 
// existing in Unity 2017 etc, and it throws an error due to missing reference
//#define AVPRO_PACKAGE_TIMELINE
#if (UNITY_2018_1_OR_NEWER && AVPRO_PACKAGE_TIMELINE)
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2020-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Playables
{
	public class MediaPlayerControlMixerBehaviour : PlayableBehaviour
	{
		public float audioVolume = 1f;
		public string videoPath = null;

		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			MediaPlayer mediaPlayer = playerData as MediaPlayer;
			float finalVolume = 0f;

			if (!mediaPlayer)
				return;

			int inputCount = playable.GetInputCount(); //get the number of all clips on this track
			for (int i = 0; i < inputCount; i++)
			{
				float inputWeight = playable.GetInputWeight(i);
				ScriptPlayable<MediaPlayerControlBehaviour> inputPlayable = (ScriptPlayable<MediaPlayerControlBehaviour>)playable.GetInput(i);
				MediaPlayerControlBehaviour input = inputPlayable.GetBehaviour();
				
				// Use the above variables to process each frame of this playable.
				finalVolume += input.audioVolume * inputWeight;
			}

			if (mediaPlayer != null)
			{
				mediaPlayer.AudioVolume = finalVolume;
				if (mediaPlayer.Control != null)
				{
					mediaPlayer.Control.SetVolume(finalVolume);
				}
			}
		}
	}
}
#endif