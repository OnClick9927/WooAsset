using UnityEngine;
using UnityEditor;

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
		private SerializedProperty _propVolume;
		private SerializedProperty _propBalance;
		private SerializedProperty _propMuted;
		private SerializedProperty _propAudioHeadTransform;
		private SerializedProperty _propAudioEnableFocus;
		private SerializedProperty _propAudioFocusOffLevelDB;
		private SerializedProperty _propAudioFocusWidthDegrees;
		private SerializedProperty _propAudioFocusTransform;

		private void OnInspectorGUI_Audio()
		{
			if (EditorUtility.audioMasterMute)
			{
				EditorGUILayout.LabelField("Muted in Editor");
			}
			EditorGUI.BeginDisabledGroup(EditorUtility.audioMasterMute);
			EditorGUILayout.BeginVertical(GUI.skin.box);
			//GUILayout.Label("Audio", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_propVolume, new GUIContent("Volume"));
			if (EditorGUI.EndChangeCheck())
			{
				foreach (MediaPlayer player in this.targets)
				{
					player.AudioVolume = _propVolume.floatValue;
				}
			}

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_propBalance, new GUIContent("Balance"));
			if (EditorGUI.EndChangeCheck())
			{
				foreach (MediaPlayer player in this.targets)
				{
					player.AudioBalance = _propBalance.floatValue;
				}
			}

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_propMuted, new GUIContent("Muted"));
			if (EditorGUI.EndChangeCheck())
			{
				foreach (MediaPlayer player in this.targets)
				{
					player.AudioMuted = _propMuted.boolValue;
				}
			}

			EditorGUILayout.EndVertical();

			if (_showUltraOptions)
			{
				EditorGUILayout.BeginVertical(GUI.skin.box);
				GUILayout.Label("Audio 360", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_propAudioHeadTransform, new GUIContent("Head Transform", "Set this to your head camera transform. Only currently used for Facebook Audio360"));
				EditorGUILayout.PropertyField(_propAudioEnableFocus, new GUIContent("Enable Focus", "Enables focus control. Only currently used for Facebook Audio360"));
				if (_propAudioEnableFocus.boolValue)
				{
					EditorGUILayout.PropertyField(_propAudioFocusOffLevelDB, new GUIContent("Off Focus Level DB", "Sets the off-focus level in DB, with the range being between -24 to 0 DB. Only currently used for Facebook Audio360"));
					EditorGUILayout.PropertyField(_propAudioFocusWidthDegrees, new GUIContent("Focus Width Degrees", "Set the focus width in degrees, with the range being between 40 and 120 degrees. Only currently used for Facebook Audio360"));
					EditorGUILayout.PropertyField(_propAudioFocusTransform, new GUIContent("Focus Transform", "Set this to where you wish to focus on the video. Only currently used for Facebook Audio360"));
				}
				EditorGUILayout.EndVertical();
			}

			EditorGUI.EndDisabledGroup();
		}
	}
}