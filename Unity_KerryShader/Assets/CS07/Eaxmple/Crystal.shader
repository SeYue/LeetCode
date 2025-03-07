// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Crystal"
{
	Properties
	{
		_NormalMap("NormalMap", 2D) = "white" {}
		_EmissMask("EmissMask", 2D) = "black" {}
		_RimBias("RimBias", Float) = 0
		_RimPower("RimPower", Float) = 1
		_RimScale("RimScale", Float) = 1
		_RimColor("RimColor", Color) = (0,0,0,0)
		_ReflectTex("ReflectTex", CUBE) = "white" {}
		_ReflectIntensity("ReflectIntensity", Float) = 1
		_InsideTex("InsideTex", 2D) = "white" {}
		_TillingOsset("TillingOsset", Vector) = (1,1,0,0)
		_UVDisctort("UVDisctort", Float) = 0
		_InsideColor("InsideColor", Color) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float2 uv_texcoord;
			float3 worldRefl;
		};

		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform float _RimBias;
		uniform float _RimScale;
		uniform float _RimPower;
		uniform sampler2D _EmissMask;
		SamplerState sampler_EmissMask;
		uniform float4 _EmissMask_ST;
		uniform float4 _RimColor;
		uniform samplerCUBE _ReflectTex;
		uniform float _ReflectIntensity;
		uniform sampler2D _InsideTex;
		uniform float4 _TillingOsset;
		uniform float _UVDisctort;
		uniform float4 _InsideColor;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Normal = float3(0,0,1);
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			float4 tex2DNode9 = tex2D( _NormalMap, uv_NormalMap );
			float3 NormalWorld27 = normalize( (WorldNormalVector( i , tex2DNode9.rgb )) );
			float fresnelNdotV1 = dot( NormalWorld27, ase_worldViewDir );
			float fresnelNode1 = ( _RimBias + _RimScale * pow( 1.0 - fresnelNdotV1, _RimPower ) );
			float2 uv_EmissMask = i.uv_texcoord * _EmissMask_ST.xy + _EmissMask_ST.zw;
			float4 RimColor12 = ( ( fresnelNode1 + tex2D( _EmissMask, uv_EmissMask ).r ) * _RimColor );
			float4 NormalMap26 = tex2DNode9;
			float dotResult33 = dot( NormalWorld27 , ase_worldViewDir );
			float clampResult37 = clamp( ( 1.0 - dotResult33 ) , 0.0 , 1.0 );
			float FresnelFactor39 = clampResult37;
			float4 ReflectColor23 = ( texCUBE( _ReflectTex, normalize( WorldReflectionVector( i , NormalMap26.rgb ) ) ) * _ReflectIntensity * ( FresnelFactor39 * FresnelFactor39 ) );
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 objToView56 = mul( UNITY_MATRIX_MV, float4( ase_vertex3Pos, 1 ) ).xyz;
			float3 objToView57 = mul( UNITY_MATRIX_MV, float4( float3(0,0,0), 1 ) ).xyz;
			float3 objToViewDir66 = mul( UNITY_MATRIX_IT_MV, float4( NormalWorld27, 0 ) ).xyz;
			float4 lerpResult68 = lerp( tex2D( _InsideTex, ( float4( ( ( (( objToView56 - objToView57 )).xy * (_TillingOsset).xy ) + (_TillingOsset).zw ), 0.0 , 0.0 ) + ( (objToViewDir66).xyzz * _UVDisctort ) ).xy ) , _InsideColor , FresnelFactor39);
			float4 InsideColor42 = lerpResult68;
			o.Emission = ( RimColor12 + ReflectColor23 + InsideColor42 ).rgb;
			o.Alpha = 1;
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
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
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
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
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
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.worldRefl = -worldViewDir;
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
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
252;496;1104;433;1660.049;67.9272;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;30;-1468.621,49.05227;Inherit;False;926.8612;333.4476;Comment;4;9;26;2;27;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;9;-1418.621,99.5225;Inherit;True;Property;_NormalMap;NormalMap;0;0;Create;True;0;0;False;0;False;-1;None;44f0dc08b36d5774da9f633230babc75;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;67;-1764.97,2092.439;Inherit;False;2363.917;1074.389;InsideColor;20;55;59;57;56;47;60;61;54;49;66;48;65;50;62;64;51;63;41;44;46;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldNormalVector;2;-1015.265,199.4998;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;59;-1687.904,2351.96;Inherit;False;Constant;_Vector0;Vector 0;10;0;Create;True;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PosVertexDataNode;55;-1714.97,2142.439;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;27;-765.7602,193.4974;Inherit;True;NormalWorld;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformPositionNode;56;-1465.97,2147.439;Inherit;False;Object;View;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TransformPositionNode;57;-1441.362,2353.037;Inherit;False;Object;View;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;22;-1782.767,1272.825;Inherit;False;1693.102;718.7722;ReflectColor;10;20;21;38;16;39;19;37;28;35;33;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;31;-2111.576,1636.499;Inherit;True;27;NormalWorld;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;60;-1222.812,2274.445;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector4Node;47;-1277.529,2712.712;Inherit;False;Property;_TillingOsset;TillingOsset;9;0;Create;True;0;0;False;0;False;1,1,0,0;0.65,0.46,0.49,-0.44;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;61;-1518.582,2947.93;Inherit;False;27;NormalWorld;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;34;-2136.576,1839.499;Inherit;True;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;33;-1876.576,1653.499;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TransformDirectionNode;66;-1201.008,2945.53;Inherit;False;Object;View;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SwizzleNode;49;-1071.528,2715.712;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;54;-1041.005,2272.02;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;50;-1072.528,2807.712;Inherit;False;FLOAT2;2;3;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;65;-897.897,3050.828;Inherit;False;Property;_UVDisctort;UVDisctort;10;0;Create;True;0;0;False;0;False;0;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;-847.5286,2640.712;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;13;-1466.796,520.3329;Inherit;False;1877.669;662.907;RimColor;11;3;5;6;4;1;7;11;10;8;12;29;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SwizzleNode;62;-895.1072,2944.301;Inherit;False;FLOAT4;0;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;26;-1018.838,-55.84477;Inherit;True;NormalMap;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;35;-1242.672,1650.49;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-1055.456,983.2399;Inherit;False;Property;_RimScale;RimScale;4;0;Create;True;0;0;False;0;False;1;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;37;-1029.078,1666.769;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;3;-1080.456,682.2399;Inherit;True;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;4;-1070.212,907.8659;Inherit;False;Property;_RimBias;RimBias;2;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;29;-933.342,571.2427;Inherit;False;27;NormalWorld;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-1051.456,1067.24;Inherit;False;Property;_RimPower;RimPower;3;0;Create;True;0;0;False;0;False;1;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;64;-654.8973,3001.828;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;51;-618.0057,2648.413;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;28;-1056.239,1324.827;Inherit;True;26;NormalMap;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldReflectionVector;19;-808.0648,1316.881;Inherit;True;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;39;-740.5676,1664.923;Inherit;False;FresnelFactor;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;10;-744.8719,836.7535;Inherit;True;Property;_EmissMask;EmissMask;1;0;Create;True;0;0;False;0;False;-1;None;35ddf83e0ad6f984e8a089e2d9280837;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;1;-723.6085,576.8161;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;63;-449.0529,2877.019;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-397.2321,1541.438;Inherit;False;Property;_ReflectIntensity;ReflectIntensity;7;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-454.988,1654.999;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;16;-511.0648,1337.881;Inherit;True;Property;_ReflectTex;ReflectTex;6;0;Create;True;0;0;False;0;False;-1;None;f86e5eb647816f444a48e5c3b1dd1fa7;True;0;False;white;LockedToCube;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;7;-284.6121,844.3124;Inherit;False;Property;_RimColor;RimColor;5;0;Create;True;0;0;False;0;False;0,0,0,0;0.3121263,0.2877358,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;70;241.4617,3273.6;Inherit;False;39;FresnelFactor;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;41;14.22548,2825.594;Inherit;True;Property;_InsideTex;InsideTex;8;0;Create;True;0;0;False;0;False;-1;None;ebdd90393659b1647bbe7d07a9750a72;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;69;234.4617,3080.6;Inherit;False;Property;_InsideColor;InsideColor;11;0;Create;True;0;0;False;0;False;0,0,0,0;0.254196,0.2349591,0.9056604,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;11;-300.331,605.2452;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;68;514.9291,2922.851;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-26.86776,667.4032;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-118.942,1384.909;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;12;237.5723,652.8915;Inherit;True;RimColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;23;130.7365,1425.929;Inherit;False;ReflectColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;42;1005.415,2801.222;Inherit;False;InsideColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;14;529.4592,-86.70007;Inherit;True;12;RimColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;43;525.006,330.3594;Inherit;True;42;InsideColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;25;521.0984,110.8867;Inherit;True;23;ReflectColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SwizzleNode;46;-1047.528,2542.712;Inherit;False;FLOAT4;0;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;40;768.5856,120.0557;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldPosInputsNode;44;-1267.529,2541.712;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;24;972.1844,69.65237;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Crystal;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;2;0;9;0
WireConnection;27;0;2;0
WireConnection;56;0;55;0
WireConnection;57;0;59;0
WireConnection;60;0;56;0
WireConnection;60;1;57;0
WireConnection;33;0;31;0
WireConnection;33;1;34;0
WireConnection;66;0;61;0
WireConnection;49;0;47;0
WireConnection;54;0;60;0
WireConnection;50;0;47;0
WireConnection;48;0;54;0
WireConnection;48;1;49;0
WireConnection;62;0;66;0
WireConnection;26;0;9;0
WireConnection;35;0;33;0
WireConnection;37;0;35;0
WireConnection;64;0;62;0
WireConnection;64;1;65;0
WireConnection;51;0;48;0
WireConnection;51;1;50;0
WireConnection;19;0;28;0
WireConnection;39;0;37;0
WireConnection;1;0;29;0
WireConnection;1;4;3;0
WireConnection;1;1;4;0
WireConnection;1;2;5;0
WireConnection;1;3;6;0
WireConnection;63;0;51;0
WireConnection;63;1;64;0
WireConnection;38;0;39;0
WireConnection;38;1;39;0
WireConnection;16;1;19;0
WireConnection;41;1;63;0
WireConnection;11;0;1;0
WireConnection;11;1;10;1
WireConnection;68;0;41;0
WireConnection;68;1;69;0
WireConnection;68;2;70;0
WireConnection;8;0;11;0
WireConnection;8;1;7;0
WireConnection;20;0;16;0
WireConnection;20;1;21;0
WireConnection;20;2;38;0
WireConnection;12;0;8;0
WireConnection;23;0;20;0
WireConnection;42;0;68;0
WireConnection;46;0;44;0
WireConnection;40;0;14;0
WireConnection;40;1;25;0
WireConnection;40;2;43;0
WireConnection;24;2;40;0
ASEEND*/
//CHKSM=CD8C6398B4BC810823FD1BE824EFB126991CD32E