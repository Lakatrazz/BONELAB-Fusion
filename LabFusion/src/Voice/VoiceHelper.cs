using LabFusion.Representation;

namespace LabFusion.Voice;

public static class VoiceHelper
{
    public static void OnVoiceChatUpdate()
    {
        VoiceInfo.VoiceManager?.UpdateManager();
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