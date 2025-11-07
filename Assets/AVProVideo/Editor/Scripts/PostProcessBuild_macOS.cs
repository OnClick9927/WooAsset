#if UNITY_EDITOR && UNITY_2019_1_OR_NEWER

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

//-----------------------------------------------------------------------------
// Copyright 2012-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	public class PBXProjectHandlerException : System.Exception
	{
		public PBXProjectHandlerException(string message)
		:	base(message)
		{

		}
	}

	public class PBXProjectHandler
	{
		private static System.Type _PBXProjectType;
		private static System.Type PBXProjectType
		{
			get
			{
				if (_PBXProjectType == null)
				{
					_PBXProjectType = System.Type.GetType("UnityEditor.iOS.Xcode.PBXProject, UnityEditor.iOS.Extensions.Xcode, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
					if (_PBXProjectType == null)
					{
						throw new PBXProjectHandlerException("Failed to get type \"PBXProject\"");
					}
				}
				return _PBXProjectType;
			}
		}

		private static Dictionary<string, MethodInfo> _PBXProjectTypeMethods;
		private static Dictionary<string, MethodInfo> PBXProjectTypeMethods
		{
			get
			{
				if (_PBXProjectTypeMethods == null)
				{
					_PBXProjectTypeMethods = new Dictionary<string, MethodInfo>();
				}
				return _PBXProjectTypeMethods;
			}
		}

		private static MethodInfo GetMethod(string name, System.Type[] types)
		{
			string lookup = name + types.ToString();
			MethodInfo method;
			if (!PBXProjectTypeMethods.TryGetValue(lookup, out method))
			{
				method = _PBXProjectType.GetMethod(name, types);
				if (method != null)
				{
					_PBXProjectTypeMethods[lookup] = method;
				}
				else
				{
					throw new PBXProjectHandlerException(string.Format("Unknown method \"{0}\"", name));
				}
			}
			return method;
		}

		private object _project;

		public PBXProjectHandler()
		{
			_project = System.Activator.CreateInstance(PBXProjectType);
		}

		public void ReadFromFile(string path)
		{
			MethodInfo method = GetMethod("ReadFromFile", new System.Type[] { typeof(string) });
			Debug.LogFormat("[AVProVideo] Reading Xcode project at: {0}", path);
			method.Invoke(_project, new object[] { path });
		}

		public void WriteToFile(string path)
		{
			MethodInfo method = GetMethod("WriteToFile", new System.Type[] { typeof(string) });
			Debug.LogFormat("[AVProVideo] Writing Xcode project to: {0}", path);
			method.Invoke(_project, new object[] { path });
		}

		public string TargetGuidByName(string name)
		{
			MethodInfo method = GetMethod("TargetGuidByName", new System.Type[] { typeof(string) });
			string guid = (string)method.Invoke(_project, new object[] { name });
			Debug.LogFormat("[AVProVideo] Target GUID for '{0}' is '{1}'", name, guid);
			return guid;
		}

		public void SetBuildProperty(string guid, string property, string value)
		{
			MethodInfo method = GetMethod("SetBuildProperty", new System.Type[] { typeof(string), typeof(string), typeof(string) });
			Debug.LogFormat("[AVProVideo] Setting build property '{0}' to '{1}' for target with guid '{2}'", property, value, guid);
			method.Invoke(_project, new object[] { guid, property, value });
		}
	}

	public class PostProcessBuild_macOS
	{
		private static bool ActualModifyProjectAtPath(string path)
		{
			if (!Directory.Exists(path))
			{
				Debug.LogWarningFormat("[AVProVideo] Failed to find Xcode project with path: {0}", path);
				return false;
			}

			Debug.LogFormat("[AVProVideo] Modifying Xcode project at: {0}", path);
			string projectPath = Path.Combine(path, "project.pbxproj");
			try
			{
				PBXProjectHandler handler = new PBXProjectHandler();
				handler.ReadFromFile(projectPath);
				string guid = handler.TargetGuidByName(Application.productName);
				handler.SetBuildProperty(guid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
				handler.WriteToFile(projectPath);
				return true;
			}
			catch (PBXProjectHandlerException ex)
			{
				Debug.LogErrorFormat("[AVProVideo] {0}", ex);
			}

			return false;
		}

		[PostProcessBuild]
		public static void ModifyProject(BuildTarget target, string path)
		{
			if (target != BuildTarget.StandaloneOSX)
				return;

#if AVPROVIDEO_SUPPORT_MACOSX_10_14_3_AND_OLDER

			Debug.Log("[AVProVideo] Post-processing Xcode project");

			string projectPath = Path.Combine(path, Path.GetFileName(path) + ".xcodeproj");
			if (ActualModifyProjectAtPath(projectPath))
			{
				Debug.Log("[AVProVideo] Finished");
			}
			else
			{
				Debug.LogError("[AVProVideo] Failed to modify Xcode project");
				Debug.Log("[AVProVideo] You will need to manually set \"Always Embed Swift Standard Libraries\" to \"YES\" in the target's build settings if you're targetting macOS versions prior to 10.14.4");
			}

#endif // AVPROVIDEO_SUPPORT_MACOSX_10_14_3_AND_OLDER

		}
	}

}   // namespace RenderHeads.Media.AVProVideo.Editor

#endif
