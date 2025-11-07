#if !(UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_TVOS)
using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	public partial class MediaPlayer : MonoBehaviour
	{
#region Application Focus and Pausing
#if !UNITY_EDITOR
		void OnApplicationFocus(bool focusStatus)
		{
#if !(UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
//			Debug.Log("OnApplicationFocus: focusStatus: " + focusStatus);

			if (focusStatus)
			{
				if (Control != null && _wasPlayingOnPause)
				{
					_wasPlayingOnPause = false;
					Control.Play();

					Helper.LogInfo("OnApplicationFocus: playing video again");
				}
			}
#endif
		}

		void OnApplicationPause(bool pauseStatus)
		{
#if !(UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
//			Debug.Log("OnApplicationPause: pauseStatus: " + pauseStatus);

			if (pauseStatus)
			{
				if (_pauseMediaOnAppPause)
				{
					if (Control!= null && Control.IsPlaying())
					{
						_wasPlayingOnPause = true;
#if !UNITY_IPHONE
						Control.Pause();
#endif
						Helper.LogInfo("OnApplicationPause: pausing video");
					}
				}
			}
			else
			{
				if (_playMediaOnAppUnpause)
				{
					// Catch coming back from power off state when no lock screen
					OnApplicationFocus(true);
				}
			}
#endif
		}
#endif
#endregion // Application Focus and Pausing
	}
}
#endif
