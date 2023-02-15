Shader "SLZ/Particle/Motion Vector Billboard Correct Shadows"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		 _EmissionColor("Emission Color", Color) = (0,0,0,0)
		[NoScaleOffset]_MainTex("Main Texture", 2D) = "white" {}
		[NoScaleOffset]_EmissionMap("Emission Texture", 2D) = "white" {}

		[NoScaleOffset]_MotionVectors("Motion Vectors", 2D) = "white" {}
		_UVMotionMultiplier("UV Motion Multiplier", Float) = 0
		_Color("Color", Color) = (1,1,1,1)
		[Toggle(_BRDFMAP)] BRDFMAP("Enable BRDF map", Float) = 0
		[NoScaleOffset]g_tBRDFMap("BRDF map", 2D) = "white" {}
		_Depth("Depth", Float) = 0
		[ASEEnd][Toggle(_SCALEDEPTHDITHER_ON)] _ScaleDepthDither("Scale Depth Dither", Float) = 0
		[HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

		
	}

	SubShader
	{
		LOD 0
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
		Cull Back
		ZWrite Off
		ZTest LEqual
		Offset 0 , 0
		AlphaToMask Off
		
		HLSLINCLUDE
		#pragma target 3.0

		#pragma prefer_hlslcc gles
		#pragma exclude_renderers d3d11_9x 
		#pragma multi_compile
		
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ColorMask RGBA
			

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			//#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ _VOLUMETRICS_ENABLED
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define ASE_SRP_VERSION 120102


			// #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
			#define _DISABLE_LIGHTMAPS
			#define _DISABLE_REFLECTIONPROBES
			#define _DISABLE_SSAO
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DefaultLitVariants.hlsl"

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_FORWARD

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Include/Particle/billboard.hlsl"
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_COLOR
			#define ASE_NEEDS_FRAG_SCREEN_POSITION
			#pragma shader_feature_local _SCALEDEPTHDITHER_ON
			#pragma shader_feature_local_fragment _BRDFMAP

			#ifndef SHADER_API_MOBILE
			#define ASE_DEPTH_WRITE_ON
			#endif

			struct VertexInput
			{
				float4 vertex : POSITION;
				half3 normal : NORMAL;
				half4 tangent : TANGENT;
				float4 texcoord : TEXCOORD0; //uv, uv2
				float4 texcoord1 : TEXCOORD1; //anim blend, anim frame, center.xy
				float texcoord2 : TEXCOORD2; //center.z
				half4 vColor : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				half4 vColor : COLOR;
				half4 uv01 : TEXCOORD0;
				half3 SH : TEXCOORD1;
				half4 fog_x_vLight_rgb : TEXCOORD2;
				half4 wNormal_xyz_animBlend_x : TEXCOORD3;
				float3 wPos : TEXCOORD4;
				float4 pCenter_xyz_animFrame_x : TEXCOORD5;

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord : TEXCOORD6;
				#endif
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 screenPos : TEXCOORD7;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
				float2 dynamicLightmapUV : TEXCOORD8;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Color;
			float4 _EmissionColor;
			float _UVMotionMultiplier;
			float _Depth;

			CBUFFER_END
			Texture2D _MainTex; SamplerState sampler_MainTex;
			Texture2D _EmissionMap; SamplerState sampler_EmissionMap;
			Texture2D _MotionVectors; SamplerState sampler_MotionVectors;
			TEXTURE2D_ARRAY(_SLZ_DitherTex2D);
			int NoisePixels;
			int _SLZ_TexSel;
			int NoiseArraySize;
			SAMPLER(sampler_SLZ_DitherTex2D);


			inline float4 ASE_ComputeGrabScreenPos( float4 pos )
			{
				#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
				#else
				float scale = 1.0;
				#endif
				float4 o = pos;
				o.y = pos.w * 0.5f;
				o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
				return o;
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				
				float3 particleCenter = float3(v.texcoord1.z , v.texcoord1.w , v.texcoord2.x);
				v.vertex.xyz = particle_face_camera(v.vertex.xyz , v.normal, particleCenter);
				
				o.vColor = v.vColor;
				o.uv01 = v.texcoord;
				

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				o.wPos = positionWS;

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.normal, v.tangent );

				o.wNormal_xyz_animBlend_x = half4(normalInput.normalWS.xyz, v.texcoord1.x);
				o.pCenter_xyz_animFrame_x = float4(v.texcoord1.zw, v.texcoord2.x, v.texcoord1.y);

				#if defined(DYNAMICLIGHTMAP_ON)
				o.dynamicLightmapUV.xy = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif

				#if !defined(LIGHTMAP_ON)
				OUTPUT_SH( normalInput.normalWS.xyz, o.SH.xyz );
				#endif

				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );

				#ifdef ASE_FOG
					half fogFactor = ComputeFogFactor( positionCS.z );
				#else
					half fogFactor = 0;
				#endif

				o.fog_x_vLight_rgb = half4(fogFactor, vertexLight);
				
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				
				o.clipPos = positionCS;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				o.screenPos = ComputeScreenPos(positionCS);
				#endif
				return o;
			}
			

			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}


			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif

			half4 frag ( VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				
				half3 WorldNormal = normalize( IN.wNormal_xyz_animBlend_x.xyz);

				float3 WorldPosition = IN.wPos;
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
					float4 ScreenPos = IN.screenPos;
				#endif


	
				WorldViewDirection = SafeNormalize( WorldViewDirection );

				

				half animBlend = IN.wNormal_xyz_animBlend_x.w;
				float animFrame = IN.pCenter_xyz_animFrame_x.w;

				float2 motionVectorOffset1 = (SAMPLE_TEXTURE2D( _MotionVectors, sampler_MotionVectors, IN.uv01.xy).rg - 0.5 ) * animBlend * -_UVMotionMultiplier;
				float2 motionVectorOffset2 = (SAMPLE_TEXTURE2D( _MotionVectors, sampler_MotionVectors, IN.uv01.zw).rg - 0.5 ) * ( 1.0 - animBlend ) * _UVMotionMultiplier;

				float4 albedo = lerp( SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, IN.uv01.xy +  motionVectorOffset1),
									  SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, IN.uv01.zw + motionVectorOffset2), animBlend);
				albedo = IN.vColor * albedo * _Color;

				float4 emission = lerp( SAMPLE_TEXTURE2D( _EmissionMap, sampler_EmissionMap, IN.uv01.xy +  motionVectorOffset1),
									  SAMPLE_TEXTURE2D( _EmissionMap, sampler_EmissionMap, IN.uv01.zw + motionVectorOffset2), animBlend);

			
				float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ScreenPos );
				float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
				
				
				#ifdef _SCALEDEPTHDITHER_ON
				float staticSwitch72 = ( _Depth * IN.uv01.z );
				#else
				float staticSwitch72 = _Depth;
				#endif
				float2 depthDitherUV = ase_grabScreenPosNorm.xy * _ScreenParams.xy / NoisePixels;
				float depthDitherIndex = fmod((float)_SLZ_TexSel + floor((float)NoiseArraySize * animFrame)  ,  (float)NoiseArraySize);
				float clipOffset = (SAMPLE_TEXTURE2D_ARRAY( _SLZ_DitherTex2D, sampler_SLZ_DitherTex2D, depthDitherUV ,  depthDitherIndex).r - 0.5) * staticSwitch72;
				float offsetClipW = ( IN.clipPos.w + clipOffset);
				
				float3 Albedo = albedo.rgb;
				float3 Normal = float3(0, 0, 1);
				float3 Emission = (emission*_EmissionColor).rgb;
