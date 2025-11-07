using System.Collections;
using System.Collections.Generic;
using System.Text;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	public class TextCue
	{
		private TextCue() { }

		internal TextCue(string text) 
		{
			Text = text;
		}

		public string Text { get; private set; }
	}

	public partial class BaseMediaPlayer : ITextTracks
	{
		protected TextCue _currentTextCue = null;
		public TextCue				GetCurrentTextCue() { return _currentTextCue; }	// Returns null when there is no active text

		protected bool UpdateTextCue(bool force = false)
		{
			bool result = false;
			// Has it changed since the last 'tick'
			if (force || InternalIsChangedTextCue())
			{
				_currentTextCue = null;
				string text = InternalGetCurrentTextCue();
				if (!string.IsNullOrEmpty(text))
				{
					_currentTextCue = new TextCue(text);
				}
				result = true;
			}
			return result;
		}

		internal abstract bool InternalIsChangedTextCue();
		internal abstract string InternalGetCurrentTextCue();
	}
}