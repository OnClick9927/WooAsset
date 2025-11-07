using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	public partial class MediaPlayer : MonoBehaviour, ISerializationCallbackReceiver
	{
		#region Upgrade from Version 1.x
		[SerializeField, HideInInspector]
		private string m_VideoPath;
		[SerializeField, HideInInspector]
		private FileLocation m_VideoLocation = FileLocation.RelativeToStreamingAssetsFolder;

		private enum FileLocation
		{
			AbsolutePathOrURL,
			RelativeToProjectFolder,
			RelativeToStreamingAssetsFolder,
			RelativeToDataFolder,
			RelativeToPersistentDataFolder,
		}

		/*
		[SerializeField, HideInInspector]
		private StereoPacking m_StereoPacking;
		[SerializeField, HideInInspector]
		private AlphaPacking m_AlphaPacking;
		*/

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			/*
			m_StereoPacking = _fallbackMediaHints.stereoPacking;
			m_AlphaPacking = _fallbackMediaHints.alphaPacking;
			*/
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (!string.IsNullOrEmpty(m_VideoPath))
			{
				MediaPathType mediaPathType = MediaPathType.AbsolutePathOrURL;
				switch (m_VideoLocation)
				{
					default:
					case FileLocation.AbsolutePathOrURL:
						mediaPathType = MediaPathType.AbsolutePathOrURL;
						break;
					case FileLocation.RelativeToProjectFolder:
						mediaPathType = MediaPathType.RelativeToProjectFolder;
						break;
					case FileLocation.RelativeToStreamingAssetsFolder:
						mediaPathType = MediaPathType.RelativeToStreamingAssetsFolder;
						break;
					case FileLocation.RelativeToDataFolder:
						mediaPathType = MediaPathType.RelativeToDataFolder;
						break;
					case FileLocation.RelativeToPersistentDataFolder:
						mediaPathType = MediaPathType.RelativeToPersistentDataFolder;
						break;
				}
				_mediaPath = new MediaPath(m_VideoPath, mediaPathType);
				_mediaSource = MediaSource.Path;
				m_VideoPath = null;
			}

			/*
			if (m_StereoPacking != _fallbackMediaHints.stereoPacking)
			{
				_fallbackMediaHints.stereoPacking = m_StereoPacking;
			}
			if (m_AlphaPacking != _fallbackMediaHints.alphaPacking)
			{
				_fallbackMediaHints.alphaPacking = m_AlphaPacking;
			}
			*/
		}
		#endregion	// Upgrade from Version 1.x
	}
}