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
		private readonly static GUIContent[] _audioModesAndroid =
		{
			new GUIContent("System Direct"),
			new GUIContent("Unity"),
			new GUIContent("Facebook Audio 360", "Initialises player with Facebook Audio 360 support"),
		};

		private readonly static FieldDescription _optionFileOffset = new FieldDescription(".fileOffset", GUIContent.none);
		private readonly static FieldDescription _optionUseFastOesPath = new FieldDescription(".useFastOesPath", new GUIContent("Use OES Rendering", "Enables a faster rendering path using OES textures.  This requires that all rendering in Unity uses special GLSL shaders."));
		private readonly static FieldDescription _optionShowPosterFrames = new FieldDescription(".showPosterFrame", new GUIContent("Show Poster Frame", "Allows a paused loaded video to display the initial frame. This uses up decoder resources."));
		private readonly static FieldDescription _optionPreferSoftwareDecoder = new FieldDescription(".preferSoftwareDecoder", GUIContent.none);
		private readonly static FieldDescription _optionPreferredMaximumResolution = new FieldDescription("._preferredMaximumResolution", new GUIContent("Preferred Maximum Resolution", "The desired maximum resolution of the video."));
#if UNITY_2017_2_OR_NEWER
		private readonly static FieldDescription _optionCustomPreferredMaxResolution = new FieldDescription("._customPreferredMaximumResolution", new GUIContent(" "));
