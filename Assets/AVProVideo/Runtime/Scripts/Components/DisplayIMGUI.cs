#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_IOS || UNITY_TVOS || UNITY_ANDROID || (UNITY_WEBGL && UNITY_2017_2_OR_NEWER)
	#define UNITY_PLATFORM_SUPPORTS_LINEAR
#endif

using UnityEngine;
using UnityEngine.Serialization;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Displays the video from MediaPlayer component using IMGUI
	/// </summary>
	[AddComponentMenu("AVPro Video/Display IMGUI", 200)]
	[HelpURL("http://renderheads.com/products/avpro-video/")]
	[ExecuteInEditMode]
	public class DisplayIMGUI : MonoBehaviour
	{
		[SerializeField] MediaPlayer _mediaPlayer = null;
		public MediaPlayer Player
		{
			get { return _mediaPlayer; }
			set { _mediaPlayer = value; Update(); }
		}

		[SerializeField] ScaleMode	_scaleMode	= ScaleMode.ScaleToFit;
		public ScaleMode ScaleMode { get { return _scaleMode; } set { _scaleMode = value; } }

		[SerializeField] Color _color = UnityEngine.Color.white;
		public Color Color { get { return _color; } set { _color = value; } }

		[FormerlySerializedAs("_alphaBlend")]
		[SerializeField] bool _allowTransparency = false;
		public bool AllowTransparency { get { return _allowTransparency; } set { _allowTransparency = value; } }

		[SerializeField] bool _useDepth = false;
		public bool UseDepth { get { return _useDepth; } set { _useDepth = value; } }

		[SerializeField] int _depth = 0;
		public int Depth { get { return _depth; } set { _depth = value; } }

		[Header("Area")]

		[FormerlySerializedAs("_fullScreen")]
		[SerializeField] bool _isAreaFullScreen = true;
		public bool IsAreaFullScreen { get { return _isAreaFullScreen; } set { _isAreaFullScreen = value; } }

		[FormerlySerializedAs("_x")]
		[Range(0f, 1f)]
		[SerializeField] float _areaX = 0f;
		public float AreaX { get { return _areaX; } set { _areaX = value; } }

		[FormerlySerializedAs("_y")]
		[Range(0f, 1f)]
		[SerializeField] float _areaY = 0f;
		public float AreaY { get { return _areaY; } set { _areaY = value; } }

		[FormerlySerializedAs("_width")]
		[Range(0f, 1f)]
		[SerializeField] float _areaWidth = 1f;
		public float AreaWidth { get { return _areaWidth; } set { _areaWidth = value; } }

		[FormerlySerializedAs("_height")]
		[Range(0f, 1f)]
		[SerializeField] float _areaHeight = 1f;
		public float AreaHeight { get { return _areaHeight; } set { _areaHeight = value; } }

		[FormerlySerializedAs("_displayInEditor")]
		[SerializeField] bool _showAreaInEditor = false;
		public bool ShowAreaInEditor { get { return _showAreaInEditor; } set { _showAreaInEditor = value; } }

		private static Shader	_shaderAlphaPacking;
		private Material		_material;

		void Start()
		{
			// Disabling useGUILayout lets you skip the GUI layout phase which helps performance, but this also breaks the GUI.depth usage.
			if (!_useDepth)
			{
				this.useGUILayout = false;
			}

			if (!_shaderAlphaPacking)
			{
				_shaderAlphaPacking = Shader.Find("AVProVideo/Internal/IMGUI/Texture Transparent");
				if (!_shaderAlphaPacking)
				{
					Debug.LogWarning("[AVProVideo] Missing shader 'AVProVideo/IMGUI/Transparent Packed'");
				}
			}
		}

		public void Update()
		{
			if (_mediaPlayer != null)
			{
				SetupMaterial();
			}
		}
		
		void OnDestroy()
		{
			// Destroy existing material
			if (_material != null)
			{
#if UNITY_EDITOR
				Material.DestroyImmediate(_material);
#else
				Material.Destroy(_material);
#endif
				_material = null;
			}
		}

		private Shader GetRequiredShader()
		{
			Shader result = null;

			if (result == null && _mediaPlayer.TextureProducer != null)
			{
				switch (_mediaPlayer.TextureProducer.GetTextureAlphaPacking())
				{
					case AlphaPacking.None:
						break;
					case AlphaPacking.LeftRight:
					case AlphaPacking.TopBottom:
						result = _shaderAlphaPacking;
						break;
				}
			}

#if UNITY_PLATFORM_SUPPORTS_LINEAR
			if (result == null && _mediaPlayer.Info != null)
			{
				// If the player does support generating sRGB textures then we need to use a shader to convert them for display via IMGUI
				if (QualitySettings.activeColorSpace == ColorSpace.Linear && !_mediaPlayer.Info.PlayerSupportsLinearColorSpace())
				{
					result = _shaderAlphaPacking;
				}
			}
#endif
			if (result == null && _mediaPlayer.TextureProducer != null)
			{
				if (_mediaPlayer.TextureProducer.GetTextureCount() == 2)
				{
					result = _shaderAlphaPacking;
				}
			}
			return result;
		}

		private void SetupMaterial()
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
		}

