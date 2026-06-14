namespace LabFusion.Entities;

public static class EntityGraphTraversal
{
    public static List<NetworkEntity> GetAllLinkedEntities(NetworkEntity startEntity)
    {
        List<NetworkEntity> allEntities = new();
        HashSet<ushort> visitedEntities = new();

        GetAllLinkedEntitiesRecursive(startEntity, allEntities, visitedEntities);

        return allEntities;
    }

    private static void GetAllLinkedEntitiesRecursive(NetworkEntity entity, List<NetworkEntity> allEntities, HashSet<ushort> visitedEntities)
    {
        if (!visitedEntities.Add(entity.ID))
        {
            return;
        }

        allEntities.Add(entity);

        foreach (var linkedEntity in entity.LinkedEntities)
        {
            if (!linkedEntity.IsRegistered)
            {
                continue;
            }

            GetAllLinkedEntitiesRecursive(linkedEntity, allEntities, visitedEntities);
        }
    }
}
