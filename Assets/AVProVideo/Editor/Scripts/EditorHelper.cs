using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// <summary>
	/// Helper methods for editor components
	/// </summary>
	public static class EditorHelper
	{
		/// <summary>
		/// Loads from EditorPrefs, converts a CSV string to List<string> and returns it
		/// </summary>
		internal static List<string> GetEditorPrefsToStringList(string key, char separator = ';')
		{
			string items = EditorPrefs.GetString(key, string.Empty);
			return new List<string>(items.Split(new char[] { separator }, System.StringSplitOptions.RemoveEmptyEntries));
		}

		/// <summary>
		/// Converts a List<string> into a CSV string and saves it in EditorPrefs
		/// </summary>
		internal static void SetEditorPrefsFromStringList(string key, List<string> items, char separator = ';')
		{
			string value = string.Empty;
			if (items != null && items.Count > 0)
			{
				value = string.Join(separator.ToString(), items.ToArray());
			}
			EditorPrefs.SetString(key, value);
		}

		public static SerializedProperty CheckFindProperty(this UnityEditor.Editor editor, string propertyName)
		{
			SerializedProperty result = editor.serializedObject.FindProperty(propertyName);
			Debug.Assert(result != null, "Missing property: " + propertyName);
			return result;
		}

		/// <summary>
		/// Only lets the property if the proposed path doesn't contain invalid characters
		/// Also changes all backslash characters to forwardslash for better cross-platform compatability
		/// </summary>
		internal static bool SafeSetPathProperty(string path, SerializedProperty property)
		{
			bool result = false;
			if (path == null)
			{
				path = string.Empty;
			}
			else if (path.IndexOfAny(System.IO.Path.GetInvalidPathChars()) < 0)
			{
				path = path.Replace("\\", "/");
			}
			if (path.StartsWith("//"))
			{
				path = path.Substring(2);
			}

			if (path != property.stringValue)
			{
				property.stringValue = path;
				result = true;
			}
			
			return result;
		}

		/// <summary>
		/// Returns whether a define exists for a specific platform
		/// </summary>
		internal static bool HasScriptDefine(string define, BuildTargetGroup buildTarget = BuildTargetGroup.Unknown)
		{
			if (buildTarget == BuildTargetGroup.Unknown) { buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup; }
			string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
			return defines.Contains(define);
		}

		/// <summary>
		/// Adds a define if it doesn't already exist for a specific platform
		/// </summary>
		internal static void AddScriptDefine(string define, BuildTargetGroup buildTarget = BuildTargetGroup.Unknown)
		{
			if (buildTarget == BuildTargetGroup.Unknown) { buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup; }
			string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
			if (!defines.Contains(define))
			{
				defines += ";" + define + ";";
				PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, defines);
			}
		}

		/// <summary>
		/// Removes a define if it exists for a specific platform
		/// </summary>
		internal static void RemoveScriptDefine(string define, BuildTargetGroup buildTarget = BuildTargetGroup.Unknown)
		{
			if (buildTarget == BuildTargetGroup.Unknown) { buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup; }
			string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
			if (defines.Contains(define))
			{
				defines = defines.Replace(define, "");
				PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, defines);
			}
		}

		/// <summary>
		/// Given a partial file path and MediaLocation, return a directory path suitable for a file browse dialog to start in
		/// </summary>
		internal static string GetBrowsableFolder(string path, MediaPathType fileLocation)
		{
			// Try to resolve based on file path + file location
			string result = Helper.GetFilePath(path, fileLocation);
			if (!string.IsNullOrEmpty(result))
			{
				if (System.IO.File.Exists(result))
				{
					result = System.IO.Path.GetDirectoryName(result);
				}
			}

			if (!System.IO.Directory.Exists(result))
			{
				// Just resolve on file location
				result = Helper.GetPath(fileLocation);
			}
			if (string.IsNullOrEmpty(result))
			{
				// Fallback
				result = Application.streamingAssetsPath;
			}
			return result;
		}

		internal static bool OpenMediaFileDialog(string startPath, ref MediaPath mediaPath, ref string fullPath, string extensions)
		{
			bool result = false;

			string path = UnityEditor.EditorUtility.OpenFilePanel("Browse Media File", startPath, extensions);
			if (!string.IsNullOrEmpty(path) && !path.EndsWith(".meta"))
			{
				mediaPath = GetMediaPathFromFullPath(path);
				result = true;
			}

			return result;
		}

		/*private static bool IsPathWithin(string fullPath, string targetPath)
		{
			return fullPath.StartsWith(targetPath);
		}*/

		private static string GetPathRelativeTo(string root, string fullPath)
		{
			string result = fullPath.Remove(0, root.Length);
			if (result.StartsWith(System.IO.Path.DirectorySeparatorChar.ToString()) || result.StartsWith(System.IO.Path.AltDirectorySeparatorChar.ToString()))
			{
				result = result.Remove(0, 1);
			}
			return result;
		}

		internal static MediaPath GetMediaPathFromFullPath(string fullPath)
		{
			MediaPath result = null;
			string projectRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, ".."));
			projectRoot = projectRoot.Replace('\\', '/');

			if (fullPath.StartsWith(projectRoot))
			{
				if (fullPath.StartsWith(Application.streamingAssetsPath))
				{
					// Must be StreamingAssets relative path
					result = new MediaPath(GetPathRelativeTo(Application.streamingAssetsPath, fullPath), MediaPathType.RelativeToStreamingAssetsFolder);
				}
				else if (fullPath.StartsWith(Application.dataPath))
				{
					// Must be Assets relative path
					result = new MediaPath(GetPathRelativeTo(Application.dataPath, fullPath), MediaPathType.RelativeToDataFolder);
				}
				else
				{
					// Must be project relative path
					result = new MediaPath(GetPathRelativeTo(projectRoot, fullPath), MediaPathType.RelativeToProjectFolder);
				}
			}
			else
			{
				// Must be persistant data
				if (fullPath.StartsWith(Application.persistentDataPath))
				{
					result = new MediaPath(GetPathRelativeTo(Application.persistentDataPath, fullPath), MediaPathType.RelativeToPersistentDataFolder);
				}

				// Must be absolute path
				result = new MediaPath(fullPath, MediaPathType.AbsolutePathOrURL);
			}
			return result;
		}

		internal class IMGUI
		{
			private static GUIStyle _copyableStyle = null;
			private static GUIStyle _wordWrappedTextAreaStyle = null;
			private static GUIStyle _rightAlignedLabelStyle = null;
			private static GUIStyle _centerAlignedLabelStyle = null;

			/// <summary>
			/// Displays an IMGUI warning text box inline
			/// </summary>
			internal static void WarningTextBox(string title, string body, Color bgColor, Color titleColor, Color bodyColor)
			{
				BeginWarningTextBox(title, body, bgColor, titleColor, bodyColor);
				EndWarningTextBox();
			}

			/// <summary>
			/// Displays an IMGUI warning text box inline
			/// </summary>
			internal static void BeginWarningTextBox(string title, string body, Color bgColor, Color titleColor, Color bodyColor)
			{
				GUI.backgroundColor = bgColor;
				EditorGUILayout.BeginVertical(GUI.skin.box);
				if (!string.IsNullOrEmpty(title))
				{
					GUI.color = titleColor;
					GUILayout.Label(title, EditorStyles.boldLabel);
				}
				if (!string.IsNullOrEmpty(body))
				{
					GUI.color = bodyColor;
					GUILayout.Label(body, EditorStyles.wordWrappedLabel);
				}
			}

			internal static void EndWarningTextBox()
			{
				EditorGUILayout.EndVertical();
				GUI.backgroundColor = Color.white;
				GUI.color = Color.white;
			}

			/// <summary>
			/// Displays an IMGUI box containing a copyable string that wraps
			/// Usedful for very long strings eg file paths/urls
			/// </summary>
			internal static void CopyableFilename(string path)
			{
				// The box disappars unless it has some content
				if (string.IsNullOrEmpty(path))
				{
					path = " ";
				}

				// Display the file name so it's easy to read and copy to the clipboard
				if (!string.IsNullOrEmpty(path) && 0 > path.IndexOfAny(System.IO.Path.GetInvalidPathChars()))
				{
					// Some GUI hacks here because SelectableLabel wants to be double height and it doesn't want to be centered because it's an EditorGUILayout function...
					string text = System.IO.Path.GetFileName(path);

					if (_copyableStyle == null)
					{
						_copyableStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
						_copyableStyle.fontStyle = FontStyle.Bold;
						_copyableStyle.stretchWidth = true;
						_copyableStyle.stretchHeight = true;
						_copyableStyle.alignment = TextAnchor.MiddleCenter;
						_copyableStyle.margin.top = 8;
						_copyableStyle.margin.bottom = 16;
					}

					float height = _copyableStyle.CalcHeight(new GUIContent(text), Screen.width)*1.5f;
					EditorGUILayout.SelectableLabel(text, _copyableStyle, GUILayout.Height(height), GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));
				}
			}

			/// <summary>
			/// </summary>
			internal static GUIStyle GetWordWrappedTextAreaStyle()
			{
				if (_wordWrappedTextAreaStyle == null)
				{
					_wordWrappedTextAreaStyle = new GUIStyle(EditorStyles.textArea);
					_wordWrappedTextAreaStyle.wordWrap = true;
				}
				return _wordWrappedTextAreaStyle;
			}

			internal static GUIStyle GetRightAlignedLabelStyle()
			{
				if (_rightAlignedLabelStyle == null)
				{
					_rightAlignedLabelStyle = new GUIStyle(GUI.skin.label);
					_rightAlignedLabelStyle.alignment = TextAnchor.UpperRight;
				}
				return _rightAlignedLabelStyle;
			}

			internal static GUIStyle GetCenterAlignedLabelStyle()
			{
				if (_centerAlignedLabelStyle == null)
				{
					_centerAlignedLabelStyle = new GUIStyle(GUI.skin.label);
					_centerAlignedLabelStyle.alignment = TextAnchor.MiddleCenter;
				}
				return _centerAlignedLabelStyle;
			}			

			/// <summary>
			/// Displays IMGUI box in red/yellow for errors/warnings
			/// </summary>
			internal static void NoticeBox(MessageType messageType, string message)
			{
				//GUI.backgroundColor = Color.yellow;
				//EditorGUILayout.HelpBox(message, messageType);

				switch (messageType)
				{
					case MessageType.Error:
						GUI.color = Color.red;
						message = "Error: " + message;
						break;
					case MessageType.Warning:
						GUI.color = Color.yellow;
						message = "Warning: " + message;
						break;
				}

				//GUI.color = Color.yellow;
				GUILayout.TextArea(message);
				GUI.color = Color.white;
			}

			/// <summary>
			/// Displays IMGUI text centered horizontally
			/// </summary>
			internal static void CentreLabel(string text, GUIStyle style = null)
			{
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (style == null)
				{
					GUILayout.Label(text);
				}
				else
				{
					GUILayout.Label(text, style);
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}

			internal static bool ToggleScriptDefine(string label, string define)
			{
				EditorGUI.BeginChangeCheck();
				bool isEnabled = EditorGUILayout.Toggle(label, EditorHelper.HasScriptDefine(define));
				if (EditorGUI.EndChangeCheck())
				{
					if (isEnabled)
					{
						EditorHelper.AddScriptDefine(define);
					}
					else
					{
						EditorHelper.RemoveScriptDefine(define);
					}
				}
				return isEnabled;
			}
		}
	}

	internal class HorizontalFlowScope : GUI.Scope
	{
		private float _windowWidth;
		private float _width;

		public HorizontalFlowScope(int windowWidth)
		{
			_windowWidth = windowWidth;
			_width = _windowWidth;
			GUILayout.BeginHorizontal();
		}

		protected override void CloseScope()
		{
			GUILayout.EndHorizontal();
		}

		public void AddItem(GUIContent content, GUIStyle style)
		{
			_width -= style.CalcSize(content).x + style.padding.horizontal;
			if (_width <= 0f)
			{
				_width += Screen.width;
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
			}
		}
	}
}