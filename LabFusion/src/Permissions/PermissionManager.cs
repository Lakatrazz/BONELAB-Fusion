using LabFusion.Data;
using LabFusion.Network;

namespace LabFusion.Permissions;

/// <summary>
/// Manages the saving and loading of client permissions for the local server.
/// </summary>
public static class PermissionManager
{
    /// <summary>
    /// The name of the file storing the permissions.
    /// </summary>
    public static readonly string FileName = "permissions.json";

    /// <summary>
    /// The relative path of the permissions to the persistent data folder.
    /// </summary>
    public static string RelativeFilePath => Path.Combine(ResourcePaths.ServerRelativePath, FileName);

    /// <summary>
    /// The actively loaded local permission list.
    /// </summary>
    public static PermissionList PermissionList { get; private set; } = new();

    /// <summary>
    /// Reads the permission list from the file, if available.
    /// </summary>
    public static void ReadFile()
    {
        PermissionList = new();

        var deserializedList = JsonSaver.ReadJsonFromFileWithBackup<PermissionList>(RelativeFilePath);
        
        if (deserializedList != null)
        {
            PermissionList = deserializedList;
        }
    }

    /// <summary>
    /// Writes the current permission list to the file.
    /// </summary>
    public static void WriteFile()
    {
        JsonSaver.WriteJsonToFileWithBackup(FileName, PermissionList);
    }

    /// <summary>
    /// Gets the permission level for a client on a given platform. Note that this will get the permission level based on the local server's settings and not the permission level for the currently joined server.
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="platformID"></param>
    /// <returns></returns>
    public static PermissionLevel GetPermissionLevel(string platform, ClientPlatformID platformID)
    {
        var clientList = PermissionList.GetClientList(platform);

        if (clientList.PermissionsByClient.TryGetValue(platformID, out var permissionInfo))
        {
            return permissionInfo.Level;
        }

        return PermissionLevel.Default;
    }

    /// <summary>
    /// Sets the permission level for a client on a given platform. Note this will set the permission level for the local server and not the server that is currently joined if there is one.
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="platformID"></param>
    /// <param name="level"></param>
    public static void SetPermissionLevel(string platform, ClientPlatformID platformID, PermissionLevel level)
    {
        var clientList = PermissionList.GetClientList(platform);

        clientList.PermissionsByClient[platformID] = new PermissionInfo()
        {
            Level = level,
        };

        WriteFile();
    }

    /// <summary>
    /// Returns true if the given permission level is at least the required permission level.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="required"></param>
    /// <returns></returns>
    public static bool IsSufficient(PermissionLevel level, PermissionLevel required) => level >= required;

    /// <summary>
    /// Returns true if the given permission level is greater than the required permission level.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="required"></param>
    /// <returns></returns>
    public static bool IsGreater(PermissionLevel level, PermissionLevel required) => level > required;
}
