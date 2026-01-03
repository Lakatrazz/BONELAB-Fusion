using LabFusion.Network;
using LabFusion.Entities;
using LabFusion.Marrow.Extenders;

namespace LabFusion.Utilities;

public static class PooleeUtilities
{
    public static void DespawnAll()
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        // Loop through all NetworkProps and despawn them
        var entities = NetworkEntityManager.IDManager.RegisteredEntities.EntityIDLookup.Keys.ToArray();
        foreach (var networkEntity in entities)
        {
            var prop = networkEntity.GetExtender<NetworkProp>();

            if (prop == null)
            {
                continue;
            }

            var poolee = networkEntity.GetExtender<PooleeExtender>();

            if (poolee == null)
            {
                continue;
            }

            // Don't despawn fixtures
            if (IsFixture(networkEntity))
            {
                continue;
            }

            poolee.Component.Despawn();
        }
    }

    private static bool IsFixture(NetworkEntity entity)
    {
        if (entity.GetExtender<CircuitSocketExtender>() != null)
        {
            return true;
        }

        return false;
    }
}