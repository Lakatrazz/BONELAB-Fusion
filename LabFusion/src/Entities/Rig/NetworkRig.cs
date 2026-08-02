using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Marrow.Extensions;
using LabFusion.Marrow.Messages;
using LabFusion.Marrow.Rig;
using LabFusion.Marrow.Data;
using LabFusion.Math;
using LabFusion.Math.Numerics;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Entities;

public class NetworkRig : IEntityExtender, IMarrowEntityExtender
{
    public static readonly FusionComponentCache<RigManager, NetworkRig> Cache = new();

    public static readonly FusionComponentCache<WorldGrip, NetworkRig> WorldGripCache = new();

    public bool IsRegistered { get; private set; } = false;

    public bool IsRigAssigned { get; private set; } = false;

    public NetworkEntity NetworkEntity { get; private set; } = null;

    public MarrowEntity MarrowEntity { get; private set; } = null;

    public EntityIgnorer Ignorer { get; private set; } = null;

    public RigRefs RigRefs { get; private set; } = null;

    public RigSkeleton RigSkeleton { get; private set; } = null;

    public RigPose RigPose { get; private set; } = null;

    public RigArt RigArt { get; private set; } = null;

    public RigPhysics RigPhysics { get; private set; } = null;

    public RigGrabber RigGrabber { get; private set; } = null;

    public RigAvatarSetter AvatarSetter { get; private set; } = null;

    public EntityPoseReceiver PoseReceiver { get; private set; } = new();

    public ManagedTransform[] SmoothTrackedTransforms { get; private set; } = null;

    public bool HasRig => RigRefs != null && RigRefs.IsValid;

    public event Action OnBeforeTeleportToPose;

    public event Action OnAfterTeleportToPose;

    public RigComponentManager ComponentManager { get; private set; } = new();

    public bool IsCulled
    {
        get => _isCulled;
        private set
        {
            _isCulled = value;

            OnHiddenChanged();
        }
    }

    public bool ForceHide
    {
        get => _forceHide;
        set
        {
            _forceHide = value;

            OnHiddenChanged();
        }
    }

    public bool IsHidden => ForceHide || IsCulled;

    public event Action<bool> HiddenChanged;

    public float Health { get; private set; } = 10f;

    public float MaxHealth { get; private set; } = 10f;

    private bool _isCulled = false;
    private bool _forceHide = false;

    private Il2CppSystem.Action _onAvatarSwappedAction = null;

    private Il2CppSystem.Action _onJumpAction = null;

    private Action _onReadyCallback = null;

    private readonly EntityPose _receivedEntityPose = new(1);

    private readonly Queue<PhysicsRigStateData> _physicsRigStates = new();

    public NetworkRig()
    {
        // The only synced body is the pelvis, so its initialized with one
        PoseReceiver.InitializePoses(1);

        // Create the AvatarSetter
        AvatarSetter = new();
    }

    public void ConnectToEntity(NetworkEntity networkEntity)
    {
        NetworkEntity = networkEntity;
        networkEntity.ConnectExtender(this);

        AvatarSetter.SetEntity(networkEntity);
    }

    public void DisconnectFromEntity() 
    {
        NetworkEntity?.DisconnectExtender(this);
    }

    public void AssignRig(RigManager rigManager)
    {
        OnRigAssigned(rigManager);

        IsRigAssigned = true;
    }

    public void UnassignRig()
    {
        if (!IsRigAssigned)
        {
            return;
        }

        IsRigAssigned = false;

        UnregisterComponents();

        UnhookRig();

        RigRefs = null;

        RigPose = null;
        PoseReceiver.ClearPoseState();

        NetworkEntity?.ClearDataCaughtUpPlayers();
    }

    public void HookOnReady(Action callback)
    {
        if (HasRig)
        {
            callback();
        }
        else
        {
            _onReadyCallback += callback;
        }
    }

    public void OnEntityCull(bool isInactive) => IsCulled = isInactive;

