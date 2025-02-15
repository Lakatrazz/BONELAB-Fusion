using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.SDK.Gamemodes;

namespace LabFusion.Marrow;

public static class NetworkCombatManager
{
    public static bool CanAttack(NetworkPlayer player)
    {
        // If a Gamemode is active, check if the gamemode gives attack permission
        if (GamemodeManager.IsGamemodeStarted)
        {
            return GamemodeManager.ActiveGamemode.CanAttack(player.PlayerId);
        }

        bool friendlyFire = LobbyInfoManager.LobbyInfo.FriendlyFire;

        return friendlyFire;
    }
}
