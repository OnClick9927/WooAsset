Shader "AVProVideo/Internal/IMGUI/Texture Transparent"
{
	Properties
	{
		_MainTex("Texture", any) = "" {}
		_ChromaTex("Chroma", any) = "" {}
		_VertScale("Vertical Scale", Range(-1, 1)) = 1.0
		[KeywordEnum(None, Top_Bottom, Left_Right)] AlphaPack("Alpha Pack", Float) = 0
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
		[Toggle(USE_YPCBCR)] _UseYpCbCr("Use YpCbCr", Float) = 0
	}

	SubShader
	{
		Tags { "ForceSupported" = "True" "RenderType" = "Overlay" }

		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// TODO: replace use multi_compile_local instead (Unity 2019.1 feature)
			#pragma multi_compile ALPHAPACK_NONE ALPHAPACK_TOP_BOTTOM ALPHAPACK_LEFT_RIGHT
			#pragma multi_compile __ APPLY_GAMMA
			#pragma multi_compile __ USE_YPCBCR

			#include "UnityCG.cginc"
			#include "../AVProVideo.cginc"

			struct appdata_t 
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f 
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float4 uv : TEXCOORD0;
			};

			uniform sampler2D _MainTex;
#if USE_YPCBCR
			uniform sampler2D _ChromaTex;
			uniform float4x4 _YpCbCrTransform;
#endif
			uniform float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;
			uniform float _VertScale;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = XFormObjectToClip(v.vertex);
				o.color = v.color;
				o.uv = OffsetAlphaPackingUV(_MainTex_TexelSize.xy, TRANSFORM_TEX(v.texcoord, _MainTex), _VertScale < 0.0);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col;
#if USE_YPCBCR
				col = SampleYpCbCr(_MainTex, _ChromaTex, i.uv.xy, _YpCbCrTransform);
#else
				col = SampleRGBA(_MainTex, i.uv.xy);
#endif

#if ALPHAPACK_TOP_BOTTOM | ALPHAPACK_LEFT_RIGHT
				col.a = SamplePackedAlpha(_MainTex, i.uv.zw);
#endif
				return col * i.color;
			}
			ENDCG
		}
	}

	Fallback off
}