#if UNITY_ANDROID

using UnityEngine;
using UnityEditor.Android;
using System.IO;
using System.Text;

//-----------------------------------------------------------------------------
// Copyright 2012-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	public class PostProcessBuild_Android : IPostGenerateGradleAndroidProject
	{
		public int callbackOrder { get { return 1; } }

		public void OnPostGenerateGradleAndroidProject( string path )
		{
			GradleProperty( path );
		}

		private void GradleProperty( string path )
		{
#if UNITY_2020_1_OR_NEWER || UNITY_2020_OR_NEWER
			// When using Unity 2020.1 and above it has been seen that the build process overly optimises which causes issues in the ExoPlayer library.
			// To overcome this issue, we need to add 'android.enableDexingArtifactTransform=false' to the gradle.properties.
			// Note that this can be done by the developer at project level already.

			Debug.Log("[AVProVideo] Post-processing Android project: patching gradle.properties");

			StringBuilder stringBuilder = new StringBuilder();

			// Path to gradle.properties
			string filePath = Path.Combine( path, "..", "gradle.properties" );

			if( File.Exists( filePath ) )
			{
				// Load in all the lines in the file
				string[] allLines = File.ReadAllLines( filePath );

				foreach( string line in allLines )
				{
					if( line.Length > 0 )
					{
						// Add everything except enableDexingArtifactTransform
						if ( !line.Contains( "android.enableDexingArtifactTransform" ) )
						{
							stringBuilder.AppendLine( line );
						}
					}
				}
			}

			// Add in line to set enableDexingArtifactTransform to false
			stringBuilder.AppendLine( "android.enableDexingArtifactTransform=false" );

			// Write out the amended file
			File.WriteAllText( filePath, stringBuilder.ToString() );
#endif
		}
	}
}

#endif // UNITY_ANDROID