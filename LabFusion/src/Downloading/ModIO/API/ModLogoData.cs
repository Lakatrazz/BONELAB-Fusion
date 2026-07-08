using System.Text.Json.Serialization;

namespace LabFusion.Downloading.ModIO;

[Serializable]
public readonly struct ModLogoData
{
    /// <summary>
    /// The mod thumbnail at 320x180 resolution.
    /// </summary>
    [JsonPropertyName("thumb_320x180")]
    public string ThumbnailLowUrl { get; init; }

    /// <summary>
    /// The mod thumbnail at 640x360 resolution.
    /// </summary>
    [JsonPropertyName("thumb_640x360")]
    public string ThumbnailMediumUrl { get; init; }

    /// <summary>
    /// The mod thumbnail at 1280x720 resolution.
    /// </summary>
    [JsonPropertyName("thumb_1280x720")]
    public string ThumbnailHighUrl { get; init; }
}
