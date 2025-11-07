#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_TVOS
	#define UNITY_PLATFORM_SUPPORTS_YPCBCR
#endif

#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_IOS || UNITY_TVOS || UNITY_ANDROID || (UNITY_WEBGL && UNITY_2017_2_OR_NEWER)
	#define UNITY_PLATFORM_SUPPORTS_LINEAR
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
#if AVPRO_FEATURE_VIDEORESOLVE
	[System.Serializable]
	public class VideoResolve : ITextureProducer
	{
		[SerializeField] VideoResolveOptions _options = VideoResolveOptions.Create();
		[SerializeField] RenderTexture _targetRenderTexture = null;
		[SerializeField] ScaleMode _targetRenderTextureScale = ScaleMode.ScaleToFit;

		void SetSource(ITextureProducer textureSource)
		{
			//_commandBuffer.IssuePluginEvent(blahCallback, 0);
			//Graphics.ExecuteCommandBuffer(_commandBuffer);
		}

		// ITextureProducer implementation
		
		/// <inheritdoc/>
		public int GetTextureCount() { return 1; }
		
		/// <inheritdoc/>
		public Texture GetTexture(int index = 0) { return _texture; }
		
		/// <inheritdoc/>
		public int GetTextureFrameCount() { return _textureSource.GetTextureFrameCount(); }
		
		/// <inheritdoc/>
		public bool SupportsTextureFrameCount() { return _textureSource.SupportsTextureFrameCount(); }
		
		/// <inheritdoc/>
		public long GetTextureTimeStamp() { return _textureSource.GetTextureTimeStamp(); }

		/// <inheritdoc/>
		public bool RequiresVerticalFlip() { return false; }

		/// <inheritdoc/>
		public StereoPacking GetTextureStereoPacking() { return StereoPacking.None; }

		/// <inheritdoc/>
		public TransparencyMode GetTextureTransparency() { return TransparencyMode.Transparent; }

		/// <inheritdoc/>
		public AlphaPacking GetTextureAlphaPacking() { return AlphaPacking.None; }

		/// <inheritdoc/>
		public Matrix4x4 GetYpCbCrTransform() { return Matrix4x4.identity; }

		private ITextureProducer _textureSource;
		private Texture _texture;
		private CommandBuffer _commandBuffer;
	}
