Shader "AVProVideo/Internal/Transition"
{
	Properties
	{
		_MainTex ("Texture To", 2D) = "white" {}
		_FromTex ("Texture From", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// TODO: replace use multi_compile_local instead (Unity 2019.1 feature)
			#pragma multi_compile LERP_NONE LERP_FADE LERP_BLACK LERP_WHITE LERP_TRANSP LERP_HORIZ LERP_HORIZ_MIRROR LERP_VERT LERP_VERT_MIRROR LERP_DIAG LERP_DIAG_MIRROR LERP_CIRCLE LERP_SCROLL_VERT LERP_SCROLL_HORIZ LERP_DIAMOND LERP_BLINDS LERP_RECTS_VERT LERP_ARROW LERP_SLIDE_HORIZ LERP_SLIDE_VERT LERP_ZOOM_FADE

			#include "UnityCG.cginc"
			#include "../AVProVideo.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _FromTex;
			float4 _MainTex_ST;
			float4 _FromTex_ST;
			float _Fade;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = XFormObjectToClip(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 uv1 = i.uv;
				float2 uv2 = i.uv;

#if LERP_FADE
				float t = _Fade;
#elif LERP_BLACK
				float t = _Fade;
				if (t < 0.5)
				{
					return lerp(tex2D(_FromTex, uv1), float4(0.0, 0.0, 0.0, 1.0), t * 2.0);
				}
				else
				{
					return lerp(float4(0.0, 0.0, 0.0, 1.0), tex2D(_MainTex, uv2), 2.0 * (t - 0.5));
				}
#elif LERP_WHITE
				float t = _Fade;
				if (t < 0.5)
				{
					return lerp(tex2D(_FromTex, uv1), float4(1.0, 1.0, 1.0, 1.0), t * 2.0);
				}
				else
				{
					return lerp(float4(1.0, 1.0, 1.0, 1.0), tex2D(_MainTex, uv2), 2.0 * (t - 0.5));
				}
#elif LERP_TRANSP
				float t = _Fade;
				if (t < 0.5)
				{
					return lerp(tex2D(_FromTex, uv1), float4(0.0, 0.0, 0.0, 0.0), t * 2.0);
				}
				else
				{
					return lerp(float4(0.0, 0.0, 0.0, 0.0), tex2D(_MainTex, uv2), 2.0 * (t - 0.5));
				}
#elif LERP_HORIZ
				float t = step(i.uv.x, _Fade);
#elif LERP_HORIZ_MIRROR
				float t = step(abs(i.uv.x - 0.5), _Fade);
#elif LERP_VERT
				float t = step(i.uv.y, _Fade);
#elif LERP_VERT_MIRROR
				float t = step(abs(i.uv.y - 0.5), _Fade);
#elif LERP_DIAG
				float t = step((i.uv.y+i.uv.x)*0.5, _Fade);
#elif LERP_DIAG_MIRROR
				float t = step(abs(i.uv.y - i.uv.x), _Fade);
#elif LERP_CIRCLE
				float t = distance(float2(i.uv.x*1.777, i.uv.y), float2(0.5*1.7777, 0.5));
				t = step(t, _Fade*2.1);
#elif LERP_SCROLL_VERT
				float t = _Fade;
				uv1.y += _Fade;
				t = step(1 - uv1.y, 0);
#elif LERP_SCROLL_HORIZ
				float t = _Fade;
				uv1.x += _Fade;
				t = step(1 - uv1.x, 0);
#elif LERP_DIAMOND	
				float2 origin = float2(0.5 * 1.7777, 0.5);

				float t = abs(uv1.x*1.7777 - origin.x);
				t += abs(uv1.y - origin.y);
				
				t = step(t, _Fade*1.4);
#elif LERP_BLINDS

				float x = frac(uv1.x*4.0);
				float t = step(x, _Fade);
			
#elif LERP_ARROW
				// Arrow
				float y = abs(i.uv.y - 0.5) * 0.5;
				float x = lerp(0.5, 1.0, i.uv.x);
				float t = step(x, y + _Fade);
#elif LERP_SLIDE_HORIZ
				// Slide horiz
				float t = _Fade;
				uv1.x += _Fade;
				uv2.x -= 1.0 - _Fade;
				t = step(1 - uv1.x, 0);
#elif LERP_SLIDE_VERT
				// slide vert
				float t = _Fade;
				uv1.y += _Fade;
				uv2.y -= 1.0 - _Fade;
				t = step(1 - uv1.y, 0);
#elif LERP_ZOOM_FADE
				// zoom-fade
				float scale = lerp(1.0, 0.15, _Fade);
				float scale2 = lerp(1.0, 0.15, 1.0-_Fade);
				uv1 -= 0.5;
				uv2 -= 0.5;
				uv1 *= scale;
				uv2 *= scale2;
				uv1 += 0.5;
				uv2 += 0.5;
				float t = smoothstep(0.5, 1.0, _Fade);

#elif LERP_RECTS_VERT

				float x = uv1.x;

				float bf = _Fade / 1.5;

				bf = frac(uv1.y * 8.0);
				bf = (int)fmod(uv1.y * 8.0, 8.0);
				bf += 1.0;

				bf *= _Fade / 2.0;

				float t = step(abs(x - 0.5), bf);

#endif

#if LERP_NONE
				return tex2D(_MainTex, uv1);
#else
				float4 cola = tex2D(_FromTex, uv1);
				float4 colb = tex2D(_MainTex, uv2);

				float4 col = lerp(cola, colb, t);
				return col;						
#endif
			}
			ENDCG
		}
	}
}