using System.Text.Json.Serialization;

namespace LabFusion.SDK.Gamemodes;

[Serializable]
public class LevelRotation
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "New Level Rotation";

    [JsonPropertyName("levelBarcodes")]
    public List<string> LevelBarcodes { get; set; } = new();
}
