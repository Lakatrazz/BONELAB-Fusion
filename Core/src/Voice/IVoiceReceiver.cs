using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Voice;

public interface IVoiceReceiver
{
    float GetVoiceAmplitude();

    bool HasVoiceActivity();

    byte[] GetCompressedVoiceData();

    void UpdateVoice(bool enabled);

    void Enable();

    void Disable();
}