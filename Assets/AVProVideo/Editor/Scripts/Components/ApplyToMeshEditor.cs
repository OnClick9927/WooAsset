using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// <summary>
	/// Editor for the ApplyToMesh component
	/// </summary>
	[CanEditMultipleObjects]
	[CustomEditor(typeof(ApplyToMesh))]
	public class ApplyToMeshEditor : UnityEditor.Editor
	{
		private static readonly GUIContent _guiTextTextureProperty = new GUIContent("Texture Property");

		private SerializedProperty _propTextureOffset;
		private SerializedProperty _propTextureScale;
		private SerializedProperty _propMediaPlayer;
		private SerializedProperty _propRenderer;
		private SerializedProperty _propMaterialIndex;
		private SerializedProperty _propTexturePropertyName;
		private SerializedProperty _propDefaultTexture;
		private SerializedProperty _propAutomaticStereoPacking;
		private SerializedProperty _propOverrideStereoPacking;
		private SerializedProperty _propStereoRedGreenTint;
		private GUIContent[] _materialTextureProperties = new GUIContent[0];

		void OnEnable()
		{
			_propTextureOffset = this.CheckFindProperty("_offset");
			_propTextureScale = this.CheckFindProperty("_scale");
			_propMediaPlayer = this.CheckFindProperty("_media");
			_propRenderer = this.CheckFindProperty("_renderer");
			_propMaterialIndex = this.CheckFindProperty("_materialIndex");
			_propTexturePropertyName = this.CheckFindProperty("_texturePropertyName");
			_propDefaultTexture = this.CheckFindProperty("_defaultTexture");
			_propAutomaticStereoPacking = this.CheckFindProperty("_automaticStereoPacking");
			_propOverrideStereoPacking = this.CheckFindProperty("_overrideStereoPacking");
			_propStereoRedGreenTint = this.CheckFindProperty("_stereoRedGreenTint");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			if (_propRenderer == null)
			{
				return;
			}

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(_propMediaPlayer);
			EditorGUILayout.PropertyField(_propDefaultTexture);
			EditorGUILayout.PropertyField(_propRenderer);

			bool hasKeywords = false;
			int materialCount = 0;
			int texturePropertyIndex = 0;
			_materialTextureProperties = new GUIContent[0];
			if (_propRenderer.objectReferenceValue != null)
			{
				Renderer r = (Renderer)(_propRenderer.objectReferenceValue);

				materialCount = r.sharedMaterials.Length;
				List<Material> nonNullMaterials = new List<Material>(r.sharedMaterials);
				// Remove any null materials (otherwise MaterialEditor.GetMaterialProperties() errors)
				for (int i = 0; i < nonNullMaterials.Count; i++)
				{
					if (nonNullMaterials[i] == null)
					{
						nonNullMaterials.RemoveAt(i);
						i--;
					}
				}
				
				if (nonNullMaterials.Count > 0)
				{
					// Detect if there are any keywords
					foreach (Material mat in nonNullMaterials)
					{
						if (mat.shaderKeywords.Length > 0)
						{
							hasKeywords = true;
							break;
						}
					}

					// Get unique list of texture property names
					List<GUIContent> items = new List<GUIContent>(16);
					List<string> textureNames = new List<string>(8);
					foreach (Material mat in nonNullMaterials)
					{
						// NOTE: we process each material separately instead of passing them all into  MaterialEditor.GetMaterialProperties() as it errors if the materials have different properties
						MaterialProperty[] matProps = MaterialEditor.GetMaterialProperties(new Object[] { mat });
						foreach (MaterialProperty matProp in matProps)
						{
							if (matProp.type == MaterialProperty.PropType.Texture)
							{
								if (!textureNames.Contains(matProp.name))
								{
									if (matProp.name == _propTexturePropertyName.stringValue)
									{
										texturePropertyIndex = items.Count;
									}
									textureNames.Add(matProp.name);
									items.Add(new GUIContent(matProp.name));
								}
							}
						}
					}
					_materialTextureProperties = items.ToArray();
				}
			}

			if (materialCount > 0)
			{
				GUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("All Materials");
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.Toggle(_propMaterialIndex.intValue < 0);
				if (EditorGUI.EndChangeCheck())
				{
					if (_propMaterialIndex.intValue < 0)
					{
						_propMaterialIndex.intValue = 0;
					}
					else
					{
						_propMaterialIndex.intValue = -1;
					}
				}
				GUILayout.EndHorizontal();

				if (_propMaterialIndex.intValue >= 0)
				{
					GUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Material Index");
					_propMaterialIndex.intValue = EditorGUILayout.IntSlider(_propMaterialIndex.intValue, 0, materialCount - 1);
					GUILayout.EndHorizontal();
				}
			}

			int newTexturePropertyIndex = EditorGUILayout.Popup(_guiTextTextureProperty, texturePropertyIndex, _materialTextureProperties);
			if (newTexturePropertyIndex >= 0 && newTexturePropertyIndex < _materialTextureProperties.Length)
			{
				_propTexturePropertyName.stringValue = _materialTextureProperties[newTexturePropertyIndex].text;
			}

			if (hasKeywords && _propTexturePropertyName.stringValue != Helper.UnityBaseTextureName)
			{
				EditorGUILayout.HelpBox("When using an uber shader you may need to enable the keywords on a material for certain texture slots to take effect.  You can sometimes achieve this (eg with Standard shader) by putting a dummy texture into the texture slot.", MessageType.Info);
			}

			EditorGUILayout.PropertyField(_propTextureOffset);
			EditorGUILayout.PropertyField(_propTextureScale);


			EditorGUILayout.PropertyField(_propAutomaticStereoPacking);
			if (!_propAutomaticStereoPacking.boolValue)
			{
				EditorGUILayout.PropertyField(_propOverrideStereoPacking);
			}
			EditorGUILayout.PropertyField(_propStereoRedGreenTint);

			serializedObject.ApplyModifiedProperties();

			bool wasModified = EditorGUI.EndChangeCheck();

			if (Application.isPlaying && wasModified)
			{
				foreach (Object obj in this.targets)
				{
					((ApplyToMesh)obj).ForceUpdate();
				}
			}
		}
	}
}