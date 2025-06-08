using LabFusion.Data;
using LabFusion.Player;

namespace LabFusion.Voice.Unity;

using System;

public class UnityVoiceSpeaker : VoiceSpeaker
{
    public override float Amplitude { get; set; } = 0f;

    public UnityVoiceSpeaker(PlayerID id)
    {
        // Save the id
        _id = id;

        // Hook into contact info changing
        ContactsList.OnContactUpdated += OnContactUpdated;

        // Update the contact info
        OnContactUpdated(ContactsList.GetContact(id));
    }

    public override void Cleanup()
    {
        // Unhook contact updating
        ContactsList.OnContactUpdated -= OnContactUpdated;
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
        byte[] decompressed = VoiceCompressor.DecompressVoiceData(data);

        // Convert the byte array back to a float array and enqueue it
        float volume = VoiceVolume.GetVolumeMultiplier() * Volume;

        float logarithmicVolume = volume * volume;

        float amplitude = 0f;
        int sampleCount = 0;

        var sources = VoiceSourceManager.GetVoicesByID(ID.SmallID);

        for (int i = 0; i < decompressed.Length; i += sizeof(float))
        {
            float value = BitConverter.ToSingle(decompressed, i) * logarithmicVolume;

            VoiceSourceManager.EnqueueSample(sources, value);

            amplitude += Math.Abs(value);
            sampleCount++;
        }

        if (sampleCount > 0)
        {
            amplitude /= sampleCount;
        }

        Amplitude = amplitude;

        VoiceSourceManager.SetAmplitude(sources, amplitude);
    }
}