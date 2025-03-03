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

        entity.OnEntityCatchup += OnEntityCatchup;
    }

    protected override void OnUnregister(NetworkEntity entity, ObjectDestructible[] components)
    {
        foreach (var component in components)
        {
            Cache.Remove(component);
        }

        entity.OnEntityCatchup -= OnEntityCatchup;
    }

    private void OnEntityCatchup(NetworkEntity entity, PlayerId player)
    {
        foreach (var destructible in Components)
        {
            OnEntityCatchup(destructible, entity, player);
        }
    }

    private void OnEntityCatchup(ObjectDestructible destructible, NetworkEntity entity, PlayerId player)
    {
        if (!destructible._isDead)
        {
            return;
        }

        var data = ComponentIndexData.Create(entity.Id, GetIndex(destructible).Value);

        MessageRelay.RelayNative(data, NativeMessageTag.ObjectDestructibleDestroy, NetworkChannel.Reliable, RelayType.ToTarget, player);
    }
}