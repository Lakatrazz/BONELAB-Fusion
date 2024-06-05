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