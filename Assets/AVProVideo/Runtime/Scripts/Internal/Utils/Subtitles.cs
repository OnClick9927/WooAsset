using UnityEngine;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	public class Subtitle
	{
		public int index;
		// Rich string can contain <font color=""> <u> etc
		public string text;
		public double timeStart, timeEnd;

		public bool IsBefore(double time)
		{
			return (time > timeStart && time > timeEnd);
		}

		public bool IsTime(double time)
		{
			return (time >= timeStart && time < timeEnd);
		}
	}

    public class SubtitlePlayer
    {
        // min time, max time
        // set time
        // event for change(subs added, subs removed)
        // list of subs on
    }

    public class SubtitleUtils
    {
		/// <summary>
		/// Parse time in format: 00:00:48,924 and convert to seconds
		/// </summary>
		private static double ParseTimeToSeconds(string text)
		{
			double result = 0.0;

			string[] digits = text.Split(new char[] { ':', ',' });

			if (digits.Length == 4)
			{
				int hours = int.Parse(digits[0]);
				int minutes = int.Parse(digits[1]);
				int seconds = int.Parse(digits[2]);
				int milliseconds = int.Parse(digits[3]);

				result = (milliseconds / 1000.0) + (seconds + (minutes + (hours * 60)) * 60);
			}

			return result;
		}

		/// <summary>
		/// Parse subtitles in the SRT format and convert to a list of ordered Subtitle objects
		/// </summary>
		public static List<Subtitle> ParseSubtitlesSRT(string data)
		{
			List<Subtitle> result = null;

			if (!string.IsNullOrEmpty(data))
			{
				data = data.Trim();
				var rx = new System.Text.RegularExpressions.Regex("\n\r|\r\n|\n|\r");
				string[] lines = rx.Split(data);

				if (lines.Length >= 3)
				{
					result = new List<Subtitle>(256);

					int count = 0;
					int index = 0;
					Subtitle subtitle = null;
					for (int i = 0; i < lines.Length; i++)
					{
						if (index == 0)
						{
							subtitle = new Subtitle();
							subtitle.index = count;// int.Parse(lines[i]);
						}
						else if (index == 1)
						{
							string[] times = lines[i].Split(new string[] { " --> " }, System.StringSplitOptions.RemoveEmptyEntries);
							if (times.Length == 2)
							{
								subtitle.timeStart = ParseTimeToSeconds(times[0]);
								subtitle.timeEnd = ParseTimeToSeconds(times[1]);
							}
							else
							{
								throw new System.FormatException("SRT format doesn't appear to be valid");
							}
						}
						else
						{
							if (!string.IsNullOrEmpty(lines[i]))
							{
								if (index == 2)
								{
									subtitle.text = lines[i];
								}
								else
								{
									subtitle.text += "\n" + lines[i];
								}
							}
						}

						if (string.IsNullOrEmpty(lines[i]) && index > 1)
						{
							result.Add(subtitle);
							index = 0;
							count++;
							subtitle = null;
						}
						else
						{
							index++;
						}
					}

					// Handle the last one
					if (subtitle != null)
					{
						result.Add(subtitle);
						subtitle = null;
					}
				}
				else
				{
					Debug.LogWarning("[AVProVideo] SRT format doesn't appear to be valid");
				}
			}

			return result;
		}
	}
}