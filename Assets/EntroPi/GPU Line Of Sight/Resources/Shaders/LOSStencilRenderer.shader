Shader "Hidden/LOS Stencil Renderer"
{
	SubShader
	{
		Tags { "DisableBatching " = "True" }
		
		ColorMask 0
		Pass
		{
			ZWrite Off			

			Stencil
			{
				WriteMask 1
				Ref 1
				Comp always
				Pass replace
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;

				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				return float4(1,1,1,1);
			}

			ENDCG
		}
	}
}