// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/RayMarchExample"
{

	Properties
	{
		_Iterations ("Iterations", Range(0,200)) = 100
		_ViewDistance ("ViewDistance", Range(0, 5)) = 2
		_SkyColor ("SkyColor", Color) = (0.176, 0.478, 0.871, 1)
		_CloudColor("Cloud Color", Color) = (1,1,1,1)
		_CloudsDensity("Cloud Density", Range(0, 1)) = 0.5

		_NoiseTex ("Noise Texture", 2D) = "white" {}
	}

	SubShader
	{

		Pass
		{
			Blend SrcAlpha Zero

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			//global vars
			sampler2D _NoiseTex;
			float3 _CamPosition;
			float3 _CamUp;
			float3 _CamRight;
			float3 _CamForward;

			float _AspectRatio;
			float _FieldOfView;

			//local vars
			int _Iterations;
			float3 _SkyColor;
			float4 _CloudColor;
			float _ViewDistance;
			float _CloudDensity;


			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata_full v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;

				return o;
			}

			float noise (float3 x)
			{
				x *= 4.0;
				float3 p = floor(x);
				float3 f = frac(x);
				f = f*f*(3.0-2.0*f);
				float2 uv = (p.xy + float2(37.0, 17.0)*p.z) + f.xy;
				float2 rg = tex2D(_NoiseTex, (uv + 0.5 ) / 512.0).yx;

				return lerp(rg.x, rg.y, f.z);
			}

			float fbm (float3 pos, int octaves)
			{
				float f = 0.0;
				for (int i = 0; i < octaves; i++)
				{	
					f += noise(pos) / pow(2, i+1);
					pos *= 2.01;
				}

				f /= 1 - (1 / pow(2, octaves+1));

				return f;
			}


			//float distFunc(float3 pos)
			//{
			//	float sphereRadius = 1;
			//	return length(pos) - sphereRadius;
			//}

			//fixed4 renderSurface(float3 pos)
			//{
			//	const float2 eps = float2(0.0, 0.01);
			//
			//	float ambientIntensity = 0.5;
			//	float3 lightDir = float3(0, -0.5, 0.5);
			//
			//	float3 normal = normalize(float3(distFunc(pos + eps.yxx) - distFunc(pos - eps.yxx), distFunc(pos + eps.xyx) - distFunc(pos - eps.xyx), distFunc(pos + eps.xxy) - distFunc(pos - eps.xxy)));
			//
			//	float diffuse = ambientIntensity + max(0, dot(-lightDir, normal));
			//
			//	return float4(diffuse, diffuse, diffuse, 1);
			//
			//}


			fixed4 frag (v2f i) : SV_Target
			{

				float2 uv = (i.uv - 0.5) * _FieldOfView;
				uv.x *= _AspectRatio;

				float3 ray = _CamUp * uv.y + _CamRight * uv.x + _CamForward;

				float3 pos = _CamPosition * 4;
				float3 p = pos;
				float density = 0.0;




				for (int i = 0; i < _Iterations; i++)
				{
					float f = i / _Iterations;

					float alpha = smoothstep(0, _Iterations * 0.2, i) * (1 - f) * (1 - f);

					float denseClouds = smoothstep(_CloudDensity, 0.75, fbm(p, 5));
					float lightClouds = (smoothstep(-0.2, 1.2, fbm(p * 2, 2)) - 0.5) * 0.5;

					density += (lightClouds + denseClouds) * alpha;

					p = pos + ray * f * _ViewDistance;




				}

				float3 color = _SkyColor + (_CloudColor.rgb - 0.5) * (density / _Iterations) * 20 * _CloudColor.a;

				return fixed4(color, 1);
			}




			ENDCG
		}
	}
}
