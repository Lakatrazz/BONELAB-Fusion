using LabFusion.Representation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{
    public interface IVoiceManager {
        public List<VoiceHandler> VoiceHandlers { get; }

        public bool CanTalk { get; }
        public bool CanHear { get; }

        public VoiceHandler GetVoiceHandler(PlayerId id);

        public void Update();
        public void Remove(PlayerId id);

        public void RemoveAll();
    }

    public abstract class VoiceManager : IVoiceManager
    {
        protected List<VoiceHandler> _voiceHandlers = new();
        public List<VoiceHandler> VoiceHandlers => _voiceHandlers;

        public virtual bool CanTalk => true;
        public virtual bool CanHear => true;

        protected bool TryGetHandler(PlayerId id, out VoiceHandler handler) {
            handler = null;

            for (var i = 0; i < VoiceHandlers.Count; i++) {
                var result = VoiceHandlers[i];

                if (result.ID == id) { 
                    handler = result;
                    return true;
                }
            }

            return false;
        }

        public abstract VoiceHandler GetVoiceHandler(PlayerId id);

        public void Update() {
            for (var i = 0; i < VoiceHandlers.Count; i++) {
                VoiceHandlers[i].Update();
            }
        }

        public void Remove(PlayerId id)
        {
            VoiceHandler playerHandler = null;

            foreach (var handler in VoiceHandlers) {
                if (handler.ID == id) {
                    playerHandler = handler;
                    break;
                }
            }

            if (playerHandler != null) {
                playerHandler.Cleanup();
                _voiceHandlers.Remove(playerHandler);
            }
        }

        public void RemoveAll()
        {
            foreach (var handler in VoiceHandlers) {
                handler.Cleanup();
            }

            _voiceHandlers.Clear();
        }
    }
}
