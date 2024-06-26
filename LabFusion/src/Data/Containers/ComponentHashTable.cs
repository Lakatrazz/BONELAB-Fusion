using LabFusion.Extensions;
using LabFusion.Network;

using UnityEngine;

namespace LabFusion.Data;

public class ComponentHashData : IFusionSerializable
{
    public int hash;
    public int index;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(hash);
        writer.Write(index);
    }

    public void Deserialize(FusionReader reader)
    {
        hash = reader.ReadInt32();
        index = reader.ReadInt32();
    }
}

public class ComponentHashTable<TComponent> where TComponent : Component
{
    private readonly FusionDictionary<int, List<TComponent>> _hashToComponents = new();
    private readonly FusionDictionary<TComponent, int> _componentToHash = new(new UnityComparer());

    public FusionDictionary<int, List<TComponent>> HashToComponents => _hashToComponents;
    public FusionDictionary<TComponent, int> ComponentToHash => _componentToHash;

    public int AddComponent(int hash, TComponent component)
    {
        if (!HashToComponents.TryGetValue(hash, out var components))
        {
            components = new();
            HashToComponents.Add(hash, components);
        }

        components.Add(component);
        ComponentToHash.Add(component, hash);

        return components.Count - 1;
    }

    public void RemoveComponent(TComponent component)
    {
        if (!ComponentToHash.TryGetValue(component, out var hash))
        {
            return;
        }

        ComponentToHash.Remove(component);

        if (HashToComponents.TryGetValue(hash, out var components))
        {
            // Regular remove will not work for IL2CPP objects
            // So we use RemoveAll
            components.RemoveAll((e) => e == component);

            if (components.Count <= 0)
            {
                HashToComponents.Remove(hash);
            }
        }
    }

    public TComponent GetComponentFromData(ComponentHashData data)
    {
        if (!HashToComponents.TryGetValue(data.hash, out var components))
        {
            return null;
        }

        if (data.index >= components.Count || data.index < 0)
        {
            return null;
        }

        return components[data.index];
    }

    public ComponentHashData GetDataFromComponent(TComponent component)
    {
        if (!ComponentToHash.TryGetValue(component, out var hash))
        {
            return null;
        }

        var list = HashToComponents[hash];
        var index = list.FindIndex((e) => e == component);

        return new ComponentHashData()
        {
            hash = hash,
            index = index,
        };
    }
}
