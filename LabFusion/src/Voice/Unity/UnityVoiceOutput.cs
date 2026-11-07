using LabFusion.Player;
using LabFusion.Contacts;
using LabFusion.Network;

namespace LabFusion.Voice.Unity;

using System;

public class UnityVoiceOutput : IVoiceOutput
{
    public PlayerID PlayerID { get; }
    public float Volume { get; set; } = 1f;
    public float Amplitude { get; set; } = 0f;

    public UnityVoiceOutput(PlayerID playerID)
    {
        PlayerID = playerID;
    }

    public void OnEnable()
    {
        // Hook into contact info changing
        ContactManager.ContactUpdated += OnContactUpdated;

        // Update the contact info
        OnContactUpdated(ContactManager.GetContactInfo(PlayerID.PlatformID));
    }

    public void OnDisable()
    {
        // Unhook contact updating
        ContactManager.ContactUpdated -= OnContactUpdated;
    }

    private void OnContactUpdated(ClientPlatformID platformID, ContactInfo contactInfo)
    {
        if (platformID != PlayerID.PlatformID)
        {
            return;
        }

        OnContactUpdated(contactInfo);
    }

    private void OnContactUpdated(ContactInfo contactInfo)
    {
        Volume = contactInfo.Volume;
    }

    void IVoiceOutput.OnEncodedDataReceived(byte[] data)
    {
        short[] smallSamples = VoiceConverter.Decode(data);

        int sampleCount = smallSamples.Length;

        float[] samples = new float[sampleCount];

        VoiceConverter.CopySamples(smallSamples, samples, sampleCount);

        // Convert the byte array back to a float array and enqueue it
        float volume = VoiceVolume.GetVolumeMultiplier() * Volume;

        float logarithmicVolume = volume * volume;

        float amplitude = 0f;

        var sources = VoiceSourceManager.GetVoicesByID((int)PlayerID.SmallID);

        for (int i = 0; i < sampleCount; i++)
        {
            float sample = samples[i] * logarithmicVolume * VoiceVolume.DefaultSampleMultiplier;

            VoiceSourceManager.EnqueueSample(sources, sample);

            amplitude += Math.Abs(sample);
        }

        if (sampleCount > 0)
        {
            amplitude /= sampleCount;
        }

        Amplitude = amplitude;

        VoiceSourceManager.SetAmplitude(sources, amplitude);
    }
}