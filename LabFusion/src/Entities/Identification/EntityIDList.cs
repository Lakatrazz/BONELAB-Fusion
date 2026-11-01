namespace LabFusion.Entities;

/// <summary>
/// A delegate that runs when changes occur to an EntityIDList.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <param name="id"></param>
/// <param name="entity"></param>
public delegate void EntityIDDelegate<TEntity>(NetworkEntityID id, TEntity entity);

/// <summary>
/// A list of entities and their IDs.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class EntityIDList<TEntity>
{
    /// <summary>
    /// The list of entity IDs by their entity.
    /// </summary>
    public Dictionary<NetworkEntityID, TEntity> IDByEntity { get; } = new();

    /// <summary>
    /// The list of entities by their ID.
    /// </summary>
    public Dictionary<TEntity, NetworkEntityID> EntityByID { get; } = new();

    /// <summary>
    /// The list of reserved entity IDs. These IDs cannot be allocated automatically and have to manually be used.
    /// </summary>
    public HashSet<NetworkEntityID> ReservedIDs { get; } = new();

    /// <summary>
    /// An event that is invoked when an entity is added to the list.
    /// </summary>
    public event EntityIDDelegate<TEntity> EntityAdded;

    /// <summary>
    /// An event that is invoked when an entity is removed from the list.
    /// </summary>
    public event EntityIDDelegate<TEntity> EntityRemoved;

    /// <summary>
    /// The last ID that was allocated for the list. New entities will allocate the next ID after. Reserved IDs cannot be allocated.
    /// </summary>
    public NetworkEntityID LastID { get; private set; } = NetworkEntityID.Empty;

    /// <summary>
    /// Reserves an entity ID so that it cannot be automatically allocated.
    /// </summary>
    /// <param name="id"></param>
    public void Reserve(NetworkEntityID id)
    {
        if (ReservedIDs.Contains(id))
        {
            return;
        }

        if (LastID <= id)
        {
            LastID = id.GetNext();
        }

        ReservedIDs.Add(id);
    }

    /// <summary>
    /// Unreserves an entity ID so that it is free to be allocated.
    /// </summary>
    /// <param name="id"></param>
    public void Unreserve(NetworkEntityID id)
    {
        ReservedIDs.Remove(id);
    }

    /// <summary>
    /// Returns true if an entity ID is reserved.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool IsReserved(NetworkEntityID id)
    {
        return ReservedIDs.Contains(id);
    }

    /// <summary>
    /// Returns true if an entity ID is currently taken, whether registered or reserved.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool IsUsed(NetworkEntityID id)
    {
        return IDByEntity.ContainsKey(id) || IsReserved(id);
    }

    /// <summary>
    /// Allocates a unique ID if available.
    /// <para>If none are available, this will currently just return the maximum ID. This is not ideal as this would mean multiple entities trying to use the same ID, but the ID should never get this large regardless.</para>
    /// </summary>
    /// <returns></returns>
    public NetworkEntityID AllocateUniqueID()
    {
        LastID = LastID.GetNext();

        // If the ID is used, keep looping until we get a new valid one, or we run out
        while (IsUsed(LastID) && LastID.HasSpace())
        {
            LastID = LastID.GetNext();
        }

        return LastID;
    }


    /// <summary>
    /// Clears the last allocated ID.
    /// </summary>
    public void ClearID()
    {
        // Get highest unused id
        LastID = NetworkEntityID.Empty;

        while (IsUsed(LastID) && LastID.HasSpace())
        {
            LastID = LastID.GetNext();
        }
    }


    /// <summary>
    /// Returns true if an entity is registered at an ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool HasEntity(NetworkEntityID id)
    {
        return IDByEntity.ContainsKey(id);
    }

    /// <summary>
    /// Gets an entity from its ID, or returns null if none is found.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public TEntity GetEntity(NetworkEntityID id)
    {
        if (IDByEntity.TryGetValue(id, out var entity))
        {
            return entity;
        }

        return default;
    }

    /// <summary>
    /// Adds an entity to the list at a given ID.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="entity"></param>
    public void AddEntity(NetworkEntityID id, TEntity entity)
    {
        IDByEntity.Add(id, entity);
        EntityByID.Add(entity, id);

        EntityAdded?.Invoke(id, entity);
    }

    /// <summary>
    /// Removes an entity from the list at a given ID.
    /// </summary>
    /// <param name="id"></param>
    public void RemoveEntity(NetworkEntityID id)
    {
        if (!IDByEntity.ContainsKey(id))
        {
            return;
        }

        var entity = IDByEntity[id];

        RemoveEntity(id, entity);
    }

    /// <summary>
    /// Removes an entity from the list.
    /// </summary>
    /// <param name="entity"></param>
    public void RemoveEntity(TEntity entity)
    {
        if (!EntityByID.ContainsKey(entity))
        {
            return;
        }

        var id = EntityByID[entity];

        RemoveEntity(id, entity);
    }

    /// <summary>
    /// Clears all entities as well as the last allocated ID.
    /// </summary>
    public void Clear()
    {
        foreach (var entity in IDByEntity)
        {
            EntityRemoved?.Invoke(entity.Key, entity.Value);
        }

        IDByEntity.Clear();
        EntityByID.Clear();

        ClearID();
    }

    private void RemoveEntity(NetworkEntityID id, TEntity entity)
    {
        IDByEntity.Remove(id);
        EntityByID.Remove(entity);

        EntityRemoved?.Invoke(id, entity);
    }
}