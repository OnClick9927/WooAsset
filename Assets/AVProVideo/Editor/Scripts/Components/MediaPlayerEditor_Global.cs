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
		private void OnInspectorGUI_GlobalSettings()
		{
			EditorGUI.BeginDisabledGroup(Application.isPlaying);
			EditorGUILayout.LabelField("Target Platform", EditorUserBuildSettings.selectedBuildTargetGroup.ToString());
			if (EditorUserBuildSettings.selectedBuildTargetGroup != BuildTargetGroup.Standalone)
			{
				EditorHelper.IMGUI.NoticeBox(MessageType.Warning, "These global options only affect the current target platform so will not apply in-editor unless you change your Build Target and reapply them.");
			}

			EditorGUILayout.BeginVertical(GUI.skin.box);
			GUILayout.Label("Video Capture", EditorStyles.boldLabel);

			// TimeScale
			{
				const string TimeScaleDefine = "AVPROVIDEO_BETA_SUPPORT_TIMESCALE";
				if (EditorHelper.IMGUI.ToggleScriptDefine("TimeScale Support", TimeScaleDefine))
				{
					EditorHelper.IMGUI.NoticeBox(MessageType.Warning, "This will affect performance if you change Time.timeScale or Time.captureFramerate.  This feature is useful for supporting video capture system that adjust time scale during capturing.");
				}
			}
			EditorGUILayout.EndVertical();
			
			EditorGUILayout.BeginVertical(GUI.skin.box);
			GUILayout.Label("Other", EditorStyles.boldLabel);

			// Disable Logging
			{
				const string DisableLogging = "AVPROVIDEO_DISABLE_LOGGING";
				EditorHelper.IMGUI.ToggleScriptDefine("Disable Logging", DisableLogging);
			}

			// Show Ultra Options
			{
				const string ShowUltraOptions = "AVPROVIDEO_SHOW_ULTRA_OPTIONS";
				EditorHelper.IMGUI.ToggleScriptDefine("Show Ultra Options", ShowUltraOptions);
			}

			_allowDeveloperMode = EditorGUILayout.Toggle(new GUIContent("Developer Mode", "Enables some additional information useful for debugging"), _allowDeveloperMode);

			if (_allowDeveloperMode)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginVertical(GUI.skin.box);
				GUILayout.Label("BETA / Experimental", EditorStyles.boldLabel);

				// Disable Debug GUI
				{
					const string SupportBufferedDisplayDefine = "AVPROVIDEO_SUPPORT_BUFFERED_DISPLAY";
					if (!EditorHelper.IMGUI.ToggleScriptDefine("Support Buffered Display", SupportBufferedDisplayDefine))
					{
						EditorHelper.IMGUI.NoticeBox(MessageType.Info, "The Debug GUI can be disabled globally for builds to help reduce garbage generation each frame.");
					}
				}

				EditorGUILayout.EndVertical();
				EditorGUI.indentLevel--;
			}

			EditorGUILayout.EndVertical();

			EditorGUI.EndDisabledGroup();
		}
	}
}