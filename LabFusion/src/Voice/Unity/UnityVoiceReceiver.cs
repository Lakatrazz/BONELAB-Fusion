using Il2CppInterop.Runtime.InteropTypes.Arrays;

using UnityEngine;

namespace LabFusion.Voice.Unity;

public sealed class UnityVoiceReceiver : IVoiceReceiver
{
    private byte[] _uncompressedData = null;

    private bool _hasVoiceActivity = false;

    private AudioClip _voiceClip = null;

    private int _lastSample = 0;

    private float _amplitude = 0f;

    public float GetVoiceAmplitude()
    {
        return _amplitude;
    }

    public byte[] GetCompressedVoiceData()
    {
        return VoiceCompressor.CompressVoiceData(_uncompressedData);
    }

    public bool HasVoiceActivity()
    {
        return _hasVoiceActivity;
    }

    public string GetValidMicrophoneName()
    {
        return string.Empty;
    }

    private void ClearData()
    {
        _uncompressedData = null;
        _hasVoiceActivity = false;
        _amplitude = 0f;
    }

    public void UpdateVoice(bool enabled)
    {
        if (!UnityVoice.IsSupported())
        {
            ClearData();
            return;
        }

        string microphoneName = GetValidMicrophoneName();

        if (enabled && !Microphone.IsRecording(microphoneName))
        {
            _voiceClip = Microphone.Start(microphoneName, true, UnityVoice.ClipLength, UnityVoice.SampleRate);
        }
        else if (!enabled && Microphone.IsRecording(microphoneName))
        {
            Microphone.End(null);
        }

        if (!enabled || _voiceClip == null)
        {
            ClearData();
            return;
        }

        int position = Microphone.GetPosition(microphoneName);

        if (position < _lastSample)
        {
            _lastSample = 0;
        }

        int sampleCount = position - _lastSample;

        var audioData = new Il2CppStructArray<float>(sampleCount);
        _voiceClip.GetData(audioData, _lastSample);

        _lastSample = position;

        int elementSize = sizeof(float);
        byte[] byteArray = new byte[audioData.Length * elementSize];

        bool isTalking = false;
        _amplitude = 0f;

        for (int i = 0; i < audioData.Length; i++)
        {
            float sample = audioData[i] * VoiceVolume.DefaultSampleMultiplier;
            _amplitude += Mathf.Abs(sample);

            int elementPosition = i * elementSize;

            unsafe
            {
                byte* p = (byte*)&sample;

                for (var j = 0; j < elementSize; j++)
                {
                    byteArray[j + elementPosition] = *p++;
                }
            }

            // Check for talking
            if (isTalking)
            {
                continue;
            }

            isTalking = Math.Abs(sample) >= VoiceVolume.MinimumVoiceVolume;
        }

        if (audioData.Length > 0)
        {
            _amplitude /= audioData.Length;
        }

        _uncompressedData = byteArray;
        _hasVoiceActivity = isTalking;

        if (!isTalking)
        {
            _amplitude = 0f;
        }
    }

    public void Enable()
    {
        
    }

    public void Disable()
    {
        _uncompressedData = null;
        _hasVoiceActivity = false;
    }
}