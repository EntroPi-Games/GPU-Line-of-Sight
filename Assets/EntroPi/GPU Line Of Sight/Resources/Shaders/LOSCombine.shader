Shader "Hidden/Line Of Sight Combiner"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "black" {}
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	uniform sampler2D _MainTex;
	uniform sampler2D _PreEffectTex;
	uniform sampler2D _MaskTex;
	uniform sampler2D _StencilMask;
	uniform float4 _StencilMask_ST;

	float4 frag (v2f_img i) : COLOR
	{
		float4 postEffectColor = tex2D(_MainTex, i.uv);
		float4 preEffectColor = tex2D(_PreEffectTex, i.uv);
		float4 mask = tex2D(_MaskTex, i.uv);

		float4 finalColor = float4(1,1,1,1);
		finalColor.rgb = (preEffectColor.rgb * mask.rgb) + (postEffectColor.rgb * (1 - mask.a));

		return finalColor;
	}

	float4 fragStencil (v2f_img i) : COLOR
	{
		float4 postEffectColor = tex2D(_MainTex, i.uv);
		float4 preEffectColor = tex2D(_PreEffectTex, i.uv);
		float4 mask = tex2D(_MaskTex, i.uv);
		float stencilMask = tex2D(_StencilMask, TRANSFORM_TEX(i.uv, _StencilMask)).a;

		float4 finalColor = float4(1,1,1,1);
		finalColor.rgb = (preEffectColor.rgb * mask.rgb) + (postEffectColor.rgb * (1 - mask.a));

		finalColor = lerp(finalColor, preEffectColor, stencilMask);

		return finalColor;
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
			#pragma fragment frag
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
			#pragma fragment fragStencil
			#pragma fragmentoption ARB_precision_hint_fastest

			ENDCG
		}
	}

	Fallback off
}