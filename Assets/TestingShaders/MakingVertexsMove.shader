Shader "Unlit/MakingVertexsMove"
{
	Properties {
	}

	SubShader
	{
		Tags { "RenderType" = "Transparent" "DisableBatching" = "True"}

		Pass
		{
			CGPROGRAM
			#pragma vertex vertCol
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata 
			{

				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 color : COLOR;
			};

			v2f vertCol (appdata v)
			{
				v2f o;

				float4x4 mv = UNITY_MATRIX_MV;

				mv._m00 = 1.0f;
				mv._m10 = 0.0f;
				mv._m10 = 0.0f;

				mv._m01 = 0.0f;
				mv._m11 = 1.0f;
				mv._m12 = 0.0f;

				mv._m02 = 0.0f;
				mv._m12 = 0.0f;
				mv._m22 = 1.0f;

				o.pos = mul (UNITY_MATRIX_P, mul(mv, v.vertex));

				o.color = v.color;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{

				return i.color;

			}

			ENDCG
		}

	}




}
