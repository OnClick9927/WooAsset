#define AVPROVIDEO_SUPPORT_LIVEEDITMODE
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// <summary>
	/// Editor for the MediaPlayer component
	/// </summary>
	[CanEditMultipleObjects]
	[CustomEditor(typeof(MediaPlayer))]
	public partial class MediaPlayerEditor : UnityEditor.Editor
	{
		internal const string SettingsPrefix = "AVProVideo-MediaPlayerEditor-";

		private SerializedProperty _propAutoOpen;
		private SerializedProperty _propAutoStart;
		private SerializedProperty _propLoop;
		private SerializedProperty _propRate;
		private SerializedProperty _propPersistent;
		private SerializedProperty _propFilter;
		private SerializedProperty _propWrap;
		private SerializedProperty _propAniso;
#if AVPRO_FEATURE_VIDEORESOLVE
		private SerializedProperty _propUseVideoResolve;
		private SerializedProperty _propVideoResolve;
		private SerializedProperty _propVideoResolveOptions;
#endif
		private SerializedProperty _propResample;
		private SerializedProperty _propResampleMode;
		private SerializedProperty _propResampleBufferSize;
		private SerializedProperty _propVideoMapping;
		private SerializedProperty _propForceFileFormat;
		private SerializedProperty _propFallbackMediaHints;

		private static Texture2D _icon;
		private static bool _isTrialVersion = false;
		private static GUIStyle _styleSectionBox = null;

		private AnimCollapseSection _sectionMediaInfo;
		private AnimCollapseSection _sectionDebug;
		private AnimCollapseSection _sectionSettings;
		private AnimCollapseSection _sectionAboutHelp;
		private List<AnimCollapseSection> _settingSections = new List<AnimCollapseSection>(16);
		private List<AnimCollapseSection> _platformSections = new List<AnimCollapseSection>(8);

		[MenuItem("GameObject/Video/AVPro Video - Media Player", false, 100)]
		public static void CreateMediaPlayerEditor()
		{
			GameObject go = new GameObject("MediaPlayer");
			go.AddComponent<MediaPlayer>();
			Selection.activeGameObject = go;
		}

		[MenuItem("GameObject/Video/AVPro Video - Media Player with Unity Audio", false, 101)]
		public static void CreateMediaPlayerWithUnityAudioEditor()
		{
			GameObject go = new GameObject("MediaPlayer");
			go.AddComponent<MediaPlayer>();
			go.AddComponent<AudioSource>();
			AudioOutput ao = go.AddComponent<AudioOutput>();
			// Move the AudioOutput component above the AudioSource so that it acts as the audio generator
			UnityEditorInternal.ComponentUtility.MoveComponentUp(ao);
			Selection.activeGameObject = go;
		}

		private static void LoadSettings()
		{
			_platformIndex = EditorPrefs.GetInt(SettingsPrefix + "PlatformIndex", -1);
			_showAlpha = EditorPrefs.GetBool(SettingsPrefix + "ShowAlphaChannel", false);
			_showPreview = EditorPrefs.GetBool(SettingsPrefix + "ShowPreview", true);
			_allowDeveloperMode = EditorPrefs.GetBool(SettingsPrefix + "AllowDeveloperMode", false);
			_HTTPHeadersToggle = EditorPrefs.GetBool(SettingsPrefix + "HTTPHeadersToggle", false);
			RecentItems.Load();
		}

		private void SaveSettings()
		{
			_sectionMediaInfo.Save();
			_sectionDebug.Save();
			_sectionSettings.Save();
			_sectionAboutHelp.Save();

			foreach (AnimCollapseSection section in _settingSections)
			{
				section.Save();
			}
			foreach (AnimCollapseSection section in _platformSections)
			{
				section.Save();
			}

			_sectionDevModeState.Save();
			_sectionDevModeTexture.Save();
			_sectionDevModePlaybackQuality.Save();
			_sectionDevModeHapNotchLCDecoder.Save();
			_sectionDevModeBufferedFrames.Save();

			EditorPrefs.SetInt(SettingsPrefix + "PlatformIndex", _platformIndex);
			EditorPrefs.SetBool(SettingsPrefix + "ShowAlphaChannel", _showAlpha);
			EditorPrefs.SetBool(SettingsPrefix + "ShowPreview", _showPreview);
			EditorPrefs.SetBool(SettingsPrefix + "AllowDeveloperMode", _allowDeveloperMode);
			EditorPrefs.SetBool(SettingsPrefix + "HTTPHeadersToggle", _HTTPHeadersToggle);
			RecentItems.Save();
		}

		//[MenuItem("RenderHeads/AVPro Video/Reset Settings", false, 101)]
		internal static void DeleteSettings()
		{
			EditorPrefs.DeleteKey(SettingsPrefix + "PlatformIndex");
			EditorPrefs.DeleteKey(SettingsPrefix + "ShowAlphaChannel");
			EditorPrefs.DeleteKey(SettingsPrefix + "AllowDeveloperMode");
			EditorPrefs.DeleteKey(SettingsPrefix + "HTTPHeadersToggle");

			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName("Media"));
			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName("Debug"));
			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName("Settings"));
			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName("About / Help"));
			
			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName("Source"));
			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName("Main"));
			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName("Audio"));
			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName("Visual"));
			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName("Network"));
			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName("Media"));
			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName("Subtitles"));
			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName("Events"));
			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName("Platform Specific"));
			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName("Global"));

			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName(GetPlatformButtonContent(Platform.Windows).text));
			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName(GetPlatformButtonContent(Platform.MacOSX).text));
			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName(GetPlatformButtonContent(Platform.Android).text));
			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName(GetPlatformButtonContent(Platform.iOS).text));
			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName(GetPlatformButtonContent(Platform.tvOS).text));
			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName(GetPlatformButtonContent(Platform.WindowsUWP).text));
			EditorPrefs.DeleteKey(AnimCollapseSection.GetPrefName(GetPlatformButtonContent(Platform.WebGL).text));
		}

		private void CreateSections()
		{
			const float colorSaturation = 0.66f;
			Color mediaInfoColor = Color.HSVToRGB(0.55f, colorSaturation, 1f);
			Color sourceColor = Color.HSVToRGB(0.4f, colorSaturation, 1f);
			Color platformSpecificColor = Color.HSVToRGB(0.85f, colorSaturation, 1f);
			Color platformColor = platformSpecificColor;
			if (EditorGUIUtility.isProSkin)
			{ 
				platformColor *= 0.66f;
			}

			_sectionMediaInfo = new AnimCollapseSection("Media Info", false, false, OnInspectorGUI_MediaInfo, this, mediaInfoColor);
			_sectionDebug = new AnimCollapseSection("Debug", false, true, OnInspectorGUI_Debug, this, Color.white);
			_sectionSettings = new AnimCollapseSection("Settings", false, true, OnInspectorGUI_Settings, this, Color.white);
			_sectionAboutHelp = new AnimCollapseSection("About / Help", false, false, OnInspectorGUI_About, this, Color.white);

			_settingSections.Clear();
			_settingSections.Add(new AnimCollapseSection("Source", false, true, OnInspectorGUI_Source, this, sourceColor));
			_settingSections.Add(new AnimCollapseSection("Main", false, false, OnInspectorGUI_Main, this, Color.white));
			_settingSections.Add(new AnimCollapseSection("Audio", false, false, OnInspectorGUI_Audio, this, Color.white));
			_settingSections.Add(new AnimCollapseSection("Visual", true, false, OnInspectorGUI_Visual, this, Color.white));
			//_settingSections.Add(new AnimCollapseSection("Network", true, false, OnInspectorGUI_Network, this, Color.white));
			_settingSections.Add(new AnimCollapseSection("Subtitles", true, false, OnInspectorGUI_Subtitles, this, Color.white));
			_settingSections.Add(new AnimCollapseSection("Events", true, false, OnInspectorGUI_Events, this, Color.white));
			_settingSections.Add(new AnimCollapseSection("Platform Specific", true, false, OnInspectorGUI_PlatformOverrides, this, platformSpecificColor));
			_settingSections.Add(new AnimCollapseSection("Global", true, false, OnInspectorGUI_GlobalSettings, this, Color.white));

			_platformSections.Clear();
			_platformSections.Add(new AnimCollapseSection(GetPlatformButtonContent(Platform.Windows), true, false, OnInspectorGUI_Override_Windows, this, platformColor, _platformSections));
			_platformSections.Add(new AnimCollapseSection(GetPlatformButtonContent(Platform.MacOSX), true, false, OnInspectorGUI_Override_MacOSX, this, platformColor, _platformSections));
			_platformSections.Add(new AnimCollapseSection(GetPlatformButtonContent(Platform.Android), true, false, OnInspectorGUI_Override_Android, this, platformColor, _platformSections));
			_platformSections.Add(new AnimCollapseSection(GetPlatformButtonContent(Platform.iOS), true, false, OnInspectorGUI_Override_iOS, this, platformColor, _platformSections));
			_platformSections.Add(new AnimCollapseSection(GetPlatformButtonContent(Platform.tvOS), true, false, OnInspectorGUI_Override_tvOS, this, platformColor, _platformSections));
			_platformSections.Add(new AnimCollapseSection(GetPlatformButtonContent(Platform.WindowsUWP), true, false, OnInspectorGUI_Override_WindowsUWP, this, platformColor, _platformSections));
			_platformSections.Add(new AnimCollapseSection(GetPlatformButtonContent(Platform.WebGL), true, false, OnInspectorGUI_Override_WebGL, this, platformColor, _platformSections));

			_sectionDevModeState = new AnimCollapseSection("State", false, false, OnInspectorGUI_DevMode_State, this, Color.white);
			_sectionDevModeTexture = new AnimCollapseSection("Texture", false, false, OnInspectorGUI_DevMode_Texture, this, Color.white);
			_sectionDevModePlaybackQuality = new AnimCollapseSection("Presentation Quality", false, false, OnInspectorGUI_DevMode_PresentationQuality, this, Color.white);
			_sectionDevModeHapNotchLCDecoder = new AnimCollapseSection("Hap/NotchLC Decoder", false, false, OnInspectorGUI_DevMode_HapNotchLCDecoder, this, Color.white);
			_sectionDevModeBufferedFrames = new AnimCollapseSection("Buffers", false, false, OnInspectorGUI_DevMode_BufferedFrames, this, Color.white);
		}

		private void ResolveProperties()
		{
			_propMediaSource = this.CheckFindProperty("_mediaSource");
			_propMediaReference = this.CheckFindProperty("_mediaReference");
			_propMediaPath = this.CheckFindProperty("_mediaPath");
			_propAutoOpen = this.CheckFindProperty("_autoOpen");
			_propAutoStart = this.CheckFindProperty("_autoPlayOnStart");
			_propLoop = this.CheckFindProperty("_loop");
			_propRate = this.CheckFindProperty("_playbackRate");
			_propVolume = this.CheckFindProperty("_audioVolume");
			_propBalance = this.CheckFindProperty("_audioBalance");
			_propMuted = this.CheckFindProperty("_audioMuted");
			_propPersistent = this.CheckFindProperty("_persistent");
			_propEvents = this.CheckFindProperty("_events");
			_propEventMask = this.CheckFindProperty("_eventMask");
			_propPauseMediaOnAppPause = this.CheckFindProperty("_pauseMediaOnAppPause");
			_propPlayMediaOnAppUnpause = this.CheckFindProperty("_playMediaOnAppUnpause");
			_propFilter = this.CheckFindProperty("_textureFilterMode");
			_propWrap = this.CheckFindProperty("_textureWrapMode");
			_propAniso = this.CheckFindProperty("_textureAnisoLevel");
#if AVPRO_FEATURE_VIDEORESOLVE
			_propUseVideoResolve = this.CheckFindProperty("_useVideoResolve");
			_propVideoResolve = this.CheckFindProperty("_videoResolve");
			_propVideoResolveOptions = this.CheckFindProperty("_videoResolveOptions");
#endif
			_propVideoMapping = this.CheckFindProperty("_videoMapping");
			_propForceFileFormat = this.CheckFindProperty("_forceFileFormat");
			_propFallbackMediaHints = this.CheckFindProperty("_fallbackMediaHints");
			_propSubtitles = this.CheckFindProperty("_sideloadSubtitles");
			_propSubtitlePath = this.CheckFindProperty("_subtitlePath");
			_propResample = this.CheckFindProperty("_useResampler");
			_propResampleMode = this.CheckFindProperty("_resampleMode");
			_propResampleBufferSize = this.CheckFindProperty("_resampleBufferSize");
			_propAudioHeadTransform = this.CheckFindProperty("_audioHeadTransform");
			_propAudioEnableFocus = this.CheckFindProperty("_audioFocusEnabled");
			_propAudioFocusOffLevelDB = this.CheckFindProperty("_audioFocusOffLevelDB");
			_propAudioFocusWidthDegrees = this.CheckFindProperty("_audioFocusWidthDegrees");
			_propAudioFocusTransform = this.CheckFindProperty("_audioFocusTransform");
		}

		private static Texture GetPlatformIcon(Platform platform)
		{
			string iconName = string.Empty;
			switch (platform)
			{
				case Platform.Windows:
				case Platform.MacOSX:
					iconName = "BuildSettings.Standalone.Small";
					break;
				case Platform.Android:
					iconName = "BuildSettings.Android.Small";
					break;
				case Platform.iOS:
					iconName = "BuildSettings.iPhone.Small";
					break;
				case Platform.tvOS:
					iconName = "BuildSettings.tvOS.Small";
					break;
				case Platform.WindowsUWP:
					iconName = "BuildSettings.Metro.Small";
					break;
				case Platform.WebGL:
					iconName = "BuildSettings.WebGL.Small";
					break;
			}
			Texture iconTexture = null;
			if (!string.IsNullOrEmpty(iconName))
			{
				iconTexture = EditorGUIUtility.IconContent(iconName).image;
			}
			return iconTexture;
		}

		private static GUIContent GetPlatformButtonContent(Platform platform)
		{
			return new GUIContent(Helper.GetPlatformName(platform), GetPlatformIcon(platform));
		}

		private void FixRogueEditorBug()
		{
			// NOTE: There seems to be a bug in Unity where the editor script will call OnEnable and OnDisable twice.
			// This is resolved by setting the Window Layout mode to Default.
			// It causes a problem (at least in Unity 2020.1.11) where the System.Action invocations (usd by AnimCollapseSection)
			// seem to be in a different 'this' context and so their pointers to serializedObject is not the same, resulting in 
			// properties modified not marking the serialisedObject as dirty.  To get around this issue we use this static bool
			// so that OnEnable can only be called once.
			// https://answers.unity.com/questions/1216599/custom-editor-gets-created-multiple-times-and-rece.html
			var remainingBuggedEditors  = FindObjectsOfType<MediaPlayerEditor>();
			foreach(var editor in remainingBuggedEditors)
			{
				if (editor == this)
				{
					continue;
				}
				DestroyImmediate(editor);
			}
		}

		private void OnEnable()
		{
			FixRogueEditorBug();

			CreateSections();

			LoadSettings();

			_isTrialVersion = IsTrialVersion();

			if (_platformNames == null)
			{
				_platformNames = new GUIContent[]
				{
					GetPlatformButtonContent(Platform.Windows),
					GetPlatformButtonContent(Platform.MacOSX),
					GetPlatformButtonContent(Platform.iOS),
					GetPlatformButtonContent(Platform.tvOS),
					GetPlatformButtonContent(Platform.Android),
					GetPlatformButtonContent(Platform.WindowsUWP),
					GetPlatformButtonContent(Platform.WebGL)
				};
			}

			ResolveProperties();
		}

		private void OnDisable()
		{
			ClosePreview();
			SaveSettings();

			if (!Application.isPlaying)
			{
				// NOTE: For some reason when transitioning into Play mode, Dispose() is not called in the MediaPlayer if
				// it was playing before the transition because all members are reset to null.  So we must force this
				// dispose of all resources to handle this case.
				// Sadly it means we can't keep persistent playback in the inspector when it loses focus, but
				// hopefully we can find a way to achieve this in the future
				/*if (EditorApplication.isPlayingOrWillChangePlaymode)
				{
					// NOTE: This seems to work for the above issue, but
					// we'd need to move it to the global event for when the play state changes
					MediaPlayer.EditorAllPlayersDispose();
				}*/
				foreach (MediaPlayer player in this.targets)
				{
					player.ForceDispose();
				}
			}
		}

		private void CreateStyles()
		{
			if (_styleSectionBox == null)
			{
				_styleSectionBox = new GUIStyle(GUI.skin.box);
				if (!EditorGUIUtility.isProSkin)
				{
					_styleSectionBox = new GUIStyle(GUI.skin.box);
					//_styleSectionBox.normal.background = Texture2D.redTexture;
				}
			}

			_iconPlayButton = EditorGUIUtility.IconContent("d_PlayButton");
			_iconPauseButton = EditorGUIUtility.IconContent("d_PauseButton");
			_iconSceneViewAudio = EditorGUIUtility.IconContent("d_SceneViewAudio");
			_iconProject = EditorGUIUtility.IconContent("d_Project");
			_iconRotateTool = EditorGUIUtility.IconContent("d_RotateTool");

			AnimCollapseSection.CreateStyles();
		}

		public override void OnInspectorGUI()
		{
			MediaPlayer media = (this.target) as MediaPlayer;

			// NOTE: It is important that serializedObject.Update() is called before media.EditorUpdate()
			// as otherwise the serializedPropertys are not correctly detected as modified
			serializedObject.Update();

#if AVPROVIDEO_SUPPORT_LIVEEDITMODE
			bool isPlayingInEditor = false;
			// Update only during the layout event so that nothing updates for the render event
			if (!Application.isPlaying && Event.current.type == EventType.Layout)
			{
				isPlayingInEditor = media.EditorUpdate();
			}
#endif

			if (media == null || _propMediaPath == null)
			{
				return;
			}

			CreateStyles();

			_icon = GetIcon(_icon);

			ShowImportantMessages();

			if (media != null)
			{
				OnInspectorGUI_Player(media, media.TextureProducer);
			}

			AnimCollapseSection.Show(_sectionMediaInfo);
			if (_allowDeveloperMode)
			{
				AnimCollapseSection.Show(_sectionDebug);
			}
			AnimCollapseSection.Show(_sectionSettings);

			if (serializedObject.ApplyModifiedProperties())
			{
				EditorUtility.SetDirty(target);
			}

			AnimCollapseSection.Show(_sectionAboutHelp);
#if AVPROVIDEO_SUPPORT_LIVEEDITMODE
			if (isPlayingInEditor)
			{
				GL.InvalidateState();
				// NOTE: there seems to be a bug in Unity (2019.3.13) where if you don't have 
				// GL.sRGBWrite = true and then call RepaintAllViews() it makes the current Inspector
				// background turn black.  This only happens when using D3D12
				// UPDATE: this is happening in Unity 2019.4.15 as well, and in D3D11 mode.  It only
				// happens when loading a video via the Recent Menu.

				bool originalSRGBWrite = GL.sRGBWrite;
				GL.sRGBWrite = true;
				//this.Repaint();
				UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
				GL.sRGBWrite = originalSRGBWrite;
			}
			// TODO: OnDisable - stop the video if it's playing (and unload it?)
#endif
		}

		private void OnInspectorGUI_Settings()
		{
			foreach (AnimCollapseSection section in _settingSections)
			{
				AnimCollapseSection.Show(section, indentLevel:1);
			}
		}

		private void ShowSupportWindowButton()
		{
			//GUI.backgroundColor = new Color(0.96f, 0.25f, 0.47f);
			//if (GUILayout.Button("◄ AVPro Video ►\nHelp & Support"))
			if (GUILayout.Button("Click here for \nHelp & Support"))
			{
				SupportWindow.Init();
			}
			//GUI.backgroundColor = Color.white;
		}

		private void ShowImportantMessages()
		{
			// Describe the watermark for trial version
			if (_isTrialVersion)
			{
				string message = string.Empty;
				if (Application.isPlaying)
				{
				#if UNITY_EDITOR_WIN
					MediaPlayer media = (this.target) as MediaPlayer;
					message = "The watermark is the horizontal bar that moves vertically and the small 'AVPRO TRIAL' text.";
					if (media.Info != null && media.Info.GetPlayerDescription().Contains("MF-MediaEngine-Hardware"))
					{
						message = "The watermark is the RenderHeads logo that moves around the image.";
					}
				#elif UNITY_EDITOR_OSX
					message = "The RenderHeads logo is the watermark.";
				#endif
				}

				EditorHelper.IMGUI.BeginWarningTextBox("AVPRO VIDEO - TRIAL WATERMARK", message, Color.yellow, Color.yellow, Color.white);
				if (GUILayout.Button("Purchase"))
				{
					Application.OpenURL(LinkPurchase);
				}
				EditorHelper.IMGUI.EndWarningTextBox();
			}

			// Warning about not using multi-threaded rendering
			{
				bool showWarningMT = false;

				if (/*EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.iOS ||
					EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.tvOS ||*/
					EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Android)
				{
#if UNITY_2017_2_OR_NEWER
					showWarningMT = !UnityEditor.PlayerSettings.GetMobileMTRendering(BuildTargetGroup.Android);
#else
					showWarningMT = !UnityEditor.PlayerSettings.mobileMTRendering;
#endif
				}
				/*if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.WSA)
				{
				}*/
				if (showWarningMT)
				{
					EditorHelper.IMGUI.WarningTextBox("Performance Warning", "Deploying to Android with multi-threaded rendering disabled is not recommended.  Enable multi-threaded rendering in the Player Settings > Other Settings panel.", Color.yellow, Color.yellow, Color.white);
				}
			}

#if !UNITY_2019_3_OR_NEWER
			if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D12)
			{
				EditorHelper.IMGUI.WarningTextBox("Compatibility Warning", "Direct3D 12 is not supported until Unity 2019.3", Color.yellow, Color.yellow, Color.white);
			}
