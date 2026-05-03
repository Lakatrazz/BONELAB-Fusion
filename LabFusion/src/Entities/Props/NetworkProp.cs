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

    private float _ownerSleepElapsed = 0f;

    private DestroySensor _destroySensor = null;

    public bool IsSleeping { get; private set; } = false;
    public bool IsFrozen { get; private set; } = false;

    public bool InitialCull { get; private set; } = false;

    public bool IsCulled { get; private set; } = false;

    public bool IsCulledForOwner { get; private set; } = false;

    private HashSet<IEntityComponentExtender> _componentExtenders = null;

    /// <summary>
    /// The prop's local pose captured so that it can be sent to other clients.
    /// This is captured on every network tick given by <see cref="NetworkTickManager.IsTickThisFrame"/> rather than every frame for performance.
    /// <para>Only valid if <see cref="HasCapturedPose"/> is true.</para>
    /// </summary>
    public EntityPose CapturedPose { get; private set; } = null;

    /// <summary>
    /// The last <see cref="CapturedPose"/> that was sent to other clients. 
    /// This is useful for comparing if the prop has moved and needs to send data again.
    /// <para>Only valid if <see cref="HasCapturedPose"/> is true.</para>
    /// </summary>
    public EntityPose SentPose { get; private set; } = null;

    /// <summary>
    /// Whether the prop's local pose has been captured in <see cref="CapturedPose"/> to be sent to other clients.
    /// </summary>
    public bool HasCapturedPose { get; private set; } = false;

    /// <summary>
    /// The pose in use right before the latest pose was received from the prop's owner.
    /// Used for interpolation to smooth out network latency.
    /// <para>Only valid if <see cref="HasReceivedPose"/> is true.</para>
    /// </summary>
    public EntityPose LastReceivedPose { get; private set; } = null;

    /// <summary>
    /// The latest pose received from the prop's owner.
    /// This is the pose that prediction is based on, and will be used to replicate the prop locally if the owner stops sending messages.
    /// <para>Only valid if <see cref="HasReceivedPose"/> is true.</para>
    /// </summary>
    public EntityPose ReceivedPose { get; private set; } = null;

    /// <summary>
    /// The <see cref="ReceivedPose"/>, but predicted right after being received based on the network latency.
    /// <para>Only valid if <see cref="HasReceivedPose"/> is true.</para>
    /// </summary>
    public EntityPose PredictedPose { get; private set; } = null;

    /// <summary>
    /// The current interpolated pose between <see cref="LastReceivedPose"/> and <see cref="PredictedPose"/>.
    /// Additional prediction based on velocity is applied on top to simulate the object's travel while waiting for the next pose.
    /// This is the pose actively used to replicate props locally per physics update.
    /// <para>Only valid if <see cref="HasReceivedPose"/> is true.</para>
    /// </summary>
    public EntityPose InterpolatedPose { get; private set; } = null;

    /// <summary>
    /// Whether an updated pose has been received from the prop's owner and applied to <see cref="ReceivedPose"/>.
    /// </summary>
    public bool HasReceivedPose { get; private set; } = false;

    /// <summary>
    /// The time in seconds since we received an EntityPose from the prop's owner.
    /// </summary>
    public float TimeSinceReceivedPose { get; private set; } = 0f;

    /// <summary>
    /// The percent from 0 to 1 of interpolation from <see cref="LastReceivedPose"/> to <see cref="PredictedPose"/>.
    /// </summary>
    public float InterpolationPercent { get; private set; } = 0f;

    public const float MinMoveMagnitude = 0.005f;
    public const float MinMoveSqrMagnitude = MinMoveMagnitude * MinMoveMagnitude;
    public const float MinMoveAngle = 0.15f;

    public const float SleepTimer = 0.5f;

    private float _ownedTickDeltaTime = 0f;

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
        ResetPoseState();

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

        CapturedPose = new(bodyCount);
        SentPose = new(bodyCount);

        LastReceivedPose = new(bodyCount);
        ReceivedPose = new(bodyCount);
        PredictedPose = new(bodyCount);
        InterpolatedPose = new(bodyCount);

        HoldReceivedPose();
    }

    private void InitializeComponents()
    {
        _componentExtenders = EntityComponentManager.ApplyComponents(NetworkEntity, MarrowEntity.gameObject);
    }

    public void ReceivePose(EntityPose pose)
    {
        if (HasReceivedPose)
        {
            InterpolatedPose.WriteTo(LastReceivedPose);
        }
        else
        {
            pose.WriteTo(LastReceivedPose);
        }

        pose.WriteTo(ReceivedPose);

        pose.WriteTo(PredictedPose);

        float predictionTime = MathF.Min(TimeSinceReceivedPose, NetworkTickManager.MaxPredictionTime);

        PredictedPose.Predict(predictionTime);

        LastReceivedPose.WriteTo(InterpolatedPose);

        RefreshReceivedState();
    }

    public void HoldReceivedPose()
    {
        WriteToPose(ReceivedPose);

        ResyncReceivedPose();

        RefreshReceivedState();
    }

    public void ResyncReceivedPose()
    {
        ReceivedPose.WriteTo(LastReceivedPose);
        ReceivedPose.WriteTo(PredictedPose);
        ReceivedPose.WriteTo(InterpolatedPose);
    }

    public void ReceiveCullStatus(bool isCulled)
    {
        IsCulledForOwner = isCulled;
    }

    private void RefreshReceivedState()
    {
        Unfreeze();

        TimeSinceReceivedPose = 0f;
        HasReceivedPose = true;
    }

    private void ResetPoseState()
    {
        TimeSinceReceivedPose = 0f;
        HasReceivedPose = false;
        HasCapturedPose = false;
    }

    public void TeleportToPose()
    {
        if (!HasReceivedPose)
        {
            return;
        }

        TeleportToPose(InterpolatedPose);
    }

    public void TeleportToPose(EntityPose pose)
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

            var bodyPose = pose.Bodies[i];

            transform.position = bodyPose.Position;
            transform.rotation = bodyPose.Rotation;
            rigidbody.velocity = bodyPose.Velocity;
            rigidbody.angularVelocity = bodyPose.AngularVelocity;
        }
    }

    public void CapturePose()
    {
        WriteToPose(CapturedPose);
        HasCapturedPose = true;
    }

    public void WriteToPose(EntityPose pose)
    {
        for (var i = 0; i < _bodies.Length; i++)
        {
            var body = _bodies[i];

            if (!body.HasRigidbody)
            {
                continue;
            }

            var bodyPose = pose.Bodies[i];

            var rigidbody = body._rigidbody;

            bodyPose.ReadFrom(rigidbody);
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
        var currentPose = CapturedPose.Bodies[index];
        var sentPose = SentPose.Bodies[index];

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
        _ownedTickDeltaTime += deltaTime;

        // Only update if on our tick rate
        if (!NetworkTickManager.IsTickThisFrame)
        {
            return;
        }

        float totalDeltaTime = _ownedTickDeltaTime;
        _ownedTickDeltaTime = 0f;

        OnOwnedTick(totalDeltaTime);
    }

    private void OnOwnedTick(float deltaTime)
    {
        // Capture the current pose to read from
        CapturePose();

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
        bool fallingAsleep = !wasSleeping && sleeping;

        if (fallingAsleep)
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

        // Prop is awake, we can send the pose
        SendEntityPose(CommonMessageRoutes.UnreliableToOtherClients);
    }

    private void SendEntityPose(MessageRoute route)
    {
        var data = new EntityPoseUpdateData()
        {
            Entity = new(NetworkEntity),
            Pose = CapturedPose,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.EntityPoseUpdate, route);

        CapturedPose.WriteTo(SentPose);
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
        OnProcessReceivedPose(deltaTime);

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

    private void OnProcessReceivedPose(float deltaTime)
    {
        if (IsSleeping)
        {
            return;
        }

        if (!HasReceivedPose)
        {
            return;
        }

        float unscaledDeltaTime = deltaTime / TimeReferences.SafeTimeScale;

        TimeSinceReceivedPose += unscaledDeltaTime;

        InterpolationPercent = ManagedMathf.Clamp01(TimeSinceReceivedPose / NetworkTickManager.LinearInterpolationLength);

        InterpolatedPose.Interpolate(LastReceivedPose, PredictedPose, InterpolationPercent);

        float predictionTime = MathF.Min(TimeSinceReceivedPose, NetworkTickManager.MaxPredictionTime);

        InterpolatedPose.PredictFrom(predictionTime, ReceivedPose);
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
        if (!HasReceivedPose)
        {
            return;
        }

        // Check if this hasn't received an update in a while
        if (TimeSinceReceivedPose >= SleepTimer)
        {
            ResyncReceivedPose();
            TeleportToPose(ReceivedPose);
            Freeze();
            return;
        }

        _spdState.CalculatingForces = true;

        for (var i = 0; i < _bodies.Length; i++)
        {
            OnPreCalculateRigidbodyForces(i);
        }
    }

    private void OnPreCalculateRigidbodyForces(int index)
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

        var bodyPose = InterpolatedPose.Bodies[index];

        _spdState.SetLinearInput(index, rigidbody.position.ToNumericsVector3(), rigidbody.velocity.ToNumericsVector3(), bodyPose.Position.ToNumericsVector3(), bodyPose.Velocity.ToNumericsVector3());
        _spdState.EnabledForces[index] = true;

        // Don't add torque if rotation is frozen
        if (!rigidbody.freezeRotation)
        {
            _spdState.SetAngularInput(index, rigidbody.rotation.ToNumericsQuaternion(), rigidbody.angularVelocity.ToNumericsVector3(), bodyPose.Rotation.ToNumericsQuaternion(), bodyPose.AngularVelocity.ToNumericsVector3());
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

        bool isOwner = NetworkEntity.IsOwner;

        if (isOwner)
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
