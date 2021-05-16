// Upgrade NOTE: upgraded instancing buffer 'FrostyIcicle' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FrostyIcicle"
{
	Properties
	{
		_BaseAlbedo("BaseAlbedo", 2D) = "black" {}
		_FrostTexture("FrostTexture", 2D) = "white" {}
		_IceSlider("IceSlider", Range( 0 , 1)) = 0.4083336
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _BaseAlbedo;
		uniform sampler2D _FrostTexture;

		UNITY_INSTANCING_BUFFER_START(FrostyIcicle)
			UNITY_DEFINE_INSTANCED_PROP(float4, _BaseAlbedo_ST)
#define _BaseAlbedo_ST_arr FrostyIcicle
			UNITY_DEFINE_INSTANCED_PROP(float4, _FrostTexture_ST)
#define _FrostTexture_ST_arr FrostyIcicle
			UNITY_DEFINE_INSTANCED_PROP(float, _IceSlider)
#define _IceSlider_arr FrostyIcicle
		UNITY_INSTANCING_BUFFER_END(FrostyIcicle)

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			float _IceSlider_Instance = UNITY_ACCESS_INSTANCED_PROP(_IceSlider_arr, _IceSlider);
			float IceSlider10 = _IceSlider_Instance;
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			float yMask15 = saturate( ( IceSlider10 * ( ase_worldNormal.y * -0.2 ) ) );
			float3 VertexNormal20 = ( ase_vertexNormal * yMask15 );
			v.vertex.xyz += VertexNormal20;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 _BaseAlbedo_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_BaseAlbedo_ST_arr, _BaseAlbedo_ST);
			float2 uv_BaseAlbedo = i.uv_texcoord * _BaseAlbedo_ST_Instance.xy + _BaseAlbedo_ST_Instance.zw;
			float4 _FrostTexture_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_FrostTexture_ST_arr, _FrostTexture_ST);
			float2 uv_FrostTexture = i.uv_texcoord * _FrostTexture_ST_Instance.xy + _FrostTexture_ST_Instance.zw;
			float _IceSlider_Instance = UNITY_ACCESS_INSTANCED_PROP(_IceSlider_arr, _IceSlider);
			float4 lerpResult3 = lerp( tex2D( _BaseAlbedo, uv_BaseAlbedo ) , tex2D( _FrostTexture, uv_FrostTexture ) , _IceSlider_Instance);
			float4 Ice5 = lerpResult3;
			o.Albedo = Ice5.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17500
2172;-158;1832;1335;1650.984;1414.282;1.660353;True;True
Node;AmplifyShaderEditor.CommentaryNode;7;-879.1149,-1309.152;Inherit;False;913.4299;577;Albedo;6;1;2;4;3;5;10;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-829.1149,-847.1517;Inherit;False;InstancedProperty;_IceSlider;IceSlider;2;0;Create;True;0;0;False;0;0.4083336;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;16;-1083.747,-557.2431;Inherit;False;1271.695;430.7175;yMask;6;8;9;12;14;11;15;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldNormalVector;8;-1033.747,-391.5894;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;10;-491.152,-856.9429;Inherit;False;IceSlider;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;11;-762.8522,-507.2431;Inherit;False;10;IceSlider;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-772.2873,-379.5256;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;-0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-505.4522,-457.8427;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;14;-248.8808,-461.6711;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;15;-40.05205,-469.5423;Inherit;False;yMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;21;-906.4526,-33.58443;Inherit;False;683.3852;315.9993;Vertex Offset;4;18;17;19;20;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;2;-829.1149,-1054.152;Inherit;True;Property;_FrostTexture;FrostTexture;1;0;Create;True;0;0;False;0;-1;31e4798fe80834744a0cdadd42f843ee;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-829.1149,-1259.152;Inherit;True;Property;_BaseAlbedo;BaseAlbedo;0;0;Create;True;0;0;False;0;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;18;-851.0113,167.4149;Inherit;False;15;yMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;17;-856.4526,16.41557;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;3;-472.1149,-1188.152;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-636.0754,38.18119;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;20;-466.0312,35.46049;Inherit;False;VertexNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;5;-193.685,-1195.78;Inherit;False;Ice;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;22;590.9623,-134.5836;Inherit;False;20;VertexNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;6;608.0991,-408.4084;Inherit;False;5;Ice;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;839.9308,-410.6682;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;FrostyIcicle;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;10;0;4;0
WireConnection;9;0;8;2
WireConnection;12;0;11;0
WireConnection;12;1;9;0
WireConnection;14;0;12;0
WireConnection;15;0;14;0
WireConnection;3;0;1;0
WireConnection;3;1;2;0
WireConnection;3;2;4;0
WireConnection;19;0;17;0
WireConnection;19;1;18;0
WireConnection;20;0;19;0
WireConnection;5;0;3;0
WireConnection;0;0;6;0
WireConnection;0;11;22;0
ASEEND*/
//CHKSM=3E7A0C4DA93EDB53AADE3E7F08B8CF27AB93164D