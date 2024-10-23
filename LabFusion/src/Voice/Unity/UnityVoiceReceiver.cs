using Il2CppInterop.Runtime.InteropTypes.Arrays;

using LabFusion.Preferences.Client;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Voice.Unity;

public sealed class UnityVoiceReceiver : IVoiceReceiver
{
    private byte[] _uncompressedData = null;

    private bool _hasVoiceActivity = false;

    private AudioClip _voiceClip = null;

    private int _lastSample = 0;

    private float _amplitude = 0f;

    private float _lastTalkTime = 0f;

    private bool _loopedData = false;

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

    public static string GetValidMicrophoneName()
    {
        return ClientSettings.VoiceChat.InputDevice.Value;
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
        var microphoneId = Microphone.GetMicrophoneDeviceIDFromName(microphoneName);

        // Invalid microphone name, don't record
        if (microphoneId == -1)
        {
            ClearData();
            return;
        }

        bool isRecording = Microphone.IsRecording(microphoneName);

        if (enabled && !isRecording)
        {
            _voiceClip = Microphone.Start(microphoneName, true, UnityVoice.ClipLength, UnityVoice.SampleRate);
        }
        else if (!enabled && isRecording)
        {
            Microphone.End(microphoneName);
        }

        if (!enabled || _voiceClip == null)
        {
            ClearData();
            return;
        }

        int position = Microphone.GetPosition(microphoneName);

        if (position < _lastSample)
        {
            _loopedData = true;
            position = UnityVoice.SampleRate;
        }

        int sampleCount = position - _lastSample;

        var audioData = new Il2CppStructArray<float>(sampleCount);

        // Check if we have any samples
        // If we don't, we don't need to waste the GetData call
        if (sampleCount > 0)
        {
            _voiceClip.GetData(audioData, _lastSample);
        }

        if (_loopedData)
        {
            _lastSample = 0;
            _loopedData = false;
        }
        else
        {
            _lastSample = position;
        }

        int elementSize = sizeof(float);
        byte[] byteArray = new byte[sampleCount * elementSize];

        bool isTalking = false;
        _amplitude = 0f;

        for (int i = 0; i < sampleCount; i++)
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

        if (sampleCount > 0)
        {
            _amplitude /= sampleCount;
        }

        CheckTalkingTimeout(ref isTalking);

        _wasTalking = isTalking;

        _uncompressedData = byteArray;
        _hasVoiceActivity = isTalking;

        if (!isTalking)
        {
            _amplitude = 0f;
        }
    }

    private bool _wasTalking = false;

    private void CheckTalkingTimeout(ref bool isTalking)
    {
        if (isTalking)
        {
            _lastTalkTime = TimeUtilities.TimeSinceStartup;
            return;
        }

        isTalking = TimeUtilities.TimeSinceStartup - _lastTalkTime <= VoiceVolume.TalkTimeoutTime;
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