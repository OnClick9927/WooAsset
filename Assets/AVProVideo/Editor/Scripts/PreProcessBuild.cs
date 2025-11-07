#if UNITY_2018_1_OR_NEWER
	#define UNITY_SUPPORTS_BUILD_REPORT
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEditor.Build;
#if UNITY_SUPPORTS_BUILD_REPORT
using UnityEditor.Build.Reporting;
#endif

namespace RenderHeads.Media.AVProVideo.Editor
{
	public class PreProcessBuild : 
		#if UNITY_SUPPORTS_BUILD_REPORT
		IPreprocessBuildWithReport
		#else
		IPreprocessBuild
		#endif
	{
		public int callbackOrder { get { return 0; } }

	#if UNITY_SUPPORTS_BUILD_REPORT
		public void OnPreprocessBuild(BuildReport report)
		{
			OnPreprocessBuild(report.summary.platform, report.summary.outputPath);
		}
	#endif

		public void OnPreprocessBuild(BuildTarget target, string path)
		{
			if (IsTargetMacOS(target) || target == BuildTarget.iOS || target == BuildTarget.tvOS)
			{
				int indexMetal = GetGraphicsApiIndex(target, GraphicsDeviceType.Metal);
				int indexOpenGLCore = GetGraphicsApiIndex(target, GraphicsDeviceType.OpenGLCore);
				int indexOpenGLES2 = GetGraphicsApiIndex(target, GraphicsDeviceType.OpenGLES2);
				int indexOpenGLES3 = GetGraphicsApiIndex(target, GraphicsDeviceType.OpenGLES3);

				if (indexMetal < 0)
				{
					string message = "Metal graphics API is required by AVPro Video.";
					message += "\n\nPlease go to Player Settings > Auto Graphics API and add Metal to the top of the list.";
					ShowAbortDialog(message);
				}

				if (indexOpenGLCore >= 0 && indexMetal >=0 && indexOpenGLCore < indexMetal)
				{
					string message = "OpenGL graphics API is not supported by AVPro Video.";
					message += "\n\nVideo will play but no video frames will be displayed.";
					message += "\n\nPlease go to Player Settings > Auto Graphics API and add Metal to the top of the list.";
					ShowAbortDialog(message);
				}

				if (indexOpenGLES2 >= 0 && indexMetal >=0 && indexOpenGLES2 < indexMetal)
				{
					string message = "OpenGLES2 graphics API is not supported by AVPro Video.";
					message += "\n\nVideo will play but no video frames will be displayed.";
					message += "\n\nPlease go to Player Settings > Auto Graphics API and add Metal to the top of the list.";
					ShowAbortDialog(message);
				}

				if (indexOpenGLES3 >= 0 && indexMetal >=0 && indexOpenGLES3 < indexMetal)
				{
					string message = "OpenGLES3 graphics API is not supported by AVPro Video.";
					message += "\n\nVideo will play but no video frames will be displayed.";
					message += "\n\nPlease go to Player Settings > Auto Graphics API and add Metal to the top of the list.";
					ShowAbortDialog(message);
				}
			}

			int indexVulkan = GetGraphicsApiIndex(target, GraphicsDeviceType.Vulkan);
			if (indexVulkan >= 0)
			{
				string message = "Vulkan graphics API is not supported by AVPro Video.";
				if (target == BuildTarget.Android)
				{
					message += "\n\nPlease go to Player Settings > Android > Auto Graphics API and remove Vulkan from the list.\nOnly OpenGL ES 2.0 and 3.0 are supported on Android.";
				}
				else
				{
					message += "\n\nPlease go to Player Settings > Auto Graphics API and remove Vulkan from the list.";
				}
				ShowAbortDialog(message);
			}
		}

		static void ShowAbortDialog(string message)
		{
			if (!EditorUtility.DisplayDialog("Continue Build?", message, "Continue", "Cancel"))
			{
				throw new BuildFailedException(message);
			}
		}

		static bool IsTargetMacOS(BuildTarget target)
		{
			#if UNITY_2017_3_OR_NEWER
			return (target == BuildTarget.StandaloneOSX);
			#else
			return (target == BuildTarget.StandaloneOSXUniversal || target == BuildTarget.StandaloneOSXIntel);
			#endif
		}

		static int GetGraphicsApiIndex(BuildTarget target, GraphicsDeviceType api)
		{
			int result = -1;
			GraphicsDeviceType[] devices = UnityEditor.PlayerSettings.GetGraphicsAPIs(target);
			for (int i = 0; i < devices.Length; i++)
			{
				if (devices[i] == api)
				{
					result = i;
					break;
				}
			}
			return result;
		}
	}
}