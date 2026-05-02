using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Math;
using LabFusion.MonoBehaviours;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Entities;

public class NetworkProp : IEntityExtender, IMarrowEntityExtender, IEntityUpdatable, IParallelFixedUpdatable
{
    private NetworkEntity _networkEntity = null;

    private MarrowEntity _marrowEntity = null;

    public NetworkEntity NetworkEntity => _networkEntity;

    public MarrowEntity MarrowEntity => _marrowEntity;

    private MarrowBody[] _bodies = null;
    private SPDState _spdState = null;

    private EntityPose _pose = null;
    private EntityPose _sentPose = null;

    private bool _receivedPose = false;
    private float _lastReceivedTime = 0f;

    private float _ownerSleepElapsed = 0f;

    private DestroySensor _destroySensor = null;

    public bool IsSleeping { get; private set; } = false;
    public bool IsFrozen { get; private set; } = false;

    private static int _globalSleepOffset = 0;
    private int _sleepFrameOffset = 0;
    private const int _sleepCheckInterval = 20;

    public bool InitialCull { get; private set; } = false;

    public bool IsCulled { get; private set; } = false;

    public bool IsCulledForOwner { get; private set; } = false;

    private HashSet<IEntityComponentExtender> _componentExtenders = null;

    public EntityPose EntityPose => _pose;

    public const float MinMoveMagnitude = 0.005f;
    public const float MinMoveSqrMagnitude = MinMoveMagnitude * MinMoveMagnitude;
    public const float MinMoveAngle = 0.15f;

    public const float SleepTimer = 0.5f;

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
        _receivedPose = false;

        if (entity.IsOwner)
        {
            Unfreeze();

            IsCulledForOwner = IsCulled;
            SendCullStatus(IsCulled, CommonMessageRoutes.ReliableToOtherClients);

            ResetDrag();
        }
        else
        {
            ClearDrag();
        }

