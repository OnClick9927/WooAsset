using UnityEngine;
using System.Collections;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	public partial class MediaPlayer : MonoBehaviour
	{
		public bool EnableSubtitles(MediaPath mediaPath)
		{
			bool result = false;
			if (_subtitlesInterface != null)
			{
				if (mediaPath != null && !string.IsNullOrEmpty(mediaPath.Path))
				{
					string fullPath = mediaPath.GetResolvedFullPath();

					bool checkForFileExist = true;
					if (fullPath.Contains("://"))
					{
						checkForFileExist = false;
					}
#if (!UNITY_EDITOR && UNITY_ANDROID)
					checkForFileExist = false;
#endif

					if (checkForFileExist && !System.IO.File.Exists(fullPath))
					{
						Debug.LogError("[AVProVideo] Subtitle file not found: " + fullPath, this);
					}
					else
					{
						Helper.LogInfo("Opening subtitles " + fullPath, this);

						_previousSubtitleIndex = -1;

						try
						{
							if (fullPath.Contains("://"))
							{
								// Use coroutine and WWW class for loading
								if (_loadSubtitlesRoutine != null)
								{
									StopCoroutine(_loadSubtitlesRoutine);
									_loadSubtitlesRoutine = null;
								}
								_loadSubtitlesRoutine = StartCoroutine(LoadSubtitlesCoroutine(fullPath, mediaPath));
							}
							else
							{
								// Load directly from file
								string subtitleData = System.IO.File.ReadAllText(fullPath);
								if (_subtitlesInterface.LoadSubtitlesSRT(subtitleData))
								{
									_subtitlePath = mediaPath;
									_sideloadSubtitles = false;
									result = true;
								}
								else
								{
									Debug.LogError("[AVProVideo] Failed to load subtitles" + fullPath, this);
								}
							}

						}
						catch (System.Exception e)
						{
							Debug.LogError("[AVProVideo] Failed to load subtitles " + fullPath, this);
							Debug.LogException(e, this);
						}
					}
				}
				else
				{
					Debug.LogError("[AVProVideo] No subtitle file path specified", this);
				}
			}
			else
			{
				_queueSubtitlePath = mediaPath;
			}

			return result;
		}

		private IEnumerator LoadSubtitlesCoroutine(string url, MediaPath mediaPath)
		{
			UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(url);
			#if UNITY_2017_2_OR_NEWER
			yield return www.SendWebRequest();
			#else
			yield return www.Send();
			#endif

			string subtitleData = string.Empty;

			#if UNITY_2020_1_OR_NEWER
			if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
			#elif UNITY_2017_1_OR_NEWER
			if (!www.isNetworkError)
			#else
			if (!www.isError)
			#endif
			{
				subtitleData = ((UnityEngine.Networking.DownloadHandler)www.downloadHandler).text;
			}
			else
			{
				Debug.LogError("[AVProVideo] Error loading subtitles '" + www.error + "' from " + url);
			}

			if (_subtitlesInterface.LoadSubtitlesSRT(subtitleData))
			{
				_subtitlePath = mediaPath;
				_sideloadSubtitles = false;
			}
			else
			{
				Debug.LogError("[AVProVideo] Failed to load subtitles" + url, this);
			}

			_loadSubtitlesRoutine = null;

			www.Dispose();
		}

		public void DisableSubtitles()
		{
			if (_loadSubtitlesRoutine != null)
			{
				StopCoroutine(_loadSubtitlesRoutine);
				_loadSubtitlesRoutine = null;
			}

			if (_subtitlesInterface != null)
			{
				_previousSubtitleIndex = -1;
				_sideloadSubtitles = false;
				_subtitlesInterface.LoadSubtitlesSRT(string.Empty);
			}
			else
			{
				_queueSubtitlePath = null;
			}
		}
	}
}