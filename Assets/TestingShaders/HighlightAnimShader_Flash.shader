// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Unlit/HighlightAnimShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_HighlightTex ("Highlight Sprite", 2D) = "white" {}
		_FlashTime ("Flash Time", float) = 1.0
		_Brightness("Brightness (bias fraction)", float) = 0.25

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
			uniform fixed4 _TintColor;
			uniform float _FlashTime;
			uniform float _Brightness;
			
			FragInput vert (VertexInput v)
			{
				FragInput o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);

				o.uv = v.uv;
				o.uv1 = v.uv1;

				return o;
			}
			
			fixed4 frag (FragInput i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);

				fixed4 secondCol = tex2D(_HighlightTex, i.uv1);


				float tintFlash = ((sin(_Time.y * _FlashTime) * _Brightness) + _Brightness);

				fixed4 finalCol;
				finalCol.rgb = lerp(col.rgb, col.rgb * tintFlash * _TintColor, secondCol.a);
				finalCol.a = col.a;

				return finalCol;
			}
			ENDCG
		}
	}
}
