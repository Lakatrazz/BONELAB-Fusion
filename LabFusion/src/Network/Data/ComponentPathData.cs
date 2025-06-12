using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Network;

public class ComponentPathData : INetSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort) * 2 + sizeof(bool) + ComponentHashData.Size;

    public bool HasEntity;

    public ushort EntityID;
    public ushort ComponentIndex;

    public ComponentHashData HashData;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref HasEntity);
        serializer.SerializeValue(ref EntityID);
        serializer.SerializeValue(ref ComponentIndex);

        bool hasHashData = HashData != null;

        serializer.SerializeValue(ref hasHashData);

        if (hasHashData)
        {
            serializer.SerializeValue(ref HashData);
        }
    }

    public static ComponentPathData CreateFromComponent<TComponent, TExtender>(TComponent component, ComponentHashTable<TComponent> hashTable, FusionComponentCache<TComponent, NetworkEntity> cache) where TExtender : EntityComponentArrayExtender<TComponent> where TComponent : Component
    {
        var hashData = hashTable.GetDataFromComponent(component);

        var hasNetworkEntity = false;
        ushort entityID = 0;
        ushort componentIndex = 0;

        if (cache.TryGet(component, out var entity))
        {
            hasNetworkEntity = true;
            var extender = entity.GetExtender<TExtender>();

            entityID = entity.ID;
            componentIndex = extender.GetIndex(component).Value;
        }

        return new ComponentPathData()
        {
            HasEntity = hasNetworkEntity,
            EntityID = entityID,
            ComponentIndex = componentIndex,
            HashData = hashData,
        };
    }

    public bool TryGetComponent<TComponent, TExtender>(ComponentHashTable<TComponent> hashTable, out TComponent component) where TComponent : Component where TExtender : EntityComponentArrayExtender<TComponent>
    {
        component = null;

        if (HasEntity)
        {
            var entity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(EntityID);

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
        else
        {
            component = hashTable.GetComponentFromData(HashData);

            return component != null;
        }
    }
}
