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
#if UNITY_EDITOR_OSX
		internal const string SubtitleFileExtensions = "srt";
#else
		internal const string SubtitleFileExtensions = "Subtitle Files;*.srt";
#endif

		private SerializedProperty _propSubtitles;
		private SerializedProperty _propSubtitlePath;

		private void OnInspectorGUI_Subtitles()
		{
			// TODO: add support for multiple targets?
			MediaPlayer media = (this.target) as MediaPlayer;

			//EditorGUILayout.BeginVertical();
			EditorGUILayout.PropertyField(_propSubtitles, new GUIContent("Sideload Subtitles"));
		
			EditorGUI.BeginDisabledGroup(!_propSubtitles.boolValue);

			EditorGUILayout.BeginVertical(GUI.skin.box);

			EditorGUILayout.PropertyField(_propSubtitlePath);

			//if (!Application.isPlaying)
			{
				GUI.color = Color.white;
				GUILayout.BeginHorizontal();

				if (Application.isPlaying)
				{
					if (GUILayout.Button("Load"))
					{
						MediaPath mediaPath = new MediaPath(_propSubtitlePath.FindPropertyRelative("_path").stringValue, (MediaPathType)_propSubtitlePath.FindPropertyRelative("_pathType").enumValueIndex);
						media.EnableSubtitles(mediaPath);
					}
					if (GUILayout.Button("Clear"))
					{
						media.DisableSubtitles();
					}
				}
				else
				{
					GUILayout.FlexibleSpace();
				}

				MediaPathDrawer.ShowBrowseSubtitlesButtonIcon(_propSubtitlePath);

				GUILayout.EndHorizontal();
				if (_propSubtitles.boolValue)
				{
					///MediaPath mediaPath = new MediaPath(_propSubtitlePath.FindPropertyRelative("_path").stringValue, (MediaPathType)_propSubtitlePath.FindPropertyRelative("_pathType").enumValueIndex);
					//ShowFileWarningMessages(mediaPath, media.AutoOpen, Platform.Unknown);
					//GUI.color = Color.white;
				}
			}

			//EditorGUILayout.EndVertical();

			EditorGUILayout.EndVertical();
			EditorGUI.EndDisabledGroup();
		}
	}
}