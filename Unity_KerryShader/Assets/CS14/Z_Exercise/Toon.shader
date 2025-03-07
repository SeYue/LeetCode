Shader "Unlit/Toon"
{
	Properties
	{
		_BaseMap("Base Map", 2D) = "white"{}
		_SSSMap("SSS Map", 2D) = "black" {}

		_ToonThresHold("Toon阈值", Range(0, 1)) = 0.5
		_ToonHardness("Toon过度区域生硬度", float) = 20
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile fwdbase

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord0 : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float3 normal : NORMAL;
				float4 color : COLOR;	// 顶点颜色
			};

			struct v2f
			{
				float4 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
				float3 pos_world : TEXCOORD1;
				float3 normal_world : TEXCOORD2;
			};

			sampler2D _BaseMap;
			sampler2D _SSSMap;
			
			half _ToonThresHold;
			half _ToonHardness;

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.pos_world = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.normal_world = UnityObjectToWorldNormal(v.normal);
				o.uv = float4(v.texcoord0, v.texcoord1);
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				half2 uv1 = i.uv.xy;
				half2 uv2 = i.uv.zw;
				half4 base_map = tex2D(_BaseMap, uv1);
				half4 sss_map = tex2D(_SSSMap, uv1);

				// 1.NDOtL
				float3 normalDir = normalize(i.normal_world);
				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);				
				half NDotL = dot(normalDir, lightDir);	
				half half_lambert = (NDotL + 1) * 0.5;	// 把NDotL的范围从(-1,1),缩放至(0,1)

				// 2.色阶化				
				half offsetLightPos = half_lambert - _ToonThresHold;	// 偏移光照位置
				half toon_diffuse = saturate(offsetLightPos * _ToonHardness);
				return toon_diffuse.xxxx;
			}
			ENDCG
		}
	}
}
