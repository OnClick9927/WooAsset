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
		private void OnInspectorGUI_Network()
		{
			if (_showUltraOptions)
			{
				SerializedProperty httpHeadersProp = serializedObject.FindProperty("_httpHeaders.httpHeaders");
				OnInspectorGUI_HttpHeaders(httpHeadersProp);

				SerializedProperty keyAuthProp = serializedObject.FindProperty("_keyAuth");
				OnInspectorGUI_HlsDecryption(keyAuthProp);
			}
		}

		private void OnInspectorGUI_HlsDecryption(SerializedProperty keyAuthProp)
		{
			if (keyAuthProp == null) return;

			EditorGUILayout.BeginVertical(GUI.skin.box);
			GUILayout.Label("HLS Decryption", EditorStyles.boldLabel);

			// Key server auth token
			SerializedProperty prop = keyAuthProp.FindPropertyRelative("keyServerToken");
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop, new GUIContent("Auth Token", "Token to pass to the key server in the 'Authorization' HTTP header field"));
			}

			//GUILayout.Label("Overrides");
			//EditorGUI.indentLevel++;

			// Key server override
			/*prop = serializedObject.FindProperty(optionsVarName + ".keyServerURLOverride");
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop, new GUIContent("Key Server URL", "Overrides the key server URL if present in a HLS manifest."));
			}*/
			
			// Key data blob override
			prop = keyAuthProp.FindPropertyRelative("overrideDecryptionKeyBase64");
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop, new GUIContent("Key Override (Base64)", "Override key to use for decoding encrypted HLS streams (in Base64 format)."));
			}

			//EditorGUI.indentLevel--;

			EditorGUILayout.EndVertical();
		}

		private void OnInspectorGUI_HttpHeaders(SerializedProperty httpHeadersProp)
		{
			if (httpHeadersProp ==  null) return;
			
			//GUILayout.Space(8f);
			bool isExpanded = _HTTPHeadersToggle;
			if (isExpanded)
			{
				EditorGUILayout.BeginVertical(GUI.skin.box);
			}
			bool hasHeaders = (httpHeadersProp.arraySize > 0);
			Color tintColor = hasHeaders?Color.yellow:Color.white;
			if (AnimCollapseSection.BeginShow("Custom HTTP Headers", ref _HTTPHeadersToggle, tintColor))
			{
				{
					if (httpHeadersProp.arraySize > 0)
					{
						int deleteIndex = -1;
						for (int i = 0; i < httpHeadersProp.arraySize; ++i)
						{
							SerializedProperty httpHeaderProp = httpHeadersProp.GetArrayElementAtIndex(i);
							SerializedProperty headerProp = httpHeaderProp.FindPropertyRelative("name");

							GUILayout.BeginVertical(GUI.skin.box);
							GUILayout.BeginHorizontal();

							GUI.color = HttpHeader.IsValid(headerProp.stringValue)?Color.white:Color.red;
							EditorGUILayout.PropertyField(headerProp, GUIContent.none);
							headerProp.stringValue = headerProp.stringValue.Trim();
							GUI.color = Color.white;

							if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
							{
								deleteIndex = i;
							}
							GUILayout.EndHorizontal();

							SerializedProperty valueProp = httpHeaderProp.FindPropertyRelative("value");
							GUI.color = HttpHeader.IsValid(valueProp.stringValue)?Color.white:Color.red;
							valueProp.stringValue = EditorGUILayout.TextArea(valueProp.stringValue, EditorHelper.IMGUI.GetWordWrappedTextAreaStyle());
							GUI.color = Color.white;
							valueProp.stringValue = valueProp.stringValue.Trim();
							GUILayout.EndVertical();
							GUILayout.Space(4f);
						}

						if (deleteIndex >= 0)
						{
							httpHeadersProp.DeleteArrayElementAtIndex(deleteIndex);
						}
					}
					if (GUILayout.Button("+"))
					{
						httpHeadersProp.InsertArrayElementAtIndex(httpHeadersProp.arraySize);
					}
				}
			}
			AnimCollapseSection.EndShow();

			if (isExpanded)
			{
				EditorGUILayout.EndVertical();
			}
			
			//GUILayout.Space(8f);
		}
	}
}