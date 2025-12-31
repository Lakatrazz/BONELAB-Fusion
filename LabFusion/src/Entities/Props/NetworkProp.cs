using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Data;
using LabFusion.Marrow.Utilities;
using LabFusion.MonoBehaviours;
using LabFusion.Network;
using LabFusion.Player;
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

    public bool IsSleeping { get; private set; } = false;

    public EntityFreezer Freezer { get; } = new();

    private static int _globalSleepOffset = 0;
    private int _sleepFrameOffset = 0;
    private const int _sleepCheckInterval = 20;

    private bool _isCulled = false;

    private HashSet<IEntityComponentExtender> _componentExtenders = null;

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

        networkEntity.HookOnRegistered(OnPropRegistered);
        networkEntity.OnEntityUnregistered += OnPropUnregistered;
    }

    private void OnEntityOwnershipTransfer(NetworkEntity entity, PlayerID player)
    {
        if (player.IsMe)
        {
            Unfreeze();
        }

        OnReregisterUpdates();
    }

    private void OnEntityDataCatchup(NetworkEntity entity, PlayerID player)
    {
        if (entity.IsOwner)
        {
            SendEntityPose(new MessageRoute(player.SmallID, NetworkChannel.Reliable));
        }
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
        UpdateReceiveTime();
    }

    private void InitializeComponents()
    {
        _componentExtenders = EntityComponentManager.ApplyComponents(NetworkEntity, MarrowEntity.gameObject);
    }

    private int _receivedFrame = 0;

    public void OnReceivePose(EntityPose pose)
    {
        var frameCount = TimeUtilities.FrameCount;

        if (_receivedFrame == frameCount)
        {
            return;
        }

        _receivedFrame = frameCount;

        pose.CopyTo(EntityPose);

        UpdateReceiveTime();
    }

    private void UpdateReceiveTime()
    {
        Unfreeze();

        _receivedPose = true;
        _lastReceivedTime = TimeUtilities.TimeSinceStartup;
    }

    public void ResetPrediction()
    {
        EntityPose.ResetPrediction();
    }

    public void TeleportToPose()
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

            var pose = EntityPose.Bodies[i];

            rigidbody.position = pose.PredictedPosition;
            rigidbody.rotation = pose.Rotation;
            rigidbody.velocity = pose.Velocity;
            rigidbody.angularVelocity = pose.AngularVelocity;
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

            var pose = _pose.Bodies[i];

            var rigidbody = body._rigidbody;

            pose.Position = rigidbody.position;
            pose.Rotation = rigidbody.rotation;
            pose.Velocity = rigidbody.velocity;
            pose.AngularVelocity = rigidbody.angularVelocity;

            pose.ResetPrediction();
        }
    }

    private bool IsMarrowEntityDestroyed()
    {
        return MarrowEntity == null || MarrowEntity.IsDestroyed || MarrowEntity.IsDespawned;
    }

    private void OnPropRegistered(NetworkEntity entity)
    {
        // Make sure the entity wasn't destroyed while waiting for registration
        if (IsMarrowEntityDestroyed())
        {
            OnSensorDestroyed();
            return;
        }

        entity.ConnectExtender(this);

        entity.OnEntityOwnershipTransfer += OnEntityOwnershipTransfer;
        entity.OnEntityDataCatchup += OnEntityDataCatchup;

        OnReregisterUpdates();

        AddDestroySensor();

        // Cull the entity if it needs to be
        OnEntityCull(MarrowEntity.IsCulled);

        // Cycle sleep offset
        _sleepFrameOffset = _globalSleepOffset;
        _globalSleepOffset++;

        if (_globalSleepOffset >= 30)
        {
            _globalSleepOffset = 0;
        }

        // Invoke ready callback
        _onReadyCallback?.InvokeSafe("executing NetworkProp.OnReadyCallback");
        _onReadyCallback = null;
    }

    private void OnPropUnregistered(NetworkEntity entity)
    {
        IMarrowEntityExtender.Cache.Remove(MarrowEntity);

        entity.DisconnectExtender(this);

        entity.OnEntityOwnershipTransfer -= OnEntityOwnershipTransfer;
        entity.OnEntityDataCatchup -= OnEntityDataCatchup;

        _networkEntity = null;
        _marrowEntity = null;

        Unfreeze();

        RemoveDestroySensor();

        OnUnregisterUpdates();
    }

    private void AddDestroySensor()
    {
        _destroySensor = MarrowEntity.gameObject.AddComponent<DestroySensor>();
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
#if DEBUG
            FusionLogger.Log($"Unregistered prop at ID {NetworkEntity.ID} after the GameObject was destroyed.");
#endif

            NetworkEntityManager.IDManager.UnregisterEntity(NetworkEntity);
        }

        _destroySensor = null;
    }

    public void OnEntityUpdate(float deltaTime)
    {
        // OnEntityUpdate is only registered when we own the prop
        OnOwnedUpdate();
    }

    private bool HasBodyMoved(int index)
    {
        var currentPose = _pose.Bodies[index];
        var sentPose = _sentPose.Bodies[index];

        return (sentPose.Position - currentPose.Position).sqrMagnitude > MinMoveSqrMagnitude || Quaternion.Angle(sentPose.Rotation, currentPose.Rotation) > MinMoveAngle; 
    }

    public void Freeze()
    {
        IsSleeping = true;
        Freezer.Freeze(_bodies); 
    }

    public void Unfreeze() 
    {
        IsSleeping = false;
        Freezer.Unfreeze(); 
    }

    private bool CheckOwnedSleeping()
    {
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
                return false;
            }
        }

        return true;
    }

    private void OnOwnedUpdate()
    {
        // If we were sleeping last frame, only check so often
        if (IsSleeping && !TimeUtilities.IsMatchingFrame(_sleepCheckInterval, _sleepFrameOffset))
        {
            return;
        }

        // Copy all body positions to current position
        CopyBodiesToPose();

        // Update sleeping
        bool sleeping = CheckOwnedSleeping();

        if (!sleeping)
        {
            IsSleeping = false;
            _lastSentTime = TimeUtilities.TimeSinceStartup;
        }

        // If a rigidbody has not moved within half a second, stop sending
        if (TimeUtilities.TimeSinceStartup - _lastSentTime >= 0.5f)
        {
            IsSleeping = true;
            SendEntityPose(CommonMessageRoutes.ReliableToOtherClients);
            return;
        }

        SendEntityPose(CommonMessageRoutes.UnreliableToOtherClients);
    }

    private void SendEntityPose(MessageRoute route)
    {
        var data = new EntityPoseUpdateData()
        {
            Entity = new(NetworkEntity),
            Pose = EntityPose,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.EntityPoseUpdate, route);

        EntityPose.CopyTo(_sentPose);
    }

    public void OnEntityFixedUpdate(float deltaTime)
    {
        // OnEntityFixedUpdate is only registered when we do not own the prop
        OnReceivedUpdate(deltaTime);
    }

    private void OnReceivedUpdate(float deltaTime)
    {
        // Make sure the prop isn't sleeping
        if (IsSleeping)
        {
            return;
        }

        // Make sure we actually have a pose
        if (!_receivedPose)
        {
            return;
        }

        // Check if this hasn't received an update in a while
        float timeSinceMessage = TimeUtilities.TimeSinceStartup - _lastReceivedTime;

        if (timeSinceMessage >= 1f)
        {
            ResetPrediction();
            TeleportToPose();
            Freeze();
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

        var pose = EntityPose.Bodies[index];

        // Don't over predict
        if (timeSinceMessage <= 0.6f)
        {
            pose.PredictPosition(deltaTime);
        }

        // Add proper forces
        pdController.SavedForce = pdController.GetForce(rigidbody.position, rigidbody.velocity, pose.PredictedPosition, pose.Velocity);
        pdController.SavedTorque = pdController.GetTorque(rigidbody.rotation, rigidbody.angularVelocity, pose.Rotation, pose.AngularVelocity);

        rigidbody.AddForce(pdController.SavedForce, ForceMode.Acceleration);
        rigidbody.AddTorque(pdController.SavedTorque, ForceMode.Acceleration);
    }

    public void OnEntityCull(bool isInactive)
    {
        _isCulled = isInactive;

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

        if (_isCulled)
        {
            return;
        }

        if (NetworkEntity.IsOwner)
        {
            NetworkEntityManager.UpdatableManager.UpdateManager.Register(this);
        }
        else
        {
            NetworkEntityManager.UpdatableManager.FixedUpdateManager.Register(this);
        }
    }

    private void OnUnregisterUpdates()
    {
        NetworkEntityManager.UpdatableManager.UpdateManager.Unregister(this);
        NetworkEntityManager.UpdatableManager.FixedUpdateManager.Unregister(this);
    }

    private Action _onReadyCallback = null;

    public void HookOnReady(Action callback)
    {
        if (MarrowEntity != null && NetworkEntity.IsRegistered)
        {
            callback();
        }
        else
        {
            _onReadyCallback += callback;
        }
    }
}
