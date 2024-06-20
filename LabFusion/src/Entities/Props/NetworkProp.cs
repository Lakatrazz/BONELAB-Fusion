using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Data;
using LabFusion.MonoBehaviours;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Entities;

public class NetworkProp : IEntityExtender, IMarrowEntityExtender, IEntityUpdatable, IEntityFixedUpdatable
{
    private NetworkEntity _networkEntity = null;

    private MarrowEntity _marrowEntity = null;

    public NetworkEntity NetworkEntity => _networkEntity;

    public MarrowEntity MarrowEntity => _marrowEntity;

    private MarrowBody[] _bodies = null;
    private PDController[] _pdControllers = null;

    private EntityPose _pose = null;
    private EntityPose _sentPose = null;

    private bool _receivedPose = false;
    private float _lastReceivedTime = 0f;

    private float _lastSentTime = 0f;

    private DestroySensor _destroySensor = null;

    private bool _isCulled = false;

    private List<IEntityComponentExtender> _componentExtenders = null;

    public EntityPose EntityPose => _pose;

    public const float MinMoveMagnitude = 0.005f;
    public const float MinMoveSqrMagnitude = MinMoveMagnitude * MinMoveMagnitude;
    public const float MinMoveAngle = 0.15f;

    public NetworkProp(NetworkEntity networkEntity, MarrowEntity marrowEntity)
    {
        _networkEntity = networkEntity;
        _marrowEntity = marrowEntity;

        InitializeBodies();
        InitializeComponents();

        IMarrowEntityExtender.Cache.Add(marrowEntity, networkEntity);

        OnEntityCull(marrowEntity.IsCulled);

        networkEntity.HookOnRegistered(OnPropRegistered);
        networkEntity.OnEntityUnregistered += OnPropUnregistered;
    }

    private void InitializeBodies()
    {
        _bodies = MarrowEntity.Bodies;

        _pose = new EntityPose(_bodies.Length);
        _sentPose = new EntityPose(_bodies.Length);

        _pdControllers = new PDController[_bodies.Length];

        for (var i = 0; i < _pdControllers.Length; i++)
        {
            _pdControllers[i] = new();
        }

        // Copy current bodies into the entity pose so that they're held up by forces
        CopyBodiesToPose();
        _receivedPose = true;
        _lastReceivedTime = TimeUtilities.TimeSinceStartup;
    }

    private void InitializeComponents()
    {
        _componentExtenders = EntityComponentManager.ApplyComponents(NetworkEntity, MarrowEntity.gameObject);
    }

    public void OnReceivePose(EntityPose pose)
    {
        pose.CopyTo(EntityPose);

        _receivedPose = true;
        _lastReceivedTime = TimeUtilities.TimeSinceStartup;
    }

    private void TeleportToPose()
    {
        for (var i = 0; i < _bodies.Length; i++)
        {
            var body = _bodies[i];

            if (!body.HasRigidbody)
            {
                continue;
            }

            var rigidbody = body._rigidbody;

            if (rigidbody.isKinematic)
            {
                continue;
            }

            var pose = _pose.bodies[i];

            rigidbody.position = pose.position;
            rigidbody.rotation = pose.rotation;
            rigidbody.velocity = pose.velocity;
            rigidbody.angularVelocity = pose.angularVelocity;
        }
    }

    private void CopyBodiesToPose() 
    {
        _receivedPose = false;

        for (var i = 0; i < _bodies.Length; i++)
        {
            var body = _bodies[i];

            if (!body.HasRigidbody)
            {
                continue;
            }

            var pose = _pose.bodies[i];

            var rigidbody = body._rigidbody;

            pose.position = rigidbody.position;
            pose.rotation = rigidbody.rotation;
            pose.velocity = rigidbody.velocity;
            pose.angularVelocity = rigidbody.angularVelocity;
        }
    }

    private void OnPropRegistered(NetworkEntity entity)
    {
        entity.ConnectExtender(this);

        OnReregisterUpdates();

        _destroySensor = MarrowEntity.gameObject.AddComponent<DestroySensor>();
        _destroySensor.Hook(OnSensorDestroyed);
    }

    private void OnPropUnregistered(NetworkEntity entity)
    {
        IMarrowEntityExtender.Cache.Remove(MarrowEntity);

        entity.DisconnectExtender(this);
        _networkEntity = null;
        _marrowEntity = null;

        OnUnregisterUpdates();
    }

