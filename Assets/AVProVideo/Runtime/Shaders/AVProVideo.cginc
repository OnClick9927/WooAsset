//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

//#define AVPRO_CHEAP_GAMMA_CONVERSION

#if defined (SHADERLAB_GLSL)
	#define INLINE
	#define HALF float
	#define HALF2 vec2
	#define HALF3 vec3
	#define HALF4 vec4
	#define FLOAT2 vec2
	#define FLOAT3 vec3
	#define FLOAT4 vec4
	#define FIXED4 vec4
	#define FLOAT3X3 mat3
	#define FLOAT4X4 mat4
	#define LERP mix
#else
	#define INLINE inline
	#define HALF half
	#define HALF2 half2
	#define HALF3 half3
	#define HALF4 half4
	#define FLOAT2 float2
	#define FLOAT3 float3
	#define FLOAT4 float4
	#define FIXED4 fixed4
	#define FLOAT3X3 float3x3
	#define FLOAT4X4 float4x4
	#define LERP lerp
#endif

// Specify this so Unity doesn't automatically update our shaders.
#define UNITY_SHADER_NO_UPGRADE 1

// We use this method so that when Unity automatically updates the shader from the old
// mul(UNITY_MATRIX_MVP.. to UnityObjectToClipPos that it only changes in one place.
INLINE FLOAT4 XFormObjectToClip(FLOAT4 vertex)
{
#if defined(SHADERLAB_GLSL)
	return gl_ModelViewProjectionMatrix * vertex;
#else
	return UnityObjectToClipPos(vertex);
#endif
}

uniform FLOAT4X4 _ViewMatrix;
uniform FLOAT3 _CameraPosition;

INLINE bool IsStereoEyeLeft()
{
#if defined(FORCEEYE_LEFT)
	return true;
#elif defined(FORCEEYE_RIGHT)
	return false;
#elif defined(UNITY_SINGLE_PASS_STEREO) || defined (UNITY_STEREO_INSTANCING_ENABLED)
	// Unity 5.4 has this new variable
	return (unity_StereoEyeIndex == 0);
#elif defined (UNITY_DECLARE_MULTIVIEW)
	// OVR_multiview extension
	return (UNITY_VIEWID == 0);
#else
	// worldNosePosition is the camera positon passed in from Unity via script
	// We need to determine whether _WorldSpaceCameraPos (Unity shader variable) is to the left or to the right of _CameraPosition

	FLOAT3 worldNosePosition = _CameraPosition;

#if defined (SHADERLAB_GLSL)
	FLOAT3 worldCameraRight = _ViewMatrix[0].xyz;
#else
	FLOAT3 worldCameraRight = UNITY_MATRIX_V[0].xyz;
#endif

	float dRight = distance(worldNosePosition + worldCameraRight, _WorldSpaceCameraPos);
	float dLeft = distance(worldNosePosition - worldCameraRight, _WorldSpaceCameraPos);
	return (dRight > dLeft);
#endif
}

#if defined(STEREO_TOP_BOTTOM) || defined(STEREO_LEFT_RIGHT)
FLOAT4 GetStereoScaleOffset(bool isLeftEye, bool isYFlipped)
{
	FLOAT2 scale = FLOAT2(1.0, 1.0);
	FLOAT2 offset = FLOAT2(0.0, 0.0);

	// Top-Bottom
#if defined(STEREO_TOP_BOTTOM)

	scale.y = 0.5;
	offset.y = 0.0;

	if (!isLeftEye)
	{
		offset.y = 0.5;
	}

#if !defined(SHADERLAB_GLSL)
//#if !defined(UNITY_UV_STARTS_AT_TOP)	// UNITY_UV_STARTS_AT_TOP is for directx
	if (!isYFlipped)
	{
		// Currently this only runs for Android and Windows using DirectShow
		offset.y = 0.5 - offset.y;
	}
//#endif
#endif

	// Left-Right
#elif defined(STEREO_LEFT_RIGHT)

	scale.x = 0.5;
	offset.x = 0.0;
	if (!isLeftEye)
	{
		offset.x = 0.5;
	}

#endif

	return FLOAT4(scale, offset);
}
#endif

#if defined(STEREO_DEBUG)
INLINE FLOAT4 GetStereoDebugTint(bool isLeftEye)
{
	FLOAT4 tint = FLOAT4(1.0, 1.0, 1.0, 1.0);

#if defined(STEREO_TOP_BOTTOM) || defined(STEREO_LEFT_RIGHT) || defined(STEREO_CUSTOM_UV)
	FLOAT4 leftEyeColor = FLOAT4(0.0, 1.0, 0.0, 1.0);		// green
	FLOAT4 rightEyeColor = FLOAT4(1.0, 0.0, 0.0, 1.0);		// red

	if (isLeftEye)
	{
		tint = leftEyeColor;
	}
	else
	{
		tint = rightEyeColor;
	}
#endif

#if defined(UNITY_UV_STARTS_AT_TOP)
	//tint.b = 0.5;
#endif
/*#if defined(UNITY_SINGLE_PASS_STEREO) || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_DECLARE_MULTIVIEW)
	tint.b = 1.0;
#endif*/

	return tint;
}
#endif

