using LabFusion.Player;

using UnityEngine;

namespace LabFusion.Voice.Unity;

public sealed class UnityVoiceManager : VoiceManager
{
    public override bool CanTalk 
    {
        get
        {
            // If there are no microphones, or the device has been patched without adding microphone permissions, we cannot talk
            if (!UnityVoice.IsSupported())
            {
                return false;
            }

            return true;
        }
    }

    public override string[] InputDevices
    {
        get
        {
            if (!UnityVoice.IsSupported())
            {
                return Array.Empty<string>();
            }

            return Microphone.devices;
        }
    }

    protected override IVoiceReceiver OnCreateReceiverOrDefault()
    {
        return new UnityVoiceReceiver();
    }

    protected override IVoiceSpeaker OnCreateSpeaker(PlayerID id)
    {
        return new UnityVoiceSpeaker(id);
    }
}
