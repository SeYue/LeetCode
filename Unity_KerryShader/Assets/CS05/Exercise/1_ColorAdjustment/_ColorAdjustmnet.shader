Shader "Hidden/_BrokenGlass"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Brightness("Brightness", float) = 1
		_Saturation("Saturation", float) = 1
		_Contrast("_Contrast", float) = 1
		_vignetteIntensity("_vignetteIntensity", Range(0.05,3)) = 1.5
		_VignetteRoundness("_VignetteRoundness", Range(0.05,5)) = 5
		_VignetteSmoothness("_VignetteSmoothness", Range(0,10)) = 1	// 光滑度

		_HueShift("_HueShift", Range(0,1)) = 0
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float _Brightness;
			float _Saturation;
			float _Contrast;
			float _vignetteIntensity;
			float _VignetteRoundness;
			float _VignetteSmoothness;
			float _HueShift;			

			float3 HSVToRGB(float3 c)
			{
				float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
				return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
			}

			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
				float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
				float d = q.x - min(q.w, q.y);
				float e = 1.0e-10;
				return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			half4 frag(v2f_img i) : SV_Target
			{
				half4 col = tex2D(_MainTex, i.uv);
				half3 finalcolor = col.rgb;
				// 色相
				float3 hsv= RGBToHSV(finalcolor);
				hsv.r = hsv.r + _HueShift;	// r通道就是色相值;
				finalcolor = HSVToRGB(hsv);

				// 亮度
				finalcolor = finalcolor * _Brightness;
				// 饱和度
				float3 lumin = dot(finalcolor, float3(0.22, 0.707, 0.071));	// Gamma空间
				//float lumin = dot(finalcolor, float3(0.0396, 0.458, 0.071))	// 线性空间
				finalcolor = lerp(lumin, finalcolor, _Saturation);
				// 对比度
				float3 midpoint = float3(0.5, 0.5, 0.5);
				finalcolor = lerp(midpoint, finalcolor, _Contrast);
				// 暗角/晕影
				// 第一版
				// i.uv : (0,0)~(1,1)
				// i.uv - half2(0.5,0.5) : 把uv坐标转换到(-0.5,-0.5)~(0.5,0.5)
				// *5.0 让效果更集中
				float2 d = abs(i.uv - half2(0.5, 0.5));
				d *= _vignetteIntensity;	// 转换前的图形是平滑的，乘上一个强度之后，就变成曲线的了,就是一个区域
				d = pow(saturate(d), _VignetteRoundness);
				float dist = length(d);	// 求二维向量d的模长,在屏幕上表现为绘制一个圆
				// 做反向处理
				float vFactor = pow(saturate(1 - dist*dist), _VignetteSmoothness);	// 求出暗角的衰减值

				// 第二版
				//vFactor = 1.0 - length(i.uv - half2(0.5, 0.5));

				finalcolor = finalcolor * vFactor;

				return float4(finalcolor, col.a);
			}

			ENDCG
		}
	}
}
