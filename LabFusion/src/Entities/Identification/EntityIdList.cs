using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;

namespace LabFusion.Entities;

public delegate void EntityIdEvent<TEntity>(ushort id, TEntity entity);

public class EntityIdList<TEntity>
{
    private readonly FusionDictionary<ushort, TEntity> _idsToEntities = new();
    private readonly FusionDictionary<TEntity, ushort> _entitiesToIds = new();

    private readonly List<ushort> _reservedIds = new();

    public FusionDictionary<ushort, TEntity> IdEntityLookup => _idsToEntities;
    public FusionDictionary<TEntity, ushort> EntityIdLookup => _entitiesToIds;

    public List<ushort> ReservedIds => _reservedIds;

    public event EntityIdEvent<TEntity> OnEntityAdded, OnEntityRemoved;

    private ushort _lastId = 0;

    public ushort LastId => _lastId;

    public void ReserveId(ushort id)
    {
        if (_reservedIds.Contains(id))
        {
            return;
        }

        if (_lastId <= id)
        {
            _lastId = id;
            _lastId++;
        }

        _reservedIds.Add(id);
    }

    public void Unreserve(ushort id)
    {
        _reservedIds.Remove(id);
    }

    public bool IsReserved(ushort id)
    {
        return _reservedIds.Contains(id);
    }

    private bool IsUsedId(ushort id)
    {
        return IdEntityLookup.ContainsKey(id) || IsReserved(id);
    }

    public ushort AllocateNewId()
    {
        _lastId++;

        // Check if the id is already being used or reserved
        if (IsUsedId(LastId))
        {
            while (IsUsedId(LastId) && LastId < ushort.MaxValue)
            {
                _lastId++;
            }
        }

        return LastId;
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
        _entitiesToIds.Add(entity, id);

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
        if (!_entitiesToIds.ContainsKey(entity))
        {
            return;
        }

        var id = _entitiesToIds[entity];

        RemoveEntity(id, entity);
    }

    private void RemoveEntity(ushort id, TEntity entity)
    {
        _idsToEntities.Remove(id);
        _entitiesToIds.Remove(entity);

        OnEntityRemoved?.Invoke(id, entity);
    }

    public void ClearId()
    {
        // Get highest unused id
        _lastId = 0;

        while (IsUsedId(LastId) && LastId < ushort.MaxValue)
        {
            _lastId++;
        }
    }

    public void Clear()
    {
        foreach (var entity in _idsToEntities)
        {
            OnEntityRemoved?.Invoke(entity.Key, entity.Value);
        }

        _idsToEntities.Clear();
        _entitiesToIds.Clear();

        ClearId();
    }
}