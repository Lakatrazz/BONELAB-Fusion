using LabFusion.Entities;
using LabFusion.Representation;

using UnityEngine;

namespace LabFusion.Voice;

public interface IVoiceSpeaker
{
    PlayerId ID { get; }
    NetworkPlayer Player { get; }
    AudioSource Source { get; }

    bool IsDestroyed { get; }

    float Volume { get; set; }

    float GetVoiceAmplitude();

    void CreateAudioSource();
    void VerifyRep();
    void OnVoiceDataReceived(byte[] data);

    void Cleanup();
    void Update();
}

public abstract class VoiceSpeaker : IVoiceSpeaker
{
    protected PlayerId _id;
    public PlayerId ID { get { return _id; } }

    protected NetworkPlayer _player;
    protected bool _hasPlayer;
    public NetworkPlayer Player { get { return _player; } }

    protected AudioSource _source;
    protected GameObject _sourceGo;
    public AudioSource Source { get { return _source; } }

    protected bool _isDestroyed;
    public bool IsDestroyed { get { return _isDestroyed; } }

    protected float _volume = 1f;
    public float Volume { get { return _volume; } set { _volume = value; } }

    public bool MicrophoneDisabled { get { return _hasPlayer && _player.MicrophoneDisabled; } }

    public virtual void CreateAudioSource()
    {
        _sourceGo = new GameObject($"{ID.SmallId} Voice Source");
        _source = _sourceGo.AddComponent<AudioSource>();

        GameObject.DontDestroyOnLoad(_source);
        GameObject.DontDestroyOnLoad(_sourceGo);
        _sourceGo.hideFlags = HideFlags.DontUnloadUnusedAsset;

        _source.rolloffMode = AudioRolloffMode.Linear;
        _source.loop = true;
    }

    public virtual void VerifyRep()
    {
        if (!_hasPlayer && ID != null)
        {
            if (NetworkPlayerManager.TryGetPlayer(ID, out _player))
            {
                _player.InsertVoiceSource(this, Source);
                _hasPlayer = true;
            }
        }
    }

    public virtual void Update() { }

    public virtual void Cleanup()
    {
        // Destroy audio source
        if (_source != null)
        {
            // Get rid of the clip
            if (_source.clip != null)
                GameObject.Destroy(_source.clip);

            GameObject.Destroy(_sourceGo);
        }

        _isDestroyed = true;
    }

    public abstract void OnVoiceDataReceived(byte[] data);

    public abstract float GetVoiceAmplitude();
}