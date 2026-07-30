using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Network;

public class ComponentPathData : INetSerializable
{
    public const int Size = sizeof(bool) * 2 + ComponentIndexData.Size + ComponentHashData.Size;

    public bool HasEntity => IndexData != null;

    public bool HasHash => HashData != null;

    public ComponentIndexData IndexData;

    public ComponentHashData HashData;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        bool hasEntity = HasEntity;

        serializer.SerializeValue(ref hasEntity);

        if (hasEntity)
        {
            serializer.SerializeValue(ref IndexData);
        }

        bool hasHash = HasHash;

        serializer.SerializeValue(ref hasHash);

        if (hasHash)
        {
            serializer.SerializeValue(ref HashData);
        }
    }

    public static ComponentPathData CreateFromComponent<TComponent, TExtender>(TComponent component, ComponentHashTable<TComponent> hashTable, FusionComponentCache<TComponent, NetworkEntity> cache) where TExtender : EntityComponentArrayExtender<TComponent> where TComponent : Component
    {
        var indexData = ComponentIndexData.CreateFromComponent<TComponent, TExtender>(component, cache);

        var hashData = hashTable.GetDataFromComponent(component);

        return new ComponentPathData()
        {
            IndexData = indexData,
            HashData = hashData,
        };
    }

    public bool TryGetComponent<TComponent, TExtender>(ComponentHashTable<TComponent> hashTable, out TComponent component) where TComponent : Component where TExtender : EntityComponentArrayExtender<TComponent>
    {
        if (HasEntity)
        {
            return IndexData.TryGetComponent<TComponent, TExtender>(out component);
        }
        else if (HasHash)
        {
            component = hashTable.GetComponentFromData(HashData);

            return component != null;
        }

        component = null;
        return false;
    }
}
