Shader "Baked Shadergraphs/TMP_SDF-URP Lit"
{
    Properties
    {
        [NoScaleOffset]_MainTex("_MainTex", 2D) = "white" {}
        _OutlineOffset1("_OutlineOffset1", Vector) = (0, 0, 0, 0)
        _OutlineOffset2("_OutlineOffset2", Vector) = (0, 0, 0, 0)
        _OutlineOffset3("_OutlineOffset3", Vector) = (0, 0, 0, 0)
        _GradientScale("_GradientScale", Float) = 10
        [HDR]_FaceColor("_FaceColor", Color) = (1, 1, 1, 1)
        _IsoPerimeter("Outline Width", Vector) = (0, 0, 0, 0)
        [HDR]_OutlineColor1("_OutlineColor1", Color) = (0, 1, 0.9600265, 1)
        [HDR]_OutlineColor2("_OutlineColor2", Color) = (0, 0.06557035, 1, 1)
        [HDR]_OutlineColor3("_OutlineColor3", Color) = (0, 0, 0, 0)
        [ToggleUI]_OutlineMode("_OutlineMode", Float) = 0
        _Softness("_Softness", Vector) = (0, 0, 0, 0)
        [NoScaleOffset]_FaceTex("_FaceTex", 2D) = "white" {}
        _FaceUVSpeed("_FaceUVSpeed", Vector) = (0, 0, 0, 0)
        _FaceTex_ST("_FaceTex_ST", Vector) = (2, 2, 0, 0)
        _OutlineTex_ST("_OutlineTex_ST", Vector) = (1, 1, 0, 0)
        _OutlineUVSpeed("_OutlineUVSpeed", Vector) = (0, 0, 0, 0)
        _UnderlayColor("_UnderlayColor", Color) = (0, 0, 0, 1)
        _UnderlayOffset("_UnderlayOffset", Vector) = (0, 0, 0, 0)
        _UnderlayDilate("_UnderlayDilate", Float) = 0
        _UnderlaySoftness("_UnderlaySoftness", Float) = 0
        [ToggleUI]_BevelType("_BevelType", Float) = 0
        _BevelAmount("_BevelAmount", Range(0, 1)) = 0
        _BevelOffset("_BevelOffset", Range(-0.5, 0.5)) = 0
        _BevelWidth("_BevelWidth", Range(0, 0.5)) = 0.5
        _BevelRoundness("_BevelRoundness", Range(0, 1)) = 0
        _BevelClamp("_BevelClamp", Range(0, 1)) = 0
        [HDR]_SpecularColor("Light color", Color) = (1, 1, 1, 1)
        _LightAngle("_LightAngle", Range(0, 6.28)) = 0
        _SpecularPower("_SpecularPower", Range(0, 4)) = 0
        _Reflectivity("Reflectivity Power", Range(5, 15)) = 5
        _Diffuse("Diffuse Shadow", Range(0, 1)) = 0.3
        _Ambient("Ambient Shadow", Range(0, 1)) = 0.3
        [NoScaleOffset]_OutlineTex("_OutlineTex", 2D) = "white" {}
        _ScaleRatioA("_ScaleRatioA", Float) = 0
        Emissive("Emissive", Color) = (0, 0, 0, 0)
        _smoothness("Smoothness", Range(0, 1)) = 0.5
        _Metalic("Metalic", Range(0, 1)) = 0
        [HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Lit"
            "Queue"="Transparent"
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalLitSubTarget"
        }
        Pass
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
        
        // Render State
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        //#pragma instancing_options renderinglayer
        //#pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag
        
        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>
        
        // Keywords
        //#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
        #pragma multi_compile _ _VOLUMETRICS_ENABLED
        #define _DISABLE_LIGHTMAPS
        #define _DISABLE_REFLECTIONPROBES
        #define _DISABLE_SSAO
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DefaultLitVariants.hlsl"
        /*
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DYNAMICLIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
        #pragma multi_compile_fragment _ _SHADOWS_SOFT
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
        #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile_fragment _ _LIGHT_LAYERS
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        #pragma multi_compile_fragment _ _LIGHT_COOKIES
        #pragma multi_compile _ _CLUSTERED_RENDERING
        */
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define ATTRIBUTES_NEED_TEXCOORD3
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD1
        #define VARYINGS_NEED_TEXCOORD2
        #define VARYINGS_NEED_TEXCOORD3
        #define VARYINGS_NEED_COLOR
        #define VARYINGS_NEED_VIEWDIRECTION_WS
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define VARYINGS_NEED_SHADOW_COORD
        #define VARYINGS_NEED_CULLFACE
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_FORWARD
        #define _FOG_FRAGMENT 1
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _ALPHATEST_ON 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
             float4 uv3 : TEXCOORD3;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
             float4 texCoord1;
             float4 texCoord2;
             float4 texCoord3;
             float4 color;
             float3 viewDirectionWS;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
             float4 fogFactorAndVertexLight;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
             float4 shadowCoord;
            #endif
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float4 uv0;
             float4 uv1;
             float4 uv2;
             float4 uv3;
             float4 VertexColor;
             float FaceSign;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 interp0 : INTERP0;
             float3 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
             float4 interp4 : INTERP4;
             float4 interp5 : INTERP5;
             float4 interp6 : INTERP6;
             float4 interp7 : INTERP7;
             float3 interp8 : INTERP8;
             float2 interp9 : INTERP9;
             float2 interp10 : INTERP10;
             float3 interp11 : INTERP11;
             float4 interp12 : INTERP12;
             float4 interp13 : INTERP13;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.tangentWS;
            output.interp3.xyzw =  input.texCoord0;
            output.interp4.xyzw =  input.texCoord1;
            output.interp5.xyzw =  input.texCoord2;
            output.interp6.xyzw =  input.texCoord3;
            output.interp7.xyzw =  input.color;
            output.interp8.xyz =  input.viewDirectionWS;
            #if defined(LIGHTMAP_ON)
            output.interp9.xy =  input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.interp10.xy =  input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.interp11.xyz =  input.sh;
            #endif
            output.interp12.xyzw =  input.fogFactorAndVertexLight;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.interp13.xyzw =  input.shadowCoord;
            #endif
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.normalWS = input.interp1.xyz;
            output.tangentWS = input.interp2.xyzw;
            output.texCoord0 = input.interp3.xyzw;
            output.texCoord1 = input.interp4.xyzw;
            output.texCoord2 = input.interp5.xyzw;
            output.texCoord3 = input.interp6.xyzw;
            output.color = input.interp7.xyzw;
            output.viewDirectionWS = input.interp8.xyz;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.interp9.xy;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.interp10.xy;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.interp11.xyz;
            #endif
            output.fogFactorAndVertexLight = input.interp12.xyzw;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = input.interp13.xyzw;
            #endif
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_TexelSize;
        float2 _OutlineOffset1;
        float2 _OutlineOffset2;
        float2 _OutlineOffset3;
        float _GradientScale;
        float4 _FaceColor;
        float4 _IsoPerimeter;
        float4 _OutlineColor1;
        float4 _OutlineColor2;
        float4 _OutlineColor3;
        float _OutlineMode;
        float3 _Softness;
        float4 _FaceTex_TexelSize;
        float2 _FaceUVSpeed;
        float4 _FaceTex_ST;
        float4 _OutlineTex_ST;
        float2 _OutlineUVSpeed;
        float4 _UnderlayColor;
        float2 _UnderlayOffset;
        float _UnderlayDilate;
        float _UnderlaySoftness;
        float _BevelType;
        float _BevelAmount;
        float _BevelOffset;
        float _BevelWidth;
        float _BevelRoundness;
        float _BevelClamp;
        float4 _SpecularColor;
        float _LightAngle;
        float _SpecularPower;
        float _Reflectivity;
        float _Diffuse;
        float _Ambient;
        float4 _OutlineTex_TexelSize;
        float _ScaleRatioA;
        float4 Emissive;
        float _smoothness;
        float _Metalic;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_FaceTex);
        SAMPLER(sampler_FaceTex);
        SAMPLER(SamplerState_Linear_Clamp);
        TEXTURE2D(_OutlineTex);
        SAMPLER(sampler_OutlineTex);
        
        // Graph Includes
        #include "Assets/TextMesh Pro/Shaders/SDFFunctions.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Divide_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float Smoothness;
            float Occlusion;
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _UV_533a59502f0abc8bb2c09828f32ebead_Out_0 = IN.uv0;
            UnityTexture2D _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_389f09eeac16ed8eb7a6151195af0507_Width_0 = _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0.texelSize.z;
            float _TexelSize_389f09eeac16ed8eb7a6151195af0507_Height_2 = _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0.texelSize.w;
            float _ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3;
            ScreenSpaceRatio_float((_UV_533a59502f0abc8bb2c09828f32ebead_Out_0.xy), _TexelSize_389f09eeac16ed8eb7a6151195af0507_Width_0, 0, _ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3);
            UnityTexture2D _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(IN.uv0.xy));
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_R_4 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.r;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_G_5 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.g;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_B_6 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.b;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_A_7 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.a;
            float4 _UV_b26868a97b712882abeca1b58698beb0_Out_0 = IN.uv0;
            float2 _Property_e4fdf31842293c8fb0a7eb85f924b8b8_Out_0 = _OutlineOffset1;
            float _Property_c650c0154e947e898564d7d1d007d48e_Out_0 = _GradientScale;
            UnityTexture2D _Property_6e377359c1349380b9482e9613fcec6b_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Width_0 = _Property_6e377359c1349380b9482e9613fcec6b_Out_0.texelSize.z;
            float _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Height_2 = _Property_6e377359c1349380b9482e9613fcec6b_Out_0.texelSize.w;
            float4 _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4;
            float3 _Combine_fd0cd2353f78958d948ad3086c76645e_RGB_5;
            float2 _Combine_fd0cd2353f78958d948ad3086c76645e_RG_6;
            Unity_Combine_float(_TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Width_0, _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Height_2, 0, 0, _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4, _Combine_fd0cd2353f78958d948ad3086c76645e_RGB_5, _Combine_fd0cd2353f78958d948ad3086c76645e_RG_6);
            float4 _Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2;
            Unity_Divide_float4((_Property_c650c0154e947e898564d7d1d007d48e_Out_0.xxxx), _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4, _Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2);
            float2 _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2;
            Unity_Multiply_float2_float2(_Property_e4fdf31842293c8fb0a7eb85f924b8b8_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2);
            float2 _Subtract_8ecda3d29d47068e8b76538959fab084_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2, _Subtract_8ecda3d29d47068e8b76538959fab084_Out_2);
            float4 _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_8ecda3d29d47068e8b76538959fab084_Out_2));
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_R_4 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.r;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_G_5 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.g;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_B_6 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.b;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_A_7 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.a;
            float2 _Property_5a3269796f550283a99abee895aeedd4_Out_0 = _OutlineOffset2;
            float2 _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2;
            Unity_Multiply_float2_float2((_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Property_5a3269796f550283a99abee895aeedd4_Out_0, _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2);
            float2 _Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2, _Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2);
            float4 _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2));
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_R_4 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.r;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_G_5 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.g;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_B_6 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.b;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_A_7 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.a;
            float2 _Property_20cbcbce438f6581847bd3da66673af8_Out_0 = _OutlineOffset3;
            float2 _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2;
            Unity_Multiply_float2_float2(_Property_20cbcbce438f6581847bd3da66673af8_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2);
            float2 _Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2, _Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2);
            float4 _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2));
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_R_4 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.r;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_G_5 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.g;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_B_6 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.b;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_A_7 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.a;
            float4 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4;
            float3 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGB_5;
            float2 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RG_6;
            Unity_Combine_float(_SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_A_7, _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_A_7, _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_A_7, _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_A_7, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGB_5, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RG_6);
            float _Property_bdf7e7b4670fdc86a20f193fb041efd3_Out_0 = _GradientScale;
            float4 _Property_f5d0374e48ad748a9e62658056ab81a3_Out_0 = _IsoPerimeter;
            float3 _Property_05d545af6f7ff08387320e1130c67389_Out_0 = _Softness;
            float _Property_ab4e94278c3a0b8eba9ceccb7822f574_Out_0 = _OutlineMode;
            float4 _ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6;
            ComputeSDF44_float(_ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4, _Property_bdf7e7b4670fdc86a20f193fb041efd3_Out_0, _Property_f5d0374e48ad748a9e62658056ab81a3_Out_0, (float4(_Property_05d545af6f7ff08387320e1130c67389_Out_0, 1.0)), _Property_ab4e94278c3a0b8eba9ceccb7822f574_Out_0, _ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6);
            float4 _Property_eaed1eca838e7183ae708f9502a50dba_Out_0 = _FaceColor;
            UnityTexture2D _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0 = UnityBuildTexture2DStructNoScale(_FaceTex);
            float4 _UV_90e11fefcc71b18b8d88b417548d224f_Out_0 = IN.uv0;
            float4 _Property_26b98e8b908b638d92c1cf36c28e2c9f_Out_0 = _FaceTex_ST;
            float2 _Property_ced4707a1339568eb002c789407f8a3b_Out_0 = _FaceUVSpeed;
            float2 _GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3;
            GenerateUV_float((_UV_90e11fefcc71b18b8d88b417548d224f_Out_0.xy), _Property_26b98e8b908b638d92c1cf36c28e2c9f_Out_0, _Property_ced4707a1339568eb002c789407f8a3b_Out_0, _GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3);
            float4 _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0 = SAMPLE_TEXTURE2D(_Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.tex, _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.samplerstate, _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.GetTransformedUV(_GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3));
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_R_4 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.r;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_G_5 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.g;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_B_6 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.b;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_A_7 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.a;
            float4 _Multiply_48133834a18f14828ac11b58bc205702_Out_2;
            Unity_Multiply_float4_float4(_Property_eaed1eca838e7183ae708f9502a50dba_Out_0, _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0, _Multiply_48133834a18f14828ac11b58bc205702_Out_2);
            float4 _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2;
            Unity_Multiply_float4_float4(IN.VertexColor, _Multiply_48133834a18f14828ac11b58bc205702_Out_2, _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2);
            float4 _Property_4637336160f04f8397ca8f3b6531e88e_Out_0 = _OutlineColor1;
            UnityTexture2D _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0 = UnityBuildTexture2DStructNoScale(_OutlineTex);
            float4 _UV_343788cf38e6598db275321a22b05984_Out_0 = IN.uv0;
            float4 _Property_f5b7ef749a5e5a80911752b8c751465e_Out_0 = _OutlineTex_ST;
            float2 _Property_11ca84fc4adc63868541229e4f404eae_Out_0 = _OutlineUVSpeed;
            float2 _GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3;
            GenerateUV_float((_UV_343788cf38e6598db275321a22b05984_Out_0.xy), _Property_f5b7ef749a5e5a80911752b8c751465e_Out_0, _Property_11ca84fc4adc63868541229e4f404eae_Out_0, _GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3);
            float4 _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0 = SAMPLE_TEXTURE2D(_Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.tex, _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.samplerstate, _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.GetTransformedUV(_GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3));
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_R_4 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.r;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_G_5 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.g;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_B_6 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.b;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_A_7 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.a;
            float4 _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2;
            Unity_Multiply_float4_float4(_Property_4637336160f04f8397ca8f3b6531e88e_Out_0, _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0, _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2);
            float4 _Property_6e8acbed3933c6819383cb89fe70a942_Out_0 = _OutlineColor2;
            float2 _Property_baa8c75173022e86bde4cc075585af43_Out_0 = _OutlineOffset3;
            float4 _Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5;
            Layer4_float(_ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6, _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2, _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2, _Property_6e8acbed3933c6819383cb89fe70a942_Out_0, (float4(_Property_baa8c75173022e86bde4cc075585af43_Out_0, 0.0, 1.0)), _Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5);
            UnityTexture2D _Property_5b0219fd9b23c785b6053d92dc125ed9_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            UnityTexture2D _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_abc928409a963289955625c60e3c3674_Width_0 = _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0.texelSize.z;
            float _TexelSize_abc928409a963289955625c60e3c3674_Height_2 = _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0.texelSize.w;
            float4 _UV_7afe81a63629c789aece6cf7ed368769_Out_0 = IN.uv0;
            float _IsFrontFace_33ef6b7d7f95818ba696de9dae14e559_Out_0 = max(0, IN.FaceSign.x);
            float3 _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5;
            GetSurfaceNormal_float(_Property_5b0219fd9b23c785b6053d92dc125ed9_Out_0.tex, _TexelSize_abc928409a963289955625c60e3c3674_Width_0, _TexelSize_abc928409a963289955625c60e3c3674_Height_2, (_UV_7afe81a63629c789aece6cf7ed368769_Out_0).x, _IsFrontFace_33ef6b7d7f95818ba696de9dae14e559_Out_0, _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5);
            float4 _EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2;
            EvaluateLight_float(_Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5, _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5, _EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2);
            UnityTexture2D _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _UV_87b50231796c43898a28be7b2f8daea3_Out_0 = IN.uv0;
            float2 _Property_c20a130451687b8c87b5a623d8ca6d73_Out_0 = _UnderlayOffset;
            float2 _Multiply_7d754a30a319c9839ca945e596548e63_Out_2;
            Unity_Multiply_float2_float2(_Property_c20a130451687b8c87b5a623d8ca6d73_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_7d754a30a319c9839ca945e596548e63_Out_2);
            float2 _Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2;
            Unity_Subtract_float2((_UV_87b50231796c43898a28be7b2f8daea3_Out_0.xy), _Multiply_7d754a30a319c9839ca945e596548e63_Out_2, _Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2);
            float4 _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0 = SAMPLE_TEXTURE2D(_Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.tex, _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.samplerstate, _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.GetTransformedUV(_Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2));
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_R_4 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.r;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_G_5 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.g;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_B_6 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.b;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_A_7 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.a;
            float _Property_cb906f6db0fb388a91590590b04091ab_Out_0 = _GradientScale;
            float _Property_22272afe5996178386aac669fbda451b_Out_0 = _UnderlayDilate;
            float _Property_1cd799c4097e1685b9dbf5dd80b6062b_Out_0 = _UnderlaySoftness;
            float _ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0;
            ComputeSDF_float(_ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3, (_SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0).x, _Property_cb906f6db0fb388a91590590b04091ab_Out_0, _Property_22272afe5996178386aac669fbda451b_Out_0, _Property_1cd799c4097e1685b9dbf5dd80b6062b_Out_0, _ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0);
            float4 _Property_4be3e1d3be22b78f9f4e23b78e8d6064_Out_0 = _UnderlayColor;
            float4 _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2;
            Layer1_float(_ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0, _Property_4be3e1d3be22b78f9f4e23b78e8d6064_Out_0, _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2);
            float4 _CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2;
            Composite_float(_EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2, _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2, _CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2);
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_R_1 = IN.VertexColor[0];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_G_2 = IN.VertexColor[1];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_B_3 = IN.VertexColor[2];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_A_4 = IN.VertexColor[3];
            float4 _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2;
            Unity_Multiply_float4_float4(_CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2, (_Split_79de04ac9d55bc8693a6a09c54eed8bb_A_4.xxxx), _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2);
            float4 _Property_c74b31cf958e978797249554cd8dd6be_Out_0 = Emissive;
            float4 _Multiply_e1a973f199b55389b5df6b0985db43a7_Out_2;
            Unity_Multiply_float4_float4(_Property_c74b31cf958e978797249554cd8dd6be_Out_0, _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2, _Multiply_e1a973f199b55389b5df6b0985db43a7_Out_2);
            float _Split_9b3e8141df129b848c4c1b364b613b86_R_1 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[0];
            float _Split_9b3e8141df129b848c4c1b364b613b86_G_2 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[1];
            float _Split_9b3e8141df129b848c4c1b364b613b86_B_3 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[2];
            float _Split_9b3e8141df129b848c4c1b364b613b86_A_4 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[3];
            float4 _Multiply_b32230c509acbc8f89fb8f34c19f9513_Out_2;
            Unity_Multiply_float4_float4(_Multiply_e1a973f199b55389b5df6b0985db43a7_Out_2, (_Split_9b3e8141df129b848c4c1b364b613b86_A_4.xxxx), _Multiply_b32230c509acbc8f89fb8f34c19f9513_Out_2);
            float _Property_c8a6020bb811b98d84a5e74d0bd3ee3b_Out_0 = _Metalic;
            float _Property_7f5c7787a25f4c86a3ab8cfae98caa25_Out_0 = _smoothness;
            surface.BaseColor = (_Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2.xyz);
            surface.NormalTS = _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5;
            surface.Emission = (_Multiply_b32230c509acbc8f89fb8f34c19f9513_Out_2.xyz);
            surface.Metallic = _Property_c8a6020bb811b98d84a5e74d0bd3ee3b_Out_0;
            surface.Smoothness = _Property_7f5c7787a25f4c86a3ab8cfae98caa25_Out_0;
            surface.Occlusion = 1;
            surface.Alpha = _Split_9b3e8141df129b848c4c1b364b613b86_A_4;
            surface.AlphaClipThreshold = 0.5;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
            output.uv0 = input.texCoord0;
            output.uv1 = input.texCoord1;
            output.uv2 = input.texCoord2;
            output.uv3 = input.texCoord3;
            output.VertexColor = input.color;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
            BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
        
        // Render State
        Cull Off
        ZTest LEqual
        ZWrite On
        ColorMask 0
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
       // #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag
        
        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>
        
        // Keywords
        #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define ATTRIBUTES_NEED_TEXCOORD3
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD1
        #define VARYINGS_NEED_TEXCOORD2
        #define VARYINGS_NEED_TEXCOORD3
        #define VARYINGS_NEED_COLOR
        #define VARYINGS_NEED_CULLFACE
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SHADOWCASTER
        #define _ALPHATEST_ON 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
             float4 uv3 : TEXCOORD3;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 normalWS;
             float4 texCoord0;
             float4 texCoord1;
             float4 texCoord2;
             float4 texCoord3;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
             float4 uv1;
             float4 uv2;
             float4 uv3;
             float4 VertexColor;
             float FaceSign;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 interp0 : INTERP0;
             float4 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
             float4 interp4 : INTERP4;
             float4 interp5 : INTERP5;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.normalWS;
            output.interp1.xyzw =  input.texCoord0;
            output.interp2.xyzw =  input.texCoord1;
            output.interp3.xyzw =  input.texCoord2;
            output.interp4.xyzw =  input.texCoord3;
            output.interp5.xyzw =  input.color;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.normalWS = input.interp0.xyz;
            output.texCoord0 = input.interp1.xyzw;
            output.texCoord1 = input.interp2.xyzw;
            output.texCoord2 = input.interp3.xyzw;
            output.texCoord3 = input.interp4.xyzw;
            output.color = input.interp5.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_TexelSize;
        float2 _OutlineOffset1;
        float2 _OutlineOffset2;
        float2 _OutlineOffset3;
        float _GradientScale;
        float4 _FaceColor;
        float4 _IsoPerimeter;
        float4 _OutlineColor1;
        float4 _OutlineColor2;
        float4 _OutlineColor3;
        float _OutlineMode;
        float3 _Softness;
        float4 _FaceTex_TexelSize;
        float2 _FaceUVSpeed;
        float4 _FaceTex_ST;
        float4 _OutlineTex_ST;
        float2 _OutlineUVSpeed;
        float4 _UnderlayColor;
        float2 _UnderlayOffset;
        float _UnderlayDilate;
        float _UnderlaySoftness;
        float _BevelType;
        float _BevelAmount;
        float _BevelOffset;
        float _BevelWidth;
        float _BevelRoundness;
        float _BevelClamp;
        float4 _SpecularColor;
        float _LightAngle;
        float _SpecularPower;
        float _Reflectivity;
        float _Diffuse;
        float _Ambient;
        float4 _OutlineTex_TexelSize;
        float _ScaleRatioA;
        float4 Emissive;
        float _smoothness;
        float _Metalic;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_FaceTex);
        SAMPLER(sampler_FaceTex);
        SAMPLER(SamplerState_Linear_Clamp);
        TEXTURE2D(_OutlineTex);
        SAMPLER(sampler_OutlineTex);
        
        // Graph Includes
        #include "Assets/TextMesh Pro/Shaders/SDFFunctions.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Divide_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _UV_533a59502f0abc8bb2c09828f32ebead_Out_0 = IN.uv0;
            UnityTexture2D _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_389f09eeac16ed8eb7a6151195af0507_Width_0 = _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0.texelSize.z;
            float _TexelSize_389f09eeac16ed8eb7a6151195af0507_Height_2 = _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0.texelSize.w;
            float _ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3;
            ScreenSpaceRatio_float((_UV_533a59502f0abc8bb2c09828f32ebead_Out_0.xy), _TexelSize_389f09eeac16ed8eb7a6151195af0507_Width_0, 0, _ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3);
            UnityTexture2D _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(IN.uv0.xy));
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_R_4 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.r;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_G_5 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.g;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_B_6 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.b;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_A_7 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.a;
            float4 _UV_b26868a97b712882abeca1b58698beb0_Out_0 = IN.uv0;
            float2 _Property_e4fdf31842293c8fb0a7eb85f924b8b8_Out_0 = _OutlineOffset1;
            float _Property_c650c0154e947e898564d7d1d007d48e_Out_0 = _GradientScale;
            UnityTexture2D _Property_6e377359c1349380b9482e9613fcec6b_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Width_0 = _Property_6e377359c1349380b9482e9613fcec6b_Out_0.texelSize.z;
            float _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Height_2 = _Property_6e377359c1349380b9482e9613fcec6b_Out_0.texelSize.w;
            float4 _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4;
            float3 _Combine_fd0cd2353f78958d948ad3086c76645e_RGB_5;
            float2 _Combine_fd0cd2353f78958d948ad3086c76645e_RG_6;
            Unity_Combine_float(_TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Width_0, _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Height_2, 0, 0, _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4, _Combine_fd0cd2353f78958d948ad3086c76645e_RGB_5, _Combine_fd0cd2353f78958d948ad3086c76645e_RG_6);
            float4 _Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2;
            Unity_Divide_float4((_Property_c650c0154e947e898564d7d1d007d48e_Out_0.xxxx), _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4, _Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2);
            float2 _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2;
            Unity_Multiply_float2_float2(_Property_e4fdf31842293c8fb0a7eb85f924b8b8_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2);
            float2 _Subtract_8ecda3d29d47068e8b76538959fab084_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2, _Subtract_8ecda3d29d47068e8b76538959fab084_Out_2);
            float4 _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_8ecda3d29d47068e8b76538959fab084_Out_2));
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_R_4 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.r;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_G_5 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.g;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_B_6 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.b;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_A_7 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.a;
            float2 _Property_5a3269796f550283a99abee895aeedd4_Out_0 = _OutlineOffset2;
            float2 _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2;
            Unity_Multiply_float2_float2((_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Property_5a3269796f550283a99abee895aeedd4_Out_0, _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2);
            float2 _Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2, _Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2);
            float4 _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2));
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_R_4 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.r;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_G_5 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.g;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_B_6 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.b;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_A_7 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.a;
            float2 _Property_20cbcbce438f6581847bd3da66673af8_Out_0 = _OutlineOffset3;
            float2 _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2;
            Unity_Multiply_float2_float2(_Property_20cbcbce438f6581847bd3da66673af8_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2);
            float2 _Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2, _Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2);
            float4 _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2));
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_R_4 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.r;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_G_5 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.g;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_B_6 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.b;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_A_7 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.a;
            float4 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4;
            float3 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGB_5;
            float2 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RG_6;
            Unity_Combine_float(_SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_A_7, _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_A_7, _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_A_7, _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_A_7, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGB_5, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RG_6);
            float _Property_bdf7e7b4670fdc86a20f193fb041efd3_Out_0 = _GradientScale;
            float4 _Property_f5d0374e48ad748a9e62658056ab81a3_Out_0 = _IsoPerimeter;
            float3 _Property_05d545af6f7ff08387320e1130c67389_Out_0 = _Softness;
            float _Property_ab4e94278c3a0b8eba9ceccb7822f574_Out_0 = _OutlineMode;
            float4 _ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6;
            ComputeSDF44_float(_ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4, _Property_bdf7e7b4670fdc86a20f193fb041efd3_Out_0, _Property_f5d0374e48ad748a9e62658056ab81a3_Out_0, (float4(_Property_05d545af6f7ff08387320e1130c67389_Out_0, 1.0)), _Property_ab4e94278c3a0b8eba9ceccb7822f574_Out_0, _ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6);
            float4 _Property_eaed1eca838e7183ae708f9502a50dba_Out_0 = _FaceColor;
            UnityTexture2D _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0 = UnityBuildTexture2DStructNoScale(_FaceTex);
            float4 _UV_90e11fefcc71b18b8d88b417548d224f_Out_0 = IN.uv0;
            float4 _Property_26b98e8b908b638d92c1cf36c28e2c9f_Out_0 = _FaceTex_ST;
            float2 _Property_ced4707a1339568eb002c789407f8a3b_Out_0 = _FaceUVSpeed;
            float2 _GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3;
            GenerateUV_float((_UV_90e11fefcc71b18b8d88b417548d224f_Out_0.xy), _Property_26b98e8b908b638d92c1cf36c28e2c9f_Out_0, _Property_ced4707a1339568eb002c789407f8a3b_Out_0, _GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3);
            float4 _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0 = SAMPLE_TEXTURE2D(_Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.tex, _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.samplerstate, _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.GetTransformedUV(_GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3));
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_R_4 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.r;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_G_5 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.g;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_B_6 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.b;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_A_7 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.a;
            float4 _Multiply_48133834a18f14828ac11b58bc205702_Out_2;
            Unity_Multiply_float4_float4(_Property_eaed1eca838e7183ae708f9502a50dba_Out_0, _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0, _Multiply_48133834a18f14828ac11b58bc205702_Out_2);
            float4 _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2;
            Unity_Multiply_float4_float4(IN.VertexColor, _Multiply_48133834a18f14828ac11b58bc205702_Out_2, _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2);
            float4 _Property_4637336160f04f8397ca8f3b6531e88e_Out_0 = _OutlineColor1;
            UnityTexture2D _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0 = UnityBuildTexture2DStructNoScale(_OutlineTex);
            float4 _UV_343788cf38e6598db275321a22b05984_Out_0 = IN.uv0;
            float4 _Property_f5b7ef749a5e5a80911752b8c751465e_Out_0 = _OutlineTex_ST;
            float2 _Property_11ca84fc4adc63868541229e4f404eae_Out_0 = _OutlineUVSpeed;
            float2 _GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3;
            GenerateUV_float((_UV_343788cf38e6598db275321a22b05984_Out_0.xy), _Property_f5b7ef749a5e5a80911752b8c751465e_Out_0, _Property_11ca84fc4adc63868541229e4f404eae_Out_0, _GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3);
            float4 _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0 = SAMPLE_TEXTURE2D(_Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.tex, _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.samplerstate, _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.GetTransformedUV(_GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3));
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_R_4 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.r;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_G_5 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.g;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_B_6 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.b;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_A_7 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.a;
            float4 _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2;
            Unity_Multiply_float4_float4(_Property_4637336160f04f8397ca8f3b6531e88e_Out_0, _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0, _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2);
            float4 _Property_6e8acbed3933c6819383cb89fe70a942_Out_0 = _OutlineColor2;
            float2 _Property_baa8c75173022e86bde4cc075585af43_Out_0 = _OutlineOffset3;
            float4 _Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5;
            Layer4_float(_ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6, _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2, _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2, _Property_6e8acbed3933c6819383cb89fe70a942_Out_0, (float4(_Property_baa8c75173022e86bde4cc075585af43_Out_0, 0.0, 1.0)), _Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5);
            UnityTexture2D _Property_5b0219fd9b23c785b6053d92dc125ed9_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            UnityTexture2D _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_abc928409a963289955625c60e3c3674_Width_0 = _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0.texelSize.z;
            float _TexelSize_abc928409a963289955625c60e3c3674_Height_2 = _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0.texelSize.w;
            float4 _UV_7afe81a63629c789aece6cf7ed368769_Out_0 = IN.uv0;
            float _IsFrontFace_33ef6b7d7f95818ba696de9dae14e559_Out_0 = max(0, IN.FaceSign.x);
            float3 _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5;
            GetSurfaceNormal_float(_Property_5b0219fd9b23c785b6053d92dc125ed9_Out_0.tex, _TexelSize_abc928409a963289955625c60e3c3674_Width_0, _TexelSize_abc928409a963289955625c60e3c3674_Height_2, (_UV_7afe81a63629c789aece6cf7ed368769_Out_0).x, _IsFrontFace_33ef6b7d7f95818ba696de9dae14e559_Out_0, _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5);
            float4 _EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2;
            EvaluateLight_float(_Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5, _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5, _EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2);
            UnityTexture2D _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _UV_87b50231796c43898a28be7b2f8daea3_Out_0 = IN.uv0;
            float2 _Property_c20a130451687b8c87b5a623d8ca6d73_Out_0 = _UnderlayOffset;
            float2 _Multiply_7d754a30a319c9839ca945e596548e63_Out_2;
            Unity_Multiply_float2_float2(_Property_c20a130451687b8c87b5a623d8ca6d73_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_7d754a30a319c9839ca945e596548e63_Out_2);
            float2 _Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2;
            Unity_Subtract_float2((_UV_87b50231796c43898a28be7b2f8daea3_Out_0.xy), _Multiply_7d754a30a319c9839ca945e596548e63_Out_2, _Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2);
            float4 _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0 = SAMPLE_TEXTURE2D(_Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.tex, _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.samplerstate, _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.GetTransformedUV(_Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2));
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_R_4 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.r;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_G_5 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.g;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_B_6 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.b;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_A_7 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.a;
            float _Property_cb906f6db0fb388a91590590b04091ab_Out_0 = _GradientScale;
            float _Property_22272afe5996178386aac669fbda451b_Out_0 = _UnderlayDilate;
            float _Property_1cd799c4097e1685b9dbf5dd80b6062b_Out_0 = _UnderlaySoftness;
            float _ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0;
            ComputeSDF_float(_ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3, (_SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0).x, _Property_cb906f6db0fb388a91590590b04091ab_Out_0, _Property_22272afe5996178386aac669fbda451b_Out_0, _Property_1cd799c4097e1685b9dbf5dd80b6062b_Out_0, _ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0);
            float4 _Property_4be3e1d3be22b78f9f4e23b78e8d6064_Out_0 = _UnderlayColor;
            float4 _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2;
            Layer1_float(_ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0, _Property_4be3e1d3be22b78f9f4e23b78e8d6064_Out_0, _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2);
            float4 _CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2;
            Composite_float(_EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2, _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2, _CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2);
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_R_1 = IN.VertexColor[0];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_G_2 = IN.VertexColor[1];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_B_3 = IN.VertexColor[2];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_A_4 = IN.VertexColor[3];
            float4 _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2;
            Unity_Multiply_float4_float4(_CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2, (_Split_79de04ac9d55bc8693a6a09c54eed8bb_A_4.xxxx), _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2);
            float _Split_9b3e8141df129b848c4c1b364b613b86_R_1 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[0];
            float _Split_9b3e8141df129b848c4c1b364b613b86_G_2 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[1];
            float _Split_9b3e8141df129b848c4c1b364b613b86_B_3 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[2];
            float _Split_9b3e8141df129b848c4c1b364b613b86_A_4 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[3];
            surface.Alpha = _Split_9b3e8141df129b848c4c1b364b613b86_A_4;
            surface.AlphaClipThreshold = 0.5;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
            output.uv0 = input.texCoord0;
            output.uv1 = input.texCoord1;
            output.uv2 = input.texCoord2;
            output.uv3 = input.texCoord3;
            output.VertexColor = input.color;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
            BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }
        
        // Render State
        Cull Off
        ZTest LEqual
        ZWrite On
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
        //#pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag
        
        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define ATTRIBUTES_NEED_TEXCOORD3
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD1
        #define VARYINGS_NEED_TEXCOORD2
        #define VARYINGS_NEED_TEXCOORD3
        #define VARYINGS_NEED_COLOR
        #define VARYINGS_NEED_CULLFACE
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHNORMALS
        #define _ALPHATEST_ON 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
             float4 uv3 : TEXCOORD3;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
             float4 texCoord1;
             float4 texCoord2;
             float4 texCoord3;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float4 uv0;
             float4 uv1;
             float4 uv2;
             float4 uv3;
             float4 VertexColor;
             float FaceSign;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 interp0 : INTERP0;
             float4 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
             float4 interp4 : INTERP4;
             float4 interp5 : INTERP5;
             float4 interp6 : INTERP6;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.normalWS;
            output.interp1.xyzw =  input.tangentWS;
            output.interp2.xyzw =  input.texCoord0;
            output.interp3.xyzw =  input.texCoord1;
            output.interp4.xyzw =  input.texCoord2;
            output.interp5.xyzw =  input.texCoord3;
            output.interp6.xyzw =  input.color;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.normalWS = input.interp0.xyz;
            output.tangentWS = input.interp1.xyzw;
            output.texCoord0 = input.interp2.xyzw;
            output.texCoord1 = input.interp3.xyzw;
            output.texCoord2 = input.interp4.xyzw;
            output.texCoord3 = input.interp5.xyzw;
            output.color = input.interp6.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_TexelSize;
        float2 _OutlineOffset1;
        float2 _OutlineOffset2;
        float2 _OutlineOffset3;
        float _GradientScale;
        float4 _FaceColor;
        float4 _IsoPerimeter;
        float4 _OutlineColor1;
        float4 _OutlineColor2;
        float4 _OutlineColor3;
        float _OutlineMode;
        float3 _Softness;
        float4 _FaceTex_TexelSize;
        float2 _FaceUVSpeed;
        float4 _FaceTex_ST;
        float4 _OutlineTex_ST;
        float2 _OutlineUVSpeed;
        float4 _UnderlayColor;
        float2 _UnderlayOffset;
        float _UnderlayDilate;
        float _UnderlaySoftness;
        float _BevelType;
        float _BevelAmount;
        float _BevelOffset;
        float _BevelWidth;
        float _BevelRoundness;
        float _BevelClamp;
        float4 _SpecularColor;
        float _LightAngle;
        float _SpecularPower;
        float _Reflectivity;
        float _Diffuse;
        float _Ambient;
        float4 _OutlineTex_TexelSize;
        float _ScaleRatioA;
        float4 Emissive;
        float _smoothness;
        float _Metalic;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_FaceTex);
        SAMPLER(sampler_FaceTex);
        SAMPLER(SamplerState_Linear_Clamp);
        TEXTURE2D(_OutlineTex);
        SAMPLER(sampler_OutlineTex);
        
        // Graph Includes
        #include "Assets/TextMesh Pro/Shaders/SDFFunctions.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Divide_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 NormalTS;
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_5b0219fd9b23c785b6053d92dc125ed9_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            UnityTexture2D _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_abc928409a963289955625c60e3c3674_Width_0 = _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0.texelSize.z;
            float _TexelSize_abc928409a963289955625c60e3c3674_Height_2 = _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0.texelSize.w;
            float4 _UV_7afe81a63629c789aece6cf7ed368769_Out_0 = IN.uv0;
            float _IsFrontFace_33ef6b7d7f95818ba696de9dae14e559_Out_0 = max(0, IN.FaceSign.x);
            float3 _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5;
            GetSurfaceNormal_float(_Property_5b0219fd9b23c785b6053d92dc125ed9_Out_0.tex, _TexelSize_abc928409a963289955625c60e3c3674_Width_0, _TexelSize_abc928409a963289955625c60e3c3674_Height_2, (_UV_7afe81a63629c789aece6cf7ed368769_Out_0).x, _IsFrontFace_33ef6b7d7f95818ba696de9dae14e559_Out_0, _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5);
            float4 _UV_533a59502f0abc8bb2c09828f32ebead_Out_0 = IN.uv0;
            UnityTexture2D _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_389f09eeac16ed8eb7a6151195af0507_Width_0 = _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0.texelSize.z;
            float _TexelSize_389f09eeac16ed8eb7a6151195af0507_Height_2 = _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0.texelSize.w;
            float _ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3;
            ScreenSpaceRatio_float((_UV_533a59502f0abc8bb2c09828f32ebead_Out_0.xy), _TexelSize_389f09eeac16ed8eb7a6151195af0507_Width_0, 0, _ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3);
            UnityTexture2D _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(IN.uv0.xy));
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_R_4 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.r;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_G_5 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.g;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_B_6 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.b;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_A_7 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.a;
            float4 _UV_b26868a97b712882abeca1b58698beb0_Out_0 = IN.uv0;
            float2 _Property_e4fdf31842293c8fb0a7eb85f924b8b8_Out_0 = _OutlineOffset1;
            float _Property_c650c0154e947e898564d7d1d007d48e_Out_0 = _GradientScale;
            UnityTexture2D _Property_6e377359c1349380b9482e9613fcec6b_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Width_0 = _Property_6e377359c1349380b9482e9613fcec6b_Out_0.texelSize.z;
            float _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Height_2 = _Property_6e377359c1349380b9482e9613fcec6b_Out_0.texelSize.w;
            float4 _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4;
            float3 _Combine_fd0cd2353f78958d948ad3086c76645e_RGB_5;
            float2 _Combine_fd0cd2353f78958d948ad3086c76645e_RG_6;
            Unity_Combine_float(_TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Width_0, _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Height_2, 0, 0, _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4, _Combine_fd0cd2353f78958d948ad3086c76645e_RGB_5, _Combine_fd0cd2353f78958d948ad3086c76645e_RG_6);
            float4 _Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2;
            Unity_Divide_float4((_Property_c650c0154e947e898564d7d1d007d48e_Out_0.xxxx), _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4, _Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2);
            float2 _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2;
            Unity_Multiply_float2_float2(_Property_e4fdf31842293c8fb0a7eb85f924b8b8_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2);
            float2 _Subtract_8ecda3d29d47068e8b76538959fab084_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2, _Subtract_8ecda3d29d47068e8b76538959fab084_Out_2);
            float4 _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_8ecda3d29d47068e8b76538959fab084_Out_2));
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_R_4 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.r;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_G_5 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.g;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_B_6 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.b;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_A_7 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.a;
            float2 _Property_5a3269796f550283a99abee895aeedd4_Out_0 = _OutlineOffset2;
            float2 _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2;
            Unity_Multiply_float2_float2((_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Property_5a3269796f550283a99abee895aeedd4_Out_0, _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2);
            float2 _Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2, _Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2);
            float4 _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2));
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_R_4 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.r;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_G_5 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.g;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_B_6 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.b;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_A_7 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.a;
            float2 _Property_20cbcbce438f6581847bd3da66673af8_Out_0 = _OutlineOffset3;
            float2 _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2;
            Unity_Multiply_float2_float2(_Property_20cbcbce438f6581847bd3da66673af8_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2);
            float2 _Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2, _Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2);
            float4 _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2));
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_R_4 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.r;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_G_5 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.g;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_B_6 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.b;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_A_7 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.a;
            float4 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4;
            float3 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGB_5;
            float2 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RG_6;
            Unity_Combine_float(_SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_A_7, _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_A_7, _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_A_7, _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_A_7, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGB_5, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RG_6);
            float _Property_bdf7e7b4670fdc86a20f193fb041efd3_Out_0 = _GradientScale;
            float4 _Property_f5d0374e48ad748a9e62658056ab81a3_Out_0 = _IsoPerimeter;
            float3 _Property_05d545af6f7ff08387320e1130c67389_Out_0 = _Softness;
            float _Property_ab4e94278c3a0b8eba9ceccb7822f574_Out_0 = _OutlineMode;
            float4 _ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6;
            ComputeSDF44_float(_ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4, _Property_bdf7e7b4670fdc86a20f193fb041efd3_Out_0, _Property_f5d0374e48ad748a9e62658056ab81a3_Out_0, (float4(_Property_05d545af6f7ff08387320e1130c67389_Out_0, 1.0)), _Property_ab4e94278c3a0b8eba9ceccb7822f574_Out_0, _ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6);
            float4 _Property_eaed1eca838e7183ae708f9502a50dba_Out_0 = _FaceColor;
            UnityTexture2D _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0 = UnityBuildTexture2DStructNoScale(_FaceTex);
            float4 _UV_90e11fefcc71b18b8d88b417548d224f_Out_0 = IN.uv0;
            float4 _Property_26b98e8b908b638d92c1cf36c28e2c9f_Out_0 = _FaceTex_ST;
            float2 _Property_ced4707a1339568eb002c789407f8a3b_Out_0 = _FaceUVSpeed;
            float2 _GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3;
            GenerateUV_float((_UV_90e11fefcc71b18b8d88b417548d224f_Out_0.xy), _Property_26b98e8b908b638d92c1cf36c28e2c9f_Out_0, _Property_ced4707a1339568eb002c789407f8a3b_Out_0, _GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3);
            float4 _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0 = SAMPLE_TEXTURE2D(_Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.tex, _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.samplerstate, _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.GetTransformedUV(_GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3));
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_R_4 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.r;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_G_5 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.g;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_B_6 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.b;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_A_7 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.a;
            float4 _Multiply_48133834a18f14828ac11b58bc205702_Out_2;
            Unity_Multiply_float4_float4(_Property_eaed1eca838e7183ae708f9502a50dba_Out_0, _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0, _Multiply_48133834a18f14828ac11b58bc205702_Out_2);
            float4 _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2;
            Unity_Multiply_float4_float4(IN.VertexColor, _Multiply_48133834a18f14828ac11b58bc205702_Out_2, _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2);
            float4 _Property_4637336160f04f8397ca8f3b6531e88e_Out_0 = _OutlineColor1;
            UnityTexture2D _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0 = UnityBuildTexture2DStructNoScale(_OutlineTex);
            float4 _UV_343788cf38e6598db275321a22b05984_Out_0 = IN.uv0;
            float4 _Property_f5b7ef749a5e5a80911752b8c751465e_Out_0 = _OutlineTex_ST;
            float2 _Property_11ca84fc4adc63868541229e4f404eae_Out_0 = _OutlineUVSpeed;
            float2 _GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3;
            GenerateUV_float((_UV_343788cf38e6598db275321a22b05984_Out_0.xy), _Property_f5b7ef749a5e5a80911752b8c751465e_Out_0, _Property_11ca84fc4adc63868541229e4f404eae_Out_0, _GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3);
            float4 _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0 = SAMPLE_TEXTURE2D(_Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.tex, _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.samplerstate, _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.GetTransformedUV(_GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3));
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_R_4 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.r;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_G_5 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.g;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_B_6 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.b;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_A_7 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.a;
            float4 _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2;
            Unity_Multiply_float4_float4(_Property_4637336160f04f8397ca8f3b6531e88e_Out_0, _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0, _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2);
            float4 _Property_6e8acbed3933c6819383cb89fe70a942_Out_0 = _OutlineColor2;
            float2 _Property_baa8c75173022e86bde4cc075585af43_Out_0 = _OutlineOffset3;
            float4 _Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5;
            Layer4_float(_ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6, _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2, _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2, _Property_6e8acbed3933c6819383cb89fe70a942_Out_0, (float4(_Property_baa8c75173022e86bde4cc075585af43_Out_0, 0.0, 1.0)), _Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5);
            float4 _EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2;
            EvaluateLight_float(_Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5, _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5, _EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2);
            UnityTexture2D _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _UV_87b50231796c43898a28be7b2f8daea3_Out_0 = IN.uv0;
            float2 _Property_c20a130451687b8c87b5a623d8ca6d73_Out_0 = _UnderlayOffset;
            float2 _Multiply_7d754a30a319c9839ca945e596548e63_Out_2;
            Unity_Multiply_float2_float2(_Property_c20a130451687b8c87b5a623d8ca6d73_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_7d754a30a319c9839ca945e596548e63_Out_2);
            float2 _Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2;
            Unity_Subtract_float2((_UV_87b50231796c43898a28be7b2f8daea3_Out_0.xy), _Multiply_7d754a30a319c9839ca945e596548e63_Out_2, _Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2);
            float4 _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0 = SAMPLE_TEXTURE2D(_Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.tex, _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.samplerstate, _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.GetTransformedUV(_Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2));
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_R_4 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.r;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_G_5 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.g;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_B_6 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.b;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_A_7 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.a;
            float _Property_cb906f6db0fb388a91590590b04091ab_Out_0 = _GradientScale;
            float _Property_22272afe5996178386aac669fbda451b_Out_0 = _UnderlayDilate;
            float _Property_1cd799c4097e1685b9dbf5dd80b6062b_Out_0 = _UnderlaySoftness;
            float _ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0;
            ComputeSDF_float(_ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3, (_SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0).x, _Property_cb906f6db0fb388a91590590b04091ab_Out_0, _Property_22272afe5996178386aac669fbda451b_Out_0, _Property_1cd799c4097e1685b9dbf5dd80b6062b_Out_0, _ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0);
            float4 _Property_4be3e1d3be22b78f9f4e23b78e8d6064_Out_0 = _UnderlayColor;
            float4 _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2;
            Layer1_float(_ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0, _Property_4be3e1d3be22b78f9f4e23b78e8d6064_Out_0, _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2);
            float4 _CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2;
            Composite_float(_EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2, _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2, _CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2);
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_R_1 = IN.VertexColor[0];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_G_2 = IN.VertexColor[1];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_B_3 = IN.VertexColor[2];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_A_4 = IN.VertexColor[3];
            float4 _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2;
            Unity_Multiply_float4_float4(_CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2, (_Split_79de04ac9d55bc8693a6a09c54eed8bb_A_4.xxxx), _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2);
            float _Split_9b3e8141df129b848c4c1b364b613b86_R_1 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[0];
            float _Split_9b3e8141df129b848c4c1b364b613b86_G_2 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[1];
            float _Split_9b3e8141df129b848c4c1b364b613b86_B_3 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[2];
            float _Split_9b3e8141df129b848c4c1b364b613b86_A_4 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[3];
            surface.NormalTS = _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5;
            surface.Alpha = _Split_9b3e8141df129b848c4c1b364b613b86_A_4;
            surface.AlphaClipThreshold = 0.5;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
            output.uv0 = input.texCoord0;
            output.uv1 = input.texCoord1;
            output.uv2 = input.texCoord2;
            output.uv3 = input.texCoord3;
            output.VertexColor = input.color;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
            BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma vertex vert
        #pragma fragment frag
        
        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>
        
        // Keywords
        #pragma shader_feature _ EDITOR_VISUALIZATION
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define ATTRIBUTES_NEED_TEXCOORD3
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD1
        #define VARYINGS_NEED_TEXCOORD2
        #define VARYINGS_NEED_TEXCOORD3
        #define VARYINGS_NEED_COLOR
        #define VARYINGS_NEED_CULLFACE
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_META
        #define _FOG_FRAGMENT 1
        #define _ALPHATEST_ON 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
             float4 uv3 : TEXCOORD3;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
             float4 texCoord1;
             float4 texCoord2;
             float4 texCoord3;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
             float4 uv1;
             float4 uv2;
             float4 uv3;
             float4 VertexColor;
             float FaceSign;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 interp0 : INTERP0;
             float4 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
             float4 interp4 : INTERP4;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyzw =  input.texCoord0;
            output.interp1.xyzw =  input.texCoord1;
            output.interp2.xyzw =  input.texCoord2;
            output.interp3.xyzw =  input.texCoord3;
            output.interp4.xyzw =  input.color;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp0.xyzw;
            output.texCoord1 = input.interp1.xyzw;
            output.texCoord2 = input.interp2.xyzw;
            output.texCoord3 = input.interp3.xyzw;
            output.color = input.interp4.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_TexelSize;
        float2 _OutlineOffset1;
        float2 _OutlineOffset2;
        float2 _OutlineOffset3;
        float _GradientScale;
        float4 _FaceColor;
        float4 _IsoPerimeter;
        float4 _OutlineColor1;
        float4 _OutlineColor2;
        float4 _OutlineColor3;
        float _OutlineMode;
        float3 _Softness;
        float4 _FaceTex_TexelSize;
        float2 _FaceUVSpeed;
        float4 _FaceTex_ST;
        float4 _OutlineTex_ST;
        float2 _OutlineUVSpeed;
        float4 _UnderlayColor;
        float2 _UnderlayOffset;
        float _UnderlayDilate;
        float _UnderlaySoftness;
        float _BevelType;
        float _BevelAmount;
        float _BevelOffset;
        float _BevelWidth;
        float _BevelRoundness;
        float _BevelClamp;
        float4 _SpecularColor;
        float _LightAngle;
        float _SpecularPower;
        float _Reflectivity;
        float _Diffuse;
        float _Ambient;
        float4 _OutlineTex_TexelSize;
        float _ScaleRatioA;
        float4 Emissive;
        float _smoothness;
        float _Metalic;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_FaceTex);
        SAMPLER(sampler_FaceTex);
        SAMPLER(SamplerState_Linear_Clamp);
        TEXTURE2D(_OutlineTex);
        SAMPLER(sampler_OutlineTex);
        
        // Graph Includes
        #include "Assets/TextMesh Pro/Shaders/SDFFunctions.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Divide_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 Emission;
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _UV_533a59502f0abc8bb2c09828f32ebead_Out_0 = IN.uv0;
            UnityTexture2D _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_389f09eeac16ed8eb7a6151195af0507_Width_0 = _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0.texelSize.z;
            float _TexelSize_389f09eeac16ed8eb7a6151195af0507_Height_2 = _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0.texelSize.w;
            float _ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3;
            ScreenSpaceRatio_float((_UV_533a59502f0abc8bb2c09828f32ebead_Out_0.xy), _TexelSize_389f09eeac16ed8eb7a6151195af0507_Width_0, 0, _ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3);
            UnityTexture2D _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(IN.uv0.xy));
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_R_4 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.r;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_G_5 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.g;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_B_6 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.b;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_A_7 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.a;
            float4 _UV_b26868a97b712882abeca1b58698beb0_Out_0 = IN.uv0;
            float2 _Property_e4fdf31842293c8fb0a7eb85f924b8b8_Out_0 = _OutlineOffset1;
            float _Property_c650c0154e947e898564d7d1d007d48e_Out_0 = _GradientScale;
            UnityTexture2D _Property_6e377359c1349380b9482e9613fcec6b_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Width_0 = _Property_6e377359c1349380b9482e9613fcec6b_Out_0.texelSize.z;
            float _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Height_2 = _Property_6e377359c1349380b9482e9613fcec6b_Out_0.texelSize.w;
            float4 _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4;
            float3 _Combine_fd0cd2353f78958d948ad3086c76645e_RGB_5;
            float2 _Combine_fd0cd2353f78958d948ad3086c76645e_RG_6;
            Unity_Combine_float(_TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Width_0, _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Height_2, 0, 0, _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4, _Combine_fd0cd2353f78958d948ad3086c76645e_RGB_5, _Combine_fd0cd2353f78958d948ad3086c76645e_RG_6);
            float4 _Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2;
            Unity_Divide_float4((_Property_c650c0154e947e898564d7d1d007d48e_Out_0.xxxx), _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4, _Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2);
            float2 _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2;
            Unity_Multiply_float2_float2(_Property_e4fdf31842293c8fb0a7eb85f924b8b8_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2);
            float2 _Subtract_8ecda3d29d47068e8b76538959fab084_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2, _Subtract_8ecda3d29d47068e8b76538959fab084_Out_2);
            float4 _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_8ecda3d29d47068e8b76538959fab084_Out_2));
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_R_4 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.r;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_G_5 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.g;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_B_6 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.b;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_A_7 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.a;
            float2 _Property_5a3269796f550283a99abee895aeedd4_Out_0 = _OutlineOffset2;
            float2 _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2;
            Unity_Multiply_float2_float2((_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Property_5a3269796f550283a99abee895aeedd4_Out_0, _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2);
            float2 _Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2, _Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2);
            float4 _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2));
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_R_4 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.r;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_G_5 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.g;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_B_6 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.b;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_A_7 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.a;
            float2 _Property_20cbcbce438f6581847bd3da66673af8_Out_0 = _OutlineOffset3;
            float2 _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2;
            Unity_Multiply_float2_float2(_Property_20cbcbce438f6581847bd3da66673af8_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2);
            float2 _Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2, _Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2);
            float4 _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2));
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_R_4 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.r;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_G_5 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.g;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_B_6 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.b;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_A_7 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.a;
            float4 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4;
            float3 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGB_5;
            float2 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RG_6;
            Unity_Combine_float(_SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_A_7, _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_A_7, _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_A_7, _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_A_7, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGB_5, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RG_6);
            float _Property_bdf7e7b4670fdc86a20f193fb041efd3_Out_0 = _GradientScale;
            float4 _Property_f5d0374e48ad748a9e62658056ab81a3_Out_0 = _IsoPerimeter;
            float3 _Property_05d545af6f7ff08387320e1130c67389_Out_0 = _Softness;
            float _Property_ab4e94278c3a0b8eba9ceccb7822f574_Out_0 = _OutlineMode;
            float4 _ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6;
            ComputeSDF44_float(_ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4, _Property_bdf7e7b4670fdc86a20f193fb041efd3_Out_0, _Property_f5d0374e48ad748a9e62658056ab81a3_Out_0, (float4(_Property_05d545af6f7ff08387320e1130c67389_Out_0, 1.0)), _Property_ab4e94278c3a0b8eba9ceccb7822f574_Out_0, _ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6);
            float4 _Property_eaed1eca838e7183ae708f9502a50dba_Out_0 = _FaceColor;
            UnityTexture2D _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0 = UnityBuildTexture2DStructNoScale(_FaceTex);
            float4 _UV_90e11fefcc71b18b8d88b417548d224f_Out_0 = IN.uv0;
            float4 _Property_26b98e8b908b638d92c1cf36c28e2c9f_Out_0 = _FaceTex_ST;
            float2 _Property_ced4707a1339568eb002c789407f8a3b_Out_0 = _FaceUVSpeed;
            float2 _GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3;
            GenerateUV_float((_UV_90e11fefcc71b18b8d88b417548d224f_Out_0.xy), _Property_26b98e8b908b638d92c1cf36c28e2c9f_Out_0, _Property_ced4707a1339568eb002c789407f8a3b_Out_0, _GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3);
            float4 _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0 = SAMPLE_TEXTURE2D(_Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.tex, _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.samplerstate, _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.GetTransformedUV(_GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3));
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_R_4 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.r;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_G_5 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.g;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_B_6 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.b;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_A_7 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.a;
            float4 _Multiply_48133834a18f14828ac11b58bc205702_Out_2;
            Unity_Multiply_float4_float4(_Property_eaed1eca838e7183ae708f9502a50dba_Out_0, _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0, _Multiply_48133834a18f14828ac11b58bc205702_Out_2);
            float4 _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2;
            Unity_Multiply_float4_float4(IN.VertexColor, _Multiply_48133834a18f14828ac11b58bc205702_Out_2, _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2);
            float4 _Property_4637336160f04f8397ca8f3b6531e88e_Out_0 = _OutlineColor1;
            UnityTexture2D _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0 = UnityBuildTexture2DStructNoScale(_OutlineTex);
            float4 _UV_343788cf38e6598db275321a22b05984_Out_0 = IN.uv0;
            float4 _Property_f5b7ef749a5e5a80911752b8c751465e_Out_0 = _OutlineTex_ST;
            float2 _Property_11ca84fc4adc63868541229e4f404eae_Out_0 = _OutlineUVSpeed;
            float2 _GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3;
            GenerateUV_float((_UV_343788cf38e6598db275321a22b05984_Out_0.xy), _Property_f5b7ef749a5e5a80911752b8c751465e_Out_0, _Property_11ca84fc4adc63868541229e4f404eae_Out_0, _GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3);
            float4 _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0 = SAMPLE_TEXTURE2D(_Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.tex, _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.samplerstate, _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.GetTransformedUV(_GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3));
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_R_4 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.r;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_G_5 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.g;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_B_6 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.b;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_A_7 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.a;
            float4 _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2;
            Unity_Multiply_float4_float4(_Property_4637336160f04f8397ca8f3b6531e88e_Out_0, _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0, _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2);
            float4 _Property_6e8acbed3933c6819383cb89fe70a942_Out_0 = _OutlineColor2;
            float2 _Property_baa8c75173022e86bde4cc075585af43_Out_0 = _OutlineOffset3;
            float4 _Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5;
            Layer4_float(_ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6, _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2, _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2, _Property_6e8acbed3933c6819383cb89fe70a942_Out_0, (float4(_Property_baa8c75173022e86bde4cc075585af43_Out_0, 0.0, 1.0)), _Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5);
            UnityTexture2D _Property_5b0219fd9b23c785b6053d92dc125ed9_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            UnityTexture2D _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_abc928409a963289955625c60e3c3674_Width_0 = _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0.texelSize.z;
            float _TexelSize_abc928409a963289955625c60e3c3674_Height_2 = _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0.texelSize.w;
            float4 _UV_7afe81a63629c789aece6cf7ed368769_Out_0 = IN.uv0;
            float _IsFrontFace_33ef6b7d7f95818ba696de9dae14e559_Out_0 = max(0, IN.FaceSign.x);
            float3 _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5;
            GetSurfaceNormal_float(_Property_5b0219fd9b23c785b6053d92dc125ed9_Out_0.tex, _TexelSize_abc928409a963289955625c60e3c3674_Width_0, _TexelSize_abc928409a963289955625c60e3c3674_Height_2, (_UV_7afe81a63629c789aece6cf7ed368769_Out_0).x, _IsFrontFace_33ef6b7d7f95818ba696de9dae14e559_Out_0, _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5);
            float4 _EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2;
            EvaluateLight_float(_Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5, _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5, _EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2);
            UnityTexture2D _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _UV_87b50231796c43898a28be7b2f8daea3_Out_0 = IN.uv0;
            float2 _Property_c20a130451687b8c87b5a623d8ca6d73_Out_0 = _UnderlayOffset;
            float2 _Multiply_7d754a30a319c9839ca945e596548e63_Out_2;
            Unity_Multiply_float2_float2(_Property_c20a130451687b8c87b5a623d8ca6d73_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_7d754a30a319c9839ca945e596548e63_Out_2);
            float2 _Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2;
            Unity_Subtract_float2((_UV_87b50231796c43898a28be7b2f8daea3_Out_0.xy), _Multiply_7d754a30a319c9839ca945e596548e63_Out_2, _Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2);
            float4 _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0 = SAMPLE_TEXTURE2D(_Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.tex, _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.samplerstate, _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.GetTransformedUV(_Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2));
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_R_4 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.r;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_G_5 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.g;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_B_6 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.b;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_A_7 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.a;
            float _Property_cb906f6db0fb388a91590590b04091ab_Out_0 = _GradientScale;
            float _Property_22272afe5996178386aac669fbda451b_Out_0 = _UnderlayDilate;
            float _Property_1cd799c4097e1685b9dbf5dd80b6062b_Out_0 = _UnderlaySoftness;
            float _ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0;
            ComputeSDF_float(_ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3, (_SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0).x, _Property_cb906f6db0fb388a91590590b04091ab_Out_0, _Property_22272afe5996178386aac669fbda451b_Out_0, _Property_1cd799c4097e1685b9dbf5dd80b6062b_Out_0, _ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0);
            float4 _Property_4be3e1d3be22b78f9f4e23b78e8d6064_Out_0 = _UnderlayColor;
            float4 _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2;
            Layer1_float(_ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0, _Property_4be3e1d3be22b78f9f4e23b78e8d6064_Out_0, _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2);
            float4 _CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2;
            Composite_float(_EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2, _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2, _CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2);
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_R_1 = IN.VertexColor[0];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_G_2 = IN.VertexColor[1];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_B_3 = IN.VertexColor[2];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_A_4 = IN.VertexColor[3];
            float4 _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2;
            Unity_Multiply_float4_float4(_CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2, (_Split_79de04ac9d55bc8693a6a09c54eed8bb_A_4.xxxx), _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2);
            float4 _Property_c74b31cf958e978797249554cd8dd6be_Out_0 = Emissive;
            float4 _Multiply_e1a973f199b55389b5df6b0985db43a7_Out_2;
            Unity_Multiply_float4_float4(_Property_c74b31cf958e978797249554cd8dd6be_Out_0, _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2, _Multiply_e1a973f199b55389b5df6b0985db43a7_Out_2);
            float _Split_9b3e8141df129b848c4c1b364b613b86_R_1 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[0];
            float _Split_9b3e8141df129b848c4c1b364b613b86_G_2 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[1];
            float _Split_9b3e8141df129b848c4c1b364b613b86_B_3 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[2];
            float _Split_9b3e8141df129b848c4c1b364b613b86_A_4 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[3];
            float4 _Multiply_b32230c509acbc8f89fb8f34c19f9513_Out_2;
            Unity_Multiply_float4_float4(_Multiply_e1a973f199b55389b5df6b0985db43a7_Out_2, (_Split_9b3e8141df129b848c4c1b364b613b86_A_4.xxxx), _Multiply_b32230c509acbc8f89fb8f34c19f9513_Out_2);
            surface.BaseColor = (_Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2.xyz);
            surface.Emission = (_Multiply_b32230c509acbc8f89fb8f34c19f9513_Out_2.xyz);
            surface.Alpha = _Split_9b3e8141df129b848c4c1b364b613b86_A_4;
            surface.AlphaClipThreshold = 0.5;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
            output.uv0 = input.texCoord0;
            output.uv1 = input.texCoord1;
            output.uv2 = input.texCoord2;
            output.uv3 = input.texCoord3;
            output.VertexColor = input.color;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
            BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "SceneSelectionPass"
            Tags
            {
                "LightMode" = "SceneSelectionPass"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma vertex vert
        #pragma fragment frag
        
        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define ATTRIBUTES_NEED_TEXCOORD3
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD1
        #define VARYINGS_NEED_TEXCOORD2
        #define VARYINGS_NEED_TEXCOORD3
        #define VARYINGS_NEED_COLOR
        #define VARYINGS_NEED_CULLFACE
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENESELECTIONPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        #define _ALPHATEST_ON 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
             float4 uv3 : TEXCOORD3;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
             float4 texCoord1;
             float4 texCoord2;
             float4 texCoord3;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
             float4 uv1;
             float4 uv2;
             float4 uv3;
             float4 VertexColor;
             float FaceSign;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 interp0 : INTERP0;
             float4 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
             float4 interp4 : INTERP4;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyzw =  input.texCoord0;
            output.interp1.xyzw =  input.texCoord1;
            output.interp2.xyzw =  input.texCoord2;
            output.interp3.xyzw =  input.texCoord3;
            output.interp4.xyzw =  input.color;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp0.xyzw;
            output.texCoord1 = input.interp1.xyzw;
            output.texCoord2 = input.interp2.xyzw;
            output.texCoord3 = input.interp3.xyzw;
            output.color = input.interp4.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_TexelSize;
        float2 _OutlineOffset1;
        float2 _OutlineOffset2;
        float2 _OutlineOffset3;
        float _GradientScale;
        float4 _FaceColor;
        float4 _IsoPerimeter;
        float4 _OutlineColor1;
        float4 _OutlineColor2;
        float4 _OutlineColor3;
        float _OutlineMode;
        float3 _Softness;
        float4 _FaceTex_TexelSize;
        float2 _FaceUVSpeed;
        float4 _FaceTex_ST;
        float4 _OutlineTex_ST;
        float2 _OutlineUVSpeed;
        float4 _UnderlayColor;
        float2 _UnderlayOffset;
        float _UnderlayDilate;
        float _UnderlaySoftness;
        float _BevelType;
        float _BevelAmount;
        float _BevelOffset;
        float _BevelWidth;
        float _BevelRoundness;
        float _BevelClamp;
        float4 _SpecularColor;
        float _LightAngle;
        float _SpecularPower;
        float _Reflectivity;
        float _Diffuse;
        float _Ambient;
        float4 _OutlineTex_TexelSize;
        float _ScaleRatioA;
        float4 Emissive;
        float _smoothness;
        float _Metalic;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_FaceTex);
        SAMPLER(sampler_FaceTex);
        SAMPLER(SamplerState_Linear_Clamp);
        TEXTURE2D(_OutlineTex);
        SAMPLER(sampler_OutlineTex);
        
        // Graph Includes
        #include "Assets/TextMesh Pro/Shaders/SDFFunctions.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Divide_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _UV_533a59502f0abc8bb2c09828f32ebead_Out_0 = IN.uv0;
            UnityTexture2D _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_389f09eeac16ed8eb7a6151195af0507_Width_0 = _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0.texelSize.z;
            float _TexelSize_389f09eeac16ed8eb7a6151195af0507_Height_2 = _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0.texelSize.w;
            float _ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3;
            ScreenSpaceRatio_float((_UV_533a59502f0abc8bb2c09828f32ebead_Out_0.xy), _TexelSize_389f09eeac16ed8eb7a6151195af0507_Width_0, 0, _ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3);
            UnityTexture2D _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(IN.uv0.xy));
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_R_4 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.r;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_G_5 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.g;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_B_6 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.b;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_A_7 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.a;
            float4 _UV_b26868a97b712882abeca1b58698beb0_Out_0 = IN.uv0;
            float2 _Property_e4fdf31842293c8fb0a7eb85f924b8b8_Out_0 = _OutlineOffset1;
            float _Property_c650c0154e947e898564d7d1d007d48e_Out_0 = _GradientScale;
            UnityTexture2D _Property_6e377359c1349380b9482e9613fcec6b_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Width_0 = _Property_6e377359c1349380b9482e9613fcec6b_Out_0.texelSize.z;
            float _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Height_2 = _Property_6e377359c1349380b9482e9613fcec6b_Out_0.texelSize.w;
            float4 _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4;
            float3 _Combine_fd0cd2353f78958d948ad3086c76645e_RGB_5;
            float2 _Combine_fd0cd2353f78958d948ad3086c76645e_RG_6;
            Unity_Combine_float(_TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Width_0, _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Height_2, 0, 0, _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4, _Combine_fd0cd2353f78958d948ad3086c76645e_RGB_5, _Combine_fd0cd2353f78958d948ad3086c76645e_RG_6);
            float4 _Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2;
            Unity_Divide_float4((_Property_c650c0154e947e898564d7d1d007d48e_Out_0.xxxx), _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4, _Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2);
            float2 _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2;
            Unity_Multiply_float2_float2(_Property_e4fdf31842293c8fb0a7eb85f924b8b8_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2);
            float2 _Subtract_8ecda3d29d47068e8b76538959fab084_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2, _Subtract_8ecda3d29d47068e8b76538959fab084_Out_2);
            float4 _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_8ecda3d29d47068e8b76538959fab084_Out_2));
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_R_4 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.r;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_G_5 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.g;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_B_6 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.b;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_A_7 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.a;
            float2 _Property_5a3269796f550283a99abee895aeedd4_Out_0 = _OutlineOffset2;
            float2 _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2;
            Unity_Multiply_float2_float2((_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Property_5a3269796f550283a99abee895aeedd4_Out_0, _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2);
            float2 _Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2, _Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2);
            float4 _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2));
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_R_4 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.r;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_G_5 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.g;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_B_6 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.b;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_A_7 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.a;
            float2 _Property_20cbcbce438f6581847bd3da66673af8_Out_0 = _OutlineOffset3;
            float2 _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2;
            Unity_Multiply_float2_float2(_Property_20cbcbce438f6581847bd3da66673af8_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2);
            float2 _Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2, _Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2);
            float4 _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2));
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_R_4 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.r;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_G_5 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.g;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_B_6 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.b;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_A_7 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.a;
            float4 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4;
            float3 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGB_5;
            float2 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RG_6;
            Unity_Combine_float(_SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_A_7, _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_A_7, _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_A_7, _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_A_7, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGB_5, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RG_6);
            float _Property_bdf7e7b4670fdc86a20f193fb041efd3_Out_0 = _GradientScale;
            float4 _Property_f5d0374e48ad748a9e62658056ab81a3_Out_0 = _IsoPerimeter;
            float3 _Property_05d545af6f7ff08387320e1130c67389_Out_0 = _Softness;
            float _Property_ab4e94278c3a0b8eba9ceccb7822f574_Out_0 = _OutlineMode;
            float4 _ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6;
            ComputeSDF44_float(_ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4, _Property_bdf7e7b4670fdc86a20f193fb041efd3_Out_0, _Property_f5d0374e48ad748a9e62658056ab81a3_Out_0, (float4(_Property_05d545af6f7ff08387320e1130c67389_Out_0, 1.0)), _Property_ab4e94278c3a0b8eba9ceccb7822f574_Out_0, _ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6);
            float4 _Property_eaed1eca838e7183ae708f9502a50dba_Out_0 = _FaceColor;
            UnityTexture2D _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0 = UnityBuildTexture2DStructNoScale(_FaceTex);
            float4 _UV_90e11fefcc71b18b8d88b417548d224f_Out_0 = IN.uv0;
            float4 _Property_26b98e8b908b638d92c1cf36c28e2c9f_Out_0 = _FaceTex_ST;
            float2 _Property_ced4707a1339568eb002c789407f8a3b_Out_0 = _FaceUVSpeed;
            float2 _GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3;
            GenerateUV_float((_UV_90e11fefcc71b18b8d88b417548d224f_Out_0.xy), _Property_26b98e8b908b638d92c1cf36c28e2c9f_Out_0, _Property_ced4707a1339568eb002c789407f8a3b_Out_0, _GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3);
            float4 _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0 = SAMPLE_TEXTURE2D(_Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.tex, _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.samplerstate, _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.GetTransformedUV(_GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3));
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_R_4 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.r;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_G_5 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.g;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_B_6 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.b;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_A_7 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.a;
            float4 _Multiply_48133834a18f14828ac11b58bc205702_Out_2;
            Unity_Multiply_float4_float4(_Property_eaed1eca838e7183ae708f9502a50dba_Out_0, _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0, _Multiply_48133834a18f14828ac11b58bc205702_Out_2);
            float4 _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2;
            Unity_Multiply_float4_float4(IN.VertexColor, _Multiply_48133834a18f14828ac11b58bc205702_Out_2, _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2);
            float4 _Property_4637336160f04f8397ca8f3b6531e88e_Out_0 = _OutlineColor1;
            UnityTexture2D _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0 = UnityBuildTexture2DStructNoScale(_OutlineTex);
            float4 _UV_343788cf38e6598db275321a22b05984_Out_0 = IN.uv0;
            float4 _Property_f5b7ef749a5e5a80911752b8c751465e_Out_0 = _OutlineTex_ST;
            float2 _Property_11ca84fc4adc63868541229e4f404eae_Out_0 = _OutlineUVSpeed;
            float2 _GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3;
            GenerateUV_float((_UV_343788cf38e6598db275321a22b05984_Out_0.xy), _Property_f5b7ef749a5e5a80911752b8c751465e_Out_0, _Property_11ca84fc4adc63868541229e4f404eae_Out_0, _GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3);
            float4 _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0 = SAMPLE_TEXTURE2D(_Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.tex, _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.samplerstate, _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.GetTransformedUV(_GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3));
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_R_4 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.r;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_G_5 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.g;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_B_6 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.b;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_A_7 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.a;
            float4 _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2;
            Unity_Multiply_float4_float4(_Property_4637336160f04f8397ca8f3b6531e88e_Out_0, _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0, _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2);
            float4 _Property_6e8acbed3933c6819383cb89fe70a942_Out_0 = _OutlineColor2;
            float2 _Property_baa8c75173022e86bde4cc075585af43_Out_0 = _OutlineOffset3;
            float4 _Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5;
            Layer4_float(_ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6, _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2, _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2, _Property_6e8acbed3933c6819383cb89fe70a942_Out_0, (float4(_Property_baa8c75173022e86bde4cc075585af43_Out_0, 0.0, 1.0)), _Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5);
            UnityTexture2D _Property_5b0219fd9b23c785b6053d92dc125ed9_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            UnityTexture2D _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_abc928409a963289955625c60e3c3674_Width_0 = _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0.texelSize.z;
            float _TexelSize_abc928409a963289955625c60e3c3674_Height_2 = _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0.texelSize.w;
            float4 _UV_7afe81a63629c789aece6cf7ed368769_Out_0 = IN.uv0;
            float _IsFrontFace_33ef6b7d7f95818ba696de9dae14e559_Out_0 = max(0, IN.FaceSign.x);
            float3 _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5;
            GetSurfaceNormal_float(_Property_5b0219fd9b23c785b6053d92dc125ed9_Out_0.tex, _TexelSize_abc928409a963289955625c60e3c3674_Width_0, _TexelSize_abc928409a963289955625c60e3c3674_Height_2, (_UV_7afe81a63629c789aece6cf7ed368769_Out_0).x, _IsFrontFace_33ef6b7d7f95818ba696de9dae14e559_Out_0, _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5);
            float4 _EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2;
            EvaluateLight_float(_Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5, _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5, _EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2);
            UnityTexture2D _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _UV_87b50231796c43898a28be7b2f8daea3_Out_0 = IN.uv0;
            float2 _Property_c20a130451687b8c87b5a623d8ca6d73_Out_0 = _UnderlayOffset;
            float2 _Multiply_7d754a30a319c9839ca945e596548e63_Out_2;
            Unity_Multiply_float2_float2(_Property_c20a130451687b8c87b5a623d8ca6d73_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_7d754a30a319c9839ca945e596548e63_Out_2);
            float2 _Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2;
            Unity_Subtract_float2((_UV_87b50231796c43898a28be7b2f8daea3_Out_0.xy), _Multiply_7d754a30a319c9839ca945e596548e63_Out_2, _Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2);
            float4 _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0 = SAMPLE_TEXTURE2D(_Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.tex, _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.samplerstate, _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.GetTransformedUV(_Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2));
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_R_4 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.r;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_G_5 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.g;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_B_6 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.b;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_A_7 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.a;
            float _Property_cb906f6db0fb388a91590590b04091ab_Out_0 = _GradientScale;
            float _Property_22272afe5996178386aac669fbda451b_Out_0 = _UnderlayDilate;
            float _Property_1cd799c4097e1685b9dbf5dd80b6062b_Out_0 = _UnderlaySoftness;
            float _ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0;
            ComputeSDF_float(_ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3, (_SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0).x, _Property_cb906f6db0fb388a91590590b04091ab_Out_0, _Property_22272afe5996178386aac669fbda451b_Out_0, _Property_1cd799c4097e1685b9dbf5dd80b6062b_Out_0, _ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0);
            float4 _Property_4be3e1d3be22b78f9f4e23b78e8d6064_Out_0 = _UnderlayColor;
            float4 _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2;
            Layer1_float(_ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0, _Property_4be3e1d3be22b78f9f4e23b78e8d6064_Out_0, _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2);
            float4 _CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2;
            Composite_float(_EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2, _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2, _CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2);
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_R_1 = IN.VertexColor[0];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_G_2 = IN.VertexColor[1];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_B_3 = IN.VertexColor[2];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_A_4 = IN.VertexColor[3];
            float4 _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2;
            Unity_Multiply_float4_float4(_CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2, (_Split_79de04ac9d55bc8693a6a09c54eed8bb_A_4.xxxx), _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2);
            float _Split_9b3e8141df129b848c4c1b364b613b86_R_1 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[0];
            float _Split_9b3e8141df129b848c4c1b364b613b86_G_2 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[1];
            float _Split_9b3e8141df129b848c4c1b364b613b86_B_3 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[2];
            float _Split_9b3e8141df129b848c4c1b364b613b86_A_4 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[3];
            surface.Alpha = _Split_9b3e8141df129b848c4c1b364b613b86_A_4;
            surface.AlphaClipThreshold = 0.5;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
            output.uv0 = input.texCoord0;
            output.uv1 = input.texCoord1;
            output.uv2 = input.texCoord2;
            output.uv3 = input.texCoord3;
            output.VertexColor = input.color;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
            BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ScenePickingPass"
            Tags
            {
                "LightMode" = "Picking"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma vertex vert
        #pragma fragment frag
        
        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define ATTRIBUTES_NEED_TEXCOORD3
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD1
        #define VARYINGS_NEED_TEXCOORD2
        #define VARYINGS_NEED_TEXCOORD3
        #define VARYINGS_NEED_COLOR
        #define VARYINGS_NEED_CULLFACE
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENEPICKINGPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        #define _ALPHATEST_ON 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
             float4 uv3 : TEXCOORD3;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
             float4 texCoord1;
             float4 texCoord2;
             float4 texCoord3;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
             float4 uv1;
             float4 uv2;
             float4 uv3;
             float4 VertexColor;
             float FaceSign;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 interp0 : INTERP0;
             float4 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
             float4 interp4 : INTERP4;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyzw =  input.texCoord0;
            output.interp1.xyzw =  input.texCoord1;
            output.interp2.xyzw =  input.texCoord2;
            output.interp3.xyzw =  input.texCoord3;
            output.interp4.xyzw =  input.color;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp0.xyzw;
            output.texCoord1 = input.interp1.xyzw;
            output.texCoord2 = input.interp2.xyzw;
            output.texCoord3 = input.interp3.xyzw;
            output.color = input.interp4.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_TexelSize;
        float2 _OutlineOffset1;
        float2 _OutlineOffset2;
        float2 _OutlineOffset3;
        float _GradientScale;
        float4 _FaceColor;
        float4 _IsoPerimeter;
        float4 _OutlineColor1;
        float4 _OutlineColor2;
        float4 _OutlineColor3;
        float _OutlineMode;
        float3 _Softness;
        float4 _FaceTex_TexelSize;
        float2 _FaceUVSpeed;
        float4 _FaceTex_ST;
        float4 _OutlineTex_ST;
        float2 _OutlineUVSpeed;
        float4 _UnderlayColor;
        float2 _UnderlayOffset;
        float _UnderlayDilate;
        float _UnderlaySoftness;
        float _BevelType;
        float _BevelAmount;
        float _BevelOffset;
        float _BevelWidth;
        float _BevelRoundness;
        float _BevelClamp;
        float4 _SpecularColor;
        float _LightAngle;
        float _SpecularPower;
        float _Reflectivity;
        float _Diffuse;
        float _Ambient;
        float4 _OutlineTex_TexelSize;
        float _ScaleRatioA;
        float4 Emissive;
        float _smoothness;
        float _Metalic;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_FaceTex);
        SAMPLER(sampler_FaceTex);
        SAMPLER(SamplerState_Linear_Clamp);
        TEXTURE2D(_OutlineTex);
        SAMPLER(sampler_OutlineTex);
        
        // Graph Includes
        #include "Assets/TextMesh Pro/Shaders/SDFFunctions.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Divide_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _UV_533a59502f0abc8bb2c09828f32ebead_Out_0 = IN.uv0;
            UnityTexture2D _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_389f09eeac16ed8eb7a6151195af0507_Width_0 = _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0.texelSize.z;
            float _TexelSize_389f09eeac16ed8eb7a6151195af0507_Height_2 = _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0.texelSize.w;
            float _ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3;
            ScreenSpaceRatio_float((_UV_533a59502f0abc8bb2c09828f32ebead_Out_0.xy), _TexelSize_389f09eeac16ed8eb7a6151195af0507_Width_0, 0, _ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3);
            UnityTexture2D _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(IN.uv0.xy));
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_R_4 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.r;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_G_5 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.g;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_B_6 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.b;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_A_7 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.a;
            float4 _UV_b26868a97b712882abeca1b58698beb0_Out_0 = IN.uv0;
            float2 _Property_e4fdf31842293c8fb0a7eb85f924b8b8_Out_0 = _OutlineOffset1;
            float _Property_c650c0154e947e898564d7d1d007d48e_Out_0 = _GradientScale;
            UnityTexture2D _Property_6e377359c1349380b9482e9613fcec6b_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Width_0 = _Property_6e377359c1349380b9482e9613fcec6b_Out_0.texelSize.z;
            float _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Height_2 = _Property_6e377359c1349380b9482e9613fcec6b_Out_0.texelSize.w;
            float4 _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4;
            float3 _Combine_fd0cd2353f78958d948ad3086c76645e_RGB_5;
            float2 _Combine_fd0cd2353f78958d948ad3086c76645e_RG_6;
            Unity_Combine_float(_TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Width_0, _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Height_2, 0, 0, _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4, _Combine_fd0cd2353f78958d948ad3086c76645e_RGB_5, _Combine_fd0cd2353f78958d948ad3086c76645e_RG_6);
            float4 _Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2;
            Unity_Divide_float4((_Property_c650c0154e947e898564d7d1d007d48e_Out_0.xxxx), _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4, _Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2);
            float2 _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2;
            Unity_Multiply_float2_float2(_Property_e4fdf31842293c8fb0a7eb85f924b8b8_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2);
            float2 _Subtract_8ecda3d29d47068e8b76538959fab084_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2, _Subtract_8ecda3d29d47068e8b76538959fab084_Out_2);
            float4 _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_8ecda3d29d47068e8b76538959fab084_Out_2));
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_R_4 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.r;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_G_5 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.g;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_B_6 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.b;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_A_7 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.a;
            float2 _Property_5a3269796f550283a99abee895aeedd4_Out_0 = _OutlineOffset2;
            float2 _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2;
            Unity_Multiply_float2_float2((_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Property_5a3269796f550283a99abee895aeedd4_Out_0, _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2);
            float2 _Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2, _Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2);
            float4 _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2));
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_R_4 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.r;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_G_5 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.g;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_B_6 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.b;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_A_7 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.a;
            float2 _Property_20cbcbce438f6581847bd3da66673af8_Out_0 = _OutlineOffset3;
            float2 _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2;
            Unity_Multiply_float2_float2(_Property_20cbcbce438f6581847bd3da66673af8_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2);
            float2 _Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2, _Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2);
            float4 _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2));
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_R_4 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.r;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_G_5 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.g;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_B_6 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.b;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_A_7 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.a;
            float4 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4;
            float3 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGB_5;
            float2 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RG_6;
            Unity_Combine_float(_SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_A_7, _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_A_7, _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_A_7, _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_A_7, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGB_5, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RG_6);
            float _Property_bdf7e7b4670fdc86a20f193fb041efd3_Out_0 = _GradientScale;
            float4 _Property_f5d0374e48ad748a9e62658056ab81a3_Out_0 = _IsoPerimeter;
            float3 _Property_05d545af6f7ff08387320e1130c67389_Out_0 = _Softness;
            float _Property_ab4e94278c3a0b8eba9ceccb7822f574_Out_0 = _OutlineMode;
            float4 _ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6;
            ComputeSDF44_float(_ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4, _Property_bdf7e7b4670fdc86a20f193fb041efd3_Out_0, _Property_f5d0374e48ad748a9e62658056ab81a3_Out_0, (float4(_Property_05d545af6f7ff08387320e1130c67389_Out_0, 1.0)), _Property_ab4e94278c3a0b8eba9ceccb7822f574_Out_0, _ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6);
            float4 _Property_eaed1eca838e7183ae708f9502a50dba_Out_0 = _FaceColor;
            UnityTexture2D _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0 = UnityBuildTexture2DStructNoScale(_FaceTex);
            float4 _UV_90e11fefcc71b18b8d88b417548d224f_Out_0 = IN.uv0;
            float4 _Property_26b98e8b908b638d92c1cf36c28e2c9f_Out_0 = _FaceTex_ST;
            float2 _Property_ced4707a1339568eb002c789407f8a3b_Out_0 = _FaceUVSpeed;
            float2 _GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3;
            GenerateUV_float((_UV_90e11fefcc71b18b8d88b417548d224f_Out_0.xy), _Property_26b98e8b908b638d92c1cf36c28e2c9f_Out_0, _Property_ced4707a1339568eb002c789407f8a3b_Out_0, _GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3);
            float4 _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0 = SAMPLE_TEXTURE2D(_Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.tex, _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.samplerstate, _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.GetTransformedUV(_GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3));
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_R_4 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.r;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_G_5 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.g;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_B_6 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.b;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_A_7 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.a;
            float4 _Multiply_48133834a18f14828ac11b58bc205702_Out_2;
            Unity_Multiply_float4_float4(_Property_eaed1eca838e7183ae708f9502a50dba_Out_0, _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0, _Multiply_48133834a18f14828ac11b58bc205702_Out_2);
            float4 _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2;
            Unity_Multiply_float4_float4(IN.VertexColor, _Multiply_48133834a18f14828ac11b58bc205702_Out_2, _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2);
            float4 _Property_4637336160f04f8397ca8f3b6531e88e_Out_0 = _OutlineColor1;
            UnityTexture2D _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0 = UnityBuildTexture2DStructNoScale(_OutlineTex);
            float4 _UV_343788cf38e6598db275321a22b05984_Out_0 = IN.uv0;
            float4 _Property_f5b7ef749a5e5a80911752b8c751465e_Out_0 = _OutlineTex_ST;
            float2 _Property_11ca84fc4adc63868541229e4f404eae_Out_0 = _OutlineUVSpeed;
            float2 _GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3;
            GenerateUV_float((_UV_343788cf38e6598db275321a22b05984_Out_0.xy), _Property_f5b7ef749a5e5a80911752b8c751465e_Out_0, _Property_11ca84fc4adc63868541229e4f404eae_Out_0, _GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3);
            float4 _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0 = SAMPLE_TEXTURE2D(_Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.tex, _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.samplerstate, _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.GetTransformedUV(_GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3));
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_R_4 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.r;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_G_5 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.g;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_B_6 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.b;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_A_7 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.a;
            float4 _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2;
            Unity_Multiply_float4_float4(_Property_4637336160f04f8397ca8f3b6531e88e_Out_0, _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0, _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2);
            float4 _Property_6e8acbed3933c6819383cb89fe70a942_Out_0 = _OutlineColor2;
            float2 _Property_baa8c75173022e86bde4cc075585af43_Out_0 = _OutlineOffset3;
            float4 _Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5;
            Layer4_float(_ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6, _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2, _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2, _Property_6e8acbed3933c6819383cb89fe70a942_Out_0, (float4(_Property_baa8c75173022e86bde4cc075585af43_Out_0, 0.0, 1.0)), _Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5);
            UnityTexture2D _Property_5b0219fd9b23c785b6053d92dc125ed9_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            UnityTexture2D _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_abc928409a963289955625c60e3c3674_Width_0 = _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0.texelSize.z;
            float _TexelSize_abc928409a963289955625c60e3c3674_Height_2 = _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0.texelSize.w;
            float4 _UV_7afe81a63629c789aece6cf7ed368769_Out_0 = IN.uv0;
            float _IsFrontFace_33ef6b7d7f95818ba696de9dae14e559_Out_0 = max(0, IN.FaceSign.x);
            float3 _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5;
            GetSurfaceNormal_float(_Property_5b0219fd9b23c785b6053d92dc125ed9_Out_0.tex, _TexelSize_abc928409a963289955625c60e3c3674_Width_0, _TexelSize_abc928409a963289955625c60e3c3674_Height_2, (_UV_7afe81a63629c789aece6cf7ed368769_Out_0).x, _IsFrontFace_33ef6b7d7f95818ba696de9dae14e559_Out_0, _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5);
            float4 _EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2;
            EvaluateLight_float(_Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5, _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5, _EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2);
            UnityTexture2D _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _UV_87b50231796c43898a28be7b2f8daea3_Out_0 = IN.uv0;
            float2 _Property_c20a130451687b8c87b5a623d8ca6d73_Out_0 = _UnderlayOffset;
            float2 _Multiply_7d754a30a319c9839ca945e596548e63_Out_2;
            Unity_Multiply_float2_float2(_Property_c20a130451687b8c87b5a623d8ca6d73_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_7d754a30a319c9839ca945e596548e63_Out_2);
            float2 _Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2;
            Unity_Subtract_float2((_UV_87b50231796c43898a28be7b2f8daea3_Out_0.xy), _Multiply_7d754a30a319c9839ca945e596548e63_Out_2, _Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2);
            float4 _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0 = SAMPLE_TEXTURE2D(_Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.tex, _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.samplerstate, _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.GetTransformedUV(_Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2));
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_R_4 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.r;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_G_5 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.g;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_B_6 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.b;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_A_7 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.a;
            float _Property_cb906f6db0fb388a91590590b04091ab_Out_0 = _GradientScale;
            float _Property_22272afe5996178386aac669fbda451b_Out_0 = _UnderlayDilate;
            float _Property_1cd799c4097e1685b9dbf5dd80b6062b_Out_0 = _UnderlaySoftness;
            float _ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0;
            ComputeSDF_float(_ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3, (_SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0).x, _Property_cb906f6db0fb388a91590590b04091ab_Out_0, _Property_22272afe5996178386aac669fbda451b_Out_0, _Property_1cd799c4097e1685b9dbf5dd80b6062b_Out_0, _ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0);
            float4 _Property_4be3e1d3be22b78f9f4e23b78e8d6064_Out_0 = _UnderlayColor;
            float4 _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2;
            Layer1_float(_ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0, _Property_4be3e1d3be22b78f9f4e23b78e8d6064_Out_0, _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2);
            float4 _CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2;
            Composite_float(_EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2, _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2, _CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2);
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_R_1 = IN.VertexColor[0];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_G_2 = IN.VertexColor[1];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_B_3 = IN.VertexColor[2];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_A_4 = IN.VertexColor[3];
            float4 _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2;
            Unity_Multiply_float4_float4(_CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2, (_Split_79de04ac9d55bc8693a6a09c54eed8bb_A_4.xxxx), _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2);
            float _Split_9b3e8141df129b848c4c1b364b613b86_R_1 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[0];
            float _Split_9b3e8141df129b848c4c1b364b613b86_G_2 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[1];
            float _Split_9b3e8141df129b848c4c1b364b613b86_B_3 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[2];
            float _Split_9b3e8141df129b848c4c1b364b613b86_A_4 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[3];
            surface.Alpha = _Split_9b3e8141df129b848c4c1b364b613b86_A_4;
            surface.AlphaClipThreshold = 0.5;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
            output.uv0 = input.texCoord0;
            output.uv1 = input.texCoord1;
            output.uv2 = input.texCoord2;
            output.uv3 = input.texCoord3;
            output.VertexColor = input.color;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
            BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            // Name: <None>
            Tags
            {
                "LightMode" = "Universal2D"
            }
        
        // Render State
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma vertex vert
        #pragma fragment frag
        
        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define ATTRIBUTES_NEED_TEXCOORD3
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD1
        #define VARYINGS_NEED_TEXCOORD2
        #define VARYINGS_NEED_TEXCOORD3
        #define VARYINGS_NEED_COLOR
        #define VARYINGS_NEED_CULLFACE
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_2D
        #define _ALPHATEST_ON 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
             float4 uv3 : TEXCOORD3;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
             float4 texCoord1;
             float4 texCoord2;
             float4 texCoord3;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
             float4 uv1;
             float4 uv2;
             float4 uv3;
             float4 VertexColor;
             float FaceSign;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 interp0 : INTERP0;
             float4 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
             float4 interp4 : INTERP4;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyzw =  input.texCoord0;
            output.interp1.xyzw =  input.texCoord1;
            output.interp2.xyzw =  input.texCoord2;
            output.interp3.xyzw =  input.texCoord3;
            output.interp4.xyzw =  input.color;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp0.xyzw;
            output.texCoord1 = input.interp1.xyzw;
            output.texCoord2 = input.interp2.xyzw;
            output.texCoord3 = input.interp3.xyzw;
            output.color = input.interp4.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_TexelSize;
        float2 _OutlineOffset1;
        float2 _OutlineOffset2;
        float2 _OutlineOffset3;
        float _GradientScale;
        float4 _FaceColor;
        float4 _IsoPerimeter;
        float4 _OutlineColor1;
        float4 _OutlineColor2;
        float4 _OutlineColor3;
        float _OutlineMode;
        float3 _Softness;
        float4 _FaceTex_TexelSize;
        float2 _FaceUVSpeed;
        float4 _FaceTex_ST;
        float4 _OutlineTex_ST;
        float2 _OutlineUVSpeed;
        float4 _UnderlayColor;
        float2 _UnderlayOffset;
        float _UnderlayDilate;
        float _UnderlaySoftness;
        float _BevelType;
        float _BevelAmount;
        float _BevelOffset;
        float _BevelWidth;
        float _BevelRoundness;
        float _BevelClamp;
        float4 _SpecularColor;
        float _LightAngle;
        float _SpecularPower;
        float _Reflectivity;
        float _Diffuse;
        float _Ambient;
        float4 _OutlineTex_TexelSize;
        float _ScaleRatioA;
        float4 Emissive;
        float _smoothness;
        float _Metalic;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_FaceTex);
        SAMPLER(sampler_FaceTex);
        SAMPLER(SamplerState_Linear_Clamp);
        TEXTURE2D(_OutlineTex);
        SAMPLER(sampler_OutlineTex);
        
        // Graph Includes
        #include "Assets/TextMesh Pro/Shaders/SDFFunctions.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Divide_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _UV_533a59502f0abc8bb2c09828f32ebead_Out_0 = IN.uv0;
            UnityTexture2D _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_389f09eeac16ed8eb7a6151195af0507_Width_0 = _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0.texelSize.z;
            float _TexelSize_389f09eeac16ed8eb7a6151195af0507_Height_2 = _Property_1fe43c50e88d8d82b3ef889f12dcfb65_Out_0.texelSize.w;
            float _ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3;
            ScreenSpaceRatio_float((_UV_533a59502f0abc8bb2c09828f32ebead_Out_0.xy), _TexelSize_389f09eeac16ed8eb7a6151195af0507_Width_0, 0, _ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3);
            UnityTexture2D _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(IN.uv0.xy));
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_R_4 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.r;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_G_5 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.g;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_B_6 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.b;
            float _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_A_7 = _SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_RGBA_0.a;
            float4 _UV_b26868a97b712882abeca1b58698beb0_Out_0 = IN.uv0;
            float2 _Property_e4fdf31842293c8fb0a7eb85f924b8b8_Out_0 = _OutlineOffset1;
            float _Property_c650c0154e947e898564d7d1d007d48e_Out_0 = _GradientScale;
            UnityTexture2D _Property_6e377359c1349380b9482e9613fcec6b_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Width_0 = _Property_6e377359c1349380b9482e9613fcec6b_Out_0.texelSize.z;
            float _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Height_2 = _Property_6e377359c1349380b9482e9613fcec6b_Out_0.texelSize.w;
            float4 _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4;
            float3 _Combine_fd0cd2353f78958d948ad3086c76645e_RGB_5;
            float2 _Combine_fd0cd2353f78958d948ad3086c76645e_RG_6;
            Unity_Combine_float(_TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Width_0, _TexelSize_50a5d2cfcb0fb5868c2afda9bcb48dc7_Height_2, 0, 0, _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4, _Combine_fd0cd2353f78958d948ad3086c76645e_RGB_5, _Combine_fd0cd2353f78958d948ad3086c76645e_RG_6);
            float4 _Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2;
            Unity_Divide_float4((_Property_c650c0154e947e898564d7d1d007d48e_Out_0.xxxx), _Combine_fd0cd2353f78958d948ad3086c76645e_RGBA_4, _Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2);
            float2 _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2;
            Unity_Multiply_float2_float2(_Property_e4fdf31842293c8fb0a7eb85f924b8b8_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2);
            float2 _Subtract_8ecda3d29d47068e8b76538959fab084_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_9af20166c7f70c88a2d4c144834f02e5_Out_2, _Subtract_8ecda3d29d47068e8b76538959fab084_Out_2);
            float4 _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_8ecda3d29d47068e8b76538959fab084_Out_2));
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_R_4 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.r;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_G_5 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.g;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_B_6 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.b;
            float _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_A_7 = _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_RGBA_0.a;
            float2 _Property_5a3269796f550283a99abee895aeedd4_Out_0 = _OutlineOffset2;
            float2 _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2;
            Unity_Multiply_float2_float2((_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Property_5a3269796f550283a99abee895aeedd4_Out_0, _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2);
            float2 _Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_85a4a1da71c88a8aa9cf834b093b4972_Out_2, _Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2);
            float4 _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_02ec11df97f0828fb90fe51bc7b54681_Out_2));
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_R_4 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.r;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_G_5 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.g;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_B_6 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.b;
            float _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_A_7 = _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_RGBA_0.a;
            float2 _Property_20cbcbce438f6581847bd3da66673af8_Out_0 = _OutlineOffset3;
            float2 _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2;
            Unity_Multiply_float2_float2(_Property_20cbcbce438f6581847bd3da66673af8_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2);
            float2 _Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2;
            Unity_Subtract_float2((_UV_b26868a97b712882abeca1b58698beb0_Out_0.xy), _Multiply_25810e5a76f1a28eb6ec31562b2826f7_Out_2, _Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2);
            float4 _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0 = SAMPLE_TEXTURE2D(_Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.tex, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.samplerstate, _Property_954c72ffa0c26f869fe03d926f8a55be_Out_0.GetTransformedUV(_Subtract_35bf56304ae79a858535fc1f6d2bb434_Out_2));
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_R_4 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.r;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_G_5 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.g;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_B_6 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.b;
            float _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_A_7 = _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_RGBA_0.a;
            float4 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4;
            float3 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGB_5;
            float2 _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RG_6;
            Unity_Combine_float(_SampleTexture2D_c7724e68891dfd8fb3f6f6438d260c7e_A_7, _SampleTexture2D_6eff60a2fbddd98c88812ff580f90b76_A_7, _SampleTexture2D_2ecc410c1f24678bb1f0bf27189d4db2_A_7, _SampleTexture2D_ff6dd86b2d329e848816a50765c58716_A_7, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGB_5, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RG_6);
            float _Property_bdf7e7b4670fdc86a20f193fb041efd3_Out_0 = _GradientScale;
            float4 _Property_f5d0374e48ad748a9e62658056ab81a3_Out_0 = _IsoPerimeter;
            float3 _Property_05d545af6f7ff08387320e1130c67389_Out_0 = _Softness;
            float _Property_ab4e94278c3a0b8eba9ceccb7822f574_Out_0 = _OutlineMode;
            float4 _ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6;
            ComputeSDF44_float(_ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3, _Combine_a777dc93e8ca248cb6fe690cc4a77a5a_RGBA_4, _Property_bdf7e7b4670fdc86a20f193fb041efd3_Out_0, _Property_f5d0374e48ad748a9e62658056ab81a3_Out_0, (float4(_Property_05d545af6f7ff08387320e1130c67389_Out_0, 1.0)), _Property_ab4e94278c3a0b8eba9ceccb7822f574_Out_0, _ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6);
            float4 _Property_eaed1eca838e7183ae708f9502a50dba_Out_0 = _FaceColor;
            UnityTexture2D _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0 = UnityBuildTexture2DStructNoScale(_FaceTex);
            float4 _UV_90e11fefcc71b18b8d88b417548d224f_Out_0 = IN.uv0;
            float4 _Property_26b98e8b908b638d92c1cf36c28e2c9f_Out_0 = _FaceTex_ST;
            float2 _Property_ced4707a1339568eb002c789407f8a3b_Out_0 = _FaceUVSpeed;
            float2 _GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3;
            GenerateUV_float((_UV_90e11fefcc71b18b8d88b417548d224f_Out_0.xy), _Property_26b98e8b908b638d92c1cf36c28e2c9f_Out_0, _Property_ced4707a1339568eb002c789407f8a3b_Out_0, _GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3);
            float4 _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0 = SAMPLE_TEXTURE2D(_Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.tex, _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.samplerstate, _Property_8be90b40fe8d878fb21c1cc890b86378_Out_0.GetTransformedUV(_GenerateUVCustomFunction_31c71b52d27c2b86a20c03f7256c5fa5_UV_3));
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_R_4 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.r;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_G_5 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.g;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_B_6 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.b;
            float _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_A_7 = _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0.a;
            float4 _Multiply_48133834a18f14828ac11b58bc205702_Out_2;
            Unity_Multiply_float4_float4(_Property_eaed1eca838e7183ae708f9502a50dba_Out_0, _SampleTexture2D_c882f19d0ae7ed87b310a0d75334a39f_RGBA_0, _Multiply_48133834a18f14828ac11b58bc205702_Out_2);
            float4 _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2;
            Unity_Multiply_float4_float4(IN.VertexColor, _Multiply_48133834a18f14828ac11b58bc205702_Out_2, _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2);
            float4 _Property_4637336160f04f8397ca8f3b6531e88e_Out_0 = _OutlineColor1;
            UnityTexture2D _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0 = UnityBuildTexture2DStructNoScale(_OutlineTex);
            float4 _UV_343788cf38e6598db275321a22b05984_Out_0 = IN.uv0;
            float4 _Property_f5b7ef749a5e5a80911752b8c751465e_Out_0 = _OutlineTex_ST;
            float2 _Property_11ca84fc4adc63868541229e4f404eae_Out_0 = _OutlineUVSpeed;
            float2 _GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3;
            GenerateUV_float((_UV_343788cf38e6598db275321a22b05984_Out_0.xy), _Property_f5b7ef749a5e5a80911752b8c751465e_Out_0, _Property_11ca84fc4adc63868541229e4f404eae_Out_0, _GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3);
            float4 _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0 = SAMPLE_TEXTURE2D(_Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.tex, _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.samplerstate, _Property_dae1c5bdd7aa6687b1cd165feffd5710_Out_0.GetTransformedUV(_GenerateUVCustomFunction_dadb37f57181a4829003e40234b9d409_UV_3));
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_R_4 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.r;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_G_5 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.g;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_B_6 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.b;
            float _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_A_7 = _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0.a;
            float4 _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2;
            Unity_Multiply_float4_float4(_Property_4637336160f04f8397ca8f3b6531e88e_Out_0, _SampleTexture2D_44ec75290e3e9b8fbcf92c999d6ac8bc_RGBA_0, _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2);
            float4 _Property_6e8acbed3933c6819383cb89fe70a942_Out_0 = _OutlineColor2;
            float2 _Property_baa8c75173022e86bde4cc075585af43_Out_0 = _OutlineOffset3;
            float4 _Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5;
            Layer4_float(_ComputeSDF44CustomFunction_13b6209c286745868057a70fce31d5e1_Alpha_6, _Multiply_223ab9ae77672a8798690299a6dae4bd_Out_2, _Multiply_8d7e7fd5eca24b8db946e4e5a4968f70_Out_2, _Property_6e8acbed3933c6819383cb89fe70a942_Out_0, (float4(_Property_baa8c75173022e86bde4cc075585af43_Out_0, 0.0, 1.0)), _Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5);
            UnityTexture2D _Property_5b0219fd9b23c785b6053d92dc125ed9_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            UnityTexture2D _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float _TexelSize_abc928409a963289955625c60e3c3674_Width_0 = _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0.texelSize.z;
            float _TexelSize_abc928409a963289955625c60e3c3674_Height_2 = _Property_1744832cec65fb85a42db2e1d9cdff97_Out_0.texelSize.w;
            float4 _UV_7afe81a63629c789aece6cf7ed368769_Out_0 = IN.uv0;
            float _IsFrontFace_33ef6b7d7f95818ba696de9dae14e559_Out_0 = max(0, IN.FaceSign.x);
            float3 _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5;
            GetSurfaceNormal_float(_Property_5b0219fd9b23c785b6053d92dc125ed9_Out_0.tex, _TexelSize_abc928409a963289955625c60e3c3674_Width_0, _TexelSize_abc928409a963289955625c60e3c3674_Height_2, (_UV_7afe81a63629c789aece6cf7ed368769_Out_0).x, _IsFrontFace_33ef6b7d7f95818ba696de9dae14e559_Out_0, _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5);
            float4 _EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2;
            EvaluateLight_float(_Layer4CustomFunction_ab624c3948e3a08ab05aef05a98eb8f7_RGBA_5, _GetSurfaceNormalCustomFunction_27813c88a9458283b1866c0fcdaf259a_New5_5, _EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2);
            UnityTexture2D _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _UV_87b50231796c43898a28be7b2f8daea3_Out_0 = IN.uv0;
            float2 _Property_c20a130451687b8c87b5a623d8ca6d73_Out_0 = _UnderlayOffset;
            float2 _Multiply_7d754a30a319c9839ca945e596548e63_Out_2;
            Unity_Multiply_float2_float2(_Property_c20a130451687b8c87b5a623d8ca6d73_Out_0, (_Divide_5e0d454c5ea95b88a8f414c16e1bf059_Out_2.xy), _Multiply_7d754a30a319c9839ca945e596548e63_Out_2);
            float2 _Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2;
            Unity_Subtract_float2((_UV_87b50231796c43898a28be7b2f8daea3_Out_0.xy), _Multiply_7d754a30a319c9839ca945e596548e63_Out_2, _Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2);
            float4 _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0 = SAMPLE_TEXTURE2D(_Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.tex, _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.samplerstate, _Property_3407a26c1e2b438ab594b42c3b95454e_Out_0.GetTransformedUV(_Subtract_ebbd2e0341529d80a91e0d0893b6ee73_Out_2));
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_R_4 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.r;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_G_5 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.g;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_B_6 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.b;
            float _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_A_7 = _SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0.a;
            float _Property_cb906f6db0fb388a91590590b04091ab_Out_0 = _GradientScale;
            float _Property_22272afe5996178386aac669fbda451b_Out_0 = _UnderlayDilate;
            float _Property_1cd799c4097e1685b9dbf5dd80b6062b_Out_0 = _UnderlaySoftness;
            float _ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0;
            ComputeSDF_float(_ScreenSpaceRatioCustomFunction_eef499a7036c138e9121e5151cdb5be1_New3_3, (_SampleTexture2D_fe4f97a213ae1d8aa48bfc4f1aa17fa9_RGBA_0).x, _Property_cb906f6db0fb388a91590590b04091ab_Out_0, _Property_22272afe5996178386aac669fbda451b_Out_0, _Property_1cd799c4097e1685b9dbf5dd80b6062b_Out_0, _ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0);
            float4 _Property_4be3e1d3be22b78f9f4e23b78e8d6064_Out_0 = _UnderlayColor;
            float4 _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2;
            Layer1_float(_ComputeSDFCustomFunction_cb33c8373e2fb98da278a4b6f9bab088_Alpha_0, _Property_4be3e1d3be22b78f9f4e23b78e8d6064_Out_0, _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2);
            float4 _CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2;
            Composite_float(_EvaluateLightCustomFunction_cd74ed92b44ab58b95bf81cf4664fa77_Color_2, _Layer1CustomFunction_58efba96e643d281ba7d757bc713ff15_RGBA_2, _CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2);
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_R_1 = IN.VertexColor[0];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_G_2 = IN.VertexColor[1];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_B_3 = IN.VertexColor[2];
            float _Split_79de04ac9d55bc8693a6a09c54eed8bb_A_4 = IN.VertexColor[3];
            float4 _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2;
            Unity_Multiply_float4_float4(_CompositeCustomFunction_7e22b933a6629e8389cf370acfd80658_RGBA_2, (_Split_79de04ac9d55bc8693a6a09c54eed8bb_A_4.xxxx), _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2);
            float _Split_9b3e8141df129b848c4c1b364b613b86_R_1 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[0];
            float _Split_9b3e8141df129b848c4c1b364b613b86_G_2 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[1];
            float _Split_9b3e8141df129b848c4c1b364b613b86_B_3 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[2];
            float _Split_9b3e8141df129b848c4c1b364b613b86_A_4 = _Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2[3];
            surface.BaseColor = (_Multiply_17b5e496d42b5d89b09b1763aa47bb78_Out_2.xyz);
            surface.Alpha = _Split_9b3e8141df129b848c4c1b364b613b86_A_4;
            surface.AlphaClipThreshold = 0.5;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
            output.uv0 = input.texCoord0;
            output.uv1 = input.texCoord1;
            output.uv2 = input.texCoord2;
            output.uv3 = input.texCoord3;
            output.VertexColor = input.color;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
            BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
    }
    CustomEditorForRenderPipeline "UnityEditor.ShaderGraphLitGUI" "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset"
    CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
    FallBack "Hidden/Shader Graph/FallbackError"
}