FLOAT2 ScaleZoomToFit(float targetWidth, float targetHeight, float sourceWidth, float sourceHeight)
{
#if defined(ALPHAPACK_TOP_BOTTOM)
	sourceHeight *= 0.5;
#elif defined(ALPHAPACK_LEFT_RIGHT)
	sourceWidth *= 0.5;
#endif
	float targetAspect = targetHeight / targetWidth;
	float sourceAspect = sourceHeight / sourceWidth;
	FLOAT2 scale = FLOAT2(1.0, sourceAspect / targetAspect);
	if (targetAspect < sourceAspect)
	{
		scale = FLOAT2(targetAspect / sourceAspect, 1.0);
	}
	return scale;
}

FLOAT4 OffsetAlphaPackingUV(FLOAT2 texelSize, FLOAT2 uv, bool flipVertical)
{
	FLOAT4 result = uv.xyxy;

	// We don't want bilinear interpolation to cause bleeding
	// when reading the pixels at the edge of the packed areas.
	// So we shift the UV's by a fraction of a pixel so the edges don't get sampled.

#if defined(ALPHAPACK_TOP_BOTTOM)
	float offset = texelSize.y * 1.5;
	result.y = LERP(0.0 + offset, 0.5 - offset, uv.y);
	result.w = result.y + 0.5;

	if (flipVertical)
	{
		// Flip vertically (and offset to put back in 0..1 range)
		result.yw = 1.0 - result.yw;
		result.yw = result.wy;
	}
	else
	{
#if !defined(UNITY_UV_STARTS_AT_TOP)
		// For opengl we flip
		result.yw = result.wy;
#endif
	}

#elif defined(ALPHAPACK_LEFT_RIGHT)
	float offset = texelSize.x * 1.5;
	result.x = LERP(0.0 + offset, 0.5 - offset, uv.x);
	result.z = result.x + 0.5;

	if (flipVertical)
	{
		// Flip vertically (and offset to put back in 0..1 range)
		result.yw = 1.0 - result.yw;
	}

#else

	if (flipVertical)
	{
		// Flip vertically (and offset to put back in 0..1 range)
		result.yw = 1.0 - result.yw;
	}

#endif

	return result;
}

INLINE HALF3 GammaToLinear_ApproxPow(HALF3 col)
{
	#if defined (SHADERLAB_GLSL)
	return pow(col, HALF3(2.2, 2.2, 2.2));
	#else
	return pow(col, HALF3(2.2h, 2.2h, 2.2h));
	#endif
}

INLINE HALF3 LinearToGamma_ApproxPow(HALF3 col)
{
	#if defined (SHADERLAB_GLSL)
	return pow(col, HALF3(1.0/2.2, 1.0/2.2, 1.0/2.2));
	#else
	return pow(col, HALF3(1.0h/2.2h, 1.0h/2.2h, 1.0h/2.2h));
	#endif
}

// Approximate version from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
// NOTE: This is about 4 instructions vs 10 instructions for the accurate version
INLINE HALF3 GammaToLinear_ApproxFit(HALF3 col)
{
#if defined (SHADERLAB_GLSL)
	HALF a = 0.305306011;
	HALF b = 0.682171111;
	HALF c = 0.012522878;
#else
	HALF a = 0.305306011h;
	HALF b = 0.682171111h;
	HALF c = 0.012522878h;
#endif
	return col * (col * (col * a + b) + c);
}

// Approximate version from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
INLINE HALF3 LinearToGamma_ApproxFit(HALF3 col)
{
#if defined (SHADERLAB_GLSL)
	HALF a = 0.416666667;
	HALF b = 0.055;
	HALF c = 0.0;
	HALF d = 1.055;
#else
	HALF a = 0.416666667h;
	HALF b = 0.055h;
	HALF c = 0.0h;
	HALF d = 1.055h;
#endif
	return max(d * pow(col, HALF3(a, a, a)) - b, c);
}

INLINE HALF3 GammaToLinear_Accurate(HALF3 col)
{
	if (col.r <= 0.04045)
		col.r = col.r / 12.92;
	else
		col.r = pow((col.r + 0.055) / 1.055, 2.4);

	if (col.g <= 0.04045)
		col.g = col.g / 12.92;
	else
		col.g = pow((col.g + 0.055) / 1.055, 2.4);

	if (col.b <= 0.04045)
		col.b = col.b / 12.92;
	else
		col.b = pow((col.b + 0.055) / 1.055, 2.4);

	// NOTE: We tried to optimise the above, but actually the compiler does a better job..
	/*HALF3 a = col / 12.92;
	HALF3 b = pow((col + 0.055) / 1.055, 2.4);
	HALF3 c = step(col,0.04045);
	col = LERP(b, a, c);*/

	return col;
}

