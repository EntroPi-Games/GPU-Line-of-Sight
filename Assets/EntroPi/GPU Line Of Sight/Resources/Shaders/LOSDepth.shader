Shader "Hidden/Line Of Sight Depth"
{
	Category
	{
		Fog { Mode Off }

		SubShader
		{
			Tags { "RenderType"="Opaque" }

			Pass
			{
				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#pragma target 3.0
				#pragma exclude_renderers opengl
				#include "UnityCG.cginc"

				struct v2f
				{
					float4 pos : POSITION;
					float3 viewPos : TEXCOORD0;
				};

				v2f vert( appdata_base v )
				{
					v2f o;

					float4 position = v.vertex;
					position.w = 1.f;

					o.pos = mul(UNITY_MATRIX_MVP, position);
					o.viewPos = mul(UNITY_MATRIX_MV, position).xyz;

					return o;
				}

				float4 frag(v2f i) : COLOR
				{
					float fDepth = length(i.viewPos.xyz);

					// Compute partial derivatives of depth.
					float dx = ddx(fDepth);
					float dy = ddy(fDepth);

					// Compute second moment over the pixel extents.
					float moment = fDepth * fDepth + 0.25 * (dx * dx + dy * dy);

					return float4(fDepth,moment, 0, 0);
				}

				ENDCG
			}
		}

		//OpenGL Version
		SubShader
		{
			Tags { "RenderType"="Opaque" }

			Pass
			{
				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#pragma only_renderers opengl
				#include "UnityCG.cginc"

				struct v2f
				{
					float4 pos : POSITION;
					float3 viewPos : TEXCOORD0;
				};

				v2f vert( appdata_base v )
				{
					v2f o;

					float4 position = v.vertex;
					position.w = 1.f;

					o.pos = mul(UNITY_MATRIX_MVP, position);
					o.viewPos = mul(UNITY_MATRIX_MV, position).xyz;

					return o;
				}

				float4 frag(v2f i) : COLOR
				{
					float fDepth = length(i.viewPos.xyz);

					float moment = fDepth * fDepth;
					return float4(fDepth,moment, 0, 0);
				}
				ENDCG
			}
		}
	}

	Fallback Off
}