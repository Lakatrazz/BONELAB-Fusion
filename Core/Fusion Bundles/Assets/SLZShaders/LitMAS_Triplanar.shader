Shader "SLZ/LitMAS/LitMAS Triplanar"
{
    Properties
    {
        [Toggle(_EXPENSIVE_TP)] _Expensive("Fix Derivative Seams (expensive)", float) = 0
        [NoScaleOffset][MainTexture] _BaseMap("Texture", 2D) = "white" {}
        _UVScaler("Texture Scale", Float) = 1
        [MainColor] _BaseColor("BaseColor", Color) = (1,1,1,1)
            
        [ToggleUI] _RotateUVs("Rotate UVs", Float) = 0
        [ToggleUI] _Normals("Normal Map enabled", Float) = 1
        [NoScaleOffset][Normal] _BumpMap ("Normal map", 2D) = "bump" {}
        [NoScaleOffset]_MetallicGlossMap("MAS", 2D) = "white" {}
        [Space(30)][Header(Emissions)][Space(10)][ToggleUI] _Emission("Emission Enable", Float) = 0
        [NoScaleOffset]_EmissionMap("Emission Map", 2D) = "white" {}
        [HDR]_EmissionColor("Emission Color", Color) = (1,1,1,1)
        _EmissionFalloff("Emission Falloff", Float) = 1
        _BakedMutiplier("Emission Baked Mutiplier", Float) = 1
        [Space(30)][Header(Details)][Space(10)][Toggle(_DETAILS_ON)] _Details("Details enabled", Float) = 0
            [ToggleUI] _DetailsuseLocalUVs("Details use Local UVs", Float) = 0
        _DetailMap("DetailMap", 2D) = "gray" {}
                [Space(30)][Header(Screen Space Reflections)][Space(10)][Toggle(_NO_SSR)] _SSROff("Disable SSR", Float) = 0
        [Header(This should be 0 for skinned meshes)]
        _SSRTemporalMul("Temporal Accumulation Factor", Range(0, 2)) = 1.0
      
    }
    SubShader
    {
        Tags {"RenderPipeline" = "UniversalPipeline"  "RenderType" = "Opaque" "Queue" = "Geometry" }
        Blend One Zero
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		ColorMask RGBA
        LOD 100

        Pass
        {
            Name "Forward"
            Tags {"Lightmode"="UniversalForward"}
            HLSLPROGRAM
            //#pragma use_dxc
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #define LITMAS_FEATURE_LIGHTMAPPING
            #define LITMAS_FEATURE_TP
            #define LITMAS_FEATURE_EMISSION
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/PlatformCompiler.hlsl"
            #include_with_pragmas "LitMASInclude/ShaderInjector/TriplanarForward.hlsl"
            ENDHLSL
        }

		Pass
        {
            Name "DepthOnly"
            Tags {"Lightmode"="DepthOnly"}
			ZWrite On
			//ZTest Off
			ColorMask 0

            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/PlatformCompiler.hlsl"
            #include "LitMASInclude/DepthOnly.hlsl" 

            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags {"Lightmode" = "DepthNormals"}
            ZWrite On
            //ZTest Off
            //ColorMask 0

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/PlatformCompiler.hlsl"
            #include "LitMASInclude/ShaderInjector/TriplanarDepthNormals.hlsl" 

            ENDHLSL
        }

 		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual
			AlphaToMask Off
			ColorMask 0

			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
            #pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/PlatformCompiler.hlsl"
            #include "LitMASInclude/ShadowCaster.hlsl" 

			ENDHLSL
		}

        Pass
        {
            Name "Meta"
            Tags { "LightMode" = "Meta" }

            Cull Off

            HLSLPROGRAM

            #define _NORMAL_DROPOFF_TS 1
            #define _EMISSION
            #define _NORMALMAP 1

            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _ EDITOR_VISUALIZATION

            #define SHADERPASS SHADERPASS_META
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/PlatformCompiler.hlsl"
            #include "LitMASInclude/ShaderInjector/TriplanarMeta.hlsl" 
            ENDHLSL
        }

        Pass
		{
			
            Name "BakedRaytrace"
            Tags{ "LightMode" = "BakedRaytrace" }
			HLSLPROGRAM

            #include "LitMASInclude/BakedRayTrace.hlsl"

            ENDHLSL
        }
    }
    CustomEditor "UnityEditor.ShaderGraphLitGUI"
    Fallback "Hidden/InternalErrorShader"
}
