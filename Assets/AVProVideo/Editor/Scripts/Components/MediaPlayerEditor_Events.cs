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
		private SerializedProperty _propEvents;
		private SerializedProperty _propEventMask;
		private SerializedProperty _propPauseMediaOnAppPause;
		private SerializedProperty _propPlayMediaOnAppUnpause;

		private void OnInspectorGUI_Events()
		{		
			EditorGUILayout.BeginVertical(GUI.skin.box);
			
			EditorGUILayout.PropertyField(_propEvents);

			_propEventMask.intValue = EditorGUILayout.MaskField("Triggered Events", _propEventMask.intValue, System.Enum.GetNames(typeof(MediaPlayerEvent.EventType)));

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Pause Media On App Pause");
			_propPauseMediaOnAppPause.boolValue = EditorGUILayout.Toggle(_propPauseMediaOnAppPause.boolValue);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Play Media On App Unpause");
			_propPlayMediaOnAppUnpause.boolValue = EditorGUILayout.Toggle(_propPlayMediaOnAppUnpause.boolValue);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();
		}
	}
}