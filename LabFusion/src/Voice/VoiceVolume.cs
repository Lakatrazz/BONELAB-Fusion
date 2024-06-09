using LabFusion.Preferences;
using LabFusion.Utilities;

namespace LabFusion.Voice;

public static class VoiceVolume
{
    public const float DefaultSampleMultiplier = 10f;

    public const float MinimumVoiceVolume = 0.3f;

    public const float SilencingVolume = 0.1f;

    public static float GetGlobalVolumeMultiplier()
    {
        float volume = FusionPreferences.ClientSettings.GlobalVolume;

        // If we are loading, lower the volume
        if (FusionSceneManager.IsLoading())
        {
            volume *= 0.25f;
        }

        return volume;
    }
}