using LabFusion.Utilities;
using LabFusion.Player;
using LabFusion.Network;
using LabFusion.Entities;

using Il2CppSLZ.Marrow;

namespace LabFusion.Marrow.Extenders;

public class SeatExtender : EntityComponentArrayExtender<Seat>
{
    public static readonly FusionComponentCache<Seat, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, Seat[] components)
    {
        foreach (var component in components)
        {
            Cache.Add(component, entity);
        }

        entity.OnEntityDataCatchup += OnEntityDataCatchup;
    }

    protected override void OnUnregister(NetworkEntity entity, Seat[] components)
    {
        foreach (var component in components)
        {
            Cache.Remove(component);
        }

        entity.OnEntityDataCatchup -= OnEntityDataCatchup;
    }

    private void OnEntityDataCatchup(NetworkEntity entity, PlayerID player)
    {
        foreach (var component in Components)
        {
            OnEntityDataCatchup(component, entity, player);
        }
    }

    private void OnEntityDataCatchup(Seat seat, NetworkEntity entity, PlayerID player)
    {
        if (seat.rigManager == null)
        {
            return;
        }

        if (!NetworkPlayerManager.TryGetPlayer(seat.rigManager, out var seatedPlayer))
        {
            return;
        }

        var data = PlayerRepSeatData.Create(seatedPlayer.PlayerID, entity.ID, (byte)GetIndex(seat).Value, true);

        MessageRelay.RelayNative(data, NativeMessageTag.PlayerRepSeat, new MessageRoute(player.SmallID, NetworkChannel.Reliable));
    }
}