// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Glass"
{
	Properties
	{
		_MatCap("MatCap", 2D) = "white" {}
		_Min("Min", Float) = 0
		_Max("Max", Float) = 1
		_RefractMapCap("RefractMapCap", 2D) = "white" {}
		_RefractIntensity("RefractIntensity", Float) = 1
		_RefracColor("RefracColor", Color) = (0,0,0,0)
		_ThickMap("ThickMap", 2D) = "white" {}
		_ObjectHeight("ObjectHeight", Float) = 1
		_ObjectOffset("ObjectOffset", Float) = 0
		_DirtyMask("DirtyMask", 2D) = "black" {}
		_Decal("Decal", 2D) = "black" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Pass
		{
			ColorMask 0
			ZWrite On
		}

		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			float3 viewDir;
			float2 uv_texcoord;
		};

		uniform sampler2D _MatCap;
		uniform float4 _RefracColor;
		uniform sampler2D _RefractMapCap;
		uniform float _Min;
		uniform float _Max;
		uniform sampler2D _ThickMap;
		SamplerState sampler_ThickMap;
		uniform float _ObjectOffset;
		uniform float _ObjectHeight;
		uniform sampler2D _DirtyMask;
		SamplerState sampler_DirtyMask;
		uniform float4 _DirtyMask_ST;
		uniform float _RefractIntensity;
		uniform sampler2D _Decal;
		uniform float4 _Decal_ST;
		SamplerState sampler_Decal;
		SamplerState sampler_MatCap;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 objToView12 = mul( UNITY_MATRIX_MV, float4( ase_vertex3Pos, 1 ) ).xyz;
			float3 normalizeResult16 = normalize( objToView12 );
			float3 ase_worldNormal = i.worldNormal;
			float3 break18 = cross( normalizeResult16 , mul( UNITY_MATRIX_V, float4( ase_worldNormal , 0.0 ) ).xyz );
			float2 appendResult19 = (float2(-break18.y , break18.x));
			float2 Matcap_UV224 = (appendResult19*0.5 + 0.5);
			float4 tex2DNode1 = tex2D( _MatCap, Matcap_UV224 );
			float dotResult32 = dot( i.viewDir , ase_worldNormal );
			float smoothstepResult33 = smoothstep( _Min , _Max , dotResult32);
			float3 ase_worldPos = i.worldPos;
			float3 objToWorld57 = mul( unity_ObjectToWorld, float4( float3( 0,0,0 ), 1 ) ).xyz;
			float2 appendResult64 = (float2(0.5 , ( ( ( ase_worldPos.y - objToWorld57.y ) - _ObjectOffset ) / _ObjectHeight )));
			float2 uv_DirtyMask = i.uv_texcoord * _DirtyMask_ST.xy + _DirtyMask_ST.zw;
			float clampResult68 = clamp( ( ( 1.0 - smoothstepResult33 ) + tex2D( _ThickMap, appendResult64 ).r + tex2D( _DirtyMask, uv_DirtyMask ).a ) , 0.0 , 1.0 );
			float Thick46 = clampResult68;
			float temp_output_35_0 = ( Thick46 * _RefractIntensity );
			float clampResult43 = clamp( temp_output_35_0 , 0.0 , 1.0 );
			float4 lerpResult42 = lerp( ( 0.5 * _RefracColor ) , ( _RefracColor * tex2D( _RefractMapCap, ( Matcap_UV224 + temp_output_35_0 ) ) ) , clampResult43);
			float2 uv_Decal = i.uv_texcoord * _Decal_ST.xy + _Decal_ST.zw;
			float4 tex2DNode71 = tex2D( _Decal, uv_Decal );
			float4 lerpResult72 = lerp( ( tex2DNode1 + lerpResult42 ) , tex2DNode71 , tex2DNode71.a);
			o.Emission = lerpResult72.rgb;
			float clampResult45 = clamp( max( tex2DNode1.r , Thick46 ) , 0.0 , 1.0 );
			o.Alpha = ( tex2DNode71.a + clampResult45 );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit keepalpha fullforwardshadows 

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
				float3 worldNormal : TEXCOORD3;
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
				o.worldNormal = worldNormal;
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
				surfIN.viewDir = worldViewDir;
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
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
581;490;881;506;1949.831;-204.2494;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;50;-4275.407,996.9761;Inherit;False;1898.114;912.6802;Thickness;19;65;54;34;66;33;64;62;32;29;61;30;63;58;59;55;57;68;69;70;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;23;-4290.319,345.5663;Inherit;False;1888.66;533.8933;Comment;12;24;15;13;12;16;19;17;22;11;20;14;18;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TransformPositionNode;57;-4226.247,1590.18;Inherit;False;Object;World;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;55;-4223.247,1429.18;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PosVertexDataNode;11;-4240.319,402.7659;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;59;-3941.216,1622.761;Inherit;False;Property;_ObjectOffset;ObjectOffset;9;0;Create;True;0;0;False;0;False;0;-0.005;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;58;-3916.28,1489.722;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TransformPositionNode;12;-4051.529,395.5663;Inherit;False;Object;View;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;61;-3744.091,1493.054;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;29;-4224.689,1210.982;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;14;-4052.036,656.4596;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;63;-3743.344,1627.048;Inherit;False;Property;_ObjectHeight;ObjectHeight;8;0;Create;True;0;0;False;0;False;1;0.35;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ViewMatrixNode;13;-3976.038,565.5954;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;30;-4188.153,1063.093;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;32;-3965.873,1071.88;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;69;-3518.951,1148.498;Inherit;False;Property;_Min;Min;2;0;Create;True;0;0;False;0;False;0;0.08;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;70;-3516.951,1226.498;Inherit;False;Property;_Max;Max;3;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-3843.89,566.6009;Inherit;False;2;2;0;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;16;-3834.121,403.7368;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;62;-3553.546,1489.074;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;64;-3367.244,1460.126;Inherit;False;FLOAT2;4;0;FLOAT;0.5;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SmoothstepOpNode;33;-3342.174,1088.86;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CrossProductOpNode;17;-3675.692,481.1701;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;66;-3194.002,1635.836;Inherit;True;Property;_DirtyMask;DirtyMask;10;0;Create;True;0;0;False;0;False;-1;b2bd09ae30d3a2c46abd7275edaaed0e;b2bd09ae30d3a2c46abd7275edaaed0e;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;18;-3504.819,482.0914;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SamplerNode;54;-3184.639,1434.521;Inherit;True;Property;_ThickMap;ThickMap;7;0;Create;True;0;0;False;0;False;-1;None;cd602ef54fc7800458ed8f8d9920db78;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;34;-3049.851,1087.699;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;20;-3233.819,558.0915;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;65;-2783.864,1216.49;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;19;-3068.819,473.0914;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ClampOpNode;68;-2598.991,1216.784;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;22;-2904.658,475.6628;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;46;-2238.098,1157.3;Inherit;False;Thick;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;48;-2027.357,1069.386;Inherit;False;46;Thick;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;24;-2659.45,494.2645;Inherit;False;Matcap_UV2;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-2039.03,1151.976;Inherit;False;Property;_RefractIntensity;RefractIntensity;5;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-1835.03,1078.976;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;27;-1762.63,820.6149;Inherit;True;24;Matcap_UV2;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;-1472.4,883.3978;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;53;-976.2919,544.7883;Inherit;False;Constant;_Float0;Float 0;4;0;Create;True;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;38;-1186.583,894.3945;Inherit;True;Property;_RefractMapCap;RefractMapCap;4;0;Create;True;0;0;False;0;False;-1;None;1853701dbc6311c43b8301f19ed47c83;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;10;-1452.924,304.9865;Inherit;True;24;Matcap_UV2;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;41;-1172.944,604.4776;Inherit;False;Property;_RefracColor;RefracColor;6;0;Create;True;0;0;False;0;False;0,0,0,0;0.3962264,0.3962264,0.3962264,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;-801.2921,586.2882;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-863.792,885.7878;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;49;-285.4716,617.3559;Inherit;False;46;Thick;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-1222.274,308.0549;Inherit;True;Property;_MatCap;MatCap;1;0;Create;True;0;0;False;0;False;-1;None;c3beb0a5b5c106649a0905c6e34674f9;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;43;-919.9303,1097.646;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;42;-679.5996,869.78;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;40;-100.7642,558.528;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;71;195.264,372.2755;Inherit;True;Property;_Decal;Decal;11;0;Create;True;0;0;False;0;False;-1;None;742be472e75fb874591d497f42113ac3;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;74;535.043,223.4398;Inherit;False;232;209;Lerp返回0,1;1;72;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ClampOpNode;45;40.65374,557.7816;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;9;-3800.345,-113.3317;Inherit;False;1110.899;334.2828;Comment;6;8;5;4;7;6;3;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;37;-110.9558,328.5645;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;72;585.043,273.4398;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;73;584.9151,510.2693;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;7;-3106.78,28.01852;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;6;-3291.327,23.05409;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-3521.346,32.50785;Inherit;False;2;2;0;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewMatrixNode;4;-3746.513,-63.33168;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.WorldNormalVector;3;-3774.677,63.12096;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;8;-2885.805,20.54937;Inherit;False;MapCat_uv;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;860.5493,345.6879;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Glass;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;True;0;Custom;0.5;True;True;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;58;0;55;2
WireConnection;58;1;57;2
WireConnection;12;0;11;0
WireConnection;61;0;58;0
WireConnection;61;1;59;0
WireConnection;32;0;30;0
WireConnection;32;1;29;0
WireConnection;15;0;13;0
WireConnection;15;1;14;0
WireConnection;16;0;12;0
WireConnection;62;0;61;0
WireConnection;62;1;63;0
WireConnection;64;1;62;0
WireConnection;33;0;32;0
WireConnection;33;1;69;0
WireConnection;33;2;70;0
WireConnection;17;0;16;0
WireConnection;17;1;15;0
WireConnection;18;0;17;0
WireConnection;54;1;64;0
WireConnection;34;0;33;0
WireConnection;20;0;18;1
WireConnection;65;0;34;0
WireConnection;65;1;54;1
WireConnection;65;2;66;4
WireConnection;19;0;20;0
WireConnection;19;1;18;0
WireConnection;68;0;65;0
WireConnection;22;0;19;0
WireConnection;46;0;68;0
WireConnection;24;0;22;0
WireConnection;35;0;48;0
WireConnection;35;1;36;0
WireConnection;28;0;27;0
WireConnection;28;1;35;0
WireConnection;38;1;28;0
WireConnection;52;0;53;0
WireConnection;52;1;41;0
WireConnection;51;0;41;0
WireConnection;51;1;38;0
WireConnection;1;1;10;0
WireConnection;43;0;35;0
WireConnection;42;0;52;0
WireConnection;42;1;51;0
WireConnection;42;2;43;0
WireConnection;40;0;1;1
WireConnection;40;1;49;0
WireConnection;45;0;40;0
WireConnection;37;0;1;0
WireConnection;37;1;42;0
WireConnection;72;0;37;0
WireConnection;72;1;71;0
WireConnection;72;2;71;4
WireConnection;73;0;71;4
WireConnection;73;1;45;0
WireConnection;7;0;6;0
WireConnection;6;0;5;0
WireConnection;5;0;4;0
WireConnection;5;1;3;0
WireConnection;8;0;7;0
WireConnection;0;2;72;0
WireConnection;0;9;73;0
ASEEND*/
//CHKSM=6CB1A317A102865798C43434FA75C499EE792FD9