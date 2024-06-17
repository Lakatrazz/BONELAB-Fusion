using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Utilities;

namespace LabFusion.Entities;

public class EntityIdManager<TEntity> where TEntity : INetworkRegistrable
{
    private readonly EntityIdList<TEntity> _registeredEntities = new();

    private readonly EntityIdList<TEntity> _queuedEntities = new();

    public EntityIdList<TEntity> RegisteredEntities => _registeredEntities;

    public EntityIdList<TEntity> QueuedEntities => _queuedEntities;

    public void RegisterEntity(ushort id, TEntity entity)
    {
        RegisteredEntities.AddEntity(id, entity);
        entity.Register(id);
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
            UnregisterEntity(entity.Id);
        }
    }

    public ushort QueueEntity(TEntity entity)
    {
        var id = QueuedEntities.AllocateNewId();

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
