using System.Text.Json.Serialization;

namespace LabFusion.Permissions;

/// <summary>
/// The permission info for a client.
/// </summary>
[Serializable]
public class PermissionInfo
{
    /// <summary>
    /// The level of permissions.
    /// </summary>
    [JsonPropertyName("level")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PermissionLevel Level { get; set; } = PermissionLevel.Default;
}
