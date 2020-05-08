#ifndef LOS_INCLUDED
#define LOS_INCLUDED

#include "UnityCG.cginc"

struct v2f_img_ray
{
	float4 pos : POSITION;
	float2 uv : TEXCOORD0;
	float4 interpolatedRay : TEXCOORD1;
	float4 interpolatedOrigin : TEXCOORD2;
};

float LinearStep(float minValue, float maxValue, float v)
{
  return clamp((v - minValue) / (maxValue - minValue), 0, 1);
}

float ReduceLightBleeding(float p_max, float Amount)
{
	//Remove the [0, Amount] tail and linearly rescale (Amount, 1].
	return LinearStep(Amount, 1, p_max);
}

float ChebyshevUpperBound(float2 moments, float t, float minVariance)
{
	float p = (t <= moments.x);
	float variance = moments.y - (moments.x * moments.x);
	variance = max(variance, minVariance);

	float d = t - moments.x;
	float p_max = variance / (variance + d*d);
	p_max = ReduceLightBleeding(p_max, 0.5);

	return max(p, p_max);
}

float CalculateFade(float value, float fadeAmount)
{
	float fadedValue = value;
	fadedValue -= 1 - fadeAmount;
	fadedValue /= max(fadeAmount, 0.00000001);

	return 1 - saturate(fadedValue);
}

float4 DepthToWorldPosition(float depth, float4 interpolatedRay, float4 interpolatedOrigin)
{
	float4 viewRay = depth * interpolatedRay;
	float4 positionWorld = interpolatedOrigin + viewRay;
	positionWorld.w = 1;

	return positionWorld;
}

float4 SampleAndDecodeDepthNormal(sampler2D depthNormalsTexture, float2 uv)
{
	float depth;
	float3 pixelViewNormals;
	float4 encodedDepthNormals = tex2D(depthNormalsTexture, uv);

	DecodeDepthNormal(encodedDepthNormals, depth, pixelViewNormals);

	depth *= _ProjectionParams.z;

	return float4(pixelViewNormals, depth);
}

#endif