using LabFusion.Representation;

namespace LabFusion.Voice;

public sealed class SteamVoiceManager : VoiceManager
{
    protected override IVoiceReceiver OnCreateReceiverOrDefault()
    {
        return new SteamVoiceReceiver();
    }

    protected override IVoiceSpeaker OnCreateSpeaker(PlayerId id)
    {
        return new SteamVoiceSpeaker(id);
    }
}