    public void OnExtenderRegistered()
    {
        IsRegistered = true;

        NetworkEntity.EntityDataCatchingUp += OnEntityDataCatchup;
        NetworkEntity.EntityOwnershipTransferred += OnEntityOwnershipTransfer;
    }

    public void OnExtenderUnregistered()
    {
        IsRegistered = false;

        NetworkEntity.EntityOwnershipTransferred -= OnEntityOwnershipTransfer;

        UnassignRig();
    }

    public void OnNewAvatarReady()
    {
        if (!NetworkEntity.IsOwner)
        {
            return;
        }

        RelayAvatar(CommonMessageRoutes.ReliableToOtherClients);
    }

    public void RelayAvatar(MessageRoute route)
    {
        if (!HasRig || !NetworkEntity.IsOwner)
        {
            return;
        }

        var rigManager = RigRefs.RigManager;
        var avatar = rigManager.avatar;

        var stats = new SerializedAvatarStats(avatar);
        var barcode = rigManager.AvatarCrate.Barcode.ID;

        var data = new RigAvatarData()
        {
            RigReference = new(NetworkEntity),
            Stats = stats,
            Barcode = barcode,
        };

        MessageRelay.RelayModule<RigAvatarMessage, RigAvatarData>(data, route);
    }

    public void OnPoseReceived(RigPose pose)
    {
        // If we don't have a rig yet, don't store the pose
        if (!HasRig)
        {
            return;
        }

        RigPose = pose;

        bool hadReceivedPose = PoseReceiver.HasReceivedPose;

        pose.PelvisPose.WriteTo(_receivedEntityPose.Bodies[0]);

        PoseReceiver.ReceivePose(_receivedEntityPose);

        // Teleport to the pose if this is our first
        if (!hadReceivedPose)
        {
            TeleportToPose();
            CopyPosePointsToSmoothPoints();
        }

        PoseReceiver.RefreshPoseState();

        // Update the health
        Health = pose.Health;
        MaxHealth = pose.MaxHealth;

        RigSkeleton.Health.curr_Health = pose.Health;
        RigSkeleton.Health.max_Health = pose.MaxHealth;
    }

    public void TeleportToPose()
    {
        // Don't teleport if no pose
        if (!PoseReceiver.HasReceivedPose || !HasRig)
        {
            return;
        }

        OnBeforeTeleportToPose?.InvokeSafe($"executing {nameof(OnBeforeTeleportToPose)} hook");

        TeleportToPoseWithoutNotify();

        OnAfterTeleportToPose?.InvokeSafe($"executing {nameof(OnAfterTeleportToPose)} hook");
    }

    public void TeleportToPoseWithoutNotify()
    {
        // Don't teleport if no pose
        if (!PoseReceiver.HasReceivedPose || !HasRig)
        {
            return;
        }

        // Find the offsets for position and velocity to apply to the rig
        var pelvis = RigSkeleton.PhysicsPelvis;

        var currentPelvisPosition = pelvis.transform.position;
        var currentPelvisVelocity = pelvis.velocity;

        var pelvisPose = PoseReceiver.InterpolatedPose.Bodies[0];

        var targetPelvisPosition = pelvisPose.Position;
        var targetPelvisVelocity = pelvisPose.Velocity;

        var positionOffset = targetPelvisPosition - currentPelvisPosition;
        var velocityOffset = targetPelvisVelocity - currentPelvisVelocity;

        RigRefs.RigManager.TeleportWithOffset(positionOffset, velocityOffset);
    }

    public void TickRig(float deltaTime)
    {
        if (!HasRig)
        {
            return;
        }

        var remapRig = RigSkeleton.RemapRig;

        // SLZ doesn't clamp this by default, so it can create large values that make your rig go insanely fast
        // Usually occurs after getting your legs stuck in the ground
        remapRig._crouchSpeedLimit = ManagedMathf.Clamp01(remapRig._crouchSpeedLimit);

        if (NetworkEntity.IsOwner)
        {
            OnTickOwnedRig(deltaTime);
        }
        else
        {
            OnTickReceivedRig(deltaTime);
        }

        Ignorer.Tick(deltaTime / TimeReferences.SafeTimeScale);
    }

