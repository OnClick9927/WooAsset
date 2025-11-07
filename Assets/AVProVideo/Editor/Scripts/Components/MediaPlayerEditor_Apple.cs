using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// <summary>
	/// Editor for the MediaPlayer component
	/// </summary>
	public partial class MediaPlayerEditor : UnityEditor.Editor
	{
		private readonly static FieldDescription _optionAudioMode = new FieldDescription(".audioMode", new GUIContent("Audio Mode", "Unity mode does not work with HLS video"));
		private readonly static FieldDescription _optionTextureFormat = new FieldDescription(".textureFormat", new GUIContent("Texture Format", "BGRA32 is the most compatible.\nYCbCr420 uses ~50% of the memory of BGRA32 and has slightly better performance however it does require shader support, recommended for iOS and tvOS."));
		private readonly static FieldDescription _optionPreferredForwardBufferDuration = new FieldDescription("._preferredForwardBufferDuration", new GUIContent("Preferred Forward Buffer Duration", "The duration in seconds the player should buffer ahead of the playhead to prevent stalling. Set to 0 to let the system decide."));
		private readonly static FieldDescription _optionCustomPreferredPeakBitRateApple = new FieldDescription("._preferredPeakBitRate", new GUIContent("Preferred Peak BitRate", "The desired limit of network bandwidth consumption for playback, set to 0 for no preference."));
		private readonly static FieldDescription _optionCustomPreferredPeakBitRateUnitsApple = new FieldDescription("._preferredPeakBitRateUnits", new GUIContent());

		private void OnInspectorGUI_Override_Apple(Platform platform)
		{
			GUILayout.Space(8f);

			string optionsVarName = MediaPlayer.GetPlatformOptionsVariable(platform);

			EditorGUILayout.BeginVertical(GUI.skin.box);

			DisplayPlatformOption(optionsVarName, _optionTextureFormat);

			SerializedProperty flagsProp = serializedObject.FindProperty(optionsVarName + "._flags");
			MediaPlayer.OptionsApple.Flags flags = flagsProp != null ? (MediaPlayer.OptionsApple.Flags)flagsProp.intValue : 0;

			// Texture flags
			if (flagsProp != null)
			{
				bool generateMipmaps = flags.GenerateMipmaps();
				generateMipmaps = EditorGUILayout.Toggle(new GUIContent("Generate Mipmaps"), generateMipmaps);
				flags = flags.SetGenerateMipMaps(generateMipmaps);
			}

			// Audio
			DisplayPlatformOption(optionsVarName, _optionAudioMode);

			// Platform specific flags
			if (flagsProp != null)
			{
				if (platform == Platform.MacOSX || platform == Platform.iOS)
				{
					bool b = flags.AllowExternalPlayback();
					b = EditorGUILayout.Toggle(new GUIContent("Allow External Playback", "Enables support for playback on external devices via AirPlay."), b);
					flags = flags.SetAllowExternalPlayback(b);
				}

				if (platform == Platform.iOS)
				{
					bool b = flags.ResumePlaybackAfterAudioSessionRouteChange();
					b = EditorGUILayout.Toggle(new GUIContent("Resume playback after audio route change", "The default behaviour is for playback to pause when the audio route changes, for instance when disconnecting headphones."), b);
					flags = flags.SetResumePlaybackAfterAudioSessionRouteChange(b);
				}

				bool playWithoutBuffering = flags.PlayWithoutBuffering();
				playWithoutBuffering = EditorGUILayout.Toggle(new GUIContent("Play without buffering"), playWithoutBuffering);
				flags = flags.SetPlayWithoutBuffering(playWithoutBuffering);

				bool useSinglePlayerItem = flags.UseSinglePlayerItem();
				useSinglePlayerItem = EditorGUILayout.Toggle(new GUIContent("Use single player item", "Restricts the media player to using only one player item. This can help reduce network usage for remote videos but will cause a stall when looping."), useSinglePlayerItem);
				flags = flags.SetUseSinglePlayerItem(useSinglePlayerItem);
			}

			SerializedProperty maximumPlaybackRateProp = serializedObject.FindProperty(optionsVarName + ".maximumPlaybackRate");
			if (maximumPlaybackRateProp != null)
			{
				EditorGUILayout.Slider(maximumPlaybackRateProp, 2.0f, 10.0f, new GUIContent("Max Playback Rate", "Increase the maximum playback rate before which playback switches to key-frames only."));
			}

			GUILayout.Space(8f);

			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Network", EditorStyles.boldLabel);

			SerializedProperty preferredMaximumResolutionProp = DisplayPlatformOption(optionsVarName, _optionPreferredMaximumResolution);
			if ((MediaPlayer.OptionsApple.Resolution)preferredMaximumResolutionProp.intValue == MediaPlayer.OptionsApple.Resolution.Custom)
			{
				#if UNITY_2017_2_OR_NEWER
				DisplayPlatformOption(optionsVarName, _optionCustomPreferredMaxResolution);
				#endif
			}

			EditorGUILayout.BeginHorizontal();
			DisplayPlatformOption(optionsVarName, _optionCustomPreferredPeakBitRateApple);
			DisplayPlatformOption(optionsVarName, _optionCustomPreferredPeakBitRateUnitsApple);
			EditorGUILayout.EndHorizontal();

			DisplayPlatformOption(optionsVarName, _optionPreferredForwardBufferDuration);

			EditorGUILayout.EndVertical();

			EditorGUILayout.EndVertical();

			// Set the flags

			if (flagsProp != null)
			{
				flagsProp.intValue = (int)flags;
			}

			if (_showUltraOptions)
			{
				SerializedProperty keyAuthProp = serializedObject.FindProperty(optionsVarName + ".keyAuth");
				if (keyAuthProp != null)
				{
					OnInspectorGUI_HlsDecryption(keyAuthProp);
				}

				SerializedProperty httpHeadersProp = serializedObject.FindProperty(optionsVarName + ".httpHeaders.httpHeaders");
				if (httpHeadersProp != null)
				{
					OnInspectorGUI_HttpHeaders(httpHeadersProp);
				}
			}
		}

		private void OnInspectorGUI_Override_MacOSX()
		{
			OnInspectorGUI_Override_Apple(Platform.MacOSX);
		}

		private void OnInspectorGUI_Override_iOS()
		{
			OnInspectorGUI_Override_Apple(Platform.iOS);
		}

		private void OnInspectorGUI_Override_tvOS()
		{
			OnInspectorGUI_Override_Apple(Platform.tvOS);
		}
	}
}
