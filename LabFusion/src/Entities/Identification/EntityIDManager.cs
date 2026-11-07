using LabFusion.Utilities;
using LabFusion.Extensions;

namespace LabFusion.Entities;

/// <summary>
/// Manages a list of entities, their IDs, and callbacks.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class EntityIDManager<TEntity> where TEntity : INetworkRegistrable
{
    /// <summary>
    /// The list of registered entities by their IDs.
    /// </summary>
    public EntityIDList<TEntity> RegisteredEntities { get; } = new();

    /// <summary>
    /// An event that is invoked when an entity is registered.
    /// </summary>
    public event Action<TEntity> EntityRegistered;

    /// <summary>
    /// An event that is invoked when an entity is unregistered.
    /// </summary>
    public event Action<TEntity> EntityUnregistered;

    /// <summary>
    /// Registers an entity at a given ID.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="entity"></param>
    public void RegisterEntity(NetworkEntityID id, TEntity entity)
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

    /// <summary>
    /// Unregisters the entity at a given ID.
    /// </summary>
    /// <param name="id"></param>
    public void UnregisterEntity(NetworkEntityID id)
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

    /// <summary>
    /// Unregisters an entity.
    /// </summary>
    /// <param name="entity"></param>
    public void UnregisterEntity(TEntity entity)
    {
        if (entity.IsRegistered)
        {
            UnregisterEntity(entity.ID);
        }
    }
}
