Shader "AVProVideo/Unlit/Opaque (texture+color+fog+stereo support)"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "black" {}
		_ChromaTex ("Chroma", 2D) = "gray" {}
		_Color("Main Color", Color) = (1,1,1,1)

		[KeywordEnum(None, Top_Bottom, Left_Right, Custom_UV)] Stereo("Stereo Mode", Float) = 0
		[Toggle(STEREO_DEBUG)] _StereoDebug("Stereo Debug Tinting", Float) = 0
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
		[Toggle(USE_YPCBCR)] _UseYpCbCr("Use YpCbCr", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "IgnoreProjector"="False" "Queue"="Geometry" }
		LOD 100
		Lighting Off
		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			// TODO: replace use multi_compile_local instead (Unity 2019.1 feature)
			#pragma multi_compile MONOSCOPIC STEREO_TOP_BOTTOM STEREO_LEFT_RIGHT STEREO_CUSTOM_UV
			#pragma multi_compile __ STEREO_DEBUG
			#pragma multi_compile __ APPLY_GAMMA
			#pragma multi_compile __ USE_YPCBCR

			#include "UnityCG.cginc"
			#include "AVProVideo.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
#if STEREO_CUSTOM_UV
				float2 uv2 : TEXCOORD1;	// Custom uv set for right eye (left eye is in TEXCOORD0)
#endif
#ifdef UNITY_STEREO_INSTANCING_ENABLED
				UNITY_VERTEX_INPUT_INSTANCE_ID
#endif
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
#if STEREO_DEBUG
				float4 tint : COLOR;
#endif
#ifdef UNITY_STEREO_INSTANCING_ENABLED
				UNITY_VERTEX_OUTPUT_STEREO
#endif
			};

			uniform sampler2D _MainTex;
#if USE_YPCBCR
			uniform sampler2D _ChromaTex;
			uniform float4x4 _YpCbCrTransform;
#endif
			uniform float4 _MainTex_ST;
			uniform fixed4 _Color;

			v2f vert (appdata v)
			{
				v2f o;

#ifdef UNITY_STEREO_INSTANCING_ENABLED
				UNITY_SETUP_INSTANCE_ID(v);						// calculates and sets the built-n unity_StereoEyeIndex and unity_InstanceID Unity shader variables to the correct values based on which eye the GPU is currently rendering
				UNITY_INITIALIZE_OUTPUT(v2f, o);				// initializes all v2f values to 0
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);		// tells the GPU which eye in the texture array it should render to
#endif
				o.vertex = XFormObjectToClip(v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);

#if STEREO_TOP_BOTTOM | STEREO_LEFT_RIGHT
				float4 scaleOffset = GetStereoScaleOffset(IsStereoEyeLeft(), _MainTex_ST.y < 0.0);
				o.uv.xy *= scaleOffset.xy;
				o.uv.xy += scaleOffset.zw;
#elif STEREO_CUSTOM_UV
				if (!IsStereoEyeLeft())
				{
					o.uv.xy = TRANSFORM_TEX(v.uv2, _MainTex);
				}
#endif

#if STEREO_DEBUG
				o.tint = GetStereoDebugTint(IsStereoEyeLeft());
#endif

				UNITY_TRANSFER_FOG(o, o.vertex);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col;
#if USE_YPCBCR
				col = SampleYpCbCr(_MainTex, _ChromaTex, i.uv.xy, _YpCbCrTransform);
#else
				col = SampleRGBA(_MainTex, i.uv.xy);
#endif
				col *= _Color;
#if STEREO_DEBUG
				col *= i.tint;
#endif				

				UNITY_APPLY_FOG(i.fogCoord, col);

				return col;
			}
			ENDCG
		}
	}
}
