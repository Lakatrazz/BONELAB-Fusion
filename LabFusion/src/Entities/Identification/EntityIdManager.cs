using LabFusion.Utilities;

namespace LabFusion.Entities;

public class EntityIDManager<TEntity> where TEntity : INetworkRegistrable
{
    private readonly EntityIDList<TEntity> _registeredEntities = new();

    private readonly EntityIDList<TEntity> _queuedEntities = new();

    public EntityIDList<TEntity> RegisteredEntities => _registeredEntities;

    public EntityIDList<TEntity> QueuedEntities => _queuedEntities;

    public event Action<TEntity> OnEntityRegistered, OnEntityUnregistered;

    public void RegisterEntity(ushort id, TEntity entity)
    {
        RegisteredEntities.AddEntity(id, entity);
        entity.Register(id);

        OnEntityRegistered?.InvokeSafe(entity, "executing OnEntityRegistered hook");
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

        OnEntityUnregistered?.InvokeSafe(entity, "executing OnEntityUnregistered hook");
    }

    public void UnregisterEntity(TEntity entity)
    {
        // Unqueue the entity
        if (entity.IsQueued)
        {
            QueuedEntities.RemoveEntity(entity);
        }

        // Unregister the entity
        if (entity.IsRegistered)
        {
            UnregisterEntity(entity.ID);
        }
    }

    public ushort QueueEntity(TEntity entity)
    {
        var id = QueuedEntities.AllocateNewID();

        QueuedEntities.AddEntity(id, entity);
        entity.Queue(id);

        return id;
    }

    public (bool, TEntity) UnqueueEntity(ushort queuedId, ushort allocatedId)
    {
        if (!QueuedEntities.HasEntity(queuedId))
        {
            return (false, default);
        }

        var entity = QueuedEntities.GetEntity(queuedId);
        QueuedEntities.RemoveEntity(entity);

        if (entity.IsDestroyed)
        {
            FusionLogger.Warn($"Attempted to unqueue an Entity with allocated id {allocatedId}, but it was destroyed!");
            return (false, default);
        }

        RegisterEntity(allocatedId, entity);

        return (true, entity);
    }
}
