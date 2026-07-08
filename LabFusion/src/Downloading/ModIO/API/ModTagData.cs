using System.Text.Json.Serialization;

namespace LabFusion.Downloading.ModIO;

[Serializable]
public readonly struct ModTagData
{
    [JsonPropertyName("name")]
    public string Name { get; init; }
}
