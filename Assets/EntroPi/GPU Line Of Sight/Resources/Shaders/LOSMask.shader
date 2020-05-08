Shader "Hidden/Line Of Sight Mask"
{
	CGINCLUDE

		#include "/LOSInclude.cginc"

		// Samplers
		uniform sampler2D _SourceDepthTex;
		uniform sampler2D _CameraDepthNormalsTexture;

		// For fast world space reconstruction
		uniform float4x4 _FrustumRays;
		uniform float4x4 _FrustumOrigins;
		uniform float4x4 _SourceWorldProj;
		uniform float4x4 _WorldToCameraMatrix;

		uniform float4 _SourceInfo; // xyz = source position, w = source far plane
		uniform float4 _ColorMask;
		uniform float4 _Settings; // x = distance fade, y = edge fade, z = min variance, w = backface fade
		uniform float4 _Flags; // x = clamp out of bound pixels, y = include / exclude out of bound pixels, z = invert mask, w = exclude backfaces
		uniform float4 _MainTex_TexelSize;
		
		v2f_img_ray Vert( appdata_img v )
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

		float CalculateBackfaceFade(float4 pixelWorldPos, float3 pixelViewNormals)
		{
			float3 directionWorld = normalize(pixelWorldPos - _SourceInfo.xyz );
			float3 directionView = mul((float3x3)_WorldToCameraMatrix, directionWorld);

			float backfaceFade = dot(directionView, pixelViewNormals);
			backfaceFade = smoothstep(0, -_Settings.w, backfaceFade);

			return backfaceFade;
		}

		float CalculateVisibility(float4 pixelWorldPos, float3 pixelViewNormals)
		{
			// Calculate distance to source in range[0 - far plane]
			float sourceDistance = distance(pixelWorldPos.xyz, _SourceInfo.xyz);

			// Convert world space to LOS cam depth texture UV's
			float4 sourcePos = mul(_SourceWorldProj, pixelWorldPos);
			float3 sourceNDC = sourcePos.xyz / sourcePos.w;

			// Clip pixels outside of source
			clip(max(min(sourcePos.w, 1 - abs(sourceNDC.x)), _Flags.z - 0.5));

			// Convert from NDC to UV
			float2 sourceUV = sourceNDC.xy;
			sourceUV *= 0.5f;
			sourceUV += 0.5f;

			// VSM
			float2 moments = tex2D(_SourceDepthTex, sourceUV).rg;
			float visible = ChebyshevUpperBound(moments, sourceDistance, _Settings.z);

			// Backface Fade
			float backfaceFade = CalculateBackfaceFade(pixelWorldPos, pixelViewNormals);
			visible *= lerp(1, backfaceFade, _Flags.w);

			// Handle vertical out of bound pixels
			visible += _Flags.x * _Flags.y * (1 - step(abs(sourceNDC.y), 1.0));
			visible = saturate(visible);

			// Ignore pixels behind source
			visible *= step(-sourcePos.w, 0);

			// Calculate fading
			float edgeFade = CalculateFade(abs(sourceNDC.x), _Settings.y);
			float distanceFade = CalculateFade(sourceDistance / _SourceInfo.w, _Settings.x);

			// Apply fading
			visible *= distanceFade;
			visible *= edgeFade;

			return visible;
		}

		float4 GenerateMask(float visible)
		{
			// Invert visibility if needed
			if(_Flags.z > 0.0)
			{
				visible = 1 - visible;
			}

			// Apply mask color
			float4 mainColor = visible * _ColorMask;

			return mainColor;
		}

		half4 Frag (v2f_img_ray i) : COLOR
		{
			float4 normalDepth = SampleAndDecodeDepthNormal(_CameraDepthNormalsTexture, i.uv);
			float4 positionWorld = DepthToWorldPosition(normalDepth.w, i.interpolatedRay, i.interpolatedOrigin);
			float visible = CalculateVisibility(positionWorld, normalDepth.xyz);

			return GenerateMask(visible);
		}

	ENDCG

	SubShader
	{
		Pass
		{
			ZTest Always
			ZWrite Off
			Cull Off
			Blend One One

			Fog { Mode off }

			CGPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag
			#pragma fragmentoption ARB_precision_hint_nicest
			#pragma exclude_renderers flash
			#pragma target 3.0

			ENDCG
		}
	}

	Fallback off
}