// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Dissolve_easy"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_ChangeAmount("ChangeAmount", Range( 0 , 1)) = 0.5
		[HDR]_EdgeColor("EdgeColor", Color) = (1,0.8126906,0,0)
		_EdgeWidth("EdgeWidth", Range( 0.02 , 2)) = 0.3927059
		_EdgeColorIntensity("EdgeColorIntensity", Range( 0 , 5)) = 1.44966
		[Toggle(_MANNULCONTROL_ON)] _MANNULCONTROL("MANNULCONTROL", Float) = 0
		_Spread("Spread", Range( 0 , 1)) = 0
		_Noise("Noise", 2D) = "white" {}
		_ObjectScale("ObjectScale", Float) = 0
		[Toggle(_DIR_INV_ON)] _DIR_INV("DIR_INV", Float) = 0
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
		#pragma shader_feature_local _DIR_INV_ON
		#pragma shader_feature_local _MANNULCONTROL_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform float4 _EdgeColor;
		uniform float _EdgeColorIntensity;
		uniform float _ObjectScale;
		uniform float _ChangeAmount;
		uniform float _Spread;
		uniform sampler2D _Noise;
		SamplerState sampler_Noise;
		uniform float _EdgeWidth;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 temp_cast_0 = (0.18).xxx;
			o.Albedo = temp_cast_0;
			float3 ase_worldPos = i.worldPos;
			float3 objToWorld53 = mul( unity_ObjectToWorld, float4( float3( 0,0,0 ), 1 ) ).xyz;
			float clampResult57 = clamp( ( ( ase_worldPos.y - objToWorld53.y ) / _ObjectScale ) , 0.0 , 1.0 );
			#ifdef _DIR_INV_ON
				float staticSwitch64 = ( 1.0 - clampResult57 );
			#else
				float staticSwitch64 = clampResult57;
			#endif
			float mulTime30 = _Time.y * 0.2;
			#ifdef _MANNULCONTROL_ON
				float staticSwitch35 = _ChangeAmount;
			#else
				float staticSwitch35 = frac( mulTime30 );
			#endif
			float Gradient24 = ( ( ( staticSwitch64 - (-_Spread + (staticSwitch35 - 0.0) * (1.0 - -_Spread) / (1.0 - 0.0)) ) / _Spread ) * 2.0 );
			float2 panner43 = ( 1.0 * _Time.y * float2( 0,0 ) + i.uv_texcoord);
			float Noise45 = ( 1.0 - tex2D( _Noise, panner43 ).r );
			float temp_output_47_0 = ( Gradient24 - Noise45 );
			float clampResult23 = clamp( ( 1.0 - ( distance( temp_output_47_0 , 0.5 ) / _EdgeWidth ) ) , 0.0 , 1.0 );
			o.Emission = ( _EdgeColor * _EdgeColorIntensity * clampResult23 ).rgb;
			o.Alpha = 1;
			clip( step( 0.5 , temp_output_47_0 ) - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18500
203;484;947;490;2784.312;121.5534;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;36;-3345.526,343.5571;Inherit;False;2111.12;733.6765;Comment;12;24;39;5;9;38;35;33;37;4;30;34;49;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TransformPositionNode;53;-3345.063,138.8728;Inherit;False;Object;World;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;52;-3319.667,-18.5932;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;34;-3269.656,525.5834;Inherit;False;Constant;_Float1;Float 1;7;0;Create;True;0;0;False;0;False;0.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;59;-3073.722,141.3852;Inherit;False;Property;_ObjectScale;ObjectScale;10;0;Create;True;0;0;False;0;False;0;3.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;54;-3064.968,-3.955091;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;30;-3096.074,531.8544;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;58;-2874.375,-3.688596;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;33;-2898.081,531.933;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-3016.264,776.3847;Inherit;False;Property;_ChangeAmount;ChangeAmount;2;0;Create;True;0;0;False;0;False;0.5;0.721;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;37;-2801.222,936.806;Inherit;False;Property;_Spread;Spread;7;0;Create;True;0;0;False;0;False;0;0.3;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;57;-2697.83,-5.173213;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;62;-2508.526,99.19515;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;35;-2642.585,595.9456;Inherit;True;Property;_MANNULCONTROL;MANNULCONTROL;6;0;Create;True;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;38;-2478.156,828.3433;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;42;-2167.686,1237.555;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;64;-2293.181,-14.02747;Inherit;False;Property;_DIR_INV;DIR_INV;11;0;Create;True;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;44;-2144.686,1430.555;Inherit;False;Constant;_NoiseSpeed;NoiseSpeed;10;0;Create;True;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TFHCRemapNode;9;-2241.021,600.9101;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;5;-1905.365,505.5994;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;43;-1883.686,1238.555;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;39;-1702.047,791.6573;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-1633.075,1027.565;Inherit;False;Constant;_Float2;Float 2;10;0;Create;True;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;41;-1673.686,1212.555;Inherit;True;Property;_Noise;Noise;8;0;Create;True;0;0;False;0;False;-1;None;8d4bbe4803e00c145abae6ba5ba794bf;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;61;-1325.394,1242.272;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;-1466.474,903.7656;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;24;-1311.349,903.6183;Inherit;False;Gradient;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;45;-1074.17,1230.88;Inherit;False;Noise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;40;-1098.881,483.6004;Inherit;True;24;Gradient;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;28;-365.2409,822.3701;Inherit;False;1219.079;405.5975;EdgeColor;6;14;16;13;15;17;23;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;46;-1101.229,686.2968;Inherit;True;45;Noise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-315.2409,992.5103;Inherit;False;Constant;_Float0;Float 0;4;0;Create;True;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;47;-790.5911,511.0392;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-86.27882,1111.968;Inherit;False;Property;_EdgeWidth;EdgeWidth;4;0;Create;True;0;0;False;0;False;0.3927059;0.39;0.02;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;13;-34.05495,878.2367;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;15;193.632,882.7817;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;17;425.8247,889.0577;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;18;441.7029,97.63387;Inherit;False;Property;_EdgeColor;EdgeColor;3;1;[HDR];Create;True;0;0;False;0;False;1,0.8126906,0,0;0,1.160784,5.992157,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;21;403.0894,280.8188;Inherit;False;Property;_EdgeColorIntensity;EdgeColorIntensity;5;0;Create;True;0;0;False;0;False;1.44966;1.5;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;23;599.8391,892.7666;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;60;1038.523,50.44124;Inherit;False;Constant;_Float3;Float 3;11;0;Create;True;0;0;False;0;False;0.18;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;7;-426.9285,489.0078;Inherit;True;2;0;FLOAT;0.5;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;694.5506,104.7498;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;440.5525,-143.2595;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;1;47.28919,-146.2623;Inherit;True;Property;_MainTex;MainTex;1;0;Create;True;0;0;False;0;False;-1;0b06a95f0a779f24ab2a475a87d0a6c9;b97db8acddac10d4c867939fcd38e487;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;50;136.512,48.00253;Inherit;False;Property;_MainColor;MainColor;9;0;Create;True;0;0;False;0;False;0,0,0,0;0.490566,0.490566,0.490566,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1258.182,59.65186;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Dissolve_easy;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Opaque;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;54;0;52;2
WireConnection;54;1;53;2
WireConnection;30;0;34;0
WireConnection;58;0;54;0
WireConnection;58;1;59;0
WireConnection;33;0;30;0
WireConnection;57;0;58;0
WireConnection;62;0;57;0
WireConnection;35;1;33;0
WireConnection;35;0;4;0
WireConnection;38;0;37;0
WireConnection;64;1;57;0
WireConnection;64;0;62;0
WireConnection;9;0;35;0
WireConnection;9;3;38;0
WireConnection;5;0;64;0
WireConnection;5;1;9;0
WireConnection;43;0;42;0
WireConnection;43;2;44;0
WireConnection;39;0;5;0
WireConnection;39;1;37;0
WireConnection;41;1;43;0
WireConnection;61;0;41;1
WireConnection;49;0;39;0
WireConnection;49;1;48;0
WireConnection;24;0;49;0
WireConnection;45;0;61;0
WireConnection;47;0;40;0
WireConnection;47;1;46;0
WireConnection;13;0;47;0
WireConnection;13;1;14;0
WireConnection;15;0;13;0
WireConnection;15;1;16;0
WireConnection;17;0;15;0
WireConnection;23;0;17;0
WireConnection;7;1;47;0
WireConnection;20;0;18;0
WireConnection;20;1;21;0
WireConnection;20;2;23;0
WireConnection;51;0;1;0
WireConnection;51;1;50;0
WireConnection;0;0;60;0
WireConnection;0;2;20;0
WireConnection;0;10;7;0
ASEEND*/
//CHKSM=BC5494104796A42780D600F5F01854E647973A57