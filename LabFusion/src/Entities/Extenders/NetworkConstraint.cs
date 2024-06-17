using Il2CppSLZ.Bonelab;

using LabFusion.MonoBehaviours;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Entities;

public class NetworkConstraint : IEntityExtender
{
    public static FusionComponentCache<ConstraintTracker, NetworkEntity> Cache = new();

    private NetworkEntity _networkEntity = null;
    private ConstraintTracker _tracker = null;

    private DestroySensor _destroySensor = null;

    public NetworkEntity NetworkEntity => _networkEntity;

    public ConstraintTracker Tracker => _tracker;

    public NetworkConstraint(NetworkEntity networkEntity, ConstraintTracker tracker)
    {
        _networkEntity = networkEntity;
        _tracker = tracker;

        networkEntity.HookOnRegistered(OnConstraintRegistered);
        networkEntity.OnEntityUnregistered += OnConstraintUnregistered;
    }

    private void OnConstraintRegistered(NetworkEntity entity)
    {
        Cache.Add(Tracker, NetworkEntity);

        _destroySensor = Tracker.gameObject.AddComponent<DestroySensor>();
        _destroySensor.Hook(OnSensorDestroyed);
    }

    private void OnConstraintUnregistered(NetworkEntity entity)
    {
        if (Tracker != null)
        {
            Cache.Remove(Tracker);
        }

        if (_destroySensor != null)
        {
            GameObject.Destroy(_destroySensor);
        }

        _tracker = null;
        _networkEntity = null;
    }

    private void OnSensorDestroyed()
    {
        if (NetworkEntity != null && NetworkEntity.IsRegistered)
        {
            NetworkEntityManager.IdManager.UnregisterEntity(NetworkEntity);
        }

        _destroySensor = null;
    }
}