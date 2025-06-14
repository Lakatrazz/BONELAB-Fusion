using LabFusion.Player;

namespace LabFusion.Voice;

public interface IVoiceSpeaker
{
    PlayerID ID { get; }

    float Volume { get; set; }
    float Amplitude { get; set; }

    void OnVoiceDataReceived(byte[] data);

    void Update();
    void Cleanup();
}

public abstract class VoiceSpeaker : IVoiceSpeaker
{
    protected PlayerID _id;
    public PlayerID ID { get { return _id; } }

    protected float _volume = 1f;
    public float Volume
    {
        get
        {
            return _volume;
        }
        set
        {
            _volume = value;
        }
    }

    public abstract float Amplitude { get; set; }

    public virtual void Update() { }

    public abstract void OnVoiceDataReceived(byte[] data);

    public abstract void Cleanup();
}