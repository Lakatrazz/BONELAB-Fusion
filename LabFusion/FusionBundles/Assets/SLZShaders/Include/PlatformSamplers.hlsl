#if defined(SHADER_API_MOBILE) // Quest 2 (XR2) works better with combined texture-samplers apparently

#ifdef TEXTURE2D
#undef TEXTURE2D
#endif
#ifdef SAMPLER
#undef SAMPLER
#endif
#ifdef SAMPLE_TEXTURE2D
#undef SAMPLE_TEXTURE2D
#endif

#define TEXTURE2D(textureName) sampler2D textureName
#define SAMPLER(samplerName)
#define SAMPLE_TEXTURE2D(textureName, samplerName, coord) tex2D(textureName, coord)
#endif