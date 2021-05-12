// Upgrade NOTE: upgraded instancing buffer 'ASE_CustomOutlineTint_HillBoxGold' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ASE_Custom/OutlineTint_HillBoxGold"
{
	Properties
	{
		_ASEOutlineWidth( "Outline Width", Float ) = 0.03
		_ASEOutlineColor( "Outline Color", Color ) = (0,0,0,0)
		_Albedo("Albedo", 2D) = "white" {}
		_Tint("Tint", Color) = (1,1,1,1)
		[HDR]_Emission("Emission", Color) = (0,0,0,0)
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Intense("Intense", Float) = 2
		_FullGoldEmission("FullGoldEmission", 2D) = "black" {}
		_Noise("Noise", 2D) = "white" {}
		_GoldRatio("GoldRatio", Range( 0 , 1)) = 0
		_RemapMin("RemapMin", Range( 0 , 1)) = 0
		_RemapMinOld("RemapMinOld", Range( 0 , 1)) = 0
		_RemapMax("RemapMax", Range( 0 , 1)) = 1
		_RemapMaxOld("RemapMaxOld", Range( 0 , 1)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ }
		Cull Front
		CGPROGRAM
		#pragma target 3.0
		#pragma surface outlineSurf Outline nofog  keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nometa noforwardadd vertex:outlineVertexDataFunc 
		
		
		
		struct Input {
			half filler;
		};
		UNITY_INSTANCING_BUFFER_START(ASE_CustomOutlineTint_HillBoxGold)
		UNITY_DEFINE_INSTANCED_PROP( half4, _ASEOutlineColor )
#define _ASEOutlineColor_arr ASE_CustomOutlineTint_HillBoxGold
		UNITY_DEFINE_INSTANCED_PROP(half, _ASEOutlineWidth)
#define _ASEOutlineWidth_arr ASE_CustomOutlineTint_HillBoxGold
		UNITY_INSTANCING_BUFFER_END(ASE_CustomOutlineTint_HillBoxGold)
		void outlineVertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			v.vertex.xyz *= ( 1 + UNITY_ACCESS_INSTANCED_PROP(_ASEOutlineWidth_arr, _ASEOutlineWidth));
		}
		inline half4 LightingOutline( SurfaceOutput s, half3 lightDir, half atten ) { return half4 ( 0,0,0, s.Alpha); }
		void outlineSurf( Input i, inout SurfaceOutput o )
		{
			o.Emission = UNITY_ACCESS_INSTANCED_PROP(_ASEOutlineColor_arr, _ASEOutlineColor).rgb;
			o.Alpha = 1;
		}
		ENDCG
		

		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		ZTest LEqual
		CGPROGRAM
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Albedo;
		uniform half4 _Tint;
		uniform half4 _Emission;
		uniform sampler2D _FullGoldEmission;
		uniform sampler2D _Noise;
		uniform half _RemapMinOld;
		uniform half _RemapMaxOld;
		uniform half _RemapMin;
		uniform half _RemapMax;
		uniform half _Intense;
		uniform half _Metallic;
		uniform half _Smoothness;

		UNITY_INSTANCING_BUFFER_START(ASE_CustomOutlineTint_HillBoxGold)
			UNITY_DEFINE_INSTANCED_PROP(half4, _Albedo_ST)
#define _Albedo_ST_arr ASE_CustomOutlineTint_HillBoxGold
			UNITY_DEFINE_INSTANCED_PROP(half4, _FullGoldEmission_ST)
#define _FullGoldEmission_ST_arr ASE_CustomOutlineTint_HillBoxGold
			UNITY_DEFINE_INSTANCED_PROP(half4, _Noise_ST)
#define _Noise_ST_arr ASE_CustomOutlineTint_HillBoxGold
			UNITY_DEFINE_INSTANCED_PROP(half, _GoldRatio)
#define _GoldRatio_arr ASE_CustomOutlineTint_HillBoxGold
		UNITY_INSTANCING_BUFFER_END(ASE_CustomOutlineTint_HillBoxGold)

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			half4 _Albedo_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_Albedo_ST_arr, _Albedo_ST);
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST_Instance.xy + _Albedo_ST_Instance.zw;
			o.Albedo = ( tex2D( _Albedo, uv_Albedo ) * _Tint ).rgb;
			half4 _FullGoldEmission_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_FullGoldEmission_ST_arr, _FullGoldEmission_ST);
			float2 uv_FullGoldEmission = i.uv_texcoord * _FullGoldEmission_ST_Instance.xy + _FullGoldEmission_ST_Instance.zw;
			half4 _Noise_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_Noise_ST_arr, _Noise_ST);
			float2 uv_Noise = i.uv_texcoord * _Noise_ST_Instance.xy + _Noise_ST_Instance.zw;
			half _GoldRatio_Instance = UNITY_ACCESS_INSTANCED_PROP(_GoldRatio_arr, _GoldRatio);
			o.Emission = ( _Emission + ( ( tex2D( _FullGoldEmission, uv_FullGoldEmission ) * step( tex2D( _Noise, uv_Noise ).r , (_RemapMin + (_GoldRatio_Instance - _RemapMinOld) * (_RemapMax - _RemapMin) / (_RemapMaxOld - _RemapMinOld)) ) ) * _Intense ) ).rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=17500
