using LabFusion.Extensions;
using LabFusion.Representation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Network
{
    public interface IVoiceHandler {
        public PlayerId ID { get; }
        public PlayerRep Rep { get; }
        public AudioSource Source { get; }

        public bool IsDestroyed { get; }

        public float Volume { get; set; }

        public void CreateAudioSource();
        public void VerifyRep();
        public void OnVoiceBytesReceived(byte[] bytes);

        public void Cleanup();
        public void Update();
    }

    public abstract class VoiceHandler : IVoiceHandler {
        protected PlayerId _id;
        public PlayerId ID { get { return _id; } }

        protected PlayerRep _rep;
        protected bool _hasRep;
        public PlayerRep Rep { get { return _rep; } }

        protected AudioSource _source;
        protected GameObject _sourceGo;
        public AudioSource Source { get { return _source; } }

        protected bool _isDestroyed;
        public bool IsDestroyed { get { return _isDestroyed; } }

        protected float _volume = 1f;
        public float Volume { get { return _volume; } set { _volume = value; } }

        public bool MicrophoneDisabled { get { return _hasRep && Rep.MicrophoneDisabled; } }

        public virtual void CreateAudioSource() {
            _sourceGo = new GameObject($"{ID.SmallId} Voice Source");
            _source = _sourceGo.AddComponent<AudioSource>();

            GameObject.DontDestroyOnLoad(_source);
            GameObject.DontDestroyOnLoad(_sourceGo);
            _sourceGo.hideFlags = HideFlags.DontUnloadUnusedAsset;

            _source.rolloffMode = AudioRolloffMode.Linear;
            _source.loop = true;
        }

        public virtual void VerifyRep() {
            if (!_hasRep && ID != null) {
                if (PlayerRepManager.TryGetPlayerRep(ID, out _rep)) {
                    _rep.InsertVoiceSource(Source);
                    _hasRep = true;
                }
            }
        }

        public virtual void Cleanup() {
            // Destroy audio source
            if (_source != null) {
                // Get rid of the clip
                if (_source.clip != null)
                    GameObject.Destroy(_source.clip);

                GameObject.Destroy(_sourceGo);
            }

            _isDestroyed = true;
        }

        public virtual void Update() { }

        public abstract void OnVoiceBytesReceived(byte[] bytes);
    }
}
