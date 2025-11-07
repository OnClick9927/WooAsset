// UnityEngine.UI was moved to a package in 2019.2.0
// Unfortunately no way to test for this across all Unity versions yet
// You can set up the asmdef to reference the new package, but the package doesn't
// existing in Unity 2017 etc, and it throws an error due to missing reference
#define AVPRO_PACKAGE_UNITYUI
#if (UNITY_2019_2_OR_NEWER && AVPRO_PACKAGE_UNITYUI) || (!UNITY_2019_2_OR_NEWER)

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_TVOS
	#define UNITY_PLATFORM_SUPPORTS_YPCBCR
#endif

#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_IOS || UNITY_TVOS || UNITY_ANDROID || (UNITY_WEBGL && UNITY_2017_2_OR_NEWER)
	#define UNITY_PLATFORM_SUPPORTS_LINEAR
#endif

#if (!UNITY_STANDALONE_WIN && !UNITY_EDITOR_WIN) && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_TVOS || UNITY_ANDROID)
	#define UNITY_PLATFORM_SUPPORTS_VIDEOTRANSFORM
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Displays the video from MediaPlayer component using uGUI
	/// </summary>
	[HelpURL("http://renderheads.com/products/avpro-video/")]
	[AddComponentMenu("AVPro Video/Display uGUI", 200)]
	//[ExecuteInEditMode]
	public class DisplayUGUI : MaskableGraphic
	{
		[SerializeField] MediaPlayer _mediaPlayer;

		public MediaPlayer Player
		{
			get { return _mediaPlayer; }
			set { ChangeMediaPlayer(value); }
		}

		[Tooltip("Default texture to display when the video texture is preparing")]
		[SerializeField] Texture _defaultTexture;

		public Texture DefaultTexture
		{
			get { return _defaultTexture; }
			set { if (_defaultTexture != value) { _defaultTexture = value; } }
		}

		[FormerlySerializedAs("m_UVRect")]
		[SerializeField] Rect _uvRect = new Rect(0f, 0f, 1f, 1f);

		public Rect UVRect
		{
			get { return _uvRect; }
			set { _uvRect = value; }
		}

		[SerializeField] bool _setNativeSize = false;

		public bool ApplyNativeSize
		{
			get { return _setNativeSize; }
			set { _setNativeSize = value; }
		}

		[SerializeField] ScaleMode _scaleMode = ScaleMode.ScaleToFit;

		public ScaleMode ScaleMode
		{
			get { return _scaleMode; }
			set { _scaleMode = value; }
		}

		[SerializeField] bool _noDefaultDisplay = true;

		public bool NoDefaultDisplay
		{
			get { return _noDefaultDisplay; }
			set { _noDefaultDisplay = value; }
		}

		[SerializeField] bool _displayInEditor = true;

		public bool DisplayInEditor
		{
			get { return _displayInEditor; }
			set { _displayInEditor = value; }
		}

		private int _lastWidth;
		private int _lastHeight;
		private Orientation _lastOrientation;
		private bool _flipY;
		private Texture _lastTexture;
		private static Shader _shaderStereoPacking;
		private static Shader _shaderAlphaPacking;
#if (!UNITY_EDITOR && UNITY_ANDROID)
		private static Shader _shaderAndroidOES;
#endif

		private bool _isUserMaterial = true;
		private Material _material;

		private List<UIVertex> _vertices = new List<UIVertex>(4);
		private static List<int> QuadIndices = new List<int>(new int[] { 0, 1, 2, 2, 3, 0 });

		protected override void Awake()
		{
			if (_mediaPlayer != null)
			{
				_mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
			}

			base.Awake();
		}

		// Callback function to handle events
		private void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			switch (et)
			{
				case MediaPlayerEvent.EventType.FirstFrameReady:
					if (_isUserMaterial && null != GetRequiredShader())
					{
						Debug.LogWarning("[AVProVideo] Custom material is being used but the video requires our internal shader for correct rendering.  Consider removing custom shader or modifying it for AVPro Video support.", this);
					}
					LateUpdate();
					break;
				case MediaPlayerEvent.EventType.PropertiesChanged:
				case MediaPlayerEvent.EventType.ResolutionChanged:
				case MediaPlayerEvent.EventType.Closing:
					LateUpdate();
					break;
			}
			// TODO: remove this, we're just doing this so we can make sure texture is correct when running in EDIT mode
			LateUpdate();
		}

		private void ChangeMediaPlayer(MediaPlayer player)
		{
			if (_mediaPlayer != player)
			{
				if (_mediaPlayer != null)
				{
					_mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
				}
				_mediaPlayer = player;
				if (_mediaPlayer != null)
				{
					_mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
				}
				LateUpdate();
			}
		}

		private static Shader EnsureShader(Shader shader, string name)
		{
			if (shader == null)
			{
				shader = Shader.Find(name);
				if (shader == null)
				{
					Debug.LogWarning("[AVProVideo] Missing shader " + name);
				}
			}

			return shader;
		}

		private static Shader EnsureAlphaPackingShader()
		{
			_shaderAlphaPacking = EnsureShader(_shaderAlphaPacking, "AVProVideo/Internal/UI/Transparent Packed");
			return _shaderAlphaPacking;
		}

		private static Shader EnsureStereoPackingShader()
		{
			_shaderStereoPacking = EnsureShader(_shaderStereoPacking, "AVProVideo/Internal/UI/Stereo");
			return _shaderStereoPacking;
		}

