using UnityEngine;
using UnityEditor;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// <summary>
	/// About/Help section of the editor for the MediaPlayer component
	/// </summary>
	public partial class MediaPlayerEditor : UnityEditor.Editor
	{
		public const string LinkPluginWebsite = "https://renderheads.com/products/avpro-video/";
		public const string LinkForumPage = "https://forum.unity.com/threads/released-avpro-video-complete-video-playback-solution.385611/";
		public const string LinkForumLastPage = "https://forum.unity.com/threads/released-avpro-video-complete-video-playback-solution.385611/page-100";
		public const string LinkGithubIssues = "https://github.com/RenderHeads/UnityPlugin-AVProVideo/issues";
		public const string LinkGithubIssuesNew = "https://github.com/RenderHeads/UnityPlugin-AVProVideo/issues/new/choose";
		public const string LinkAssetStorePage = "https://assetstore.unity.com/packages/slug/181844?aid=1101lcNgx";
		public const string LinkUserManual = "https://www.renderheads.com/content/docs/AVProVideo/";
		public const string LinkScriptingClassReference = "https://www.renderheads.com/content/docs/AVProVideo/api/RenderHeads.Media.AVProVideo.html";
		public const string LinkPurchase = "https://www.renderheads.com/content/docs/AVProVideo/articles/download.html";

		private struct Native
		{
#if UNITY_EDITOR_WIN
			[System.Runtime.InteropServices.DllImport("AVProVideo")]
			public static extern System.IntPtr GetPluginVersion();
#elif UNITY_EDITOR_OSX
			[System.Runtime.InteropServices.DllImport("AVProVideo")]
			public static extern System.IntPtr AVPPluginGetVersionStringPointer();
#endif
		}

		private static string GetPluginVersion()
		{
			string version = "Unknown";
			try
			{
#if UNITY_EDITOR_WIN
				version = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(Native.GetPluginVersion());
#elif UNITY_EDITOR_OSX
				version = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(Native.AVPPluginGetVersionStringPointer());
#endif
			}
			catch (System.DllNotFoundException e)
			{
#if UNITY_EDITOR_OSX
				Debug.LogError("[AVProVideo] Failed to load Bundle. " + e.Message);
#else
				Debug.LogError("[AVProVideo] Failed to load DLL. " + e.Message);
#endif
			}
			return version;
		}

		private static Texture2D GetIcon(Texture2D icon)
		{
			if (icon == null)
			{
				icon = Resources.Load<Texture2D>("AVProVideoIcon");
			}
			return icon;
		}

		private void OnInspectorGUI_About()
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			_icon = GetIcon(_icon);
			if (_icon != null)
			{
				GUILayout.Label(new GUIContent(_icon));
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			GUI.color = Color.yellow;
			EditorHelper.IMGUI.CentreLabel("AVPro Video by RenderHeads Ltd", EditorStyles.boldLabel);
			EditorHelper.IMGUI.CentreLabel("version " + Helper.AVProVideoVersion + " (plugin v" + GetPluginVersion() + ")");
			GUI.color = Color.white;
			GUI.backgroundColor = Color.white;

			if (_icon != null)
			{
				GUILayout.Space(8f);
				ShowSupportWindowButton();
				GUILayout.Space(8f);
			}

			EditorGUILayout.LabelField("Links", EditorStyles.boldLabel);

			GUILayout.Space(8f);

			EditorGUILayout.LabelField("Documentation");
			if (GUILayout.Button("User Manual, FAQ, Release Notes", GUILayout.ExpandWidth(false)))
			{
				Application.OpenURL(LinkUserManual);
			}
			if (GUILayout.Button("Scripting Class Reference", GUILayout.ExpandWidth(false)))
			{
				Application.OpenURL(LinkScriptingClassReference);
			}

			GUILayout.Space(16f);

			GUILayout.Label("Bugs and Support");
			if (GUILayout.Button("Open Help & Support", GUILayout.ExpandWidth(false)))
			{
				SupportWindow.Init();
			}

			GUILayout.Space(16f);

			GUILayout.Label("Rate and Review (★★★★☆)", GUILayout.ExpandWidth(false));
			if (GUILayout.Button("Asset Store Page", GUILayout.ExpandWidth(false)))
			{
				Application.OpenURL(LinkAssetStorePage);
			}

			GUILayout.Space(16f);

			GUILayout.Label("Community");
			if (GUILayout.Button("Forum Thread", GUILayout.ExpandWidth(false)))
			{
				Application.OpenURL(LinkForumPage);
			}

			GUILayout.Space(16f);

			GUILayout.Label("Homepage", GUILayout.ExpandWidth(false));
			if (GUILayout.Button("Official Website", GUILayout.ExpandWidth(false)))
			{
				Application.OpenURL(LinkPluginWebsite);
			}

			GUILayout.Space(32f);

			EditorGUILayout.LabelField("Credits", EditorStyles.boldLabel);
			GUILayout.Space(8f);

			EditorHelper.IMGUI.CentreLabel("Programming", EditorStyles.boldLabel);
			EditorHelper.IMGUI.CentreLabel("Andrew Griffiths");
			EditorHelper.IMGUI.CentreLabel("Morris Butler");
			EditorHelper.IMGUI.CentreLabel("Ste Butcher");
			EditorHelper.IMGUI.CentreLabel("Richard Turnbull");
			EditorHelper.IMGUI.CentreLabel("Sunrise Wang");
			EditorHelper.IMGUI.CentreLabel("Muano Mainganye");
			EditorHelper.IMGUI.CentreLabel("Shane Marks");
			GUILayout.Space(8f);
			EditorHelper.IMGUI.CentreLabel("Graphics", EditorStyles.boldLabel);
			GUILayout.Space(8f);
			EditorHelper.IMGUI.CentreLabel("Jeff Rusch");
			EditorHelper.IMGUI.CentreLabel("Luke Godward");

			GUILayout.Space(32f);
		}
	}
}