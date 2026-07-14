using LabFusion.Utilities;
using LabFusion.Player;
using LabFusion.Network;
using LabFusion.Entities;
using LabFusion.Marrow.Extensions;

using Il2CppSLZ.Marrow;

namespace LabFusion.Marrow.Extenders;

public class SeatExtender : EntityComponentArrayExtender<Seat>
{
    public static readonly FusionComponentCache<Seat, NetworkEntity> Cache = new();

    public IMarrowEntityExtender MarrowEntityExtender { get; set; } = null;

    protected override void OnRegister(NetworkEntity entity, Seat[] components)
    {
        foreach (var component in components)
        {
            Cache.Add(component, entity);
        }

        entity.OnEntityDataCatchup += OnEntityDataCatchup;

        MarrowEntityExtender = entity.GetExtender<IMarrowEntityExtender>();

        if (MarrowEntityExtender != null)
        {
            MarrowEntityExtender.OnAfterTeleportToPose += OnAfterTeleportToPose;
        }
    }

    protected override void OnUnregister(NetworkEntity entity, Seat[] components)
    {
        foreach (var component in components)
        {
            Cache.Remove(component);
        }

        entity.OnEntityDataCatchup -= OnEntityDataCatchup;

        if (MarrowEntityExtender != null)
        {
            MarrowEntityExtender.OnAfterTeleportToPose -= OnAfterTeleportToPose;
            MarrowEntityExtender = null;
        }
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

        var data = new PlayerRepSeatData()
        {
            SeatID = entity.ID,
            SeatIndex = (byte)GetIndex(seat).Value,
            IsIngress = true,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.PlayerRepSeat, new MessageRoute(player.SmallID, NetworkChannel.Reliable));
    }

    private void OnAfterTeleportToPose()
    {
        foreach (var component in Components)
        {
            component.TeleportRigToSeat();
        }
    }
}