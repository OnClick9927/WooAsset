Shader "AVProVideo/VR/InsideSphere Unlit Transparent (stereo+color+fog+alpha)"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "black" {}
		_ChromaTex("Chroma", 2D) = "white" {}
		_Color("Main Color", Color) = (1,1,1,1)

		[KeywordEnum(None, Top_Bottom, Left_Right, Custom_UV)] Stereo ("Stereo Mode", Float) = 0
		[KeywordEnum(None, Top_Bottom, Left_Right)] AlphaPack("Alpha Pack", Float) = 0
		[Toggle(STEREO_DEBUG)] _StereoDebug ("Stereo Debug Tinting", Float) = 0
		[KeywordEnum(None, EquiRect180)] Layout("Layout", Float) = 0
		[Toggle(HIGH_QUALITY)] _HighQuality ("High Quality", Float) = 0
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
		[Toggle(USE_YPCBCR)] _UseYpCbCr("Use YpCbCr", Float) = 0
		_EdgeFeather("Edge Feather", Range (0, 1)) = 0.02
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		ZWrite On
		//ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Front
		Lighting Off

		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "AVProVideo.cginc"
#if HIGH_QUALITY || APPLY_GAMMA
			#pragma target 3.0
#endif
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile_fog
			// TODO: replace use multi_compile_local instead (Unity 2019.1 feature)
			#pragma multi_compile MONOSCOPIC STEREO_TOP_BOTTOM STEREO_LEFT_RIGHT STEREO_CUSTOM_UV
			#pragma multi_compile ALPHAPACK_NONE ALPHAPACK_TOP_BOTTOM ALPHAPACK_LEFT_RIGHT
			#pragma multi_compile __ STEREO_DEBUG
			#pragma multi_compile __ HIGH_QUALITY
			#pragma multi_compile __ APPLY_GAMMA
			#pragma multi_compile __ USE_YPCBCR
			#pragma multi_compile __ LAYOUT_EQUIRECT180

			struct appdata
			{
				float4 vertex : POSITION; // vertex position
#if HIGH_QUALITY
				float3 normal : NORMAL;
#else
				float2 uv : TEXCOORD0; // texture coordinate
	#if STEREO_CUSTOM_UV
				float2 uv2 : TEXCOORD1;	// Custom uv set for right eye (left eye is in TEXCOORD0)
	#endif
#endif

#ifdef UNITY_STEREO_INSTANCING_ENABLED
				UNITY_VERTEX_INPUT_INSTANCE_ID
#endif
			};

			struct v2f
			{
				float4 vertex : SV_POSITION; // clip space position
#if HIGH_QUALITY
				float3 normal : TEXCOORD0;
				
	#if STEREO_TOP_BOTTOM || STEREO_LEFT_RIGHT
				float4 scaleOffset : TEXCOORD1; // texture coordinate
				UNITY_FOG_COORDS(2)
	#else
				UNITY_FOG_COORDS(1)
	#endif
#else
				float4 uv : TEXCOORD0; // texture coordinate
				UNITY_FOG_COORDS(1)
#endif

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
			uniform float4 _MainTex_TexelSize;
			uniform fixed4 _Color;
			uniform float _EdgeFeather;

			v2f vert (appdata v)
			{
				v2f o;

#ifdef UNITY_STEREO_INSTANCING_ENABLED
				UNITY_SETUP_INSTANCE_ID(v);						// calculates and sets the built-n unity_StereoEyeIndex and unity_InstanceID Unity shader variables to the correct values based on which eye the GPU is currently rendering
				UNITY_INITIALIZE_OUTPUT(v2f, o);				// initializes all v2f values to 0
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);		// tells the GPU which eye in the texture array it should render to
#endif
								
				o.vertex = XFormObjectToClip(v.vertex);
#if !HIGH_QUALITY
				o.uv.zw = 0.0;
				o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
				#if LAYOUT_EQUIRECT180
				o.uv.x = ((o.uv.x - 0.5) * 2.0) + 0.5;
				// Set value for clipping if UV area is behind viewer
				o.uv.z = -1.0;
				if (v.uv.x > 0.25 && v.uv.x < 0.75)
				{
					o.uv.z = 1.0;
				}
				#endif
				o.uv.xy = float2(1.0-o.uv.x, o.uv.y);
#endif

#if STEREO_TOP_BOTTOM || STEREO_LEFT_RIGHT
				float4 scaleOffset = GetStereoScaleOffset(IsStereoEyeLeft(), _MainTex_ST.y < 0.0);

				#if !HIGH_QUALITY
				o.uv.xy *= scaleOffset.xy;
				o.uv.xy += scaleOffset.zw;
				#else
				o.scaleOffset = scaleOffset;
				#endif
#elif STEREO_CUSTOM_UV && !HIGH_QUALITY
				if (!IsStereoEyeLeft())
				{
					o.uv.xy = TRANSFORM_TEX(v.uv2, _MainTex);
					o.uv.xy = float2(1.0 - o.uv.x, o.uv.y);
				}
#endif
				
#if !HIGH_QUALITY
	#if ALPHAPACK_TOP_BOTTOM || ALPHAPACK_LEFT_RIGHT
				o.uv = OffsetAlphaPackingUV(_MainTex_TexelSize.xy, o.uv.xy, _MainTex_ST.y > 0.0);
	#endif
#endif

#if HIGH_QUALITY
				o.normal = v.normal;
#endif

				#if STEREO_DEBUG
				o.tint = GetStereoDebugTint(IsStereoEyeLeft());
				#endif

				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 uv = 0;
#if HIGH_QUALITY
				float3 n = normalize(i.normal);
				#if LAYOUT_EQUIRECT180
				clip(-n.z);	// Clip pixels on the back of the sphere
				#endif

				float M_1_PI = 1.0 / 3.1415926535897932384626433832795;
				float M_1_2PI = 1.0 / 6.283185307179586476925286766559;
				uv.x = 0.5 - atan2(n.z, n.x) * M_1_2PI;
				uv.y = 0.5 - asin(-n.y) * M_1_PI;
				uv.x += 0.75;
				uv.x = fmod(uv.x, 1.0);
				//uv.x = uv.x % 1.0;
				uv.xy = TRANSFORM_TEX(uv, _MainTex);
				#if LAYOUT_EQUIRECT180
				uv.x = ((uv.x - 0.5) * 2.0) + 0.5;
				#endif
				#if STEREO_TOP_BOTTOM | STEREO_LEFT_RIGHT
				uv.xy *= i.scaleOffset.xy;
				uv.xy += i.scaleOffset.zw;
				#endif
				#if ALPHAPACK_TOP_BOTTOM | ALPHAPACK_LEFT_RIGHT
				uv = OffsetAlphaPackingUV(_MainTex_TexelSize.xy, uv.xy, _MainTex_ST.y < 0.0);
				#endif
#else
				uv = i.uv;
				#if LAYOUT_EQUIRECT180
				clip(i.uv.z);	// Clip pixels on the back of the sphere
				#endif
#endif

				fixed4 col;
#if USE_YPCBCR
				col = SampleYpCbCr(_MainTex, _ChromaTex, uv.xy, _YpCbCrTransform);
#else
				col = SampleRGBA(_MainTex, uv.xy);
#endif

#if ALPHAPACK_TOP_BOTTOM | ALPHAPACK_LEFT_RIGHT
				col.a = SamplePackedAlpha(_MainTex, uv.zw);
#endif

#if STEREO_DEBUG
				col *= i.tint;
#endif

				col *= _Color;

				UNITY_APPLY_FOG(i.fogCoord, col);

#if LAYOUT_EQUIRECT180
				// Apply edge feathering based on UV mapping - this is useful if you're using a hemisphere mesh for 180 degree video and want to have soft edges
				if (_EdgeFeather > 0.0)
				{
					float4 featherDirection = float4(0.0, 0.0, 1.0, 1.0);
					
#if STEREO_TOP_BOTTOM 
					if (uv.y > 0.5)
					{
						featherDirection.y = 0.5;
					}
					else
					{
						featherDirection.w = 0.5;
					}
#endif

#if STEREO_LEFT_RIGHT
					if (uv.x > 0.5)
					{
						featherDirection.x = 0.5;
					}
					else
					{
						featherDirection.z = 0.5;
					}
#endif


#if ALPHAPACK_TOP_BOTTOM
					featherDirection.w *= 0.5;
#endif

#if ALPHAPACK_LEFT_RIGHT
					featherDirection.z *= 0.5;
#endif

					float d = min(uv.x - featherDirection.x, min((uv.y - featherDirection.y), min(featherDirection.z - uv.x, featherDirection.w - uv.y)));
					float a = smoothstep(0.0, _EdgeFeather, d);
					col.a *= a;
				}
#endif

				return col;

			}
			ENDCG
		}
	}
}