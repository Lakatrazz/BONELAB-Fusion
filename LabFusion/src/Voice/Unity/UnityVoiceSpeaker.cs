using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Math;
using LabFusion.Audio;

using UnityEngine;

namespace LabFusion.Voice.Unity;

using System;

public class UnityVoiceSpeaker : VoiceSpeaker
{
    public AudioStreamFilter StreamFilter { get; set; } = null;

    public AudioLowPassFilter LowPassFilter { get; set; } = null;

    public const float HighFrequency = 25000f;

    public float Frequency { get; set; } = HighFrequency;

    public float TargetFrequency { get; set; } = HighFrequency;

    public float OcclusionMultiplier { get; set; } = 1f;

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
                StreamFilter.enabled = true;
                Source.Play();

                // Update frequency changes
                CheckLowPass();

                TargetFrequency = HighFrequency * OcclusionMultiplier;
                Frequency = TargetFrequency;
            }
            else
            {
                Source.Stop();
                StreamFilter.enabled = false;

                StreamFilter.ReadingQueue.Clear();
                _amplitude = 0f;
            }
        }
    }

    private float _amplitude = 0f;

    private float _silentTimer = 0f;

    public UnityVoiceSpeaker(PlayerID id)
    {
        // Save the id
        _id = id;

        // Hook into contact info changing
        ContactsList.OnContactUpdated += OnContactUpdated;

        // Create the audio source and clip
        CreateAudioSource();

        Source.clip = AudioInfo.ToneClip;

        StreamFilter = Source.gameObject.AddComponent<AudioStreamFilter>();
        LowPassFilter = Source.gameObject.AddComponent<AudioLowPassFilter>();

        LowPassFilter.lowpassResonanceQ = 0.1f;

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

    private float _lowPassCheckTimer = 0f;

    public override void Update()
    {
        if (!Playing)
        {
            return;
        }

        if (StreamFilter.ReadingQueue.Count <= 0 || _amplitude < VoiceVolume.MinimumVoiceVolume)
        {
            _silentTimer += TimeUtilities.DeltaTime;
        }

        if (_silentTimer > 1f)
        {
            Playing = false;
        }

        if (_lowPassCheckTimer < 0.05f)
        {
            _lowPassCheckTimer += TimeUtilities.DeltaTime;
        }
        else
        {
            CheckLowPass();
            _lowPassCheckTimer = 0f;
        }

        TargetFrequency = HighFrequency * OcclusionMultiplier;

        Frequency = ManagedMathf.Lerp(Frequency, TargetFrequency, Smoothing.CalculateDecay(24f, TimeUtilities.DeltaTime));

        LowPassFilter.cutoffFrequency = Frequency;
    }

    private void CheckLowPass()
    {
        OcclusionMultiplier = 1f;

        if (!RigData.HasPlayer)
        {
            return;
        }

        var listener = RigData.Refs.Headset.position;
        var source = Source.transform.position;

        OcclusionMultiplier = AudioOcclusion.RaycastOcclusionMultiplier(listener, source);
    }

    public override void Cleanup()
    {
        // Unhook contact updating
        ContactsList.OnContactUpdated -= OnContactUpdated;

        base.Cleanup();
    }

    private void OnContactUpdated(Contact contact)
    {
        if (contact.id != _id.PlatformID)
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
        float volume = VoiceVolume.GetGlobalVolumeMultiplier();

        float logarithmicVolume = volume * volume;

        float amplitude = 0f;
        int sampleCount = 0;

        for (int i = 0; i < decompressed.Length; i += sizeof(float))
        {
            float value = BitConverter.ToSingle(decompressed, i) * logarithmicVolume;

            StreamFilter.ReadingQueue.Enqueue(value);

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
}