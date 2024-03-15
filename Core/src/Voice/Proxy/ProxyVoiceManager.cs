using LabFusion.Representation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Voice;

public sealed class ProxyVoiceManager : VoiceManager
{
    public override bool CanTalk => false;

    protected override IVoiceReceiver OnCreateReceiverOrDefault()
    {
        return null;
    }

    protected override IVoiceSpeaker OnCreateSpeaker(PlayerId id)
    {
        return new ProxyVoiceSpeaker(id);
    }
}