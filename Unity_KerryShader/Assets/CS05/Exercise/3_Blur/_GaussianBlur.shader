Shader "Hidden/_GaussinBlur"
{
    // 写在这里,就表示通用代码库
    CGINCLUDE
    
    #include "UnityCG.cginc"

    sampler2D _MainTex;
    // 潜规则,声明了这个变量之后，就能获取到这张贴图的宽高比
    // x = 1 / width
    // y = 1 / height
    // z = width, w = height
    //float4 _MainTex_TexelSize;  
    float4 _BlurOffset;
    
    // 水平模糊阶段
    fixed4 frag_HorizontalBlur(v2f_img i) : SV_Target
    {
        // 高斯模糊分两个阶段,一个是水平模糊，一个是垂直模糊，因为高斯模糊原来的25次采样太费了，改成这样
        half2 uv1 = i.uv + _BlurOffset.xy * half2(-2,0);
        half2 uv2 = i.uv + _BlurOffset.xy * half2(-1,0);
        half2 uv3 = i.uv;
        half2 uv4 = i.uv + _BlurOffset.xy * half2( 1,0);
        half2 uv5 = i.uv + _BlurOffset.xy * half2( 2,0);

        // 采样颜色
        half4 s = 0;
        s += tex2D(_MainTex, uv1) * 0.05;  // 左上
        s += tex2D(_MainTex, uv2) * 0.25;  // 左下
        s += tex2D(_MainTex, uv3) * 0.40;  // 右上
        s += tex2D(_MainTex, uv4) * 0.25;  // 右下
        s += tex2D(_MainTex, uv4) * 0.05;  // 右下
        return s;
    }

    // 垂直模糊阶段
    fixed4 frag_VerticalBlur(v2f_img i) : SV_Target
    {
        // 高斯模糊分两个阶段,一个是水平模糊，一个是垂直模糊，因为高斯模糊原来的25次采样太费了，改成这样
        half2 uv1 = i.uv + _BlurOffset.xy * half2(0, 2);
        half2 uv2 = i.uv + _BlurOffset.xy * half2(0, 1);
        half2 uv3 = i.uv;
        half2 uv4 = i.uv + _BlurOffset.xy * half2(0,-1);
        half2 uv5 = i.uv + _BlurOffset.xy * half2(0,-2);

        // 采样颜色
        half4 s = 0;
        s += tex2D(_MainTex, uv1) * 0.05;  // 左上
        s += tex2D(_MainTex, uv2) * 0.25;  // 左下
        s += tex2D(_MainTex, uv3) * 0.40;  // 右上
        s += tex2D(_MainTex, uv4) * 0.25;  // 右下
        s += tex2D(_MainTex, uv4) * 0.05;  // 右下
        return s;
    }

    ENDCG

    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurOffset ("BlurOffset", float) = 1   // 模糊的偏移值
    }
    SubShader
    {
        Cull Off 
        ZWrite Off 
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_HorizontalBlur
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_VerticalBlur
            ENDCG
        }
    }
}
