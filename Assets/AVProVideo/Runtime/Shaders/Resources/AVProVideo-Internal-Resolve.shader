Shader "AVProVideo/Internal/Resolve"
{
	Properties
	{
		_MainTex("Texture", any) = "" {}
		_ChromaTex("Chroma", any) = "" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_VertScale("Vertical Scale", Range(-1, 1)) = 1.0

		[Toggle(USE_HSBC)] _UseHSBC("Use HSBC", Float) = 0
		_Hue("Hue", Range(0, 1.0)) = 0
		_Saturation("Saturation", Range(0, 1.0)) = 0.5
		_Brightness("Brightness", Range(0, 1.0)) = 0.5
		_Contrast("Contrast", Range(0, 1.0)) = 0.5
		_InvGamma("InvGamma", Range(0.0001, 10000.0)) = 1.0

		[KeywordEnum(None, Top_Bottom, Left_Right)] Stereo("Stereo Mode", Float) = 0
		[KeywordEnum(None, Left, Right)] ForceEye ("Force Eye Mode", Float) = 0		
		[KeywordEnum(None, Top_Bottom, Left_Right)] AlphaPack("Alpha Pack", Float) = 0
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
		[Toggle(USE_YPCBCR)] _UseYpCbCr("Use YpCbCr", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"IgnoreProjector"="True" 
			"PreviewType"="Plane"
		}

		Lighting Off
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			Name "RESOLVE"

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// TODO: replace use multi_compile_local instead (Unity 2019.1 feature)
			#pragma multi_compile MONOSCOPIC STEREO_TOP_BOTTOM STEREO_LEFT_RIGHT
			#pragma multi_compile ALPHAPACK_NONE ALPHAPACK_TOP_BOTTOM ALPHAPACK_LEFT_RIGHT
			#pragma multi_compile __ APPLY_GAMMA
			#pragma multi_compile __ USE_YPCBCR
			#pragma multi_compile __ USE_HSBC

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
#if USE_HSBC
			uniform	fixed _Hue, _Saturation, _Brightness, _Contrast, _InvGamma;
#endif
			uniform fixed4 _Color;
			uniform float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;
			uniform float _VertScale;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = XFormObjectToClip(v.vertex);
				o.color = v.color * _Color;
				o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv.wz = 0.0;

#if STEREO_TOP_BOTTOM || STEREO_LEFT_RIGHT
				float4 scaleOffset = GetStereoScaleOffset(IsStereoEyeLeft(), _MainTex_ST.y < 0.0);

				o.uv.xy *= scaleOffset.xy;
				o.uv.xy += scaleOffset.zw;
#endif

				// NOTE: this always runs because it's also used to flip vertically
				o.uv = OffsetAlphaPackingUV(_MainTex_TexelSize.xy, o.uv.xy, _VertScale < 0.0);

				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				half4 col;
#if USE_YPCBCR
				col = SampleYpCbCr(_MainTex, _ChromaTex, i.uv.xy, _YpCbCrTransform);
#else
				col = SampleRGBA(_MainTex, i.uv.xy);
#endif

#if ALPHAPACK_TOP_BOTTOM | ALPHAPACK_LEFT_RIGHT
				col.a = SamplePackedAlpha(_MainTex, i.uv.zw);
#endif

#if USE_HSBC
				col.rgb = ApplyHSBEffect(col.rgb, fixed4(_Hue, _Saturation, _Brightness, _Contrast));
				col.rgb = pow(col.rgb, _InvGamma);
#endif
				return col * i.color;
			}
			ENDCG
		}
	}

	Fallback off
}