Shader "AVProVideo/Internal/UI/Transparent Packed"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _ChromaTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15

		_VertScale("Vertical Scale", Range(-1, 1)) = 1.0

		[KeywordEnum(None, Top_Bottom, Left_Right)] AlphaPack("Alpha Pack", Float) = 0
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
		[Toggle(USE_YPCBCR)] _UseYpCbCr("Use YpCbCr", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// TODO: replace use multi_compile_local instead (Unity 2019.1 feature)
			#pragma multi_compile ALPHAPACK_NONE ALPHAPACK_TOP_BOTTOM ALPHAPACK_LEFT_RIGHT
			#pragma multi_compile __ APPLY_GAMMA
			#pragma multi_compile __ USE_YPCBCR

#if APPLY_GAMMA
			//#pragma target 3.0
#endif
			#include "UnityCG.cginc"
            // TODO: once we drop support for Unity 4.x then we can include this
			//#include "UnityUI.cginc"    
			#include "../AVProVideo.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half4 uv  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
			};
			
			uniform fixed4 _Color;
			uniform sampler2D _MainTex;
#if USE_YPCBCR
			uniform sampler2D _ChromaTex;
			uniform float4x4 _YpCbCrTransform;
#endif
			uniform float4 _MainTex_TexelSize;
			uniform float _VertScale;
			uniform float4 _ClipRect;

			inline float UnityGet2DClipping (in float2 position, in float4 clipRect)
			{
			 	float2 inside = step(clipRect.xy, position.xy) * step(position.xy, clipRect.zw);
			 	return inside.x * inside.y;
			}

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.worldPosition = IN.vertex;

				OUT.vertex = XFormObjectToClip(IN.vertex);

#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
#endif

				OUT.uv.xy = IN.texcoord.xy;

				// Horrible hack to undo the scale transform to fit into our UV packing layout logic...
				if (_VertScale < 0.0)
				{
					OUT.uv.y = 1.0 - OUT.uv.y;
				}

				OUT.uv = OffsetAlphaPackingUV(_MainTex_TexelSize.xy, OUT.uv.xy, _VertScale < 0.0);

				OUT.color = IN.color * _Color;
				return OUT;
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
				col *= i.color;
				
				col.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
				clip(col.a - 0.001);

				return col;
			}

		ENDCG
		}
	}
}
