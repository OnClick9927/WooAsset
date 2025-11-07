// Credit to Inigo Quilez (https://www.iquilezles.org/www/articles/distfunctions2d/distfunctions2d.htm) for many of the 2D functions used
Shader "Unlit/MediaPlayerUI"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15
		[KeywordEnum(Circle, Play, Pause, PlayPause, Buffering, Volume, Forward, Back, CC, Options, Spectrum)] UI("UI Element", Float) = 0
		_Morph("Morph", Range(0, 1)) = 0
		_Mute("Mute", Range(0, 1)) = 0
		_Volume("Volume", Range(0, 1)) = 1

		[Toggle(DRAW_OUTLINE)] _DrawOutline("Draw Outline", Float) = 0
		_OutlineSize("Outline Size", Range(0, 0.1)) = 0.05
		_OutlineOpacity("Outline Opacity", Range(0, 1)) = 0.25
		_OutlineSoftness("Outline Softness", Range(0, 1)) = 0
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
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Fog{ Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask[_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile UI_CIRCLE UI_PLAY UI_PAUSE UI_PLAYPAUSE UI_BUFFERING UI_VOLUME UI_FORWARD UI_BACK UI_CC UI_OPTIONS UI_SPECTRUM
			#pragma shader_feature DRAW_OUTLINE
			#include "UnityCG.cginc"

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
				half2 uv  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
			};

			uniform fixed4 _Color;
			uniform float _Morph;
			uniform float _Volume;
			uniform float _Mute;
			uniform float _OutlineSize;
			uniform float _OutlineOpacity;
			uniform float _OutlineSoftness;
#if UI_SPECTRUM
			#if SHADER_API_GLES
			uniform float _Spectrum[4];
			#else
			uniform float _Spectrum[128];
			#endif
			uniform float _SpectrumRange;
			uniform float _SpectrumMax;
#endif
			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw - 1.0)*float2(-1, 1);
#endif
				OUT.uv.xy = IN.texcoord.xy;
				OUT.color = IN.color * _Color;

				return OUT;
			}
			
			float ndot(float2 a, float2 b) { return a.x*b.x - a.y*b.y; }

			float opU(float d1, float d2) { return min(d1, d2); }

			float opS(float d1, float d2) { return max(-d1, d2); }

			float opI(float d1, float d2) { return max(d1, d2); }

			float sdCircle(in float2 p, float r)
			{
				return length(p) - r;
			}

			float sdRhombus(in float2 p, in float2 b)
			{
				float2 q = abs(p);
				float h = clamp((-2.0*ndot(q, b) + ndot(b, b)) / dot(b, b), -1.0, 1.0);
				float d = length(q - 0.5*b*float2(1.0 - h, 1.0 + h));
				return d * sign(q.x*b.y + q.y*b.x - b.x*b.y);
			}

			float sdEquilateralTriangle(in float2 p)
			{
				const float k = sqrt(3.0);

				p.x = abs(p.x) - 1.0;
				p.y = p.y + 1.0 / k;
				if (p.x + k*p.y > 0.0) p = float2(p.x - k*p.y, -k*p.x - p.y) / 2.0;
				p.x -= clamp(p.x, -2.0, 0.0);
				return -length(p)*sign(p.y);
			}

			float sdTriangle(in float2 p, in float2 p0, in float2 p1, in float2 p2)
			{
				float2 e0 = p1 - p0, e1 = p2 - p1, e2 = p0 - p2;
				float2 v0 = p - p0, v1 = p - p1, v2 = p - p2;

				float2 pq0 = v0 - e0*clamp(dot(v0, e0) / dot(e0, e0), 0.0, 1.0);
				float2 pq1 = v1 - e1*clamp(dot(v1, e1) / dot(e1, e1), 0.0, 1.0);
				float2 pq2 = v2 - e2*clamp(dot(v2, e2) / dot(e2, e2), 0.0, 1.0);

				float s = sign(e0.x*e2.y - e0.y*e2.x);
				float2 d = min(min(float2(dot(pq0, pq0), s*(v0.x*e0.y - v0.y*e0.x)),
					float2(dot(pq1, pq1), s*(v1.x*e1.y - v1.y*e1.x))),
					float2(dot(pq2, pq2), s*(v2.x*e2.y - v2.y*e2.x)));

				return -sqrt(d.x)*sign(d.y);
			}


			float sdTriangleIsosceles(in float2 p, in float2 q)
			{
				p.x = abs(p.x);

				float2 a = p - q*clamp(dot(p, q) / dot(q, q), 0.0, 1.0);
				float2 b = p - q*float2(clamp(p.x / q.x, 0.0, 1.0), 1.0);
				float s = -sign(q.y);
				float2 d = min(float2(dot(a, a), s*(p.x*q.y - p.y*q.x)),
					float2(dot(b, b), s*(p.y - q.y)));

				return -sqrt(d.x)*sign(d.y);
			}

			float sdBox(in float2 p, in float2 b)
			{
				float2 d = abs(p) - b;
				return length(max(d, float2(0.0, 0.0))) + min(max(d.x, d.y), 0.0);
			}

			float sdRoundedBox(in float2 p, in float2 b, in float4 r)
			{
				r.xy = (p.x>0.0)?r.xy : r.zw;
				r.x  = (p.y>0.0)?r.x  : r.y;
				float2 q = abs(p)-b+r.x;
				return min(max(q.x,q.y),0.0) + length(max(q,0.0)) - r.x;
			}

			float sdArc(in float2 p, in float2 sca, in float2 scb, in float ra, float rb)
			{
				p = mul(float2x2(sca.x,sca.y,-sca.y,sca.x), p);
				p.x = abs(p.x);
				float k = (scb.y*p.x>scb.x*p.y) ? dot(p.xy,scb) : length(p.xy);
				return sqrt( dot(p,p) + ra*ra - 2.0*ra*k ) - rb;
			}

			float2 rotate2d(float2 v, float a) 
			{
				float s = sin(a);
				float c = cos(a);
				float2x2 m = float2x2(c, -s, s, c);
				return mul(m,v);
			}

