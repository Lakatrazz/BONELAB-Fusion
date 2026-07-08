using System.Text.Json.Serialization;

namespace LabFusion.Downloading.ModIO;

public delegate void ModCallback(ModCallbackInfo info);

public struct ModCallbackInfo
{
    public static readonly ModCallbackInfo FailedCallback = new()
    {
        Data = default,
        Result = ModResult.FAILED,
    };

    public ModData Data;

    public ModResult Result;
}

[Serializable]
public readonly struct ModData
{
    /// <summary>
    /// The mod's name ID.
    /// </summary>
    [JsonPropertyName("name_id")]
    public string NameID { get; init; }

    /// <summary>
    /// The mod's integer ID.
    /// </summary>
    [JsonPropertyName("id")]
    public int ID { get; init; }

    /// <summary>
    /// The maturity setting for the mod.
    /// </summary>
    [JsonPropertyName("maturity_option")]
    public int MaturityOption { get; init; }

    /// <summary>
    /// The platform data for each platform the mod has uploaded.
    /// </summary>
    [JsonPropertyName("platforms")]
    public List<ModPlatformData> Platforms { get; init; }

    /// <summary>
    /// The logo data for the mod, containing the url for the thumbnail.
    /// </summary>
    [JsonPropertyName("logo")]
    public ModLogoData Logo { get; init; }

    /// <summary>
    /// The tag data for every tag added to this mod.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<ModTagData> Tags { get; init; }

    /// <summary>
    /// Whether or not this mod is marked as mature.
    /// </summary>
    public bool Mature => MaturityOption > 0;

    /// <summary>
    /// Checks if the mod has a certain tag.
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public bool HasTag(string tag)
    {
        if (Tags == null)
        {
            return false;
        }

        foreach (var modTag in Tags)
        {
            if (modTag.Name == tag)
            {
                return true;
            }
        }

        return false;
    }
}
