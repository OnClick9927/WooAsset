using UnityEngine;

#if UNITY_EDITOR

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	public partial class MediaPlayer : MonoBehaviour
	{
#region Play/Pause Support for Unity Editor
		// This code handles the pause/play buttons in the editor
		private static void SetupEditorPlayPauseSupport()
		{
			#if UNITY_2017_2_OR_NEWER
			UnityEditor.EditorApplication.pauseStateChanged -= OnUnityPauseModeChanged;
			UnityEditor.EditorApplication.pauseStateChanged += OnUnityPauseModeChanged;
			#else
			UnityEditor.EditorApplication.playmodeStateChanged -= OnUnityPlayModeChanged;
			UnityEditor.EditorApplication.playmodeStateChanged += OnUnityPlayModeChanged;
			#endif
		}

		#if UNITY_2017_2_OR_NEWER
		private static void OnUnityPauseModeChanged(UnityEditor.PauseState state)
		{
			OnUnityPlayModeChanged();
		}
		#endif

		private static void OnUnityPlayModeChanged()
		{
			if (UnityEditor.EditorApplication.isPlaying)
			{
				bool isPaused = UnityEditor.EditorApplication.isPaused;
				MediaPlayer[] players = Resources.FindObjectsOfTypeAll<MediaPlayer>();
				foreach (MediaPlayer player in players)
				{
					if (isPaused)
					{
						player.EditorPause();
					}
					else
					{
						player.EditorUnpause();
					}
				}
			}
		}

		private void EditorPause()
		{
			if (this.isActiveAndEnabled)
			{
				if (_controlInterface != null && _controlInterface.IsPlaying())
				{
					_wasPlayingOnPause = true;
					_controlInterface.Pause();
				}
				StopRenderCoroutine();
			}
		}

		private void EditorUnpause()
		{
			if (this.isActiveAndEnabled)
			{
				if (_controlInterface != null && _wasPlayingOnPause)
				{
					_autoPlayOnStart = true;
					_wasPlayingOnPause = false;
					_autoPlayOnStartTriggered = false;
				}
				StartRenderCoroutine();
			}
		}
#endregion // Play/Pause Support for Unity Editor
	}
}

#endif