    public void TickPhysics(float deltaTime)
    {
        if (!HasRig)
        {
            return;
        }

        if (!NetworkEntity.IsOwner)
        {
            OnTickReceivedPhysics(deltaTime);
        }
    }

    public void MarkDirty()
    {
        AvatarSetter.SetDirty();

        _physicsRigStates.Clear();
    }

    public void EnqueuePhysicsRigState(PhysicsRigStateData data)
    {
        _physicsRigStates.Enqueue(data);
    }

    public void OnOverrideControllerRig()
    {
        var rigManager = RigRefs.RigManager;

        if (!PoseReceiver.HasReceivedPose)
        {
            rigManager.remapHeptaRig.inWeight = 0f;
            return;
        }

        rigManager.remapHeptaRig.inWeight = 1f;

        for (var i = 0; i < RigSkeleton.TrackerCount; i++)
        {
            var posePoint = RigPose.TrackedPoints[i];

            var smoothPoint = SmoothTrackedTransforms[i];
            smoothPoint = new ManagedTransform(
                Vector3.Lerp(smoothPoint.Position, posePoint.position, NetworkTickManager.SmoothInterpolationTime),
                Quaternion.Slerp(smoothPoint.Rotation, posePoint.rotation, NetworkTickManager.SmoothInterpolationTime));
            SmoothTrackedTransforms[i] = smoothPoint;

            var trackedPoint = RigSkeleton.TrackedPoints[i];

            trackedPoint.SetLocalPositionAndRotation(smoothPoint.Position, smoothPoint.Rotation);
        }
    }

    private void CopyPosePointsToSmoothPoints()
    {
        if (!PoseReceiver.HasReceivedPose)
        {
            return;
        }

        for (var i = 0; i < RigSkeleton.TrackerCount; i++)
        {
            var posePoint = RigPose.TrackedPoints[i];
            SmoothTrackedTransforms[i] = new ManagedTransform(posePoint.position, posePoint.rotation);
        }
    }

    private void OnTickOwnedRig(float deltaTime)
    {
        if (!NetworkTickManager.IsTickThisFrame)
        {
            return;
        }

        RigPose.ReadSkeleton(RigSkeleton);

        // Read health
        Health = RigRefs.Health.curr_Health;
        MaxHealth = RigRefs.Health.max_Health;

        var data = new RigPoseUpdateData()
        {
            RigReference = new(NetworkEntity),
            Pose = RigPose,
        };

         MessageRelay.RelayModule<RigPoseUpdateMessage, RigPoseUpdateData>(data, CommonMessageRoutes.UnreliableToOtherClients);
    }

    private void OnTickReceivedRig(float deltaTime)
    {
        OnProcessReceivedPose(deltaTime);

        OnProcessReceivedHands();

        OnProcessPhysicsRigState(RigRefs.RigManager.physicsRig);

        OnProcessAvatar();

        var remapRig = RigRefs.RigManager.remapHeptaRig;

        remapRig._crouchTarget = RigPose.CrouchTarget;
        remapRig._feetOffset = RigPose.FeetOffset;

        var trackedPlayspace = RigSkeleton.TrackedPlayspace;

        trackedPlayspace.rotation = Quaternion.Slerp(trackedPlayspace.rotation, RigPose.TrackedPlayspaceExpanded, NetworkTickManager.SmoothInterpolationTime);
    }

    private void OnProcessReceivedPose(float deltaTime)
    {
        float unscaledDeltaTime = deltaTime / TimeReferences.SafeTimeScale;

        PoseReceiver.TickPose(unscaledDeltaTime);
    }

    private void OnProcessReceivedHands()
    {
        OnProcessReceivedHand(RigRefs.LeftHand);
        OnProcessReceivedHand(RigRefs.RightHand);
    }

    private void OnProcessReceivedHand(Hand hand)
    {
        var controllerPose = hand.handedness == Handedness.LEFT ? RigPose.LeftController : RigPose.RightController;

        controllerPose?.CopyTo(hand.Controller);
    }

