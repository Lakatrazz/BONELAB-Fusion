using LabFusion.Utilities;
using LabFusion.Player;
using LabFusion.Network;
using LabFusion.Entities;
using LabFusion.Marrow.Messages;

using Il2CppSLZ.Marrow;

namespace LabFusion.Marrow.Extenders;

public class ObjectDestructibleExtender : EntityComponentArrayExtender<ObjectDestructible>
{
    public static readonly FusionComponentCache<ObjectDestructible, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, ObjectDestructible[] components)
    {
        foreach (var component in components)
        {
            Cache.Add(component, entity);
        }

        entity.OnEntityDataCatchup += OnEntityDataCatchup;
    }

    protected override void OnUnregister(NetworkEntity entity, ObjectDestructible[] components)
    {
        foreach (var component in components)
        {
            Cache.Remove(component);
        }

        entity.OnEntityDataCatchup -= OnEntityDataCatchup;
    }

    private void OnEntityDataCatchup(NetworkEntity entity, PlayerID player)
    {
        foreach (var destructible in Components)
        {
            OnEntityDataCatchup(destructible, entity, player);
        }
    }

    private void OnEntityDataCatchup(ObjectDestructible destructible, NetworkEntity entity, PlayerID player)
    {
        bool destroyed = IsDestroyed(destructible);

        if (!destroyed)
        {
            return;
        }

        var data = ComponentIndexData.Create(entity.ID, GetIndex(destructible).Value);

        MessageRelay.RelayModule<ObjectDestructibleDestroyMessage, ComponentIndexData>(data, new MessageRoute(player.SmallID, NetworkChannel.Reliable));
    }

    private static bool IsDestroyed(ObjectDestructible destructible)
    {
        if (destructible._isDead)
        {
            return true;
        }

        if (destructible._poolee != null && destructible.IsDespawned)
        {
            return true;
        }

        return false;
    }
}