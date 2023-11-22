using LabFusion.Representation;

namespace LabFusion.Network
{
    public sealed class SteamVoiceManager : VoiceManager
    {
        public override VoiceHandler GetVoiceHandler(PlayerId id)
        {
            if (TryGetHandler(id, out var handler))
                return handler;

            var newIdentifier = new SteamVoiceHandler(id);
            VoiceHandlers.Add(newIdentifier);
            return newIdentifier;
        }
    }
}
