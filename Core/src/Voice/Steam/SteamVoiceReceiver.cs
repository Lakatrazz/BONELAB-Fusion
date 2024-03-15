using LabFusion.Network;
using LabFusion.Senders;

using Steamworks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Voice;

public sealed class SteamVoiceReceiver : IVoiceReceiver
{
    private byte[] _compressedData = null;

    private bool _hasVoiceActivity = false;

    public void Enable()
    {
        
    }

    public void Disable()
    {
        _hasVoiceActivity = false;
        _compressedData = null;

        SteamUser.VoiceRecord = false;
    }

    public byte[] GetCompressedVoiceData()
    {
        return _compressedData;
    }

    public bool HasVoiceActivity()
    {
        return _hasVoiceActivity;
    }

    public void UpdateVoice(bool enabled)
    {
        if (SteamUser.VoiceRecord != enabled)
            SteamUser.VoiceRecord = enabled;

        _hasVoiceActivity = enabled && SteamUser.HasVoiceData;

        if (_hasVoiceActivity)
        {
            _compressedData = SteamUser.ReadVoiceDataBytes();
        }
        else
        {
            _compressedData = null;
        }
    }
}