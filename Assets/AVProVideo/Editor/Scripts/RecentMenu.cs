using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// <summary>
	/// </summary>
	public class RecentMenu
	{
		public static void Create(SerializedProperty propPath, SerializedProperty propMediaSource, string fileExtensions, bool autoLoadMedia, int mediaReferencePickerId = -1)
		{
			RecentItems.Load();
			RecentMenu menu = new RecentMenu();
			menu.FileBrowseButton(propPath, propMediaSource, fileExtensions, autoLoadMedia, mediaReferencePickerId);
		}

		private void FileBrowseButton(SerializedProperty propPath, SerializedProperty propMediaSource, string fileExtensions, bool autoLoadMedia, int mediaReferencePickerId = -1)
		{
			GenericMenu toolsMenu = new GenericMenu();
			if (mediaReferencePickerId >= 0)
			{
				toolsMenu.AddItem(new GUIContent("Media References..."), false, Callback_BrowseMediaReferences, (object)mediaReferencePickerId);
			}
			toolsMenu.AddItem(new GUIContent("Browse..."), false, Callback_Browse, new BrowseData(propPath, propMediaSource, fileExtensions, autoLoadMedia));
			CreateMenu_StreamingAssets(toolsMenu, "StreamingAssets/", propPath, propMediaSource, autoLoadMedia);
			CreateMenu_RecentFiles(toolsMenu, RecentItems.Files, "Recent Files/" , propPath, propMediaSource, autoLoadMedia);
			CreateMenu_RecentUrls(toolsMenu, RecentItems.Urls, "Recent URLs/", propPath, propMediaSource, autoLoadMedia);
			toolsMenu.ShowAsContext();
		}

		private struct RecentMenuItemData
		{
			public RecentMenuItemData(string path, SerializedProperty propPath, SerializedProperty propMediaSource, bool autoLoadMedia)
			{
				this.path = path;
				this.propPath = propPath;
				this.propMediaSource = propMediaSource;
				this.autoLoadMedia = autoLoadMedia;
			}

			public string path;
			public bool autoLoadMedia;
			public SerializedProperty propPath;
			public SerializedProperty propMediaSource;
		}

		private void Callback_Select(object obj)
		{
			RecentMenuItemData data = (RecentMenuItemData)obj;

			// Move it to the top of the list
			RecentItems.Add(data.path);

			// Resolve to relative path
			MediaPath mediaPath = EditorHelper.GetMediaPathFromFullPath(data.path);

			SerializedProperty propMediaPath = data.propPath.FindPropertyRelative("_path");
			SerializedProperty propMediaPathType = data.propPath.FindPropertyRelative("_pathType");

			// Assign to properties
			propMediaPath.stringValue = mediaPath.Path.Replace("\\", "/");
			propMediaPathType.enumValueIndex = (int)mediaPath.PathType;
			if (data.propMediaSource != null) data.propMediaSource.enumValueIndex = (int)MediaSource.Path;

			// Mark as modified
			data.propPath.serializedObject.ApplyModifiedProperties();
			foreach (Object o in data.propPath.serializedObject.targetObjects)
			{
				EditorUtility.SetDirty(o);
			}

			if (data.autoLoadMedia)
			{
				MediaPlayer mediaPlayer = (MediaPlayer)data.propPath.serializedObject.targetObject;
				if (mediaPlayer != null)
				{
					mediaPlayer.OpenMedia(mediaPlayer.MediaPath, autoPlay:true);
				}
			}
		}

		private void Callback_ClearList(object obj)
		{
			((List<string>)obj).Clear();
			RecentItems.Save();
		}

		private void Callback_ClearMissingFiles()
		{
			RecentItems.ClearMissingFiles();
			RecentItems.Save();
		}

		private struct BrowseData
		{
			public BrowseData(SerializedProperty propPath, SerializedProperty propMediaSource, string extensions, bool autoLoadMedia)
			{
				this.extensions = extensions;
				this.propPath = propPath;
				this.propMediaSource = propMediaSource;
				this.autoLoadMedia = autoLoadMedia;
			}

			public bool autoLoadMedia;
			public string extensions;
			public SerializedProperty propPath;
			public SerializedProperty propMediaSource;
		}

		private void Callback_BrowseMediaReferences(object obj)
		{
			int controlID = (int)obj;
			EditorGUIUtility.ShowObjectPicker<MediaReference>(null, false, "", controlID);
		}

		private void Callback_Browse(object obj)
		{
			BrowseData data = (BrowseData)obj;
			SerializedProperty propFilePath = data.propPath.FindPropertyRelative("_path");
			SerializedProperty propFilePathType = data.propPath.FindPropertyRelative("_pathType");
			string startFolder = EditorHelper.GetBrowsableFolder(propFilePath.stringValue, (MediaPathType)propFilePathType.enumValueIndex);
			string videoPath = propFilePath.stringValue;
			string fullPath = string.Empty;
			MediaPath mediaPath = new MediaPath();
			if (EditorHelper.OpenMediaFileDialog(startFolder, ref mediaPath, ref fullPath, data.extensions))
			{
				// Assign to properties
				propFilePath.stringValue = mediaPath.Path.Replace("\\", "/");
				propFilePathType.enumValueIndex = (int)mediaPath.PathType;
				if (data.propMediaSource != null) data.propMediaSource.enumValueIndex = (int)MediaSource.Path;

				// Mark as modified
				data.propPath.serializedObject.ApplyModifiedProperties();
				foreach (Object o in data.propPath.serializedObject.targetObjects)
				{
					EditorUtility.SetDirty(o);
				}

				if (data.autoLoadMedia)
				{
					MediaPlayer mediaPlayer = (MediaPlayer)data.propPath.serializedObject.targetObject;
					if (mediaPlayer != null)
					{
						mediaPlayer.OpenMedia(mediaPlayer.MediaPath, autoPlay:true);
					}
				}

				RecentItems.Add(fullPath);
			}
		}

		private void CreateMenu_RecentFiles(GenericMenu menu, List<string> items, string prefix, SerializedProperty propPath, SerializedProperty propMediaSource, bool autoLoadMedia)
		{
			int missingCount = 0;
			for (int i = 0; i < items.Count; i++)
			{
				string path = items[i];
				// Slashes in path must be replaced as they cause the menu to create submenuts
				string itemName = ReplaceSlashes(path);
				// TODO: shorten if itemName too long
				if (System.IO.File.Exists(path))
				{
					menu.AddItem(new GUIContent(prefix + itemName), false, Callback_Select, new RecentMenuItemData(path, propPath, propMediaSource, autoLoadMedia));
				}
				else
				{
					menu.AddDisabledItem(new GUIContent(prefix + itemName));
					missingCount++;
				}
			}
			if (items.Count > 0)
			{
				menu.AddSeparator(prefix + "");
				menu.AddItem(new GUIContent(prefix + "Clear"), false, Callback_ClearList, items);
				if (missingCount > 0)
				{
					menu.AddItem(new GUIContent(prefix + "Clear Missing (" + missingCount + ")"), false, Callback_ClearMissingFiles);
				}
			}
			else
			{
				menu.AddDisabledItem(new GUIContent(prefix + "No recent files yet"));
			}
		}

		private void CreateMenu_RecentUrls(GenericMenu menu, List<string> items, string prefix, SerializedProperty propPath, SerializedProperty propMediaSource, bool autoLoadMedia)
		{
			for (int i = 0; i < items.Count; i++)
			{
				string path = items[i];
				// Slashes in path must be replaced as they cause the menu to create submenuts
				string itemName = ReplaceSlashes(path);
				// TODO: shorten if itemName too long
				menu.AddItem(new GUIContent(prefix + itemName), false, Callback_Select, new RecentMenuItemData(path, propPath, propMediaSource, autoLoadMedia));
			}
			if (items.Count > 0)
			{
				menu.AddSeparator(prefix + "");
				menu.AddItem(new GUIContent(prefix + "Clear"), false, Callback_ClearList, items);
			}
			else
			{
				menu.AddDisabledItem(new GUIContent(prefix + "No recent URLs yet"));
			}
		}

		private static string ReplaceSlashes(string text)
		{
			string slashReplacement = "\u2215";
#if UNITY_EDITOR_WIN
			// Special replacement for "//" in URLS so they aren't spaced too far apart
			text = text.Replace("//", " \u2215 \u2215 ");

			// On Windows we have to add extra spaces so it doesn't look squashed together
			slashReplacement = " \u2215 ";
#endif

			text = text.Replace("/", slashReplacement).Replace("\\", slashReplacement);	

			// Unity will place text after " _" on the right of the menu, so we replace it so this doesn't happen
			text = text.Replace(" _", "_");

			return text;
		}

		private static List<string> FindMediaFilesInStreamingAssetsFolder()
		{
			List<string> files = new List<string>();
			if (System.IO.Directory.Exists(Application.streamingAssetsPath))
			{
				string[] allFiles = System.IO.Directory.GetFiles(Application.streamingAssetsPath, "*", System.IO.SearchOption.AllDirectories);
				if (allFiles != null && allFiles.Length > 0)
				{
					// Filter by type
					for (int i = 0; i < allFiles.Length; i++)
					{
						bool remove = false;
						if (allFiles[i].EndsWith(".meta", System.StringComparison.InvariantCultureIgnoreCase))
						{
							remove = true;
						}

#if UNITY_EDITOR_OSX
						remove = remove || allFiles[i].EndsWith(".DS_Store");
#endif

						if (!remove)
						{
							files.Add(allFiles[i]);
						}
					}
				}
			}
			return files;
		}

		private void CreateMenu_StreamingAssets(GenericMenu menu, string prefix, SerializedProperty propPath, SerializedProperty propMediaSource, bool autoLoadMedia)
		{
			List<string> files = FindMediaFilesInStreamingAssetsFolder();
			if (files.Count > 0)
			{
				for (int i = 0; i < files.Count; i++)
				{
					string path = files[i];
					if (System.IO.File.Exists(path))
					{
						string itemName = path.Replace(Application.streamingAssetsPath, "");
						if (itemName.StartsWith("/") || itemName.StartsWith("\\"))
						{
							itemName = itemName.Remove(0, 1);
						}
						itemName = itemName.Replace("\\", "/");

						menu.AddItem(new GUIContent(prefix + itemName), false, Callback_Select, new RecentMenuItemData(path, propPath, propMediaSource, autoLoadMedia));
					}
				}
			}
			else
			{
				menu.AddDisabledItem(new GUIContent(prefix + "StreamingAssets folder missing or contains no files"));
			}
		}
	}
}