1920;-336.6667;2560;1388.333;1743.529;53.48128;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;36;-1345.558,362.5705;Inherit;False;958.4644;727.7797;Gold Emission;9;39;37;29;35;28;23;40;41;34;;0.4245283,0.4245283,0.4245283,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-714.3459,926.0902;Inherit;False;Property;_RemapMinOld;RemapMinOld;10;0;Create;True;0;0;False;0;0;0.36;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-712.3459,1005.091;Inherit;False;Property;_RemapMaxOld;RemapMaxOld;12;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-1242.894,801.9502;Inherit;False;InstancedProperty;_GoldRatio;GoldRatio;8;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;38;-1246.346,932.5905;Inherit;False;Property;_RemapMin;RemapMin;9;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;39;-1244.346,1011.591;Inherit;False;Property;_RemapMax;RemapMax;11;0;Create;True;0;0;False;0;1;0.078;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;28;-1136.758,597.0701;Inherit;True;Property;_Noise;Noise;7;0;Create;True;0;0;False;0;-1;fecbc2dbf76d9ab4089330c3e5ee2423;c20c2cf4606c0d24f84e531029b7d0f7;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;37;-907.125,798.3576;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;0.91;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;29;-811.7938,543.3503;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0.490566;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;23;-1137.558,409.5705;Inherit;True;Property;_FullGoldEmission;FullGoldEmission;6;0;Create;True;0;0;False;0;-1;7ab00d43c16c90748b54204e61827daa;7ab00d43c16c90748b54204e61827daa;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;8;-335.7,668.3209;Inherit;False;Property;_Intense;Intense;5;0;Create;True;0;0;False;0;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-590.0934,473.2501;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;4;-197.2718,386.727;Inherit;False;Property;_Emission;Emission;2;1;[HDR];Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;2;-327.5,80.59998;Inherit;False;Property;_Tint;Tint;1;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-375.5,-159.4;Inherit;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;-1;None;492803ad332207441addf82cfc72cde7;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;-135.6261,588.1339;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;15;151.928,467.1594;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-7.499994,-63.39999;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-30.6718,826.927;Inherit;False;Property;_Smoothness;Smoothness;3;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-27.6718,742.9271;Inherit;False;Property;_Metallic;Metallic;4;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;333.2,410.6;Half;False;True;-1;2;;0;0;Standard;ASE_Custom/OutlineTint_HillBoxGold;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;True;0.03;0,0,0,0;VertexScale;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;37;0;34;0
WireConnection;37;1;40;0
WireConnection;37;2;41;0
WireConnection;37;3;38;0
WireConnection;37;4;39;0
WireConnection;29;0;28;1
WireConnection;29;1;37;0
WireConnection;35;0;23;0
WireConnection;35;1;29;0
WireConnection;11;0;35;0
WireConnection;11;1;8;0
WireConnection;15;0;4;0
WireConnection;15;1;11;0
WireConnection;3;0;1;0
WireConnection;3;1;2;0
WireConnection;0;0;3;0
WireConnection;0;2;15;0
WireConnection;0;3;5;0
WireConnection;0;4;6;0
ASEEND*/
//CHKSM=C493E408D7FD97872B16D7DA43D147F94A38BA1F