using LabFusion.Marrow.Messages;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Scene;

namespace LabFusion.Marrow.Player;

public enum PlayerInteractType
{
    /// <summary>
    /// No interaction was set.
    /// </summary>
    None,

    /// <summary>
    /// The player damaged another player.
    /// </summary>
    DamagedOtherPlayer,

    /// <summary>
    /// The player was killed by another player.
    /// </summary>
    KilledByOtherPlayer,
}

public delegate void PlayerInteractDelegate(PlayerID playerID, PlayerID otherPlayerID, PlayerInteractType type);

/// <summary>
/// Events relating to the interactions of two players.
/// </summary>
public static class PlayerInteractManager
{
    /// <summary>
    /// Invoked when one player had a specific interaction with another player as given by a <see cref="PlayerInteractType"/>.
    /// </summary>
    public static event PlayerInteractDelegate PlayersInteracted;

    public static void RelayPlayerInteraction(PlayerReference otherPlayerReference, PlayerInteractType type)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        var data = new PlayerInteractData()
        {
            OtherPlayerReference = otherPlayerReference,
            Type = type,
        };

        MessageRelay.RelayModule<PlayerInteractMessage, PlayerInteractData>(data, CommonMessageRoutes.ReliableToClients);
    }

    public static void OnPlayerInteraction(PlayerID playerID, PlayerID otherPlayerID, PlayerInteractType type)
    {
        PlayersInteracted?.InvokeSafe(playerID, otherPlayerID, type, "executing PlayerInteractManager.PlayersInteracted event");
    }
}