    private void OnProcessPhysicsRigState(PhysicsRig physicsRig)
    {
        while (_physicsRigStates.Count > 0)
        {
            _physicsRigStates.Dequeue().Apply(physicsRig);
        }
    }

    private void OnProcessAvatar()
    {
        AvatarSetter.Resolve(RigRefs);
    }

    private void OnTickReceivedPhysics(float deltaTime)
    {
        OnApplyForces(deltaTime);
    }

    private void OnApplyForces(float deltaTime)
    {
        if (!PoseReceiver.HasReceivedPose)
        {
            return;
        }

        var pelvisPose = PoseReceiver.InterpolatedPose.Bodies[0];

        // Stop bodies
        if (pelvisPose == null)
        {
            return;
        }

        // Check for seating
        var rigManager = RigRefs.RigManager;

        if (rigManager.activeSeat)
        {
            return;
        }

        var pelvis = RigSkeleton.PhysicsPelvis;
        var pelvisPosition = pelvis.position;
        var pelvisRotation = pelvis.rotation;

        var numericsPelvisPosition = pelvisPosition.ToNumericsVector3();

        var numericsPelvisTargetPosition = pelvisPose.Position.ToNumericsVector3();
        var numericsPelvisTargetVelocity = pelvisPose.Velocity.ToNumericsVector3();

        // Teleport to the rig pose if the position is too desynced
        if (NetworkTransformManager.IsLinearDesynced(numericsPelvisPosition, numericsPelvisTargetPosition, numericsPelvisTargetVelocity))
        {
            TeleportToPose();
            return;
        }

        var numericsPelvisVelocity = pelvis.velocity.ToNumericsVector3();

        // Get the rig's body mass
        // This is applied based on distance to make the rig easy to move when close to the target but harder when far from the target
        var physTorso = rigManager.physicsRig.torso;

        float pelvisMass = physTorso.rbPelvis.mass;
        float positionError = (numericsPelvisTargetPosition - numericsPelvisPosition).Length();
        float positionStrength = ManagedMathf.Clamp01(positionError);

        float forceMultiplier = ManagedMathf.Lerp(pelvisMass * 0.1f, pelvisMass, positionStrength);

        // Apply forces
        var force = SPDController.CalculateForce(numericsPelvisPosition, numericsPelvisVelocity, numericsPelvisTargetPosition, numericsPelvisTargetVelocity, deltaTime).ToUnityVector3();

        pelvis.AddForce(force * forceMultiplier, ForceMode.Force);

        // Only apply angular force when the pelvis is free
        if (!rigManager.physicsRig.ballLocoEnabled)
        {
            var torque = SPDController.CalculateTorque(pelvisRotation.ToNumericsQuaternion(), pelvis.angularVelocity.ToNumericsVector3(), pelvisPose.Rotation.ToNumericsQuaternion(), pelvisPose.AngularVelocity.ToNumericsVector3(), deltaTime).ToUnityVector3();

            pelvis.AddTorque(torque, ForceMode.Acceleration);
        }

        // Have the rig walk any extra distance into place
        // This accounts for desync caused by friction preventing the forces from reaching the destination
        // Translating the RemapRig seems to be the best way I can find to get the rig to start walking somewhere physically
        // Bugs occur in seats, but theres already a seat check above to prevent forces
        var offset = numericsPelvisTargetPosition - numericsPelvisPosition;
        offset.Y = 0f;

        var remapRig = RigSkeleton.RemapRig;
        var walkSpeed = remapRig.maxVelocity;

        offset = NumericsMathVector3.ClampMagnitude(offset, 1f) * walkSpeed;
        var delta = offset * deltaTime;

        remapRig.transform.position += delta.ToUnityVector3();
    }

    private void OnHiddenChanged()
    {
        if (NetworkEntity.IsOwner)
        {
            return;
        }

        bool hidden = IsHidden;

        if (HasRig)
        {
            ApplyHidden(hidden);
        }

        HiddenChanged?.InvokeSafe(hidden, "executing NetworkRig.HiddenChanged event");
    }

