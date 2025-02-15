using Il2CppSLZ.Marrow.Audio;

using LabFusion.Preferences.Client;

namespace LabFusion.Voice;

public static class VoiceVolume
{
    public const float DefaultSampleMultiplier = 10f;

    public const float DefaultVolumeMultiplier = 2f;

    public const float MinimumVoiceVolume = 0.3f;

    public const float SilencingVolume = 0.1f;

    public const float TalkTimeoutTime = 1f;

    public static float GetGlobalVolumeMultiplier()
    {
        float audioPercent = Audio3dManager.audio_SFXVolume * 0.1f;

        return ClientSettings.VoiceChat.GlobalVolume.Value * audioPercent * DefaultVolumeMultiplier;
    }
}