using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// <summary>
	/// Editor for the MediaPlaylist class
	/// </summary>
	[CustomPropertyDrawer(typeof(MediaPlaylist))]
	public class MediaPlaylistDrawer : PropertyDrawer
	{
		private static readonly GUIContent _guiTextInsert = new GUIContent("Insert");
		private static readonly GUIContent _guiTextDelete = new GUIContent("Delete");
		private static readonly GUIContent _guiTextUp = new GUIContent("Up");
		private static readonly GUIContent _guiTextDown = new GUIContent("Down");

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			EditorGUI.BeginProperty(position, label, property);


			SerializedProperty propItems = property.FindPropertyRelative("_items");

			if (propItems.arraySize == 0)
			{
				if (GUILayout.Button("Insert Item"))
				{
					propItems.InsertArrayElementAtIndex(0);
				}
			}

			for (int i = 0; i < propItems.arraySize; i++)
			{
				SerializedProperty propItem = propItems.GetArrayElementAtIndex(i);

				GUILayout.BeginVertical(GUI.skin.box);
				propItem.isExpanded = EditorGUILayout.ToggleLeft("Item " + i, propItem.isExpanded);
				if (propItem.isExpanded)
				{
					EditorGUILayout.PropertyField(propItem);
					GUILayout.BeginHorizontal();
					if (GUILayout.Button(_guiTextInsert))
					{
						propItems.InsertArrayElementAtIndex(i);
					}
					if (GUILayout.Button(_guiTextDelete))
					{
						propItems.DeleteArrayElementAtIndex(i);
					}
					EditorGUI.BeginDisabledGroup((i - 1) < 0);
					if (GUILayout.Button(_guiTextUp))
					{
						propItems.MoveArrayElement(i, i - 1);
					}
					EditorGUI.EndDisabledGroup();
					EditorGUI.BeginDisabledGroup((i + 1) >= propItems.arraySize);
					if (GUILayout.Button(_guiTextDown))
					{
						propItems.MoveArrayElement(i, i + 1);
					}
					EditorGUI.EndDisabledGroup();
					GUILayout.EndHorizontal();
				}

				GUILayout.EndVertical();
			}		

			EditorGUI.EndProperty();
		}
	}

	/// <summary>
	/// Editor for the MediaPlaylist.MediaItem class
	/// </summary>
	[CustomPropertyDrawer(typeof(MediaPlaylist.MediaItem))]
	public class MediaPlaylistItemDrawer : PropertyDrawer
	{
		private static readonly GUIContent _guiTextTransition = new GUIContent("Transition");
		private static readonly GUIContent _guiTextOverrideTransition = new GUIContent("Override Transition");
		private static readonly GUIContent _guiTextDuration = new GUIContent("Duration");
		private static readonly GUIContent _guiTextEasing = new GUIContent("Easing");
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty propSourceType = property.FindPropertyRelative("sourceType");

			EditorGUILayout.PropertyField(propSourceType);
			if (propSourceType.enumValueIndex == 0)
			{
				EditorGUILayout.PropertyField(property.FindPropertyRelative("mediaPath"));
				MediaPathDrawer.ShowBrowseButton(property.FindPropertyRelative("mediaPath"));
			}
			else
			{
				//EditorGUILayout.PropertyField(property.FindPropertyRelative("texture"));
				//EditorGUILayout.PropertyField(property.FindPropertyRelative("textureDuration"));
			}

			EditorGUILayout.Space();

			//EditorGUILayout.PropertyField(property.FindPropertyRelative("stereoPacking"));
			//EditorGUILayout.PropertyField(property.FindPropertyRelative("alphaPacking"));

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(property.FindPropertyRelative("loop"));
			EditorGUILayout.PropertyField(property.FindPropertyRelative("startMode"));
			SerializedProperty propProgressMode = property.FindPropertyRelative("progressMode");
			EditorGUILayout.PropertyField(propProgressMode);
			if (propProgressMode.enumValueIndex == (int)PlaylistMediaPlayer.ProgressMode.BeforeFinish)
			{
				EditorGUILayout.PropertyField(property.FindPropertyRelative("progressTimeSeconds"));
			}

			EditorGUILayout.Space();

			SerializedProperty propIsOverrideTransition = property.FindPropertyRelative("isOverrideTransition");
			EditorGUILayout.PropertyField(propIsOverrideTransition, _guiTextOverrideTransition);
			if (propIsOverrideTransition.boolValue)
			{
				EditorGUI.indentLevel++;
				SerializedProperty propTransitionMode = property.FindPropertyRelative("overrideTransition");
				EditorGUILayout.PropertyField(propTransitionMode, _guiTextTransition);
				if (propTransitionMode.enumValueIndex != (int)PlaylistMediaPlayer.Transition.None)
				{
					EditorGUILayout.PropertyField(property.FindPropertyRelative("overrideTransitionDuration"), _guiTextDuration);
					EditorGUILayout.PropertyField(property.FindPropertyRelative("overrideTransitionEasing"), _guiTextEasing);
				}
				EditorGUI.indentLevel--;
			}
		}
	}

	/// <summary>
	/// Editor for the PlaylistMediaPlayer component
	/// </summary>
	[CanEditMultipleObjects]
	[CustomEditor(typeof(PlaylistMediaPlayer))]
	public class PlaylistMediaPlayerEditor : UnityEditor.Editor
	{
		private SerializedProperty _propPlayerA;
		private SerializedProperty _propPlayerB;
		private SerializedProperty _propPlaylist;
		private SerializedProperty _propPlaylistAutoProgress;
		private SerializedProperty _propAutoCloseVideo;
		private SerializedProperty _propPlaylistLoopMode;
		private SerializedProperty _propPausePreviousOnTransition;
		private SerializedProperty _propDefaultTransition;
		private SerializedProperty _propDefaultTransitionDuration;
		private SerializedProperty _propDefaultTransitionEasing;
		private SerializedProperty _propAudioVolume;
		private SerializedProperty _propAudioMuted;
		
		private static bool _expandPlaylistItems = false;

		private static GUIStyle _sectionBoxStyle = null;

		private const string SettingsPrefix = "AVProVideo-PlaylistMediaPlayerEditor-";

		private void OnEnable()
		{
			_propPlayerA = this.CheckFindProperty("_playerA");
			_propPlayerB = this.CheckFindProperty("_playerB");
			_propDefaultTransition = this.CheckFindProperty("_defaultTransition");
			_propDefaultTransitionDuration = this.CheckFindProperty("_defaultTransitionDuration");
			_propDefaultTransitionEasing = this.CheckFindProperty("_defaultTransitionEasing");
			_propPausePreviousOnTransition = this.CheckFindProperty("_pausePreviousOnTransition");
			_propPlaylist = this.CheckFindProperty("_playlist");
			_propPlaylistAutoProgress = this.CheckFindProperty("_playlistAutoProgress");
			_propAutoCloseVideo = this.CheckFindProperty("_autoCloseVideo");
			_propPlaylistLoopMode = this.CheckFindProperty("_playlistLoopMode");
			_propAudioVolume = this.CheckFindProperty("_playlistAudioVolume");
			_propAudioMuted = this.CheckFindProperty("_playlistAudioMuted");

			_expandPlaylistItems = EditorPrefs.GetBool(SettingsPrefix + "ExpandPlaylistItems", false);
		}

		private void OnDisable()
		{
			EditorPrefs.SetBool(SettingsPrefix + "ExpandPlaylistItems", _expandPlaylistItems);
		}

		public override bool RequiresConstantRepaint()
		{
			PlaylistMediaPlayer media = (this.target) as PlaylistMediaPlayer;
			return (media.Control != null && media.isActiveAndEnabled);
		}

		public override void OnInspectorGUI()
		{
			PlaylistMediaPlayer media = (this.target) as PlaylistMediaPlayer;

			serializedObject.Update();

			if (media == null || _propPlayerA == null)
			{
				return;
			}

			if (_sectionBoxStyle == null)
			{
				_sectionBoxStyle = new GUIStyle(GUI.skin.box);
				_sectionBoxStyle.padding.top = 0;
				_sectionBoxStyle.padding.bottom = 0;
			}

			EditorGUILayout.PropertyField(_propPlayerA);
			EditorGUILayout.PropertyField(_propPlayerB);
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			GUILayout.Label("Audio", EditorStyles.boldLabel);
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_propAudioVolume, new GUIContent("Volume"));
			if (EditorGUI.EndChangeCheck())
			{
				foreach (PlaylistMediaPlayer player in this.targets)
				{
					player.AudioVolume = _propAudioVolume.floatValue;
				}
			}
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_propAudioMuted, new GUIContent("Muted"));
			if (EditorGUI.EndChangeCheck())
			{
				foreach (PlaylistMediaPlayer player in this.targets)
				{
					player.AudioMuted = _propAudioMuted.boolValue;
				}
			}
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			GUILayout.Label("Playlist", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(_propPlaylistAutoProgress, new GUIContent("Auto Progress"));
			EditorGUILayout.PropertyField(_propPlaylistLoopMode, new GUIContent("Loop Mode"));
			EditorGUILayout.PropertyField(_propAutoCloseVideo);

			{
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				GUI.color = Color.white;
				GUI.backgroundColor = Color.clear;
				if (_expandPlaylistItems)
				{
					GUI.color = Color.white;
					GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.1f);
					if (EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = Color.black;
					}
				}
				GUILayout.BeginVertical(_sectionBoxStyle);
				GUI.backgroundColor = Color.white;
				if (GUILayout.Button("Playlist Items", EditorStyles.toolbarButton))
				{
					_expandPlaylistItems = !_expandPlaylistItems;
				}
				GUI.color = Color.white;

				if (_expandPlaylistItems)
				{	
					EditorGUILayout.PropertyField(_propPlaylist);
				}
				GUILayout.EndVertical();
			}
			EditorGUILayout.Space(); 
			EditorGUILayout.Space();
			GUILayout.Label("Default Transition", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(_propDefaultTransition, new GUIContent("Transition"));
			EditorGUILayout.PropertyField(_propDefaultTransitionEasing, new GUIContent("Easing"));
			EditorGUILayout.PropertyField(_propDefaultTransitionDuration, new GUIContent("Duration"));
			EditorGUILayout.PropertyField(_propPausePreviousOnTransition, new GUIContent("Pause Previous"));
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			if (Application.isPlaying)
			{
				ITextureProducer textureSource = media.TextureProducer;

				Texture texture = null;
				if (textureSource != null)
				{
					texture = textureSource.GetTexture();
				}
				if (texture == null)
				{
					texture = EditorGUIUtility.whiteTexture;
				}

				float ratio = 1f;// (float)texture.width / (float)texture.height;

				// Reserve rectangle for texture
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				Rect textureRect;
				Rect alphaRect = new Rect(0f, 0f, 1f, 1f);
				if (texture != EditorGUIUtility.whiteTexture)
				{
					textureRect = GUILayoutUtility.GetRect(Screen.width / 2, Screen.width / 2, (Screen.width / 2) / ratio, (Screen.width / 2) / ratio);
				}
				else
				{
					textureRect = GUILayoutUtility.GetRect(1920f / 40f, 1080f / 40f);
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				string rateText = "0";
				string playerText = string.Empty;
				if (media.Info != null)
				{
					rateText = media.Info.GetVideoDisplayRate().ToString("F2");
					playerText = media.Info.GetPlayerDescription();
				}

				EditorGUILayout.LabelField("Display Rate", rateText);
				EditorGUILayout.LabelField("Using", playerText);
								
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
					{
						GUI.DrawTexture(textureRect, texture, ScaleMode.ScaleToFit, false);
						EditorGUI.DrawTextureAlpha(alphaRect, texture, ScaleMode.ScaleToFit);
					}
				}
				GUI.matrix = prevMatrix;
			}

			EditorGUI.BeginDisabledGroup(!(media.Control != null && media.Control.CanPlay() && media.isActiveAndEnabled && !EditorApplication.isPaused));
			OnInspectorGUI_PlayControls(media.Control, media.Info);
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUI.BeginDisabledGroup(!Application.isPlaying);

			GUILayout.Label("Current Item: " + media.PlaylistIndex + " / " + Mathf.Max(0, media.Playlist.Items.Count - 1) );

			GUILayout.BeginHorizontal();
			EditorGUI.BeginDisabledGroup(!media.CanJumpToItem(media.PlaylistIndex - 1));
			if (GUILayout.Button("Prev"))
			{
				media.PrevItem();
			}
			EditorGUI.EndDisabledGroup();
			EditorGUI.BeginDisabledGroup(!media.CanJumpToItem(media.PlaylistIndex + 1));
			if (GUILayout.Button("Next"))
			{
				media.NextItem();
			}
			EditorGUI.EndDisabledGroup();
			GUILayout.EndHorizontal();
			EditorGUI.EndDisabledGroup();

			serializedObject.ApplyModifiedProperties();
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
	}
}