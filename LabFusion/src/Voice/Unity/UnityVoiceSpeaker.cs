using Il2CppInterop.Runtime.InteropTypes.Arrays;

using LabFusion.Data;
using LabFusion.Representation;

using UnityEngine;

using PCMReaderCallback = UnityEngine.AudioClip.PCMReaderCallback;

namespace LabFusion.Voice.Unity;

public class UnityVoiceSpeaker : VoiceSpeaker
{
    private readonly Queue<float> _readingQueue = new();

    private float _amplitude = 0f;

    private bool _clearedAudio = false;

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

    public override float GetVoiceAmplitude()
    {
        return _amplitude;
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

    private void ClearVoiceData()
    {
        _readingQueue.Clear();
    }

    public override void Update()
    {
        if (!_clearedAudio && _amplitude <= VoiceVolume.SilencingVolume)
        {
            _clearedAudio = true;
            ClearVoiceData();
        }
        else if (_clearedAudio && _amplitude >= VoiceVolume.MinimumVoiceVolume)
        {
            _clearedAudio = false;
        }
    }

    public override void OnVoiceDataReceived(byte[] data)
    {
        if (MicrophoneDisabled)
        {
            ClearVoiceData();
            return;
        }

        VerifyRep();

        byte[] decompressed = VoiceCompressor.DecompressVoiceData(data);

        // Convert the byte array back to a float array and enqueue it
        float volumeMultiplier = GetVoiceMultiplier();

        for (int i = 0; i < decompressed.Length; i += sizeof(float))
        {
            float value = BitConverter.ToSingle(decompressed, i) * volumeMultiplier;

            _readingQueue.Enqueue(value);
        }
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
        _amplitude = 0f;

        for (int i = 0; i < data.Length; i++)
        {
            float output = 0f;

            if (_readingQueue.Count > 0)
            {
                output = _readingQueue.Dequeue();
            }

            data[i] = output;

            _amplitude += Math.Abs(output);
        }

        _amplitude /= data.Length;
    }
}