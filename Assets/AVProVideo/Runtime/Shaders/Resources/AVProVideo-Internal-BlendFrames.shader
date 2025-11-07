//-----------------------------------------------------------------------------
// Copyright 2014-2017 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

Shader "AVProVideo/Internal/BlendFrames"
{
	Properties
	{
		_MainTex("Before Texture", 2D) = "white" {}
		_AfterTex("After Texture", 2D) = "white" {}
		_t("t", Float) = 0.5
	}
	
	SubShader
	{
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			Fog{ Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma exclude_renderers flash xbox360 ps3 gles
			#include "UnityCG.cginc"
			#include "../AVProVideo.cginc"

			uniform sampler2D _MainTex;
			uniform sampler2D _AfterTex;
			uniform float _t;

			struct v2f 
			{
				float4 pos : POSITION;
				float4 uv : TEXCOORD0;
			};

			v2f vert(appdata_img v)
			{
				v2f o;
				o.uv = float4(0.0, 0.0, 0.0, 0.0);
				o.pos = XFormObjectToClip(v.vertex);

				o.uv.xy = v.texcoord.xy;
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				float4 before = tex2D(_MainTex, i.uv.xy);
				float4 after = tex2D(_AfterTex, i.uv.xy);

				float4 result = ((1.0 -_t) * before) + (_t * after);

				return result;
			}

			ENDCG
		}
	}

	FallBack Off
}