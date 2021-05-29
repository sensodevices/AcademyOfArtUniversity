
Shader "Unlit/HighlightAnimShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_HighlightTex ("Highlight Sprite", 2D) = "white" {}
		_RotationSpeed ("Rotation Speed", Float) = 2.0

		_TintColor("Tint Color", Color) = (1.0, 1.0, 1.0, 1.0)
	}
	SubShader
	{
		Tags 
		{ 
			"Queue"="Transparent" 
			"RenderType"="Transparent" 

		}
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha


		Pass
		{


			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			
			#include "UnityCG.cginc"

			struct VertexInput
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD0;
			};

			struct FragInput
			{

				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float4 color : COLOR0;
			};

			uniform sampler2D _MainTex;
			uniform sampler2D _HighlightTex;
			uniform float _RotationSpeed;
			uniform fixed4 _TintColor;
			
			FragInput vert (VertexInput v)
			{
				FragInput o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

				float sinX = sin ( _RotationSpeed * _Time.y) ;
				float cosX = cos ( _RotationSpeed * _Time.y );
				float sinY = sin ( _RotationSpeed * _Time.y );
				float2x2 rotationMatrix = float2x2 ( cosX, -sinX, sinY, cosX);

				//v.uv1.xy = v.uv1.xy - 0.5;
				rotationMatrix = (rotationMatrix* 0.5) + 0.5;
				rotationMatrix = (rotationMatrix * 2) - 1;

				v.uv1.xy -= 0.5;
				o.uv1.xy = mul ( v.uv1.xy, rotationMatrix);
				o.uv1.xy -= 0.5;


				//o.uv.xy = v.uv.xy + frac(_Time.y * float2(0.1, 0.1));
				//o.uv1.xy = v.uv1.xy + frac(_Time.y * float2(-0.8, 0));
				o.uv = v.uv;

				return o;
			}
			
			fixed4 frag (FragInput i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				col *= _TintColor;

				fixed4 secondCol = tex2D(_HighlightTex, i.uv1);


				fixed4 combineCol = lerp(col, secondCol, col.a);
				fixed4 combineAlpha = (combineCol.r,combineCol.g, combineCol.b, lerp(col.a, secondCol.a , col.a));


				col += combineAlpha;
				return col;
			}
			ENDCG
		}
	}
}
