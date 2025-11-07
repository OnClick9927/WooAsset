#if UNITY_2018_1_OR_NEWER || (UNITY_2017_4_OR_NEWER && !UNITY_2017_4_0 && !UNITY_2017_4_1 && !UNITY_2017_4_2 && !UNITY_2017_4_3 && !UNITY_2017_4_4 && !UNITY_2017_4_5 && !UNITY_2017_4_6 && !UNITY_2017_4_7 && !UNITY_2017_4_8 && !UNITY_2017_4_9 && !UNITY_2017_4_10 && !UNITY_2017_4_11 && !UNITY_2017_4_12 && !UNITY_2017_4_13 && !UNITY_2017_4_14 && !UNITY_2017_4_15 && !UNITY_2017_4_15)
	// Unity added Android ARM64 support in 2018.1, and backported to 2017.4.16
	#define AVPROVIDEO_UNITY_ANDROID_ARM64_SUPPORT
#endif
#if !UNITY_2019_3_OR_NEWER || UNITY_2021_2_OR_NEWER || (UNITY_2020_3_OR_NEWER && !UNITY_2020_3_0 && !UNITY_2020_3_1 && !UNITY_2020_3_2 && !UNITY_2020_3_3 && !UNITY_2020_3_4 && !UNITY_2020_3_5 && !UNITY_2020_3_6 && !UNITY_2020_3_7 && !UNITY_2020_3_8 && !UNITY_2020_3_9 && !UNITY_2020_3_10 && !UNITY_2020_3_11 && !UNITY_2020_3_12 && !UNITY_2020_3_13 && !UNITY_2020_3_14 && !UNITY_2020_3_15 && !UNITY_2020_3_16) || (UNITY_2019_4_OR_NEWER && !UNITY_2019_4_0 && !UNITY_2019_4_1 && !UNITY_2019_4_2 && !UNITY_2019_4_3 && !UNITY_2019_4_4 && !UNITY_2019_4_5 && !UNITY_2019_4_6 && !UNITY_2019_4_7 && !UNITY_2019_4_8 && !UNITY_2019_4_9 && !UNITY_2019_4_10 && !UNITY_2019_4_11 && !UNITY_2019_4_12 && !UNITY_2019_4_13 && !UNITY_2019_4_14 && !UNITY_2019_4_15 && !UNITY_2019_4_16 && !UNITY_2019_4_17 && !UNITY_2019_4_18 && !UNITY_2019_4_19 && !UNITY_2019_4_20 && !UNITY_2019_4_21 && !UNITY_2019_4_22 && !UNITY_2019_4_23 && !UNITY_2019_4_24 && !UNITY_2019_4_25 && !UNITY_2019_4_26 && !UNITY_2019_4_27 && !UNITY_2019_4_28 && !UNITY_2019_4_29 && !UNITY_2019_4_30)
	// Unity dropped Android x86 support in 2019, but then added it back in 2021.2.0 and backported to 2020.3.17 and 2019.4.31
	#define AVPROVIDEO_UNITY_ANDROID_X86_SUPPORT
