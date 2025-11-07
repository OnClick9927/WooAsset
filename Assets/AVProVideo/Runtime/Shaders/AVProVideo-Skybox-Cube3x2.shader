Shader "AVProVideo/Skybox/3x2 Cube"
{
	Properties
	{
		_Tint ("Tint Color", Color) = (.5, .5, .5, .5)
		[Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
		_Rotation ("Rotation", Range(0, 360)) = 0
		[NoScaleOffset] _MainTex ("MainTex (HDR)", 2D) = "grey" { }
		[NoScaleOffset] _ChromaTex ("Chroma", 2D) = "grey" { }
		[KeywordEnum(None, Top_Bottom, Left_Right, Custom_UV)] Stereo ("Stereo Mode", Float) = 0
		[Toggle(STEREO_DEBUG)] _StereoDebug ("Stereo Debug Tinting", Float) = 0
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
		[Toggle(USE_YPCBCR)] _UseYpCbCr("Use YpCbCr", Float) = 0
	}

	SubShader
	{
		Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
		Cull Off ZWrite Off

		CGINCLUDE
		// TODO: replace use multi_compile_local instead (Unity 2019.1 feature)
		#pragma multi_compile MONOSCOPIC STEREO_TOP_BOTTOM STEREO_LEFT_RIGHT STEREO_CUSTOM_UV
		#pragma multi_compile FORCEEYE_NONE FORCEEYE_LEFT FORCEEYE_RIGHT
		#pragma multi_compile __ STEREO_DEBUG
		#pragma multi_compile __ APPLY_GAMMA
		#pragma multi_compile __ USE_YPCBCR
		#include "UnityCG.cginc"
		#include "AVProVideo.cginc"

		half4 _Tint;
		half _Exposure;
		float _Rotation;

		sampler2D _MainTex;
		float4 _MainTex_TexelSize;
		half4 _MainTex_HDR;
		float4 _MainTex_ST;
#if USE_YPCBCR
		sampler2D _ChromaTex;
		float4x4 _YpCbCrTransform;
#endif

		float3 RotateAroundYInDegrees (float3 vertex, float degrees)
		{
			const float CONST_PI = 3.14159265359f;
			float alpha = degrees * CONST_PI / 180.0;
			float sina, cosa;
			sincos(alpha, sina, cosa);
			float2x2 m = float2x2(cosa, -sina, sina, cosa);
			return float3(mul(m, vertex.xz), vertex.y).xzy;
		}

		struct appdata_t {
			float4 vertex : POSITION;
			float2 texcoord : TEXCOORD0;
#ifdef UNITY_STEREO_INSTANCING_ENABLED
			UNITY_VERTEX_INPUT_INSTANCE_ID
#endif
		};
		
		struct v2f {
			float4 vertex : SV_POSITION;
			float2 texcoord : TEXCOORD0;

#ifdef UNITY_STEREO_INSTANCING_ENABLED
			UNITY_VERTEX_OUTPUT_STEREO
#endif
#if STEREO_TOP_BOTTOM | STEREO_LEFT_RIGHT
	#if STEREO_DEBUG
			float4 tint : COLOR;
	#endif
#endif
		};

		static const float onehalf = 1.0 / 2.0;
		static const float onethird = 1.0 / 3.0;
		static const float twothird = 2.0 / 3.0;
		static const float exp_coeff = 1.01;
		static const float2 face_scale = float2(onethird, onehalf);

		v2f sb_vert(appdata_t v, float2 face_offset)
		{
			v2f o;

#ifdef UNITY_STEREO_INSTANCING_ENABLED
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
#endif
			float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
			o.vertex = XFormObjectToClip(float4(rotated, 0.0));

			// Remap texcoords for the cubemap face with the offset passed in.
			float2 t = _MainTex_TexelSize.wz * face_scale;
			float2 p = (floor((t * exp_coeff) - t) / t) * 0.5;
			float2 hp = p * 0.5;
			float2 of = face_offset + hp;
			float2 s = face_scale - p;
			float2 uv = v.texcoord * s + of;
			o.texcoord = TRANSFORM_TEX(uv, _MainTex);

#if STEREO_TOP_BOTTOM | STEREO_LEFT_RIGHT
			float4 scaleOffset = GetStereoScaleOffset(IsStereoEyeLeft(), _MainTex_ST.y < 0.0);
			o.texcoord *= scaleOffset.xy;
			o.texcoord += scaleOffset.zw;

			#if STEREO_DEBUG
			o.tint = GetStereoDebugTint(IsStereoEyeLeft());
			#endif
#endif

			return o;
		}

		half4 frag(v2f i) : SV_Target
		{
			half4 tex;
#if USE_YPCBCR
			tex = SampleYpCbCr(_MainTex, _ChromaTex, i.texcoord, _YpCbCrTransform);
#else
			tex = SampleRGBA(_MainTex, i.texcoord);
#endif
			half3 c = tex.rgb;
			//c = DecodeHDR(tex, _MainTex_HDR);
			//c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
			c *= _Exposure;

#if STEREO_TOP_BOTTOM | STEREO_LEFT_RIGHT
	#if STEREO_DEBUG
			c *= i.tint;
	#endif
#endif
			return half4(c, 1.0);
		}
		ENDCG

		Pass	// Front
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			v2f vert(appdata_t v)
			{
				return sb_vert(v, float2(onethird, 0));
			}
			ENDCG
		}
		
		Pass	// Back
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			v2f vert(appdata_t v)
			{
				return sb_vert(v, float2(twothird, 0));
			}
			ENDCG
		}
		
		Pass	// Right
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			v2f vert(appdata_t v)
			{
				return sb_vert(v, float2(0, onehalf));
			}
			ENDCG
		}
		
		Pass	// Left
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			v2f vert(appdata_t v)
			{
				return sb_vert(v, float2(onethird, onehalf));
			}
			ENDCG
		}
		
		Pass	// Top
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			v2f vert(appdata_t v)
			{
				return sb_vert(v, float2(twothird, onehalf));
			}
			ENDCG
		}
		
		Pass	// Bottom
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			v2f vert(appdata_t v)
			{
				return sb_vert(v, float2(0, 0));
			}
			ENDCG
		}
	}
}
