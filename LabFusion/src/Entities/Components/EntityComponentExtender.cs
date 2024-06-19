using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Entities;

public abstract class EntityComponentExtender<TComponent> : IEntityComponentExtender where TComponent : Component
{
    private NetworkEntity _networkEntity = null;
    private TComponent _component = null;

    public NetworkEntity NetworkEntity => _networkEntity;

    public TComponent Component => _component;

    public bool TryRegister(NetworkEntity networkEntity, params GameObject[] parents)
    {
        foreach (var parent in parents)
        {
            var component = parent.GetComponentInChildren<TComponent>(true);

            if (component != null)
            {
                Register(networkEntity, component);
                return true;
            }
        }

        return false;
    }

    public void Register(NetworkEntity networkEntity, TComponent component)
    {
        _networkEntity = networkEntity;
        _component = component;

        networkEntity.ConnectExtender(this);

        networkEntity.OnEntityUnregistered += Unregister;

        OnRegister(NetworkEntity, Component);
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

        OnUnregister(NetworkEntity, Component);

        _networkEntity = null;
        _component = null;
    }

    protected abstract void OnRegister(NetworkEntity networkEntity, TComponent component);
    protected abstract void OnUnregister(NetworkEntity networkEntity, TComponent component);
}