#endif
#if UNITY_2021_2_OR_NEWER || (UNITY_2020_3_OR_NEWER && !UNITY_2020_3_0 && !UNITY_2020_3_1 && !UNITY_2020_3_2 && !UNITY_2020_3_3 && !UNITY_2020_3_4 && !UNITY_2020_3_5 && !UNITY_2020_3_6 && !UNITY_2020_3_7 && !UNITY_2020_3_8 && !UNITY_2020_3_9 && !UNITY_2020_3_10 && !UNITY_2020_3_11 && !UNITY_2020_3_12 && !UNITY_2020_3_13 && !UNITY_2020_3_14 && !UNITY_2020_3_15 && !UNITY_2020_3_16) || (UNITY_2019_4_OR_NEWER && !UNITY_2019_4_0 && !UNITY_2019_4_1 && !UNITY_2019_4_2 && !UNITY_2019_4_3 && !UNITY_2019_4_4 && !UNITY_2019_4_5 && !UNITY_2019_4_6 && !UNITY_2019_4_7 && !UNITY_2019_4_8 && !UNITY_2019_4_9 && !UNITY_2019_4_10 && !UNITY_2019_4_11 && !UNITY_2019_4_12 && !UNITY_2019_4_13 && !UNITY_2019_4_14 && !UNITY_2019_4_15 && !UNITY_2019_4_16 && !UNITY_2019_4_17 && !UNITY_2019_4_18 && !UNITY_2019_4_19 && !UNITY_2019_4_20 && !UNITY_2019_4_21 && !UNITY_2019_4_22 && !UNITY_2019_4_23 && !UNITY_2019_4_24 && !UNITY_2019_4_25 && !UNITY_2019_4_26 && !UNITY_2019_4_27 && !UNITY_2019_4_28 && !UNITY_2019_4_29 && !UNITY_2019_4_30)
	// Unity added Android x86_64 support in 2021.2.0 and backported to 2020.3.17 and 2019.4.31
	#define AVPROVIDEO_UNITY_ANDROID_X8664_SUPPORT
#endif
#if UNITY_2019_1_OR_NEWER
	#define AVPROVIDEO_UNITY_UWP_ARM64_SUPPORT
#endif
#if UNITY_2018_1_OR_NEWER
	#define AVPROVIDEO_UNITY_BUILDWITHREPORT_SUPPORT
#endif

