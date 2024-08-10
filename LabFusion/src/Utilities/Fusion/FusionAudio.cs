using Il2CppSLZ.Marrow.Audio;
using UnityEngine;

namespace LabFusion.Utilities
{
    public static class FusionAudio
    {
        public static AudioSource Play2D(AudioClip clip, float volume = 1f)
        {
            GameObject go = new("Fusion 2D Audio Source");
            var source = go.AddComponent<AudioSource>();
            source.volume = volume;
            source.clip = clip;
            source.spatialBlend = 0f;

            source.outputAudioMixerGroup = Audio3dManager.inHead;

            source.Play();

            return source;
        }

        public static AudioSource Play3D(Vector3 position, AudioClip clip, float volume = 1f, bool loop = false)
        {
            GameObject go = new("Fusion 3D Audio Source");
            var source = go.AddComponent<AudioSource>();
            go.transform.position = position;

            source.volume = volume;
            source.clip = clip;
            source.spatialBlend = 1f;
            source.loop = loop;

            source.outputAudioMixerGroup = Audio3dManager.hardInteraction;

            source.Play();

            return source;
        }
    }
}
