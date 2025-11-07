// UnityEngine.UI was moved to a package in 2019.2.0
// Unfortunately no way to test for this across all Unity versions yet
// You can set up the asmdef to reference the new package, but the package doesn't 
// existing in Unity 2017 etc, and it throws an error due to missing reference
#define AVPRO_PACKAGE_UNITYUI
#if (UNITY_2019_2_OR_NEWER && AVPRO_PACKAGE_UNITYUI) || (!UNITY_2019_2_OR_NEWER)

using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// <summary>
	/// Editor for the DisplayUGUI component
	/// </summary>
	[CustomEditor(typeof(DisplayUGUI), true)]
	[CanEditMultipleObjects]
	public class DisplayUGUIEditor : GraphicEditor
	{
		// Note we have precedence for calling rectangle for just rect, even in the Inspector.
		// For example in the Camera component's Viewport Rect.
		// Hence sticking with Rect here to be consistent with corresponding property in the API.
		private static readonly GUIContent m_guiTextUVRectContent = new GUIContent("UV Rect");

		private SerializedProperty _propMediaPlayer;
		private SerializedProperty _propUVRect;
		private SerializedProperty _propDefaultTexture;
		private SerializedProperty _propNoDefaultDisplay;
		private SerializedProperty _propDisplayInEditor;
		private SerializedProperty _propSetNativeSize;
		private SerializedProperty _propScaleMode;

		[MenuItem("GameObject/UI/AVPro Video uGUI", false, 0)]
		public static void CreateGameObject()
		{
			GameObject parent = Selection.activeGameObject;
			RectTransform parentCanvasRenderer = ( parent != null ) ? parent.GetComponent<RectTransform>() : null;
			if( parentCanvasRenderer )
			{
				GameObject go = new GameObject("AVPro Video");
				go.transform.SetParent(parent.transform, false);
				go.AddComponent<RectTransform>();
				go.AddComponent<CanvasRenderer>();
				go.AddComponent<DisplayUGUI>();
				Selection.activeGameObject = go;
			}
			else
			{
				EditorUtility.DisplayDialog("AVPro Video", "You must make the AVPro Video uGUI object as a child of a Canvas.", "Ok");
			}
		}

		public override bool RequiresConstantRepaint()
		{
			DisplayUGUI displayComponent = target as DisplayUGUI;
			return (displayComponent != null && displayComponent.HasValidTexture());
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			_propMediaPlayer = this.CheckFindProperty("_mediaPlayer");
			_propUVRect = this.CheckFindProperty("_uvRect");
			_propSetNativeSize = this.CheckFindProperty("_setNativeSize");
			_propScaleMode = this.CheckFindProperty("_scaleMode");
			_propNoDefaultDisplay = this.CheckFindProperty("_noDefaultDisplay");
			_propDisplayInEditor = this.CheckFindProperty("_displayInEditor");
			_propDefaultTexture = this.CheckFindProperty("_defaultTexture");

			SetShowNativeSize(true);
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_propMediaPlayer);
			EditorGUILayout.PropertyField(_propDisplayInEditor);
			EditorGUILayout.PropertyField(_propNoDefaultDisplay);
			if (!_propNoDefaultDisplay.boolValue)
			{
				EditorGUILayout.PropertyField(_propDefaultTexture);
			}
			AppearanceControlsGUI();
			RaycastControlsGUI();
			EditorGUILayout.PropertyField(_propUVRect, m_guiTextUVRectContent);

			EditorGUILayout.PropertyField(_propSetNativeSize);
			EditorGUILayout.PropertyField(_propScaleMode);

			SetShowNativeSize(false);
			NativeSizeButtonGUI();

			serializedObject.ApplyModifiedProperties();
		}

		private void SetShowNativeSize(bool instant)
		{
			base.SetShowNativeSize(_propMediaPlayer.objectReferenceValue != null, instant);
		}

		/// <summary>
		/// Allow the texture to be previewed.
		/// </summary>
		public override bool HasPreviewGUI()
		{
			DisplayUGUI rawImage = target as DisplayUGUI;
			return rawImage != null;
		}

		/// <summary>
		/// Draw the Image preview.
		/// </summary>
		public override void OnPreviewGUI(Rect drawArea, GUIStyle background)
		{
			DisplayUGUI rawImage = target as DisplayUGUI;
			Texture tex = rawImage.mainTexture;

			if (tex == null)
				return;

			// Create the texture rectangle that is centered inside rect.
			Rect outerRect = drawArea;

			Matrix4x4 m = GUI.matrix;
			// Flip the image vertically
			if (rawImage.HasValidTexture())
			{
				if (rawImage.Player.TextureProducer.RequiresVerticalFlip())
				{
					GUIUtility.ScaleAroundPivot(new Vector2(1f, -1f), new Vector2(0f, outerRect.y + (outerRect.height / 2f)));
				}
			}

			EditorGUI.DrawTextureTransparent(outerRect, tex, ScaleMode.ScaleToFit);//, outer.width / outer.height);
			//SpriteDrawUtility.DrawSprite(tex, rect, outer, rawImage.uvRect, rawImage.canvasRenderer.GetColor());

			GUI.matrix = m;
		}

		/// <summary>
		/// Info String drawn at the bottom of the Preview
		/// </summary>
		public override string GetInfoString()
		{
			DisplayUGUI rawImage = target as DisplayUGUI;

			string text = string.Empty;
			if (rawImage.HasValidTexture())
			{
				text += string.Format("Video Size: {0}x{1}\n",
										Mathf.RoundToInt(Mathf.Abs(rawImage.mainTexture.width)),
										Mathf.RoundToInt(Mathf.Abs(rawImage.mainTexture.height)));
			}

			// Image size Text
			text += string.Format("Display Size: {0}x{1}",
					Mathf.RoundToInt(Mathf.Abs(rawImage.rectTransform.rect.width)),
					Mathf.RoundToInt(Mathf.Abs(rawImage.rectTransform.rect.height)));

			return text;
		}
	}
}

#endif