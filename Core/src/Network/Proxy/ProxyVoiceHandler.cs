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

using PCMReaderCallback = UnityEngine.AudioClip.PCMReaderCallback;
using LiteNetLib.Utils;

namespace LabFusion.Network {
    public class ProxyVoiceHandler : VoiceHandler {
        private const uint _androidSampleRate = 48000;
        private const float _defaultVolumeMultiplier = 10f;
        private const float _defaultPitchMultiplier = 0.5f;

        private readonly MemoryStream _compressedVoiceStream = new();
        private readonly MemoryStream _decompressedVoiceStream = new();
        private readonly Queue<float> _streamingReadQueue = new();

        public ProxyVoiceHandler(PlayerId id) {
            // Save the id
            _id = id;
            OnContactUpdated(ContactsList.GetContact(id));

            // Hook into contact info changing
            ContactsList.OnContactUpdated += OnContactUpdated;

            // Create the audio source and clip
            CreateAudioSource();

            _source.clip = AudioClip.Create("ProxyVoice", Convert.ToInt32(_androidSampleRate),
                        1, Convert.ToInt32(_androidSampleRate), true, (PCMReaderCallback)PcmReaderCallback);

            // Pitch fix, I don't know
            _source.pitch = _defaultPitchMultiplier;

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

        public override void OnVoiceBytesReceived(byte[] bytes)
        {
            if (true) // TODO: sending vc
            {
                NetDataWriter writer = ProxyNetworkLayer.NewWriter(FusionHelper.Network.MessageTypes.DecompressVoice);
                writer.Put(_id.LongId);
                writer.PutBytesWithLength(bytes);
                ProxyNetworkLayer.Instance.SendToProxyServer(writer);
            }
            else
            {
                OnDecompressedVoiceBytesReceived(bytes);
            }
        }

        public void OnDecompressedVoiceBytesReceived(byte[] bytes)
        {
            if (MicrophoneDisabled) {
                return;
            }

            VerifyRep();

            _decompressedVoiceStream.Position = 0;

            int length = bytes.Length;
            _decompressedVoiceStream.Write(bytes, 0, length);

            _decompressedVoiceStream.Position = 0;

            while (_decompressedVoiceStream.Position < length)
            {
                byte byte1 = (byte)_decompressedVoiceStream.ReadByte();
                byte byte2 = (byte)_decompressedVoiceStream.ReadByte();

                short pcmShort = (short)((byte2 << 8) | (byte1 << 0));
                float pcmFloat = Convert.ToSingle(pcmShort) / short.MaxValue;

                _streamingReadQueue.Enqueue(pcmFloat);
            }
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
