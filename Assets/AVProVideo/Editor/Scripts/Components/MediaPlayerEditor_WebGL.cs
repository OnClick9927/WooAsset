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
		private readonly static FieldDescription _optionExternalLibrary = new FieldDescription(".externalLibrary", GUIContent.none);

		private void OnInspectorGUI_Override_WebGL()
		{
			GUILayout.Space(8f);

			string optionsVarName = MediaPlayer.GetPlatformOptionsVariable(Platform.WebGL);

			EditorGUILayout.BeginVertical(GUI.skin.box);

			DisplayPlatformOption(optionsVarName, _optionExternalLibrary);

			SerializedProperty propUseTextureMips = DisplayPlatformOption(optionsVarName, _optionTextureMips);
			if (propUseTextureMips.boolValue && ((FilterMode)_propFilter.enumValueIndex) != FilterMode.Trilinear)
			{
				EditorHelper.IMGUI.NoticeBox(MessageType.Info, "Recommend changing the texture filtering mode to Trilinear when using mip-maps.");
			}

			EditorGUILayout.EndVertical();
		}
	}
}