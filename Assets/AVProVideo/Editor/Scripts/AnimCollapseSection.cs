#define AVPROVIDEO_SUPPORT_LIVEEDITMODE
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// A collapsable GUI section that animates during open and close
	internal class AnimCollapseSection
	{
		internal const string SettingsPrefix = "AVProVideo-MediaPlayerEditor-";
		private const float CollapseSpeed = 2f;
		private static GUIStyle _styleCollapsableSection = null;
		private static GUIStyle _styleButtonFoldout = null;
		private static GUIStyle _styleHelpBoxNoPad = null;

		public AnimCollapseSection(string label, bool showOnlyInEditMode, bool isDefaultExpanded, System.Action action, UnityEditor.Editor editor, Color backgroundColor, List<AnimCollapseSection> groupItems = null)
			: this(new GUIContent(label), showOnlyInEditMode, isDefaultExpanded, action, editor, backgroundColor, groupItems)
		{
		}
		public AnimCollapseSection(GUIContent label, bool showOnlyInEditMode, bool isDefaultExpanded, System.Action action, UnityEditor.Editor editor, Color backgroundColor, List<AnimCollapseSection> groupItems = null)
		{
			Label = label;
			_name = Label.text;
			Label.text = " " + Label.text;		// Add a space for aesthetics
			ShowOnlyInEditMode = showOnlyInEditMode;
			_action = action;
			isDefaultExpanded = EditorPrefs.GetBool(PrefName, isDefaultExpanded);
			BackgroundColor = backgroundColor;
			_groupItems = groupItems;
			_anim = new UnityEditor.AnimatedValues.AnimBool(isDefaultExpanded);
			_anim.speed = CollapseSpeed;
			_anim.valueChanged.AddListener(editor.Repaint);
		}
		~AnimCollapseSection()
		{
			_anim.valueChanged.RemoveAllListeners();
		}

		private string _name;
		private UnityEditor.AnimatedValues.AnimBool _anim;
		private System.Action _action;
		private List<AnimCollapseSection> _groupItems;

		public void Invoke()
		{
			_action.Invoke();
		}

		public bool IsExpanded { get { return _anim.target; } set { if (_anim.target != value) { _anim.target = value; if (value) CollapseSiblings(); } } }
		public float Faded { get { return _anim.faded; } }
		public GUIContent Label { get; private set; }
		public bool ShowOnlyInEditMode { get; private set; }
		public Color BackgroundColor { get; private set; }
		private string PrefName { get { return GetPrefName(_name); } }

		public void Save()
		{
			EditorPrefs.SetBool(PrefName, IsExpanded);
		}

		private void CollapseSiblings()
		{
			// Ensure only a single item is in an expanded state
			if (_groupItems != null)
			{
				foreach (AnimCollapseSection section in _groupItems)
				{
					if (section != this && section.IsExpanded)
					{
						section.IsExpanded = false;
					}
				}
			}
		}

		internal static string GetPrefName(string label)
		{
			return SettingsPrefix + "Expand-" + label;
		}

		internal static void CreateStyles()
		{
			if (_styleCollapsableSection == null)
			{
				_styleCollapsableSection = new GUIStyle(GUI.skin.box);
				_styleCollapsableSection.padding.top = 0;
				_styleCollapsableSection.padding.bottom = 0;
			}
			if (_styleButtonFoldout == null)
			{
				_styleButtonFoldout = new GUIStyle(EditorStyles.foldout);
				_styleButtonFoldout.margin = new RectOffset();
				_styleButtonFoldout.fontStyle = FontStyle.Bold;
				_styleButtonFoldout.alignment = TextAnchor.MiddleLeft;
			}
			if (_styleHelpBoxNoPad == null)
			{
				_styleHelpBoxNoPad = new GUIStyle(EditorStyles.helpBox);
				_styleHelpBoxNoPad.padding = new RectOffset();
				//_styleHelpBoxNoPad.border = new RectOffset();
				_styleHelpBoxNoPad.overflow = new RectOffset();
				_styleHelpBoxNoPad.margin = new RectOffset();
				_styleHelpBoxNoPad.margin = new RectOffset(8, 0, 0, 0);
				_styleHelpBoxNoPad.stretchWidth = false;
				_styleHelpBoxNoPad.stretchHeight = false;
				//_styleHelpBoxNoPad.normal.background = Texture2D.whiteTexture;
			}
		}

		internal static void Show(AnimCollapseSection section, int indentLevel = 0)
		{
			if (section.ShowOnlyInEditMode && Application.isPlaying) return;

			float headerGlow = Mathf.Lerp(0.5f, 0.85f, section.Faded);
			//float headerGlow = Mathf.Lerp(0.85f, 1f, section.Faded);
			if (EditorGUIUtility.isProSkin)
			{
				GUI.backgroundColor = section.BackgroundColor * new Color(headerGlow, headerGlow, headerGlow, 1f);
			}
			else
			{
				headerGlow = Mathf.Lerp(0.75f, 1f, section.Faded);
				GUI.backgroundColor = section.BackgroundColor * new Color(headerGlow, headerGlow, headerGlow, 1f);
			}
			GUILayout.BeginVertical(_styleHelpBoxNoPad);
			GUILayout.Box(GUIContent.none, EditorStyles.miniButton, GUILayout.ExpandWidth(true));
			GUI.backgroundColor = Color.white;
			Rect buttonRect = GUILayoutUtility.GetLastRect();
			if (Event.current.type != EventType.Layout)
			{
				buttonRect.xMin += indentLevel * EditorGUIUtility.fieldWidth / 3f;
				EditorGUI.indentLevel++;
				EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
				section.IsExpanded = EditorGUI.Foldout(buttonRect, section.IsExpanded, section.Label, true, _styleButtonFoldout);
				EditorGUIUtility.SetIconSize(Vector2.zero);
				EditorGUI.indentLevel--;
			}

			if (EditorGUILayout.BeginFadeGroup(section.Faded))
			{
				section.Invoke();
			}
			EditorGUILayout.EndFadeGroup();
			GUILayout.EndVertical();
		}

		internal static void Show(string label, ref bool isExpanded, System.Action action, bool showOnlyInEditMode)
		{
			if (showOnlyInEditMode && Application.isPlaying) return;

			if (BeginShow(label, ref isExpanded, Color.white))
			{
				action.Invoke();
			}
			EndShow();
		}

		internal static bool BeginShow(string label, ref bool isExpanded, Color tintColor)
		{
			GUI.color = Color.white;
			GUI.backgroundColor = Color.clear;
			if (isExpanded)
			{
				GUI.color = Color.white;
				GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.1f);
				if (EditorGUIUtility.isProSkin)
				{
					GUI.backgroundColor = Color.black;
				}
			}

			GUILayout.BeginVertical(_styleCollapsableSection);
			GUI.color = tintColor;
			GUI.backgroundColor = Color.white;
			if (GUILayout.Button(label, EditorStyles.toolbarButton))
			{
				isExpanded = !isExpanded;
			}
			GUI.color = Color.white;

			return isExpanded;
		}

		internal static void EndShow()
		{
			GUILayout.EndVertical();
		}
	}
}