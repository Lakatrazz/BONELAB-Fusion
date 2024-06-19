using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Entities;

public abstract class EntityComponentArrayExtender<TComponent> : IEntityComponentExtender where TComponent : Component
{
    private NetworkEntity _networkEntity = null;
    private TComponent[] _components = null;

    public NetworkEntity NetworkEntity => _networkEntity;

    public TComponent[] Components => _components;

    public bool TryRegister(NetworkEntity networkEntity, params GameObject[] parents)
    {
        List<TComponent> components = new();

        foreach (var parent in parents)
        {
            components.AddRange(parent.GetComponentsInChildren<TComponent>(true));
        }

        if (components.Count > 0)
        {
            Register(networkEntity, components.ToArray());
            return true;
        }

        return false;
    }

    public void Register(NetworkEntity networkEntity, TComponent[] components)
    {
        _networkEntity = networkEntity;
        _components = components;

        networkEntity.ConnectExtender(this);

        networkEntity.OnEntityUnregistered += Unregister;

        OnRegister(NetworkEntity, Components);
    }

    public void Unregister()
    {
        if (NetworkEntity != null)
        {
            Unregister(NetworkEntity);
        }
    }

    public void Unregister(NetworkEntity networkEntity)
    {
        if (_networkEntity == null)
        {
            return;
        }

        networkEntity.OnEntityUnregistered -= Unregister;

        networkEntity.DisconnectExtender(this);

        OnUnregister(NetworkEntity, Components);

        _networkEntity = null;
        _components = null;
    }

    public ushort? GetIndex(TComponent component)
    {
        for (ushort i = 0; i < Components.Length; i++)
        {
            if (Components[i] == component)
                return i;
        }

        return null;
    }

    public TComponent GetComponent(ushort index)
    {
        if (Components != null && Components.Length > index)
        {
            return Components[index];
        }

        return null;
    }

    protected abstract void OnRegister(NetworkEntity networkEntity, TComponent[] components);
    protected abstract void OnUnregister(NetworkEntity networkEntity, TComponent[] components);
}