        OnReregisterUpdates();
    }

    private void OnEntityDataCatchup(NetworkEntity entity, PlayerID player)
    {
        var route = new MessageRoute(player.SmallID, NetworkChannel.Reliable);

        SendEntityPose(route);
        SendCullStatus(IsCulled, route);
    }

    private void InitializeBodies()
    {
        _bodies = MarrowEntity.Bodies;
        int bodyCount = _bodies.Length;

        _spdState = new SPDState(bodyCount);
        _pose = new EntityPose(bodyCount);
        _sentPose = new EntityPose(bodyCount);

        // Copy current bodies into the entity pose so that they're held up by forces
        CopyBodiesToPose();
        UpdateReceiveTime();
    }

    private void InitializeComponents()
    {
        _componentExtenders = EntityComponentManager.ApplyComponents(NetworkEntity, MarrowEntity.gameObject);
    }

    public void OnReceivePose(EntityPose pose)
    {
        pose.CopyTo(EntityPose);

        UpdateReceiveTime();
    }

    public void OnReceiveCullStatus(bool isCulled)
    {
        IsCulledForOwner = isCulled;
    }

    private void UpdateReceiveTime()
    {
        Unfreeze();

        _receivedPose = true;

        float timeSinceStartup = TimeReferences.TimeSinceStartup;

        float timeSinceMessage = timeSinceStartup - _lastReceivedTime;

        EntityPose.Predict(ManagedMathf.Clamp01(timeSinceMessage));

        _lastReceivedTime = timeSinceStartup;
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

            var transform = rigidbody.transform;

            var pose = EntityPose.Bodies[i];

            transform.position = pose.PredictedPosition;
            transform.rotation = pose.Rotation;
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

            pose.ReadFrom(rigidbody);

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

        InitialCull = true;

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
        Unfreeze();

        IMarrowEntityExtender.Cache.Remove(MarrowEntity);

        entity.DisconnectExtender(this);

        entity.OnEntityOwnershipTransfer -= OnEntityOwnershipTransfer;
        entity.OnEntityDataCatchup -= OnEntityDataCatchup;

        _networkEntity = null;
        _marrowEntity = null;

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
        OnOwnedUpdate(deltaTime);
    }

    private bool HasBodyMoved(int index)
    {
        var currentPose = _pose.Bodies[index];
        var sentPose = _sentPose.Bodies[index];

        return (sentPose.Position - currentPose.Position).sqrMagnitude > MinMoveSqrMagnitude || Quaternion.Angle(sentPose.Rotation, currentPose.Rotation) > MinMoveAngle; 
    }

    private void ResetDrag()
    {
        if (MarrowEntity == null)
        {
            return;
        }

        foreach (var body in _bodies)
        {
            float drag = body._defaultRigidbodyInfo.drag;
            float angularDrag = body._defaultRigidbodyInfo.angularDrag;

            body._cachedRigidbodyInfo.drag = drag;
            body._cachedRigidbodyInfo.angularDrag = angularDrag;

            var rb = body._rigidbody;

            if (rb == null)
            {
                continue;
            }

            rb.drag = drag;
            rb.angularDrag = angularDrag;
        }
    }

    private void ClearDrag()
    {
        if (MarrowEntity == null)
        {
            return;
        }

        foreach (var body in _bodies)
        {
            float drag = 0f;
            float angularDrag = 0f;

            body._cachedRigidbodyInfo.drag = drag;
            body._cachedRigidbodyInfo.angularDrag = angularDrag;

            var rb = body._rigidbody;

            if (rb == null)
            {
                continue;
            }

            rb.drag = drag;
            rb.angularDrag = angularDrag;
        }
    }

    public void Freeze()
    {
        IsSleeping = true;

        if (IsFrozen)
        {
            return;
        }

        IsFrozen = true;

        if (MarrowEntity != null)
        {
            MarrowEntity.Hibernate(true, MarrowEntity.HibernationSources.Default);
        }
    }

    public void Unfreeze() 
    {
        IsSleeping = false;

        if (!IsFrozen)
        {
            return;
        }

        IsFrozen = false;

        if (MarrowEntity != null)
        {
            MarrowEntity.Hibernate(false, MarrowEntity.HibernationSources.Default);
        }
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

    private void OnOwnedUpdate(float deltaTime)
    {
        // If we were sleeping last frame, only check so often
        if (IsSleeping && !TimeReferences.IsMatchingFrame(_sleepCheckInterval, _sleepFrameOffset))
        {
            return;
        }

        // Copy all body positions to current position
        CopyBodiesToPose();

        bool wasSleeping = IsSleeping;
        bool sleeping = CheckOwnedSleeping();

        // No change in sleep state
        if (wasSleeping && sleeping)
        {
            return;
        }

        // Woke up
        if (wasSleeping && !sleeping)
        {
            _ownerSleepElapsed = 0f;
            IsSleeping = false;
        }
        
        // Starting to sleep
        if (!wasSleeping && sleeping)
        {
            _ownerSleepElapsed += deltaTime;

            // Fully fell asleep
            if (_ownerSleepElapsed >= SleepTimer)
            {
                IsSleeping = sleeping;
                _ownerSleepElapsed = 0f;

                SendEntityPose(CommonMessageRoutes.ReliableToOtherClients);
                return;
            }
        }

        // Only send if on our tick rate
        if (!NetworkTickManager.IsTickThisFrame)
        {
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

    private void SendCullStatus(bool isCulled, MessageRoute route)
    {
        var data = new EntityCullStatusData()
        {
            Entity = new(NetworkEntity),
            IsCulled = isCulled,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.EntityCullStatus, route);
    }

    void IParallelFixedUpdatable.OnPreParallelFixedUpdate(float deltaTime)
    {
        OnPreCalculateForces(deltaTime);
    }

    void IParallelFixedUpdatable.OnParallelFixedUpdate(float deltaTime)
    {
        OnCalculateForces(deltaTime);
    }

    void IParallelFixedUpdatable.OnPostParallelFixedUpdate(float deltaTime)
    {
        OnApplyForces(deltaTime);
    }

    private void OnPreCalculateForces(float deltaTime)
    {
        _spdState.CalculatingForces = false;

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
        float timeSinceMessage = TimeReferences.TimeSinceStartup - _lastReceivedTime;

        if (timeSinceMessage >= SleepTimer)
        {
            ResetPrediction();
            TeleportToPose();
            Freeze();
            return;
        }

        _spdState.CalculatingForces = true;

        for (var i = 0; i < _bodies.Length; i++)
        {
            OnPreCalculateRigidbodyForces(deltaTime, timeSinceMessage, i);
        }
    }

    private void OnPreCalculateRigidbodyForces(float deltaTime, float timeSinceMessage, int index)
    {
        _spdState.EnabledForces[index] = false;
        _spdState.EnabledTorques[index] = false;

        var body = _bodies[index];

        if (!body.HasRigidbody)
        {
            return;
        }

        var rigidbody = body._rigidbody;

        // Check if this is kinematic
        // If so, just ignore values
        if (rigidbody.isKinematic)
        {
            return;
        }

        var pose = EntityPose.Bodies[index];

        // Don't over predict
        if (timeSinceMessage <= 0.6f)
        {
            pose.PredictPosition(deltaTime);
        }

        _spdState.SetLinearInput(index, rigidbody.position.ToNumericsVector3(), rigidbody.velocity.ToNumericsVector3(), pose.PredictedPosition.ToNumericsVector3(), pose.Velocity.ToNumericsVector3());
        _spdState.EnabledForces[index] = true;

        // Don't add torque if rotation is frozen
        if (!rigidbody.freezeRotation)
        {
            _spdState.SetAngularInput(index, rigidbody.rotation.ToNumericsQuaternion(), rigidbody.angularVelocity.ToNumericsVector3(), pose.Rotation.ToNumericsQuaternion(), pose.AngularVelocity.ToNumericsVector3());
            _spdState.EnabledTorques[index] = true;
        }
    }

    private void OnCalculateForces(float deltaTime)
    {
        if (!_spdState.CalculatingForces)
        {
            return;
        }

        for (var i = 0; i < _spdState.Count; i++)
        {
            OnCalculateRigidbodyForces(deltaTime, i);
        }
    }

    private void OnCalculateRigidbodyForces(float deltaTime, int index)
    {
        if (_spdState.EnabledForces[index])
        {
            var force = SPDController.CalculateForce(_spdState.Positions[index], _spdState.Velocities[index], _spdState.TargetPositions[index], _spdState.TargetVelocities[index], deltaTime);
            _spdState.Forces[index] = force;
        }

        if (_spdState.EnabledTorques[index])
        {
            var torque = SPDController.CalculateTorque(_spdState.Rotations[index], _spdState.AngularVelocities[index], _spdState.TargetRotations[index], _spdState.TargetAngularVelocities[index], deltaTime);
            _spdState.Torques[index] = torque;
        }
    }

    private void OnApplyForces(float deltaTime)
    {
        if (!_spdState.CalculatingForces)
        {
            return;
        }

        for (var i = 0; i < _bodies.Length; i++)
        {
            OnApplyRigidbodyForces(deltaTime, i);
        }
    }

    private void OnApplyRigidbodyForces(float deltaTime, int index)
    {
        bool enabledForce = _spdState.EnabledForces[index];
        bool enabledTorque = _spdState.EnabledTorques[index];

        if (!enabledForce && !enabledTorque)
        {
            return;
        }

        var body = _bodies[index];

        var rigidbody = body._rigidbody;

        if (enabledForce)
        {
            var force = _spdState.Forces[index].ToUnityVector3();
            rigidbody.AddForce(force, ForceMode.Acceleration);
        }

        if (enabledTorque)
        {
            var torque = _spdState.Torques[index].ToUnityVector3();
            rigidbody.AddTorque(torque, ForceMode.Acceleration);
        }
    }

    public void OnEntityCull(bool isInactive)
    {
        IsCulled = isInactive;

        bool isOwner = NetworkEntity.IsOwner;
        bool hasOwner = NetworkEntity.HasOwner;

        if (isOwner)
        {
            IsCulledForOwner = IsCulled;
            SendCullStatus(IsCulled, CommonMessageRoutes.ReliableToOtherClients);
        }

        // Culled
        if (isInactive)
        {
            OnUnregisterUpdates();
            return;
        }

        // Unculled
        OnReregisterUpdates();

        if (!isOwner && hasOwner)
        {
            TeleportToPose();

            TryTakeUncullOwnership();
        }
    }

    private void TryTakeUncullOwnership()
    {
        if (!InitialCull)
        {
            return;
        }

        if (IsCulled)
        {
            return;
        }

        if (!IsCulledForOwner)
        {
            return;
        }

        if (NetworkEntity.IsOwnerLocked)
        {
            return;
        }

        NetworkEntityManager.TakeOwnership(NetworkEntity);
    }

    private void OnReregisterUpdates()
    {
        OnUnregisterUpdates();

        if (IsCulled)
        {
            return;
        }

        if (NetworkEntity.IsOwner)
        {
            NetworkEntityManager.UpdatableManager.UpdateManager.Register(this);
        }
        else
        {
            ParallelManager.UpdatableManager.FixedUpdateManager.Register(this);
        }
    }

    private void OnUnregisterUpdates()
    {
        NetworkEntityManager.UpdatableManager.UpdateManager.Unregister(this);
        ParallelManager.UpdatableManager.FixedUpdateManager.Unregister(this);
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
