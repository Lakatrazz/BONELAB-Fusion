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
        return MonoDisc != null && MonoDisc.Barcode != Barcode.EMPTY;
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
}