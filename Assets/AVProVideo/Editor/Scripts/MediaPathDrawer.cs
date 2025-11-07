using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RenderHeads.Media.AVProVideo.Editor
{
	[CustomPropertyDrawer(typeof(MediaPath))]
	public class MediaPathDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return 0f; }

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, GUIContent.none, property);

			SerializedProperty propPath = property.FindPropertyRelative("_path");
			SerializedProperty propPathType = property.FindPropertyRelative("_pathType");

			EditorGUILayout.PropertyField(propPathType, GUIContent.none);

			//GUI.color = HttpHeader.IsValid(valueProp.stringValue)?Color.white:Color.red;
			string newUrl = EditorGUILayout.TextArea(propPath.stringValue, EditorHelper.IMGUI.GetWordWrappedTextAreaStyle());
			//GUI.color = Color.white;
			newUrl = newUrl.Trim();
			if (EditorHelper.SafeSetPathProperty(newUrl, propPath))
			{
				// TODO: shouldn't we set all targets?
				EditorUtility.SetDirty(property.serializedObject.targetObject);
			}
			MediaPlayerEditor.ShowFileWarningMessages(propPath.stringValue, (MediaPathType)propPathType.enumValueIndex, null, MediaSource.Path, false, Platform.Unknown);
			GUI.color = Color.white;
			
			EditorGUI.EndProperty();
		}

		public static void ShowBrowseButton(SerializedProperty propMediaPath)
		{
			GUIContent buttonText = new GUIContent("Browse", EditorGUIUtility.IconContent("d_Project").image);
			if (GUILayout.Button(buttonText, GUILayout.ExpandWidth(true)))
			{
				RecentMenu.Create(propMediaPath, null, MediaPlayerEditor.MediaFileExtensions, false);
			}
		}

		public static void ShowBrowseButtonIcon(SerializedProperty propMediaPath, SerializedProperty propMediaSource)
		{
			if (GUILayout.Button(EditorGUIUtility.IconContent("d_Project"), GUILayout.ExpandWidth(false)))
			{
				RecentMenu.Create(propMediaPath, propMediaSource, MediaPlayerEditor.MediaFileExtensions, false, 100);
			}
		}

		public static void ShowBrowseSubtitlesButtonIcon(SerializedProperty propMediaPath)
		{
			if (GUILayout.Button(EditorGUIUtility.IconContent("d_Project"), GUILayout.ExpandWidth(false)))// GUILayout.Height(EditorGUIUtility.singleLineHeight)))
			{
				RecentMenu.Create(propMediaPath, null, MediaPlayerEditor.SubtitleFileExtensions, false);
			}
		}
	}
}