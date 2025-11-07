using UnityEngine;
using UnityEditor;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// <summary>
	/// Editor for the MediaPlayer component
	/// </summary>
	public partial class MediaPlayerEditor : UnityEditor.Editor
	{
		private static bool _allowDeveloperMode = false;
		private static bool _showUltraOptions = true;

		private AnimCollapseSection _sectionDevModeState;
		private AnimCollapseSection _sectionDevModeTexture;
		private AnimCollapseSection _sectionDevModeHapNotchLCDecoder;
		private AnimCollapseSection _sectionDevModeBufferedFrames;
		private AnimCollapseSection _sectionDevModePlaybackQuality;

		private static readonly GUIContent _guiTextMetaData = new GUIContent("MetaData");
		private static readonly GUIContent _guiTextPaused = new GUIContent("Paused");
		private static readonly GUIContent _guiTextPlaying = new GUIContent("Playing");
		private static readonly GUIContent _guiTextSeeking = new GUIContent("Seeking");
		private static readonly GUIContent _guiTextBuffering = new GUIContent("Buffering");
		private static readonly GUIContent _guiTextStalled = new GUIContent("Stalled");
		private static readonly GUIContent _guiTextFinished = new GUIContent("Finished");
		private static readonly GUIContent _guiTextTimeColon= new GUIContent("Time: ");
		private static readonly GUIContent _guiTextFrameColon = new GUIContent("Frame: ");

		private static readonly GUIContent _guiTextFrameDec = new GUIContent("<");
		private static readonly GUIContent _guiTextFrameInc = new GUIContent(">");
		private static readonly GUIContent _guiTextSelectTexture = new GUIContent("Select Texture");
		private static readonly GUIContent _guiTextSaveFramePNG = new GUIContent("Save Frame PNG");
		private static readonly GUIContent _guiTextSaveFrameEXR = new GUIContent("Save Frame EXR");

		private static readonly GUIContent _guiTextDecodeStats = new GUIContent("Decode Stats");
		private static readonly GUIContent _guiTextParallelFrames = new GUIContent("Parallel Frames");
		private static readonly GUIContent _guiTextDecodedFrames = new GUIContent("Decoded Frames");
		private static readonly GUIContent _guiTextDroppedFrames = new GUIContent("Dropped Frames");

		private static readonly GUIContent _guiTextBufferedFrames = new GUIContent("Buffered Frames");
		private static readonly GUIContent _guiTextFreeFrames = new GUIContent("Free Frames");
		//private static readonly GUIContent _guiTextDisplayTimestamp = new GUIContent("Display Timstamp");
		//private static readonly GUIContent _guiTextMinTimestamp = new GUIContent("Min Timstamp");
		//private static readonly GUIContent _guiTextMaxTimestamp = new GUIContent("Max Timstamp");
		private static readonly GUIContent _guiTextFlush = new GUIContent("Flush");
		private static readonly GUIContent _guiTextReset = new GUIContent("Reset");

		private void OnInspectorGUI_DevMode_State()
		{
			MediaPlayer mediaPlayer = (this.target) as MediaPlayer;
			if (mediaPlayer.Control != null)
			{
				// State

				GUIStyle style = GUI.skin.button;
				using (HorizontalFlowScope flow = new HorizontalFlowScope(Screen.width))
				{
					flow.AddItem(_guiTextMetaData, style);
					GUI.color = mediaPlayer.Control.HasMetaData() ? Color.green : Color.white;
					GUILayout.Toggle(true, _guiTextMetaData, style);

					flow.AddItem(_guiTextPaused, style);
					GUI.color = mediaPlayer.Control.IsPaused() ? Color.green : Color.white;
					GUILayout.Toggle(true, _guiTextPaused, style);

					flow.AddItem(_guiTextPlaying, style);
					GUI.color = mediaPlayer.Control.IsPlaying() ? Color.green : Color.white;
					GUILayout.Toggle(true, _guiTextPlaying, style);

					flow.AddItem(_guiTextSeeking, style);
					GUI.color = mediaPlayer.Control.IsSeeking() ? Color.green : Color.white;
					GUILayout.Toggle(true, _guiTextSeeking, style);

					flow.AddItem(_guiTextBuffering, style);
					GUI.color = mediaPlayer.Control.IsBuffering() ? Color.green : Color.white;
					GUILayout.Toggle(true, _guiTextBuffering, style);

					flow.AddItem(_guiTextStalled, style);
					GUI.color = mediaPlayer.Info.IsPlaybackStalled() ? Color.green : Color.white;
					GUILayout.Toggle(true, _guiTextStalled, style);

					flow.AddItem(_guiTextFinished, style);
					GUI.color = mediaPlayer.Control.IsFinished() ? Color.green : Color.white;
					GUILayout.Toggle(true, _guiTextFinished, style);
				}
				GUI.color = Color.white;

				// Time, FPS, Frame stepping
				GUILayout.BeginHorizontal();
				GUILayout.Label(_guiTextTimeColon);
				GUILayout.Label(mediaPlayer.Control.GetCurrentTime().ToString());
				GUILayout.FlexibleSpace();
				GUILayout.Label(_guiTextFrameColon);
				GUILayout.Label(mediaPlayer.Control.GetCurrentTimeFrames().ToString());
				EditorGUI.BeginDisabledGroup(mediaPlayer.Info.GetVideoFrameRate() <= 0f);
				if (GUILayout.Button(_guiTextFrameDec))
				{
					mediaPlayer.Control.SeekToFrameRelative(-1);
				}
				if (GUILayout.Button(_guiTextFrameInc))
				{
					mediaPlayer.Control.SeekToFrameRelative(1);
				}
				EditorGUI.EndDisabledGroup();
				GUILayout.EndHorizontal();
			}
		}

		private void OnInspectorGUI_DevMode_Texture()
		{
			MediaPlayer mediaPlayer = (this.target) as MediaPlayer;
			if (mediaPlayer.Control != null)
			{
				// Raw texture preview
				if (mediaPlayer.TextureProducer != null)
				{
					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.FlexibleSpace();
					for (int i = 0; i < mediaPlayer.TextureProducer.GetTextureCount(); i++)
					{
						Texture texture = mediaPlayer.TextureProducer.GetTexture(i);
						if (texture != null)
						{
							GUILayout.BeginVertical();
							Rect textureRect = GUILayoutUtility.GetRect(128f, 128f);
							if (Event.current.type == EventType.Repaint)
							{
								GUI.color = Color.gray;
								EditorGUI.DrawTextureTransparent(textureRect, Texture2D.blackTexture, ScaleMode.StretchToFill);
								GUI.color = Color.white;
							}
							GUI.DrawTexture(textureRect, texture, ScaleMode.ScaleToFit, false);
							GUILayout.Label(texture.width + "x" + texture.height + " ");
							if (GUILayout.Button(_guiTextSelectTexture, GUILayout.ExpandWidth(false)))
							{
								Selection.activeObject = texture;
							}
							GUILayout.EndVertical();
						}
					}
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();

					GUILayout.Label("Updates: " + mediaPlayer.TextureProducer.GetTextureFrameCount());
					GUILayout.Label("TimeStamp: " + mediaPlayer.TextureProducer.GetTextureTimeStamp());

					GUILayout.BeginHorizontal();
					if (GUILayout.Button(_guiTextSaveFramePNG, GUILayout.ExpandWidth(true)))
					{
						mediaPlayer.SaveFrameToPng();
					}
					if (GUILayout.Button(_guiTextSaveFrameEXR, GUILayout.ExpandWidth(true)))
					{
						mediaPlayer.SaveFrameToExr();
					}
					GUILayout.EndHorizontal();
				}
			}
		}

		private void OnInspectorGUI_DevMode_HapNotchLCDecoder()
		{
			MediaPlayer mediaPlayer = (this.target) as MediaPlayer;
			if (mediaPlayer.Info != null)
			{
				int activeDecodeThreadCount = 0;
				int decodedFrameCount = 0;
				int droppedFrameCount = 0;
				if (mediaPlayer.Info.GetDecoderPerformance(ref activeDecodeThreadCount, ref decodedFrameCount, ref droppedFrameCount))
				{
					GUILayout.Label(_guiTextDecodeStats);
					EditorGUI.indentLevel++;
					EditorGUILayout.Slider(_guiTextParallelFrames, activeDecodeThreadCount, 0f, mediaPlayer.PlatformOptionsWindows.parallelFrameCount);
					EditorGUILayout.Slider(_guiTextDecodedFrames, decodedFrameCount, 0f, mediaPlayer.PlatformOptionsWindows.prerollFrameCount * 2);
					EditorGUILayout.IntField(_guiTextDroppedFrames, droppedFrameCount);
					EditorGUI.indentLevel--;
				}
			}
		}

		private float _lastBufferedFrameCount;
		private float _lastFreeFrameCount;

		private void OnInspectorGUI_DevMode_BufferedFrames()
		{
			MediaPlayer mediaPlayer = (this.target) as MediaPlayer;
			if (mediaPlayer.Control != null)
			{
				IBufferedDisplay bufferedDisplay = mediaPlayer.BufferedDisplay;
				if (bufferedDisplay != null)
				{
					BufferedFramesState state = bufferedDisplay.GetBufferedFramesState();

					GUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel(_guiTextBufferedFrames);
					Rect progressRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
					EditorGUI.ProgressBar(progressRect, _lastBufferedFrameCount, state.bufferedFrameCount.ToString());
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel(_guiTextFreeFrames);
					progressRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
					EditorGUI.ProgressBar(progressRect, _lastFreeFrameCount, state.freeFrameCount.ToString());
					GUILayout.EndHorizontal();

					_lastBufferedFrameCount = Mathf.MoveTowards(_lastBufferedFrameCount, state.bufferedFrameCount / 12f, Time.deltaTime);
					_lastFreeFrameCount = Mathf.MoveTowards(_lastFreeFrameCount, state.freeFrameCount / 12f, Time.deltaTime);

					//EditorGUILayout.LabelField(_guiTextDisplayTimestamp, new GUIContent(mediaPlayer.TextureProducer.GetTextureTimeStamp().ToString() + " " + (mediaPlayer.TextureProducer.GetTextureTimeStamp() / Helper.SecondsToHNS).ToString() + "s"));
					//EditorGUILayout.LabelField(_guiTextMinTimestamp, new GUIContent(state.minTimeStamp.ToString() + " " + (state.minTimeStamp / Helper.SecondsToHNS).ToString() + "s"));
					//EditorGUILayout.LabelField(_guiTextMaxTimestamp, new GUIContent(state.maxTimeStamp.ToString() + " " + (state.maxTimeStamp / Helper.SecondsToHNS).ToString() + "s"));
					if (GUILayout.Button(_guiTextFlush))
					{
						// Seek causes a flush
						mediaPlayer.Control.Seek(mediaPlayer.Control.GetCurrentTime());
					}
				}
			}
		}

		private void OnInspectorGUI_DevMode_PresentationQuality()
		{
			MediaPlayer mediaPlayer = (this.target) as MediaPlayer;
			if (mediaPlayer.Info != null)
			{
				PlaybackQualityStats stats = mediaPlayer.Info.GetPlaybackQualityStats();
				//stats.LogIssues = true;
				stats.LogIssues = EditorGUILayout.Toggle("Log Issues", stats.LogIssues);
				GUILayout.Label("Video", EditorStyles.boldLabel);
				EditorGUI.indentLevel++;
				EditorGUILayout.LabelField("Skipped Frames", stats.SkippedFrames.ToString());
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Duplicate Frames", stats.DuplicateFrames.ToString());
				GUILayout.Label(stats.VSyncStatus);
				GUILayout.EndHorizontal();
				EditorGUILayout.LabelField("Perfect Frames", (stats.PerfectFramesT * 100f).ToString("F2") + "%");
				EditorGUI.indentLevel--;
				GUILayout.Label("Unity", EditorStyles.boldLabel);
				EditorGUI.indentLevel++;
				EditorGUILayout.LabelField("Dropped Frames", stats.UnityDroppedFrames.ToString());
				EditorGUI.indentLevel--;
				if (GUILayout.Button(_guiTextReset))
				{
					stats.Reset();
				}
			}
		}

		private void OnInspectorGUI_Debug()
		{
			MediaPlayer mediaPlayer = (this.target) as MediaPlayer;
			IMediaInfo info = mediaPlayer.Info;
			if (info != null)
			{
				AnimCollapseSection.Show(_sectionDevModeState);
				AnimCollapseSection.Show(_sectionDevModeTexture);
				AnimCollapseSection.Show(_sectionDevModePlaybackQuality);
			}

			if (info != null)
			{
#if UNITY_EDITOR_WIN
				if (mediaPlayer.PlatformOptionsWindows.useHapNotchLC)
				{
					AnimCollapseSection.Show(_sectionDevModeHapNotchLCDecoder);
				}
				if (mediaPlayer.PlatformOptionsWindows.bufferedFrameSelection != BufferedFrameSelectionMode.None)
				{
					AnimCollapseSection.Show(_sectionDevModeBufferedFrames);
				}
#endif
			}
			else
			{
				GUILayout.Label("No media loaded");
			}
		}
	}
}