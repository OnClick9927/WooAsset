using UnityEngine;
using UnityEditor;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// <summary>
	/// Editor for the DisplayIMGUI component
	/// </summary>
	[CanEditMultipleObjects]
	[CustomEditor(typeof(DisplayIMGUI))]
	public class DisplayIMGUIEditor : UnityEditor.Editor
	{
		private SerializedProperty _propMediaPlayer;
		private SerializedProperty _propScaleMode;
		private SerializedProperty _propColor;
		private SerializedProperty _propAllowTransparency;
		private SerializedProperty _propUseDepth;
		private SerializedProperty _propDepth;
		private SerializedProperty _propAreaFullscreen;
		private SerializedProperty _propAreaX;
		private SerializedProperty _propAreaY;
		private SerializedProperty _propAreaWidth;
		private SerializedProperty _propAreaHeight;
		private SerializedProperty _propShowAreaInEditor;

		void OnEnable()
		{
			_propMediaPlayer = this.CheckFindProperty("_mediaPlayer");
			_propScaleMode = this.CheckFindProperty("_scaleMode");
			_propColor = this.CheckFindProperty("_color");
			_propAllowTransparency = this.CheckFindProperty("_allowTransparency");
			_propUseDepth = this.CheckFindProperty("_useDepth");
			_propDepth = this.CheckFindProperty("_depth");
			_propAreaFullscreen = this.CheckFindProperty("_isAreaFullScreen");
			_propAreaX = this.CheckFindProperty("_areaX");
			_propAreaY = this.CheckFindProperty("_areaY");
			_propAreaWidth = this.CheckFindProperty("_areaWidth");
			_propAreaHeight = this.CheckFindProperty("_areaHeight");
			_propShowAreaInEditor = this.CheckFindProperty("_showAreaInEditor");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(_propMediaPlayer);
			EditorGUILayout.PropertyField(_propScaleMode);
			EditorGUILayout.PropertyField(_propColor);
			EditorGUILayout.PropertyField(_propAllowTransparency);
			EditorGUILayout.PropertyField(_propUseDepth);
			if (_propUseDepth.boolValue)
			{
				EditorGUILayout.PropertyField(_propDepth);
			}

			// Area
			EditorGUILayout.PropertyField(_propAreaFullscreen, new GUIContent("Full Screen"));
			if (!_propAreaFullscreen.boolValue)
			{
				EditorGUILayout.PropertyField(_propAreaX, new GUIContent("X"));
				EditorGUILayout.PropertyField(_propAreaY, new GUIContent("Y"));
				EditorGUILayout.PropertyField(_propAreaWidth, new GUIContent("Width"));
				EditorGUILayout.PropertyField(_propAreaHeight, new GUIContent("Height"));
			}
			EditorGUILayout.PropertyField(_propShowAreaInEditor, new GUIContent("Show in Editor"));

			serializedObject.ApplyModifiedProperties();

			// Force update
			bool unhandledChanges = (EditorGUI.EndChangeCheck() && Application.isPlaying);
			if (unhandledChanges)
			{
				foreach (Object obj in this.targets)
				{
					((DisplayIMGUI)obj).Update();
				}
			}
		}
	}
}