#if UI_FORWARD || UI_BACK
			float forward(float2 uv)
			{
				float r1 = 100000.0;
				r1 = sdTriangleIsosceles(float2(uv.y, -uv.x) + float2(0.0, 0.333), float2(0.666, 0.666));
				r1 = opS(r1, sdTriangleIsosceles(float2(uv.y, -uv.x) + float2(0.0, 0.333 * 1.5), float2(0.666, 0.666)));

				uv.x += 0.2;

				float r2 = 100000.0;
				r2 = sdTriangleIsosceles(float2(uv.y, -uv.x) + float2(0.0, 0.333), float2(0.666, 0.666));
				//r2 = opS(r2, sdTriangleIsosceles(float2(uv.y, -uv.x) + float2(0.0, 0.333 * 1.5), float2(0.666, 0.666)));

				float r = opU(r1, r2);

				return r;
			}
#endif

#if UI_VOLUME
			float volume(float2 uv)
			{
				float r = 100000.0;

				uv.x += 0.25;

				// Cone
				r = opU(r, sdBox(uv + float2(0.1, 0.0), float2(0.25, 0.25)));
				r = opU(r, sdTriangleIsosceles(float2(uv.y, uv.x) + float2(0.0, 0.3), float2(0.6, 0.6)));

				// Ripple occluder
				float s = sdBox(uv + float2(0, 0.0), float2(0.4, 0.7));

				// Ripple thickness
				float rt = 0.15;

				// Ripple 1
				float offset = -0.25;
				if (_Volume > 0.0)
				{
					float t = saturate(_Volume / 0.5);
					r = opU(r, opS(s, sdCircle(uv + float2(offset, 0.0), rt*lerp(1, 2, t))));
				}

				// Ripple 2
				float a;
				//a = sdCircle(uv + float2(offset, 0.0), 0.6 - rt * 3);
				//a = opS(a, opS(s, sdCircle(uv + float2(offset, 0.0), 0.6 - rt * 2)));
				//r = opU(a, r);

				// Ripple 3
				if (_Volume > 0.5)
				{
					float t = saturate((_Volume - 0.5) / 0.5);
					a = sdCircle(uv + float2(offset, 0.0), (0.6 - rt));
					a = opS(a, opS(s, sdCircle(uv + float2(offset, 0.0), lerp(0.6-rt, 0.6, t))));
					r = opU(a, r);
				}

				// Crossout
				if (_Mute > 0.0)
				{
					float maxLength = 0.8;
					float length = _Mute * maxLength;
					{
						// Cutout
						r = opS(-r, -sdBox(rotate2d(uv - float2(0.25, 0), -3.14/4) + float2(-0.1, length/1 - maxLength/1), float2(0.1, length*1)));

						// Line
						r = opU(r, sdBox(rotate2d(uv - float2(0.25, 0), -3.14/4) + float2(0.0, length/1 - maxLength/1), float2(0.1, length*1)));
					}
				}

				return r;
			}
