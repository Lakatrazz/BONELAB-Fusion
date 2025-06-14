using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.SDK.Gamemodes;

namespace LabFusion.Player;

public static class NetworkCombatManager
{
    /// <summary>
    /// Returns if the Local Player can attack a specific NetworkPlayer.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static bool CanAttack(NetworkPlayer player)
    {
        // If a Gamemode is active, check if the gamemode gives attack permission
        if (GamemodeManager.IsGamemodeStarted)
        {
            return GamemodeManager.ActiveGamemode.CanAttack(player.PlayerID);
        }

        bool friendlyFire = LobbyInfoManager.LobbyInfo.FriendlyFire;

        return friendlyFire;
    }
}
