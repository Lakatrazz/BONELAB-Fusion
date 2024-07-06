using LabFusion.Extensions;

using UnityEngine;

namespace LabFusion.Entities;

public interface IEntityComponentExtender : IEntityExtender
{
    bool TryRegister(NetworkEntity networkEntity, GameObject[] parents, GameObject[] blacklist = null);

    void Unregister();

    public static bool CheckBlacklist<TComponent>(TComponent component, GameObject[] blacklist = null) where TComponent : Component
    {
        if (blacklist == null)
        {
            return false;
        }

        var go = component.gameObject;

        foreach (var blacklisted in blacklist)
        {
            if (go.InHierarchyOf(blacklisted))
            {
                return true;
            }
        }

        return false;
    }
}
