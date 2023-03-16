using LabFusion.Extensions;
using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.Utilities;
using Steamworks;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnhollowerBaseLib;

using UnityEngine;

using PCMReaderCallback = UnityEngine.AudioClip.PCMReaderCallback;

namespace LabFusion.Network {
    public class SteamVoiceIdentifier {
        public static List<SteamVoiceIdentifier> VoiceIdentifiers = new List<SteamVoiceIdentifier>();

        private const float _defaultVolumeMultiplier = 10f;

        private readonly MemoryStream _compressedVoiceStream = new MemoryStream();
        private readonly MemoryStream _decompressedVoiceStream = new MemoryStream();
        private readonly Queue<float> _streamingReadQueue = new Queue<float>();

        private AudioSource _source;
        private PlayerId _id;
        private PlayerRep _rep;
        private bool _hasRep;

        private float _lastClearTime;

        public SteamVoiceIdentifier(PlayerId id) {
            // Create the audio source and clip
            _source = new GameObject($"{id.SmallId} Voice Source").AddComponent<AudioSource>();
            GameObject.DontDestroyOnLoad(_source);
            GameObject.DontDestroyOnLoad(_source.gameObject);
            _source.gameObject.hideFlags = HideFlags.DontUnloadUnusedAsset;

            _source.clip = AudioClip.Create("SteamVoice", Convert.ToInt32(SteamUser.SampleRate),
                        1, Convert.ToInt32(SteamUser.SampleRate), true, (PCMReaderCallback)PcmReaderCallback);

            // Setup the mixing settings
            _source.rolloffMode = AudioRolloffMode.Linear;

            // Set it to loop and play so its constantly active
            _source.loop = true;
            _source.Play();

            // Save values
            _id = id;
            VerifyPlayerRep();

            // Add to list
            VoiceIdentifiers.Add(this);
        }

        public void Cleanup() {
            // Destroy audio source
            if (!_source.IsNOC()) {
                // Get rid of clip
                if (!_source.clip.IsNOC())
                    GameObject.Destroy(_source.clip);

                GameObject.Destroy(_source.gameObject);
            }

            // Remove from list
            VoiceIdentifiers.Remove(this);
        }

        public void Update() {
            float time = Time.realtimeSinceStartup;

            // Every five seconds of no audio, clear the buffer
            if (time - _lastClearTime >= 5f) {
                // Clear audio data
                var clip = _source.clip;
                float[] samples = new float[clip.samples * clip.channels];
                _source.clip.SetData(samples, 0);

                // Clear the queue
                _streamingReadQueue.Clear();

                // Reset time
                _lastClearTime = time;
            }
        }

        public static void OnUpdate() {
            for (var i = 0; i < VoiceIdentifiers.Count; i++)
                VoiceIdentifiers[i].Update();
        }

        public static void RemoveVoiceIdentifier(PlayerId id) {
            foreach (var identifer in VoiceIdentifiers.ToArray()) {
                if (identifer._id == id) {
                    identifer.Cleanup();
                    break;
                }
            }
        }

        public static void CleanupAll() {
            foreach (var identifer in VoiceIdentifiers.ToArray()) {
                identifer.Cleanup();
            }
        }

        public static SteamVoiceIdentifier GetVoiceIdentifier(PlayerId id) {
            for (var i = 0; i < VoiceIdentifiers.Count; i++) {
                var identifier = VoiceIdentifiers[i];

                if (identifier._id == id)
                    return identifier;
            }

            var newIdentifier = new SteamVoiceIdentifier(id);
            return newIdentifier;
        }

        private void VerifyPlayerRep() {
            if (_id != null && !_hasRep) {
                PlayerRepManager.TryGetPlayerRep(_id, out _rep);

                if (_rep != null) {
                    _rep.InsertVoiceSource(_source);
                    _hasRep = true;
                }
            }
        }

        public void OnVoiceBytesReceived(byte[] bytes) {
            VerifyPlayerRep();

            // Decompress the voice data
            _compressedVoiceStream.Position = 0;
            _compressedVoiceStream.Write(bytes, 0, bytes.Length);

            _compressedVoiceStream.Position = 0;
            _decompressedVoiceStream.Position = 0;

            int numBytesWritten = SteamUser.DecompressVoice(_compressedVoiceStream, bytes.Length, _decompressedVoiceStream);

            _decompressedVoiceStream.Position = 0;

            while (_decompressedVoiceStream.Position < numBytesWritten)
            {
                byte byte1 = (byte)_decompressedVoiceStream.ReadByte();
                byte byte2 = (byte)_decompressedVoiceStream.ReadByte();

                short pcmShort = (short)((byte2 << 8) | (byte1 << 0));
                float pcmFloat = Convert.ToSingle(pcmShort) / short.MaxValue;

                _streamingReadQueue.Enqueue(pcmFloat);
            }

            // Reset clear time since we received a message
            _lastClearTime = Time.realtimeSinceStartup;
        }

        private float GetVoiceMultiplier() {
            float mult = _defaultVolumeMultiplier * FusionPreferences.ClientSettings.GlobalVolume;

            // If we are loading, half the volume
            if (FusionSceneManager.IsLoading()) {
                mult *= 0.5f;
            }

            return mult;
        }

        private void PcmReaderCallback(Il2CppStructArray<float> data)
        {
            float mult = GetVoiceMultiplier();

            for (int i = 0; i < data.Length; i++)
            {
                if (_streamingReadQueue.Count > 0)
                {
                    data[i] = _streamingReadQueue.Dequeue() * mult;
                }
                else
                {
                    data[i] = 0.0f;  // Nothing in the queue means we should just play silence
                }
            }
        }
    }
}
