using LabFusion.Representation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Voice;

public sealed class SteamVoiceManager : VoiceManager
{
    protected override IVoiceSpeaker OnCreateSpeaker(PlayerId id)
    {
        return new SteamVoiceSpeaker(id);
    }
}
