Shader "AVProVideo/VR/InsideSphere Unlit Transparent (stereo+color+alpha) - Android OES ONLY" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "black" {}
		_ChromaTex("Chroma", 2D) = "white" {}			// For fallback shader
		_Color("Color", Color) = (0.0, 1.0, 0.0, 1.0)
		[KeywordEnum(None, Top_Bottom, Left_Right, Custom_UV)] Stereo("Stereo Mode", Float) = 0
		[KeywordEnum(None, Top_Bottom, Left_Right)] AlphaPack("Alpha Pack", Float) = 0
		[KeywordEnum(None, Left, Right)] ForceEye("Force Eye Mode", Float) = 0
		[KeywordEnum(None, EquiRect180)] Layout("Layout", Float) = 0
		[Toggle(STEREO_DEBUG)] _StereoDebug("Stereo Debug Tinting", Float) = 0
		[Toggle(HIGH_QUALITY)] _HighQuality("High Quality", Float) = 0
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
		_EdgeFeather("Edge Feather", Range (0, 1)) = 0.02
	}
	SubShader 
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		Pass
		{ 
			ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Front
			Lighting Off

			GLSLPROGRAM

			#pragma only_renderers gles gles3
			#pragma multi_compile MONOSCOPIC STEREO_TOP_BOTTOM STEREO_LEFT_RIGHT STEREO_CUSTOM_UV
			#pragma multi_compile ALPHAPACK_NONE ALPHAPACK_TOP_BOTTOM ALPHAPACK_LEFT_RIGHT
			#pragma multi_compile FORCEEYE_NONE FORCEEYE_LEFT FORCEEYE_RIGHT
			#pragma multi_compile __ LAYOUT_EQUIRECT180
			#pragma multi_compile __ STEREO_DEBUG
			#pragma multi_compile __ HIGH_QUALITY
			#pragma multi_compile __ APPLY_GAMMA
			#pragma multi_compile __ USING_DEFAULT_TEXTURE

			#extension GL_OES_EGL_image_external : require
			#extension GL_OES_EGL_image_external_essl3 : enable
			precision mediump float;

#include "UnityCG.glslinc"
#define SHADERLAB_GLSL
#include "AVProVideo.cginc"

			#ifdef VERTEX

#if defined(HIGH_QUALITY)
		varying vec3 texNormal;
	#if defined(STEREO_TOP_BOTTOM) || defined(STEREO_LEFT_RIGHT)
		varying vec4 texScaleOffset;
	#endif
#else
		varying vec3 texVal;
	#if defined(ALPHAPACK_TOP_BOTTOM) || defined(ALPHAPACK_LEFT_RIGHT)
		varying vec2 alphaPackOffset;
	#endif
		uniform vec4 _MainTex_ST;
		uniform vec4 _MainTex_TexelSize;
		uniform mat4 _TextureMatrix;
#endif
#if defined(STEREO_DEBUG)
		varying vec4 tint;
#endif


			/// @fix: explicit TRANSFORM_TEX(); Unity's preprocessor chokes when attempting to use the TRANSFORM_TEX() macro in UnityCG.glslinc
			/// 	(as of Unity 4.5.0f6; issue dates back to 2011 or earlier: http://forum.unity3d.com/threads/glsl-transform_tex-and-tiling.93756/)
			vec2 transformTex(vec2 texCoord, vec4 texST) 
			{
				return (texCoord * texST.xy + texST.zw);
			}

			void main()
			{
				gl_Position = XFormObjectToClip(gl_Vertex);

#if defined(HIGH_QUALITY)
				texNormal = normalize(gl_Normal.xyz);
	#if defined(STEREO_TOP_BOTTOM) || defined(STEREO_LEFT_RIGHT)
				bool isLeftEye = IsStereoEyeLeft();
				texScaleOffset = GetStereoScaleOffset(isLeftEye, false);
	#endif
#else
				texVal.xy = gl_MultiTexCoord0.xy;
				texVal.xy = transformTex(gl_MultiTexCoord0.xy, _MainTex_ST);
				texVal.x = 1.0 - texVal.x;
	#if defined(LAYOUT_EQUIRECT180)
				texVal.x = ((texVal.x - 0.5) * 2.0) + 0.5;

				// Set value for clipping if UV area is behind viewer
				texVal.z = (gl_MultiTexCoord0.x > 0.25 && gl_MultiTexCoord0.x < 0.75) ? 1.0 : -1.0;
	#else
				texVal.z = 0.0;
	#endif

				// Apply texture transformation matrix - adjusts for offset/cropping (when the decoder decodes in blocks that overrun the video frame size, it pads)
				texVal.xy = (_TextureMatrix * vec4(texVal.x, texVal.y, 0.0, 1.0)).xy;

	#if defined(STEREO_TOP_BOTTOM) || defined(STEREO_LEFT_RIGHT)
				bool isLeftEye = IsStereoEyeLeft();
				vec4 scaleOffset = GetStereoScaleOffset(isLeftEye, false);

				texVal.xy *= scaleOffset.xy;
				texVal.xy += scaleOffset.zw;
	#elif defined(STEREO_CUSTOM_UV)
				if (!IsStereoEyeLeft())
				{
					texVal.xy= transformTex(gl_MultiTexCoord1.xy, _MainTex_ST);
					texVal.xy = vec2(1.0, 1.0) - texVal.xy;
				}
	#endif

	#if defined(ALPHAPACK_TOP_BOTTOM) || defined(ALPHAPACK_LEFT_RIGHT)
				vec4 alphaOffset = OffsetAlphaPackingUV(_MainTex_TexelSize.xy, texVal.xy, _MainTex_ST.y < 0.0);
		#if defined(ALPHAPACK_TOP_BOTTOM)
				alphaOffset.yw = alphaOffset.wy;
		#endif

				texVal.xy = alphaOffset.xy;
				alphaPackOffset = alphaOffset.zw;
	#endif
#endif

#if defined(STEREO_DEBUG)
				tint = GetStereoDebugTint(IsStereoEyeLeft());
#endif
			}
			#endif

			#ifdef FRAGMENT

#if defined(HIGH_QUALITY)
	#if defined (GL_FRAGMENT_PRECISION_HIGH)
			precision highp float;
	#endif
			varying vec3 texNormal;
	#if defined(STEREO_TOP_BOTTOM) || defined(STEREO_LEFT_RIGHT)
			varying vec4 texScaleOffset;
	#endif
			uniform mat4 _TextureMatrix;
#else
			varying vec3 texVal;
	#if defined(ALPHAPACK_TOP_BOTTOM) || defined(ALPHAPACK_LEFT_RIGHT)
			varying vec2 alphaPackOffset;
	#endif
#endif
			uniform float _EdgeFeather;
#if defined(STEREO_DEBUG)
			varying vec4 tint;
#endif

#if defined(HIGH_QUALITY)
			vec2 NormalToEquiRect(vec3 n)
			{
				const float M_1_PI = 0.31830988618379067153776752674503;  // 1.0/PI
				const float M_1_2PI = 0.15915494309189533576888376337251; // 2.0/PI
				vec2 uv;
				uv.x = 0.5 - atan(n.z, n.x) * M_1_2PI;
				uv.y = 0.5 - asin(-n.y) * M_1_PI;
				return uv;
			}

			/// @fix: explicit TRANSFORM_TEX(); Unity's preprocessor chokes when attempting to use the TRANSFORM_TEX() macro in UnityCG.glslinc
			/// 	(as of Unity 4.5.0f6; issue dates back to 2011 or earlier: http://forum.unity3d.com/threads/glsl-transform_tex-and-tiling.93756/)
			vec2 transformTex(vec2 texCoord, vec4 texST) 
			{
				return (texCoord * texST.xy + texST.zw);
			}

			uniform vec4 _MainTex_ST;
			uniform vec4 _MainTex_TexelSize;
#endif

			uniform vec4 _Color;
#if defined(USING_DEFAULT_TEXTURE)
			uniform sampler2D _MainTex;
#else
			uniform samplerExternalOES _MainTex;
#endif

			void main()
			{
				vec4 uv = vec4(0.0, 0.0, 0.0, 0.0);

#if defined(HIGH_QUALITY)
				vec3 n = normalize(texNormal);
	#if defined(LAYOUT_EQUIRECT180)
				if( n.z > 0.0001 )
				{
					// Clip pixels on the back of the sphere
					discard;
				}
	#endif

				uv.xy = NormalToEquiRect(n);
				uv.x += 0.75;
				uv.x = mod(uv.x, 1.0);
				uv.xy = transformTex(uv.xy, _MainTex_ST);

	#if defined(LAYOUT_EQUIRECT180)
				uv.x = ((uv.x - 0.5) * 2.0) + 0.5;
	#endif

				// Apply texture transformation matrix - adjusts for offset/cropping (when the decoder decodes in blocks that overrun the video frame size, it pads)
				uv.xy = (_TextureMatrix * vec4(uv.x, uv.y, 0.0, 1.0)).xy;

	#if defined(STEREO_TOP_BOTTOM) || defined(STEREO_LEFT_RIGHT)
				uv.xy *= texScaleOffset.xy;
				uv.xy += texScaleOffset.zw;
	#endif

	#if defined(ALPHAPACK_TOP_BOTTOM) || defined(ALPHAPACK_LEFT_RIGHT)
				uv = OffsetAlphaPackingUV(_MainTex_TexelSize.xy, uv.xy, _MainTex_ST.y < 0.0);
		#if defined(ALPHAPACK_TOP_BOTTOM)
				uv.yw = uv.wy;
		#endif
	#endif
#else
				uv.xy = texVal.xy;
	#if defined(ALPHAPACK_TOP_BOTTOM) || defined(ALPHAPACK_LEFT_RIGHT)
				uv.zw = alphaPackOffset;
	#endif
	#if defined(LAYOUT_EQUIRECT180)
				if( texVal.z < -0.0001 )
				{
					// Clip pixels on the back of the sphere
					discard;
				}
	#endif
#endif

				vec4 col = vec4(1.0, 1.0, 0.0, 1.0);
#if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
	#if __VERSION__ < 300
				col = texture2D(_MainTex, uv.xy);
	#else
				col = texture(_MainTex, uv.xy);
	#endif
#endif
				col *= _Color;

#if defined(APPLY_GAMMA)
				col.rgb = GammaToLinear(col.rgb);
#endif

#if defined(ALPHAPACK_TOP_BOTTOM) || defined(ALPHAPACK_LEFT_RIGHT)
	#if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
		#if __VERSION__ < 300
				vec3 rgb = texture2D(_MainTex, uv.zw).rgb;
		#else
				vec3 rgb = texture(_MainTex, uv.zw).rgb;
		#endif
				col.a = (rgb.r + rgb.g + rgb.b) / 3.0;
	#else
				col.a = 1.0;
	#endif
#endif

#if defined(STEREO_DEBUG)
				col *= tint;
#endif

#if defined(LAYOUT_EQUIRECT180)
				// Apply edge feathering based on UV mapping - this is useful if you're using a hemisphere mesh for 180 degree video and want to have soft edges
				if (_EdgeFeather > 0.0)
				{
					vec4 featherDirection = vec4(0.0, 0.0, 1.0, 1.0);
					
#if defined(STEREO_TOP_BOTTOM)
					if (uv.y > 0.5)
					{
						featherDirection.y = 0.5;
					}
					else
					{
						featherDirection.w = 0.5;
					}
#endif

#if defined(STEREO_LEFT_RIGHT)
					if (uv.x > 0.5)
					{
						featherDirection.x = 0.5;
					}
					else
					{
						featherDirection.z = 0.5;
					}
#endif

#if defined(ALPHAPACK_TOP_BOTTOM)
					featherDirection.w *= 0.5;
#endif

#if defined(ALPHAPACK_LEFT_RIGHT)
					featherDirection.z *= 0.5;
#endif

					float d = min(uv.x - featherDirection.x, min((uv.y - featherDirection.y), min(featherDirection.z - uv.x, featherDirection.w - uv.y)));
					float a = smoothstep(0.0, _EdgeFeather, d);
					col.a *= a;
				}
#endif

				gl_FragColor = col;
			}
			#endif
				
			ENDGLSL
		}
	}
	
	Fallback "AVProVideo/VR/InsideSphere Unlit Transparent (stereo+color+fog+alpha)"
}