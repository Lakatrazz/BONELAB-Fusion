Shader "SLZ/Sky With Fog"
{
    Properties
    {
        _SkyTex ("Sky Texture", CUBE) = "black" {}
        [HDR] _SkyColor ("Sky Color", Color) = (1,1,1,1)
        _FogDist ("Fog Max Distance (0 to camera far clip)", Range(0,1)) = 1.0
    }
    SubShader
    {
        Tags {"RenderPipeline" = "UniversalPipeline"  "RenderType" = "Opaque" "Queue" = "AlphaTest+51" "IgnoreProjector"="True"}
        Blend One Zero
		ZWrite Off
		ZTest LEqual
		//Offset 1,1
		ColorMask RGBA
        LOD 100
        Cull Off

        Pass
        {
            Name "Forward"
            Tags {"Lightmode"="UniversalForward"}
            HLSLPROGRAM
            
             #pragma vertex vert
            #pragma fragment frag
#pragma exclude_renderers gles
            #pragma multi_compile_fragment _ _VOLUMETRICS_ENABLED
            #pragma multi_compile_fragment _ FOG_LINEAR FOG_EXP2
            #define SHADERPASS SHADERPASS_FORWARD

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"


            struct appdata
            {
                //float4 vertex : POSITION;
                half2 uv0 : TEXCOORD0;
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 wPos_xyz_fog_x : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURECUBE(_SkyTex);
            SamplerState sampler_SkyTex;
            CBUFFER_START(UnityPerMaterial)
            half4 _SkyColor;
            float _FogDist;
            CBUFFER_END


            /* Gets the position of a vertex as a part of a right triangle that completely covers the screen
             * Assumes a single triangle mesh, with the positions based on the vertex's ID. 
             * CCW order 
             * 0 : 0,1     0
             * 1 : 0,0     | \
             * 2 : 1,0     1--2
             */
            float4 GetQuadVertexPosition2(uint vertexID, float z = UNITY_NEAR_CLIP_VALUE)
            {
                uint topBit = vertexID >> 1u;
                uint botBit = (vertexID & 1u);
                float y = 1.0f - ((vertexID & 2u) >> 1);
                float x = (vertexID & 1u);//1 - (topBit + botBit) & 1; // produces 1 for indices 0,3 and 0 for 1,2
                return float4(x, y, z, 1.0);
            }
            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                //float z = (rcp(_SkyDist) - _ZBufferParams.y) / (_ZBufferParams.x); // Transform from linear 0-1 depth to clipspace Z
                float4 clipQuad = GetQuadVertexPosition(v.vertexID, UNITY_RAW_FAR_CLIP_VALUE);
                clipQuad.xy = 4.0f * clipQuad.xy - 1.0f;
                //clipQuad.xy = 0.5f * clipQuad.xy - 0.5f;
                float4 wPos = mul(UNITY_MATRIX_I_VP, clipQuad);
                o.wPos_xyz_fog_x.xyz = wPos.xyz / wPos.w;
                o.vertex = clipQuad;
                half clipZ_0Far = lerp(_ProjectionParams.y, _ProjectionParams.z, _FogDist);
                o.wPos_xyz_fog_x.w = unity_FogParams.x * clipZ_0Far;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
               
                float3 viewDir = normalize(float3(i.wPos_xyz_fog_x.xyz - _WorldSpaceCameraPos));
                half4 col = SAMPLE_TEXTURECUBE_LOD(_SkyTex, sampler_SkyTex, viewDir, 0);
                col *= _SkyColor;

                col.rgb = MixFog(col.rgb, viewDir, i.wPos_xyz_fog_x.w);

                col = Volumetrics(col, i.wPos_xyz_fog_x.xyz);

                return col;
            }
            ENDHLSL
        }
    }
}
