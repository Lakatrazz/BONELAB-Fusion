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
    public static event Action<PlayerId> OnPlayerServerCatchup;

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

        RequestEntityDataCatchup(entity.OwnerId ?? PlayerIdManager.GetHostId(), entityReference);
    }

    public static void RequestEntityDataCatchup(PlayerId ownerId, NetworkEntityReference entityReference)
    {
        var data = new EntityPlayerData() { PlayerId = PlayerIdManager.LocalSmallId, Entity = entityReference };

        MessageRelay.RelayNative(data, NativeMessageTag.EntityDataRequest, NetworkChannel.Reliable, RelayType.ToTarget, ownerId.SmallId);
    }

    internal static void InvokePlayerServerCatchup(PlayerId playerId) => OnPlayerServerCatchup.InvokeSafe(playerId, "executing OnPlayerCatchup hook");

    internal static void InvokeEntityDataCatchup(PlayerId playerId, NetworkEntityReference entityReference)
    {
        if (!entityReference.TryGetEntity(out var entity))
        {
            return;
        }

        entity.InvokeDataCatchup(playerId);

        OnEntityDataCatchup?.InvokeSafe(playerId, entity, "executing CatchupManager.OnEntityDataCatchup");
    }
}
