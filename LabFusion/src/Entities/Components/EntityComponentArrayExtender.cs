using UnityEngine;

namespace LabFusion.Entities;

public abstract class EntityComponentArrayExtender<TComponent> : IEntityComponentExtender where TComponent : Component
{
    private NetworkEntity _networkEntity = null;

    private TComponent[] _registeredComponents = null;
    private List<TComponent> _dynamicComponents = new();
    private TComponent[] _components = null;

    public NetworkEntity NetworkEntity => _networkEntity;

    public TComponent[] Components => _components;

    public bool TryRegister(NetworkEntity entity, GameObject parent)
    {
        TComponent[] components = parent.GetComponentsInChildren<TComponent>(true);

        if (components.Length <= 0)
        {
            return false;
        }

        Register(entity, components);
        return true;
    }

    public void Register(NetworkEntity entity, TComponent[] components)
    {
        _networkEntity = entity;
        _registeredComponents = components;

        entity.ConnectExtender(this);

        entity.OnEntityUnregistered += Unregister;

        ApplyComponents();
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

        OnUnregister(NetworkEntity, Components);

        _networkEntity = null;
        _registeredComponents = null;
        _components = null;
        _dynamicComponents = null;
    }

    public void RegisterDynamics(NetworkEntity entity, GameObject parent)
    {
        var dynamicComponents = parent.GetComponentsInChildren<TComponent>(true);

        if (dynamicComponents.Length <= 0)
        {
            return;
        }

        _dynamicComponents.AddRange(dynamicComponents);

        ApplyComponents();
    }

    public void UnregisterDynamics()
    {
        if (_dynamicComponents == null)
        {
            return;
        }

        if (_dynamicComponents.Count <= 0)
        {
            return;
        }

        _dynamicComponents.Clear();

        ApplyComponents();
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

    private void ApplyComponents()
    {
        if (Components != null)
        {
            OnUnregister(NetworkEntity, Components);
            _components = null;
        }

        int registeredCount = _registeredComponents.Length;
        int dynamicCount = _dynamicComponents.Count;

        _components = new TComponent[registeredCount + dynamicCount];

        for (var i = 0; i < registeredCount; i++)
        {
            _components[i] = _registeredComponents[i];
        }

        for (var i = 0; i < dynamicCount; i++)
        {
            _components[i + registeredCount] = _dynamicComponents[i];
        }

        OnRegister(NetworkEntity, Components);
    }

    protected abstract void OnRegister(NetworkEntity entity, TComponent[] components);
    protected abstract void OnUnregister(NetworkEntity entity, TComponent[] components);
}