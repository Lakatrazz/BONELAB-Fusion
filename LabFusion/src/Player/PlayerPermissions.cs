using LabFusion.Network;
using LabFusion.Permissions;

namespace LabFusion.Player;

public static class PlayerPermissions
{
    /// <summary>
    /// Returns the permission level for the local instance. 
    /// <para>If a server is running, this will always return <see cref="PermissionLevel.Owner"/>.</para>
    /// <para>If the client is connected to a server, this will return the permission set by the server.</para>
    /// <para>Otherwise, it will return <see cref="PermissionLevel.Default"/>.</para>
    /// </summary>
    /// <returns></returns>
    public static PermissionLevel GetLocalPermissionLevel()
    {
        if (ServerManager.IsServerRunning)
        {
            return PermissionLevel.Owner;
        }

        if (PlayerIDManager.HasLocalPlatformID)
        {
            return GetPermissionLevel(PlayerIDManager.LocalPlatformID.Value);
        }

        return PermissionLevel.Default;
    }

    /// <summary>
    /// Returns the permission level for a given client.
    /// <para>If a server is running, the permission level will be read directly from the saved permission list.</para>
    /// <para>If the client is connected to a server, the permission level will be read from the synchronized permission level.</para>
    /// <para>Otherwise, <see cref="PermissionLevel.Default"/> will be returned.</para>
    /// </summary>
    /// <param name="platformID"></param>
    /// <returns></returns>
    public static PermissionLevel GetPermissionLevel(ClientPlatformID platformID)
    {
        if (ServerManager.IsServerRunning)
        {
            return GetPermissionLevelOnServer(platformID);
        }
        else if (ClientManager.IsClientConnected)
        {
            return GetPermissionLevelOnClient(platformID);
        }

        return PermissionLevel.Default;
    }

    /// <summary>
    /// Attempts to set the permission level of a given client. This will only run on the local server and will not function when the client is connected to a server.
    /// <para>This will also update the metadata for the player so that other players are aware of the new permission level.</para>
    /// </summary>
    /// <param name="platformID"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public static bool TrySetPermissionLevel(ClientPlatformID platformID, PermissionLevel level)
    {
        if (!ServerManager.IsServerRunning)
        {
            return false;
        }

        PermissionManager.SetPermissionLevel(NetworkLayerManager.Platform, platformID, level);

        var playerID = PlayerIDManager.GetPlayerID(platformID);
        playerID?.Metadata.PermissionLevel.SetValue(level.ToString());

        return true;
    }

    internal static void Initialize()
    {
        LocalPlayer.Metadata.PermissionLevel.SetValue(PermissionLevel.Default.ToString());

        LocalPlayer.OnApplyInitialMetadata += OnApplyInitialMetadata;
    }

    private static void OnApplyInitialMetadata()
    {
        var level = ServerManager.IsServerRunning ? PermissionLevel.Owner.ToString() : PermissionLevel.Default.ToString();
        LocalPlayer.Metadata.PermissionLevel.SetValue(level);
    }

    private static PermissionLevel GetPermissionLevelOnServer(ClientPlatformID platformID)
    {
        if (PlayerIDManager.LocalPlatformID == platformID)
        {
            return PermissionLevel.Owner;
        }

        return PermissionManager.GetPermissionLevel(NetworkLayerManager.Platform, platformID);
    }

    private static PermissionLevel GetPermissionLevelOnClient(ClientPlatformID platformID)
    {
        var playerID = PlayerIDManager.GetPlayerID(platformID);

        if (playerID == null)
        {
            return PermissionLevel.Default;
        }

        if (Enum.TryParse<PermissionLevel>(playerID.Metadata.PermissionLevel.GetValue(), out var level))
        {
            return level;
        }

        return PermissionLevel.Default;
    }
}
