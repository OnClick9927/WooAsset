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
	public partial class MediaPlayerEditor : UnityEditor.Editor
	{
#if UNITY_EDITOR_OSX
		internal const string MediaFileExtensions = "mp4,m4v,mov,mpg,avi,mp3,m4a,aac,ac3,au,aiff,caf,wav,m3u8";
#else
		internal const string MediaFileExtensions = "Media Files;*.mp4;*.mov;*.m4v;*.avi;*.mkv;*.ts;*.webm;*.flv;*.vob;*.ogg;*.ogv;*.mpg;*.wmv;*.3gp;Audio Files;*wav;*.mp3;*.mp2;*.m4a;*.wma;*.aac;*.au;*.flac;*.m3u8;*.mpd;*.ism;";
#endif

		private readonly static GUIContent[] _fileFormatGuiNames =
		{
			new GUIContent("Automatic (by extension)"),
			new GUIContent("Apple HLS (.m3u8)"),
			new GUIContent("MPEG-DASH (.mdp)"),
			new GUIContent("MS Smooth Streaming (.ism)"),
		};

		private SerializedProperty _propMediaSource;
		private SerializedProperty _propMediaReference;
		private SerializedProperty _propMediaPath;

		private void OnInspectorGUI_Source()
		{
			// Display the file name and buttons to load new files
			MediaPlayer mediaPlayer = (this.target) as MediaPlayer;

			EditorGUILayout.PropertyField(_propMediaSource);

			if (MediaSource.Reference == (MediaSource)_propMediaSource.enumValueIndex)
			{
				EditorGUILayout.PropertyField(_propMediaReference);
			}

			EditorGUILayout.BeginVertical(GUI.skin.box);

			if (MediaSource.Reference != (MediaSource)_propMediaSource.enumValueIndex)
			{
				OnInspectorGUI_CopyableFilename(mediaPlayer.MediaPath.Path);
				EditorGUILayout.PropertyField(_propMediaPath);
			}

			//if (!Application.isPlaying)
			{
				GUI.color = Color.white;
				GUILayout.BeginHorizontal();

				if (_allowDeveloperMode)
				{
					if (GUILayout.Button("Rewind"))
					{
						mediaPlayer.Rewind(true);
					}
					if (GUILayout.Button("Preroll"))
					{
						mediaPlayer.RewindPrerollPause();
					}
					if (GUILayout.Button("End"))
					{
						mediaPlayer.Control.Seek(mediaPlayer.Info.GetDuration());
					}
				}
				if (GUILayout.Button("Close"))
				{
					mediaPlayer.CloseMedia();
				}
				if (GUILayout.Button("Load"))
				{
					if (mediaPlayer.MediaSource == MediaSource.Path)
					{
						mediaPlayer.OpenMedia(mediaPlayer.MediaPath.PathType, mediaPlayer.MediaPath.Path, mediaPlayer.AutoStart);
					}
					else if (mediaPlayer.MediaSource == MediaSource.Reference)
					{
						mediaPlayer.OpenMedia(mediaPlayer.MediaReference, mediaPlayer.AutoStart);
					}
				}
				/*if (media.Control != null)
				{
					if (GUILayout.Button("Unload"))
					{
						media.CloseVideo();
					}
				}*/

				if (EditorGUIUtility.GetObjectPickerControlID() == 100 &&
					Event.current.commandName == "ObjectSelectorClosed")
				{
					MediaReference mediaRef = (MediaReference)EditorGUIUtility.GetObjectPickerObject();
					if (mediaRef)
					{
						_propMediaSource.enumValueIndex = (int)MediaSource.Reference;
						_propMediaReference.objectReferenceValue = mediaRef;
					}
				}

				GUI.color = Color.green;
				MediaPathDrawer.ShowBrowseButtonIcon(_propMediaPath, _propMediaSource);
				GUI.color = Color.white;

				GUILayout.EndHorizontal();

				//MediaPath mediaPath = new MediaPath(_propMediaPath.FindPropertyRelative("_path").stringValue, (MediaPathType)_propMediaPath.FindPropertyRelative("_pathType").enumValueIndex);
				//ShowFileWarningMessages((MediaSource)_propMediaSource.enumValueIndex, mediaPath, (MediaReference)_propMediaReference.objectReferenceValue, mediaPlayer.AutoOpen, Platform.Unknown);
				GUI.color = Color.white;
			}

			if (MediaSource.Reference != (MediaSource)_propMediaSource.enumValueIndex)
			{
				GUILayout.Label("Fallback Media Hints", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_propFallbackMediaHints);
			}

			EditorGUILayout.EndVertical();
		}

		internal static void OnInspectorGUI_CopyableFilename(string path)
		{
			if (EditorGUIUtility.isProSkin)
			{
				GUI.backgroundColor = Color.black;
				GUI.color = Color.cyan;
			}

			EditorHelper.IMGUI.CopyableFilename(path);

			GUI.color = Color.white;
			GUI.backgroundColor = Color.white;
		}

		internal static void ShowFileWarningMessages(MediaSource mediaSource, MediaPath mediaPath, MediaReference mediaReference, bool isAutoOpen, Platform platform)
		{
			MediaPath result = null;

			if (mediaSource == MediaSource.Path)
			{
				if (mediaPath != null)
				{
					result = mediaPath;
				}
			}
			else if (mediaSource == MediaSource.Reference)
			{
				if (mediaReference != null)
				{
					result = mediaReference.GetCurrentPlatformMediaReference().MediaPath;
				}
			}

			ShowFileWarningMessages(result, isAutoOpen, platform);
		}

		internal static void ShowFileWarningMessages(string filePath, MediaPathType fileLocation, MediaReference mediaReference, MediaSource mediaSource, bool isAutoOpen, Platform platform)
		{
			MediaPath mediaPath = null;

			if (mediaSource == MediaSource.Path)
			{
				mediaPath = new MediaPath(filePath, fileLocation);
			}
			else if (mediaSource == MediaSource.Reference)
			{
				if (mediaReference != null)
				{
					mediaPath = mediaReference.GetCurrentPlatformMediaReference().MediaPath;
				}
			}

			ShowFileWarningMessages(mediaPath, isAutoOpen, platform);
		}

		internal static void ShowFileWarningMessages(MediaPath mediaPath, bool isAutoOpen, Platform platform)
		{
			string fullPath = string.Empty;
			if (mediaPath != null)
			{
				fullPath = mediaPath.GetResolvedFullPath();
			}
			if (string.IsNullOrEmpty(fullPath))
			{
				if (isAutoOpen)
				{
					EditorHelper.IMGUI.NoticeBox(MessageType.Error, "No media specified");
				}
				else
				{
					EditorHelper.IMGUI.NoticeBox(MessageType.Warning, "No media specified");
				}
			}
			else
			{
				bool isPlatformAndroid = (platform == Platform.Android) || (platform == Platform.Unknown && BuildTargetGroup.Android == UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup);
				bool isPlatformIOS = (platform == Platform.iOS);
				isPlatformIOS |= (platform == Platform.Unknown && BuildTargetGroup.iOS == UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup);
				bool isPlatformTVOS = (platform == Platform.tvOS);

				isPlatformTVOS |= (platform == Platform.Unknown && BuildTargetGroup.tvOS == UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup);

				// Test file extensions
				{
					bool isExtensionAVI = fullPath.ToLower().EndsWith(".avi");
					bool isExtensionMOV = fullPath.ToLower().EndsWith(".mov");
					bool isExtensionMKV = fullPath.ToLower().EndsWith(".mkv");

					if (isPlatformAndroid && isExtensionMOV)
					{
						EditorHelper.IMGUI.NoticeBox(MessageType.Warning, "MOV file detected. Android doesn't support MOV files, you should change the container file.");
					}
					if (isPlatformAndroid && isExtensionAVI)
					{
						EditorHelper.IMGUI.NoticeBox(MessageType.Warning, "AVI file detected. Android doesn't support AVI files, you should change the container file.");
					}
					if (isPlatformAndroid && isExtensionMKV)
					{
						EditorHelper.IMGUI.NoticeBox(MessageType.Warning, "MKV file detected. Android doesn't support MKV files until Android 5.0.");
					}
					if (isPlatformIOS && isExtensionAVI)
					{
						EditorHelper.IMGUI.NoticeBox(MessageType.Warning, "AVI file detected. iOS doesn't support AVI files, you should change the container file.");
					}
				}

				if (fullPath.Contains("://"))
				{
					if (fullPath.ToLower().Contains("rtmp://"))
					{
						EditorHelper.IMGUI.NoticeBox(MessageType.Warning, "RTMP protocol is not supported by AVPro Video, except when Windows DirectShow is used with an external codec library (eg LAV Filters)");
					}
					if (fullPath.ToLower().Contains("youtube.com/watch"))
					{
						EditorHelper.IMGUI.NoticeBox(MessageType.Warning, "YouTube URL detected. YouTube website URL contains no media, a direct media file URL (eg MP4 or M3U8) is required.  See the documentation FAQ for YouTube support.");
					}
					if (mediaPath.PathType != MediaPathType.AbsolutePathOrURL)
					{
						EditorHelper.IMGUI.NoticeBox(MessageType.Warning, "URL detected, change location type to URL?");
					}
					else
					{
						// Display warning to iOS users if they're trying to use HTTP url without setting the permission
						if (isPlatformIOS || isPlatformTVOS)
						{
							if (!PlayerSettings.iOS.allowHTTPDownload && fullPath.StartsWith("http://"))
							{
								EditorHelper.IMGUI.NoticeBox(MessageType.Warning, "Starting with iOS 9 'allow HTTP downloads' must be enabled for HTTP connections (see Player Settings)");
							}
						}
#if UNITY_ANDROID
						if (fullPath.StartsWith("http://"))
						{
							EditorHelper.IMGUI.NoticeBox(MessageType.Warning, "Starting with Android 8 unsecure HTTP is not allowed by default and HTTPS must be used, unless a custom cleartext security policy is assigned");
						}
#endif
						// Display warning for Android users if they're trying to use a URL without setting permission
						if (isPlatformAndroid && !PlayerSettings.Android.forceInternetPermission)
						{
							EditorHelper.IMGUI.NoticeBox(MessageType.Warning, "You need to set 'Internet Access' to 'require' in your Player Settings for Android builds when using URLs");
						}

						// Display warning for UWP users if they're trying to use a URL without setting permission
						if (platform == Platform.WindowsUWP || (platform == Platform.Unknown && (
							BuildTargetGroup.WSA == UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup
							)))
						{
							if (!PlayerSettings.WSA.GetCapability(PlayerSettings.WSACapability.InternetClient))
							{
								EditorHelper.IMGUI.NoticeBox(MessageType.Warning, "You need to set 'InternetClient' capability in your Player Settings when using URLs");
							}
						}
					}
				}
				else
				{
					// [MOZ] All paths on (i|mac|tv)OS are absolute so this check just results in an incorrect warning being shown
					#if !UNITY_EDITOR_OSX
					if (mediaPath.PathType != MediaPathType.AbsolutePathOrURL && fullPath.StartsWith("/"))
					{
						EditorHelper.IMGUI.NoticeBox(MessageType.Warning, "Absolute path detected, change location to Absolute path?");
					}
					#endif

					// Display warning for Android users if they're trying to use absolute file path without permission
					if (isPlatformAndroid && !PlayerSettings.Android.forceSDCardPermission)
					{
						EditorHelper.IMGUI.NoticeBox(MessageType.Warning, "You may need to access the local file system you may need to set 'Write Access' to 'External(SDCard)' in your Player Settings for Android");
					}

					if (platform == Platform.Unknown || platform == MediaPlayer.GetPlatform())
					{
						if (!System.IO.File.Exists(fullPath))
						{
							EditorHelper.IMGUI.NoticeBox(MessageType.Error, "File not found");
						}
						else
						{
							// Check the case
							// This approach is very slow, so we only run it when the app isn't playing
							if (!Application.isPlaying)
							{
								string comparePath = fullPath.Replace('\\', '/');
								string folderPath = System.IO.Path.GetDirectoryName(comparePath);
								if (!string.IsNullOrEmpty(folderPath))
								{

									string[] files = System.IO.Directory.GetFiles(folderPath, "*", System.IO.SearchOption.TopDirectoryOnly);
									bool caseMatch = false;
									if (files != null && files.Length > 0)
									{
										for (int i = 0; i < files.Length; i++)
										{
											if (files[i].Replace('\\', '/') == comparePath)
											{
												caseMatch = true;
												break;
											}
										}
									}
									if (!caseMatch)
									{
										EditorHelper.IMGUI.NoticeBox(MessageType.Warning, "File found but case doesn't match");
									}
								}
							}
						}
					}
				}
			}

			if (mediaPath != null && mediaPath.PathType == MediaPathType.RelativeToStreamingAssetsFolder)
			{
				if (!System.IO.Directory.Exists(Application.streamingAssetsPath))
				{
					GUILayout.BeginHorizontal();
					GUI.color = Color.yellow;
					GUILayout.TextArea("Warning: No StreamingAssets folder found");

					if (GUILayout.Button("Create Folder"))
					{
						System.IO.Directory.CreateDirectory(Application.streamingAssetsPath);
						AssetDatabase.Refresh();
					}
					GUILayout.EndHorizontal();
				}
				else
				{
					bool checkAndroidFileSize = false;
#if UNITY_ANDROID
					if (platform == Platform.Unknown)
					{
						checkAndroidFileSize = true;
					}
#endif
					if (platform == Platform.Android)
					{
						checkAndroidFileSize = true;
					}

					if (checkAndroidFileSize)
					{
						try
						{
							System.IO.FileInfo info = new System.IO.FileInfo(fullPath);
							if (info != null && info.Length > (1024 * 1024 * 512))
							{
								EditorHelper.IMGUI.NoticeBox(MessageType.Warning, "Using this very large file inside StreamingAssets folder on Android isn't recommended.  Deployments will be slow and mapping the file from the StreamingAssets JAR may cause storage and memory issues.  We recommend loading from another folder on the device.");
							}
						}
						catch (System.Exception)
						{
						}
					}
				}
			}

			GUI.color = Color.white;
		}

	}
}