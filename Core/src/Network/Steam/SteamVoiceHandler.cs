using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.Utilities;

using Steamworks;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnhollowerBaseLib;

using UnityEngine;

using PCMReaderCallback = UnityEngine.AudioClip.PCMReaderCallback;

namespace LabFusion.Network
{
    public class SteamVoiceHandler : VoiceHandler
    {
        private Queue<float> readingQueue = new();

        private const float _defaultVolumeMultiplier = 10f;

        public SteamVoiceHandler(PlayerId id)
        {
            // Save the id
            _id = id;
            OnContactUpdated(ContactsList.GetContact(id));

            // Hook into contact info changing
            ContactsList.OnContactUpdated += OnContactUpdated;

            // Create the audio source and clip
            CreateAudioSource();

            _source.clip = AudioClip.Create("ProxyVoice", Convert.ToInt32(41000),
                        1, Convert.ToInt32(41000), true, (PCMReaderCallback)PcmReaderCallback);

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

        private void OnContactUpdated(Contact contact)
        {
            Volume = contact.volume;
        }

        public override void OnVoiceBytesReceived(byte[] bytes)
        {

            if (MicrophoneDisabled)
            {
                return;
            }

            VerifyRep();

            byte[] voiceData = VoiceHelper.DecompressVoiceData(bytes);

            // Convert the byte array back to a float array and enqueue it
            for (int i = 0; i < voiceData.Length; i += 4)
            {
                float value = BitConverter.ToSingle(voiceData, i);

                readingQueue.Enqueue(value);
            }
        }

        private float GetVoiceMultiplier()
        {
            float mult = _defaultVolumeMultiplier * FusionPreferences.ClientSettings.GlobalVolume * Volume;

            // If we are loading or the audio is 2D, lower the volume
            if (FusionSceneManager.IsLoading() || _source.spatialBlend <= 0f)
            {
                mult *= 0.25f;
            }

            return mult;
        }

        private void PcmReaderCallback(Il2CppStructArray<float> data)
        {
            // Fill the data array with the received audio data
            for (int i = 0; i < data.Length; i++)
            {
                if (readingQueue.Count > 0)
                {
                    data[i] = readingQueue.Dequeue() * GetVoiceMultiplier();
                }
                else
                {
                    data[i] = 0f;
                }
            }
        }
    }
}
