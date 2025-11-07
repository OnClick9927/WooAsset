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
	public static class RecentItems
	{
		private const int MaxRecentItems = 16;

		private static List<string> _recentFiles = new List<string>(MaxRecentItems);
		private static List<string> _recentUrls = new List<string>(MaxRecentItems);
		// TODO: add a list for favourites to allow user to create their own list?

		public static List<string> Files { get { return _recentFiles; } }
		public static List<string> Urls { get { return _recentUrls; } }

		static RecentItems()
		{
			MediaPlayer.InternalMediaLoadedEvent.RemoveListener(Add);
			MediaPlayer.InternalMediaLoadedEvent.AddListener(Add);
		}

		public static void Load()
		{
			_recentFiles = EditorHelper.GetEditorPrefsToStringList(MediaPlayerEditor.SettingsPrefix + "RecentFiles");
			_recentUrls = EditorHelper.GetEditorPrefsToStringList(MediaPlayerEditor.SettingsPrefix + "RecentUrls");
		}

		public static void Save()
		{
			EditorHelper.SetEditorPrefsFromStringList(MediaPlayerEditor.SettingsPrefix + "RecentFiles", _recentFiles);
			EditorHelper.SetEditorPrefsFromStringList(MediaPlayerEditor.SettingsPrefix + "RecentUrls", _recentUrls);
		}

		public static void Add(string path)
		{
			if (path.Contains("://"))
			{
				Add(path, _recentUrls);
			}
			else
			{
				Add(path, _recentFiles);
			}
		}

		private static void Add(string path, List<string> list)
		{
			if (!list.Contains(path))
			{
				list.Insert(0, path);
				if (list.Count > MaxRecentItems)
				{
					// Remove the oldest item from the list
					list.RemoveAt(list.Count - 1);
				}
			}
			else
			{
				// If it already contains the item, then move it to the top
				list.Remove(path);
				list.Insert(0, path);
			}
			Save();
		}

		public static void ClearMissingFiles()
		{
			if (_recentFiles != null && _recentFiles.Count > 0)
			{
				List<string> newList = new List<string>(_recentFiles.Count);
				for (int i = 0; i < _recentFiles.Count; i++)
				{
					string path = _recentFiles[i];
					if (System.IO.File.Exists(path))
					{
						newList.Add(path);
					}
				}
				_recentFiles = newList;
			}
		}
	}
}