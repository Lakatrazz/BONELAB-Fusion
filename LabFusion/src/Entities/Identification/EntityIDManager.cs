using LabFusion.Utilities;
using LabFusion.Extensions;

namespace LabFusion.Entities;

public class EntityIDManager<TEntity> where TEntity : INetworkRegistrable
{
    public EntityIDList<TEntity> RegisteredEntities { get; } = new();

    public event Action<TEntity> EntityRegistered, EntityUnregistered;

    public void RegisterEntity(ushort id, TEntity entity)
    {
        if (RegisteredEntities.HasEntity(id))
        {
            FusionLogger.Warn($"Tried registering an entity with ID {id}, but an entity was already registered. The original entity will be unregistered.");

            UnregisterEntity(id);
        }

        RegisteredEntities.AddEntity(id, entity);
        entity.Register(id);

        EntityRegistered?.InvokeSafe(entity, "executing OnEntityRegistered hook");
    }

    public void UnregisterEntity(ushort id)
    {
        // Check if the entity is registered
        if (!RegisteredEntities.HasEntity(id))
        {
            return;
        }

        var entity = RegisteredEntities.GetEntity(id);
        RegisteredEntities.RemoveEntity(id);

        entity.Unregister();

        EntityUnregistered?.InvokeSafe(entity, "executing OnEntityUnregistered hook");
    }

    public void UnregisterEntity(TEntity entity)
    {
        if (entity.IsRegistered)
        {
            UnregisterEntity(entity.ID);
        }
    }
}
