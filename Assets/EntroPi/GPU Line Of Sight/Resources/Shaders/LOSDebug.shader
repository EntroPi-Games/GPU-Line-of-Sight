Shader "Hidden/Line Of Sight Debug"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "black" {}
	}

	CGINCLUDE
	#include "/LOSInclude.cginc"

	uniform sampler2D _MainTex;
	uniform sampler2D _DebugTex;
	uniform sampler2D _CameraDepthNormalsTexture;

	uniform float4 _MainTex_TexelSize;
	uniform float4x4 _FrustumRays;
	uniform float4x4 _FrustumOrigins;

	float4 FragDepth (v2f_img i) : COLOR
	{
		float4 normalDepth = SampleAndDecodeDepthNormal(_CameraDepthNormalsTexture, i.uv);
		return normalDepth.w;
	}

	float4 FragNormals (v2f_img i) : COLOR
	{
		float4 normalDepth = SampleAndDecodeDepthNormal(_CameraDepthNormalsTexture, i.uv);
		return float4(normalDepth.xyz, 1);
	}

	v2f_img_ray VertWorldPosition( appdata_img v )
	{
		v2f_img_ray o;
		int index = v.vertex.z;
		v.vertex.z = 0.0f;

		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.texcoord.xy;

#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			o.uv.y = 1-o.uv.y;
#endif

		o.interpolatedRay = _FrustumRays[index];
		o.interpolatedRay.w = index;

		o.interpolatedOrigin = _FrustumOrigins[index];
		o.interpolatedOrigin.w = index;

		return o;
	}

	float4 FragWorldPosition (v2f_img_ray i) : COLOR
	{
		float4 normalDepth = SampleAndDecodeDepthNormal(_CameraDepthNormalsTexture, i.uv);
		float4 positionWorld = DepthToWorldPosition(normalDepth.w, i.interpolatedRay, i.interpolatedOrigin);

		return positionWorld;
	}

	ENDCG

	SubShader
	{
		Pass
		{
			ZTest Always
			Cull Off
			ZWrite Off

			Fog { Mode off }

			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment FragNormals
			#pragma fragmentoption ARB_precision_hint_fastest

			ENDCG
		}

		Pass
		{
			ZTest Always
			Cull Off
			ZWrite Off

			Fog { Mode off }

			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment FragDepth
			#pragma fragmentoption ARB_precision_hint_fastest

			ENDCG
		}

		Pass
		{
			ZTest Always
			Cull Off
			ZWrite Off

			Fog { Mode off }

			CGPROGRAM

			#pragma vertex VertWorldPosition
			#pragma fragment FragWorldPosition
			#pragma fragmentoption ARB_precision_hint_fastest

			ENDCG
		}
	}

	Fallback off
}