using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
#if AVPROVIDEO_UNITY_BUILDWITHREPORT_SUPPORT
using UnityEditor.Build.Reporting;
#endif
using System.Collections.Generic;
using System.IO;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// <summary>
	/// Some versions of Unity do not support specific CPU architectures for plugin files
	/// so this Build Preprocessor checks the plugin files for those and either disables
	/// them if their arch is not supported, or assigns the correct arch and enables them
	/// </summary>
	public class PluginProcessor : 
	#if AVPROVIDEO_UNITY_BUILDWITHREPORT_SUPPORT
		IPreprocessBuildWithReport
	#else
		IPreprocessBuild
	#endif
	{
		internal class CpuArchitecture
		{
			internal CpuArchitecture(string code, bool isSupportedByThisUnityVersion)
			{
				_code = code;
				_isSupportedByThisUnityVersion = isSupportedByThisUnityVersion;
			}
			private string _code;
			private bool _isSupportedByThisUnityVersion;

			internal string Code()
			{
				return _code;
			}

			internal bool IsSupportedByThisUnityVersion()
			{
				return _isSupportedByThisUnityVersion;
			}
		}
		
		internal class PluginFile
		{
			internal PluginFile(BuildTarget buildTarget, string relativeFilePath, bool supportsEditor, CpuArchitecture cpuArchitecture)
			{
				_buildTarget = buildTarget;
				_relativeFilePath = relativeFilePath;
				_cpuArchitecture = cpuArchitecture;
				_supportsEditor = supportsEditor;
			}

			internal bool IsBuildTarget(BuildTarget buildTarget)
			{
				return (_buildTarget == buildTarget);
			}

			internal BuildTarget BuildTarget()
			{
				return _buildTarget;
			}

			internal bool IsForFile(string path)
			{
				return path.Replace("\\", "/").Contains(_relativeFilePath);
			}

			internal bool IsSupportedByThisUnityVersion()
			{
				return _cpuArchitecture.IsSupportedByThisUnityVersion();
			}

			internal string CpuArchitectureCode()
			{
				return _cpuArchitecture.Code();
			}

			internal bool SupportsEditor()
			{
				return _supportsEditor;
			}

			private BuildTarget _buildTarget;
			private string _relativeFilePath;
			private CpuArchitecture _cpuArchitecture;
			private bool _supportsEditor;
		}

		private static List<PluginFile> _pluginFiles = new List<PluginFile>(32);

		internal static void AddPluginFiles(BuildTarget buildTarget, string[] filenames, string folderPrefix, bool supportsEditor, CpuArchitecture cpuArchitecture)
		{
			foreach (string filename in filenames)
			{
				_pluginFiles.Add(new PluginFile(buildTarget, folderPrefix + filename, supportsEditor, cpuArchitecture));
			}
		}

		internal static void AddPlugins_Android()
		{
			#if AVPROVIDEO_UNITY_ANDROID_ARM64_SUPPORT
			const bool IsAndroidArm64Supported = true;
			#else
			const bool IsAndroidArm64Supported = false;
			#endif
			#if AVPROVIDEO_UNITY_ANDROID_X86_SUPPORT
			const bool IsAndroidX86Supported = true;
			#else
			const bool IsAndroidX86Supported = false;
			#endif
			#if AVPROVIDEO_UNITY_ANDROID_X8664_SUPPORT
			const bool IsAndroidX8664Supported = true;
			#else
			const bool IsAndroidX8664Supported = false;
			#endif
			string[] filenames = {
				"libAudio360.so",
				"libAudio360-JNI.so",
				"libAVProVideo2Native.so",
				"libopus.so",
				"libopusJNI.so",
				"libresample-rh.so",
				"libsamplerate-android.so",
				"libssrc-android.so",
			};
			BuildTarget target = BuildTarget.Android;
			AddPluginFiles(target, filenames, "Android/libs/armeabi-v7a/", false, new CpuArchitecture("ARMv7", true));
			AddPluginFiles(target, filenames, "Android/libs/arm64-v8a/", false, new CpuArchitecture("ARM64", IsAndroidArm64Supported));
			AddPluginFiles(target, filenames, "Android/libs/x86/", false, new CpuArchitecture("X86", IsAndroidX86Supported));
			AddPluginFiles(target, filenames, "Android/libs/x86_64/", false, new CpuArchitecture("X86_64", IsAndroidX8664Supported));
		}

		internal static void AddPlugins_UWP()
		{
			#if AVPROVIDEO_UNITY_UWP_ARM64_SUPPORT
			const bool IsUwpArm64Supported = true;
			#else
			const bool IsUwpArm64Supported = false;
			#endif

			string[] filenames = {
				"Audio360.dll",
				"AVProVideo.dll",
				"AVProVideoWinRT.dll",
			};
			BuildTarget target = BuildTarget.WSAPlayer;
			AddPluginFiles(target, filenames, "WSA/UWP/ARM/", false, new CpuArchitecture("ARM", true));
			AddPluginFiles(target, filenames, "WSA/UWP/ARM64/", false, new CpuArchitecture("ARM64", IsUwpArm64Supported));
			AddPluginFiles(target, filenames, "WSA/UWP/x86/", false, new CpuArchitecture("X86", true));
			AddPluginFiles(target, filenames, "WSA/UWP/x86_64/", false, new CpuArchitecture("X64", true));
		}

		private static void BuildPluginFileList()
		{
			_pluginFiles.Clear();
			AddPlugins_Android();
			AddPlugins_UWP();
		}

        private class SFileToDelete
        {
            public SFileToDelete(string fn)
            {
                filename = fn;
                fullPath = "";
                found = false;
            }

            public string filename;
            public string fullPath;
            public bool found;
        };

        private static void RemoveLegacyPluginFiles()
        {
            List<SFileToDelete> aFilesToDelete = new List<SFileToDelete>();

#if (UNITY_EDITOR && UNITY_ANDROID)
            aFilesToDelete.Add( new SFileToDelete( "Android/guava-27.1-android.jar" ) );
#endif

            if ( aFilesToDelete.Count > 0 )
            {
                int iNumFoundFilesToDelete = 0;
                string aFilesToDeleteString = "";

                PluginImporter[] importers = PluginImporter.GetAllImporters();
                foreach (PluginImporter pi in importers)
                {
                    foreach( SFileToDelete fileToDelete in aFilesToDelete )
                    {
                        string pluginFilename = pi.assetPath;
                        pluginFilename.Replace("\\", "/");
                        if( pluginFilename.Contains( fileToDelete.filename ) )
                        {
                            fileToDelete.fullPath = pi.assetPath;
                            fileToDelete.found = true;

                            if( iNumFoundFilesToDelete > 0 )
                            {
                                aFilesToDeleteString += "\n";
                            }
                            aFilesToDeleteString += pi.assetPath;
                            ++iNumFoundFilesToDelete;
                        }
                    }
                }

                if( iNumFoundFilesToDelete > 0 )
                {
                    string message = ( iNumFoundFilesToDelete == 1 ) ? "A legacy AVPro Video plugin file has been found that requires deleting in order to build." : "Legacy AVPro Video plugin files have been found that require deleting in order to build.";
                    Debug.Log("[AVProVideo] " + message + " Files: " + aFilesToDeleteString );
                    if ( EditorUtility.DisplayDialog( "AVPro Video Legacy File", message + "\n\nDelete the following files?\n\n" + aFilesToDeleteString, "Delete", "Ignore" ) )
                    {
                        foreach( SFileToDelete fileToDelete in aFilesToDelete )
                        {
                            bool bDeleted = AssetDatabase.DeleteAsset( fileToDelete.fullPath );
                            if( bDeleted )
                            {
                                Debug.Log( "[AVProVideo] Deleting " + fileToDelete.fullPath );
                            }
                        }
                    }
                }
            }
        }

        public int callbackOrder { get { return 0; } }

#if AVPROVIDEO_UNITY_BUILDWITHREPORT_SUPPORT
		public void OnPreprocessBuild(BuildReport report)
		{
            RemoveLegacyPluginFiles();

            BuildPluginFileList();
			CheckNativePlugins(report.summary.platform);
		}
#else
		public void OnPreprocessBuild(BuildTarget target, string path)
		{
            RemoveLegacyPluginFiles();

			BuildPluginFileList();
			CheckNativePlugins(target);
		}
#endif

		internal static void CheckNativePlugins(BuildTarget target)
		{
			PluginImporter[] importers = PluginImporter.GetAllImporters();
			foreach (PluginImporter pi in importers)
			{
				// Currently we're only interested in native plugins
				if (!pi.isNativePlugin) continue;

				// Skip plugins that aren't in the AVProVideo path
				// NOTE: This is commented out for now to allow the case where users have moved the plugin files to another folder.
				// Eventually might need a more robust method, perhaps using GUIDS
				//if (!pi.assetPath.Contains("AVProVideo")) continue;

				foreach (PluginFile pluginFile in _pluginFiles)
				{
					if (pluginFile.IsBuildTarget(target) && 
						pluginFile.IsForFile(pi.assetPath))
					{
						pi.SetCompatibleWithAnyPlatform(false);
						if (pluginFile.IsSupportedByThisUnityVersion())
						{
							Debug.Log("[AVProVideo] Enabling " + pluginFile.CpuArchitectureCode() + " " + pi.assetPath);
							pi.SetCompatibleWithEditor(pluginFile.SupportsEditor());
							pi.SetCompatibleWithPlatform(pluginFile.BuildTarget(), true);
							pi.SetPlatformData(pluginFile.BuildTarget(), "CPU", pluginFile.CpuArchitectureCode());
						}
						else
						{
							pi.SetCompatibleWithEditor(false);
							pi.SetCompatibleWithPlatform(pluginFile.BuildTarget(), false);
							pi.SetPlatformData(pluginFile.BuildTarget(), "CPU", "");
							Debug.Log("[AVProVideo] Disabling " + pluginFile.CpuArchitectureCode() + " " + pi.assetPath);
						}
						pi.SaveAndReimport();
						break;
					}
				}
			}
		}
	}
}