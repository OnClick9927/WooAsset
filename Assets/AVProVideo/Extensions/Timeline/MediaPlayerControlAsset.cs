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
	[System.Serializable]
	public class MediaPlayerControlAsset : PlayableAsset
	{
		public Object binding { get; set; }
		//public ExposedReference<MediaPlayer> mediaPlayer;

		public MediaReference mediaReference;

		[Range(0f, 1f)]
		public float audioVolume = 1f;
		public double startTime = -1.0;
		public bool pauseOnEnd = true;

		public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
		{
			var playable = ScriptPlayable<MediaPlayerControlBehaviour>.Create(graph);

			var behaviour = playable.GetBehaviour();
			//behaviour.mediaPlayer = mediaPlayer.Resolve(graph.GetResolver());
			behaviour.audioVolume = audioVolume;
			behaviour.pauseOnEnd = pauseOnEnd;
			behaviour.startTime = startTime;
			behaviour.mediaReference = mediaReference;
			behaviour.mediaPlayer = (MediaPlayer)binding;

			return playable;
		}
	}
}
#endif