#endif


#if UI_CC
			float ccbutton(float2 uv)
			{
				float r = 100000.0;

				float barHeight = 0.1;

				float boxSize = 0.65;
				float r3 = sdRoundedBox(uv, float2(boxSize, boxSize * 0.9), float4(0.25, 0.25, 0.25, 0.25));

				float angle1 = 0.0;
				float angle2 = 2.3;
				float thickeness = 0.08;
				float size = 0.25;
				float rLeftC = sdArc(float2(0.35 + uv.x*1.5, uv.y), float2(sin(angle1), cos(angle1)), float2(sin(angle2), cos(angle2)), size, thickeness);
				float rRightC = sdArc(float2(-0.40 + uv.x*1.5, uv.y), float2(sin(angle1), cos(angle1)), float2(sin(angle2), cos(angle2)), size, thickeness);

				r = opU(rLeftC, rRightC);
				r = opS(r, r3);

				if (_Morph > 0.0)
				{
					float barWidth = lerp(0.0, boxSize, _Morph);
					float r4 = sdBox(uv + float2(0.0, boxSize + barHeight * 2.0), float2(barWidth, barHeight));
					r = opU(r, r4);
				}

				return r;
			}
#endif

#if UI_OPTIONS
			float optionsgear(float2 uv, float radius, float discRatio, float holeRatio, float spokeRatio)
			{
				float r = 100000.0;

				float r1 = sdCircle(uv, radius * holeRatio);
				float r2 = sdCircle(uv, radius * discRatio);

				float rotationOffset = lerp(0.0, 3.141592654/6.0, _Morph);

				float b1 = sdBox(rotate2d(uv, (3.141592654 / 2.0) + rotationOffset), float2(radius, radius * spokeRatio));
				float b2 = sdBox(rotate2d(uv, (3.141592654 / 6.0) + rotationOffset), float2(radius, radius * spokeRatio));
				float b3 = sdBox(rotate2d(uv,-(3.141592654 / 6.0) + rotationOffset), float2(radius, radius * spokeRatio));

				r = r2;				// Base circle
				r = opU(r, b1);		// Spoke 1
				r = opU(r, b2);		// Spoke 2
				r = opU(r, b3);		// Spoke 3
				r = opS(r1, r);		// Hole

				return r;
			}
#endif

#if UI_BUFFERING
			float CircularDistance(float a, float b, float range)
			{
				float d1 = abs(a-b);
				//float d2 = range - d1;
				//return lerp((a-b), d2, d1 / (range/2));
				if (d1 > range/2)
				{
					d1 = (range - d1);
				}
				else
				{
					d1 = a-b;
				}
				return d1;
			}
