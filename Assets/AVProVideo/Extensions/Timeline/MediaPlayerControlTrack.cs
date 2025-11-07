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
	[TrackClipType(typeof(MediaPlayerControlAsset))]
	[TrackBindingType(typeof(MediaPlayer))]
	public class MediaPlayerControlTrack : TrackAsset
	{
		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) 
		{
			// before building, update the binding field in the clips assets;
			var director = go.GetComponent<PlayableDirector>();
			var binding = director.GetGenericBinding(this);
			
			foreach (var c in GetClips())
			{
				var myAsset = c.asset as MediaPlayerControlAsset;
				if (myAsset != null)
				{
					myAsset.binding = binding;
				}
			}

			//return base.CreateTrackMixer(graph, go, inputCount);
			return ScriptPlayable<MediaPlayerControlMixerBehaviour>.Create(graph, inputCount);
		}
	}
}
#endif