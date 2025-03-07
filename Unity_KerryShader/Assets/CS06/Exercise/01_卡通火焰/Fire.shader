// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Fire"
{
	Properties
	{
		_Noise("Noise", 2D) = "white" {}
		_NoiseSpeed("NoiseSpeed", Vector) = (0,-0.5,0,0)
		_Gradient("Gradient", 2D) = "white" {}
		_Softness("Softness", Range( 0 , 1)) = 0.1
		[HDR]_Color("Color", Color) = (1.981132,0.02023846,0,0)
		_EmissionIntensity("EmissionIntensity", Range( 0 , 10)) = 1
		_EndMiss("EndMiss", Range( 0 , 1)) = 1
		_GradientEndControl("GradientEndControl", Float) = 2
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_NoiseIntensity("NoiseIntensity", Float) = 0.1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _Color;
		uniform float _EmissionIntensity;
		uniform float _EndMiss;
		uniform sampler2D _Gradient;
		SamplerState sampler_Gradient;
		uniform float4 _Gradient_ST;
		uniform float _GradientEndControl;
		uniform sampler2D _Noise;
		SamplerState sampler_Noise;
		uniform float2 _NoiseSpeed;
		uniform float4 _Noise_ST;
		uniform float _Softness;
		uniform sampler2D _TextureSample0;
		SamplerState sampler_TextureSample0;
		uniform float _NoiseIntensity;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 break36 = ( _Color * _EmissionIntensity );
			float2 uv_Gradient = i.uv_texcoord * _Gradient_ST.xy + _Gradient_ST.zw;
			float4 tex2DNode16 = tex2D( _Gradient, uv_Gradient );
			float GradientEnd29 = ( ( 1.0 - tex2DNode16.r ) * _GradientEndControl );
			float2 uv_Noise = i.uv_texcoord * _Noise_ST.xy + _Noise_ST.zw;
			float2 panner10 = ( 1.0 * _Time.y * _NoiseSpeed + uv_Noise);
			float Noise22 = tex2D( _Noise, panner10 ).r;
			float4 appendResult38 = (float4(break36.r , ( break36.g + ( _EndMiss * GradientEnd29 * Noise22 ) ) , break36.b , break36.a));
			o.Emission = appendResult38.xyz;
			float clampResult20 = clamp( ( Noise22 - _Softness ) , 0.0 , 1.0 );
			float Gradient21 = tex2DNode16.r;
			float smoothstepResult17 = smoothstep( clampResult20 , Noise22 , Gradient21);
			float2 appendResult57 = (float2(( i.uv_texcoord.x + ( (Noise22*2.0 + -1.0) * _NoiseIntensity * GradientEnd29 ) ) , i.uv_texcoord.y));
			float4 tex2DNode44 = tex2D( _TextureSample0, appendResult57 );
			float clampResult62 = clamp( ( tex2DNode44.r * tex2DNode44.r * 3.0 ) , 0.0 , 1.0 );
			float FireShape64 = clampResult62;
			o.Alpha = ( smoothstepResult17 * FireShape64 );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit alpha:fade keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18500
423;323;1393;649;1982.697;2106.254;4.927385;True;False
Node;AmplifyShaderEditor.CommentaryNode;31;-1279.307,-1688.149;Inherit;False;1613.549;742.5652;Gradient;7;29;27;26;28;21;16;1;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;9;-1309.203,-349.277;Inherit;False;0;7;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;1;-1179.307,-1418.721;Inherit;False;0;16;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;12;-1254.546,-212.1473;Inherit;False;Property;_NoiseSpeed;NoiseSpeed;1;0;Create;True;0;0;False;0;False;0,-0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;16;-905.2889,-1442.828;Inherit;True;Property;_Gradient;Gradient;2;0;Create;True;0;0;False;0;False;-1;7a1cebf5fe9d61a47b2c5ed338d89180;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;10;-1009.692,-332.2446;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-763.1793,-1194.385;Inherit;False;Property;_GradientEndControl;GradientEndControl;7;0;Create;True;0;0;False;0;False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;28;-498.8116,-1368.516;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;7;-802.8238,-360.3483;Inherit;True;Property;_Noise;Noise;0;0;Create;True;0;0;False;0;False;-1;4020dd0b592ce7643bbda0ac6044da13;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;68;13.33406,2.50256;Inherit;False;2723.666;764.4872;Shape;15;47;53;55;51;50;58;49;45;48;57;60;44;59;62;64;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-247.9465,-1270.257;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;22;-475.2164,-343.0423;Inherit;False;Noise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;47;63.33406,164.482;Inherit;True;22;Noise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;53;133.1761,369.9803;Inherit;False;Constant;_Float0;Float 0;10;0;Create;True;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;55;131.4342,464.609;Inherit;False;Constant;_Float1;Float 1;10;0;Create;True;0;0;False;0;False;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;29;87.97659,-1289.513;Inherit;False;GradientEnd;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;51;633.913,536.9897;Inherit;True;29;GradientEnd;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;50;634.6498,411.6342;Inherit;False;Property;_NoiseIntensity;NoiseIntensity;9;0;Create;True;0;0;False;0;False;0.1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;58;330.2193,348.8304;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;859.5605,348.8244;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;45;784.2537,72.70901;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;48;1196.551,52.50256;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;57;1456.725,95.71445;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;66;644.4338,-1817.284;Inherit;False;1523.02;805.2557;Color;10;36;38;34;15;35;39;42;40;41;43;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;44;1637.758,104.931;Inherit;True;Property;_TextureSample0;Texture Sample 0;8;0;Create;True;0;0;False;0;False;-1;538ce49acf5cd374aa2def71f4a97b06;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;60;1791.572,323.2374;Inherit;False;Constant;_Float2;Float 2;10;0;Create;True;0;0;False;0;False;3;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;67;646.7589,-839.4404;Inherit;False;1287.395;699.8226;Alpha;7;17;18;23;19;24;25;20;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;15;741.6006,-1767.284;Inherit;False;Property;_Color;Color;4;1;[HDR];Create;True;0;0;False;0;False;1.981132,0.02023846,0,0;1.981132,0.02023846,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;1970.624,108.1523;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;34;694.4338,-1541.665;Inherit;False;Property;_EmissionIntensity;EmissionIntensity;5;0;Create;True;0;0;False;0;False;1;2;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;696.7589,-373.1113;Inherit;False;Property;_Softness;Softness;3;0;Create;True;0;0;False;0;False;0.1;1.34;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;23;760.0887,-584.9843;Inherit;True;22;Noise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;21;-572.639,-1638.149;Inherit;False;Gradient;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;18;1014.782,-559.4861;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;41;1118.783,-1454.321;Inherit;True;29;GradientEnd;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;1017.628,-1762.615;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;62;2208.038,108.0938;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;43;1118.528,-1242.028;Inherit;True;22;Noise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;40;1068.346,-1551.403;Inherit;False;Property;_EndMiss;EndMiss;6;0;Create;True;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;20;1339.654,-549.0453;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;1388.27,-1548.272;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;64;2513,115;Inherit;False;FireShape;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;36;1210.497,-1763.654;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.GetLocalVarNode;24;1368.8,-369.6178;Inherit;True;22;Noise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;25;1354.526,-789.4404;Inherit;True;21;Gradient;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;17;1680.153,-567.4904;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;65;2016.65,-411.7427;Inherit;True;64;FireShape;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;39;1563.318,-1555.413;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;2266.656,-525.0373;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;38;2006.454,-1752.928;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2567.731,-1064.827;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Fire;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;16;1;1;0
WireConnection;10;0;9;0
WireConnection;10;2;12;0
WireConnection;28;0;16;1
WireConnection;7;1;10;0
WireConnection;27;0;28;0
WireConnection;27;1;26;0
WireConnection;22;0;7;1
WireConnection;29;0;27;0
WireConnection;58;0;47;0
WireConnection;58;1;53;0
WireConnection;58;2;55;0
WireConnection;49;0;58;0
WireConnection;49;1;50;0
WireConnection;49;2;51;0
WireConnection;48;0;45;1
WireConnection;48;1;49;0
WireConnection;57;0;48;0
WireConnection;57;1;45;2
WireConnection;44;1;57;0
WireConnection;59;0;44;1
WireConnection;59;1;44;1
WireConnection;59;2;60;0
WireConnection;21;0;16;1
WireConnection;18;0;23;0
WireConnection;18;1;19;0
WireConnection;35;0;15;0
WireConnection;35;1;34;0
WireConnection;62;0;59;0
WireConnection;20;0;18;0
WireConnection;42;0;40;0
WireConnection;42;1;41;0
WireConnection;42;2;43;0
WireConnection;64;0;62;0
WireConnection;36;0;35;0
WireConnection;17;0;25;0
WireConnection;17;1;20;0
WireConnection;17;2;24;0
WireConnection;39;0;36;1
WireConnection;39;1;42;0
WireConnection;46;0;17;0
WireConnection;46;1;65;0
WireConnection;38;0;36;0
WireConnection;38;1;39;0
WireConnection;38;2;36;2
WireConnection;38;3;36;3
WireConnection;0;2;38;0
WireConnection;0;9;46;0
ASEEND*/
//CHKSM=F60C02FE83BF84EDAB5022AB2B573F1AC49BF775