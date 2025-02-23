using Il2CppSLZ.Marrow.Audio;

using UnityEngine;
using UnityEngine.Audio;

namespace LabFusion.Marrow;

public static class SafeAudio3dPlayer
{
    public static AudioMixerGroup NonDiegeticMusic => Audio3dManager.nonDiegeticMusic;

    public static float MusicVolume => 0.2f;

    public static void Play2dOneShot(AudioClip clip, AudioMixerGroup mixer, float volume = 1f, float pitch = 1f)
    {
        Audio3dManager.Play2dOneShot(clip, mixer, new Il2CppSystem.Nullable<float>(volume), new Il2CppSystem.Nullable<float>(pitch));
    }

    public static void PlayAtPoint(AudioClip[] clips, Vector3 position, AudioMixerGroup mixer, float volume = 1f, float pitch = 1f, float minDistance = 1f)
    {
        Audio3dManager.PlayAtPoint(clips, position, mixer, volume, pitch, new(0f), new(minDistance), new(1f));
    }

    public static void PlayAtPoint(AudioClip clip, Vector3 position, AudioMixerGroup mixer, float volume = 1f, float pitch = 1f, float minDistance = 1f)
    {
        Audio3dManager.PlayAtPoint(clip, position, mixer, volume, pitch, new(0f), new(minDistance), new(1f));
    }
}