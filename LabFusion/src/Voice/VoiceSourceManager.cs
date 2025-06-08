using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Voice;

public static class VoiceSourceManager
{
    public static List<VoiceSource> ActiveVoices { get; } = new();

    public static IEnumerable<VoiceSource> GetVoicesByID(int id) => ActiveVoices.Where(voice => voice.ID == id);

    public static void EnqueueSample(IEnumerable<VoiceSource> sources, float sample)
    {
        foreach (var source in sources)
        {
            try
            {
                source.StreamFilter.Enqueue(sample);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("enqueueing VoiceSource sample", e);
            }
        }
    }

    public static void SetAmplitude(IEnumerable<VoiceSource> sources, float amplitude)
    {
        foreach (var source in sources)
        {
            try
            {
                source.ReceivingInput = true;
                source.Amplitude = amplitude;
            }
            catch (Exception e)
            {
                FusionLogger.LogException("setting VoiceSource amplitude", e);
            }
        }
    }

    public static VoiceSource CreateVoiceSource(int id)
    {
        var gameObject = new GameObject($"Voice Source {id}");

        return CreateVoiceSource(gameObject, id);
    }

    public static VoiceSource CreateVoiceSource(GameObject gameObject, int id)
    {
        var source = gameObject.AddComponent<VoiceSource>();

        source.ID = id;

        return source;
    }

    internal static void OnInitializeMelon()
    {
        VoiceSource.OnVoiceEnabled += OnVoiceEnabled;
        VoiceSource.OnVoiceDisabled += OnVoiceDisabled;
    }

    private static void OnVoiceEnabled(VoiceSource source)
    {
        ActiveVoices.Add(source);
    }

    private static void OnVoiceDisabled(VoiceSource source)
    {
        ActiveVoices.RemoveAll(s => s == source);
    }
}
