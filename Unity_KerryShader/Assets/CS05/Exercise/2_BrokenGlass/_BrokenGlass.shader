Shader "Hidden/_ColorAdjustment"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		// ��������ͼ��
		_GlassMask("GlassMask", 2D) = "black"{}
		_GlassCrack("GlassCrack", Float) = 1
		// ����ͼ
		_GlassNormal("GlassNormal", 2D) = "bump" {}
		_Distort("Distort", Float) = 1
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
			sampler2D _GlassMask;
			float4 _GlassMask_ST;
			float _GlassCrack;
			sampler2D _GlassNormal;
			float _Distort;

			half4 frag(v2f_img i) : SV_Target
			{
				half2 glass_uv = i.uv * _GlassMask_ST.xy + _GlassMask_ST.zw;
				
				// ��߱�
				// _ScreenParams.x ��Ļ�Ŀ��
				// _ScreenParams.y ��Ļ�ĸ߶�
				float aspect = _ScreenParams.x / _ScreenParams.y;
				glass_uv.x = glass_uv.x * aspect;

				// ����glass
				half glass_opacity = tex2D(_GlassMask, glass_uv).r;
				half3 glass_normal = UnpackNormal(tex2D(_GlassNormal, glass_uv));
				
				half2 d = abs(i.uv * 2.0 - 1.0);
				d = smoothstep(0.95, 1, d);	// ��dֵ������[0,1]��
				d = 1 - d;	// ����
				half vfactor = d.x * d.y;

				// ����mask
				float2 d_mask = step(0.005, abs(glass_normal.xy));
				float mask = d_mask.x * d_mask.y;

				// �÷���ͼ��uv��һ��Ť��
				half2 uv_distort = i.uv + glass_normal.xy * _Distort * vfactor * mask;
				half4 col = tex2D(_MainTex, uv_distort);
				half3 finalcolor = col.rgb;
				finalcolor = lerp(finalcolor, _GlassCrack.xxx, glass_opacity);
				return float4(finalcolor, col.a);
			}

			ENDCG
		}
	}
}
