Shader "Hidden/Line Of Sight Stencil Mask" 
{
	SubShader
	{

		Pass 
		{
			Cull Off
			ZTest Always
			ZWrite Off

			Stencil
			{
				ReadMask 1
				Ref 1
				Comp Equal
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct VertexInput
			{
				float4 vertex : POSITION;
			};

			struct VertexOutput
			{
				float4 pos : SV_POSITION;
			};

			VertexOutput vert(VertexInput v)
			{
				VertexOutput o = (VertexOutput)0;
				
				o.pos = v.vertex * float4(2, 2, 0, 0) + float4(0, 0, 0, 1);
				
				return o;
			}
			
			float4 frag(VertexOutput i) : COLOR
			{
				return float4(1,1,1,1);
			}
			
			ENDCG
		}
	}
}