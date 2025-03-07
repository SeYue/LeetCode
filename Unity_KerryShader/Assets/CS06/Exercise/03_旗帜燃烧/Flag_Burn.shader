// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Flag_Burn"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_flag1pos("flag1.pos", 2D) = "white" {}
		_flag1norm("flag1.norm", 2D) = "white" {}
		_FrameCount("FrameCount", Float) = 100
		_Speed("Speed", Float) = 0
		_BoundingMax("BoundingMax", Float) = 1
		_BoundingMin("BoundingMin", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _flag1pos;
		uniform float _Speed;
		uniform float _FrameCount;
		uniform float _BoundingMax;
		uniform float _BoundingMin;
		uniform sampler2D _flag1norm;
		uniform float4 _flag1norm_ST;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float _Metallic;
		uniform float _Smoothness;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float CurrentFrame17 = ( ( -ceil( ( frac( ( _Time.y * _Speed ) ) * ( _FrameCount - 1.0 ) ) ) / _FrameCount ) + ( -1.0 / _FrameCount ) );
			float2 appendResult20 = (float2(v.texcoord1.xy.x , CurrentFrame17));
			float2 UV_VAT21 = appendResult20;
			float4 break30 = ( ( (tex2Dlod( _flag1pos, float4( UV_VAT21, 0, 0.0) )).rgba * ( _BoundingMax - _BoundingMin ) ) + _BoundingMin );
			float4 appendResult32 = (float4(-break30.x , break30.z , break30.y , 0.0));
			float4 VAT_VertexOffset33 = appendResult32;
			v.vertex.xyz += VAT_VertexOffset33.xyz;
			v.vertex.w = 1;
			float2 uv_flag1norm = v.texcoord * _flag1norm_ST.xy + _flag1norm_ST.zw;
			float3 break41 = ((tex2Dlod( _flag1norm, float4( uv_flag1norm, 0, 0.0) )).rgb*-1.0 + 1.0);
			float3 appendResult38 = (float3(-break41.x , break41.z , break41.y));
			float3 VAT_VertexNORMAL35 = appendResult38;
			v.normal = VAT_VertexNORMAL35;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			o.Albedo = tex2D( _MainTex, uv_MainTex ).rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18500
235;593;951;402;695.1831;-137.8441;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;4;-3006.942,754.6563;Inherit;False;2310.919;1680.564;VAT;37;41;40;39;38;37;36;35;34;33;32;31;30;28;27;26;25;24;23;22;21;20;19;18;17;16;15;14;13;12;11;10;9;8;7;6;5;44;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-2670.071,898.6562;Inherit;False;Property;_Speed;Speed;6;0;Create;True;0;0;False;0;False;0;0.25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;5;-2669.445,805.9743;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-2550.06,1075.122;Inherit;False;Property;_FrameCount;FrameCount;5;0;Create;True;0;0;False;0;False;100;100;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-2458.071,804.6563;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;9;-2316.803,955.3342;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;10;-2262.447,834.9744;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;-2124.947,840.0744;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CeilOpNode;12;-1914.955,841.6224;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;13;-1761.992,843.7702;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;15;-1526.944,1029.635;Inherit;False;2;0;FLOAT;-1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;14;-1531.666,861.8353;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;16;-1367.944,863.6353;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;17;-1205.889,865.2153;Inherit;False;CurrentFrame;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;19;-2849.678,1322.113;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;18;-2829.1,1449.155;Inherit;False;17;CurrentFrame;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;20;-2573.1,1351.155;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;21;-2409.205,1345.549;Inherit;False;UV_VAT;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;22;-2956.943,1677.111;Inherit;False;21;UV_VAT;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;24;-2755.574,1656.722;Inherit;True;Property;_flag1pos;flag1.pos;3;0;Create;True;0;0;False;0;False;-1;0680b1a9af24f4442b4e252b528cf1db;0680b1a9af24f4442b4e252b528cf1db;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;23;-2440.948,1815.438;Inherit;False;Property;_BoundingMax;BoundingMax;8;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-2435.948,1904.438;Inherit;False;Property;_BoundingMin;BoundingMin;9;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;26;-2370.966,1709.872;Inherit;False;FLOAT4;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;27;-2239.613,1821.102;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;37;-2609.978,2205.22;Inherit;True;Property;_flag1norm;flag1.norm;4;0;Create;True;0;0;False;0;False;-1;908c9706f47c33a4aa4a2f048c2d4f90;908c9706f47c33a4aa4a2f048c2d4f90;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-2066.005,1667.115;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SwizzleNode;36;-2264.61,2207.862;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;44;-1866.269,1880.673;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;40;-2038.693,2216.58;Inherit;True;3;0;FLOAT3;0,0,0;False;1;FLOAT;-1;False;2;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;30;-1575.599,1658.496;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.BreakToComponentsNode;41;-1759.886,2216.56;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.NegateNode;31;-1321.626,1659.852;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;39;-1492.629,2216.264;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;32;-1123.526,1660.352;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;38;-1329.862,2217.314;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;35;-1174.383,2207.988;Inherit;False;VAT_VertexNORMAL;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;33;-938.0226,1654.422;Inherit;False;VAT_VertexOffset;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;43;-393.209,364.7468;Inherit;False;35;VAT_VertexNORMAL;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-2920.193,1114.844;Inherit;False;Property;_Play;Play;7;1;[IntRange];Create;True;0;0;False;0;False;0;100;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-557.8522,-160.7476;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;False;0;False;-1;de5969b59a7d5db48b198da3aa63c061;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;42;-396.209,273.7468;Inherit;False;33;VAT_VertexOffset;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-538.6552,148.8452;Inherit;False;Property;_Smoothness;Smoothness;1;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;2;-540.3555,66.32637;Inherit;False;Property;_Metallic;Metallic;2;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Flag_Burn;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;8;0;5;0
WireConnection;8;1;6;0
WireConnection;9;0;7;0
WireConnection;10;0;8;0
WireConnection;11;0;10;0
WireConnection;11;1;9;0
WireConnection;12;0;11;0
WireConnection;13;0;12;0
WireConnection;15;1;7;0
WireConnection;14;0;13;0
WireConnection;14;1;7;0
WireConnection;16;0;14;0
WireConnection;16;1;15;0
WireConnection;17;0;16;0
WireConnection;20;0;19;1
WireConnection;20;1;18;0
WireConnection;21;0;20;0
WireConnection;24;1;22;0
WireConnection;26;0;24;0
WireConnection;27;0;23;0
WireConnection;27;1;25;0
WireConnection;28;0;26;0
WireConnection;28;1;27;0
WireConnection;36;0;37;0
WireConnection;44;0;28;0
WireConnection;44;1;25;0
WireConnection;40;0;36;0
WireConnection;30;0;44;0
WireConnection;41;0;40;0
WireConnection;31;0;30;0
WireConnection;39;0;41;0
WireConnection;32;0;31;0
WireConnection;32;1;30;2
WireConnection;32;2;30;1
WireConnection;38;0;39;0
WireConnection;38;1;41;2
WireConnection;38;2;41;1
WireConnection;35;0;38;0
WireConnection;33;0;32;0
WireConnection;0;0;1;0
WireConnection;0;3;2;0
WireConnection;0;4;3;0
WireConnection;0;11;42;0
WireConnection;0;12;43;0
ASEEND*/
//CHKSM=117EA3823B247736EBB731827359023C6740BDDA