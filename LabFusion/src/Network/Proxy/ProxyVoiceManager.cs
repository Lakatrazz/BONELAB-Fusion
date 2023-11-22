using LabFusion.Representation;

namespace LabFusion.Network
{
    public sealed class ProxyVoiceManager : VoiceManager
    {
        public override bool CanTalk => false;

        public override VoiceHandler GetVoiceHandler(PlayerId id)
        {
            if (TryGetHandler(id, out var handler))
                return handler;

            var newIdentifier = new ProxyVoiceHandler(id);
            VoiceHandlers.Add(newIdentifier);
            return newIdentifier;
        }
    }
}
