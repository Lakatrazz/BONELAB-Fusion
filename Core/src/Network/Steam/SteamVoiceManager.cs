using LabFusion.Representation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{
    public sealed class SteamVoiceManager : VoiceManager {
        public override VoiceHandler GetVoiceHandler(PlayerId id) {
            for (var i = 0; i < VoiceHandlers.Count; i++) {
                var handler = VoiceHandlers[i];

                if (handler.ID == id)
                    return handler;
            }

            var newIdentifier = new SteamVoiceHandler(id);
            VoiceHandlers.Add(newIdentifier);
            return newIdentifier;
        }
    }
}
