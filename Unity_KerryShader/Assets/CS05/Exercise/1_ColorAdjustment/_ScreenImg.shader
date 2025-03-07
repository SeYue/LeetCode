Shader "Hidden/_ScreenImg"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
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

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 screen_pos :TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_HDR;	// 如果是HDR贴图,需要解码

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex); // 得出的是裁剪空间的屏幕坐标
				o.uv = v.texcoord;
				// 图像是颠倒的,做下翻转
				o.screen_pos = o.pos;
				o.screen_pos.y = o.screen_pos.y * _ProjectionParams.x;
				//o.screen_pos = ComputeScreenPos(o.pos);	// unity中处理跨平台引起的坐标系差异问题
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// 透视除法,范围是(-1,1) 
				half2 screen_uv = i.screen_pos.xy / (i.screen_pos.w + 0.000001);
				screen_uv = (screen_uv + 1) * 0.5;
				half4 col = tex2D(_MainTex, screen_uv);
				col.rgb = DecodeHDR(col, _MainTex_HDR);		// HDR贴图加载出来之后,需要解码
				return col;
			}
			ENDCG
		}
	}
}
