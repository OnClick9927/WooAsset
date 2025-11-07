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
		private SerializedProperty _propSourceAudioSampleRate;
		private SerializedProperty _propSourceAudioChannels;
		private SerializedProperty _propManualSetAudioProps;

		private readonly static GUIContent[] _audioModesWindows =
		{
			new GUIContent("System Direct"),
			new GUIContent("Unity", "Allows the AudioOutput component to grab audio from the video and play it through Unity to the AudioListener"),
			new GUIContent("Facebook Audio 360", "Initialises player with Facebook Audio 360 support"),
			new GUIContent("None", "No audio"),
		};

		private readonly static GUIContent[] _audioModesUWP =
		{
			new GUIContent("System Direct"),
			new GUIContent("Unity", "Allows the AudioOutput component to grab audio from the video and play it through Unity to the AudioListener"),
			new GUIContent("Facebook Audio 360", "Initialises player with Facebook Audio 360 support"),
			new GUIContent("None", "No audio"),
		};

		private readonly static FieldDescription _optionLowLatency = new FieldDescription(".useLowLatency", new GUIContent("Use Low Latency", "Provides a hint to the decoder to use less buffering - may degrade performance and quality"));
		private readonly static FieldDescription _optionVideoAPI = new FieldDescription(".videoApi", new GUIContent("Video API", "The preferred video API to use"));
		private readonly static FieldDescription _optionTextureMips = new FieldDescription(".useTextureMips", new GUIContent("Generate Mipmaps", "Automatically create mip-maps for the texture to reducing aliasing when texture is scaled down"));
		private readonly static FieldDescription _option10BitTextures = new FieldDescription(".use10BitTextures", new GUIContent("Use 10-Bit Textures", "Provides a hint to the decoder to use 10-bit textures - allowing more quality for videos encoded with a 10-bit profile"));
		private readonly static FieldDescription _optionUseHardwareDecoding = new FieldDescription(".useHardwareDecoding", new GUIContent("Hardware Decoding"));
		private readonly static FieldDescription _optionUseStereoDetection = new FieldDescription(".useStereoDetection", new GUIContent("Use Stereo Detection", "Disable if no stereo detection is required"));
		private readonly static FieldDescription _optionUseTextTrackSupport = new FieldDescription(".useTextTrackSupport", new GUIContent("Use Text Tracks", "Disable if no text tracks are required"));
		private readonly static FieldDescription _optionUseAudioDelay = new FieldDescription(".useAudioDelay", new GUIContent("Use Audio Delay", "Allows audio to be offset"));
		private readonly static FieldDescription _optionUseFacebookAudio360Support = new FieldDescription(".useFacebookAudio360Support", new GUIContent("Use Facebook Audio 360", "Disable if no Facebook Audio 360 support is required for"));
#if AVPROVIDEO_SUPPORT_BUFFERED_DISPLAY
		private readonly static FieldDescription _optionPauseOnPrerollComplete = new FieldDescription(".pauseOnPrerollComplete", new GUIContent("Pause On Preroll Complete", "Internally pause once preroll is completed.  This is useful for syncing video playback to make sure all players are prerolled"));
		private readonly static FieldDescription _optionBufferedFrameSelection = new FieldDescription(".bufferedFrameSelection", new GUIContent("Frame Selection", "Mode for selecting the next frame to display from the buffer fo frames"));
