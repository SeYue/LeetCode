// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "VAT"
{
	Properties
	{
		_flag1pos("flag1.pos", 2D) = "white" {}
		_FrameCount("FrameCount", Float) = 100
		_Speed("Speed", Float) = 0
		_BoundingMax("BoundingMax", Float) = 1
		_BoundingMin("BoundingMin", Float) = 0
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
			half filler;
		};

		uniform sampler2D _flag1pos;
		uniform float _Speed;
		uniform float _FrameCount;
		uniform float _BoundingMax;
		uniform float _BoundingMin;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float CurrentFrame7 = ( ( -ceil( ( frac( ( _Time.y * _Speed ) ) * ( _FrameCount - 1.0 ) ) ) / _FrameCount ) + ( -1.0 / _FrameCount ) );
			float2 appendResult9 = (float2(v.texcoord1.xy.x , CurrentFrame7));
			float2 UV_VAT10 = appendResult9;
			float4 temp_cast_0 = (_BoundingMin).xxxx;
			float4 break17 = ( ( (tex2Dlod( _flag1pos, float4( UV_VAT10, 0, 0.0) )).rgba * ( _BoundingMax - _BoundingMin ) ) - temp_cast_0 );
			float4 appendResult19 = (float4(-break17.x , break17.z , break17.y , 0.0));
			float4 VAT_VertexOffset40 = appendResult19;
			v.vertex.xyz += VAT_VertexOffset40.xyz;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18500
144;734;951;483;4189.719;1697.581;7.693256;False;False
Node;AmplifyShaderEditor.CommentaryNode;47;-1047.664,-1125.825;Inherit;False;2310.919;1680.564;VAT;37;11;42;2;43;44;13;45;46;17;20;19;40;31;37;35;39;36;38;1;28;22;4;29;30;23;24;26;14;6;15;16;7;5;8;3;9;10;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleTimeNode;22;-710.1667,-1074.507;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-710.7925,-981.8251;Inherit;False;Property;_Speed;Speed;4;0;Create;True;0;0;False;0;False;0;0.25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-498.7924,-1075.825;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-590.782,-805.3592;Inherit;False;Property;_FrameCount;FrameCount;2;0;Create;True;0;0;False;0;False;100;100;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;30;-357.5251,-925.1471;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;23;-303.1687,-1045.507;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-165.6689,-1040.407;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CeilOpNode;26;44.32275,-1038.859;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;14;197.2855,-1036.711;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;6;427.612,-1018.646;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;15;432.3339,-850.8463;Inherit;False;2;0;FLOAT;-1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;16;591.3339,-1016.846;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;7;753.3892,-1015.266;Inherit;True;CurrentFrame;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;3;-890.4,-558.3683;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;8;-869.8212,-431.326;Inherit;False;7;CurrentFrame;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;9;-613.8212,-529.326;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;10;-449.9261,-534.9322;Inherit;False;UV_VAT;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;11;-997.6641,-203.3701;Inherit;False;10;UV_VAT;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;42;-481.6694,-65.04301;Inherit;False;Property;_BoundingMax;BoundingMax;5;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-796.296,-223.7591;Inherit;True;Property;_flag1pos;flag1.pos;1;0;Create;True;0;0;False;0;False;-1;0680b1a9af24f4442b4e252b528cf1db;0680b1a9af24f4442b4e252b528cf1db;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;43;-476.6694,23.957;Inherit;False;Property;_BoundingMin;BoundingMin;6;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;13;-433.874,-220.1019;Inherit;False;FLOAT4;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;44;-255.6692,-121.043;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;32.27228,-216.3661;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;46;218.5316,-215.2499;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.BreakToComponentsNode;17;383.6789,-221.9853;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.NegateNode;20;637.652,-220.6296;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;19;835.752,-220.1296;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;40;1021.255,-226.0596;Inherit;False;VAT_VertexOffset;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.NegateNode;37;507.5692,327.8447;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;38;712.6864,332.3843;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;1;-652.6997,324.7392;Inherit;True;Property;_flag1norm;flag1.norm;0;0;Create;True;0;0;False;0;False;-1;908c9706f47c33a4aa4a2f048c2d4f90;908c9706f47c33a4aa4a2f048c2d4f90;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SwizzleNode;31;-262.3311,338.3803;Inherit;False;FLOAT4;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;41;2199.528,178.2567;Inherit;False;40;VAT_VertexOffset;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-960.915,-765.6367;Inherit;False;Property;_Play;Play;3;1;[IntRange];Create;True;0;0;False;0;False;0;100;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;35;-26.54755,341.5167;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT;2;False;2;FLOAT;-1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;39;868.1659,323.0581;Inherit;False;VAT_VertexNORMAL;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.BreakToComponentsNode;36;200.5923,331.4789;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2462.305,-104.7692;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;VAT;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;29;0;22;0
WireConnection;29;1;28;0
WireConnection;30;0;4;0
WireConnection;23;0;29;0
WireConnection;24;0;23;0
WireConnection;24;1;30;0
WireConnection;26;0;24;0
WireConnection;14;0;26;0
WireConnection;6;0;14;0
WireConnection;6;1;4;0
WireConnection;15;1;4;0
WireConnection;16;0;6;0
WireConnection;16;1;15;0
WireConnection;7;0;16;0
WireConnection;9;0;3;1
WireConnection;9;1;8;0
WireConnection;10;0;9;0
WireConnection;2;1;11;0
WireConnection;13;0;2;0
WireConnection;44;0;42;0
WireConnection;44;1;43;0
WireConnection;45;0;13;0
WireConnection;45;1;44;0
WireConnection;46;0;45;0
WireConnection;46;1;43;0
WireConnection;17;0;46;0
WireConnection;20;0;17;0
WireConnection;19;0;20;0
WireConnection;19;1;17;2
WireConnection;19;2;17;1
WireConnection;40;0;19;0
WireConnection;37;0;36;0
WireConnection;38;0;37;0
WireConnection;38;1;36;2
WireConnection;38;2;36;1
WireConnection;31;0;1;0
WireConnection;35;0;31;0
WireConnection;39;0;38;0
WireConnection;36;0;35;0
WireConnection;0;11;41;0
ASEEND*/
//CHKSM=EE35EB52A1423A5AFFA189DBAD0B95FFD0A9B849