INLINE HALF3 LinearToGamma_Accurate(HALF3 col)
{
	if (col.r <= 0.0031308)
		col.r = col.r * 12.92;
	else
		col.r = 1.055 * pow(col.r, 0.4166667) - 0.055;

	if (col.g <= 0.0031308)
		col.g = col.g * 12.92;
	else
		col.g = 1.055 * pow(col.g, 0.4166667) - 0.055;

	if (col.b <= 0.0031308)
		col.b = col.b * 12.92;
	else
		col.b = 1.055 * pow(col.b, 0.4166667) - 0.055;

	return col;
}

// http://entropymine.com/imageworsener/srgbformula/
INLINE HALF3 GammaToLinear(HALF3 col)
{
#if defined(AVPRO_CHEAP_GAMMA_CONVERSION)
	return GammaToLinear_ApproxFit(col);
#else
	return GammaToLinear_Accurate(col);
#endif
}

// http://entropymine.com/imageworsener/srgbformula/
INLINE HALF3 LinearToGamma(HALF3 col)
{
#if defined(AVPRO_CHEAP_GAMMA_CONVERSION)
	return LinearToGamma_ApproxFit(col);
#else
	return LinearToGamma_Accurate(col);
#endif
}

INLINE FLOAT3 ConvertYpCbCrToRGB(FLOAT3 YpCbCr, FLOAT4X4 YpCbCrTransform)
{
#if defined(SHADERLAB_GLSL)
	return clamp(FLOAT3X3(YpCbCrTransform) * (YpCbCr + YpCbCrTransform[3].xyz), 0.0, 1.0);
#else
	return saturate(mul((FLOAT3X3)YpCbCrTransform, YpCbCr + YpCbCrTransform[3].xyz));
#endif
}

INLINE HALF4 SampleRGBA(sampler2D tex, FLOAT2 uv)
{
#if defined(SHADERLAB_GLSL)		// GLSL doesn't support tex2D, so just return for now
	return HALF4(1.0, 1.0, 0.0, 1.0);
#else
	HALF4 rgba = tex2D(tex, uv);
#if defined(APPLY_GAMMA)
	rgba.rgb = GammaToLinear(rgba.rgb);
#endif
	return rgba;
#endif
}

INLINE HALF4 SampleYpCbCr(sampler2D luma, sampler2D chroma, FLOAT2 uv, FLOAT4X4 YpCbCrTransform)
{
#if defined(SHADERLAB_GLSL)		// GLSL doesn't support tex2D, so just return for now
	return HALF4(1.0, 1.0, 0.0, 1.0);
#else
#if defined(SHADER_API_METAL) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
	FLOAT3 YpCbCr = FLOAT3(tex2D(luma, uv).r, tex2D(chroma, uv).rg);
#else
	FLOAT3 YpCbCr = FLOAT3(tex2D(luma, uv).r, tex2D(chroma, uv).ra);
#endif
	HALF4 rgba = HALF4(ConvertYpCbCrToRGB(YpCbCr, YpCbCrTransform), 1.0);
#if defined(APPLY_GAMMA)
	rgba.rgb = GammaToLinear(rgba.rgb);
#endif
	return rgba;
#endif
}

INLINE HALF SamplePackedAlpha(sampler2D tex, FLOAT2 uv)
{
#if defined(SHADERLAB_GLSL)	// GLSL doesn't support tex2D, so just return for now
	return 0.5;
#else
	HALF alpha;
#if defined(USE_YPCBCR)
	alpha = (tex2D(tex, uv).r - 0.0625) * (255.0 / 219.0);
#else
	HALF3 rgb = tex2D(tex, uv).rgb;
#if defined(APPLY_GAMMA)
	rgb = GammaToLinear(rgb);
#endif
	alpha = (rgb.r + rgb.g + rgb.b) / 3.0;
#endif
	return alpha;
#endif
}

#if defined(USE_HSBC)
INLINE HALF3 ApplyHue(HALF3 color, HALF hue)
{
	HALF angle = radians(hue);
	HALF3 k = HALF3(0.57735, 0.57735, 0.57735);
	HALF cosAngle = cos(angle);
	//Rodrigues' rotation formula
	return color * cosAngle + cross(k, color) * sin(angle) + k * dot(k, color) * (1.0 - cosAngle);
}

INLINE HALF3 ApplyHSBEffect(HALF3 color, FIXED4 hsbc)
{
	HALF hue = hsbc.r * 360.0;
	HALF saturation = hsbc.g * 2.0;
	HALF brightness = hsbc.b * 2.0 - 1.0;
	HALF contrast = hsbc.a * 2.0;

	HALF3 result = color;
	result.rgb = ApplyHue(result, hue);
	result.rgb = (result - 0.5f) * contrast + 0.5f + brightness;
	result.rgb = LERP(Luminance(result), result, saturation);
	
	return result;
}
#endif