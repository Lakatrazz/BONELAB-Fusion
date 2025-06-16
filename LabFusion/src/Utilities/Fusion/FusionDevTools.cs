using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Representation;
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
        FusionPermissions.FetchPermissionLevel(id, out var level, out _);
        if (!FusionPermissions.HasSufficientPermissions(level, LobbyInfoManager.LobbyInfo.Constrainer))
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
        FusionPermissions.FetchPermissionLevel(id, out var level, out _);
        if (!FusionPermissions.HasSufficientPermissions(level, LobbyInfoManager.LobbyInfo.DevTools))
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
        FusionPermissions.FetchPermissionLevel(id, out var level, out _);
        if (!FusionPermissions.HasSufficientPermissions(level, LobbyInfoManager.LobbyInfo.DevTools))
        {
            return true;
        }

        return false;
    }
}
