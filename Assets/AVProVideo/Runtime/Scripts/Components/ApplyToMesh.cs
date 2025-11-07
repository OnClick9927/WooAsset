using UnityEngine;
using UnityEngine.Serialization;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Sets up a mesh to display the video from a MediaPlayer
	/// </summary>
	[AddComponentMenu("AVPro Video/Apply To Mesh", 300)]
	[HelpURL("http://renderheads.com/products/avpro-video/")]
	public sealed class ApplyToMesh : ApplyToBase
	{
		// TODO: add specific material / material index to target in the mesh if there are multiple materials

		[Space(8f)]
		[Header("Display")]

		[Tooltip("Default texture to display when the video texture is preparing")]
		[SerializeField] Texture2D _defaultTexture = null;

		public Texture2D DefaultTexture
		{
			get { return _defaultTexture; }
			set { ChangeDefaultTexture(value); }
		}

		[Space(8f)]
		[FormerlySerializedAs("_mesh")]
		[Header("Renderer Target")]
		[SerializeField] Renderer _renderer = null;

		public Renderer MeshRenderer
		{
			get { return _renderer; }
			set { ChangeRenderer(value); }
		}

		[SerializeField] int _materialIndex = -1;

		public int MaterialIndex
		{
			get { return _materialIndex; }
			set { _materialIndex = value; }
		}

		private void ChangeDefaultTexture(Texture2D texture)
		{
			if (_defaultTexture != texture)
			{
				_defaultTexture = texture;
				ForceUpdate();
			}
		}
		private void ChangeRenderer(Renderer renderer)
		{
			if (_renderer != renderer)
			{
				if (_renderer)
				{
					// TODO: Remove from renderer
				}
				_renderer = renderer;
				if (_renderer)
				{
					ForceUpdate();
				}
			}
		}

		[SerializeField] string _texturePropertyName = Helper.UnityBaseTextureName;

		public string TexturePropertyName
		{
			get { return _texturePropertyName; }
			set
			{
				if (_texturePropertyName != value)
				{
					_texturePropertyName = value;
					// TODO: if the property changes, remove it from the perioud SetTexture()
					_propTexture = new LazyShaderProperty(_texturePropertyName);
					_isDirty = true;
				}
			}
		}

		[SerializeField] Vector2 _offset = Vector2.zero;

		public Vector2 Offset
		{
			get { return _offset; }
			set { if (_offset != value) { _offset = value; _isDirty = true; } }
		}

		[SerializeField] Vector2 _scale = Vector2.one;

		public Vector2 Scale
		{
			get { return _scale; }
			set { if (_scale != value) { _scale = value; _isDirty = true; } }
		}

		private Texture _lastTextureApplied;
		private LazyShaderProperty _propTexture;

		// We do a LateUpdate() to allow for any changes in the texture that may have happened in Update()
		private void LateUpdate()
		{
			Apply();
		}

		public override void Apply()
		{
			bool applied = false;

			// Try to apply texture from media
			if (_media != null && _media.TextureProducer != null)
			{
				Texture resamplerTex = _media.FrameResampler == null || _media.FrameResampler.OutputTexture == null ? null : _media.FrameResampler.OutputTexture[0];
				Texture texture = _media.UseResampler ? resamplerTex : _media.TextureProducer.GetTexture(0);
				if (texture != null)
				{
					// Check for changing texture
					if (texture != _lastTextureApplied)
					{
						_isDirty = true;
					}

					if (_isDirty)
					{
						int planeCount = _media.UseResampler ? 1 : _media.TextureProducer.GetTextureCount();
						for (int plane = 0; plane < planeCount; plane++)
						{
							Texture resamplerTexPlane = _media.FrameResampler == null || _media.FrameResampler.OutputTexture == null ? null : _media.FrameResampler.OutputTexture[plane];
							texture = _media.UseResampler ? resamplerTexPlane : _media.TextureProducer.GetTexture(plane);
							if (texture != null)
							{
								ApplyMapping(texture, _media.TextureProducer.RequiresVerticalFlip(), plane, _materialIndex);
							}
						}
					}
					applied = true;
				}
			}

			// If the media didn't apply a texture, then try to apply the default texture
			if (!applied)
			{
				if (_defaultTexture != _lastTextureApplied)
				{
					_isDirty = true;
				}
				if (_isDirty)
				{
					ApplyMapping(_defaultTexture, false, 0, _materialIndex);
				}
			}
		}

		private void ApplyMapping(Texture texture, bool requiresYFlip, int plane, int materialIndex = -1)
		{
			if (_renderer != null)
			{
				_isDirty = false;

				Material[] meshMaterials = _renderer.materials;
				if (meshMaterials != null)
				{
					for (int i = 0; i < meshMaterials.Length; i++)
					{
						if (_materialIndex < 0 || i == _materialIndex)
						{
							Material mat = meshMaterials[i];
							if (mat != null)
							{
								if (plane == 0)
								{
									VideoRender.SetupMaterialForMedia(mat, _media, _propTexture.Id, texture);
									_lastTextureApplied = texture;

									if(texture == _defaultTexture)	{ mat.EnableKeyword("USING_DEFAULT_TEXTURE"); }
									else							{ mat.DisableKeyword("USING_DEFAULT_TEXTURE"); }

									if (texture != null)
									{
										if (requiresYFlip)
										{
											mat.SetTextureScale(_propTexture.Id, new Vector2(_scale.x, -_scale.y));
											mat.SetTextureOffset(_propTexture.Id, Vector2.up + _offset);
										}
										else
										{
											mat.SetTextureScale(_propTexture.Id, _scale);
											mat.SetTextureOffset(_propTexture.Id, _offset);
										}
									}
								}
								else if (plane == 1)
								{
									if (texture != null)
									{
										if (requiresYFlip)
										{
											mat.SetTextureScale(VideoRender.PropChromaTex.Id, new Vector2(_scale.x, -_scale.y));
											mat.SetTextureOffset(VideoRender.PropChromaTex.Id, Vector2.up + _offset);
										}
										else
										{
											mat.SetTextureScale(VideoRender.PropChromaTex.Id, _scale);
											mat.SetTextureOffset(VideoRender.PropChromaTex.Id, _offset);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		protected override void OnEnable()
		{
			if (_renderer == null)
			{
				_renderer = this.GetComponent<MeshRenderer>();
				if (_renderer == null)
				{
					Debug.LogWarning("[AVProVideo] No MeshRenderer set or found in gameobject");
				}
			}

			_propTexture = new LazyShaderProperty(_texturePropertyName);

			ForceUpdate();
		}

		protected override void OnDisable()
		{
			ApplyMapping(_defaultTexture, false, 0, _materialIndex);
		}
	}
}