#endif

	public struct LazyShaderProperty
	{
		public LazyShaderProperty(string name)
		{
			_name = name;
			_id = 0;
		}

		public string Name { get { return _name;} }
		public int Id { get { if (_id == 0) { _id = Shader.PropertyToID(_name); } return _id; } }

		private string _name;
		private int _id;
	}

	/// <summary>Helper class for everything related to setting up materials for rendering/resolving videos</summary>
	public class VideoRender
	{
		public const string Shader_IMGUI = "AVProVideo/Internal/IMGUI/Texture Transparent";
		public const string Shader_Resolve = "AVProVideo/Internal/Resolve";
		public const string Shader_Preview = "AVProVideo/Internal/Preview";

	#if UNITY_PLATFORM_SUPPORTS_YPCBCR
		public const string Keyword_UseYpCbCr = "USE_YPCBCR";
	#endif
		public const string Keyword_AlphaPackTopBottom = "ALPHAPACK_TOP_BOTTOM";
		public const string Keyword_AlphaPackLeftRight = "ALPHAPACK_LEFT_RIGHT";
		public const string Keyword_AlphaPackNone = "ALPHAPACK_NONE";
		public const string Keyword_StereoTopBottom = "STEREO_TOP_BOTTOM";
		public const string Keyword_StereoLeftRight = "STEREO_LEFT_RIGHT";
		public const string Keyword_StereoCustomUV = "STEREO_CUSTOM_UV";
		public const string Keyword_StereoTwoTextures = "STEREO_TWOTEXTURES";
		public const string Keyword_StereoNone = "MONOSCOPIC";
		public const string Keyword_StereoDebug = "STEREO_DEBUG";
		public const string Keyword_LayoutEquirect180 = "LAYOUT_EQUIRECT180";
		public const string Keyword_LayoutNone = "LAYOUT_NONE";
		public const string Keyword_ForceEyeNone = "FORCEEYE_NONE";
		public const string Keyword_ForceEyeLeft = "FORCEEYE_LEFT";
		public const string Keyword_ForceEyeRight = "FORCEEYE_RIGHT";
		public const string Keyword_ApplyGamma = "APPLY_GAMMA";

		public static readonly LazyShaderProperty PropChromaTex = new LazyShaderProperty("_ChromaTex");

	#if UNITY_PLATFORM_SUPPORTS_YPCBCR
		public static readonly LazyShaderProperty PropYpCbCrTransform = new LazyShaderProperty("_YpCbCrTransform");
		public static readonly LazyShaderProperty PropUseYpCbCr = new LazyShaderProperty("_UseYpCbCr");
	#endif

		public static readonly LazyShaderProperty PropVertScale = new LazyShaderProperty("_VertScale");
		public static readonly LazyShaderProperty PropApplyGamma = new LazyShaderProperty("_ApplyGamma");
		public static readonly LazyShaderProperty PropStereo = new LazyShaderProperty("Stereo");
		public static readonly LazyShaderProperty PropAlphaPack = new LazyShaderProperty("AlphaPack");
		public static readonly LazyShaderProperty PropLayout = new LazyShaderProperty("Layout");
		public static readonly LazyShaderProperty PropViewMatrix = new LazyShaderProperty("_ViewMatrix");
		public static readonly LazyShaderProperty PropTextureMatrix = new LazyShaderProperty("_TextureMatrix");

		public static readonly LazyShaderProperty PropUseHSBC = new LazyShaderProperty("_UseHSBC");
		public static readonly LazyShaderProperty PropHue = new LazyShaderProperty("_Hue");
		public static readonly LazyShaderProperty PropSaturation = new LazyShaderProperty("_Saturation");
		public static readonly LazyShaderProperty PropContrast = new LazyShaderProperty("_Contrast");
		public static readonly LazyShaderProperty PropBrightness = new LazyShaderProperty("_Brightness");
		public static readonly LazyShaderProperty PropInvGamma = new LazyShaderProperty("_InvGamma");

		public static Material CreateResolveMaterial()
		{
			return new Material(Shader.Find(VideoRender.Shader_Resolve));
		}

		public static Material CreateIMGUIMaterial()
		{
			return new Material(Shader.Find(VideoRender.Shader_Preview));
		}

		public static void SetupLayoutMaterial(Material material, VideoMapping mapping)
		{
			switch (mapping)
			{
				default:
					material.DisableKeyword(Keyword_LayoutEquirect180);
					material.EnableKeyword(Keyword_LayoutNone);
					break;
				// Only EquiRectangular180 currently does anything in the shader
				case VideoMapping.EquiRectangular180:
					material.DisableKeyword(Keyword_LayoutNone);
					material.EnableKeyword(Keyword_LayoutEquirect180);
					break;
			}
		}

		public static void SetupStereoEyeModeMaterial(Material material, StereoEye mode)
		{
			switch (mode)
			{
				case StereoEye.Both:
					material.DisableKeyword(Keyword_ForceEyeLeft);
					material.DisableKeyword(Keyword_ForceEyeRight);
					material.EnableKeyword(Keyword_ForceEyeNone);
					break;
				case StereoEye.Left:
					material.DisableKeyword(Keyword_ForceEyeNone);
					material.DisableKeyword(Keyword_ForceEyeRight);
					material.EnableKeyword(Keyword_ForceEyeLeft);
					break;
				case StereoEye.Right:
					material.DisableKeyword(Keyword_ForceEyeNone);
					material.DisableKeyword(Keyword_ForceEyeLeft);
					material.EnableKeyword(Keyword_ForceEyeRight);
					break;
			}
		}

		public static void SetupStereoMaterial(Material material, StereoPacking packing)
		{
			switch (packing)
			{
				case StereoPacking.None:
					material.DisableKeyword(Keyword_StereoTopBottom);
					material.DisableKeyword(Keyword_StereoLeftRight);
					material.DisableKeyword(Keyword_StereoCustomUV);
					material.DisableKeyword(Keyword_StereoTwoTextures);
					material.EnableKeyword(Keyword_StereoNone);
					break;
				case StereoPacking.TopBottom:
					material.DisableKeyword(Keyword_StereoNone);
					material.DisableKeyword(Keyword_StereoLeftRight);
					material.DisableKeyword(Keyword_StereoCustomUV);
					material.DisableKeyword(Keyword_StereoTwoTextures);
					material.EnableKeyword(Keyword_StereoTopBottom);
					break;
				case StereoPacking.LeftRight:
					material.DisableKeyword(Keyword_StereoNone);
					material.DisableKeyword(Keyword_StereoTopBottom);
					material.DisableKeyword(Keyword_StereoTwoTextures);
					material.DisableKeyword(Keyword_StereoCustomUV);
					material.EnableKeyword(Keyword_StereoLeftRight);
					break;
				case StereoPacking.CustomUV:
					material.DisableKeyword(Keyword_StereoNone);
					material.DisableKeyword(Keyword_StereoTopBottom);
					material.DisableKeyword(Keyword_StereoLeftRight);
					material.DisableKeyword(Keyword_StereoTwoTextures);
					material.EnableKeyword(Keyword_StereoCustomUV);
					break;
				case StereoPacking.TwoTextures:
					material.DisableKeyword(Keyword_StereoNone);
					material.DisableKeyword(Keyword_StereoTopBottom);
					material.DisableKeyword(Keyword_StereoLeftRight);
					material.DisableKeyword(Keyword_StereoCustomUV);
					material.EnableKeyword(Keyword_StereoTwoTextures);
					break;
			}
		}

		public static void SetupGlobalDebugStereoTinting(bool enabled)
		{
			if (enabled)
			{
				Shader.EnableKeyword(Keyword_StereoDebug);
			}
			else
			{
				Shader.DisableKeyword(Keyword_StereoDebug);
			}
		}

		public static void SetupAlphaPackedMaterial(Material material, AlphaPacking packing)
		{
			switch (packing)
			{
				case AlphaPacking.None:
					material.DisableKeyword(Keyword_AlphaPackTopBottom);
					material.DisableKeyword(Keyword_AlphaPackLeftRight);
					material.EnableKeyword(Keyword_AlphaPackNone);
					break;
				case AlphaPacking.TopBottom:
					material.DisableKeyword(Keyword_AlphaPackNone);
					material.DisableKeyword(Keyword_AlphaPackLeftRight);
					material.EnableKeyword(Keyword_AlphaPackTopBottom);
					break;
				case AlphaPacking.LeftRight:
					material.DisableKeyword(Keyword_AlphaPackNone);
					material.DisableKeyword(Keyword_AlphaPackTopBottom);
					material.EnableKeyword(Keyword_AlphaPackLeftRight);
					break;
			}
		}

		public static void SetupGammaMaterial(Material material, bool playerSupportsLinear)
		{
#if UNITY_PLATFORM_SUPPORTS_LINEAR
			if (QualitySettings.activeColorSpace == ColorSpace.Linear && !playerSupportsLinear)
			{
				material.EnableKeyword(Keyword_ApplyGamma);
			}
			else
			{
				material.DisableKeyword(Keyword_ApplyGamma);
			}
#endif
		}

		public static void SetupTextureMatrix(Material material, float[] transform)
		{
#if (!UNITY_EDITOR && UNITY_ANDROID)
// STE: HasProperty doesn't work on Matrix'
//			if (material != null && (material.HasProperty(VideoRender.PropTextureMatrix.Id)))
			{
				if (transform != null)
				{
					material.SetMatrix(VideoRender.PropTextureMatrix.Id, new Matrix4x4(	new Vector4( transform[0], transform[1], transform[2], transform[3] ), 
																						new Vector4( transform[4], transform[5], transform[6], transform[7] ), 
																						new Vector4( transform[8], transform[9], transform[10], transform[11] ), 
																						new Vector4( transform[12], transform[13], transform[14], transform[15] ) ));
				}
				else
				{
					material.SetMatrix(VideoRender.PropTextureMatrix.Id, Matrix4x4.identity);
				}
			}
#endif
		}

#if UNITY_PLATFORM_SUPPORTS_YPCBCR
		public static void SetupYpCbCrMaterial(Material material, bool enable, Matrix4x4 transform, Texture texture)
		{
			if (material.HasProperty(VideoRender.PropUseYpCbCr.Id))
			{
				if (enable)
				{
					material.EnableKeyword(VideoRender.Keyword_UseYpCbCr);
					material.SetMatrix(VideoRender.PropYpCbCrTransform.Id, transform);
					material.SetTexture(VideoRender.PropChromaTex.Id, texture);
				}
				else
				{
					material.DisableKeyword(VideoRender.Keyword_UseYpCbCr);
				}
			}
		}
#endif

		public static void SetupVerticalFlipMaterial(Material material, bool flip)
		{
			material.SetFloat(VideoRender.PropVertScale.Id, flip?-1f:1f);
		}

		public static Texture GetTexture(MediaPlayer mediaPlayer, int textureIndex)
		{
			Texture result = null;
			if (mediaPlayer != null)
			{
				if (mediaPlayer.UseResampler && mediaPlayer.FrameResampler != null && mediaPlayer.FrameResampler.OutputTexture != null)
				{
					if ( mediaPlayer.FrameResampler.OutputTexture.Length > textureIndex)
					{
						result = mediaPlayer.FrameResampler.OutputTexture[textureIndex];
					}
				}
				else if (mediaPlayer.TextureProducer != null)
				{
					if (mediaPlayer.TextureProducer.GetTextureCount() > textureIndex)
					{
						result = mediaPlayer.TextureProducer.GetTexture(textureIndex);
					}
				}
			}
			return result;
		}

		public static void SetupMaterialForMedia(Material material, MediaPlayer mediaPlayer, int texturePropId = -1, Texture fallbackTexture = null)
		{
			Debug.Assert(material != null);
			if (mediaPlayer != null)
			{
				Texture mainTexture = GetTexture(mediaPlayer, 0);
				Texture yCbCrTexture = GetTexture(mediaPlayer, 1);

				if (texturePropId != -1)
				{
					if (mainTexture == null)
					{
						mainTexture = fallbackTexture;
					}
					material.SetTexture(texturePropId, mainTexture);
				}

				SetupMaterial(material,
							(mediaPlayer.TextureProducer != null)?mediaPlayer.TextureProducer.RequiresVerticalFlip():false,
							(mediaPlayer.Info != null)?mediaPlayer.Info.PlayerSupportsLinearColorSpace():true,
							(mediaPlayer.TextureProducer != null)?mediaPlayer.TextureProducer.GetYpCbCrTransform():Matrix4x4.identity,
							yCbCrTexture,
							(mediaPlayer.Info != null && mediaPlayer.PlatformOptionsAndroid.useFastOesPath)?mediaPlayer.Info.GetTextureTransform():null,
							mediaPlayer.VideoLayoutMapping,
							(mediaPlayer.TextureProducer != null)?mediaPlayer.TextureProducer.GetTextureStereoPacking():StereoPacking.None,
							(mediaPlayer.TextureProducer != null)?mediaPlayer.TextureProducer.GetTextureAlphaPacking():AlphaPacking.None);
			}
			else
			{
				if (texturePropId != -1)
				{
					material.SetTexture(texturePropId, fallbackTexture);
				}
				SetupMaterial(material, false, true, Matrix4x4.identity, null);
			}
		}

		internal static void SetupMaterial(Material material, bool flipVertically, bool playerSupportsLinear, Matrix4x4 ycbcrTransform, Texture ycbcrTexture = null, float[] textureTransform = null,
			VideoMapping mapping = VideoMapping.Normal, StereoPacking stereoPacking = StereoPacking.None, AlphaPacking alphaPacking = AlphaPacking.None)
		{
			SetupVerticalFlipMaterial(material, flipVertically);

			// Apply changes for layout
			if (material.HasProperty(VideoRender.PropLayout.Id))
			{
				VideoRender.SetupLayoutMaterial(material, mapping);
			}

			// Apply changes for stereo videos
			if (material.HasProperty(VideoRender.PropStereo.Id))
			{
				VideoRender.SetupStereoMaterial(material, stereoPacking);
			}

			// Apply changes for alpha videos
			if (material.HasProperty(VideoRender.PropAlphaPack.Id))
			{
				VideoRender.SetupAlphaPackedMaterial(material, alphaPacking);
			}

			// Apply gamma correction
			#if UNITY_PLATFORM_SUPPORTS_LINEAR
			if (material.HasProperty(VideoRender.PropApplyGamma.Id))
			{
				VideoRender.SetupGammaMaterial(material, playerSupportsLinear);
			}
			#endif

			// Adjust for cropping (when the decoder decodes in blocks that overrun the video frame size, it pads), OES only as we apply this lower down for none-OES
			#if (!UNITY_EDITOR && UNITY_ANDROID)
// STE: HasProperty doesn't work on Matrix'
//			if (material.HasProperty(VideoRender.PropTextureMatrix.Id))
			{
				VideoRender.SetupTextureMatrix(material, textureTransform);
			}
			#endif

			#if UNITY_PLATFORM_SUPPORTS_YPCBCR
			VideoRender.SetupYpCbCrMaterial(material, ycbcrTexture != null, ycbcrTransform, ycbcrTexture);
			#endif
		}

		[System.Flags]
		public enum ResolveFlags : int
		{
			Mipmaps			= 1 << 0,
			PackedAlpha		= 1 << 1,
			StereoLeft		= 1 << 2,
			StereoRight		= 1 << 3,
			ColorspaceSRGB	= 1 << 4,
		}

		public static RenderTexture ResolveVideoToRenderTexture(Material resolveMaterial, RenderTexture targetTexture, ITextureProducer texture, ResolveFlags flags, ScaleMode scaleMode = ScaleMode.StretchToFill)
		{
			int targetWidth = texture.GetTexture(0).width;
			int targetHeight = texture.GetTexture(0).height;
			GetResolveTextureSize(texture.GetTextureAlphaPacking(), texture.GetTextureStereoPacking(), StereoEye.Left, ref targetWidth, ref targetHeight);

			if (targetTexture)
			{
				bool sizeChanged = (targetTexture.width != targetWidth || targetTexture.height != targetHeight);
				if (sizeChanged)
				{
					RenderTexture.ReleaseTemporary(targetTexture); targetTexture = null;
				}
			}

			if (!targetTexture)
			{
				RenderTextureReadWrite readWrite = ((flags & ResolveFlags.ColorspaceSRGB) == ResolveFlags.ColorspaceSRGB) ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear;
				targetTexture = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32, readWrite);
			}

			// Set target mipmap generation support
			{
				bool requiresMipmap = (flags & ResolveFlags.Mipmaps) == ResolveFlags.Mipmaps;
				bool requiresRecreate = (targetTexture.IsCreated() && targetTexture.useMipMap != requiresMipmap);
				if (requiresRecreate)
				{
					targetTexture.Release();
				}
				if (!targetTexture.IsCreated())
				{
					targetTexture.useMipMap = targetTexture.autoGenerateMips = requiresMipmap;
					targetTexture.Create();
				}
			}

			// Render resolve blit
			// TODO: combine these two paths into a single material blit
			{
				bool prevSRGB = GL.sRGBWrite;
				GL.sRGBWrite = targetTexture.sRGB;
				RenderTexture prev = RenderTexture.active;
				if (scaleMode == ScaleMode.StretchToFill)
				{
					Graphics.Blit(texture.GetTexture(0), targetTexture, resolveMaterial);
				}
				else
				{
					RenderTexture.active = targetTexture;
					bool partialAreaRender = (scaleMode == ScaleMode.ScaleToFit);
					if (partialAreaRender)
					{
						GL.Clear(false, true, Color.black);
					}
					VideoRender.DrawTexture(new Rect(0f, 0f, targetTexture.width, targetTexture.height), texture.GetTexture(0), scaleMode, texture.GetTextureAlphaPacking(), resolveMaterial);
				}
				RenderTexture.active = prev;
				GL.sRGBWrite = prevSRGB;
			}

			return targetTexture;
		}

		public static void GetResolveTextureSize(AlphaPacking alphaPacking, StereoPacking stereoPacking, StereoEye eyeMode, ref int width, ref int height)
		{
			switch (alphaPacking)
			{
				case AlphaPacking.LeftRight:
					width /= 2;
					break;
				case AlphaPacking.TopBottom:
					height /= 2;
					break;
			}
			if (eyeMode != StereoEye.Both)
			{
				switch (stereoPacking)
				{
					case StereoPacking.LeftRight:
						width /= 2;
						break;
					case StereoPacking.TopBottom:
						height /= 2;
						break;
				}
			}
		}

		public static bool RequiresResolve(ITextureProducer texture)
		{
			return (texture.GetTextureAlphaPacking() != AlphaPacking.None ||
				texture.RequiresVerticalFlip() ||
				texture.GetTextureStereoPacking() != StereoPacking.None ||
				texture.GetTextureCount() > 1
			);
		}

		public static void DrawTexture(Rect destRect, Texture texture, ScaleMode scaleMode, AlphaPacking alphaPacking, Material material)
		{
			if (Event.current == null || Event.current.type == EventType.Repaint)
			{
				int sourceWidth = texture.width;
				int sourceHeight = texture.height;
				GetResolveTextureSize(alphaPacking, StereoPacking.Unknown, StereoEye.Both, ref sourceWidth, ref sourceHeight);

				float sourceRatio = (float)sourceWidth / (float)sourceHeight;
				Rect sourceRect = new Rect(0f, 0f, 1f, 1f);
				switch (scaleMode)
				{
					case ScaleMode.ScaleAndCrop:
						{
							float destRatio = destRect.width / destRect.height;
							if (destRatio > sourceRatio)
							{
								float adjust = sourceRatio / destRatio;
								sourceRect = new Rect(0f, (1f - adjust) * 0.5f, 1f, adjust);
							}
							else
							{
								float adjust = destRatio / sourceRatio;
								sourceRect = new Rect(0.5f - adjust * 0.5f, 0f, adjust, 1f);
							}
						}
						break;
					case ScaleMode.ScaleToFit:
						{
							float destRatio = destRect.width / destRect.height;
							if (destRatio > sourceRatio)
							{
								float adjust = sourceRatio / destRatio;
								destRect = new Rect(destRect.xMin + destRect.width * (1f - adjust) * 0.5f, destRect.yMin, adjust * destRect.width, destRect.height);
							}
							else
							{
								float adjust = destRatio / sourceRatio;
								destRect = new Rect(destRect.xMin, destRect.yMin + destRect.height * (1f - adjust) * 0.5f, destRect.width, adjust * destRect.height);
							}
						}
						break;
					case ScaleMode.StretchToFill:
						break;
				}

				GL.PushMatrix();
				if (RenderTexture.active == null)
				{
					//GL.LoadPixelMatrix();
					GL.LoadPixelMatrix(0f, Screen.width, Screen.height, 0f);
				}
				else
				{
					GL.LoadPixelMatrix(0f, RenderTexture.active.width, RenderTexture.active.height, 0f);
				}
				Graphics.DrawTexture(destRect, texture, sourceRect, 0, 0, 0, 0, GUI.color, material);
				GL.PopMatrix();
			}
		}
	}
}