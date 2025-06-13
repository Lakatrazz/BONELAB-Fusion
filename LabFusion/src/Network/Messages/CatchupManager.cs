using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Network;

/// <summary>
/// Allows for setting up messages that will be sent to new players as they join.
/// </summary>
public static class CatchupManager
{
    /// <summary>
    /// Callback invoked when a new player joins the server and needs to be caught up on past server messages.
    /// </summary>
    public static event Action<PlayerID> OnPlayerServerCatchup;

    /// <summary>
    /// Invoked on the entity owner's end when a Player finishes creating a NetworkEntity and needs its data to be caught up.
    /// </summary>
    public static event NetworkEntityPlayerDelegate OnEntityDataCatchup;

    public static void RequestEntityDataCatchup(NetworkEntityReference entityReference)
    {
        if (!entityReference.TryGetEntity(out var entity))
        {
            return;
        }

        RequestEntityDataCatchup(entity.OwnerID ?? PlayerIDManager.GetHostID(), entityReference);
    }

    public static void RequestEntityDataCatchup(PlayerID ownerID, NetworkEntityReference entityReference)
    {
        if (ownerID.IsMe)
        {
            return;
        }

        var data = new EntityPlayerData() { PlayerID = PlayerIDManager.LocalSmallID, Entity = entityReference };

        MessageRelay.RelayNative(data, NativeMessageTag.EntityDataRequest, new MessageRoute(ownerID.SmallID, NetworkChannel.Reliable));
    }

    internal static void InvokePlayerServerCatchup(PlayerID playerID) => OnPlayerServerCatchup.InvokeSafe(playerID, "executing OnPlayerCatchup hook");

    internal static void InvokeEntityDataCatchup(PlayerID playerID, NetworkEntityReference entityReference)
    {
        if (!entityReference.TryGetEntity(out var entity))
        {
            return;
        }

        entity.InvokeDataCatchup(playerID);

        OnEntityDataCatchup?.InvokeSafe(playerID, entity, "executing CatchupManager.OnEntityDataCatchup");
    }
}