    private void OnSensorDestroyed()
    {
        if (NetworkEntity != null && NetworkEntity.IsRegistered)
        {
            NetworkEntityManager.IdManager.UnregisterEntity(NetworkEntity);
        }

        _destroySensor = null;
    }

    public void OnEntityUpdate(float deltaTime)
    {
        if (!NetworkEntity.IsOwner)
        {
            return;
        }

        OnOwnedUpdate();
    }

    private bool HasBodyMoved(int index)
    {
        var currentPose = _pose.bodies[index];
        var sentPose = _sentPose.bodies[index];

        return (sentPose.position - currentPose.position).sqrMagnitude > MinMoveSqrMagnitude || Quaternion.Angle(sentPose.rotation, currentPose.rotation) > MinMoveAngle; 
    }

    private void OnOwnedUpdate()
    {
        CopyBodiesToPose();

        bool isAwake = false;

        for (var i = 0; i < _bodies.Length; i++)
        {
            var body = _bodies[i];

            if (!body.HasRigidbody)
            {
                continue;
            }

            var rb = body._rigidbody;

            // Don't sync kinematic rigidbodies
            if (rb.isKinematic)
            {
                continue;
            }

            if (!rb.IsSleeping() && HasBodyMoved(i))
            {
                isAwake = true;
                break;
            }
        }

        // Update send time
        if (isAwake)
        {
            _lastSentTime = TimeUtilities.TimeSinceStartup;
        }

        // If a rigidbody has not moved within half a second, stop sending
        if (TimeUtilities.TimeSinceStartup - _lastSentTime >= 0.5f)
        {
            return;
        }

        // Send pose
        using var writer = FusionWriter.Create();
        var data = EntityPoseUpdateData.Create(PlayerIdManager.LocalSmallId, NetworkEntity.Id, EntityPose);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.EntityPoseUpdate, writer);
        MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Unreliable, message);

        // Update sent pose
        EntityPose.CopyTo(_sentPose);
    }

    public void OnEntityFixedUpdate(float deltaTime)
    {
        // If we are the owner, or the prop has no owner, return
        if (NetworkEntity.IsOwner || !NetworkEntity.HasOwner)
        {
            return;
        }

        OnReceivedUpdate(deltaTime);
    }

    private void OnReceivedUpdate(float deltaTime)
    {
        // Make sure we actually have a pose
        if (!_receivedPose)
        {
            return;
        }

        // Check if this hasn't received an update in a while
        float timeSinceMessage = TimeUtilities.TimeSinceStartup - _lastReceivedTime;

        if (timeSinceMessage >= 1f)
        {
            return;
        }

        for (var i = 0; i < _bodies.Length; i++)
        {
            OnReceivedRigidbody(deltaTime, timeSinceMessage, i);
        }
    }

    private void OnReceivedRigidbody(float deltaTime, float timeSinceMessage, int index)
    {
        var body = _bodies[index];
        var pdController = _pdControllers[index];

        if (!body.HasRigidbody)
        {
            pdController.Reset();
            return;
        }

        var rigidbody = body._rigidbody;

        // Check if this is kinematic
        // If so, just ignore values
        if (rigidbody.isKinematic)
        {
            pdController.Reset();
            return;
        }

        var pose = EntityPose.bodies[index];

        // Don't over predict
        if (timeSinceMessage <= 0.6f)
        {
            pose.PredictPosition(deltaTime);
        }

        // Add proper forces
        pdController.SavedForce = pdController.GetForce(rigidbody.position, rigidbody.velocity, pose.position, pose.velocity);
        pdController.SavedTorque = pdController.GetTorque(rigidbody.rotation, rigidbody.angularVelocity, pose.rotation, pose.angularVelocity);

        rigidbody.AddForce(pdController.SavedForce, ForceMode.Acceleration);
        rigidbody.AddTorque(pdController.SavedTorque, ForceMode.Acceleration);
    }

    public void OnEntityCull(bool isInactive)
    {
        // Culled
        if (isInactive)
        {
            OnUnregisterUpdates();
            return;
        }

        // Unculled
        OnReregisterUpdates();

        if (!NetworkEntity.IsOwner && NetworkEntity.HasOwner)
        {
            TeleportToPose();
        }
    }

    private void OnReregisterUpdates()
    {
        OnUnregisterUpdates();

        NetworkEntityManager.UpdateManager.Register(this);
        NetworkEntityManager.FixedUpdateManager.Register(this);
    }

    private void OnUnregisterUpdates()
    {
        NetworkEntityManager.UpdateManager.Unregister(this);
        NetworkEntityManager.FixedUpdateManager.Unregister(this);
    }
}
