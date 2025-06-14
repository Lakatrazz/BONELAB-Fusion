using LabFusion.Utilities;

using Il2CppSLZ.Marrow;

namespace LabFusion.Entities;

public class AtvExtender : EntityComponentExtender<Atv>
{
    public static readonly FusionComponentCache<Atv, NetworkEntity> Cache = new();

    private Il2CppSystem.Action _onSeatRegistered = null;
    private Il2CppSystem.Action _onSeatDeregistered = null;

    protected override void OnRegister(NetworkEntity entity, Atv component)
    {
        Cache.Add(component, entity);

        var driverSeat = component.driverSeat;

        if (driverSeat != null)
        {
            _onSeatRegistered = (Il2CppSystem.Action)OnSeatRegistered;
            _onSeatDeregistered = (Il2CppSystem.Action)OnSeatDeregistered;

            driverSeat.RegisteredEvent += _onSeatRegistered;
            driverSeat.DeRegisteredEvent += _onSeatDeregistered;
        }
    }

    protected override void OnUnregister(NetworkEntity entity, Atv component)
    {
        Cache.Remove(component);

        var driverSeat = component.driverSeat;

        if (driverSeat != null)
        {
            driverSeat.RegisteredEvent -= _onSeatRegistered;
            driverSeat.DeRegisteredEvent -= _onSeatDeregistered;
        }

        _onSeatRegistered = null;
        _onSeatDeregistered = null;
    }

    protected void OnSeatRegistered()
    {
        var driverSeat = Component.driverSeat;

        if (driverSeat == null)
        {
            return;
        }

        var rigManager = driverSeat.rigManager;

        if (NetworkPlayerManager.TryGetPlayer(rigManager, out var player))
        {
            NetworkEntity.SetOwner(player.PlayerID);
            NetworkEntity.LockOwner();
        }
    }

    protected void OnSeatDeregistered()
    {
        NetworkEntity.UnlockOwner();
    }
}
