// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Nyx_Body"
{
	Properties
	{
		_NormalMap("NormalMap", 2D) = "bump" {}
		_RimPower("RimPower", Float) = 5
		_RimScale("RimScale", Range( 0 , 10)) = 5
		_RimBase("RimBase", Range( 0 , 1)) = 0
		_RimColor("RimColor", Color) = (1,0.7215686,0.5019608,0)
		_FlowTillingSpeed("FlowTillingSpeed", Vector) = (0.5,0.5,0,0.1)
		_EmissMap("EmissMap", 2D) = "white" {}
		_FlowColor("FlowColor", Color) = (1,0.7215686,0.4980392,0)
		_FlowRimBias("FlowRimBias", Float) = 0
		_FlowRimScale("FlowRimScale", Float) = 1
		_NebulaTilling("NebulaTilling", Vector) = (1,1,0,0)
		_NebulaTex("NebulaTex", 2D) = "white" {}
		_NebulaDistance("NebulaDistance", Float) = 1
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
			float3 worldNormal;
			INTERNAL_DATA
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform float _RimPower;
		uniform float _RimScale;
		uniform float _RimBase;
		uniform float4 _RimColor;
		uniform float4 _FlowColor;
		uniform sampler2D _EmissMap;
		SamplerState sampler_EmissMap;
		uniform float4 _FlowTillingSpeed;
		uniform float _FlowRimScale;
		uniform float _FlowRimBias;
		uniform sampler2D _NebulaTex;
		uniform float _NebulaDistance;
		uniform float2 _NebulaTilling;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Normal = float3(0,0,1);
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			float3 WorldNormal7 = normalize( (WorldNormalVector( i , UnpackNormal( tex2D( _NormalMap, uv_NormalMap ) ) )) );
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float dotResult4 = dot( WorldNormal7 , ase_worldViewDir );
			float NDotV5 = dotResult4;
			float clampResult14 = clamp( NDotV5 , 0.0 , 1.0 );
			float4 RimColor25 = ( ( ( pow( ( 1.0 - clampResult14 ) , _RimPower ) * _RimScale ) + _RimBase ) * _RimColor );
			float3 objToWorld29 = mul( unity_ObjectToWorld, float4( float3( 0,0,0 ), 1 ) ).xyz;
			float2 panner38 = ( 1.0 * _Time.y * (_FlowTillingSpeed).zw + ( ( (NDotV5*0.5 + 0.5) + (( ase_worldPos - objToWorld29 )).xy ) * (_FlowTillingSpeed).xy ));
			float FlowLight42 = tex2D( _EmissMap, panner38 ).r;
			float4 FlowLightColor52 = ( _FlowColor * FlowLight42 * ( ( ( 1.0 - NDotV5 ) * _FlowRimScale ) + _FlowRimBias ) );
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 objToView65 = mul( UNITY_MATRIX_MV, float4( ase_vertex3Pos, 1 ) ).xyz;
			float3 objToView66 = mul( UNITY_MATRIX_MV, float4( float3( 0,0,0 ), 1 ) ).xyz;
			float3 objToViewDir73 = normalize( mul( UNITY_MATRIX_IT_MV, float4( WorldNormal7, 0 ) ).xyz );
			float4 NebulaColor63 = tex2D( _NebulaTex, ( ( (( objToView65 - objToView66 )).xy + ( (objToViewDir73).xy * _NebulaDistance ) ) * _NebulaTilling ) );
			o.Emission = ( RimColor25 + FlowLightColor52 + ( NebulaColor63 * FlowLight42 ) + ( pow( NebulaColor63 , 5.0 ) * pow( FlowLight42 , 3.0 ) * 10.0 ) ).rgb;
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
351;556;1167;505;1650.238;225.9434;2.832131;True;False
Node;AmplifyShaderEditor.CommentaryNode;8;-1562.643,-322.7916;Inherit;False;912.2998;280;NormalMap;3;6;1;7;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;6;-1512.643,-272.7916;Inherit;True;Property;_NormalMap;NormalMap;0;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldNormalVector;1;-1148.943,-265.4901;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;10;-1579.387,0.1825867;Inherit;False;668.73;368;NDotV;4;2;4;5;9;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;7;-874.3433,-263.9007;Inherit;False;WorldNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;2;-1485.656,180.1826;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;9;-1529.387,65.5903;Inherit;False;7;WorldNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;4;-1280.657,61.18261;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;43;-2296.571,1097.358;Inherit;False;2194.908;1221.146;Comment;14;42;40;38;35;39;37;34;36;30;29;27;45;44;48;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;5;-1134.657,50.18259;Inherit;True;NDotV;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;79;-2266.336,3436.583;Inherit;False;2586.141;746.4287;Comment;15;72;62;66;73;65;67;75;74;68;76;77;69;70;71;63;NebulaColor;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;27;-2235.374,1617.63;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TransformPositionNode;29;-2249.374,1774.63;Inherit;False;Object;World;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PosVertexDataNode;62;-2216.336,3486.583;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;30;-1952.5,1616.826;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;45;-2010.007,1244.866;Inherit;True;5;NDotV;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;72;-2117.303,3932.405;Inherit;False;7;WorldNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformPositionNode;65;-1951.934,3492.257;Inherit;False;Object;View;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TransformPositionNode;66;-1953.309,3659.426;Inherit;False;Object;View;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TransformDirectionNode;73;-1849.7,3939.838;Inherit;False;Object;View;True;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SwizzleNode;34;-1709.047,1504.008;Inherit;True;FLOAT2;0;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;22;-2324.305,443.8452;Inherit;False;1880.384;511.0868;Comment;12;25;23;17;24;15;12;14;11;18;19;20;21;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;48;-1758.094,1245.793;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;36;-1620.982,1830.767;Inherit;False;Property;_FlowTillingSpeed;FlowTillingSpeed;5;0;Create;True;0;0;False;0;False;0.5,0.5,0,0.1;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;67;-1661.309,3615.426;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;75;-1565.537,4067.012;Inherit;False;Property;_NebulaDistance;NebulaDistance;12;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;74;-1554.655,3964.835;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;44;-1449.614,1499.656;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;37;-1408.293,1775.378;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;11;-2274.305,493.8452;Inherit;False;5;NDotV;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;-1323.272,4010.941;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;68;-1432.309,3512.426;Inherit;True;FLOAT2;0;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;60;-2242.386,2462.096;Inherit;False;1525.74;882.228;Comment;10;50;49;51;52;54;55;57;59;56;58;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-1202.652,1624.265;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ClampOpNode;14;-2069.954,504.2118;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;39;-1407.855,1880.553;Inherit;False;FLOAT2;2;3;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;69;-878.325,3817.95;Inherit;False;Property;_NebulaTilling;NebulaTilling;10;0;Create;True;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleAddOpNode;77;-842.7939,3693.437;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;54;-2192.386,3035.81;Inherit;True;5;NDotV;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;38;-981.5678,1627.019;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;12;-1867.953,503.2117;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-1850.953,601.2119;Inherit;False;Property;_RimPower;RimPower;1;0;Create;True;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;15;-1680.953,504.2117;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-1722.953,763.2119;Inherit;False;Property;_RimScale;RimScale;2;0;Create;True;0;0;False;0;False;5;0;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;70;-523.0006,3685.057;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-1944.853,3228.324;Inherit;False;Property;_FlowRimScale;FlowRimScale;9;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;55;-1943.415,2987.596;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;40;-712.5975,1618.246;Inherit;True;Property;_EmissMap;EmissMap;6;0;Create;True;0;0;False;0;False;-1;6e9e3841a0552a34cb7c38b3628da853;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;56;-1703.752,2991.207;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;42;-344.5976,1649.246;Inherit;True;FlowLight;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;59;-1692.493,3219.761;Inherit;False;Property;_FlowRimBias;FlowRimBias;8;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;71;-283.0006,3658.057;Inherit;True;Property;_NebulaTex;NebulaTex;11;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-1406.952,504.2117;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-1431.485,761.8856;Inherit;False;Property;_RimBase;RimBase;3;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;49;-1455.647,2695.441;Inherit;True;42;FlowLight;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;50;-1459.292,2512.096;Inherit;False;Property;_FlowColor;FlowColor;7;0;Create;True;0;0;False;0;False;1,0.7215686,0.4980392,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;92;-155.1392,327.4765;Inherit;False;719.9573;516.8055;Comment;6;83;84;85;87;88;86;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;63;95.80468,3665.435;Inherit;True;NebulaColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;23;-1090.094,763.7171;Inherit;False;Property;_RimColor;RimColor;4;0;Create;True;0;0;False;0;False;1,0.7215686,0.5019608,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;58;-1446.432,2992.42;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;20;-1119.646,503.8855;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-1208.647,2696.441;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-823.5939,505.0171;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;84;-105.1392,377.4766;Inherit;True;63;NebulaColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;83;-94.62565,595.2315;Inherit;True;42;FlowLight;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;52;-941.6466,2732.441;Inherit;False;FlowLightColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;85;177.0176,394.0541;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;5;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;87;205.9607,584.7575;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;89;225.4817,-160.5328;Inherit;True;63;NebulaColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;90;231.5111,38.00283;Inherit;True;42;FlowLight;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;88;246.7435,728.2822;Inherit;False;Constant;_Float0;Float 0;13;0;Create;True;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;25;-630.2693,500.6183;Inherit;False;RimColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;53;469.6209,-295.1858;Inherit;True;52;FlowLightColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;26;467.0426,-491.0439;Inherit;True;25;RimColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;91;456.6557,-16.98462;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;86;402.8182,518.3628;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;61;744.8702,-252.2445;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1277.661,-271.4991;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Nyx_Body;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;1;0;6;0
WireConnection;7;0;1;0
WireConnection;4;0;9;0
WireConnection;4;1;2;0
WireConnection;5;0;4;0
WireConnection;30;0;27;0
WireConnection;30;1;29;0
WireConnection;65;0;62;0
WireConnection;73;0;72;0
WireConnection;34;0;30;0
WireConnection;48;0;45;0
WireConnection;67;0;65;0
WireConnection;67;1;66;0
WireConnection;74;0;73;0
WireConnection;44;0;48;0
WireConnection;44;1;34;0
WireConnection;37;0;36;0
WireConnection;76;0;74;0
WireConnection;76;1;75;0
WireConnection;68;0;67;0
WireConnection;35;0;44;0
WireConnection;35;1;37;0
WireConnection;14;0;11;0
WireConnection;39;0;36;0
WireConnection;77;0;68;0
WireConnection;77;1;76;0
WireConnection;38;0;35;0
WireConnection;38;2;39;0
WireConnection;12;0;14;0
WireConnection;15;0;12;0
WireConnection;15;1;17;0
WireConnection;70;0;77;0
WireConnection;70;1;69;0
WireConnection;55;0;54;0
WireConnection;40;1;38;0
WireConnection;56;0;55;0
WireConnection;56;1;57;0
WireConnection;42;0;40;1
WireConnection;71;1;70;0
WireConnection;18;0;15;0
WireConnection;18;1;19;0
WireConnection;63;0;71;0
WireConnection;58;0;56;0
WireConnection;58;1;59;0
WireConnection;20;0;18;0
WireConnection;20;1;21;0
WireConnection;51;0;50;0
WireConnection;51;1;49;0
WireConnection;51;2;58;0
WireConnection;24;0;20;0
WireConnection;24;1;23;0
WireConnection;52;0;51;0
WireConnection;85;0;84;0
WireConnection;87;0;83;0
WireConnection;25;0;24;0
WireConnection;91;0;89;0
WireConnection;91;1;90;0
WireConnection;86;0;85;0
WireConnection;86;1;87;0
WireConnection;86;2;88;0
WireConnection;61;0;26;0
WireConnection;61;1;53;0
WireConnection;61;2;91;0
WireConnection;61;3;86;0
WireConnection;0;2;61;0
ASEEND*/
//CHKSM=3A1827FAE6C1D541C1DA24F45544E218EB524DAD