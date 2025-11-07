using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2020-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Data for handling authentication of encrypted AES-128 HLS streams
	/// </summary>
	[System.Serializable]
	public class KeyAuthData : ISerializationCallbackReceiver
	{
		public string keyServerToken = null;

		//public string keyServerURLOverride = null;

		[SerializeField, Multiline]
		private string overrideDecryptionKeyBase64 = null;
		public byte[] overrideDecryptionKey = null;

		public bool IsModified()
		{
			return (overrideDecryptionKey != null && overrideDecryptionKey.Length > 0) 
					|| (string.IsNullOrEmpty(overrideDecryptionKeyBase64) == false);
		}

		public void OnBeforeSerialize()
		{
			if (overrideDecryptionKey != null && !string.IsNullOrEmpty(overrideDecryptionKeyBase64))
			{
				overrideDecryptionKey = null;
			}
		}

		public void OnAfterDeserialize()
		{
			if (string.IsNullOrEmpty(overrideDecryptionKeyBase64)) return;

			// Convert overrideDecryptionKeyBase64 to overrideDecryptionKey
			try
			{
				overrideDecryptionKey = System.Convert.FromBase64String(overrideDecryptionKeyBase64);
				if (overrideDecryptionKey != null && overrideDecryptionKey.Length > 0)
				{
					overrideDecryptionKeyBase64 = null;
				}
			}
			catch (System.FormatException)
			{

			}
		}
	}
}