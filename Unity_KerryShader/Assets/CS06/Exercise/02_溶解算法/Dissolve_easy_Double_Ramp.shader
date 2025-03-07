// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Dissolve_easy"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_MainTex("MainTex", 2D) = "white" {}
		_Gradient("Gradient", 2D) = "white" {}
		_ChangeAmount("ChangeAmount", Range( 0 , 1)) = 0.5
		_EdgeWidth("EdgeWidth", Range( 0 , 2)) = 0.3927059
		_EdgeColorIntensity("EdgeColorIntensity", Range( 0 , 5)) = 1.44966
		[Toggle(_MANNULCONTROL_ON)] _MANNULCONTROL("MANNULCONTROL", Float) = 0
		_Spread("Spread", Range( 0 , 1)) = 0
		_Noise("Noise", 2D) = "white" {}
		_RampTex("RampTex", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _MANNULCONTROL_ON
		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform sampler2D _RampTex;
		uniform sampler2D _Gradient;
		SamplerState sampler_Gradient;
		uniform float4 _Gradient_ST;
		uniform float _ChangeAmount;
		uniform float _Spread;
		uniform sampler2D _Noise;
		SamplerState sampler_Noise;
		uniform float _EdgeWidth;
		uniform float _EdgeColorIntensity;
		SamplerState sampler_MainTex;
		uniform float _Cutoff = 0.5;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 tex2DNode1 = tex2D( _MainTex, uv_MainTex );
			float2 uv_Gradient = i.uv_texcoord * _Gradient_ST.xy + _Gradient_ST.zw;
			float mulTime30 = _Time.y * 0.2;
			#ifdef _MANNULCONTROL_ON
				float staticSwitch35 = _ChangeAmount;
			#else
				float staticSwitch35 = frac( mulTime30 );
			#endif
			float Gradient24 = ( ( ( tex2D( _Gradient, uv_Gradient ).r - (-_Spread + (staticSwitch35 - 0.0) * (1.0 - -_Spread) / (1.0 - 0.0)) ) / _Spread ) * 2.0 );
			float2 panner43 = ( 1.0 * _Time.y * float2( 0,0 ) + i.uv_texcoord);
			float Noise45 = tex2D( _Noise, panner43 ).r;
			float temp_output_47_0 = ( Gradient24 - Noise45 );
			float clampResult23 = clamp( ( 1.0 - ( distance( temp_output_47_0 , 0.5 ) / _EdgeWidth ) ) , 0.0 , 1.0 );
			float2 appendResult56 = (float2(( 1.0 - clampResult23 ) , 0.5));
			float4 RampColor58 = tex2D( _RampTex, appendResult56 );
			float4 lerpResult19 = lerp( tex2DNode1 , ( RampColor58 * _EdgeColorIntensity ) , clampResult23);
			o.Emission = lerpResult19.rgb;
			o.Alpha = 1;
			clip( ( tex2DNode1.a * step( 0.5 , temp_output_47_0 ) ) - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18500
183;775;947;560;-425.8471;-741.551;1;False;False
Node;AmplifyShaderEditor.CommentaryNode;36;-3345.526,343.5571;Inherit;False;2111.12;733.6765;Comment;13;24;39;5;2;9;38;35;33;37;4;30;34;49;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-3269.656,525.5834;Inherit;False;Constant;_Float1;Float 1;7;0;Create;True;0;0;False;0;False;0.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;30;-3096.074,531.8544;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-3016.264,776.3847;Inherit;False;Property;_ChangeAmount;ChangeAmount;3;0;Create;True;0;0;False;0;False;0.5;0.409;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;37;-2801.222,936.806;Inherit;False;Property;_Spread;Spread;7;0;Create;True;0;0;False;0;False;0;0.305;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;33;-2898.081,531.933;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;38;-2478.156,828.3433;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;35;-2642.585,595.9456;Inherit;True;Property;_MANNULCONTROL;MANNULCONTROL;6;0;Create;True;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;9;-2241.021,600.9101;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-2257.93,393.5571;Inherit;True;Property;_Gradient;Gradient;2;0;Create;True;0;0;False;0;False;-1;1f607203268ce664d80ad830741ba316;04c4d464f0fe2794fac80b76bd02866e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;5;-1905.365,505.5994;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;42;-2167.686,1237.555;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;44;-2144.686,1430.555;Inherit;False;Constant;_NoiseSpeed;NoiseSpeed;10;0;Create;True;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleDivideOpNode;39;-1702.047,791.6573;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-1633.075,1027.565;Inherit;False;Constant;_Float2;Float 2;10;0;Create;True;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;43;-1883.686,1238.555;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;-1466.474,903.7656;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;41;-1673.686,1212.555;Inherit;True;Property;_Noise;Noise;8;0;Create;True;0;0;False;0;False;-1;None;1f607203268ce664d80ad830741ba316;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;45;-1372.17,1230.88;Inherit;False;Noise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;24;-1311.349,903.6183;Inherit;False;Gradient;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;40;-1098.881,483.6004;Inherit;True;24;Gradient;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;28;-365.2409,822.3701;Inherit;False;1219.079;405.5975;EdgeColor;6;14;16;13;15;17;23;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;46;-1101.229,686.2968;Inherit;True;45;Noise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-315.2409,992.5103;Inherit;False;Constant;_Float0;Float 0;4;0;Create;True;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;47;-790.5911,511.0392;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;13;-34.05495,878.2367;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-86.27882,1111.968;Inherit;False;Property;_EdgeWidth;EdgeWidth;4;0;Create;True;0;0;False;0;False;0.3927059;2;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;15;193.632,882.7817;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;17;408.9247,887.7577;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;23;597.2391,883.6667;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;60;841.847,1057.551;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;56;904.5173,885.9576;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;57;1091.241,858.5802;Inherit;True;Property;_RampTex;RampTex;10;0;Create;True;0;0;False;0;False;-1;None;4e150b790e49c8b4f893fed0f222ad37;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;58;1473.705,890.2682;Inherit;False;RampColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;59;399.0934,-7.230462;Inherit;False;58;RampColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;21;305.5763,77.89079;Inherit;False;Property;_EdgeColorIntensity;EdgeColorIntensity;5;0;Create;True;0;0;False;0;False;1.44966;5;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;675.8113,6.854207;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;1;-28.01214,-196.3435;Inherit;True;Property;_MainTex;MainTex;1;0;Create;True;0;0;False;0;False;-1;0b06a95f0a779f24ab2a475a87d0a6c9;0b06a95f0a779f24ab2a475a87d0a6c9;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;7;-426.9285,489.0078;Inherit;True;2;0;FLOAT;0.5;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;19;936.1311,-60.65208;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;54;434.8628,-228.0952;Inherit;False;Property;_Color0;Color 0;9;1;[HDR];Create;True;0;0;False;0;False;0,0,0,0;2,0.2588235,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-201.0546,493.0352;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1729.28,-48.57862;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Dissolve_easy;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Opaque;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;30;0;34;0
WireConnection;33;0;30;0
WireConnection;38;0;37;0
WireConnection;35;1;33;0
WireConnection;35;0;4;0
WireConnection;9;0;35;0
WireConnection;9;3;38;0
WireConnection;5;0;2;1
WireConnection;5;1;9;0
WireConnection;39;0;5;0
WireConnection;39;1;37;0
WireConnection;43;0;42;0
WireConnection;43;2;44;0
WireConnection;49;0;39;0
WireConnection;49;1;48;0
WireConnection;41;1;43;0
WireConnection;45;0;41;1
WireConnection;24;0;49;0
WireConnection;47;0;40;0
WireConnection;47;1;46;0
WireConnection;13;0;47;0
WireConnection;13;1;14;0
WireConnection;15;0;13;0
WireConnection;15;1;16;0
WireConnection;17;0;15;0
WireConnection;23;0;17;0
WireConnection;60;0;23;0
WireConnection;56;0;60;0
WireConnection;57;1;56;0
WireConnection;58;0;57;0
WireConnection;20;0;59;0
WireConnection;20;1;21;0
WireConnection;7;1;47;0
WireConnection;19;0;1;0
WireConnection;19;1;20;0
WireConnection;19;2;23;0
WireConnection;3;0;1;4
WireConnection;3;1;7;0
WireConnection;0;2;19;0
WireConnection;0;10;3;0
ASEEND*/
//CHKSM=9EF06064F4E1C19B393086E6038FA606AA814A1C