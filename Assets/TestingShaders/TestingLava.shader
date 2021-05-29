// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Unlit/TestingLaca"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ExtraCol ("Color", Color) = (1,1,1,1)
		_Color ( "Diffuse something", Color) = (1.0, 1.0, 1.0, 1.0)
		_SpecColor ( "Specular Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Shine ( "Shininess", Float ) = 10

	}
	SubShader
	{
		Tags { "RenderType" = "Transparent+1" }
		LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{

			Tags { "LightMode" = "ForwardBase" }
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			// make fog work


			
			#include "UnityCG.cginc"

			struct vertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 uv : TEXCOORD0;
				float4 uv1 : TEXCOORD0;
			};

			struct fragmentInput
			{
				float4 vertex : SV_POSITION;
				half2 uv : TEXCOORD0;
				half2 uv1 : TEXCOORD1;
				float4 color : COLOR0;

			};

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform fixed4 _ExtraCol;
			uniform fixed4 _Color;
			uniform fixed4 _LightColor0;
			uniform fixed4 _SpecColor;
			uniform float _Shine;
			
			fragmentInput vert (vertexInput v)
			{
				fragmentInput o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);

				float3 normalDirection = normalize( mul( float4( v.normal, 1.0), unity_WorldToObject ).xyz );

				float3 lightDirection = normalize( _WorldSpaceLightPos0.xyz );

				float attenuation = 1.0 / length( lightDirection );
				lightDirection = normalize( lightDirection );

				float ndotl = dot( normalDirection, lightDirection );
				float3 diffuse = attenuation * _LightColor0.xyz * _Color.rgb * max( 0.0, dot( normalDirection, lightDirection ) );

				o.uv.xy = v.uv.xy + frac(_Time.y * float2(0.1, 0.1));
				o.uv1.xy = v.uv1.xy + frac(_Time.y * float2(-0.1, 0));

				float3 viewDirection = WorldSpaceViewDir( v.vertex );
				float3 specularReflection;

				if( ndotl > 0)
				{
					// Incoming_Light * Material_Specular_Constant * (ReflectedLightDirection * ViewDirection ) ^ Shininess_Constant

					float3 reflection = reflect( -lightDirection, normalDirection );
					float4 rdotv = pow( max(0.0, dot(reflection, viewDirection)), _Shine );
					specularReflection = attenuation * _LightColor0.rgb * _SpecColor.rgb * rdotv;
				}
				else
				{
					specularReflection = float3(0.0, 0.0, 0.0);
				}

				float3 ambientLighting = UNITY_LIGHTMODEL_AMBIENT.rgb * _Color.rgb;

				o.color = float4(ambientLighting + diffuse + specularReflection, 1.0);

				//o.color = half4 ( diffuse, 1.0);
				return o;
			}
			
			fixed4 frag (fragmentInput i) : SV_Target
			{
				// sample the texture
				half4 col = tex2D(_MainTex, i.uv);
				half4 secondCol = tex2D(_MainTex, i.uv1);
				half4 combineCol = lerp(col.rgba, _ExtraCol.rgba, secondCol.rgba);



				return combineCol * i.color;
			}
			ENDCG
		}


	}
}
