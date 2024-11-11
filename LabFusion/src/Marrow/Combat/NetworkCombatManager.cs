using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.SDK.Gamemodes;

namespace LabFusion.Marrow;

public static class NetworkCombatManager
{
    public static bool CanAttack(NetworkPlayer player)
    {
        // For now, have friendly fire always on for gamemodes
        // In the future, check teams
        if (GamemodeManager.IsGamemodeStarted)
        {
            return true;
        }

        bool friendlyFire = LobbyInfoManager.LobbyInfo.FriendlyFire;

        return friendlyFire;
    }
}
