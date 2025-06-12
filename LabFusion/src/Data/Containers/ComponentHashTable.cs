using LabFusion.Extensions;
using LabFusion.Network.Serialization;

using UnityEngine;

namespace LabFusion.Data;

public class ComponentHashData : INetSerializable
{
    public const int Size = sizeof(int) * 2;

    public int? GetSize() => Size;

    public int Hash;
    public int Index;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Hash);
        serializer.SerializeValue(ref Index);
    }
}

public class ComponentHashTable<TComponent> where TComponent : Component
{
    private readonly Dictionary<int, List<TComponent>> _hashToComponents = new();
    private readonly Dictionary<TComponent, int> _componentToHash = new(new UnityComparer());

    public Dictionary<int, List<TComponent>> HashToComponents => _hashToComponents;
    public Dictionary<TComponent, int> ComponentToHash => _componentToHash;

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
        if (data == null)
        {
            return null;
        }

        if (!HashToComponents.TryGetValue(data.Hash, out var components))
        {
            return null;
        }

        if (data.Index >= components.Count || data.Index < 0)
        {
            return null;
        }

        return components[data.Index];
    }

    public ComponentHashData GetDataFromComponent(TComponent component)
    {
        if (component == null)
        {
            return null;
        }

        if (!ComponentToHash.TryGetValue(component, out var hash))
        {
            return null;
        }

        var list = HashToComponents[hash];
        var index = list.FindIndex((e) => e == component);

        return new ComponentHashData()
        {
            Hash = hash,
            Index = index,
        };
    }

    public bool IsHashed(TComponent component) => component != null && ComponentToHash.ContainsKey(component);
}
