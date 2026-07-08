using System.Text.Json.Serialization;

namespace LabFusion.Downloading.ModIO;

[Serializable]
public readonly struct ModPlatformData
{
    [JsonPropertyName("platform")]
    public string Platform { get; init; }

    [JsonPropertyName("modfile_live")]
    public int ModFileLive { get; init; }
}