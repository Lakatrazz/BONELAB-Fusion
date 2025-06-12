namespace LabFusion.Entities;

public delegate void EntityIDEvent<TEntity>(ushort id, TEntity entity);

public class EntityIDList<TEntity>
{
    private readonly Dictionary<ushort, TEntity> _idsToEntities = new();
    private readonly Dictionary<TEntity, ushort> _entitiesToIDs = new();

    private readonly HashSet<ushort> _reservedIDs = new();

    public Dictionary<ushort, TEntity> IDEntityLookup => _idsToEntities;
    public Dictionary<TEntity, ushort> EntityIDLookup => _entitiesToIDs;

    public HashSet<ushort> ReservedIDs => _reservedIDs;

    public event EntityIDEvent<TEntity> OnEntityAdded, OnEntityRemoved;

    private ushort _lastID = 0;

    public ushort LastID => _lastID;

    public void ReserveID(ushort id)
    {
        if (_reservedIDs.Contains(id))
        {
            return;
        }

        if (_lastID <= id)
        {
            _lastID = id;
            _lastID++;
        }

        _reservedIDs.Add(id);
    }

    public void Unreserve(ushort id)
    {
        _reservedIDs.Remove(id);
    }

    public bool IsReserved(ushort id)
    {
        return _reservedIDs.Contains(id);
    }

    private bool IsUsedID(ushort id)
    {
        return IDEntityLookup.ContainsKey(id) || IsReserved(id);
    }

    public ushort AllocateNewID()
    {
        _lastID++;

        // Check if the id is already being used or reserved
        if (IsUsedID(LastID))
        {
            while (IsUsedID(LastID) && LastID < ushort.MaxValue)
            {
                _lastID++;
            }
        }

        return LastID;
    }

    public bool HasEntity(ushort id)
    {
        return _idsToEntities.ContainsKey(id);
    }

    public TEntity GetEntity(ushort id)
    {
        if (_idsToEntities.TryGetValue(id, out var entity))
        {
            return entity;
        }

        return default;
    }

    public void AddEntity(ushort id, TEntity entity)
    {
        _idsToEntities.Add(id, entity);
        _entitiesToIDs.Add(entity, id);

        OnEntityAdded?.Invoke(id, entity);
    }

    public void RemoveEntity(ushort id)
    {
        if (!_idsToEntities.ContainsKey(id))
        {
            return;
        }

        var entity = _idsToEntities[id];

        RemoveEntity(id, entity);
    }

    public void RemoveEntity(TEntity entity)
    {
        if (!_entitiesToIDs.ContainsKey(entity))
        {
            return;
        }

        var id = _entitiesToIDs[entity];

        RemoveEntity(id, entity);
    }

    private void RemoveEntity(ushort id, TEntity entity)
    {
        _idsToEntities.Remove(id);
        _entitiesToIDs.Remove(entity);

        OnEntityRemoved?.Invoke(id, entity);
    }

    public void ClearID()
    {
        // Get highest unused id
        _lastID = 0;

        while (IsUsedID(LastID) && LastID < ushort.MaxValue)
        {
            _lastID++;
        }
    }

    public void Clear()
    {
        foreach (var entity in _idsToEntities)
        {
            OnEntityRemoved?.Invoke(entity.Key, entity.Value);
        }

        _idsToEntities.Clear();
        _entitiesToIDs.Clear();

        ClearID();
    }
}