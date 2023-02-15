Shader "SLZ/Mod2x"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		_MainTex("MainTex", 2D) = "gray" {}
		[HDR]_Color("Color", Color) = (1,1,1,0)
		_OffsetUnits("OffsetUnits", Int) = -2
		_OffsetFactor("OffsetFactor", Int) = -2
		_Multiplier("Multiplier", Float) = 1
		[Toggle(_ALPHA_ON)] _alpha("alpha", Float) = 0
		[Toggle(_VERTEXCOLORS_ON)] _VertexColors("VertexColors", Float) = 1
		// [HideInInspector] _texcoord( "", 2D ) = "white" {}

		// [HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        // [HideInInspector]_QueueControl("_QueueControl", Float) = -1
        // [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        // [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        // [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
	}

	SubShader
	{
		LOD 0
		
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent-499" "IgnoreProjector" = "True"}
		
		Cull Back
		AlphaToMask Off
		
		HLSLINCLUDE
		#pragma target 5.0
		#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/PlatformCompiler.hlsl"
		ENDHLSL
		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend DstColor SrcColor
			ZWrite Off
			ZTest LEqual
			Offset [_OffsetFactor] , [_OffsetUnits]
			ColorMask RGBA

			HLSLPROGRAM
			
			#define _RECEIVE_SHADOWS_OFF 1
			#pragma multi_compile_instancing
			#define ASE_SRP_VERSION 999999
			#define SHADERPASS SHADERPASS_UNLIT

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"


			#define ASE_NEEDS_FRAG_COLOR
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#pragma shader_feature _ALPHA_ON
			#pragma shader_feature _VERTEXCOLORS_ON
			#pragma multi_compile _ _VOLUMETRICS_ENABLED
			#pragma multi_compile_fog
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float fogFactor : TEXCOORD2;				
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_color : COLOR;
				float4 ase_texcoord4 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _Color;
			int _OffsetUnits;
			int _OffsetFactor;
			float _Multiplier;
			CBUFFER_END
			sampler2D _MainTex;

			shared float _StaticLightMultiplier;

						
			VertexOutput VertexFunction ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord4 = screenPos;				
				o.ase_texcoord3.xy = v.ase_texcoord.xy;
				o.ase_color = v.ase_color;				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.zw = 0;
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif
				o.fogFactor = ComputeFogFactor( positionCS.z );
				o.clipPos = positionCS;
				return o;
			}

			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}

			half3 Mod2xFog(half3 fragColor, half fogFactor)
			{
				#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
					half fogIntensity = ComputeFogIntensity(fogFactor);
					fragColor = lerp(0.5, fragColor, fogIntensity);
				#endif
				return fragColor;
			}

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float localMyCustomExpression1_g126 = ( 0.0 );
				float2 uv_MainTex = IN.ase_texcoord3.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 appendResult52 = (float4(1.0 , 1.0 , 1.0 , IN.ase_color.a));
				#ifdef _VERTEXCOLORS_ON
				float4 staticSwitch38 = IN.ase_color;
				#else
				float4 staticSwitch38 = appendResult52;
				#endif
				float4 temp_output_16_0 = ( tex2D( _MainTex, uv_MainTex ) * _Color * staticSwitch38 );
				float4 temp_output_26_0 = ( ( ( temp_output_16_0 - .5 ) * _Multiplier ) + 0.5 );
				#ifdef _ALPHA_ON
				float4 lerpResult30 = lerp( .5 , temp_output_26_0 , (temp_output_16_0).a);
				float4 staticSwitch28 = lerpResult30;
				#else
				float4 staticSwitch28 = temp_output_26_0;
				#endif
				float4 color1_g126 = staticSwitch28;
				float localMyCustomExpression24_g126 = ( 0.0 );
				float4 screenPos = IN.ase_texcoord4;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float2 uv24_g126 = (ase_screenPosNorm).xy;		

				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = color1_g126.xyz;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
				#endif

				#if defined(_ALPHAPREMULTIPLY_ON)
				Color *= Alpha;
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				Color.rgb = Mod2xFog( Color, IN.fogFactor );

				#if defined(_VOLUMETRICS_ENABLED) 
				//works fine on the PC but not quest. Using a semi-plausible result otherwise.
					#if !defined(SHADER_API_MOBILE) 
						half3 FroxelColor = GetVolumetricColor(WorldPosition).rgb;
						Color.rgb = Color.rgb - 0.5* (2.0*Color.rgb - 1.0) * FroxelColor / SampleSceneColor(uv24_g126).rgb;
					#else
						half4 FroxelColor = GetVolumetricColor(IN.worldPos);				
						Color.rgb = Color.rgb + (saturate(FroxelColor.rgb)*(0.5-Color.rgb)); //rgb lerp	//x + s(y-x)		
						Color.rgb = lerp(0.5, Color , saturate(FroxelColor.a*FroxelColor.a) );
					#endif
				#endif

				return half4( Color, Alpha );
			}

			ENDHLSL
		}
	}	
	Fallback "Hidden/InternalErrorShader"	
}