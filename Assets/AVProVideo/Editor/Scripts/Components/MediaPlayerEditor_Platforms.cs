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
		private static int _platformIndex = -1;
		private static bool _HTTPHeadersToggle = false;
		private static GUIContent[] _platformNames = null;

		private void OnInspectorGUI_SelectPlatform()
		{
			// TODO: support multiple targets?
			MediaPlayer media = (this.target) as MediaPlayer;

			int i = 0;
			int platformIndex = _platformIndex;
			foreach (GUIContent platformText in _platformNames)
			{
				MediaPlayer.PlatformOptions options = media.GetPlatformOptions((Platform)i);

				Color hilight = Color.yellow;

				if (i == _platformIndex)
				{
					// Selected, unmodified
					if (!options.IsModified())
					{
						GUI.contentColor = Color.white;
					}
					else
					{
						// Selected, modified
						GUI.color = hilight;
						GUI.contentColor = Color.white;
					}
				}
				else if (options.IsModified())
				{
					// Unselected, modified
					GUI.backgroundColor = Color.grey* hilight;
					GUI.contentColor = hilight;
				}
				else
				{
					// Unselected, unmodified
					if (EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = Color.grey;
						GUI.color = new Color(0.65f, 0.66f, 0.65f);// Color.grey;
					}
				}

				if (i == _platformIndex)
				{
					if (!GUILayout.Toggle(true, _platformNames[i], GUI.skin.button))
					{
						platformIndex = -1;
					}
				}
				else
				{
					GUI.skin.button.imagePosition = ImagePosition.ImageOnly;
					if (GUILayout.Toggle(false, _platformNames[i], GUI.skin.button))
					{
						platformIndex = i;
					}
					GUI.skin.button.imagePosition = ImagePosition.ImageLeft;
				}
				
				GUI.backgroundColor = Color.white;
				GUI.contentColor = Color.white;
				GUI.color = Color.white;
				i++;
			}

			//_platformIndex = GUILayout.SelectionGrid(_platformIndex, _platformNames, 3);
			//return;
#if false
			int rowCount = 0;
			int platformIndex = _platformIndex;
			const int itemsPerLine = 4;
			for (int i = 0; i < _platformNames.Length; i++)
			{
				if (i % itemsPerLine == 0)
				{
					GUILayout.BeginHorizontal();
					rowCount++;
				}
				MediaPlayer.PlatformOptions options = media.GetPlatformOptions((Platform)i);

				Color hilight = Color.yellow;

				if (i == _platformIndex)
				{
					// Selected, unmodified
					if (!options.IsModified())
					{
						GUI.contentColor = Color.white;
					}
					else
					{
						// Selected, modified
						GUI.color = hilight;
						GUI.contentColor = Color.white;
					}
				}
				else if (options.IsModified())
				{
					// Unselected, modified
					GUI.backgroundColor = Color.grey* hilight;
					GUI.contentColor = hilight;
				}
				else
				{
					// Unselected, unmodified
					if (EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = Color.grey;
						GUI.color = new Color(0.65f, 0.66f, 0.65f);// Color.grey;
					}
				}

				if (i == _platformIndex)
				{
					if (!GUILayout.Toggle(true, _platformNames[i], GUI.skin.button))
					{
						platformIndex = -1;
					}
				}
				else
				{
					GUI.skin.button.imagePosition = ImagePosition.ImageOnly;
					if (GUILayout.Toggle(false, _platformNames[i], GUI.skin.button))
					{
						platformIndex = i;
					}
					GUI.skin.button.imagePosition = ImagePosition.ImageLeft;
				}
				if ((i+1) % itemsPerLine == 0)
				{
					rowCount--;
					GUILayout.EndHorizontal();
				}
				GUI.backgroundColor = Color.white;
				GUI.contentColor = Color.white;
				GUI.color = Color.white;
			}

			if (rowCount > 0)
			{
				GUILayout.EndHorizontal();
			}
#endif
			//platformIndex = GUILayout.SelectionGrid(_platformIndex, Helper.GetPlatformNames(), 3);
			//int platformIndex = GUILayout.Toolbar(_platformIndex, Helper.GetPlatformNames());

			if (platformIndex != _platformIndex)
			{
				_platformIndex = platformIndex;

				// We do this to clear the focus, otherwise a focused text field will not change when the Toolbar index changes
				EditorGUI.FocusTextInControl("ClearFocus");
			}
		}

		private void OnInspectorGUI_PlatformOverrides()
		{
			foreach (AnimCollapseSection section in _platformSections)
			{
				AnimCollapseSection.Show(section, indentLevel:2);
			}
		}

		private readonly static GUIContent[] _audio360ChannelMapGuiNames =
		{
			new GUIContent("(TBE_8_2) 8 channels of hybrid TBE ambisonics and 2 channels of head-locked stereo audio"),
			new GUIContent("(TBE_8) 8 channels of hybrid TBE ambisonics. NO head-locked stereo audio"),
			new GUIContent("(TBE_6_2) 6 channels of hybrid TBE ambisonics and 2 channels of head-locked stereo audio"),
			new GUIContent("(TBE_6) 6 channels of hybrid TBE ambisonics. NO head-locked stereo audio"),
			new GUIContent("(TBE_4_2) 4 channels of hybrid TBE ambisonics and 2 channels of head-locked stereo audio"),
			new GUIContent("(TBE_4) 4 channels of hybrid TBE ambisonics. NO head-locked stereo audio"),

			new GUIContent("(TBE_8_PAIR0) Channels 1 and 2 of TBE hybrid ambisonics"),
			new GUIContent("(TBE_8_PAIR1) Channels 3 and 4 of TBE hybrid ambisonics"),
			new GUIContent("(TBE_8_PAIR2) Channels 5 and 6 of TBE hybrid ambisonics"),
			new GUIContent("(TBE_8_PAIR3) Channels 7 and 8 of TBE hybrid ambisonics"),

			new GUIContent("(TBE_CHANNEL0) Channels 1 of TBE hybrid ambisonics"),
			new GUIContent("(TBE_CHANNEL1) Channels 2 of TBE hybrid ambisonics"),
			new GUIContent("(TBE_CHANNEL2) Channels 3 of TBE hybrid ambisonics"),
			new GUIContent("(TBE_CHANNEL3) Channels 4 of TBE hybrid ambisonics"),
			new GUIContent("(TBE_CHANNEL4) Channels 5 of TBE hybrid ambisonics"),
			new GUIContent("(TBE_CHANNEL5) Channels 6 of TBE hybrid ambisonics"),
			new GUIContent("(TBE_CHANNEL6) Channels 7 of TBE hybrid ambisonics"),
			new GUIContent("(TBE_CHANNEL7) Channels 8 of TBE hybrid ambisonics"),

			new GUIContent("(HEADLOCKED_STEREO) Head-locked stereo audio"),
			new GUIContent("(HEADLOCKED_CHANNEL0) Channels 1 or left of head-locked stereo audio"),
			new GUIContent("(HEADLOCKED_CHANNEL1) Channels 2 or right of head-locked stereo audio"),

			new GUIContent("(AMBIX_4) 4 channels of first order ambiX"),
			new GUIContent("(AMBIX_4_2) 4 channels of first order ambiX with 2 channels of head-locked audio"),
			new GUIContent("(AMBIX_9) 9 channels of second order ambiX"),
			new GUIContent("(AMBIX_9_2) 9 channels of second order ambiX with 2 channels of head-locked audio"),
			new GUIContent("(AMBIX_16) 16 channels of third order ambiX"),
			new GUIContent("(AMBIX_16_2) 16 channels of third order ambiX with 2 channels of head-locked audio"),

			new GUIContent("(MONO) Mono audio"),
			new GUIContent("(STEREO) Stereo audio"),
		};

		private struct FieldDescription
		{
			public FieldDescription(string fieldName, GUIContent description)
			{
				this.fieldName = fieldName;
				this.description = description;
			}
			public string fieldName;
			public GUIContent description;
		}

		private SerializedProperty DisplayPlatformOption(string platformOptionsFieldName, FieldDescription option)
		{
			return DisplayPlatformOption(this.serializedObject, platformOptionsFieldName + option.fieldName, option.description);
		}

		private static SerializedProperty DisplayPlatformOption(SerializedObject so, string fieldName, GUIContent description)
		{
			SerializedProperty prop = so.FindProperty(fieldName);
			if (prop != null)
			{
				if (description == GUIContent.none)
				{
					EditorGUILayout.PropertyField(prop, true);
				}
				else
				{
					EditorGUILayout.PropertyField(prop, description, true);
				}
			}
			else
			{
				Debug.LogWarning("Can't find property `" + fieldName + "`");
			}
			return prop;
		}

		private SerializedProperty DisplayPlatformOptionEnum(string platformOptionsFieldName, FieldDescription option, GUIContent[] enumNames)
		{
			return DisplayPlatformOptionEnum(this.serializedObject, platformOptionsFieldName + option.fieldName, option.description, enumNames);
		}

		private static SerializedProperty DisplayPlatformOptionEnum(SerializedObject so, string fieldName, GUIContent description, GUIContent[] enumNames)
		{
			SerializedProperty prop = so.FindProperty(fieldName);
			if (prop != null)
			{
				prop.enumValueIndex = EditorGUILayout.Popup(description, prop.enumValueIndex, enumNames);
			}
			else
			{
				Debug.LogWarning("Can't find property `" + fieldName + "`");
			}
			return prop;
		}

