using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	public partial class MediaPlayer : MonoBehaviour
	{
#region Audio Mute Support for Unity Editor
#if UNITY_EDITOR
		private bool _unityAudioMasterMute = false;
		private void CheckEditorAudioMute()
		{
			// Detect a change
			if (UnityEditor.EditorUtility.audioMasterMute != _unityAudioMasterMute)
			{
				_unityAudioMasterMute = UnityEditor.EditorUtility.audioMasterMute;
				if (_controlInterface != null)
				{
					_controlInterface.MuteAudio(_audioMuted || _unityAudioMasterMute);
				}
			}
		}
#endif
#endregion // Audio Mute Support for Unity Editor
	}
}