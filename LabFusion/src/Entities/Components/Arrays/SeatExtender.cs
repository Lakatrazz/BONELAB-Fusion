using LabFusion.Utilities;
using LabFusion.Player;
using LabFusion.Network;

using Il2CppSLZ.Marrow;

namespace LabFusion.Entities;

public class SeatExtender : EntityComponentArrayExtender<Seat>
{
    public static readonly FusionComponentCache<Seat, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, Seat[] components)
    {
        foreach (var component in components)
        {
            Cache.Add(component, networkEntity);
        }

        networkEntity.OnEntityCatchup += OnEntityCatchup;
    }

    protected override void OnUnregister(NetworkEntity networkEntity, Seat[] components)
    {
        foreach (var component in components)
        {
            Cache.Remove(component);
        }

        networkEntity.OnEntityCatchup -= OnEntityCatchup;
    }

    private void OnEntityCatchup(NetworkEntity entity, PlayerId player)
    {
        foreach (var component in Components)
        {
            OnEntityCatchup(component, entity, player);
        }
    }

    private void OnEntityCatchup(Seat seat, NetworkEntity entity, PlayerId player)
    {
        if (seat.rigManager == null)
        {
            return;
        }

        if (!NetworkPlayerManager.TryGetPlayer(seat.rigManager, out var seatedPlayer))
        {
            return;
        }

        var data = PlayerRepSeatData.Create(seatedPlayer.PlayerId, entity.Id, (byte)GetIndex(seat).Value, true);

        MessageRelay.RelayNative(data, NativeMessageTag.PlayerRepSeat, NetworkChannel.Reliable, RelayType.ToTarget, player);
    }
}