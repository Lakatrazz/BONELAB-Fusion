using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Senders;

namespace LabFusion.Voice;

public interface IVoiceManager
{
    List<IVoiceSpeaker> VoiceSpeakers { get; }

    bool CanTalk { get; }
    bool CanHear { get; }

    IVoiceSpeaker GetSpeaker(PlayerId id);
    void RemoveSpeaker(PlayerId id);

    IVoiceReceiver GetReceiver();

    void Enable();
    void Disable();

    void UpdateManager();
    void ClearManager();
}

public abstract class VoiceManager : IVoiceManager
{
    protected List<IVoiceSpeaker> _voiceSpeakers = new();
    public List<IVoiceSpeaker> VoiceSpeakers => _voiceSpeakers;

    public virtual bool CanTalk => true;
    public virtual bool CanHear => true;

    private IVoiceReceiver _receiver = null;

    public void Enable()
    {
        _receiver = OnCreateReceiverOrDefault();

        _receiver?.Enable();
    }

    public void Disable()
    {
        if (_receiver != null)
        {
            _receiver.Disable();

            _receiver = null;
        } 

        ClearManager();
    }

    protected bool TryGetSpeaker(PlayerId id, out IVoiceSpeaker speaker)
    {
        speaker = null;

        for (var i = 0; i < VoiceSpeakers.Count; i++)
        {
            var result = VoiceSpeakers[i];

            if (result.ID == id)
            {
                speaker = result;
                return true;
            }
        }

        return false;
    }

    protected abstract IVoiceSpeaker OnCreateSpeaker(PlayerId id);

    protected abstract IVoiceReceiver OnCreateReceiverOrDefault();

    public IVoiceSpeaker GetSpeaker(PlayerId id)
    {
        if (TryGetSpeaker(id, out var handler))
            return handler;

        var newSpeaker = OnCreateSpeaker(id);
        VoiceSpeakers.Add(newSpeaker);

        return newSpeaker;
    }

    public IVoiceReceiver GetReceiver()
    {
        return _receiver;
    }

    public void UpdateManager()
    {
        UpdateSpeakers();
        UpdateReceiver();
    }

    private void UpdateSpeakers()
    {
        for (var i = 0; i < VoiceSpeakers.Count; i++)
        {
            VoiceSpeakers[i].Update();
        }
    }

    private bool _hasDisabledVoice = true;

    private void UpdateReceiver()
    {
        if (_receiver == null || !CanTalk)
        {
            return;
        }

        bool voiceEnabled = NetworkInfo.HasServer && VoiceInfo.IsVoiceEnabled;

        // Only disable voice when state changes to not conflict with other uses of the microphone
        if (voiceEnabled)
        {
            _receiver.UpdateVoice(true);
            _hasDisabledVoice = false;
        }
        else if (!_hasDisabledVoice)
        {
            _receiver.UpdateVoice(false);
            _hasDisabledVoice = true;
        }

        if (_receiver.HasVoiceActivity())
        {
            PlayerSender.SendPlayerVoiceChat(_receiver.GetCompressedVoiceData());
        }
    }

    public void RemoveSpeaker(PlayerId id)
    {
        IVoiceSpeaker playerHandler = null;

        foreach (var handler in VoiceSpeakers)
        {
            if (handler.ID == id)
            {
                playerHandler = handler;
                break;
            }
        }

        if (playerHandler != null)
        {
            playerHandler.Cleanup();
            _voiceSpeakers.Remove(playerHandler);
        }
    }

    public void ClearManager()
    {
        foreach (var handler in VoiceSpeakers)
        {
            handler.Cleanup();
        }

        _voiceSpeakers.Clear();
    }
}