#endif
			// Warn about using Vulkan graphics API
#if UNITY_2018_1_OR_NEWER
			{
				if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Android)
				{
					bool showWarningVulkan = false;
					if (!UnityEditor.PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android))
					{
						UnityEngine.Rendering.GraphicsDeviceType[] devices = UnityEditor.PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
						foreach (UnityEngine.Rendering.GraphicsDeviceType device in devices)
						{
							if (device == UnityEngine.Rendering.GraphicsDeviceType.Vulkan)
							{
								showWarningVulkan = true;
								break;
							}
						}
					}
					if (showWarningVulkan)
					{
						EditorHelper.IMGUI.WarningTextBox("Compatibility Warning", "Vulkan graphics API is not supported.  Please go to Player Settings > Android > Auto Graphics API and remove Vulkan from the list.  Only OpenGL 2.0 and 3.0 are supported on Android.", Color.yellow, Color.yellow, Color.white);
					}
				}
			}
#endif
		}

		private void OnInspectorGUI_Main()
		{
			/////////////////// STARTUP FIELDS

			EditorGUILayout.BeginVertical(_styleSectionBox);
			GUILayout.Label("Startup", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(_propAutoOpen);
			EditorGUILayout.PropertyField(_propAutoStart, new GUIContent("Auto Play"));
			EditorGUILayout.EndVertical();

			/////////////////// PLAYBACK FIELDS

			EditorGUILayout.BeginVertical(GUI.skin.box);
			GUILayout.Label("Playback", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_propLoop);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, "Loop");
				foreach (MediaPlayer player in this.targets)
				{
					player.Loop = _propLoop.boolValue;
				}
			}

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_propRate);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, "PlaybackRate");
				foreach (MediaPlayer player in this.targets)
				{
					player.PlaybackRate = _propRate.floatValue;
				}
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUI.skin.box);
			GUILayout.Label("Other", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(_propPersistent, new GUIContent("Persistent", "Use DontDestroyOnLoad so this object isn't destroyed between level loads"));

			if (_propForceFileFormat != null)
			{
				GUIContent label = new GUIContent("Force File Format", "Override automatic format detection when using non-standard file extensions");
				_propForceFileFormat.enumValueIndex = EditorGUILayout.Popup(label, _propForceFileFormat.enumValueIndex, _fileFormatGuiNames);
			}

			EditorGUILayout.EndVertical();
		}

		private void OnInspectorGUI_Visual()
		{
#if AVPRO_FEATURE_VIDEORESOLVE
			
			{
				EditorGUILayout.BeginVertical(GUI.skin.box);
				GUILayout.Label("Resolve", EditorStyles.boldLabel);

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(_propUseVideoResolve);
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(target, "UseVideoResolve");
					foreach (MediaPlayer player in this.targets)
					{
						player.UseVideoResolve = _propUseVideoResolve.boolValue;
					}
				}

				if (_propUseVideoResolve.boolValue)
				{
					EditorGUILayout.PropertyField(_propVideoResolve);
					/*EditorGUI.indentLevel++;
					EditorGUILayout.PropertyField(_propVideoResolveOptions, true);
					EditorGUI.indentLevel--;*/
				}

				GUILayout.EndVertical();
			}
