using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RenderHeads.Media.AVProVideo.Editor
{
	[CustomPropertyDrawer(typeof(MediaHints))]
	public class MediaHintsDrawer : PropertyDrawer
	{
		private readonly static GUIContent[] StereoPackingOptions =
		{
			// NOTE: must be in the same order as enum StereoPacking
			new GUIContent("None"),
			new GUIContent("Top Bottom"),
			new GUIContent("Left Right"),
			new GUIContent("Custom UV"),
		};

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return 0f; }

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, GUIContent.none, property);

			SerializedProperty propHintsTransparency = property.FindPropertyRelative("transparency");
			SerializedProperty propHintsAlphaPacking = property.FindPropertyRelative("alphaPacking");
			SerializedProperty propHintsStereoPacking = property.FindPropertyRelative("stereoPacking");

			EditorGUILayout.PropertyField(propHintsTransparency);
			if ((TransparencyMode)propHintsTransparency.enumValueIndex == TransparencyMode.Transparent)
			{
				EditorGUILayout.PropertyField(propHintsAlphaPacking);
			}

			{
				// NOTE: We don't allow selection of 'Two Textures' as this mode is only produced by the Players as it is platform specific
				propHintsStereoPacking.enumValueIndex = EditorGUILayout.Popup(new GUIContent("Stereo Packing"), propHintsStereoPacking.enumValueIndex, StereoPackingOptions);
			}

			EditorGUI.EndProperty();
		}
	}
}