#if false
		private void OnInspectorGUI_HlsDecryption(string optionsVarName)
		{
			EditorGUILayout.BeginVertical(GUI.skin.box);
			GUILayout.Label("HLS Decryption", EditorStyles.boldLabel);

			// Key server auth token
			SerializedProperty prop = serializedObject.FindProperty(optionsVarName + ".keyAuth.keyServerToken");
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop, new GUIContent("Key Server Auth Token", "Token to pass to the key server in the 'Authorization' HTTP header field"));
			}

			GUILayout.Label("Overrides");
			EditorGUI.indentLevel++;

			// Key server override
			/*prop = serializedObject.FindProperty(optionsVarName + ".keyServerURLOverride");
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop, new GUIContent("Key Server URL", "Overrides the key server URL if present in a HLS manifest."));
			}*/
			
			// Key data blob override
			prop = serializedObject.FindProperty(optionsVarName + ".keyAuth.overrideDecryptionKeyBase64");
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop, new GUIContent("Key (Base64)", "Override key to use for decoding encrypted HLS streams (in Base64 format)."));
			}

			EditorGUI.indentLevel--;

			EditorGUILayout.EndVertical();
		}

		private void OnInspectorGUI_HttpHeaders(string platformOptionsVarName)
		{
			SerializedProperty httpHeadersProp = serializedObject.FindProperty(platformOptionsVarName + ".httpHeaders.httpHeaders");
			if (httpHeadersProp != null)
			{

				if (BeginCollapsableSection("Custom HTTP Headers", ref _HTTPHeadersToggle))
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
				EndCollapsableSection();
			}
		}
#endif
	}
}