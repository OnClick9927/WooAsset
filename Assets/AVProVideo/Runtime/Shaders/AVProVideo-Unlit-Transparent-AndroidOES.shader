Shader "AVProVideo/Unlit/Transparent (texture+alpha support) - Android OES ONLY"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "black" {}
		_ChromaTex("Chroma", 2D) = "gray" {}			// For fallback shader
		_Color("Main Color", Color) = (1,1,1,1)			// For fallback shader

		[KeywordEnum(None, Top_Bottom, Left_Right)] AlphaPack("Alpha Pack", Float) = 0
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "IgnoreProjector"="True" "Queue"="Transparent" }
		LOD 100
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off
		Cull Off

		Pass
		{
			GLSLPROGRAM

			#pragma only_renderers gles gles3
			// TODO: replace use multi_compile_local instead (Unity 2019.1 feature)
			#pragma multi_compile ALPHAPACK_NONE ALPHAPACK_TOP_BOTTOM ALPHAPACK_LEFT_RIGHT
			#pragma multi_compile __ APPLY_GAMMA
			#pragma multi_compile __ USING_DEFAULT_TEXTURE

			#extension GL_OES_EGL_image_external : require
			#extension GL_OES_EGL_image_external_essl3 : enable
			precision mediump float;

			#ifdef VERTEX

			#include "UnityCG.glslinc"
			#define SHADERLAB_GLSL
			#include "AVProVideo.cginc"
		
#if defined(ALPHAPACK_TOP_BOTTOM) || defined(ALPHAPACK_LEFT_RIGHT)
			varying vec4 texVal;
#else
			varying vec2 texVal;
#endif

			uniform vec4 _MainTex_ST;
			uniform vec4 _MainTex_TexelSize;
			uniform mat4 _TextureMatrix;

			/// @fix: explicit TRANSFORM_TEX(); Unity's preprocessor chokes when attempting to use the TRANSFORM_TEX() macro in UnityCG.glslinc
			/// 	(as of Unity 4.5.0f6; issue dates back to 2011 or earlier: http://forum.unity3d.com/threads/glsl-transform_tex-and-tiling.93756/)
			vec2 transformTex(vec4 texCoord, vec4 texST) 
			{
				return (texCoord.xy * texST.xy + texST.zw);
			}

			void main()
			{
				gl_Position = XFormObjectToClip(gl_Vertex);
				texVal.xy = transformTex(gl_MultiTexCoord0, _MainTex_ST);

				// Apply texture transformation matrix - adjusts for offset/cropping (when the decoder decodes in blocks that overrun the video frame size, it pads)
				texVal.xy = (_TextureMatrix * vec4(texVal.x, texVal.y, 0.0, 1.0) ).xy;

	#if defined(ALPHAPACK_TOP_BOTTOM) || defined(ALPHAPACK_LEFT_RIGHT)
				texVal = OffsetAlphaPackingUV(_MainTex_TexelSize.xy, texVal.xy, _MainTex_ST.y < 0.0);
			#if defined(ALPHAPACK_TOP_BOTTOM)
				texVal.yw = texVal.wy;
			#endif
	#endif
			}
			#endif

			#ifdef FRAGMENT

#if defined(ALPHAPACK_TOP_BOTTOM) || defined(ALPHAPACK_LEFT_RIGHT)
			varying vec4 texVal;
#else
			varying vec2 texVal;
#endif

#if defined(USING_DEFAULT_TEXTURE)
			uniform sampler2D _MainTex;
#else
			uniform samplerExternalOES _MainTex;
#endif

#if defined(APPLY_GAMMA)
			vec3 GammaToLinear(vec3 col)
			{
				return col * (col * (col * 0.305306011 + 0.682171111) + 0.012522878);
			}
#endif

			void main()
			{

	#if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
				vec4 col = texture2D(_MainTex, texVal.xy);
	#else
				vec4 col = vec4(1.0, 1.0, 0.0, 1.0);
	#endif

	#if defined(APPLY_GAMMA)
				col.rgb = GammaToLinear(col.rgb);
	#endif

	#if defined(ALPHAPACK_TOP_BOTTOM) || defined(ALPHAPACK_LEFT_RIGHT)
		#if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
				vec3 rgb = texture2D(_MainTex, texVal.zw).rgb;
				col.a = (rgb.r + rgb.g + rgb.b) / 3.0;
		#else
				col.a = 1.0;
		#endif
	#endif

				gl_FragColor = col;
			}
			#endif

			ENDGLSL
		}
	}
	
	Fallback "AVProVideo/Unlit/Transparent (texture+color+fog+stereo+alpha)"
}