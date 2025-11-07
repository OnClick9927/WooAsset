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
		private static GUIContent FilePathSplitEllipses = new GUIContent("-");
		private static GUIContent _iconPlayButton;
		private static GUIContent _iconPauseButton;
		private static GUIContent _iconSceneViewAudio;
		private static GUIContent _iconProject;
		private static GUIContent _iconRotateTool;

		private static bool _showAlpha = false;
		private static bool _showPreview = false;
		private static Material _materialResolve;
		private static Material _materialIMGUI;
		private static RenderTexture _previewTexture;
		private static float _lastTextureRatio = -1f;
		private static int _previewTextureFrameCount = -1;

		private MediaReference _queuedLoadMediaRef = null;
		private bool _queuedToggleShowPreview = false;

		private void OnInspectorGUI_MediaInfo()
		{
			MediaPlayer media = (this.target) as MediaPlayer;
			IMediaInfo info = media.Info;
			IMediaControl control = media.Control;
			ITextTracks textTracks = media.TextTracks;
			IAudioTracks audioTracks = media.AudioTracks;
			IVideoTracks videoTracks = media.VideoTracks;
			if (info != null)
			{
				if (!info.HasVideo() && !info.HasAudio())// && !info.HasText())
				{
					GUILayout.Label("No media loaded");
				}
				else
				{
					if (info.HasVideo())
					{
						GUILayout.BeginHorizontal();
						{
							string dimensionText = string.Format("{0}x{1}@{2:0.##}", info.GetVideoWidth(), info.GetVideoHeight(), info.GetVideoFrameRate());
							GUILayout.Label(dimensionText);
							GUILayout.FlexibleSpace();
							string rateText = "0.00";
							if (media.Info != null)
							{
								rateText = media.Info.GetVideoDisplayRate().ToString("F2");
							}
							GUILayout.Label(rateText + "FPS");
						}
						GUILayout.EndHorizontal();

						EditorGUILayout.Space();
					}
					if (info.HasVideo())
					{
						VideoTracks tracks = videoTracks.GetVideoTracks();
						if (tracks.Count > 0)
						{
							GUILayout.Label("Video Tracks: " + tracks.Count);
							foreach (VideoTrack track in tracks)
							{
								bool isActiveTrack = (track == videoTracks.GetActiveVideoTrack());
								GUI.color = isActiveTrack?Color.green:Color.white;
								{
									if (GUILayout.Button(track.DisplayName))
									{
										if (isActiveTrack)
										{
											videoTracks.SetActiveVideoTrack(null);
										}
										else
										{
											videoTracks.SetActiveVideoTrack(track);
										}
									}
								}
							}
							GUI.color = Color.white;
							EditorGUILayout.Space();
						}
					}
					if (info.HasAudio())
					{
						AudioTracks tracks = audioTracks.GetAudioTracks();
						if (tracks.Count > 0)
						{
							GUILayout.Label("Audio Tracks: " + tracks.Count);
							foreach (AudioTrack track in tracks)
							{
								bool isActiveTrack = (track == audioTracks.GetActiveAudioTrack());
								GUI.color = isActiveTrack?Color.green:Color.white;
								{
									if (GUILayout.Button(track.DisplayName))
									{
										if (isActiveTrack)
										{
											audioTracks.SetActiveAudioTrack(null);
										}
										else
										{
											audioTracks.SetActiveAudioTrack(track);
										}
									}
								}
							}
							GUI.color = Color.white;

							/*int channelCount = control.GetAudioChannelCount();
							if (channelCount > 0)
							{
								GUILayout.Label("Audio Channels: " + channelCount);
								AudioChannelMaskFlags audioChannels = control.GetAudioChannelMask();
								GUILayout.Label("(" + audioChannels + ")", EditorHelper.IMGUI.GetWordWrappedTextAreaStyle());
							}*/
							EditorGUILayout.Space();
						}
					}
					{
						TextTracks tracks = textTracks.GetTextTracks();
						if (tracks.Count > 0)
						{
							GUILayout.Label("Text Tracks: " + tracks.Count);
							foreach (TextTrack track in tracks)
							{
								bool isActiveTrack = (track == textTracks.GetActiveTextTrack());
								GUI.color = isActiveTrack?Color.green:Color.white;
								{
									if (GUILayout.Button(track.DisplayName))
									{
										if (isActiveTrack)
										{
											textTracks.SetActiveTextTrack(null);
										}
										else
										{
											textTracks.SetActiveTextTrack(track);
										}
									}
								}
							}
							GUI.color = Color.white;

							if (textTracks.GetActiveTextTrack() != null)
							{
								string text = string.Empty;
								if (textTracks.GetCurrentTextCue() != null)
								{
									text = textTracks.GetCurrentTextCue().Text;
									// Clip the text if it is too long
									if (text.Length >= 96)
									{
										text = string.Format("{0}...({1} chars)", text.Substring(0, 96), text.Length);
									}
								}
								GUILayout.Label(text, EditorHelper.IMGUI.GetWordWrappedTextAreaStyle(), GUILayout.Height(48f));
							}
							
							EditorGUILayout.Space();
						}
					}
				}
			}
			else
			{
				GUILayout.Label("No media loaded");
			}
		}

		private void ClosePreview()
		{
			if (_materialResolve)
			{
				DestroyImmediate(_materialResolve); _materialResolve = null;
			}
			if (_materialIMGUI)
			{
				DestroyImmediate(_materialIMGUI); _materialIMGUI = null;
			}
			if (_previewTexture)
			{
				//Debug.Log("closing RESOLVE");
				RenderTexture.ReleaseTemporary(_previewTexture); _previewTexture = null;
			}
		}

		private void RenderPreview(MediaPlayer media)
		{
			int textureFrameCount = media.TextureProducer.GetTextureFrameCount();
			if (textureFrameCount != _previewTextureFrameCount)
			{
				_previewTextureFrameCount = textureFrameCount;

				if (!_materialResolve)
				{
					_materialResolve = VideoRender.CreateResolveMaterial();
				}
				if (!_materialIMGUI)
				{
					_materialIMGUI = VideoRender.CreateIMGUIMaterial();
				}

				VideoRender.SetupMaterialForMedia(_materialResolve, media, -1);

				VideoRender.ResolveFlags resolveFlags = (VideoRender.ResolveFlags.ColorspaceSRGB | VideoRender.ResolveFlags.Mipmaps | VideoRender.ResolveFlags.PackedAlpha | VideoRender.ResolveFlags.StereoLeft);
				_previewTexture = VideoRender.ResolveVideoToRenderTexture(_materialResolve, _previewTexture, media.TextureProducer, resolveFlags);
			}
		}

		private void DrawCenterCroppedLabel(Rect rect, string text)
		{
			if (Event.current.type != EventType.Repaint) return;
			GUIContent textContent = new GUIContent(text);
			Vector2 textSize = GUI.skin.label.CalcSize(textContent);
			if (textSize.x > rect.width)
			{
				float ellipseWidth = GUI.skin.label.CalcSize(FilePathSplitEllipses).x;

				// Left
				Rect rleft = rect;
				rleft.xMax -= (rleft.width / 2f);
				rleft.xMax -= (ellipseWidth / 2f);
				GUI.Label(rleft, textContent);

				// Right
				Rect rRight = rect;
				rRight.xMin += (rRight.width / 2f);
				rRight.xMin += (ellipseWidth / 2f);
				GUI.Label(rRight, textContent, EditorHelper.IMGUI.GetRightAlignedLabelStyle());

				// Center
				Rect rCenter = rect;
				rCenter.xMin += (rect.width / 2f) - (ellipseWidth / 2f);
				rCenter.xMax -= (rect.width / 2f) - (ellipseWidth / 2f);
				GUI.Label(rCenter, FilePathSplitEllipses, EditorHelper.IMGUI.GetCenterAlignedLabelStyle());
			}
			else
			{
				GUI.Label(rect, textContent, EditorHelper.IMGUI.GetCenterAlignedLabelStyle());
			}
		}

		private void OnInspectorGUI_Player(MediaPlayer mediaPlayer, ITextureProducer textureSource)
		{
			EditorGUILayout.BeginVertical(GUI.skin.box);

			Rect titleRect = Rect.zero;
			// Display filename as title of preview
			{
				string mediaFileName = string.Empty;
				if ((MediaSource)_propMediaSource.enumValueIndex == MediaSource.Path)
				{
					mediaFileName = mediaPlayer.MediaPath.Path;
				}
				else if ((MediaSource)_propMediaSource.enumValueIndex == MediaSource.Reference)
				{
					if (_propMediaReference.objectReferenceValue != null)
					{
						mediaFileName = ((MediaReference)_propMediaReference.objectReferenceValue).GetCurrentPlatformMediaReference().MediaPath.Path;
					}
				}

				// Display the file name, cropping if necessary
				if (!string.IsNullOrEmpty(mediaFileName) && 
					(0 > mediaFileName.IndexOfAny(System.IO.Path.GetInvalidPathChars())))
				{
					string text = System.IO.Path.GetFileName(mediaFileName);
					titleRect = GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.label);

					// Draw background
					GUI.Box(titleRect, GUIContent.none, EditorStyles.toolbarButton);
					DrawCenterCroppedLabel(titleRect, text);
				}
			}

			// Toggle preview
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.isMouse)
			{
				if (titleRect.Contains(Event.current.mousePosition))
				{
					_queuedToggleShowPreview = true;
				}
			}

			if (_showPreview)
			{ 
				Texture texture = EditorGUIUtility.whiteTexture;
				float textureRatio = 16f / 9f;

				if (_lastTextureRatio > 0f)
				{
					textureRatio = _lastTextureRatio;
				}
			
				if (textureSource != null && textureSource.GetTexture() != null)
				{
					texture = textureSource.GetTexture();
					if (_previewTexture)
					{
						texture = _previewTexture;
					}
					_lastTextureRatio = textureRatio = (float)texture.width / (float)texture.height;
				}

				// Reserve rectangle for texture
				//GUILayout.BeginHorizontal(GUILayout.MaxHeight(Screen.height / 2f), GUILayout.ExpandHeight(true));
				//GUILayout.FlexibleSpace();
				Rect textureRect;
				//textureRect = GUILayoutUtility.GetRect(256f, 256f);
				if (texture != EditorGUIUtility.whiteTexture)
				{
					if (_showAlpha)
					{
						float rectRatio = textureRatio * 2f;
						rectRatio = Mathf.Max(1f, rectRatio);
						textureRect = GUILayoutUtility.GetAspectRect(rectRatio, GUILayout.ExpandWidth(true));
					}
					else
					{
						//textureRatio *= 2f;
						float rectRatio = Mathf.Max(1f, textureRatio);
						textureRect = GUILayoutUtility.GetAspectRect(rectRatio, GUILayout.ExpandWidth(true), GUILayout.Height(256f));
						/*GUIStyle style = new GUIStyle(GUI.skin.box);
						style.stretchHeight = true;
						style.stretchWidth = true;
						style.fixedWidth = 0;
						style.fixedHeight = 0;
						textureRect = GUILayoutUtility.GetRect(Screen.width, Screen.width, 128f, Screen.height / 1.2f, style);*/
					}
				}
				else
				{
					float rectRatio = Mathf.Max(1f, textureRatio);
					textureRect = GUILayoutUtility.GetAspectRect(rectRatio, GUILayout.ExpandWidth(true), GUILayout.Height(256f));
				}
				if (textureRect.height > (Screen.height / 2f))
				{
					//textureRect.height = Screen.height / 2f;
				}
				//Debug.Log(textureRect.height + " " + Screen.height);
				//GUILayout.FlexibleSpace();
				//GUILayout.EndHorizontal();

				// Pause / Play toggle on mouse click
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.isMouse)
				{
					if (textureRect.Contains(Event.current.mousePosition))
					{
						if (mediaPlayer.Control != null)
						{
							if (mediaPlayer.Control.IsPaused())
							{
								mediaPlayer.Play();
							}
							else
							{
								mediaPlayer.Pause();
							}
						}
					}
				}

				if (Event.current.type == EventType.Repaint)
				{
					GUI.color = Color.gray;
					EditorGUI.DrawTextureTransparent(textureRect, Texture2D.blackTexture, ScaleMode.StretchToFill);
					GUI.color = Color.white;
					//EditorGUI.DrawTextureAlpha(textureRect, Texture2D.whiteTexture, ScaleMode.ScaleToFit);
					//GUI.color = Color.black;
					//GUI.DrawTexture(textureRect, texture, ScaleMode.StretchToFill, false);
					//GUI.color = Color.white;

					// Draw the texture
					Matrix4x4 prevMatrix = GUI.matrix;
					if (textureSource != null && textureSource.RequiresVerticalFlip())
					{
						//	GUIUtility.ScaleAroundPivot(new Vector2(1f, -1f), new Vector2(0f, textureRect.y + (textureRect.height / 2f)));
					}

					if (!GUI.enabled)
					{
						//GUI.color = Color.black;
						//GUI.DrawTexture(textureRect, texture, ScaleMode.ScaleToFit, false);
						//GUI.color = Color.white;
					}
					else
					{
						if (_showPreview && texture != EditorGUIUtility.whiteTexture)
						{
							RenderPreview(mediaPlayer);
						}

						if (!_showAlpha)
						{
							if (texture != EditorGUIUtility.whiteTexture)
							{
								// TODO: In Linear mode, this displays the texture too bright, but GUI.DrawTexture displays it correctly
								//GL.sRGBWrite = true;
								//GUI.DrawTexture(textureRect, rt, ScaleMode.ScaleToFit, false);

								if (_previewTexture)
								{
									EditorGUI.DrawPreviewTexture(textureRect, _previewTexture, _materialIMGUI, ScaleMode.ScaleToFit);
								}
								//EditorGUI.DrawTextureTransparent(textureRect, rt, ScaleMode.ScaleToFit);

								//VideoRender.DrawTexture(textureRect, rt, ScaleMode.ScaleToFit, AlphaPacking.None, _materialPreview);
								//GL.sRGBWrite = false;
							}
							else
							{
								// Fill with black
								//GUI.color = Color.black;
								//GUI.DrawTexture(textureRect, texture, ScaleMode.StretchToFill, false);
								//GUI.color = Color.white;
							}
						}
						else
						{
							textureRect.width /= 2f;
							//GUI.DrawTexture(textureRect, rt, ScaleMode.ScaleToFit, false);
							//GL.sRGBWrite = true;
							//VideoRender.DrawTexture(textureRect, rt, ScaleMode.ScaleToFit, AlphaPacking.None, _materialIMGUI);
							//GL.sRGBWrite = false;
							textureRect.x += textureRect.width;
							//EditorGUI.DrawTextureAlpha(textureRect, texture, ScaleMode.ScaleToFit);
						}
					}
					GUI.matrix = prevMatrix;
				}
			}

			IMediaInfo info = mediaPlayer.Info;
			IMediaControl control = mediaPlayer.Control;
			bool showBrowseMenu = false;

			if (true)
			{
				bool isPlaying = false;
				if (control != null)
				{
					isPlaying = control.IsPlaying();
				}

				// Slider layout
				EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight/2f));
				Rect sliderRect = GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.horizontalSlider, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				EditorGUILayout.EndHorizontal();

				float currentTime = 0f;
				float durationTime = 0.001f;
				if (control != null)
				{
					currentTime = (float)control.GetCurrentTime();
					durationTime = (float)info.GetDuration();
					if (float.IsNaN(durationTime))
					{
						durationTime = 0f;
					}
				}
				
				TimeRange timelineRange = new TimeRange(0.0, 0.001);	// A tiny default duration to prevent divide by zero's
				if (info != null)
				{
					timelineRange = Helper.GetTimelineRange(info.GetDuration(), control.GetSeekableTimes());
				}

				// Slider
				{
					// Draw buffering
					if (control != null && timelineRange.Duration > 0.0 && Event.current.type == EventType.Repaint)
					{
						GUI.color = new Color(0f, 1f, 0f, 0.25f);
						TimeRanges times = control.GetBufferedTimes();
						if (timelineRange.Duration > 0.0)
						{
							for (int i = 0; i < times.Count; i++)
							{
								Rect bufferedRect = sliderRect;

								float startT = Mathf.Clamp01((float)((times[i].StartTime - timelineRange.StartTime) / timelineRange.Duration));
								float endT = Mathf.Clamp01((float)((times[i].EndTime - timelineRange.StartTime) / timelineRange.Duration));

								bufferedRect.xMin = sliderRect.xMin + sliderRect.width * startT;
								bufferedRect.xMax = sliderRect.xMin + sliderRect.width * endT;
								bufferedRect.yMin += sliderRect.height * 0.5f;
								
								GUI.DrawTexture(bufferedRect, Texture2D.whiteTexture);
							}
						}
						GUI.color = Color.white;
					}

					// Timeline slider
					{
						float newTime = GUI.HorizontalSlider(sliderRect, currentTime, (float)timelineRange.StartTime, (float)timelineRange.EndTime);
						if (newTime != currentTime)
						{
							if (control != null)
							{
								// NOTE: For unknown reasons the seeks here behave differently to the MediaPlayerUI demo
								// When scrubbing (especially with NotchLC) while the video is playing, the frames will not update and a Stalled state will be shown,
								// but using the MediaPlayerUI the same scrubbing will updates the frames.  Perhaps it's just an execution order issue
								control.Seek(newTime);
							}
						}
					}
				}

				EditorGUILayout.BeginHorizontal();
				string timeTotal = "∞";
				if (!float.IsInfinity(durationTime))
				{
					timeTotal = Helper.GetTimeString(durationTime, false);
				}
				string timeUsed = Helper.GetTimeString(currentTime - (float)timelineRange.StartTime, false);
				GUILayout.Label(timeUsed, GUILayout.ExpandWidth(false));
				//GUILayout.Label("/", GUILayout.ExpandWidth(false));
				GUILayout.FlexibleSpace();
				GUILayout.Label(timeTotal, GUILayout.ExpandWidth(false));

				EditorGUILayout.EndHorizontal();

				// In non-pro we need to make these 3 icon content black as the buttons are light
				// and the icons are white by default
				if (!EditorGUIUtility.isProSkin)
				{
					GUI.contentColor = Color.black;
				}

				EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

				// Play/Pause
				{
					float maxHeight = GUI.skin.button.CalcHeight(_iconSceneViewAudio, 0f);
					if (!isPlaying)
					{
						GUI.color = Color.green;
						if (GUILayout.Button(_iconPlayButton, GUILayout.ExpandWidth(false), GUILayout.Height(maxHeight)))
						{
							if (control != null)
							{
								control.Play();
							}
							else
							{
								if (mediaPlayer.MediaSource == MediaSource.Path)
								{
									mediaPlayer.OpenMedia(mediaPlayer.MediaPath.PathType, mediaPlayer.MediaPath.Path, true);
								}
								else if (mediaPlayer.MediaSource == MediaSource.Reference)
								{
									mediaPlayer.OpenMedia(mediaPlayer.MediaReference, true);
								}
							}
						}
					}
					else
					{
						GUI.color = Color.yellow;
						if (GUILayout.Button(_iconPauseButton, GUILayout.ExpandWidth(false), GUILayout.Height(maxHeight)))
						{
							if (control != null)
							{
								control.Pause();
							}
						}
					}
					GUI.color = Color.white;
				}

				// Looping
				{
					if (!_propLoop.boolValue)
					{
						GUI.color = Color.grey;
					}
					float maxHeight = GUI.skin.button.CalcHeight(_iconSceneViewAudio, 0f);
					//GUIContent icon = new GUIContent("∞");
					if (GUILayout.Button(_iconRotateTool, GUILayout.Height(maxHeight)))
					{
						if (control != null)
						{
							control.SetLooping(!_propLoop.boolValue);
						}
						_propLoop.boolValue = !_propLoop.boolValue;
					}
					GUI.color = Color.white;
				}				

				// Mute & Volume
				EditorGUI.BeginDisabledGroup(UnityEditor.EditorUtility.audioMasterMute);
				{
					if (_propMuted.boolValue)
					{
						GUI.color = Color.gray;
					}
					float maxWidth = _iconPlayButton.image.width;
					//if (GUILayout.Button("Muted", GUILayout.ExpandWidth(false), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
					//string iconName = "d_AudioListener Icon";		// Unity 2019+
					if (GUILayout.Button(_iconSceneViewAudio))//, GUILayout.Width(maxWidth),  GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandHeight(false)))
					{
						if (control != null)
						{
							control.MuteAudio(!_propMuted.boolValue);
						}
						_propMuted.boolValue = !_propMuted.boolValue;
					}
					GUI.color = Color.white;
				}
				if (!_propMuted.boolValue)
				{
					EditorGUI.BeginChangeCheck();
					float newVolume = GUILayout.HorizontalSlider(_propVolume.floatValue, 0f, 1f,  GUILayout.ExpandWidth(true), GUILayout.MinWidth(64f));
					if (EditorGUI.EndChangeCheck())
					{
						if (control != null)
						{
							control.SetVolume(newVolume);
						}
						_propVolume.floatValue = newVolume;
					}
				}
				EditorGUI.EndDisabledGroup();

				GUI.contentColor = Color.white;

				GUILayout.FlexibleSpace();

				if (Event.current.commandName == "ObjectSelectorClosed" && 
					EditorGUIUtility.GetObjectPickerControlID() == 200) 
				{
					_queuedLoadMediaRef = (MediaReference)EditorGUIUtility.GetObjectPickerObject();
				}

				if (GUILayout.Button(_iconProject, GUILayout.ExpandWidth(false)))
				{
					showBrowseMenu = true;
				}
				
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();

			if (showBrowseMenu)
			{
				RecentMenu.Create(_propMediaPath, _propMediaSource, MediaFileExtensions, true, 200);
			}

			if (_queuedLoadMediaRef && Event.current.type == EventType.Repaint)
			{
				//MediaPlayer mediaPlayer = (MediaPlayer)_propMediaPath.serializedObject.targetObject;
				if (mediaPlayer)
				{
					mediaPlayer.OpenMedia(_queuedLoadMediaRef, true);
					_queuedLoadMediaRef = null;
				}
			}
			if (_queuedToggleShowPreview)
			{
				_showPreview = !_showPreview;
				_queuedToggleShowPreview = false;
				this.Repaint();
			}
		}

		private void OnInspectorGUI_VideoPreview(MediaPlayer media, ITextureProducer textureSource)
		{
			EditorGUILayout.LabelField("* Inspector preview affects playback performance");

			Texture texture = null;
			if (textureSource != null)
			{
				texture = textureSource.GetTexture();
			}
			if (texture == null)
			{
				texture = EditorGUIUtility.whiteTexture;
			}

			float ratio = (float)texture.width / (float)texture.height;

			// Reserve rectangle for texture
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			Rect textureRect;
			if (texture != EditorGUIUtility.whiteTexture)
			{
				if (_showAlpha)
				{
					ratio *= 2f;
					textureRect = GUILayoutUtility.GetRect(Screen.width / 2, Screen.width / 2, (Screen.width / 2) / ratio, (Screen.width / 2) / ratio);
				}
				else
				{
					textureRect = GUILayoutUtility.GetRect(Screen.width / 2, Screen.width / 2, (Screen.width / 2) / ratio, (Screen.width / 2) / ratio);
				}
			}
			else
			{
				textureRect = GUILayoutUtility.GetRect(1920f / 40f, 1080f / 40f, GUILayout.ExpandWidth(true));
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			// Dimensions
			string dimensionText = string.Format("{0}x{1}@{2:0.##}", 0, 0, 0.0f);
			if (texture != EditorGUIUtility.whiteTexture && media.Info != null)
			{
				dimensionText = string.Format("{0}x{1}@{2:0.##}", texture.width, texture.height, media.Info.GetVideoFrameRate());
			}

			EditorHelper.IMGUI.CentreLabel(dimensionText);

			string rateText = "0";
			string playerText = string.Empty;
			if (media.Info != null)
			{
				rateText = media.Info.GetVideoDisplayRate().ToString("F2");
				playerText = media.Info.GetPlayerDescription();
			}

			EditorGUILayout.LabelField("Display Rate", rateText);
			EditorGUILayout.LabelField("Using", playerText);
			_showAlpha = EditorGUILayout.Toggle("Show Alpha", _showAlpha);

			// Draw the texture
			Matrix4x4 prevMatrix = GUI.matrix;
			if (textureSource != null && textureSource.RequiresVerticalFlip())
			{
				GUIUtility.ScaleAroundPivot(new Vector2(1f, -1f), new Vector2(0, textureRect.y + (textureRect.height / 2)));
			}

			if (!GUI.enabled)
			{
				GUI.color = Color.grey;
				GUI.DrawTexture(textureRect, texture, ScaleMode.ScaleToFit, false);
				GUI.color = Color.white;
			}
			else
			{
				if (!_showAlpha)
				{
					// TODO: In Linear mode, this displays the texture too bright, but GUI.DrawTexture displays it correctly
					EditorGUI.DrawTextureTransparent(textureRect, texture, ScaleMode.ScaleToFit);
				}
				else
				{
					textureRect.width /= 2f;
					GUI.DrawTexture(textureRect, texture, ScaleMode.ScaleToFit, false);
					textureRect.x += textureRect.width;
					EditorGUI.DrawTextureAlpha(textureRect, texture, ScaleMode.ScaleToFit);
				}
			}
			GUI.matrix = prevMatrix;

			// Select texture button
			/*if (texture != null && texture != EditorGUIUtility.whiteTexture)
			{
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.FlexibleSpace();
				for (int i = 0; i < textureSource.GetTextureCount(); i++)
				{
					Texture textures = textureSource.GetTexture(i);
					if (GUILayout.Button("Select Texture", GUILayout.ExpandWidth(false)))
					{
						Selection.activeObject = textures;
					}
				}
				if (GUILayout.Button("Save PNG", GUILayout.ExpandWidth(true)))
				{
					media.SaveFrameToPng();
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}*/
		}

		private void OnInspectorGUI_PlayControls(IMediaControl control, IMediaInfo info)
		{
			GUILayout.Space(8.0f);

			// Slider
			EditorGUILayout.BeginHorizontal();
			bool isPlaying = false;
			if (control != null)
			{
				isPlaying = control.IsPlaying();
			}
			float currentTime = 0f;
			if (control != null)
			{
				currentTime = (float)control.GetCurrentTime();
			}

			float durationTime = 0f;
			if (info != null)
			{
				durationTime = (float)info.GetDuration();
				if (float.IsNaN(durationTime))
				{
					durationTime = 0f;
				}
			}
			string timeUsed = Helper.GetTimeString(currentTime, true);
			GUILayout.Label(timeUsed, GUILayout.ExpandWidth(false));

			float newTime = GUILayout.HorizontalSlider(currentTime, 0f, durationTime, GUILayout.ExpandWidth(true));
			if (newTime != currentTime)
			{
				control.Seek(newTime);
			}

			string timeTotal = "Infinity";
			if (!float.IsInfinity(durationTime))
			{
				timeTotal = Helper.GetTimeString(durationTime, true);
			}

			GUILayout.Label(timeTotal, GUILayout.ExpandWidth(false));

			EditorGUILayout.EndHorizontal();

			// Buttons
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Rewind", GUILayout.ExpandWidth(false)))
			{
				control.Rewind();
			}

			if (!isPlaying)
			{
				GUI.color = Color.green;
				if (GUILayout.Button("Play", GUILayout.ExpandWidth(true)))
				{
					control.Play();
				}
			}
			else
			{
				GUI.color = Color.yellow;
				if (GUILayout.Button("Pause", GUILayout.ExpandWidth(true)))
				{
					control.Pause();
				}
			}
			GUI.color = Color.white;
			EditorGUILayout.EndHorizontal();
		}

		void OnInspectorGUI_Preview()
		{
			MediaPlayer media = (this.target) as MediaPlayer;

			EditorGUI.BeginDisabledGroup(!(media.TextureProducer != null && media.Info.HasVideo()));
			OnInspectorGUI_VideoPreview(media, media.TextureProducer);
			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginDisabledGroup(!(media.Control != null && media.Control.CanPlay() && media.isActiveAndEnabled && !EditorApplication.isPaused));
			OnInspectorGUI_PlayControls(media.Control, media.Info);
			EditorGUI.EndDisabledGroup();
		}
	}
}