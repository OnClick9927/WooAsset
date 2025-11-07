Shader "AVProVideo/VR/InsideSphere Unlit (stereo+fog) Stereo UV"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "black" {}

		[Toggle(STEREO_DEBUG)] _StereoDebug ("Stereo Debug Tinting", Float) = 0
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "IgnoreProjector" = "True" "Queue" = "Background" }
		ZWrite On
		//ZTest Always
		Cull Front
		Lighting Off

		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "AVProVideo.cginc"
			//#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			//#define STEREO_DEBUG 1
			//#define HIGH_QUALITY 1

			#pragma multi_compile_fog
			// TODO: replace use multi_compile_local instead (Unity 2019.1 feature)
			#pragma multi_compile __ STEREO_DEBUG
			#pragma multi_compile __ APPLY_GAMMA

			struct appdata
			{
				float4 vertex : POSITION;	// vertex position
				float2 uv : TEXCOORD0;		// texture coordinate 1
				float2 uv2 : TEXCOORD1;		// texture coordinate 2
			};

			struct v2f
			{
				float4 vertex : SV_POSITION; // clip space position
				float2 uv : TEXCOORD0; // texture coordinate

				UNITY_FOG_COORDS(1)

#if STEREO_DEBUG
				float4 tint : COLOR;
#endif
			};

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = XFormObjectToClip(v.vertex);

				if (IsStereoEyeLeft())
				{
					o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
					o.uv.xy = float2(1.0 - o.uv.x, o.uv.y);
				}
				else
				{
					o.uv.xy = TRANSFORM_TEX(v.uv2, _MainTex);
					o.uv.xy = float2(1.0 - o.uv.x, o.uv.y);
				}

				#if STEREO_DEBUG
				o.tint = GetStereoDebugTint(IsStereoEyeLeft());
				#endif

				UNITY_TRANSFER_FOG(o, o.vertex);

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float2 uv;
				uv = i.uv;

				fixed4 col = tex2D(_MainTex, uv);

#if APPLY_GAMMA
				col.rgb = GammaToLinear(col.rgb);
#endif

#if STEREO_DEBUG
				col *= i.tint;
#endif

				UNITY_APPLY_FOG(i.fogCoord, col);

				return fixed4(col.rgb, 1.0);
			}
			ENDCG
		}
	}
}