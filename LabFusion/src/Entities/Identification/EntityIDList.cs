namespace LabFusion.Entities;

public delegate void EntityIDEvent<TEntity>(NetworkEntityID id, TEntity entity);

public class EntityIDList<TEntity>
{
    public Dictionary<NetworkEntityID, TEntity> IDEntityLookup { get; } = new();
    public Dictionary<TEntity, NetworkEntityID> EntityIDLookup { get; } = new();

    public HashSet<NetworkEntityID> ReservedIDs { get; } = new();

    public event EntityIDEvent<TEntity> EntityAdded, EntityRemoved;

    public NetworkEntityID LastID { get; private set; } = NetworkEntityID.Empty;

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

    public void Unreserve(NetworkEntityID id)
    {
        ReservedIDs.Remove(id);
    }

    public bool IsReserved(NetworkEntityID id)
    {
        return ReservedIDs.Contains(id);
    }

    public bool IsUsed(NetworkEntityID id)
    {
        return IDEntityLookup.ContainsKey(id) || IsReserved(id);
    }

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

    public bool HasEntity(NetworkEntityID id)
    {
        return IDEntityLookup.ContainsKey(id);
    }

    public TEntity GetEntity(NetworkEntityID id)
    {
        if (IDEntityLookup.TryGetValue(id, out var entity))
        {
            return entity;
        }

        return default;
    }

    public void AddEntity(NetworkEntityID id, TEntity entity)
    {
        IDEntityLookup.Add(id, entity);
        EntityIDLookup.Add(entity, id);

        EntityAdded?.Invoke(id, entity);
    }

    public void RemoveEntity(NetworkEntityID id)
    {
        if (!IDEntityLookup.ContainsKey(id))
        {
            return;
        }

        var entity = IDEntityLookup[id];

        RemoveEntity(id, entity);
    }

    public void RemoveEntity(TEntity entity)
    {
        if (!EntityIDLookup.ContainsKey(entity))
        {
            return;
        }

        var id = EntityIDLookup[entity];

        RemoveEntity(id, entity);
    }

    private void RemoveEntity(NetworkEntityID id, TEntity entity)
    {
        IDEntityLookup.Remove(id);
        EntityIDLookup.Remove(entity);

        EntityRemoved?.Invoke(id, entity);
    }

    public void ClearID()
    {
        // Get highest unused id
        LastID = NetworkEntityID.Empty;

        while (IsUsed(LastID) && LastID.HasSpace())
        {
            LastID = LastID.GetNext();
        }
    }

    public void Clear()
    {
        foreach (var entity in IDEntityLookup)
        {
            EntityRemoved?.Invoke(entity.Key, entity.Value);
        }

        IDEntityLookup.Clear();
        EntityIDLookup.Clear();

        ClearID();
    }
}