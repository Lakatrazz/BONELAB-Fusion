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

    public bool IsFirst { get; set; } = false;

    public ConstrainerPointPair PointPair { get; set; }

    public ushort OtherId { get; set; } = 0;

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

        entity.OnEntityCreationCatchup += OnEntityCreationCatchup;

        Cache.Add(Tracker, NetworkEntity);

        _destroySensor = Tracker.gameObject.AddComponent<DestroySensor>();
        _destroySensor.Hook(OnSensorDestroyed);
    }

    private void OnConstraintUnregistered(NetworkEntity entity)
    {
        entity.DisconnectExtender(this);

        entity.OnEntityCreationCatchup -= OnEntityCreationCatchup;

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

    private void OnEntityCreationCatchup(NetworkEntity entity, PlayerId player)
    {
        if (!IsFirst)
        {
            return;
        }

        // Send create message
        var data = ConstraintCreateData.Create(PlayerIdManager.LocalSmallId, null, PointPair);
        data.Point1Id = NetworkEntity.Id;
        data.Point2Id = OtherId;

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