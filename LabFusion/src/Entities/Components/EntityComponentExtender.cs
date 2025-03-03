using UnityEngine;

namespace LabFusion.Entities;

public abstract class EntityComponentExtender<TComponent> : IEntityComponentExtender where TComponent : Component
{
    private NetworkEntity _networkEntity = null;
    private TComponent _component = null;

    public NetworkEntity NetworkEntity => _networkEntity;

    public TComponent Component => _component;

    public bool TryRegister(NetworkEntity entity, GameObject parent)
    {
        var component = GetComponent(parent);

        if (component == null)
        {
            return false;
        }

        Register(entity, component);
        return true;
    }

    public void Register(NetworkEntity entity, TComponent component)
    {
        _networkEntity = entity;
        _component = component;

        entity.ConnectExtender(this);

        entity.OnEntityUnregistered += Unregister;

        OnRegister(NetworkEntity, Component);
    }
    
    public void Unregister()
    {
        if (NetworkEntity != null)
        {
            Unregister(NetworkEntity);
        }
    }

    public void Unregister(NetworkEntity entity)
    {
        if (_networkEntity == null)
        {
            return;
        }

        entity.OnEntityUnregistered -= Unregister;

        entity.DisconnectExtender(this);

        OnUnregister(NetworkEntity, Component);

        _networkEntity = null;
        _component = null;
    }

    public void RegisterDynamics(NetworkEntity entity, GameObject parent) { }
    public void UnregisterDynamics() { }

    protected abstract void OnRegister(NetworkEntity entity, TComponent component);
    protected abstract void OnUnregister(NetworkEntity entity, TComponent component);

    protected virtual TComponent GetComponent(GameObject go) => go.GetComponentInChildren<TComponent>(true);
}