    private void ApplyHidden(bool hidden)
    {
        if (hidden)
        {
            OnCullRig();
        }
        else
        {
            OnUncullRig();

            TeleportToPose();
        }

        RigGrabber.OnEntityCull(hidden);
    }

    private void OnCullRig()
    {
        if (HasRig)
        {
            RigArt.CullArt(true);
            RigPhysics.CullPhysics(true);
        }
    }

    private void OnUncullRig()
    {
        if (HasRig)
        {
            RigArt.CullArt(false);
            RigPhysics.CullPhysics(false);
        }
    }

    private void OnRigAssigned(RigManager rigManager)
    {
        MarrowEntity = rigManager.physicsRig.marrowEntity;

        Ignorer = new(MarrowEntity);

        RigSkeleton = new(rigManager);
        RigRefs = new(rigManager);

        SmoothTrackedTransforms = new ManagedTransform[RigSkeleton.TrackerCount];

        RigRefs.HookOnDestroy(OnRigDestroyed);

        RigPose = new();

        RigArt = new(rigManager);

        RigPhysics = new(rigManager);

        RigGrabber = new(NetworkEntity, RigRefs);

        HookRig();

        MarkDirty();

        // Register components for the rig objects
        RegisterComponents();

        if (!NetworkEntity.IsOwner)
        {
            TeleportToPose();

            OnEntityCull(MarrowEntity.IsCulled);
        }

        _onReadyCallback?.InvokeSafe("executing NetworkPlayer.OnReadyCallback");
        _onReadyCallback = null;
    }

    private void OnRigDestroyed() => UnassignRig();

    private void RegisterComponents()
    {
        if (!HasRig)
        {
            return;
        }

        ComponentManager.RegisterComponents(NetworkEntity, RigRefs);
    }

    private void RegisterDynamicComponents()
    {
        if (!HasRig)
        {
            return;
        }

        ComponentManager.RegisterDynamicComponents(NetworkEntity, RigRefs);
    }

    private void UnregisterComponents() => ComponentManager.UnregisterComponents();

    private void UnregisterDynamicComponents() => ComponentManager.UnregisterDynamicComponents();

    private void HookRig()
    {
        var rigManager = RigRefs.RigManager;

        Cache.Add(rigManager, this);
        IMarrowEntityExtender.Cache.Add(MarrowEntity, NetworkEntity);

        var worldGrip = rigManager.worldGrip;

        if (worldGrip != null)
        {
            WorldGripCache.Add(worldGrip, this);
        }

        _onJumpAction = (Action)OnJump;
        _onAvatarSwappedAction = (Action)OnAvatarSwapped;

        rigManager.remapHeptaRig.onPlayerJump += _onJumpAction;
        rigManager.onAvatarSwapped += _onAvatarSwappedAction;
    }

    private void UnhookRig()
    {
        var rigManager = RigRefs.RigManager;

        Cache.Remove(rigManager);
        IMarrowEntityExtender.Cache.Remove(MarrowEntity);

        var worldGrip = rigManager.worldGrip;

        if (worldGrip != null)
        {
            WorldGripCache.Remove(worldGrip);
        }

        rigManager.remapHeptaRig.onPlayerJump -= _onJumpAction;
        rigManager.onAvatarSwapped -= _onAvatarSwappedAction;

        _onAvatarSwappedAction = null;
        _onJumpAction = null;
    }

    private void OnAvatarSwapped()
    {
        RegisterDynamicComponents();
    }

    private void OnJump()
    {
        if (!NetworkEntity.IsOwner)
        {
            return;
        }

        RigActionManager.RelayRigAction(new(NetworkEntity), RigActionType.Jump);
    }

    private void OnEntityOwnershipTransfer(NetworkEntity entity, PlayerID player)
    {
        bool isOwner = entity.IsOwner;

        RigGrabber?.OnRigOwnershipTransfer(isOwner);
    }

    private void OnEntityDataCatchup(NetworkEntity entity, PlayerID player)
    {
        RelayAvatar(new MessageRoute(player.SmallID, NetworkChannel.Reliable));
    }
}
