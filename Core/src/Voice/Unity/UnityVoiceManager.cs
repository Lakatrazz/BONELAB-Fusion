using LabFusion.Representation;

using UnityEngine;

namespace LabFusion.Voice.Unity;

public sealed class UnityVoiceManager : VoiceManager
{
    public override bool CanTalk 
    {
        get
        {
            // If there are no microphones, or the device has been patched without adding microphone permissions, we cannot talk
            if (Microphone.devices.Count <= 0)
            {
                return false;
            }

            return true;
        }
    }

    protected override IVoiceReceiver OnCreateReceiverOrDefault()
    {
        return new UnityVoiceReceiver();
    }

    protected override IVoiceSpeaker OnCreateSpeaker(PlayerId id)
    {
        return new UnityVoiceSpeaker(id);
    }
}