#if 0
				float3 BakedEmission = 0;
#endif
				float3 Specular = 0.5;
				float Metallic = 0;
				float Smoothness = 0.5;
				float Occlusion = 1;
				float Alpha = albedo.a;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;
				//#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = ( ( 1.0 - ( offsetClipW * _ZBufferParams.w ) ) / ( offsetClipW * _ZBufferParams.z ) );
				//#endif
				
				#ifdef _CLEARCOAT
				float CoatMask = 0;
				float CoatSmoothness = 0;
				#endif


				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif


				InputData inputData = (InputData)0;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;
				
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					inputData.shadowCoord = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					float depthZ = Linear01Depth(DepthValue, _ZBufferParams) * (_ProjectionParams.z - _ProjectionParams.y) + _ProjectionParams.y;
					float3 shadowPos = WorldViewDirection * (depthZ / -(dot(WorldViewDirection, UNITY_MATRIX_V._m20_m21_m22))) + _WorldSpaceCameraPos;
					inputData.shadowCoord = TransformWorldToShadowCoord(shadowPos);
				#else
					inputData.shadowCoord = float4(0, 0, 0, 0);
				#endif

				#ifdef _NORMALMAP
					#if _NORMAL_DROPOFF_TS
					inputData.normalWS = TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ));
					#elif _NORMAL_DROPOFF_OS
					inputData.normalWS = TransformObjectToWorldNormal(Normal);
					#elif _NORMAL_DROPOFF_WS
					inputData.normalWS = Normal;
					#endif
					inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				#else
					inputData.normalWS = WorldNormal;
				#endif




				#ifdef ASE_FOG
					inputData.fogCoord = IN.fog_x_vLight_rgb.x;
				#endif

				inputData.vertexLighting = IN.fog_x_vLight_rgb.yzw;
				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.SH.xyz;
				#endif

				#if defined(DYNAMICLIGHTMAP_ON)

				#ifdef LIGHTMAP_ON
				float4 encodedGI =  SAMPLE_GI_DIR(IN.SH.xyz, IN.dynamicLightmapUV.xy, IN.vertexSH, inputData.normalWS, Smoothness, WorldViewDirection);
				inputData.bakedGI = encodedGI.rgb;
				float BakedSpecular = encodedGI.w;

				#else
				inputData.bakedGI = SAMPLE_GI(IN.SH.xy, IN.dynamicLightmapUV.xy, IN.vertexSH , inputData.normalWS);
				#endif

				#else
				#ifdef LIGHTMAP_ON
				float4 encodedGI = SAMPLE_GI_DIR(IN.SH.xy, IN.lightmapUVOrVertexSH.xyz, inputData.normalWS, Smoothness, WorldViewDirection);
				inputData.bakedGI = encodedGI.rgb;
				float BakedSpecular = encodedGI.w;

				#else
				inputData.bakedGI = SAMPLE_GI(IN.SH.xy, IN.SH.xyz, inputData.normalWS);
				#endif				
				#endif



				#ifdef _ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif
				
				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.clipPos);
				inputData.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUVOrVertexSH.xy);

				#if defined(DEBUG_DISPLAY)
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.dynamicLightmapUV = IN.dynamicLightmapUV.xy;
					#endif

					#if defined(LIGHTMAP_ON)
						inputData.staticLightmapUV = IN.lightmapUVOrVertexSH.xy;
					#else
						inputData.vertexSH = SH;
					#endif
				#endif

				SurfaceData surfaceData;
				surfaceData.albedo              = Albedo;
				surfaceData.metallic            = saturate(Metallic);
				surfaceData.specular            = Specular;
				surfaceData.smoothness          = saturate(Smoothness),
				surfaceData.occlusion           = Occlusion,
				surfaceData.emission            = Emission,
				surfaceData.alpha               = saturate(Alpha);
				surfaceData.normalTS            = Normal;
				surfaceData.clearCoatMask       = 0;
				surfaceData.clearCoatSmoothness = 1;


				#ifdef _CLEARCOAT
					surfaceData.clearCoatMask       = saturate(CoatMask);
					surfaceData.clearCoatSmoothness = saturate(CoatSmoothness);
				#endif

				#ifdef _DBUFFER
					ApplyDecalToSurfaceData(IN.clipPos, surfaceData, inputData);
				#endif

				half4 color = UniversalFragmentPBR( inputData, surfaceData);

				#ifdef LIGHTMAP_ON
				float3 MetalSpec = lerp(kDieletricSpec.rgb, surfaceData.albedo , surfaceData.metallic);
				color.rgb += BakedSpecular * surfaceData.occlusion * MetalSpec * inputData.bakedGI.rgb;
				#endif

				#ifdef _TRANSMISSION_ASE
				{
					float shadow = _TransmissionShadow;

					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );
					half3 mainTransmission = max(0 , -dot(inputData.normalWS, mainLight.direction)) * mainAtten * Transmission;
					color.rgb += Albedo * mainTransmission;

					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );

							half3 transmission = max(0 , -dot(inputData.normalWS, light.direction)) * atten * Transmission;
							color.rgb += Albedo * transmission;
						}
					#endif
				}
				#endif

				#ifdef _TRANSLUCENCY_ASE
				{
					float shadow = _TransShadow;
					float normal = _TransNormal;
					float scattering = _TransScattering;
					float direct = _TransDirect;
					float ambient = _TransAmbient;
					float strength = _TransStrength;

					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );

					half3 mainLightDir = mainLight.direction + inputData.normalWS * normal;
					half mainVdotL = pow( saturate( dot( inputData.viewDirectionWS, -mainLightDir ) ), scattering );
					half3 mainTranslucency = mainAtten * ( mainVdotL * direct + inputData.bakedGI * ambient ) * Translucency;
					color.rgb += Albedo * mainTranslucency * strength;

					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );

							half3 lightDir = light.direction + inputData.normalWS * normal;
							half VdotL = pow( saturate( dot( inputData.viewDirectionWS, -lightDir ) ), scattering );
							half3 translucency = atten * ( VdotL * direct + inputData.bakedGI * ambient ) * Translucency;
							color.rgb += Albedo * translucency * strength;
						}
					#endif
				}
				#endif

				#ifdef _REFRACTION_ASE
					float4 projScreenPos = ScreenPos / ScreenPos.w;
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, float4( WorldNormal,0 ) ).xyz * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos.xy ) * RefractionColor;
					color.rgb = lerp( refraction, color.rgb, color.a );
					color.a = 1;
				#endif

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fog_x_vLight_rgb.x );
					#else
						color.rgb = MixFog(color.rgb, -inputData.viewDirectionWS, IN.fog_x_vLight_rgb.x);
					#endif
				#endif

				    color = Volumetrics( color, inputData.positionWS);


				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				return color;
			}

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
			
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ _VOLUMETRICS_ENABLED
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define ASE_SRP_VERSION 120102

			
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW

			#define SHADERPASS SHADERPASS_SHADOWCASTER

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Include/Particle/billboard.hlsl"
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#pragma shader_feature_local _SCALEDEPTHDITHER_ON

			#ifndef SHADER_API_MOBILE
			#define ASE_DEPTH_WRITE_ON
			#endif


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_color : COLOR;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Color;
			float _UVMotionMultiplier;
			float _Depth;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _MainTex;
			sampler2D _MotionVectors;
			TEXTURE2D_ARRAY(_SLZ_DitherTex2D);
			int NoisePixels;
			int _SLZ_TexSel;
			int NoiseArraySize;
			SAMPLER(sampler_SLZ_DitherTex2D);


			inline float4 ASE_ComputeGrabScreenPos( float4 pos )
			{
				#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
				#else
				float scale = 1.0;
				#endif
				float4 o = pos;
				o.y = pos.w * 0.5f;
				o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
				return o;
			}
			

			float3 _LightDirection;
			float3 _LightPosition;

			VertexOutput VertexFunction( VertexInput v )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float4 vertex84 = float4( v.vertex.xyz , 0.0 );
				float3 normal84 = v.ase_normal;
				float3 tangent84 = float3( 0,0,0 );
				float3 appendResult88 = (float3(v.ase_texcoord1.z , v.ase_texcoord1.w , v.ase_texcoord2.x));
				float3 center84 = appendResult88;
				float3 localparticle_face_camera84 = particle_face_camera( vertex84 , normal84, center84 );
				
				float4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord5 = screenPos;
				
				o.ase_color = v.ase_color;
				o.ase_texcoord2 = v.ase_texcoord;
				o.ase_texcoord3 = v.ase_texcoord1;
				o.ase_texcoord4 = v.vertex;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = localparticle_face_camera84;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = normal84;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif
				float3 normalWS = TransformObjectToWorldDir(v.ase_normal);


			#if _CASTING_PUNCTUAL_LIGHT_SHADOW
				float3 lightDirectionWS = normalize(_LightPosition - positionWS);
			#else
				float3 lightDirectionWS = _LightDirection;
			#endif

				float4 clipPos = ApplySLZShadowBias(positionWS, normalWS, lightDirectionWS);
			
			#if UNITY_REVERSED_Z
				clipPos.z = min(clipPos.z, UNITY_NEAR_CLIP_VALUE);
			#else
				clipPos.z = max(clipPos.z, UNITY_NEAR_CLIP_VALUE);
			#endif


				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = clipPos;
				return o;
			}

			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif

			half4 frag(	VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );
				
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 temp_output_8_0 = (IN.ase_texcoord2).xy;
				float4 temp_cast_1 = (0.5).xxxx;
				float2 temp_output_9_0 = (IN.ase_texcoord2).zw;
				float4 temp_cast_4 = (0.5).xxxx;
				float4 lerpResult4 = lerp( tex2D( _MainTex, ( float4( temp_output_8_0, 0.0 , 0.0 ) + ( ( tex2D( _MotionVectors, temp_output_8_0 ) - temp_cast_1 ) * IN.ase_texcoord3.xyz.x * -_UVMotionMultiplier ) ).rg ) , tex2D( _MainTex, ( float4( temp_output_9_0, 0.0 , 0.0 ) + ( ( tex2D( _MotionVectors, temp_output_9_0 ) - temp_cast_4 ) * ( 1.0 - IN.ase_texcoord3.xyz.x ) * -1.0 * -_UVMotionMultiplier ) ).rg ) , IN.ase_texcoord3.xyz.x);
				float4 temp_output_11_0 = ( IN.ase_color * lerpResult4 * _Color );
				
				float4 unityObjectToClipPos92 = TransformWorldToHClip(TransformObjectToWorld(IN.ase_texcoord4.xyz));
				float4 screenPos = IN.ase_texcoord5;
				float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( screenPos );
				float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
				float4 temp_cast_8 = (0.5).xxxx;
				#ifdef _SCALEDEPTHDITHER_ON
				float staticSwitch72 = ( _Depth * IN.ase_texcoord3.xyz.z );
				#else
				float staticSwitch72 = _Depth;
				#endif
				float temp_output_4_0_g44 = ( unityObjectToClipPos92.w + ( ( SAMPLE_TEXTURE2D_ARRAY( _SLZ_DitherTex2D, sampler_SLZ_DitherTex2D, ( (ase_grabScreenPosNorm).xy * ( (_ScreenParams).xy / NoisePixels ) ),(float)( ( _SLZ_TexSel + (int)( NoiseArraySize * IN.ase_texcoord3.xyz.y ) ) % NoiseArraySize ) ) - temp_cast_8 ) * staticSwitch72 ).r );
				
				float Alpha = (temp_output_11_0).a;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = ( ( 1.0 - ( temp_output_4_0_g44 * _ZBufferParams.w ) ) / ( temp_output_4_0_g44 * _ZBufferParams.z ) );
				#endif

				#ifdef _ALPHATEST_ON
					#ifdef _ALPHATEST_SHADOW_ON
						clip(Alpha - AlphaClipThresholdShadow);
					#else
						clip(Alpha - AlphaClipThreshold);
					#endif
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif
				return 0;
			}

			ENDHLSL
		}


	}
	
	CustomEditor "UnityEditor.ShaderGraphLitGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
