using LabFusion.Data;
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
using BoneLib;

using PCMReaderCallback = UnityEngine.AudioClip.PCMReaderCallback;

namespace LabFusion.Network {
    public class SteamVoiceHandler : VoiceHandler {
        private const float _defaultVolumeMultiplier = 10f;

        private readonly MemoryStream _compressedVoiceStream = new();
        private readonly MemoryStream _decompressedVoiceStream = new();
        private readonly Queue<float> _streamingReadQueue = new();

        private float _lastClearTime;

        public SteamVoiceHandler(PlayerId id) {
            // Save the id
            _id = id;
            OnContactUpdated(ContactsList.GetContact(id));

            // Hook into contact info changing
            ContactsList.OnContactUpdated += OnContactUpdated;

            // Create the audio source and clip
            CreateAudioSource();

            Source.clip = AudioClip.Create("SteamVoice", Convert.ToInt32(SteamUser.SampleRate),
                        1, Convert.ToInt32(SteamUser.SampleRate), true, (PCMReaderCallback)PcmReaderCallback);

            _source.Play();

            // Set the rep's audio source
            VerifyRep();
        }

        public override void Cleanup()
        {
            // Unhook contact updating
            ContactsList.OnContactUpdated -= OnContactUpdated;

            base.Cleanup();
        }

        private void OnContactUpdated(Contact contact) {
            Volume = contact.volume;
        }

        public override void Update() {
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

        public override void OnVoiceBytesReceived(byte[] bytes) {
            if (_hasRep && Rep.MicrophoneDisabled) {
                return;
            }

            VerifyRep();

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
            float mult = _defaultVolumeMultiplier * FusionPreferences.ClientSettings.GlobalVolume * Volume;

            // If we are loading or the audio is 2D, lower the volume
            if (FusionSceneManager.IsLoading() || _source.spatialBlend <= 0f) {
                mult *= 0.25f;
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
