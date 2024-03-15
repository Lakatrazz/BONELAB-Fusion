using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Senders;

using Steamworks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnityEngine;

namespace LabFusion.Voice.Unity;

public sealed class UnityVoiceReceiver : IVoiceReceiver
{
    private byte[] _uncompressedData = null;

    private bool _hasVoiceActivity = false;

    private AudioClip _voiceClip = null;

    private int _lastSample = 0;

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

    public void UpdateVoice(bool enabled)
    {
        if (Microphone.devices.Count <= 0)
        {
            _uncompressedData = null;
            _hasVoiceActivity = false;
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
            _uncompressedData = null;
            _hasVoiceActivity = false;
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

        byte[] byteArray = new byte[audioData.Length * sizeof(float)];

        bool isTalking = false;
        for (int i = 0; i < audioData.Length; i++)
        {
            byte[] converted = BitConverter.GetBytes(audioData[i]);
            Array.Copy(converted, 0, byteArray, i * sizeof(float), sizeof(float));

            // Check for talking
            if (Math.Abs(audioData[i]) > 0.0001f)
            {
                isTalking = true;
            }
        }

        _uncompressedData = byteArray;
        _hasVoiceActivity = isTalking;
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