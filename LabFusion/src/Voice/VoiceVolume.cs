using LabFusion.Preferences.Client;

namespace LabFusion.Voice;

public static class VoiceVolume
{
    public const float DefaultSampleMultiplier = 10f;

    public const float MinimumVoiceVolume = 0.3f;

    public const float SilencingVolume = 0.1f;

    public const float TalkTimeoutTime = 1f;

    public static float GetGlobalVolumeMultiplier()
    {
        return ClientSettings.VoiceChat.GlobalVolume.Value;
    }
}