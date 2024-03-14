using LabFusion.Representation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Voice;

public interface IVoiceManager
{
    List<IVoiceSpeaker> VoiceSpeakers { get; }

    bool CanTalk { get; }
    bool CanHear { get; }

    IVoiceSpeaker GetSpeaker(PlayerId id);

    IVoiceReceiver GetReceiver();

    void Update();
    void Remove(PlayerId id);

    void RemoveAll();
}

public abstract class VoiceManager : IVoiceManager
{
    protected List<IVoiceSpeaker> _voiceSpeakers = new();
    public List<IVoiceSpeaker> VoiceSpeakers => _voiceSpeakers;

    public virtual bool CanTalk => true;
    public virtual bool CanHear => true;

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
        return null;
    }

    public void Update()
    {
        for (var i = 0; i < VoiceSpeakers.Count; i++)
        {
            VoiceSpeakers[i].Update();
        }
    }

    public void Remove(PlayerId id)
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

    public void RemoveAll()
    {
        foreach (var handler in VoiceSpeakers)
        {
            handler.Cleanup();
        }

        _voiceSpeakers.Clear();
    }
}