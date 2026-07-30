using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Network;

public class ComponentIndexData : INetSerializable
{
    public const int Size = NetworkEntityReference.Size + sizeof(ushort);

    public NetworkEntityReference Entity;
    public ushort ComponentIndex;

    public int? GetSize() => Size;

    public static ComponentIndexData CreateFromComponent<TComponent, TExtender>(TComponent component, FusionComponentCache<TComponent, NetworkEntity> cache) where TExtender : EntityComponentArrayExtender<TComponent> where TComponent : Component
    {
        if (!cache.TryGet(component, out var entity))
        {
            return null;
        }

        var extender = entity.GetExtender<TExtender>();

        return new ComponentIndexData()
        {
            Entity = new(entity),
            ComponentIndex = extender.GetIndex(component).Value,
        };
    }

    public static ComponentIndexData CreateFromEntity(ushort entityID, ushort componentIndex)
    {
        return new ComponentIndexData()
        {
            Entity = new(entityID),
            ComponentIndex = componentIndex
        };
    }

    public bool TryGetComponent<TComponent, TExtender>(out TComponent component) where TExtender : EntityComponentArrayExtender<TComponent> where TComponent : Component
    {
        return TryGetComponentAndEntity<TComponent, TExtender>(out component, out _);
    }

    public bool TryGetComponentAndEntity<TComponent, TExtender>(out TComponent component, out NetworkEntity entity) where TExtender : EntityComponentArrayExtender<TComponent> where TComponent : Component
    {
        component = null;

        entity = Entity.GetEntity();

        if (entity == null)
        {
            return false;
        }

        var extender = entity.GetExtender<TExtender>();

        if (extender == null)
        {
            return false;
        }

        component = extender.GetComponent(ComponentIndex);

        return component != null;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Entity);
        serializer.SerializeValue(ref ComponentIndex);
    }
}