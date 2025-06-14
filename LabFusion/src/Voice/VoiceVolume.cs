using Il2CppSLZ.Marrow.Audio;

using LabFusion.Preferences.Client;

namespace LabFusion.Voice;

public static class VoiceVolume
{
    public const float DefaultSampleMultiplier = 10f;

    public const float MinimumVoiceVolume = 0.3f;

    public const float SilencingVolume = 0.1f;

    public const float TalkTimeoutTime = 1f;

    public static float GetVolumeMultiplier()
    {
        float globalVolumePercent = Audio3dManager.audio_GlobalVolume * 0.1f;
        float sfxVolumePercent = Audio3dManager.audio_SFXVolume * 0.1f;
        float voiceVolumePercent = ClientSettings.VoiceChat.GlobalVolume.Value;

        float volumePercent = globalVolumePercent * sfxVolumePercent * voiceVolumePercent;

        return volumePercent;
    }
}