#if (UNITY_IOS || UNITY_TVOS) && UNITY_2017_1_OR_NEWER
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;

//-----------------------------------------------------------------------------
// Copyright 2012-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	public class PostProcessBuild_iOS
	{
		const string PluginName = "AVProVideo.framework";

		// Simple holder for major.minor version number
		private readonly struct Version
		{
			public static Version unknown = new Version(0, 0);
			public static Version _10_0 = new Version(10, 0);
			public static Version _12_2 = new Version(12, 2);

			public readonly int major;
			public readonly int minor;

			public Version(int major, int minor)
			{
				this.major = major;
				this.minor = minor;
			}

			public static bool operator ==(Version lhs, Version rhs) => (lhs.major == rhs.major && lhs.minor == rhs.minor);
			public static bool operator !=(Version lhs, Version rhs) => (lhs.major != rhs.major || lhs.minor != rhs.minor);
			public static bool operator  <(Version lhs, Version rhs) => (lhs.major < rhs.major) || (lhs.major == rhs.major && lhs.minor < rhs.minor);
			public static bool operator  >(Version lhs, Version rhs) => (lhs.major > rhs.major) || (lhs.major == rhs.major && lhs.minor > rhs.minor);

			public override bool Equals(object obj) { return base.Equals(obj); }
			public override int GetHashCode() { return base.GetHashCode(); }
			public override string ToString() { return string.Format("{0}.{1}", major, minor); }
		}

		private class Platform
		{
			public BuildTarget target { get; }
			public string name { get; }
			public string guid { get; }

			// Accessor for PlayerSettings.Platform.targetOSVersionString
			public string targetOSVersionString
			{
				get
				{
					switch (target)
					{
					case BuildTarget.iOS:
						return PlayerSettings.iOS.targetOSVersionString;
					case BuildTarget.tvOS:
						return PlayerSettings.tvOS.targetOSVersionString;
					default:
						return null;
					}
				}
			}

			// Will lazily set version from targetOSVersionString when called for the first time
			private Version _version = Version.unknown;
			public Version targetOSVersion
			{
				get
				{
					if (_version == Version.unknown)
					{
						if (targetOSVersionString != null)
						{
							string[] version = targetOSVersionString.Split('.');
							if (version != null && version.Length >= 1)
							{
								int major = 0;
								if (int.TryParse(version[0], out major) && version.Length >= 2)
								{
									int minor = 0;
									if (int.TryParse(version[1], out minor))
									{
										_version = new Version(major, minor);
									}
								}
							}
						}
						if (_version == Version.unknown)
						{
							// 10.0 is the minumum version we support so default to this
							_version = Version._10_0;
						}
					}
					return _version;
				}
			}

			public static Platform GetPlatformForTarget(BuildTarget target)
			{
				switch (target)
				{
					case BuildTarget.iOS:
						return new Platform(BuildTarget.iOS, "iOS", "2a1facf97326449499b63c03811b1ab2");

					case BuildTarget.tvOS:
						return new Platform(BuildTarget.tvOS, "tvOS", "bcf659e3a94d748d6a100d5531540d1a");

					default:
						return null;
				}
			}

			private Platform(BuildTarget target, string name, string guid)
			{
				this.target = target;
				this.name = name;
				this.guid = guid;
			}
		}

		private static string PluginPathForPlatform(Platform platform)
		{
			// See if we can find the plugin by GUID
			string pluginPath = AssetDatabase.GUIDToAssetPath(platform.guid);

			// If not, try and find it by name
			if (pluginPath.Length == 0)
			{
				Debug.LogWarningFormat("[AVProVideo] Failed to find plugin by GUID, will attempt to find it by name.");
				string[] guids = AssetDatabase.FindAssets(PluginName);
				if (guids != null && guids.Length > 0)
				{
					foreach (string guid in guids)
					{
						string assetPath = AssetDatabase.GUIDToAssetPath(guid);
						if (assetPath.Contains(platform.name))
						{
							pluginPath = assetPath;
							break;
						}
					}
				}
			}

			if (pluginPath.Length > 0)
			{
				Debug.LogFormat("[AVProVideo] Found plugin at '{0}'", pluginPath);
			}

			return pluginPath;
		}

		// Converts the Unity asset path to the expected path in the built Xcode project.
		private static string ConvertPluginAssetPathToXcodeProjectFrameworkPath(string pluginPath)
		{
			List<string> components = new List<string>(pluginPath.Split(new char[] { '/' }));
			components[0] = "Frameworks";
#if UNITY_2019_1_OR_NEWER
				string frameworkPath = string.Join("/", components);
#else
				string frameworkPath = string.Join("/", components.ToArray());
#endif
			return frameworkPath;
		}

		// Helper to set the file execute bits
		private static void SetFileExecutePermission(string path)
		{
#if UNITY_EDITOR_OSX
			Debug.LogFormat("[AVProVideo] Checking permissions on {0}", path);

			string cmd = string.Format("if [ ! -x \"{0}\" ]; then echo \"Missing execute permissions, fixing...\"; chmod a+x \"{0}\"; else echo \"All good\"; fi", path);
			string args = string.Format("-c \"{0}\"", cmd.Replace("\"", "\\\""));

			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo("/bin/sh");
			startInfo.Arguments = args;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow = true;

			System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo);
			string result = process.StandardOutput.ReadToEnd();
			string error = process.StandardError.ReadToEnd();
			process.WaitForExit();

			if (error != null && error.Length > 0)
			{
				Debug.LogErrorFormat("[AVProVideo] Failed to set execute permissions on the plugin binary, error: {0}", error);
			}
			else if (result != null && result.Length > 0)
			{
				Debug.LogFormat("[AVProVideo] {0}", result);
			}
#else
			Debug.LogWarningFormat("[AVProVideo] Project is not being built on macOS so we are unable to check the file permissions on the plugin binary. You need to make sure that the execute bits are set on \"AVProVideo.framework/AVProVideo\" before building the Xcode project.");
#endif
		}

		[PostProcessBuild]
		public static void ModifyProject(BuildTarget target, string path)
		{
			if (target != BuildTarget.iOS && target != BuildTarget.tvOS)
				return;

			Debug.Log("[AVProVideo] Post-processing Xcode project.");
			Platform platform = Platform.GetPlatformForTarget(target);
			if (platform == null)
			{
				Debug.LogWarningFormat("[AVProVideo] Unknown build target: {0}", target.ToString());
				return;
			}

			string projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
			PBXProject project = new PBXProject();
			project.ReadFromFile(projectPath);

			// Attempt to find the plugin path
			string pluginPath = PluginPathForPlatform(platform);
			if (pluginPath.Length > 0)
			{
#if UNITY_2019_3_OR_NEWER
					string targetGuid = project.GetUnityMainTargetGuid();
#else
					string targetGuid = project.TargetGuidByName(PBXProject.GetUnityTargetName());
#endif

				string frameworkPath = ConvertPluginAssetPathToXcodeProjectFrameworkPath(pluginPath);
				string fileGuid = project.FindFileGuidByProjectPath(frameworkPath);
				if (fileGuid != null)
				{
					// Make sure the plugin binary has execute permissions set.
					// For reasons unknown these are being lost somewhere between the plugin package being built and imported from the asset store.
					string binaryPath = System.IO.Path.Combine(path, frameworkPath, "AVProVideo");
					SetFileExecutePermission(binaryPath);

					Debug.LogFormat("[AVProVideo] Adding 'AVProVideo.framework' to the list of embedded frameworks");
					PBXProjectExtensions.AddFileToEmbedFrameworks(project, targetGuid, fileGuid);

					Debug.LogFormat("[AVProVideo] Setting 'LD_RUNPATH_SEARCH_PATHS' to '$(inherited) @executable_path/Frameworks'");
					project.SetBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks");
				}
				else
				{
					Debug.LogWarningFormat("[AVProVideo] Failed to find {0} in the generated project. You will need to manually set {0} to 'Embed & Sign' in the Xcode project's framework list.", PluginName);
				}

				// See if we need to enable embedding of Swift binaries
				if (platform.targetOSVersion < Version._12_2)
				{
					Debug.LogFormat("[AVProVideo] Target OS version '{0}' is < 12.2, setting 'ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES' to 'YES'", platform.targetOSVersion);
					project.SetBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
				}

				Debug.LogFormat("[AVProVideo] Writing out Xcode project file");
				project.WriteToFile(projectPath);
			}
			else
			{
				Debug.LogErrorFormat("Failed to find '{0}' for '{1}' in the Unity project. Something is horribly wrong, please reinstall AVPro Video.", PluginName, platform);
			}
		}
	}
}

#endif
