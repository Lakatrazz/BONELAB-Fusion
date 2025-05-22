using System.Text.Json.Serialization;

namespace LabFusion.Data;

[Serializable]
public class ModInfo
{
    [JsonPropertyName("barcodes")]
    public List<string> Barcodes { get; set; } = new();

    [JsonPropertyName("modID")]
    public int ModID { get; set; } = -1;

    [JsonPropertyName("nameID")]
    public string NameID { get; set; } = null;
}
