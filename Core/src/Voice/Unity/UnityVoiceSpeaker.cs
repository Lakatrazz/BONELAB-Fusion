using LabFusion.Data;
using LabFusion.Representation;

using System;
using System.Collections.Generic;
using System.IO;

using UnhollowerBaseLib;

using UnityEngine;

using PCMReaderCallback = UnityEngine.AudioClip.PCMReaderCallback;

namespace LabFusion.Voice.Unity;

public class UnityVoiceSpeaker : VoiceSpeaker
{
    private const float _defaultVolumeMultiplier = 10f;

    private readonly Queue<float> _readingQueue = new();

    public UnityVoiceSpeaker(PlayerId id)
    {
        // Save the id
        _id = id;
        OnContactUpdated(ContactsList.GetContact(id));

        // Hook into contact info changing
        ContactsList.OnContactUpdated += OnContactUpdated;

        // Create the audio source and clip
        CreateAudioSource();

        Source.clip = AudioClip.Create("UnityVoice", UnityVoice.SampleRate,
                    1, UnityVoice.SampleRate, true, (PCMReaderCallback)PcmReaderCallback);

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

    public override void OnVoiceDataReceived(byte[] data)
    {
        if (MicrophoneDisabled)
        {
            return;
        }

        VerifyRep();

        byte[] decompressed = VoiceCompressor.DecompressVoiceData(data);

        // Convert the byte array back to a float array and enqueue it
        for (int i = 0; i < decompressed.Length; i += sizeof(float))
        {
            float value = BitConverter.ToSingle(decompressed, i);

            _readingQueue.Enqueue(value);
        }
    }

    private float GetVoiceMultiplier()
    {
        float mult = _defaultVolumeMultiplier * VoiceVolume.GetGlobalVolumeMultiplier();

        // If the audio is 2D, lower the volume
        if (_source.spatialBlend <= 0f)
        {
            mult *= 0.25f;
        }

        return mult;
    }

    private void PcmReaderCallback(Il2CppStructArray<float> data)
    {
        float mult = GetVoiceMultiplier();

        for (int i = 0; i < data.Length; i++)
        {
            if (_readingQueue.Count > 0)
            {
                data[i] = _readingQueue.Dequeue() * mult;
            }
            else
            {
                data[i] = 0.0f;
            }
        }

        _readingQueue.Clear();
    }
}