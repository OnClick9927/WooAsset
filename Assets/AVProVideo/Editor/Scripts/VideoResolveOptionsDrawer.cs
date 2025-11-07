using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RenderHeads.Media.AVProVideo.Editor
{
#if AVPRO_FEATURE_VIDEORESOLVE
	[CustomPropertyDrawer(typeof(VideoResolveOptions))]
	public class VideoResolveOptionsDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return 0f; }

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, GUIContent.none, property);

			SerializedProperty propApplyHSBC = property.FindPropertyRelative("applyHSBC");
			EditorGUILayout.PropertyField(propApplyHSBC, new GUIContent("Image Adjustments"));

			if (propApplyHSBC.boolValue)
			{
				SerializedProperty propHue = property.FindPropertyRelative("hue");
				SerializedProperty propSaturation = property.FindPropertyRelative("saturation");
				SerializedProperty propBrightness = property.FindPropertyRelative("brightness");
				SerializedProperty propContrast = property.FindPropertyRelative("contrast");
				SerializedProperty propGamma = property.FindPropertyRelative("gamma");

				EditorGUILayout.PropertyField(propHue);
				EditorGUILayout.PropertyField(propSaturation);
				EditorGUILayout.PropertyField(propBrightness);
				EditorGUILayout.PropertyField(propContrast);
				EditorGUILayout.PropertyField(propGamma);
			}

			{
				SerializedProperty propTint = property.FindPropertyRelative("tint");
				SerializedProperty propGenerateMipMaps = property.FindPropertyRelative("generateMipmaps");
				EditorGUILayout.PropertyField(propTint);
				EditorGUILayout.PropertyField(propGenerateMipMaps);
			}

			EditorGUI.EndProperty();
		}
	}

	[CustomPropertyDrawer(typeof(VideoResolve))]
	public class VideoResolveDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return 0f; }

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, GUIContent.none, property);

			SerializedProperty propOptions = property.FindPropertyRelative("_options");
			SerializedProperty propTargetRenderTexture = property.FindPropertyRelative("_targetRenderTexture");

			EditorGUILayout.PropertyField(propOptions, true);
			EditorGUILayout.PropertyField(propTargetRenderTexture, new GUIContent("Render Texture"));
			if (propTargetRenderTexture.objectReferenceValue != null)
			{
				SerializedProperty propTargetRenderTextureScale = property.FindPropertyRelative("_targetRenderTextureScale");
				EditorGUILayout.PropertyField(propTargetRenderTextureScale);
			}

			EditorGUI.EndProperty();
		}
	}	
#endif
}