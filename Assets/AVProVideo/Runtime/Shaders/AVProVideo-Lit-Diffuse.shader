Shader "AVProVideo/Lit/Diffuse (texture+color+fog+stereo support)" 
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_ChromaTex("Chroma", 2D) = "white" {}

		[KeywordEnum(None, Top_Bottom, Left_Right, Custom_UV)] Stereo("Stereo Mode", Float) = 0
		[Toggle(STEREO_DEBUG)] _StereoDebug("Stereo Debug Tinting", Float) = 0
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
		[Toggle(USE_YPCBCR)] _UseYpCbCr("Use YpCbCr", Float) = 0
	}

	SubShader
	{
		Tags { "Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Geometry" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert vertex:VertexFunction
		// TODO: replace use multi_compile_local instead (Unity 2019.1 feature)
		#pragma multi_compile MONOSCOPIC STEREO_TOP_BOTTOM STEREO_LEFT_RIGHT STEREO_CUSTOM_UV
		#pragma multi_compile __ APPLY_GAMMA
		#pragma multi_compile __ STEREO_DEBUG
		#pragma multi_compile __ USE_YPCBCR

		#include "AVProVideo.cginc"

		uniform sampler2D _MainTex;
#if USE_YPCBCR
		uniform sampler2D _ChromaTex;
		uniform float4x4 _YpCbCrTransform;
#endif
		uniform fixed4 _Color;

		struct Input 
		{
			float2 uv_MainTex;
			float4 color;
		};

		void VertexFunction(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

#if STEREO_TOP_BOTTOM | STEREO_LEFT_RIGHT
			float4 scaleOffset = GetStereoScaleOffset(IsStereoEyeLeft(), true);
			o.uv_MainTex = v.texcoord.xy *= scaleOffset.xy;
			o.uv_MainTex = v.texcoord.xy += scaleOffset.zw;
#elif STEREO_CUSTOM_UV
			o.uv_MainTex = v.texcoord.xy;
			if (!IsStereoEyeLeft())
			{
				o.uv_MainTex = v.texcoord1.xy;
			}
#endif
			o.color = _Color;
#if STEREO_DEBUG
			o.color *= GetStereoDebugTint(IsStereoEyeLeft());
#endif
		}

		void surf(Input IN, inout SurfaceOutput o) 
		{
				fixed4 c;
#if USE_YPCBCR
				c = SampleYpCbCr(_MainTex, _ChromaTex, IN.uv_MainTex, _YpCbCrTransform);
#else
				c = SampleRGBA(_MainTex, IN.uv_MainTex);
#endif
			c *= IN.color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}

	Fallback "Legacy Shaders/Transparent/VertexLit"
}