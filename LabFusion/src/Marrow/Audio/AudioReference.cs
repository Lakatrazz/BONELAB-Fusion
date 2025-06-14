using Il2CppSLZ.Marrow.Warehouse;

using UnityEngine;

namespace LabFusion.Marrow;

public struct AudioReference
{
    public MonoDiscReference MonoDisc { get; set; }
    public AudioClip ClipOverride { get; set; }

    public AudioReference(MonoDiscReference monoDisc)
    {
        MonoDisc = monoDisc;
        ClipOverride = null;
    }

    public AudioReference(AudioClip clipOverride)
    {
        ClipOverride = clipOverride;
        MonoDisc = null;
    }

    public readonly bool HasClip()
    {
        bool hasMonoDisc = MonoDisc != null && MonoDisc.IsValid();
        bool hasAudioClip = ClipOverride != null;

        return hasMonoDisc || hasAudioClip;
    }

    public readonly void LoadClip(Action<AudioClip> loadCallback)
    {
        if (ClipOverride != null)
        {
            loadCallback?.Invoke(ClipOverride);
            return;
        }

        AudioLoader.LoadMonoDisc(MonoDisc, loadCallback);
    }

    public static AudioReference[] CreateReferences(MonoDiscReference[] discs)
    {
        var references = new AudioReference[discs.Length];
        for (var i = 0; i < discs.Length; i++)
        {
            references[i] = new AudioReference(discs[i]);
        }
        return references;
    }

    public static AudioReference[] CreateReferences(AudioClip[] clips)
    {
        var references = new AudioReference[clips.Length];
        for (var i = 0; i < clips.Length; i++)
        {
            references[i] = new AudioReference(clips[i]);
        }
        return references;
    }
}