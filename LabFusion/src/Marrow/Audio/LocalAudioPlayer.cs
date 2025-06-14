using Il2CppSLZ.Marrow.Audio;

using LabFusion.Extensions;

using UnityEngine;
using UnityEngine.Audio;

namespace LabFusion.Marrow;

public class AudioPlayerSettings
{
    public static readonly AudioPlayerSettings Default = new();

    public AudioMixerGroup Mixer { get; set; } = null;

    public float Volume { get; set; } = 1f;
    public float Pitch { get; set; } = 1f;
    public float MinDistance { get; set; } = 1f;
    public float SpatialBlend { get; set; } = 1f;
}

public static class LocalAudioPlayer
{
    public static AudioPlayerSettings SFXSettings => new()
    {
        Mixer = HardInteraction,
        Volume = 0.4f,
    };

    public static AudioPlayerSettings InHeadSettings => new()
    {
        Mixer = InHead,
    };

    public static AudioPlayerSettings MusicSettings => new()
    {
        Mixer = NonDiegeticMusic,
        Volume = MusicVolume,
    };


    public static AudioMixerGroup NonDiegeticMusic => Audio3dManager.nonDiegeticMusic;

    public static AudioMixerGroup HardInteraction => Audio3dManager.hardInteraction;

    public static AudioMixerGroup SoftInteraction => Audio3dManager.softInteraction;

    public static AudioMixerGroup InHead => Audio3dManager.inHead;

    public const float MusicVolume = 0.2f;

    public static void Play2dOneShot(AudioClip clip, AudioPlayerSettings settings)
    {
        Audio3dManager.Play2dOneShot(clip, settings.Mixer, new Il2CppSystem.Nullable<float>(settings.Volume), new Il2CppSystem.Nullable<float>(settings.Pitch));
    }

    public static void Play2dOneShot(AudioReference reference, AudioPlayerSettings settings)
    {
        if (!reference.HasClip())
        {
            return;
        }

        reference.LoadClip((clip) =>
        {
            Play2dOneShot(clip, settings);
        });
    }

    public static void PlayAtPoint(AudioClip[] clips, Vector3 position, AudioPlayerSettings settings)
    {
        Audio3dManager.PlayAtPoint(clips, position, settings.Mixer, settings.Volume, settings.Pitch, new(0f), new(settings.MinDistance), new(settings.SpatialBlend));
    }

    public static void PlayAtPoint(AudioReference[] references, Vector3 position, AudioPlayerSettings settings)
    {
        if (references == null || references.Length <= 0)
        {
            return;
        }

        PlayAtPoint(references.GetRandom(), position, settings);
    }

    public static void PlayAtPoint(AudioClip clip, Vector3 position, AudioPlayerSettings settings)
    {
        Audio3dManager.PlayAtPoint(clip, position, settings.Mixer, settings.Volume, settings.Pitch, new(0f), new(settings.MinDistance), new(settings.SpatialBlend));
    }

    public static void PlayAtPoint(AudioReference reference, Vector3 position, AudioPlayerSettings settings)
    {
        if (!reference.HasClip())
        {
            return;
        }

        reference.LoadClip((clip) =>
        {
            PlayAtPoint(clip, position, settings);
        });
    }
}