#endif

			fixed4 getColorWithOutline(float d, float3 shapeColor, float3 outlineColor)
			{
				float dw = fwidth(d) * 0.5;
				float shapeAlpha = smoothstep(dw, -dw, d);
				
#if !DRAW_OUTLINE
				return float4(shapeColor, shapeAlpha);
#else

				float od = (d - _OutlineSize);
				float dw2 = fwidth(od) * 0.5;
				float outlineAlpha = smoothstep(dw2 + _OutlineSoftness, -dw2 - _OutlineSoftness, od);

				return lerp(float4(outlineColor, outlineAlpha * _OutlineOpacity), float4(shapeColor, shapeAlpha), shapeAlpha);
#endif
			}

			fixed4 frag(v2f i) : SV_Target
			{
#if UI_SPECTRUM
			// In GLES2.0 indexing from the _Spectrum[] array is not supported
			#if SHADER_API_GLES
				float v = 0.0;
				float d = 0.0;
			#else
				float x = (pow(i.uv.x, 1.0) * _SpectrumRange)-1.0;
				//_Spectrum[0] = 0.0;
				// Bilinear sample the values
				float scale = (1.0+i.uv.x * 8.0);	// Scale higher freqs to give them more movement
				float v1 = 0.0;
				float v2 = 0.0;
				int t1 = floor(x);
				int t2 = ceil(x);
				if (t1 >= 0)
				{
					v1 = (_Spectrum[t1] * scale);
				}
				if (t2 >= 0)
				{
					v2 = (_Spectrum[t2] * scale);
				}
				v1 = max(v1, 0.01);
				v2 = max(v2, 0.01);
				float2 uvn = float2(0.0, i.uv.y);
				// Get vertical distance
				float d1 = (abs(i.uv.y - 0.5) - (v1/1.0));
				float d2 = (abs(i.uv.y - 0.5) - (v2/1.0));
				// Interpolate 
				float xf = frac(x);
				float v = saturate(lerp(v1, v2, xf));
				float d = lerp(d1, d2, xf);
			#endif
				// Get colour from texture
				float yy2 = abs(i.uv.y - 0.5) * 2.0;
				float yy = v;
				float level = i.uv.y;// + pow(yy2+v, 8);
				float3 col = level;//tex2D(_MainTex, float2(level, 0.0f));
				//col.g += abs(i.uv.x);
				return getColorWithOutline(d, col, float3(0.0, 0.0, 0.0)) * i.color;
#else
#if UI_CIRCLE
				float2 uvn = (i.uv.xy - 0.5) / 0.5;
				float d = sdCircle(uvn, 1.0);
#elif UI_PLAY
				float2 uvn = (i.uv.xy - float2(0.5, 0.5)) / 0.5;
				float d = sdTriangle(uvn, float2(-0.6, 0.6), float2(0.6, 0), float2(-0.6, -0.6));
				
#elif UI_PAUSE
				float d1 = sdBox(i.uv - 0.5 + float2(0.2, 0.0), float2(0.1, 0.3));
				float d2 = sdBox(i.uv - 0.5 - float2(0.2, 0.0), float2(0.1, 0.3));
				float d = min(d1, d2);
				//c = 1.0 - saturate(smoothstep(dw-0.022, -dw, d))*0.2;
#elif UI_PLAYPAUSE
				float2 uvn = (i.uv.xy - float2(0.5, 0.5)) / 0.5;
				float d3 = sdTriangle(uvn, float2(-0.6, 0.6), float2(0.6, 0), float2(-0.6, -0.6));

				float d1 = sdBox(i.uv - 0.5 + float2(0.2, 0.0), float2(0.1, 0.3));
				float d2 = sdBox(i.uv - 0.5 - float2(0.2, 0.0), float2(0.1, 0.3));
				float d = min(d1, d2);

				float dw1 = fwidth(d) * 0.5;
				float dw3 = fwidth(d3) * 0.5;
				float dw = lerp(dw1, dw3, _Morph);
				//a = smoothstep(dw, -dw, lerp(d, d3, _Morph));
				d = lerp(d, d3, _Morph);
				//c = 1.0 - saturate(smoothstep(-0.025, 0, lerp(d, d3, _Morph)))*0.2;
#elif UI_BUFFERING
				float rsize = 0.05;
				float r = 0.5;
				float2 uvn = (i.uv.xy - 0.5) / 0.5;

				// Inner radius
				float d = sdCircle(uvn, r - rsize);

				// Outer radius
				d = opS(d, sdCircle(uvn, r + rsize));

				// Animation angle
				float za = -(_Time.x * 160) + cos(_Time.y*2.0);
				float zz = sin(_Time.y);

				// Create point at the animated angle, at the same radius as this UV
				float2 dp = float2(sin(za), cos(za)) * length(uvn);

				// Calculate angle between the UV and the new point and subtract offset
				float dy = abs(atan((dp.x - uvn.x)/(dp.y - uvn.y))) - abs(zz);
				d = opI(d, dy);

#elif UI_VOLUME
				float2 uvn = (i.uv.xy - 0.5) / 0.5;
				float d = volume(uvn);
#elif UI_FORWARD
				float2 uvn = (i.uv.xy - 0.5) / 0.5;
				float d = forward(uvn);
#elif UI_BACK
				float2 uvn = (i.uv.xy - 0.5) / 0.5;
				float d = forward(float2(-uvn.x, uvn.y));
#elif UI_CC
				float2 uvn = (i.uv.xy - 0.5) / 0.5;
				float d = ccbutton(uvn);
#elif UI_OPTIONS
				float2 uvn = (i.uv.xy - 0.5) / 0.5;
				float d = optionsgear(uvn, 0.75, 0.75, 0.35, 0.25);
#endif

				return getColorWithOutline(d, float3(1.0, 1.0, 1.0), float3(0.0, 0.0, 0.0)) * i.color;
#endif
			}
			ENDCG
		}
	}
}
