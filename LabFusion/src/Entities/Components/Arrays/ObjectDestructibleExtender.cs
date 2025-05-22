using LabFusion.Utilities;
using LabFusion.Player;
using LabFusion.Network;

using Il2CppSLZ.Marrow;

namespace LabFusion.Entities;

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

    private void OnEntityDataCatchup(NetworkEntity entity, PlayerId player)
    {
        foreach (var destructible in Components)
        {
            OnEntityDataCatchup(destructible, entity, player);
        }
    }

    private void OnEntityDataCatchup(ObjectDestructible destructible, NetworkEntity entity, PlayerId player)
    {
        bool destroyed = destructible._isDead || destructible.IsDespawned;

        if (!destroyed)
        {
            return;
        }

        var data = ComponentIndexData.Create(entity.Id, GetIndex(destructible).Value);

        MessageRelay.RelayNative(data, NativeMessageTag.ObjectDestructibleDestroy, NetworkChannel.Reliable, RelayType.ToTarget, player);
    }
}