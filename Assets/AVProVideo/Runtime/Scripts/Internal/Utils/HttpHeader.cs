using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2020-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	[System.Serializable]
	public struct HttpHeader
	{
		public string name;
		public string value;

		public HttpHeader(string name, string value) { this.name = name; this.value = value; }

		public bool IsComplete()
		{
			return (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value));
		}

		public string ToValidatedString()
		{
			string result = null;
			if (IsComplete())
			{
				if (IsValid())
				{
					result = string.Format("{0}:{1}\r\n", name, value);
				}
			}
			return result;
		}

		public static bool IsValid(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				if (!IsAscii(text)) return false;
				if (text.Contains("\r") || text.Contains("\n")) return false;
			}
			return true;
		}

		private static bool IsAscii(string text)
		{
			foreach (char c in text)
			{
				if (c >= 128) {
					return false;
				}
			}
			return true;
		}

		private bool IsValid()
		{
			if (!IsValid(name) || !IsValid(value))
			{
				return false;
			}
			// TODO: check via regular expression
			return true;
		}
	}

	/// <summary>
	/// Data for handling custom HTTP header fields
	/// </summary>
	[System.Serializable]
	public class HttpHeaderData : IEnumerable
	{
		[SerializeField]
		private List<HttpHeader> httpHeaders = new List<HttpHeader>();

		public IEnumerator GetEnumerator()
		{
			return httpHeaders.GetEnumerator();
		}

		public HttpHeader this[int index]
		{
			get
			{
				return httpHeaders[index];
			}
		}

		public void Clear()
		{
			httpHeaders.Clear();
		}

		public void Add(string name, string value)
		{
			httpHeaders.Add(new HttpHeader(name, value));
		}

		public bool IsModified()
		{
			return (httpHeaders != null && httpHeaders.Count > 0);
		}

		public string ToValidatedString()
		{
			string result = string.Empty;
			foreach (HttpHeader header in httpHeaders)
			{
				if (header.IsComplete())
				{
					string line = header.ToValidatedString();
					if (!string.IsNullOrEmpty(line))
					{
						result += line;
					}
					else
					{
						Debug.LogWarning("[AVProVideo] Custom HTTP header field ignored due to invalid format");
					}
				}
			}
			return result;
		}
	}
}
