using LabFusion.Utilities;
using LabFusion.Player;
using LabFusion.Network;
using LabFusion.Entities;
using LabFusion.Marrow.Extensions;
using LabFusion.Marrow.Messages;

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
        var rigManager = seat.rigManager;

        if (rigManager == null)
        {
            return;
        }

        if (!NetworkRig.Cache.TryGet(rigManager, out var networkRig))
        {
            return;
        }

        // TODO: Move this to be catchup on the rig because clients can sometimes own seats even when other players are in them!
        var data = new RigSeatData()
        {
            RigReference = new(networkRig.NetworkEntity),
            SeatReference = ComponentIndexData.CreateFromEntity(entity.ID, GetIndex(seat).Value),
            IsSeated = true,
        };

        MessageRelay.RelayModule<RigSeatMessage, RigSeatData>(data, new MessageRoute(player.SmallID, NetworkChannel.Reliable));
    }

    private void OnAfterTeleportToPose()
    {
        foreach (var component in Components)
        {
            component.TeleportRigToSeat();
        }
    }
}