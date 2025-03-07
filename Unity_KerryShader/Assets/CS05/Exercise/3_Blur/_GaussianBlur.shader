Shader "Hidden/_GaussinBlur"
{
    // д������,�ͱ�ʾͨ�ô����
    CGINCLUDE
    
    #include "UnityCG.cginc"

    sampler2D _MainTex;
    // Ǳ����,�������������֮�󣬾��ܻ�ȡ��������ͼ�Ŀ�߱�
    // x = 1 / width
    // y = 1 / height
    // z = width, w = height
    //float4 _MainTex_TexelSize;  
    float4 _BlurOffset;
    
    // ˮƽģ���׶�
    fixed4 frag_HorizontalBlur(v2f_img i) : SV_Target
    {
        // ��˹ģ���������׶�,һ����ˮƽģ����һ���Ǵ�ֱģ������Ϊ��˹ģ��ԭ����25�β���̫���ˣ��ĳ�����
        half2 uv1 = i.uv + _BlurOffset.xy * half2(-2,0);
        half2 uv2 = i.uv + _BlurOffset.xy * half2(-1,0);
        half2 uv3 = i.uv;
        half2 uv4 = i.uv + _BlurOffset.xy * half2( 1,0);
        half2 uv5 = i.uv + _BlurOffset.xy * half2( 2,0);

        // ������ɫ
        half4 s = 0;
        s += tex2D(_MainTex, uv1) * 0.05;  // ����
        s += tex2D(_MainTex, uv2) * 0.25;  // ����
        s += tex2D(_MainTex, uv3) * 0.40;  // ����
        s += tex2D(_MainTex, uv4) * 0.25;  // ����
        s += tex2D(_MainTex, uv4) * 0.05;  // ����
        return s;
    }

    // ��ֱģ���׶�
    fixed4 frag_VerticalBlur(v2f_img i) : SV_Target
    {
        // ��˹ģ���������׶�,һ����ˮƽģ����һ���Ǵ�ֱģ������Ϊ��˹ģ��ԭ����25�β���̫���ˣ��ĳ�����
        half2 uv1 = i.uv + _BlurOffset.xy * half2(0, 2);
        half2 uv2 = i.uv + _BlurOffset.xy * half2(0, 1);
        half2 uv3 = i.uv;
        half2 uv4 = i.uv + _BlurOffset.xy * half2(0,-1);
        half2 uv5 = i.uv + _BlurOffset.xy * half2(0,-2);

        // ������ɫ
        half4 s = 0;
        s += tex2D(_MainTex, uv1) * 0.05;  // ����
        s += tex2D(_MainTex, uv2) * 0.25;  // ����
        s += tex2D(_MainTex, uv3) * 0.40;  // ����
        s += tex2D(_MainTex, uv4) * 0.25;  // ����
        s += tex2D(_MainTex, uv4) * 0.05;  // ����
        return s;
    }

    ENDCG

    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurOffset ("BlurOffset", float) = 1   // ģ����ƫ��ֵ
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
