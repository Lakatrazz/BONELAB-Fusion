namespace LabFusion.Entities;

public delegate void EntityIDEvent<TEntity>(ushort id, TEntity entity);

public class EntityIDList<TEntity>
{
    public Dictionary<ushort, TEntity> IDEntityLookup { get; } = new();
    public Dictionary<TEntity, ushort> EntityIDLookup { get; } = new();

    public HashSet<ushort> ReservedIDs { get; } = new();

    public event EntityIDEvent<TEntity> EntityAdded, EntityRemoved;

    public ushort LastID { get; private set; } = 0;

    public void ReserveID(ushort id)
    {
        if (ReservedIDs.Contains(id))
        {
            return;
        }

        if (LastID <= id)
        {
            LastID = id;
            LastID++;
        }

        ReservedIDs.Add(id);
    }

    public void Unreserve(ushort id)
    {
        ReservedIDs.Remove(id);
    }

    public bool IsReserved(ushort id)
    {
        return ReservedIDs.Contains(id);
    }

    private bool IsUsedID(ushort id)
    {
        return IDEntityLookup.ContainsKey(id) || IsReserved(id);
    }

    public ushort AllocateNewID()
    {
        LastID++;

        // Check if the id is already being used or reserved
        if (IsUsedID(LastID))
        {
            while (IsUsedID(LastID) && LastID < ushort.MaxValue)
            {
                LastID++;
            }
        }

        return LastID;
    }

    public bool HasEntity(ushort id)
    {
        return IDEntityLookup.ContainsKey(id);
    }

    public TEntity GetEntity(ushort id)
    {
        if (IDEntityLookup.TryGetValue(id, out var entity))
        {
            return entity;
        }

        return default;
    }

    public void AddEntity(ushort id, TEntity entity)
    {
        IDEntityLookup.Add(id, entity);
        EntityIDLookup.Add(entity, id);

        EntityAdded?.Invoke(id, entity);
    }

    public void RemoveEntity(ushort id)
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

    private void RemoveEntity(ushort id, TEntity entity)
    {
        IDEntityLookup.Remove(id);
        EntityIDLookup.Remove(entity);

        EntityRemoved?.Invoke(id, entity);
    }

    public void ClearID()
    {
        // Get highest unused id
        LastID = 0;

        while (IsUsedID(LastID) && LastID < ushort.MaxValue)
        {
            LastID++;
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