#if UNITY_EDITOR
		private void DrawArea()
		{
			Rect rect = GetAreaRect();
			Rect uv = rect;
			uv.x /= Screen.width;
			uv.width /= Screen.width;
			uv.y /= Screen.height;
			uv.height /= Screen.height;
			uv.width *= 16f;
			uv.height *= 16f;
			uv.x += 0.5f;
			uv.y += 0.5f;
			Texture2D icon = Resources.Load<Texture2D>("AVProVideoIcon");
			GUI.depth = _depth;
			GUI.color = _color;
			GUI.DrawTextureWithTexCoords(rect, icon, uv);
		}
#endif

		void OnGUI()
		{
#if UNITY_EDITOR
			if (_showAreaInEditor && !Application.isPlaying)
			{
				DrawArea();
				return;
			}
#endif

			if (_mediaPlayer == null)
			{
				return;
			}

			Texture texture = null;
			if (_showAreaInEditor)
			{
#if UNITY_EDITOR
				texture = Texture2D.whiteTexture;
#endif
			}
			texture = VideoRender.GetTexture(_mediaPlayer, 0);
			if (_mediaPlayer.Info != null && !_mediaPlayer.Info.HasVideo())
			{
				texture = null;
			}

			if (texture != null)
			{
				bool isTextureVisible = (_color.a > 0f || !_allowTransparency);
				if (isTextureVisible)
				{
					GUI.depth = _depth;
					GUI.color = _color;

					Rect rect = GetAreaRect();

					// TODO: change this to a material-only path so we only have a single drawing path
					if (_material != null)
					{
						// TODO: Only setup material when needed
						VideoRender.SetupMaterialForMedia(_material, _mediaPlayer);

						// NOTE: It seems that Graphics.DrawTexture() behaves differently than GUI.DrawTexture() when it comes to sRGB writing
						// on newer versions of Unity (at least 2018.2.19 and above), so now we have to force the conversion to sRGB on writing
						bool restoreSRGBWrite = false;
#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
						if (QualitySettings.activeColorSpace == ColorSpace.Linear && !GL.sRGBWrite)
						{
							restoreSRGBWrite = true;
						}
#endif
						if (restoreSRGBWrite)
						{
							GL.sRGBWrite = true;
						}

						VideoRender.DrawTexture(rect, texture, _scaleMode, _mediaPlayer.TextureProducer.GetTextureAlphaPacking(), _material);
						
						if (restoreSRGBWrite)
						{
							GL.sRGBWrite = false;
						}
					}
					else
					{
						bool requiresVerticalFlip = false;
						if (_mediaPlayer.TextureProducer != null)
						{
							requiresVerticalFlip = _mediaPlayer.TextureProducer.RequiresVerticalFlip();
						}
						if (requiresVerticalFlip)
						{
							GUIUtility.ScaleAroundPivot(new Vector2(1f, -1f), new Vector2(0f, rect.y + (rect.height / 2f)));
						}
						GUI.DrawTexture(rect, texture, _scaleMode, _allowTransparency);
					}
				}
			}
		}

		public Rect GetAreaRect()
		{
			Rect rect;
			if (_isAreaFullScreen)
			{
				rect = new Rect(0.0f, 0.0f, Screen.width, Screen.height);
			}
			else
			{
				rect = new Rect(_areaX * (Screen.width - 1), _areaY * (Screen.height - 1), _areaWidth * Screen.width, _areaHeight * Screen.height);
			}

			return rect;
		}
	}
}
