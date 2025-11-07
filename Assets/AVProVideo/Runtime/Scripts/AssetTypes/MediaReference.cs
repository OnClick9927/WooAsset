using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RenderHeads.Media.AVProVideo
{
	[System.Serializable]
	[CreateAssetMenu(fileName = "MediaReference", menuName = "AVPro Video/Media Reference", order = 51)]
	public class MediaReference : ScriptableObject
	{
		[SerializeField] string _alias = string.Empty;
		public string Alias { get { return _alias; } set { _alias = value; } }

		[SerializeField] MediaPath _mediaPath = new MediaPath();
		public MediaPath MediaPath { get { return _mediaPath; } set { _mediaPath = value; } }

		[Header("Media Hints")]

		[SerializeField] MediaHints _hints = MediaHints.Default;
		public MediaHints Hints { get { return _hints; } set { _hints = value; } }

		[Header("Platform Overrides")]

		[SerializeField] MediaReference _macOS = null;
		[SerializeField] MediaReference _windows = null;
		[SerializeField] MediaReference _android = null;
		[SerializeField] MediaReference _iOS = null;
		[SerializeField] MediaReference _tvOS = null;
		[SerializeField] MediaReference _windowsUWP = null;
		[SerializeField] MediaReference _webGL = null;

#if UNITY_EDITOR
		[SerializeField, HideInInspector] byte[] _preview = null;

		public Texture2D GeneratePreview(Texture2D texture)
		{
			_preview = null;
			if (texture)
			{
				texture.Apply(true, false);
				_preview = texture.GetRawTextureData();
			}
			UnityEditor.EditorUtility.SetDirty(this);
			return texture;
		}

		public bool GetPreview(Texture2D texture)
		{
			if (_preview != null && _preview.Length > 0 && _preview.Length > 128*128*4)
			{
				texture.LoadRawTextureData(_preview);
				texture.Apply(true, false);
				return true;
			}
			return false;
		}
#endif

		public MediaReference GetCurrentPlatformMediaReference()
		{
			MediaReference result = null;

		#if (UNITY_EDITOR_OSX && UNITY_IOS) || (!UNITY_EDITOR && UNITY_IOS)
			result = GetPlatformMediaReference(Platform.iOS);
		#elif (UNITY_EDITOR_OSX && UNITY_TVOS) || (!UNITY_EDITOR && UNITY_TVOS)
			result = GetPlatformMediaReference(Platform.tvOS);
		#elif (UNITY_EDITOR_OSX || (!UNITY_EDITOR && UNITY_STANDALONE_OSX))
			result = GetPlatformMediaReference(Platform.MacOSX);
		#elif (UNITY_EDITOR_WIN) || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
			result = GetPlatformMediaReference(Platform.Windows);
		#elif (!UNITY_EDITOR && UNITY_WSA_10_0)
			result = GetPlatformMediaReference(Platform.WindowsUWP);
		#elif (!UNITY_EDITOR && UNITY_ANDROID)
			result = GetPlatformMediaReference(Platform.Android);
		#elif (!UNITY_EDITOR && UNITY_WEBGL)
			result = GetPlatformMediaReference(Platform.WebGL);
		#endif

			if (result == null)
			{
				result = this;
			}

			return result;
		}

		public MediaReference GetPlatformMediaReference(Platform platform)
		{
			MediaReference result = null;

			switch (platform)
			{
				case Platform.iOS:
					result = _iOS;
					break;
				case Platform.tvOS:
					result = _tvOS;
					break;
				case Platform.MacOSX:
					result = _macOS;
					break;
				case Platform.Windows:
					result = _windows;
					break;
				case Platform.WindowsUWP:
					result = _windowsUWP;
					break;
				case Platform.Android:
					result = _android;
					break;
				case Platform.WebGL:
					result = _webGL;
					break;
			}
			return result;
		}
	}
}