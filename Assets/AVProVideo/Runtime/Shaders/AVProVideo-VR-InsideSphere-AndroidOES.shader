Shader "AVProVideo/VR/InsideSphere Unlit (stereo+color) - Android OES ONLY" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "black" {}
		_ChromaTex("Chroma", 2D) = "white" {}			// For fallback shader
		_Color("Color", Color) = (0.0, 1.0, 0.0, 1.0)
		[KeywordEnum(None, Top_Bottom, Left_Right, Custom_UV)] Stereo("Stereo Mode", Float) = 0
		[KeywordEnum(None, Left, Right)] ForceEye("Force Eye Mode", Float) = 0
		[KeywordEnum(None, EquiRect180)] Layout("Layout", Float) = 0
		[Toggle(STEREO_DEBUG)] _StereoDebug("Stereo Debug Tinting", Float) = 0
		[Toggle(HIGH_QUALITY)] _HighQuality("High Quality", Float) = 0
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
	}
	SubShader 
	{
		Tags{ "Queue" = "Geometry" }
		Pass
		{ 
			Cull Front
			//ZTest Always
			ZWrite On
			Lighting Off

			GLSLPROGRAM

			#pragma only_renderers gles gles3
			// TODO: replace use multi_compile_local instead (Unity 2019.1 feature)
			#pragma multi_compile MONOSCOPIC STEREO_TOP_BOTTOM STEREO_LEFT_RIGHT STEREO_CUSTOM_UV
			#pragma multi_compile FORCEEYE_NONE FORCEEYE_LEFT FORCEEYE_RIGHT
			#pragma multi_compile __ LAYOUT_EQUIRECT180
			#pragma multi_compile __ STEREO_DEBUG
			#pragma multi_compile __ HIGH_QUALITY
			#pragma multi_compile __ APPLY_GAMMA
			#pragma multi_compile __ USING_DEFAULT_TEXTURE

			#extension GL_OES_EGL_image_external : require
			#extension GL_OES_EGL_image_external_essl3 : enable
			precision mediump float;

			#ifdef VERTEX

#include "UnityCG.glslinc"
#define SHADERLAB_GLSL
#include "AVProVideo.cginc"

#if defined(HIGH_QUALITY)
		varying vec3 texNormal;
	#if defined(STEREO_TOP_BOTTOM) || defined(STEREO_LEFT_RIGHT)
		varying vec4 texScaleOffset;
	#endif
#else
		varying vec3 texVal;
		uniform vec4 _MainTex_ST;
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
#endif
#if defined(STEREO_DEBUG)
			varying vec4 tint;
#endif

#if defined(APPLY_GAMMA)
			vec3 GammaToLinear(vec3 col)
			{
				return col * (col * (col * 0.305306011 + 0.682171111) + 0.012522878);
			}
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
#endif

			uniform vec4 _Color;
#if defined(USING_DEFAULT_TEXTURE)
			uniform sampler2D _MainTex;
#else
			uniform samplerExternalOES _MainTex;
#endif

			void main()
			{
				vec2 uv;

#if defined(HIGH_QUALITY)
				vec3 n = normalize(texNormal);
	#if defined(LAYOUT_EQUIRECT180)
				if( n.z > 0.0001 )
				{
					// Clip pixels on the back of the sphere
					discard;
				}
	#endif
				
				uv = NormalToEquiRect(n);
				uv.x += 0.75;
				uv.x = mod(uv.x, 1.0);
				uv = transformTex(uv, _MainTex_ST);

	#if defined(LAYOUT_EQUIRECT180)
				uv.x = ((uv.x - 0.5) * 2.0) + 0.5;
	#endif

				// Apply texture transformation matrix - adjusts for offset/cropping (when the decoder decodes in blocks that overrun the video frame size, it pads)
				uv.xy = (_TextureMatrix * vec4(uv.x, uv.y, 0.0, 1.0)).xy;

	#if defined(STEREO_TOP_BOTTOM) || defined(STEREO_LEFT_RIGHT)
				uv.xy *= texScaleOffset.xy;
				uv.xy += texScaleOffset.zw;
	#endif
#else
				uv = texVal.xy;
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
				col = texture2D(_MainTex, uv);
	#else
				col = texture(_MainTex, uv);
	#endif
#endif
				col *= _Color;

#if defined(APPLY_GAMMA)
				col.rgb = GammaToLinear(col.rgb);
#endif

#if defined(STEREO_DEBUG)
				col *= tint;
#endif

				gl_FragColor = col;
			}
			#endif

			ENDGLSL
		}
	}

	Fallback "AVProVideo/VR/InsideSphere Unlit (stereo+fog)"
}