#endif
		private readonly static FieldDescription _optionUseHapNotchLC = new FieldDescription(".useHapNotchLC", new GUIContent("Use Hap/NotchLC", "Disable if no Hap/NotchLC playback is required"));
		private readonly static FieldDescription _optionCustomMovParser = new FieldDescription(".useCustomMovParser", new GUIContent("Use Custom MOV Parser", "For playback of Hap and NotchLC media to handle high bit-rates"));
		private readonly static FieldDescription _optionParallelFrameCount = new FieldDescription(".parallelFrameCount", new GUIContent("Parallel Frame Count", "Number of frames to decode in parallel via multi-threading.  Higher values increase latency but can improve performance for demanding videos."));
		private readonly static FieldDescription _optionPrerollFrameCount = new FieldDescription(".prerollFrameCount", new GUIContent("Preroll Frame Count", "Number of frames to pre-decode before playback starts.  Higher values increase latency but can improve performance for demanding videos."));
		private readonly static FieldDescription _optionAudioOutput = new FieldDescription(".audioOutput", new GUIContent("Audio Output"));
		private readonly static FieldDescription _optionAudio360ChannelMode = new FieldDescription(".audio360ChannelMode", new GUIContent("Channel Mode", "Specifies what channel mode Facebook Audio 360 needs to be initialised with"));
		private readonly static FieldDescription _optionStartMaxBitrate = new FieldDescription(".startWithHighestBitrate", new GUIContent("Start Max Bitrate"));
		private readonly static FieldDescription _optionUseLowLiveLatency = new FieldDescription(".useLowLiveLatency", new GUIContent("Low Live Latency"));
		private readonly static FieldDescription _optionHintAlphaChannel = new FieldDescription(".hintAlphaChannel", new GUIContent("Alpha Channel Hint", "If a video is detected as 32-bit, use or ignore the alpha channel"));
		private readonly static FieldDescription _optionForceAudioOutputDeviceName = new FieldDescription(".forceAudioOutputDeviceName", new GUIContent("Force Audio Output Device Name", "Useful for VR when you need to output to the VR audio device"));
		private readonly static FieldDescription _optionPreferredFilters = new FieldDescription(".preferredFilters", new GUIContent("Preferred Filters", "Priority list for preferred filters to be used instead of default"));

		private void OnInspectorGUI_Override_Windows()
		{
			//MediaPlayer media = (this.target) as MediaPlayer;
			//MediaPlayer.OptionsWindows options = media._optionsWindows;

			GUILayout.Space(8f);

			EditorGUILayout.BeginVertical(GUI.skin.box);
			string optionsVarName = MediaPlayer.GetPlatformOptionsVariable(Platform.Windows);

			{
				SerializedProperty propVideoApi = DisplayPlatformOption(optionsVarName, _optionVideoAPI);
				{
					SerializedProperty propUseTextureMips = DisplayPlatformOption(optionsVarName, _optionTextureMips);
					if (propUseTextureMips.boolValue && ((FilterMode)_propFilter.enumValueIndex) != FilterMode.Trilinear)
					{
						EditorHelper.IMGUI.NoticeBox(MessageType.Info, "Recommend changing the texture filtering mode to Trilinear when using mip-maps.");
					}
				}
				{
					SerializedProperty propUseHardwareDecoding = serializedObject.FindProperty(optionsVarName + _optionUseHardwareDecoding.fieldName);
					EditorGUI.BeginDisabledGroup(!propUseHardwareDecoding.boolValue && propVideoApi.enumValueIndex == (int)Windows.VideoApi.MediaFoundation);
					{
						DisplayPlatformOption(optionsVarName, _option10BitTextures);
					}
					EditorGUI.EndDisabledGroup();
				}
			}
			EditorGUILayout.EndVertical();

			// Media Foundation Options
			{
				EditorGUILayout.BeginVertical(GUI.skin.box);
				GUILayout.Label("Media Foundation API Options", EditorStyles.boldLabel);
				{
					DisplayPlatformOption(optionsVarName, _optionUseHardwareDecoding);
				}
				{
					DisplayPlatformOption(optionsVarName, _optionLowLatency);
					DisplayPlatformOption(optionsVarName, _optionUseStereoDetection);
					DisplayPlatformOption(optionsVarName, _optionUseTextTrackSupport);
#if AVPROVIDEO_SUPPORT_BUFFERED_DISPLAY
					if (_showUltraOptions)
					{
						SerializedProperty propBufferedFrameSelection = DisplayPlatformOption(optionsVarName, _optionBufferedFrameSelection);
						if (propBufferedFrameSelection.enumValueIndex != (int)BufferedFrameSelectionMode.None)
						{
							EditorGUI.indentLevel++;
							DisplayPlatformOption(optionsVarName, _optionPauseOnPrerollComplete);
							EditorGUI.indentLevel--;
						}
					}
#endif
					if (_showUltraOptions)
					{
						SerializedProperty useHapNotchLC = DisplayPlatformOption(optionsVarName, _optionUseHapNotchLC);
						if (useHapNotchLC.boolValue)
						{
							EditorGUI.indentLevel++;
							DisplayPlatformOption(optionsVarName, _optionCustomMovParser);
							DisplayPlatformOption(optionsVarName, _optionParallelFrameCount);
							DisplayPlatformOption(optionsVarName, _optionPrerollFrameCount);
							EditorGUI.indentLevel--;
						}
					}
				}
				// Audio Output
				{
					SerializedProperty propAudioDelay = DisplayPlatformOption(optionsVarName, _optionUseAudioDelay);
					if (propAudioDelay.boolValue)
					{
						//EditorGUI.indentLevel++;
						//EditorGUI.indentLevel--;
					}
					DisplayPlatformOption(optionsVarName, _optionUseFacebookAudio360Support);
					SerializedProperty propAudioOutput = DisplayPlatformOptionEnum(optionsVarName, _optionAudioOutput, _audioModesWindows);
					if (_showUltraOptions && (Windows.AudioOutput)propAudioOutput.enumValueIndex == Windows.AudioOutput.FacebookAudio360)
					{
						EditorGUILayout.Space();
						EditorGUILayout.LabelField("Facebook Audio 360", EditorStyles.boldLabel);
					
						DisplayPlatformOptionEnum(optionsVarName, _optionAudio360ChannelMode, _audio360ChannelMapGuiNames);

						{
							SerializedProperty propForceAudioOutputDeviceName = serializedObject.FindProperty(optionsVarName + ".forceAudioOutputDeviceName");
							if (propForceAudioOutputDeviceName != null)
							{
								string[] deviceNames = { "Default", Windows.AudioDeviceOutputName_Rift, Windows.AudioDeviceOutputName_Vive, "Custom" };
								int index = 0;
								if (!string.IsNullOrEmpty(propForceAudioOutputDeviceName.stringValue))
								{
									switch (propForceAudioOutputDeviceName.stringValue)
									{
										case Windows.AudioDeviceOutputName_Rift:
											index = 1;
											break;
										case Windows.AudioDeviceOutputName_Vive:
											index = 2;
											break;
										default:
											index = 3;
											break;
									}
								}
								int newIndex = EditorGUILayout.Popup("Audio Device Name", index, deviceNames);
								if (newIndex == 0)
								{
									propForceAudioOutputDeviceName.stringValue = string.Empty;
								}
								else if (newIndex == 3)
								{
									if (index != newIndex)
									{
										if (string.IsNullOrEmpty(propForceAudioOutputDeviceName.stringValue) ||
												propForceAudioOutputDeviceName.stringValue == Windows.AudioDeviceOutputName_Rift ||
												propForceAudioOutputDeviceName.stringValue == Windows.AudioDeviceOutputName_Vive)
										{
											propForceAudioOutputDeviceName.stringValue = "?";
										}
									}
									EditorGUILayout.PropertyField(propForceAudioOutputDeviceName, new GUIContent("Audio Device Name", "Useful for VR when you need to output to the VR audio device"));
								}
								else
								{
									propForceAudioOutputDeviceName.stringValue = deviceNames[newIndex];
								}
							}
						}
					}
				}
				EditorGUILayout.EndVertical();
			}

			// WinRT Options
			{
				EditorGUILayout.BeginVertical(GUI.skin.box);
				GUILayout.Label("WinRT API Options", EditorStyles.boldLabel);
				DisplayPlatformOption(optionsVarName, _optionStartMaxBitrate);
				DisplayPlatformOption(optionsVarName, _optionUseLowLiveLatency);
				if (_showUltraOptions)
				{
					SerializedProperty httpHeadersProp = serializedObject.FindProperty(optionsVarName + ".httpHeaders.httpHeaders");
					if (httpHeadersProp != null)
					{
						OnInspectorGUI_HttpHeaders(httpHeadersProp);
					}
					GUILayout.Space(8f);
					SerializedProperty keyAuthProp = serializedObject.FindProperty(optionsVarName + ".keyAuth");
					if (keyAuthProp != null)
					{
						OnInspectorGUI_HlsDecryption(keyAuthProp);
					}
				}
				EditorGUILayout.EndVertical();
			}

			// DirectShow Options
			{
				EditorGUILayout.BeginVertical(GUI.skin.box);
				GUILayout.Label("DirectShow API Options", EditorStyles.boldLabel);

				DisplayPlatformOption(optionsVarName, _optionHintAlphaChannel);
				DisplayPlatformOption(optionsVarName, _optionForceAudioOutputDeviceName);
				
				{
					int prevIndentLevel = EditorGUI.indentLevel;
					EditorGUI.indentLevel = 1;
					SerializedProperty propPreferredFilter = DisplayPlatformOption(optionsVarName, _optionPreferredFilters);
					if (propPreferredFilter.arraySize > 0)
					{
						EditorHelper.IMGUI.NoticeBox(MessageType.Info, "Command filter names are:\n1) \"Microsoft DTV-DVD Video Decoder\" (best for compatibility when playing H.264 videos)\n2) \"LAV Video Decoder\"\n3) \"LAV Audio Decoder\"");
					}
					EditorGUI.indentLevel = prevIndentLevel;
				}
				EditorGUILayout.EndVertical();
			}
		}


		private void OnInspectorGUI_Override_WindowsUWP()
		{
			//MediaPlayer media = (this.target) as MediaPlayer;
			//MediaPlayer.OptionsWindowsUWP options = media._optionsWindowsUWP;

			GUILayout.Space(8f);

			string optionsVarName = MediaPlayer.GetPlatformOptionsVariable(Platform.WindowsUWP);

			EditorGUILayout.BeginVertical(GUI.skin.box);

			if (_showUltraOptions)
			{
				SerializedProperty propVideoApi = DisplayPlatformOption(optionsVarName, _optionVideoAPI);
				{
					SerializedProperty propUseHardwareDecoding = serializedObject.FindProperty(optionsVarName + _optionUseHardwareDecoding.fieldName);
					EditorGUI.BeginDisabledGroup(!propUseHardwareDecoding.boolValue && propVideoApi.enumValueIndex == (int)Windows.VideoApi.MediaFoundation);
					{
						DisplayPlatformOption(optionsVarName, _option10BitTextures);
					}
					EditorGUI.EndDisabledGroup();
				}
			}

			EditorGUILayout.EndVertical();

			// Media Foundation Options
			{
				EditorGUILayout.BeginVertical(GUI.skin.box);
				GUILayout.Label("Media Foundation API Options", EditorStyles.boldLabel);

				DisplayPlatformOption(optionsVarName, _optionUseHardwareDecoding);
				
				{
					SerializedProperty propUseTextureMips = DisplayPlatformOption(optionsVarName, _optionTextureMips);
					if (propUseTextureMips.boolValue && ((FilterMode)_propFilter.enumValueIndex) != FilterMode.Trilinear)
					{
						EditorHelper.IMGUI.NoticeBox(MessageType.Info, "Recommend changing the texture filtering mode to Trilinear when using mip-maps.");
					}
				}

				DisplayPlatformOption(optionsVarName, _optionLowLatency);

				DisplayPlatformOptionEnum(optionsVarName, _optionAudioOutput, _audioModesUWP);

				EditorGUILayout.EndVertical();
			}

			// WinRT Options
			{
				EditorGUILayout.BeginVertical(GUI.skin.box);
				GUILayout.Label("WinRT API Options", EditorStyles.boldLabel);
				
				DisplayPlatformOption(optionsVarName, _optionStartMaxBitrate);
				DisplayPlatformOption(optionsVarName, _optionUseLowLiveLatency);

				if (_showUltraOptions)
				{
					{
						SerializedProperty httpHeadersProp = serializedObject.FindProperty(optionsVarName + ".httpHeaders.httpHeaders");
						if (httpHeadersProp != null)
						{
							OnInspectorGUI_HttpHeaders(httpHeadersProp);
						}
					}

					{
						SerializedProperty keyAuthProp = serializedObject.FindProperty(optionsVarName + ".keyAuth");
						if (keyAuthProp != null)
						{
							OnInspectorGUI_HlsDecryption(keyAuthProp);
						}
					}
				}
				EditorGUILayout.EndVertical();
			}

			GUI.enabled = true;
		}
	}
}