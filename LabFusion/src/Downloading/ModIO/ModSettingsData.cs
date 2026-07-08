using System.Text.Json.Serialization;

namespace LabFusion.Downloading.ModIO;

[Serializable]
public readonly struct ModSettingsData
{
    [JsonPropertyName("mod.io.access_token")]
    public string ModIOAccessToken { get; init; }
}
