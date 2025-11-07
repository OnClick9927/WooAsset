Shader "AVProVideo/Internal/UI/AndroidOES"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _ChromaTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		// TODO: replace use multi_compile_local instead (Unity 2019.1 feature)
		[KeywordEnum(None, Top_Bottom, Left_Right)] Stereo("Stereo Mode", Float) = 0
		[Toggle(STEREO_DEBUG)] _StereoDebug("Stereo Debug Tinting", Float) = 0
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Stencil
			{
				Ref[_Stencil]
				Comp[_StencilComp]
				Pass[_StencilOp]
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
			}

			Cull Off
			Lighting Off
			ZWrite Off
			ZTest[unity_GUIZTestMode]
			Fog{ Mode Off }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask[_ColorMask]

			Pass
			{
			GLSLPROGRAM
			#pragma only_renderers gles gles3

			#pragma multi_compile MONOSCOPIC STEREO_TOP_BOTTOM STEREO_LEFT_RIGHT STEREO_CUSTOM_UV
			#pragma multi_compile __ STEREO_DEBUG
			#pragma multi_compile __ APPLY_GAMMA
			#pragma multi_compile __ USING_DEFAULT_TEXTURE

			#extension GL_OES_EGL_image_external : require
			#extension GL_OES_EGL_image_external_essl3 : enable

			precision mediump float;

#ifdef VERTEX

#include "UnityCG.glslinc"
#define SHADERLAB_GLSL
#include "../AVProVideo.cginc"
	varying vec2 texVal;
	uniform mat4 _TextureMatrix;

#if defined(STEREO_DEBUG)
	varying vec4 tint;
#endif

	void main()
	{
		gl_Position = XFormObjectToClip(gl_Vertex);
		texVal = gl_MultiTexCoord0.xy;

		// Apply texture transformation matrix - adjusts for offset/cropping (when the decoder decodes in blocks that overrun the video frame size, it pads)
		texVal.xy = (_TextureMatrix * vec4(texVal.x, texVal.y, 0.0, 1.0)).xy;

#if defined(STEREO_TOP_BOTTOM) | defined(STEREO_LEFT_RIGHT)
		bool isLeftEye = IsStereoEyeLeft();

		vec4 scaleOffset = GetStereoScaleOffset(isLeftEye, false);

		texVal.xy *= scaleOffset.xy;
		texVal.xy += scaleOffset.zw;
#elif defined (STEREO_CUSTOM_UV)
		if (!IsStereoEyeLeft())
		{
			texVal = gl_MultiTexCoord1.xy;
			texVal = vec2(1.0, 1.0) - texVal;
		}
#endif

#if defined(STEREO_DEBUG)
		tint = GetStereoDebugTint(IsStereoEyeLeft());
#endif

	}
#endif

#ifdef FRAGMENT
	varying vec2 texVal;

#if defined(STEREO_DEBUG)
	varying vec4 tint;
#endif

#if defined(APPLY_GAMMA)
	vec3 GammaToLinear(vec3 col)
	{
		return pow(col, vec3(2.2, 2.2, 2.2));
	}
#endif

	uniform vec4 _Color;
#if defined(USING_DEFAULT_TEXTURE)
	uniform sampler2D _MainTex;
#else
	uniform samplerExternalOES _MainTex;
#endif

	void main()
	{
		vec4 col = vec4(1.0, 1.0, 0.0, 1.0);
#if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)

	#if __VERSION__ < 300
		col = texture2D(_MainTex, texVal.xy);
	#else
		col = texture(_MainTex, texVal.xy);
	#endif

	#if defined(APPLY_GAMMA)
		col.rgb = GammaToLinear(col.rgb);
	#endif

		col *= _Color;
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

	Fallback "AVProVideo/Internal/UI/Stereo"
}
