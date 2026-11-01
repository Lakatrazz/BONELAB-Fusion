using System.Text.Json.Serialization;

namespace LabFusion.Permissions;

/// <summary>
/// A serializable list of permissions by different platforms/
/// </summary>
[Serializable]
public class PermissionList
{
    /// <summary>
    /// The list of permissions for clients by the network layer platform.
    /// </summary>
    [JsonPropertyName("permissions")]
    public Dictionary<string, PermissionClientList> PermissionsByPlatform { get; set; } = new();

    /// <summary>
    /// Gets a list of permissions by client for the current platform, or returns a new list if there is none.
    /// </summary>
    /// <param name="platform"></param>
    /// <returns></returns>
    public PermissionClientList GetClientList(string platform)
    {
        if (PermissionsByPlatform.TryGetValue(platform, out var permissions))
        {
            return permissions;
        }

        permissions = new PermissionClientList();
        PermissionsByPlatform[platform] = permissions;

        return permissions;
    }
}
