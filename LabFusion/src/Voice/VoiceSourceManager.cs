using UnityEngine;

namespace LabFusion.Voice;

public static class VoiceSourceManager
{
    public static List<VoiceSource> ActiveVoices { get; } = new();

    public static IEnumerable<VoiceSource> GetVoicesByID(int id) => ActiveVoices.Where(voice => voice.ID == id);

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