#endif

			EditorGUILayout.BeginVertical(GUI.skin.box);
			GUILayout.Label("Texture", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_propFilter, new GUIContent("Filter"));
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, "TextureFilterMode");
				foreach (MediaPlayer player in this.targets)
				{
					player.TextureFilterMode = (FilterMode)_propFilter.enumValueIndex;
				}
			}

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_propWrap, new GUIContent("Wrap"));
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, "TextureWrapMode");
				foreach (MediaPlayer player in this.targets)
				{
					player.TextureWrapMode = (TextureWrapMode)_propWrap.enumValueIndex;
				}
			}

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_propAniso, new GUIContent("Aniso"));
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, "TextureAnisoLevel");
				foreach (MediaPlayer player in this.targets)
				{
					player.TextureAnisoLevel = _propAniso.intValue;
				}
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUI.skin.box);
			GUILayout.Label("Layout Mapping", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(_propVideoMapping);
			EditorGUILayout.EndVertical();

			{
				EditorGUILayout.BeginVertical(GUI.skin.box);
				GUILayout.Label("Resampler (BETA)", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_propResample);
				EditorGUI.BeginDisabledGroup(!_propResample.boolValue);

				EditorGUILayout.PropertyField(_propResampleMode);
				EditorGUILayout.PropertyField(_propResampleBufferSize);

				EditorGUI.EndDisabledGroup();
				EditorGUILayout.EndVertical();
			}
		}

		private static bool IsTrialVersion()
		{
			string version = GetPluginVersion();
			return version.Contains("-trial");
		}

		//private int _updateFrameCount = -1;
		public override bool RequiresConstantRepaint()
		{
			MediaPlayer media = (this.target) as MediaPlayer;
			if (media != null && media.Control != null && media.isActiveAndEnabled && media.Info.GetDuration() > 0.0)
			{
				if (!media.Info.HasVideo())
				{
					if (media.Info.HasAudio())
					{
						return true;
					}
				}
				else if (media.TextureProducer.GetTexture() != null)
				{
					//int frameCount = media.TextureProducer.GetTextureFrameCount();
					//if (_updateFrameCount != frameCount)
					{
						//_updateFrameCount = frameCount;
						return true;
					}
				}
			}
			return false;
		}
	}
}