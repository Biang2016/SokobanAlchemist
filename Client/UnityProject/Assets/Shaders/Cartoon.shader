// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:3,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:False,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:9361,x:34710,y:32353,varname:node_9361,prsc:2|diff-4540-OUT,spec-564-OUT,gloss-5398-OUT,normal-7180-RGB,emission-5111-OUT,olwid-2022-OUT,olcol-7510-RGB;n:type:ShaderForge.SFN_Slider,id:7554,x:31808,y:32166,ptovrint:False,ptlb:Threshold,ptin:_Threshold,varname:node_7554,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.5,max:1;n:type:ShaderForge.SFN_RemapRange,id:6623,x:32228,y:32123,varname:node_6623,prsc:2,frmn:0,frmx:1,tomn:-1,tomx:1|IN-7554-OUT;n:type:ShaderForge.SFN_Step,id:6587,x:32535,y:32287,varname:node_6587,prsc:2|A-6623-OUT,B-1993-OUT;n:type:ShaderForge.SFN_Dot,id:1993,x:32228,y:32468,varname:node_1993,prsc:2,dt:0|A-8749-OUT,B-7742-OUT;n:type:ShaderForge.SFN_NormalVector,id:7742,x:31930,y:32581,prsc:2,pt:False;n:type:ShaderForge.SFN_Step,id:4452,x:32522,y:32636,varname:node_4452,prsc:2|A-6623-OUT,B-539-OUT;n:type:ShaderForge.SFN_Vector1,id:3209,x:32116,y:33042,varname:node_3209,prsc:2,v1:0.2;n:type:ShaderForge.SFN_Add,id:9303,x:32835,y:32437,varname:node_9303,prsc:2|A-6587-OUT,B-4452-OUT;n:type:ShaderForge.SFN_Multiply,id:3668,x:33150,y:32426,varname:node_3668,prsc:2|A-9303-OUT,B-191-OUT;n:type:ShaderForge.SFN_Vector1,id:191,x:32898,y:32630,varname:node_191,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Multiply,id:4540,x:33679,y:32287,varname:node_4540,prsc:2|A-8885-RGB,B-3668-OUT;n:type:ShaderForge.SFN_Tex2d,id:8885,x:33359,y:32195,ptovrint:False,ptlb:Main_tex,ptin:_Main_tex,varname:node_8885,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:77ac91dd42a49504c89d8228199f2bbd,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:7180,x:33802,y:32689,ptovrint:False,ptlb:Normal_tex,ptin:_Normal_tex,varname:node_7180,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:45916704a0ea9f340ad748fb82fb5cb2,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Slider,id:564,x:33626,y:32457,ptovrint:False,ptlb:Metallic,ptin:_Metallic,varname:node_564,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Slider,id:5398,x:33645,y:32562,ptovrint:False,ptlb:Gloss,ptin:_Gloss,varname:_Metallic_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Slider,id:8535,x:33892,y:32873,ptovrint:False,ptlb:Outline_Width,ptin:_Outline_Width,varname:node_8535,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Multiply,id:2022,x:34280,y:32872,varname:node_2022,prsc:2|A-8535-OUT,B-4956-OUT;n:type:ShaderForge.SFN_Vector1,id:4956,x:33987,y:32964,varname:node_4956,prsc:2,v1:0.05;n:type:ShaderForge.SFN_Dot,id:539,x:32213,y:32711,varname:node_539,prsc:2,dt:0|A-8749-OUT,B-6029-OUT;n:type:ShaderForge.SFN_NormalVector,id:6029,x:31918,y:32814,prsc:2,pt:True;n:type:ShaderForge.SFN_LightVector,id:8749,x:31886,y:32431,varname:node_8749,prsc:2;n:type:ShaderForge.SFN_Fresnel,id:5406,x:33159,y:31924,varname:node_5406,prsc:2|EXP-3756-OUT;n:type:ShaderForge.SFN_Slider,id:3756,x:32681,y:32048,ptovrint:False,ptlb:Fresnel_fanwei,ptin:_Fresnel_fanwei,varname:node_3756,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Multiply,id:5772,x:33809,y:31951,varname:node_5772,prsc:2|A-4517-RGB,B-953-OUT,C-423-OUT;n:type:ShaderForge.SFN_Power,id:953,x:33405,y:31968,varname:node_953,prsc:2|VAL-5406-OUT,EXP-4468-OUT;n:type:ShaderForge.SFN_Add,id:5111,x:34283,y:32144,varname:node_5111,prsc:2|A-8137-OUT,B-4540-OUT;n:type:ShaderForge.SFN_Color,id:4517,x:33508,y:31796,ptovrint:False,ptlb:Fresnel_Color,ptin:_Fresnel_Color,varname:node_4517,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.3161765,c2:0.5755578,c3:1,c4:1;n:type:ShaderForge.SFN_ValueProperty,id:4468,x:33080,y:32132,ptovrint:False,ptlb:Power,ptin:_Power,varname:node_4468,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2;n:type:ShaderForge.SFN_ValueProperty,id:423,x:33546,y:32120,ptovrint:False,ptlb:liangdu,ptin:_liangdu,varname:node_423,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Posterize,id:8137,x:34039,y:31966,varname:node_8137,prsc:2|IN-5772-OUT,STPS-7896-OUT;n:type:ShaderForge.SFN_ValueProperty,id:7896,x:33770,y:32148,ptovrint:False,ptlb:Fresnel_Step,ptin:_Fresnel_Step,varname:node_7896,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2;n:type:ShaderForge.SFN_NormalVector,id:8512,x:32807,y:31862,prsc:2,pt:True;n:type:ShaderForge.SFN_Color,id:7510,x:34193,y:33103,ptovrint:False,ptlb:Outline_Color,ptin:_Outline_Color,varname:node_7510,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0,c3:0,c4:1;proporder:7554-8885-7180-564-5398-8535-3756-4517-4468-423-7896-7510;pass:END;sub:END;*/

