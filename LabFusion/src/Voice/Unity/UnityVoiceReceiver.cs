using Il2CppInterop.Runtime.InteropTypes.Arrays;

using LabFusion.Preferences.Client;
using LabFusion.Utilities;
using LabFusion.Audio;
using LabFusion.Player;

using UnityEngine;

namespace LabFusion.Voice.Unity;

using System;

public sealed class UnityVoiceReceiver : IVoiceReceiver
{
    private static readonly float[] SampleBuffer = new float[AudioInfo.OutputSampleRate];

    private byte[] _encodedData = null;

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

    public byte[] GetEncodedData()
    {
        return _encodedData;
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
        _encodedData = null;
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
            _voiceClip = Microphone.Start(microphoneName, true, UnityVoice.ClipLength, AudioInfo.OutputSampleRate);
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
            position = AudioInfo.OutputSampleRate;
        }

        int sampleCount = position - _lastSample;

        if (sampleCount <= 0)
        {
            _hasVoiceActivity = false;
            return;
        }

        var rawData = new Il2CppStructArray<float>(sampleCount);

        _voiceClip.GetData(rawData, _lastSample);

        var pointer = rawData.Pointer;
        var pointerSize = IntPtr.Size;

        if (_loopedData)
        {
            _lastSample = 0;
            _loopedData = false;
        }
        else
        {
            _lastSample = position;
        }

        float[] samples = new float[sampleCount];

        InteropUtilities.Copy(pointer, pointerSize, sampleCount, samples);

        bool isTalking = false;
        _amplitude = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float sample = samples[i] * VoiceVolume.DefaultSampleMultiplier;

            SampleBuffer[i] = sample;

            _amplitude += Math.Abs(sample);

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

        _hasVoiceActivity = isTalking;

        if (!isTalking)
        {
            _amplitude = 0f;

            _encodedData = null;
        }
        else
        {
            SendToSources(SampleBuffer, sampleCount);

            // Write encoded data
            short[] smallSamples = new short[sampleCount];

            VoiceConverter.CopySamples(samples, smallSamples, sampleCount);

            _encodedData = VoiceConverter.Encode(smallSamples);
        }
    }

    private static void SendToSources(float[] buffer, int sampleCount)
    {
        var sources = VoiceSourceManager.GetVoicesByID(PlayerIDManager.LocalSmallID);

        if (!sources.Any())
        {
            return;
        }

        float volume = VoiceVolume.GetVolumeMultiplier();
        float logarithmicVolume = volume * volume;

        float amplitude = 0f;

        for (var i = 0; i < sampleCount; i++)
        {
            float sample = buffer[i] * logarithmicVolume;

            VoiceSourceManager.EnqueueSample(sources, sample);

            amplitude += Math.Abs(sample);
        }

        VoiceSourceManager.SetAmplitude(sources, amplitude);
    }

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
        _encodedData = null;
        _hasVoiceActivity = false;
    }
}