using Il2CppInterop.Runtime.InteropTypes.Arrays;

using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;

using UnityEngine;

using PCMReaderCallback = UnityEngine.AudioClip.PCMReaderCallback;

namespace LabFusion.Voice.Unity;

using System;

public class UnityVoiceSpeaker : VoiceSpeaker
{
    public UnityVoiceFilter VoiceFilter { get; set; } = null;

    private bool _playing = false;
    public bool Playing
    {
        get
        {
            return _playing;
        }
        set
        {
            if (_playing == value)
            {
                return;
            }

            _playing = value;

            if (value)
            {
                VoiceFilter.enabled = true;
                Source.Play();
            }
            else
            {
                Source.Stop();
                VoiceFilter.enabled = false;

                VoiceFilter.ReadingQueue.Clear();
                _amplitude = 0f;
            }
        }
    }

    private float _amplitude = 0f;

    private float _silentTimer = 0f;

    public UnityVoiceSpeaker(PlayerId id)
    {
        // Save the id
        _id = id;

        // Hook into contact info changing
        ContactsList.OnContactUpdated += OnContactUpdated;

        // Create the audio source and clip
        CreateAudioSource();

        Source.clip = AudioClip.Create("UnityVoice", 256, 1, UnityVoice.SampleRate, false, (PCMReaderCallback)PcmReaderCallback);

        VoiceFilter = Source.gameObject.AddComponent<UnityVoiceFilter>();

        Source.Play();

        // Update the contact info
        OnContactUpdated(ContactsList.GetContact(id));

        // Set the rep's audio source
        VerifyRep();
    }

    public override float GetVoiceAmplitude()
    {
        return _amplitude;
    }

    public override void Update()
    {
        if (!Playing)
        {
            return;
        }

        if (VoiceFilter.ReadingQueue.Count <= 0 || _amplitude < VoiceVolume.MinimumVoiceVolume)
        {
            _silentTimer += TimeUtilities.DeltaTime;
        }

        if (_silentTimer > 1f)
        {
            Playing = false;
        } 
    }

    public override void Cleanup()
    {
        // Unhook contact updating
        ContactsList.OnContactUpdated -= OnContactUpdated;

        base.Cleanup();
    }

    private void OnContactUpdated(Contact contact)
    {
        if (contact.id != _id.LongId)
        {
            return;
        }

        Volume = contact.volume;
    }

    public override void OnVoiceDataReceived(byte[] data)
    {
        if (MicrophoneDisabled)
        {
            Playing = false;
            return;
        }

        VerifyRep();

        byte[] decompressed = VoiceCompressor.DecompressVoiceData(data);

        // Convert the byte array back to a float array and enqueue it
        float volumeMultiplier = GetVoiceMultiplier();

        float amplitude = 0f;
        int sampleCount = 0;

        for (int i = 0; i < decompressed.Length; i += sizeof(float))
        {
            float value = BitConverter.ToSingle(decompressed, i) * volumeMultiplier;

            VoiceFilter.ReadingQueue.Enqueue(value);

            amplitude += Math.Abs(value);
            sampleCount++;
        }

        if (sampleCount > 0)
        {
            amplitude /= sampleCount;
        }

        _amplitude = amplitude;

        Playing = true;

        _silentTimer = 0f;
    }

    private float GetVoiceMultiplier()
    {
        float mult = VoiceVolume.GetGlobalVolumeMultiplier();

        // If the audio is 2D, lower the volume
        if (_source.spatialBlend <= 0f)
        {
            mult *= 0.25f;
        }

        return mult;
    }

    private void PcmReaderCallback(Il2CppStructArray<float> data)
    {
        // Setting all the data to 1 so it can be multiplied by the audio filter
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = 1f;
        }
    }
}