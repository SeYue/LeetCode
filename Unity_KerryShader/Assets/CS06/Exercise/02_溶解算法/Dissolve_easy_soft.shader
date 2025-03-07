// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Dissolve_easy"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_Gradient("Gradient", 2D) = "white" {}
		_ChangeAmount("ChangeAmount", Range( 0 , 1)) = 0.5
		[HDR]_EdgeColor("EdgeColor", Color) = (1,0.8126906,0,0)
		_EdgeWidth("EdgeWidth", Range( 0.02 , 2)) = 0.3927059
		_EdgeColorIntensity("EdgeColorIntensity", Range( 0 , 5)) = 1.44966
		[Toggle(_MANNULCONTROL_ON)] _MANNULCONTROL("MANNULCONTROL", Float) = 0
		_Spread("Spread", Range( 0 , 1)) = 0
		_Softness("Softness", Range( 0 , 0.5)) = 0
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
		#pragma shader_feature_local _MANNULCONTROL_ON
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float4 _EdgeColor;
		uniform float _EdgeColorIntensity;
		uniform sampler2D _Gradient;
		SamplerState sampler_Gradient;
		uniform float4 _Gradient_ST;
		uniform float _ChangeAmount;
		uniform float _Spread;
		uniform float _Softness;
		uniform float _EdgeWidth;
		SamplerState sampler_MainTex;

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
			float Gradient24 = ( ( tex2D( _Gradient, uv_Gradient ).r - (-_Spread + (staticSwitch35 - 0.0) * (1.0 - -_Spread) / (1.0 - 0.0)) ) / _Spread );
			float clampResult23 = clamp( ( 1.0 - ( distance( Gradient24 , _Softness ) / _EdgeWidth ) ) , 0.0 , 1.0 );
			float4 lerpResult19 = lerp( tex2DNode1 , ( _EdgeColor * _EdgeColorIntensity ) , clampResult23);
			o.Emission = lerpResult19.rgb;
			float smoothstepResult41 = smoothstep( _Softness , 0.5 , Gradient24);
			o.Alpha = ( tex2DNode1.a * smoothstepResult41 );
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
138;217;1031;701;1477.076;-400.9745;1.182246;True;False
Node;AmplifyShaderEditor.CommentaryNode;36;-3345.526,343.5571;Inherit;False;2111.12;733.6765;Comment;12;24;39;5;2;9;38;35;33;37;4;30;34;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-3269.656,525.5834;Inherit;False;Constant;_Float1;Float 1;7;0;Create;True;0;0;False;0;False;0.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;30;-3096.074,531.8544;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-3016.264,776.3847;Inherit;False;Property;_ChangeAmount;ChangeAmount;2;0;Create;True;0;0;False;0;False;0.5;0.4;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;37;-2801.222,936.806;Inherit;False;Property;_Spread;Spread;7;0;Create;True;0;0;False;0;False;0;0.234;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;33;-2898.081,531.933;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;38;-2472.156,884.3433;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;35;-2642.585,595.9456;Inherit;True;Property;_MANNULCONTROL;MANNULCONTROL;6;0;Create;True;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;9;-2241.021,600.9101;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-2257.93,393.5571;Inherit;True;Property;_Gradient;Gradient;1;0;Create;True;0;0;False;0;False;-1;1f607203268ce664d80ad830741ba316;1f607203268ce664d80ad830741ba316;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;5;-1905.365,505.5994;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;39;-1699.248,851.0569;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;24;-1422.45,855.6187;Inherit;False;Gradient;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;28;-1106.963,821.7467;Inherit;False;1219.079;405.5975;EdgeColor;6;14;16;13;15;17;23;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;42;-1191.243,570.1532;Inherit;False;Property;_Softness;Softness;8;0;Create;True;0;0;False;0;False;0;0.25;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;40;-1105.881,469.6004;Inherit;False;24;Gradient;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-828.0015,1111.344;Inherit;False;Property;_EdgeWidth;EdgeWidth;4;0;Create;True;0;0;False;0;False;0.3927059;0.5;0.02;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;13;-775.7778,877.6133;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;15;-548.091,882.1583;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;18;-403.1563,36.32813;Inherit;False;Property;_EdgeColor;EdgeColor;3;1;[HDR];Create;True;0;0;False;0;False;1,0.8126906,0,0;2,1.74902,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;21;-469.4512,221.6424;Inherit;False;Property;_EdgeColorIntensity;EdgeColorIntensity;5;0;Create;True;0;0;False;0;False;1.44966;3.79;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;17;-315.8983,888.4343;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-823.4622,-43.83915;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;False;0;False;-1;0b06a95f0a779f24ab2a475a87d0a6c9;0b06a95f0a779f24ab2a475a87d0a6c9;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;23;-141.8838,892.1432;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-177.99,45.57344;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;41;-795.3712,472.8295;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;19;82.32967,-21.93285;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-503.3969,476.2385;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-1056.963,991.8869;Inherit;False;Constant;_Float0;Float 0;4;0;Create;True;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;875.4783,-9.859392;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Dissolve_easy;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
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
WireConnection;24;0;39;0
WireConnection;13;0;40;0
WireConnection;13;1;42;0
WireConnection;15;0;13;0
WireConnection;15;1;16;0
WireConnection;17;0;15;0
WireConnection;23;0;17;0
WireConnection;20;0;18;0
WireConnection;20;1;21;0
WireConnection;41;0;40;0
WireConnection;41;1;42;0
WireConnection;19;0;1;0
WireConnection;19;1;20;0
WireConnection;19;2;23;0
WireConnection;3;0;1;4
WireConnection;3;1;41;0
WireConnection;0;2;19;0
WireConnection;0;9;3;0
ASEEND*/
//CHKSM=4ED2322F8BB103F61B499E8AD0250677ECD50C63