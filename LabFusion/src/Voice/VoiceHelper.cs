using LabFusion.Representation;

namespace LabFusion.Voice;

public static class VoiceHelper
{
    private static readonly JawFlapper _localJaw = new();

    public static JawFlapper LocalJaw => _localJaw;

    public static void OnVoiceChatUpdate()
    {
        VoiceInfo.VoiceManager?.UpdateManager();

        _localJaw.UpdateJaw(VoiceInfo.VoiceAmplitude);
    }

    public static void OnVoiceDataReceived(PlayerId player, byte[] data)
    {
        if (VoiceInfo.IsDeafened)
        {
            return;
        }

        var speaker = VoiceInfo.VoiceManager?.GetSpeaker(player);
        speaker?.OnVoiceDataReceived(data);
    }
}