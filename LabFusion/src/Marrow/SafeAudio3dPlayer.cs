using BoneLib.Nullables;

using Il2CppSLZ.Marrow.Audio;

using UnityEngine;
using UnityEngine.Audio;

namespace LabFusion.Marrow;

public static class SafeAudio3dPlayer
{
    public static void PlayAtPoint(AudioClip[] clips, Vector3 position, AudioMixerGroup mixer, float volume = 1f, float pitch = 1f)
    {
        var nullFloat = new BoxedNullable<float>(null);
        Audio3dManager.PlayAtPoint(clips, position, mixer, volume, pitch, nullFloat, nullFloat, nullFloat);
    }
}