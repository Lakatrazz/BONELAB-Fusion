using LabFusion.Network;

namespace LabFusion.Entities;

public static class EntityGraphTraversal
{
    public static readonly int MaxDepth = 8;

    public static List<NetworkEntity> GetAllOwnableLinkedEntities(NetworkEntity startEntity, out ClientSmallID? lockedOwner)
    {
        List<NetworkEntity> allEntities = new();
        HashSet<ushort> visitedEntities = new();

        GetAllOwnableLinkedEntitiesRecursive(startEntity, allEntities, visitedEntities, out lockedOwner);

        return allEntities;
    }

    private static void GetAllOwnableLinkedEntitiesRecursive(NetworkEntity entity, List<NetworkEntity> allEntities, HashSet<ushort> visitedEntities, out ClientSmallID? lockedOwner, int depth = 0)
    {
        lockedOwner = null;

        if (!visitedEntities.Add(entity.ID))
        {
            return;
        }

        if (entity.IsOwnerLocked)
        {
            lockedOwner = entity.OwnerID?.SmallID;
            return;
        }

        allEntities.Add(entity);

        if (depth >= MaxDepth)
        {
            return;
        }

        foreach (var linkedEntity in entity.LinkedEntities)
        {
            if (!linkedEntity.IsRegistered)
            {
                continue;
            }

            GetAllOwnableLinkedEntitiesRecursive(linkedEntity, allEntities, visitedEntities, out var linkedLockedOwner, depth + 1);

            if (!lockedOwner.HasValue && linkedLockedOwner.HasValue)
            {
                lockedOwner = linkedLockedOwner.Value;
            }
        }
    }

    public static List<NetworkEntity> GetAllLinkedEntities(NetworkEntity startEntity)
    {
        List<NetworkEntity> allEntities = new();
        HashSet<ushort> visitedEntities = new();

        GetAllLinkedEntitiesRecursive(startEntity, allEntities, visitedEntities);

        return allEntities;
    }

    private static void GetAllLinkedEntitiesRecursive(NetworkEntity entity, List<NetworkEntity> allEntities, HashSet<ushort> visitedEntities, int depth = 0)
    {
        if (!visitedEntities.Add(entity.ID))
        {
            return;
        }

        allEntities.Add(entity);

        if (depth >= MaxDepth)
        {
            return;
        }

        foreach (var linkedEntity in entity.LinkedEntities)
        {
            if (!linkedEntity.IsRegistered)
            {
                continue;
            }

            GetAllLinkedEntitiesRecursive(linkedEntity, allEntities, visitedEntities, depth + 1);
        }
    }
}
