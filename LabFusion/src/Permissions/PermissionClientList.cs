using LabFusion.Network;

using System.Text.Json.Serialization;

namespace LabFusion.Permissions;

/// <summary>
/// A list of permissions by client platform ID.
/// </summary>
[Serializable]
public class PermissionClientList
{
    /// <summary>
    /// The permission info for each client.
    /// </summary>
    [JsonPropertyName("clients")]
    public Dictionary<ClientPlatformID, PermissionInfo> PermissionsByClient { get; set; } = new();
}
