namespace LabFusion.Voice;

public interface IVoiceReceiver
{
    float GetVoiceAmplitude();

    bool HasVoiceActivity();

    byte[] GetEncodedData();

    void UpdateVoice(bool enabled);

    void Enable();

    void Disable();
}