using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;

using LabFusion.MonoBehaviours;
using LabFusion.Network;
using LabFusion.Entities;
using LabFusion.Marrow.Patching;
using LabFusion.Marrow.Messages;
using LabFusion.Player;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Marrow.Extenders;

public class NetworkConstraint : IEntityExtender
{
    public static readonly FusionComponentCache<ConstraintTracker, NetworkEntity> Cache = new();

    private NetworkEntity _networkEntity = null;
    private ConstraintTracker _tracker = null;

    private DestroySensor _destroySensor = null;

    public NetworkEntity NetworkEntity => _networkEntity;

    public bool IsRegistered { get; private set; } = false;

    public ConstraintTracker Tracker => _tracker;

    public bool IsFirst { get; set; } = false;

    public ConstrainerPointPair PointPair { get; set; }

    public NetworkEntityID OtherID { get; set; } = NetworkEntityID.Empty;

    public NetworkEntity BodyEntity { get; set; } = null;
    public NetworkEntity OtherBodyEntity { get; set; } = null;

    public NetworkConstraint(NetworkEntity networkEntity, ConstraintTracker tracker)
    {
        _networkEntity = networkEntity;
        _tracker = tracker;

        networkEntity.ConnectExtender(this);
    }

    public void OnExtenderRegistered()
    {
        IsRegistered = true;

        NetworkEntity.EntityCreationCatchingUp += OnEntityCreationCatchup;

        Cache.Add(Tracker, NetworkEntity);

        GetTrackerEntities();

        if (IsFirst)
        {
            LinkTrackerEntities();
        }

        AddDestroySensor();
    }

    public void OnExtenderUnregistered()
    {
        IsRegistered = false;

        NetworkEntity.EntityCreationCatchingUp -= OnEntityCreationCatchup;

        if (Tracker != null)
        {
            Cache.Remove(Tracker);
        }

        if (IsFirst)
        {
            UnlinkTrackerEntities();
        }

        RemoveDestroySensor();

        UnregisterOtherConstraint();

        _tracker = null;
        _networkEntity = null;
    }

    private void UnregisterOtherConstraint()
    {
        var otherEntity = NetworkEntityManager.GetEntity(OtherID);

        if (otherEntity == null)
        {
            return;
        }

        if (!otherEntity.IsRegistered)
        {
            return;
        }

        NetworkEntityManager.IDManager.UnregisterEntity(otherEntity);
    }

    private void GetTrackerEntities()
    {
        var trackerBody = MarrowBody.Cache.Get(Tracker.gameObject);

        if (trackerBody != null && trackerBody.Entity != null)
        {
            BodyEntity = IMarrowEntityExtender.Cache.Get(trackerBody.Entity);
        }

        var otherTrackerBody = MarrowBody.Cache.Get(Tracker.otherTracker.gameObject);

        if (otherTrackerBody != null && otherTrackerBody.Entity != null)
        {
            OtherBodyEntity = IMarrowEntityExtender.Cache.Get(otherTrackerBody.Entity);
        }
    }

    private void LinkTrackerEntities()
    {
        if (BodyEntity == null || OtherBodyEntity == null)
        {
            return;
        }

        BodyEntity.LinkEntity(OtherBodyEntity);
    }

    private void UnlinkTrackerEntities()
    {
        if (BodyEntity == null || OtherBodyEntity == null)
        {
            return;
        }

        BodyEntity.UnlinkEntity(OtherBodyEntity);
    }

    private void OnEntityCreationCatchup(NetworkEntity entity, PlayerID player)
    {
        if (!IsFirst)
        {
            return;
        }

        // Send create message
        var data = ConstraintCreateData.Create(PlayerIDManager.LocalSmallID.Value, null, PointPair);
        data.Point1Id = NetworkEntity.ID;
        data.Point2Id = OtherID;

        ServerManager.SendToClientModule<ConstraintCreateMessage, ConstraintCreateData>(data, NetworkChannel.Reliable, player.PlatformID);
    }

    private void AddDestroySensor()
    {
        _destroySensor = Tracker.gameObject.AddComponent<DestroySensor>();
        _destroySensor.Hook(OnSensorDestroyed);
    }

    private void RemoveDestroySensor()
    {
        if (_destroySensor != null)
        {
            GameObject.Destroy(_destroySensor);
        }
    }

    private void OnSensorDestroyed()
    {
        if (NetworkEntity != null && NetworkEntity.IsRegistered)
        {
            NetworkEntityManager.IDManager.UnregisterEntity(NetworkEntity);
        }

        _destroySensor = null;
    }
}