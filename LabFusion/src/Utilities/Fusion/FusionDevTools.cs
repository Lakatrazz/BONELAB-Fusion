using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;

namespace LabFusion.Utilities
{
    public static class FusionDevTools
    {
        public static bool DespawnConstrainer(PlayerId id)
        {
            // Check permission level
            FusionPermissions.FetchPermissionLevel(id, out var level, out _);
            if (!FusionPermissions.HasSufficientPermissions(level, LobbyInfoManager.LobbyInfo.Constrainer))
            {
                return true;
            }

            return false;
        }

        public static bool DespawnDevTool(PlayerId id)
        {
            // Check gamemode
            if (Gamemode.ActiveGamemode != null)
            {
                var gamemode = Gamemode.ActiveGamemode;

                if (gamemode.DisableDevTools)
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

        public static bool PreventSpawnGun(PlayerId id)
        {
            // Check gamemode
            if (Gamemode.ActiveGamemode != null)
            {
                var gamemode = Gamemode.ActiveGamemode;

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
}
