using LabFusion.Network;
using LabFusion.Permissions;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;

namespace LabFusion.Utilities;

public static class FusionDevTools
{
    public static bool DevToolsDisabled
    {
        get
        {
            if (GamemodeManager.IsGamemodeStarted)
            {
                var gamemode = GamemodeManager.ActiveGamemode;

                if (gamemode.DisableDevTools)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public static bool DespawnConstrainer(PlayerID id)
    {
        // Check permission level
        var level = PlayerPermissions.GetPermissionLevel(id.PlatformID);
        if (!PermissionManager.IsSufficient(level, LobbyInfoManager.LobbyInfo.Constrainer))
        {
            return true;
        }

        return false;
    }

    public static bool DespawnDevTool(PlayerID id)
    {
        if (DevToolsDisabled)
        {
            return true;
        }

        // Check permission level
        var level = PlayerPermissions.GetPermissionLevel(id.PlatformID);
        if (!PermissionManager.IsSufficient(level, LobbyInfoManager.LobbyInfo.DevTools))
        {
            return true;
        }

        return false;
    }

    public static bool PreventSpawnGun(PlayerID id)
    {
        // Check gamemode
        if (GamemodeManager.IsGamemodeStarted)
        {
            var gamemode = GamemodeManager.ActiveGamemode;

            if (gamemode.DisableSpawnGun)
                return true;
        }

        // Check permission level
        var level = PlayerPermissions.GetPermissionLevel(id.PlatformID);
        if (!PermissionManager.IsSufficient(level, LobbyInfoManager.LobbyInfo.DevTools))
        {
            return true;
        }

        return false;
    }
}