#endif
		private readonly static FieldDescription _optionCustomPreferredPeakBitRate = new FieldDescription("._preferredPeakBitRate", new GUIContent("Preferred Peak BitRate", "The desired limit of network bandwidth consumption for playback, set to 0 for no preference."));
		private readonly static FieldDescription _optionCustomPreferredPeakBitRateUnits = new FieldDescription("._preferredPeakBitRateUnits", new GUIContent());

		private readonly static FieldDescription _optionMinBufferMs = new FieldDescription(".minBufferMs", new GUIContent("Minimum Buffer Ms"));
		private readonly static FieldDescription _optionMaxBufferMs = new FieldDescription(".maxBufferMs", new GUIContent("Maximum Buffer Ms"));
		private readonly static FieldDescription _optionBufferForPlaybackMs = new FieldDescription(".bufferForPlaybackMs", new GUIContent("Buffer For Playback Ms"));
		private readonly static FieldDescription _optionBufferForPlaybackAfterRebufferMs = new FieldDescription(".bufferForPlaybackAfterRebufferMs", new GUIContent("Buffer For Playback After Rebuffer Ms"));

		private void OnInspectorGUI_Override_Android()
		{
			//MediaPlayer media = (this.target) as MediaPlayer;
			//MediaPlayer.OptionsAndroid options = media._optionsAndroid;

			GUILayout.Space(8f);

			string optionsVarName = MediaPlayer.GetPlatformOptionsVariable(Platform.Android);

			{
				EditorGUILayout.BeginVertical(GUI.skin.box);

				DisplayPlatformOption(optionsVarName, _optionVideoAPI);

				{
					SerializedProperty propFileOffset = DisplayPlatformOption(optionsVarName, _optionFileOffset);
					propFileOffset.intValue = Mathf.Max(0, propFileOffset.intValue);
				}

				{
					SerializedProperty propUseFastOesPath = DisplayPlatformOption(optionsVarName, _optionUseFastOesPath);
					if (propUseFastOesPath.boolValue)
					{
						EditorHelper.IMGUI.NoticeBox(MessageType.Info, "OES can require special shaders.  Make sure you assign an AVPro Video OES shader to your meshes/materials that need to display video.");

						// PlayerSettings.virtualRealitySupported is deprecated from 2019.3
#if !UNITY_2019_3_OR_NEWER
						if (PlayerSettings.virtualRealitySupported)
#endif
						{
							if (PlayerSettings.stereoRenderingPath != StereoRenderingPath.MultiPass)
							{
								EditorHelper.IMGUI.NoticeBox(MessageType.Error, "OES only supports multi-pass stereo rendering path, please change in Player Settings.");
							}
						}

						EditorHelper.IMGUI.NoticeBox(MessageType.Warning, "OES is not supported in the trial version.  If your Android plugin is not trial then you can ignore this warning.");
					}
				}

				EditorGUILayout.EndVertical();
			}

			if (_showUltraOptions)
			{
				SerializedProperty httpHeadersProp = serializedObject.FindProperty(optionsVarName + ".httpHeaders.httpHeaders");
				if (httpHeadersProp != null)
				{
					OnInspectorGUI_HttpHeaders(httpHeadersProp);
				}
			}

			// MediaPlayer API options
			{
				EditorGUILayout.BeginVertical(GUI.skin.box);
				GUILayout.Label("MediaPlayer API Options", EditorStyles.boldLabel);

				DisplayPlatformOption(optionsVarName, _optionShowPosterFrames);

				EditorGUILayout.EndVertical();
			}

			// ExoPlayer API options
			{
				EditorGUILayout.BeginVertical(GUI.skin.box);
				GUILayout.Label("ExoPlayer API Options", EditorStyles.boldLabel);

				DisplayPlatformOption(optionsVarName, _optionPreferSoftwareDecoder);

				// Audio
				{
					SerializedProperty propAudioOutput = DisplayPlatformOptionEnum(optionsVarName, _optionAudioOutput, _audioModesAndroid);
					if ((Android.AudioOutput)propAudioOutput.enumValueIndex == Android.AudioOutput.FacebookAudio360)
					{
						if (_showUltraOptions)
						{
							EditorGUILayout.Space();
							EditorGUILayout.LabelField("Facebook Audio 360", EditorStyles.boldLabel);
							DisplayPlatformOptionEnum(optionsVarName, _optionAudio360ChannelMode, _audio360ChannelMapGuiNames);
						}
					}
				}

				GUILayout.Space(8f);

//				EditorGUILayout.BeginVertical();
				EditorGUILayout.LabelField("Adaptive Stream", EditorStyles.boldLabel);

				DisplayPlatformOption(optionsVarName, _optionStartMaxBitrate);

				{
					SerializedProperty preferredMaximumResolutionProp = DisplayPlatformOption(optionsVarName, _optionPreferredMaximumResolution);
					if ((MediaPlayer.OptionsAndroid.Resolution)preferredMaximumResolutionProp.intValue == MediaPlayer.OptionsAndroid.Resolution.Custom)
					{
#if UNITY_2017_2_OR_NEWER
						DisplayPlatformOption(optionsVarName, _optionCustomPreferredMaxResolution);
#endif
					}
				}

				{
					EditorGUILayout.BeginHorizontal();
					DisplayPlatformOption(optionsVarName, _optionCustomPreferredPeakBitRate);
					DisplayPlatformOption(optionsVarName, _optionCustomPreferredPeakBitRateUnits);
					EditorGUILayout.EndHorizontal();
				}

				DisplayPlatformOption(optionsVarName, _optionMinBufferMs);
				DisplayPlatformOption(optionsVarName, _optionMaxBufferMs);
				DisplayPlatformOption(optionsVarName, _optionBufferForPlaybackMs);
				DisplayPlatformOption(optionsVarName, _optionBufferForPlaybackAfterRebufferMs);

				EditorGUILayout.EndVertical();
			}
			GUI.enabled = true;

			/*
			SerializedProperty propFileOffsetLow = serializedObject.FindProperty(optionsVarName + ".fileOffsetLow");
			SerializedProperty propFileOffsetHigh = serializedObject.FindProperty(optionsVarName + ".fileOffsetHigh");
			if (propFileOffsetLow != null && propFileOffsetHigh != null)
			{
				propFileOffsetLow.intValue = ;

				EditorGUILayout.PropertyField(propFileOFfset);
			}*/
		}
	}
}