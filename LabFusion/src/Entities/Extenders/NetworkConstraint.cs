using Il2CppSLZ.Marrow;

using LabFusion.MonoBehaviours;
using LabFusion.Network;
using LabFusion.Patching;
using LabFusion.Player;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Entities;

public class NetworkConstraint : IEntityExtender
{
    public static readonly FusionComponentCache<ConstraintTracker, NetworkEntity> Cache = new();

    private NetworkEntity _networkEntity = null;
    private ConstraintTracker _tracker = null;

    private DestroySensor _destroySensor = null;

    public NetworkEntity NetworkEntity => _networkEntity;

    public ConstraintTracker Tracker => _tracker;

    public ConstrainerPointPair PointPair { get; set; }

    public NetworkConstraint(NetworkEntity networkEntity, ConstraintTracker tracker)
    {
        _networkEntity = networkEntity;
        _tracker = tracker;

        networkEntity.HookOnRegistered(OnConstraintRegistered);
        networkEntity.OnEntityUnregistered += OnConstraintUnregistered;
    }

    private void OnConstraintRegistered(NetworkEntity entity)
    {
        entity.ConnectExtender(this);

        entity.OnEntityCatchup += OnEntityCatchup;

        Cache.Add(Tracker, NetworkEntity);

        _destroySensor = Tracker.gameObject.AddComponent<DestroySensor>();
        _destroySensor.Hook(OnSensorDestroyed);
    }

    private void OnConstraintUnregistered(NetworkEntity entity)
    {
        entity.DisconnectExtender(this);

        entity.OnEntityCatchup -= OnEntityCatchup;

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

    private void OnEntityCatchup(NetworkEntity entity, PlayerId player)
    {
        // Send create message
        var data = ConstraintCreateData.Create(PlayerIdManager.LocalSmallId, null, PointPair);

        MessageRelay.RelayNative(data, NativeMessageTag.ConstraintCreate, NetworkChannel.Reliable, RelayType.ToTarget, player);
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