#if (!UNITY_EDITOR && UNITY_ANDROID)
		private Shader EnsureAndroidOESShader()
		{
			_shaderAndroidOES = EnsureShader(_shaderAndroidOES, "AVProVideo/Internal/UI/AndroidOES");
			return _shaderAndroidOES;
		}
#endif

		protected override void Start()
		{
			_isUserMaterial = (this.m_Material != null);
			if (_isUserMaterial)
			{
				_material = new Material(this.material);
				this.material = _material;
			}
			base.Start();
		}

		protected override void OnDestroy()
		{
			// Destroy existing material
			if (_material != null)
			{
				this.material = null;

#if UNITY_EDITOR
				Material.DestroyImmediate(_material);
#else
				Material.Destroy(_material);
#endif
				_material = null;
			}
			ChangeMediaPlayer(null);
			base.OnDestroy();
		}

		private Shader GetRequiredShader()
		{
			Shader result = null;

			if (result == null && _mediaPlayer.TextureProducer != null)
			{
				switch (_mediaPlayer.TextureProducer.GetTextureStereoPacking())
				{
					case StereoPacking.None:
						break;
					case StereoPacking.LeftRight:
					case StereoPacking.TopBottom:
					case StereoPacking.TwoTextures:
						result = EnsureStereoPackingShader();
						break;
				}

				if (_mediaPlayer.TextureProducer.GetTextureTransparency() == TransparencyMode.Transparent)
				{
					result = EnsureAlphaPackingShader();
				}

				switch (_mediaPlayer.TextureProducer.GetTextureAlphaPacking())
				{
					case AlphaPacking.None:
						break;
					case AlphaPacking.LeftRight:
					case AlphaPacking.TopBottom:
						result = EnsureAlphaPackingShader();
						break;
				}
			}

#if UNITY_PLATFORM_SUPPORTS_LINEAR
			if (result == null && _mediaPlayer.Info != null)
			{
				if (QualitySettings.activeColorSpace == ColorSpace.Linear && !_mediaPlayer.Info.PlayerSupportsLinearColorSpace())
				{
					result = EnsureAlphaPackingShader();
				}
			}
#endif
			if (result == null && _mediaPlayer.TextureProducer != null && _mediaPlayer.TextureProducer.GetTextureCount() == 2)
			{
				result = EnsureAlphaPackingShader();
			}

#if (!UNITY_EDITOR && UNITY_ANDROID)
			if (_mediaPlayer.PlatformOptionsAndroid.useFastOesPath)
			{
				result = EnsureAndroidOESShader();
			}
#endif
			return result;
		}

		/// <summary>
		/// Returns the texture used to draw this Graphic.
		/// </summary>
		public override Texture mainTexture
		{
			get
			{
				Texture result = Texture2D.whiteTexture;
				if (HasValidTexture())
				{
					Texture resamplerTex = _mediaPlayer.FrameResampler == null || _mediaPlayer.FrameResampler.OutputTexture == null ? null : _mediaPlayer.FrameResampler.OutputTexture[0];
					result = _mediaPlayer.UseResampler ? resamplerTex : _mediaPlayer.TextureProducer.GetTexture();
				}
				else
				{
					if (_noDefaultDisplay)
					{
						result = null;
					}
					else if (_defaultTexture != null)
					{
						result = _defaultTexture;
					}

#if UNITY_EDITOR
					if (result == null && _displayInEditor)
					{
						result = Resources.Load<Texture2D>("AVProVideoIcon");
					}
#endif
				}
				return result;
			}
		}

		public bool HasValidTexture()
		{
			return (Application.isPlaying && _mediaPlayer != null && _mediaPlayer.TextureProducer != null && _mediaPlayer.TextureProducer.GetTexture() != null);
		}

		private void UpdateInternalMaterial()
		{
			if (_mediaPlayer != null)
			{
				// Get required shader
				Shader currentShader = null;
				if (_material != null)
				{
					currentShader = _material.shader;
				}
				Shader nextShader = GetRequiredShader();

				// If the shader requirement has changed
				if (currentShader != nextShader)
				{
					// Destroy existing material
					if (_material != null)
					{
						this.material = null;
#if UNITY_EDITOR
						Material.DestroyImmediate(_material);
#else
						Material.Destroy(_material);
#endif
						_material = null;
					}

					// Create new material
					if (nextShader != null)
					{
						_material = new Material(nextShader);
					}
				}

				this.material = _material;
			}
		}

		// We do a LateUpdate() to allow for any changes in the texture that may have happened in Update()
		void LateUpdate()
		{
			if (_setNativeSize)
			{
				SetNativeSize();
			}

			if (_lastTexture != mainTexture)
			{
				_lastTexture = mainTexture;
				SetVerticesDirty();
				SetMaterialDirty();
			}

			if (HasValidTexture())
			{
				if (mainTexture != null)
				{
					Orientation orientation = Helper.GetOrientation(_mediaPlayer.Info.GetTextureTransform());
					if (mainTexture.width != _lastWidth || mainTexture.height != _lastHeight || orientation != _lastOrientation)
					{
						_lastWidth = mainTexture.width;
						_lastHeight = mainTexture.height;
						_lastOrientation = orientation;
						SetVerticesDirty();
						SetMaterialDirty();
					}
				}
			}

			if (Application.isPlaying)
			{
				if (!_isUserMaterial)
				{
					UpdateInternalMaterial();
				}
			}

			if (material != null && _mediaPlayer != null)
			{
				// TODO: only run when dirty
				VideoRender.SetupMaterialForMedia(materialForRendering, _mediaPlayer);
			}
		}

		/// <summary>
		/// Texture to be used.
		/// </summary>
		public MediaPlayer CurrentMediaPlayer
		{
			get
			{
				return _mediaPlayer;
			}
			set
			{
				if (_mediaPlayer != value)
				{
					_mediaPlayer = value;
					//SetVerticesDirty();
					SetMaterialDirty();
				}
			}
		}

		/// <summary>
		/// UV rectangle used by the texture.
		/// </summary>
		public Rect uvRect
		{
			get
			{
				return _uvRect;
			}
			set
			{
				if (_uvRect == value)
				{
					return;
				}
				_uvRect = value;
				SetVerticesDirty();
			}
		}

		/// <summary>
		/// Adjust the scale of the Graphic to make it pixel-perfect.
		/// </summary>
		[ContextMenu("Set Native Size")]
		public override void SetNativeSize()
		{
			Texture tex = mainTexture;
			if (tex != null)
			{
				int w = Mathf.RoundToInt(tex.width * uvRect.width);
				int h = Mathf.RoundToInt(tex.height * uvRect.height);

				if (_mediaPlayer != null)
				{
#if UNITY_PLATFORM_SUPPORTS_VIDEOTRANSFORM && !(!UNITY_EDITOR && UNITY_ANDROID)
					if (_mediaPlayer.Info != null)
					{
						Orientation ori = Helper.GetOrientation(_mediaPlayer.Info.GetTextureTransform());
						if (ori == Orientation.Portrait || ori == Orientation.PortraitFlipped)
						{
							w = Mathf.RoundToInt(tex.height * uvRect.width);
							h = Mathf.RoundToInt(tex.width * uvRect.height);
						}
					}
#endif
					if (_mediaPlayer.TextureProducer != null)
					{
						if (_mediaPlayer.TextureProducer.GetTextureAlphaPacking() == AlphaPacking.LeftRight ||
							_mediaPlayer.TextureProducer.GetTextureStereoPacking() == StereoPacking.LeftRight)
						{
							w /= 2;
						}
						else if (_mediaPlayer.TextureProducer.GetTextureAlphaPacking() == AlphaPacking.TopBottom ||
								 _mediaPlayer.TextureProducer.GetTextureStereoPacking() == StereoPacking.TopBottom)
						{
							h /= 2;
						}
					}
				}

				rectTransform.anchorMax = rectTransform.anchorMin;
				rectTransform.sizeDelta = new Vector2(w, h);
			}
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();

			_OnFillVBO(_vertices);

			vh.AddUIVertexStream(_vertices, QuadIndices );
		}

		private void _OnFillVBO(List<UIVertex> vbo)
		{
			_flipY = false;
			if (HasValidTexture())
			{
				_flipY = _mediaPlayer.TextureProducer.RequiresVerticalFlip();
			}

			Rect uvRect = _uvRect;
			Vector4 v = GetDrawingDimensions(_scaleMode, ref uvRect);

#if UNITY_PLATFORM_SUPPORTS_VIDEOTRANSFORM
			Matrix4x4 m = Matrix4x4.identity;
			if (HasValidTexture())
			{
				m = Helper.GetMatrixForOrientation(Helper.GetOrientation(_mediaPlayer.Info.GetTextureTransform()));
			}
#endif

			vbo.Clear();

			var vert = UIVertex.simpleVert;
			vert.color = color;

			vert.position = new Vector2(v.x, v.y);

			vert.uv0 = new Vector2(uvRect.xMin, uvRect.yMin);
			if (_flipY)
			{
				vert.uv0 = new Vector2(uvRect.xMin, 1.0f - uvRect.yMin);
			}
#if UNITY_PLATFORM_SUPPORTS_VIDEOTRANSFORM
			vert.uv0 = m.MultiplyPoint3x4(vert.uv0);
#endif
			vbo.Add(vert);

			vert.position = new Vector2(v.x, v.w);
			vert.uv0 = new Vector2(uvRect.xMin, uvRect.yMax);
			if (_flipY)
			{
				vert.uv0 = new Vector2(uvRect.xMin, 1.0f - uvRect.yMax);
			}
#if UNITY_PLATFORM_SUPPORTS_VIDEOTRANSFORM
			vert.uv0 = m.MultiplyPoint3x4(vert.uv0);
#endif
			vbo.Add(vert);

			vert.position = new Vector2(v.z, v.w);
			vert.uv0 = new Vector2(uvRect.xMax, uvRect.yMax);
			if (_flipY)
			{
				vert.uv0 = new Vector2(uvRect.xMax, 1.0f - uvRect.yMax);
			}
#if UNITY_PLATFORM_SUPPORTS_VIDEOTRANSFORM
			vert.uv0 = m.MultiplyPoint3x4(vert.uv0);
#endif
			vbo.Add(vert);

			vert.position = new Vector2(v.z, v.y);
			vert.uv0 = new Vector2(uvRect.xMax, uvRect.yMin);
			if (_flipY)
			{
				vert.uv0 = new Vector2(uvRect.xMax, 1.0f - uvRect.yMin);
			}
#if UNITY_PLATFORM_SUPPORTS_VIDEOTRANSFORM
			vert.uv0 = m.MultiplyPoint3x4(vert.uv0);
#endif
			vbo.Add(vert);
		}

		private Vector4 GetDrawingDimensions(ScaleMode scaleMode, ref Rect uvRect)
		{
			Vector4 returnSize = Vector4.zero;

			if (mainTexture != null)
			{
				var padding = Vector4.zero;

				var textureSize = new Vector2(mainTexture.width, mainTexture.height);
				{
					// Adjust textureSize based on orientation
#if UNITY_PLATFORM_SUPPORTS_VIDEOTRANSFORM && !(!UNITY_EDITOR && UNITY_ANDROID)
					if (HasValidTexture())
					{
						Matrix4x4 m = Helper.GetMatrixForOrientation(Helper.GetOrientation(_mediaPlayer.Info.GetTextureTransform()));
						textureSize = m.MultiplyVector(textureSize);
						textureSize.x = Mathf.Abs(textureSize.x);
						textureSize.y = Mathf.Abs(textureSize.y);
					}
#endif
					// Adjust textureSize based on alpha/stereo packing
					if (_mediaPlayer != null && _mediaPlayer.TextureProducer != null)
					{
						if (_mediaPlayer.TextureProducer.GetTextureAlphaPacking() == AlphaPacking.LeftRight ||
							_mediaPlayer.TextureProducer.GetTextureStereoPacking() == StereoPacking.LeftRight)
						{
							textureSize.x /= 2;
						}
						else if (_mediaPlayer.TextureProducer.GetTextureAlphaPacking() == AlphaPacking.TopBottom ||
								 _mediaPlayer.TextureProducer.GetTextureStereoPacking() == StereoPacking.TopBottom)
						{
							textureSize.y /= 2;
						}
					}
				}

				Rect r = GetPixelAdjustedRect();

				// Fit the above textureSize into rectangle r
				int spriteW = Mathf.RoundToInt( textureSize.x );
				int spriteH = Mathf.RoundToInt( textureSize.y );

				var size = new Vector4( padding.x / spriteW,
										padding.y / spriteH,
										(spriteW - padding.z) / spriteW,
										(spriteH - padding.w) / spriteH );


				{
					if (textureSize.sqrMagnitude > 0.0f)
					{
						if (scaleMode == ScaleMode.ScaleToFit)
						{
							float spriteRatio = textureSize.x / textureSize.y;
							float rectRatio = r.width / r.height;

							if (spriteRatio > rectRatio)
							{
								float oldHeight = r.height;
								r.height = r.width * (1.0f / spriteRatio);
								r.y += (oldHeight - r.height) * rectTransform.pivot.y;
							}
							else
							{
								float oldWidth = r.width;
								r.width = r.height * spriteRatio;
								r.x += (oldWidth - r.width) * rectTransform.pivot.x;
							}
						}
						else if (scaleMode == ScaleMode.ScaleAndCrop)
						{
							float aspectRatio = textureSize.x / textureSize.y;
							float screenRatio = r.width / r.height;
							if (screenRatio > aspectRatio)
							{
								float adjust = aspectRatio / screenRatio;
								uvRect = new Rect(uvRect.xMin, (uvRect.yMin * adjust) + (1f - adjust) * 0.5f, uvRect.width, adjust * uvRect.height);
							}
							else
							{
								float adjust = screenRatio / aspectRatio;
								uvRect = new Rect(uvRect.xMin * adjust + (0.5f - adjust * 0.5f), uvRect.yMin, adjust * uvRect.width, uvRect.height);
							}
						}
					}
				}

				returnSize = new Vector4( r.x + r.width * size.x,
										  r.y + r.height * size.y,
										  r.x + r.width * size.z,
										  r.y + r.height * size.w  );

			}

			return returnSize;
		}
	}
}
#endif