// Upgrade NOTE: upgraded instancing buffer 'ASE_CustomMarchingSquareTerrain' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ASE_Custom/MarchingSquareTerrain"
{
	Properties
	{
		_UV_Index_X("UV_Index_X", Float) = 0
		_UV_Index_Y("UV_Index_Y", Float) = 0
		_Albedo("Albedo", 2DArray ) = "" {}
		_TextureIndex("TextureIndex", Int) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		ZTest LEqual
		CGPROGRAM
		#pragma target 3.5
		#pragma multi_compile_instancing
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform UNITY_DECLARE_TEX2DARRAY( _Albedo );

		UNITY_INSTANCING_BUFFER_START(ASE_CustomMarchingSquareTerrain)
			UNITY_DEFINE_INSTANCED_PROP(half, _UV_Index_X)
#define _UV_Index_X_arr ASE_CustomMarchingSquareTerrain
			UNITY_DEFINE_INSTANCED_PROP(half, _UV_Index_Y)
#define _UV_Index_Y_arr ASE_CustomMarchingSquareTerrain
			UNITY_DEFINE_INSTANCED_PROP(int, _TextureIndex)
#define _TextureIndex_arr ASE_CustomMarchingSquareTerrain
		UNITY_INSTANCING_BUFFER_END(ASE_CustomMarchingSquareTerrain)

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			half _UV_Index_X_Instance = UNITY_ACCESS_INSTANCED_PROP(_UV_Index_X_arr, _UV_Index_X);
			half _UV_Index_Y_Instance = UNITY_ACCESS_INSTANCED_PROP(_UV_Index_Y_arr, _UV_Index_Y);
			half4 appendResult8 = (half4(( _UV_Index_X_Instance * 0.25 ) , ( ( 3.0 - _UV_Index_Y_Instance ) * 0.25 ) , 0.0 , 0.0));
			float2 uv_TexCoord4 = i.uv_texcoord * float2( 0.23,0.23 ) + ( appendResult8 + half4( half2( 0.01,0.01 ), 0.0 , 0.0 ) ).xy;
			int _TextureIndex_Instance = UNITY_ACCESS_INSTANCED_PROP(_TextureIndex_arr, _TextureIndex);
			half4 texArray16 = UNITY_SAMPLE_TEX2DARRAY(_Albedo, float3(uv_TexCoord4, (float)_TextureIndex_Instance)  );
			o.Albedo = texArray16.xyz;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=17500
1920;-388.6667;2560;1382.333;1985.173;744.5711;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;9;-1692,3;Inherit;False;InstancedProperty;_UV_Index_Y;UV_Index_Y;1;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;11;-1484,-13;Inherit;False;2;0;FLOAT;3;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-1544.173,-204.5705;Inherit;False;InstancedProperty;_UV_Index_X;UV_Index_X;0;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-1324,-13;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.25;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-1336.173,-200.5705;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.25;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;8;-1164,-77;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector2Node;13;-1084,83;Inherit;False;Constant;_Vector0;Vector 0;3;0;Create;True;0;0;False;0;0.01,0.01;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleAddOpNode;14;-876,35;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;4;-706.173,-12.5705;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;0.23,0.23;False;1;FLOAT2;0.75,0.5;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.IntNode;18;-689.173,178.4293;Inherit;False;InstancedProperty;_TextureIndex;TextureIndex;3;0;Create;True;0;0;False;0;0;0;0;1;INT;0
Node;AmplifyShaderEditor.TextureArrayNode;16;-429.173,-33.57068;Inherit;True;Property;_Albedo;Albedo;2;0;Create;True;0;0;False;0;None;0;Object;-1;Auto;False;7;6;SAMPLER2D;;False;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-73.79999,-32.39999;Half;False;True;-1;3;;0;0;Standard;ASE_Custom/MarchingSquareTerrain;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;0.03;0,0,0,0;VertexScale;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;11;1;9;0
WireConnection;10;0;11;0
WireConnection;6;0;7;0
WireConnection;8;0;6;0
WireConnection;8;1;10;0
WireConnection;14;0;8;0
WireConnection;14;1;13;0
WireConnection;4;1;14;0
WireConnection;16;0;4;0
WireConnection;16;1;18;0
WireConnection;0;0;16;0
ASEEND*/
//CHKSM=194D7B4B3288BD847AC3D426BEBC4489DCF01696