Shader "Shader Forge/Cartoon1" {
    Properties {
        _Threshold ("Threshold", Range(0, 1)) = 0.5
        _Main_tex ("Main_tex", 2D) = "white" {}
        _Normal_tex ("Normal_tex", 2D) = "bump" {}
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Gloss ("Gloss", Range(0, 1)) = 0
        _Outline_Width ("Outline_Width", Range(0, 1)) = 0
        _Fresnel_fanwei ("Fresnel_fanwei", Range(0, 1)) = 1
        _Fresnel_Color ("Fresnel_Color", Color) = (0.3161765,0.5755578,1,1)
        _Power ("Power", Float ) = 2
        _liangdu ("liangdu", Float ) = 1
        _Fresnel_Step ("Fresnel_Step", Float ) = 2
        _Outline_Color ("Outline_Color", Color) = (0,0,0,1)
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "Outline"
            Tags {
            }
            Cull Front
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float _Outline_Width;
            uniform float4 _Outline_Color;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                UNITY_FOG_COORDS(0)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos( float4(v.vertex.xyz + v.normal*(_Outline_Width*0.05),1) );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                return fixed4(_Outline_Color.rgb,0);
            }
            ENDCG
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float _Threshold;
            uniform sampler2D _Main_tex; uniform float4 _Main_tex_ST;
            uniform sampler2D _Normal_tex; uniform float4 _Normal_tex_ST;
            uniform float _Metallic;
            uniform float _Gloss;
            uniform float _Fresnel_fanwei;
            uniform float4 _Fresnel_Color;
            uniform float _Power;
            uniform float _liangdu;
            uniform float _Fresnel_Step;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
                UNITY_FOG_COORDS(7)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _Normal_tex_var = UnpackNormal(tex2D(_Normal_tex,TRANSFORM_TEX(i.uv0, _Normal_tex)));
                float3 normalLocal = _Normal_tex_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float gloss = _Gloss;
                float perceptualRoughness = 1.0 - _Gloss;
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float3 specularColor = _Metallic;
                float specularMonochrome;
                float4 _Main_tex_var = tex2D(_Main_tex,TRANSFORM_TEX(i.uv0, _Main_tex));
                float node_6623 = (_Threshold*2.0+-1.0);
                float3 node_4540 = (_Main_tex_var.rgb*((step(node_6623,dot(lightDirection,i.normalDir))+step(node_6623,dot(lightDirection,normalDirection)))*0.5));
                float3 diffuseColor = node_4540; // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
////// Emissive:
                float3 emissive = (floor((_Fresnel_Color.rgb*pow(pow(1.0-max(0,dot(normalDirection, viewDirection)),_Fresnel_fanwei),_Power)*_liangdu) * _Fresnel_Step) / (_Fresnel_Step - 1)+node_4540);
/// Final Color:
                float3 finalColor = diffuse + specular + emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float _Threshold;
            uniform sampler2D _Main_tex; uniform float4 _Main_tex_ST;
            uniform sampler2D _Normal_tex; uniform float4 _Normal_tex_ST;
            uniform float _Metallic;
            uniform float _Gloss;
            uniform float _Fresnel_fanwei;
            uniform float4 _Fresnel_Color;
            uniform float _Power;
            uniform float _liangdu;
            uniform float _Fresnel_Step;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
                UNITY_FOG_COORDS(7)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _Normal_tex_var = UnpackNormal(tex2D(_Normal_tex,TRANSFORM_TEX(i.uv0, _Normal_tex)));
                float3 normalLocal = _Normal_tex_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float gloss = _Gloss;
                float perceptualRoughness = 1.0 - _Gloss;
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float3 specularColor = _Metallic;
                float specularMonochrome;
                float4 _Main_tex_var = tex2D(_Main_tex,TRANSFORM_TEX(i.uv0, _Main_tex));
                float node_6623 = (_Threshold*2.0+-1.0);
                float3 node_4540 = (_Main_tex_var.rgb*((step(node_6623,dot(lightDirection,i.normalDir))+step(node_6623,dot(lightDirection,normalDirection)))*0.5));
                float3 diffuseColor = node_4540; // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
