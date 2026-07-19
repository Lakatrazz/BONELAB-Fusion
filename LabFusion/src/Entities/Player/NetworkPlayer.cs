using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Scene;
using LabFusion.Preferences;
using LabFusion.Voice;
using LabFusion.Math;
using LabFusion.Extensions;
using LabFusion.Marrow.Rig;
using LabFusion.Math.Numerics;
using LabFusion.Marrow.Extensions;

using MelonLoader;

using UnityEngine;

using System.Collections;

namespace LabFusion.Entities;

public class NetworkPlayer : IEntityExtender, IMarrowEntityExtender, IEntityUpdatable, IEntityFixedUpdatable, IEntityLateUpdatable
{
    public static readonly FusionComponentCache<RigManager, NetworkPlayer> RigCache = new();

    public static readonly HashSet<NetworkPlayer> Players = new();

    /// <summary>
    /// Invoked when a new NetworkPlayer is registered. This is also invoked for the Local Player's NetworkPlayer.
    /// </summary>
    public static event Action<NetworkPlayer> OnNetworkPlayerRegistered;

    /// <summary>
    /// Invoked when a NetworkPlayer's RigManager is created. This is also invoked for the Local Player's RigManager.
    /// </summary>
    public static event Action<NetworkPlayer, RigManager> OnNetworkRigCreated;

    public NetworkEntity NetworkEntity { get; private set; } = null;

    public MarrowEntity MarrowEntity { get; private set; } = null;

    public bool IsRegistered { get; private set; } = false;

    public PlayerID PlayerID { get; private set; } = null;

    public string Username { get; private set; } = "No Name";

    public RigRefs RigRefs { get; private set; } = null;

    public RigSkeleton RigSkeleton { get; private set; } = null;

    private ManagedTransform[] _smoothTrackedTransforms = null;
    public ManagedTransform[] SmoothTrackedTransforms => _smoothTrackedTransforms;

    public RigPose RigPose { get; private set; } = null;

    /// <summary>
    /// The receiver for the MarrowEntity pose from the player that handles interpolation and prediction.
    /// </summary>
    public EntityPoseReceiver PoseReceiver { get; private set; } = new();

    /// <summary>
    /// Tracks entities that the player will ignore collision with for a specific amount of time.
    /// </summary>
    public EntityIgnorer Ignorer { get; private set; } = null;

    public RigPuppet Puppet { get; private set; } = null;

    private RigNameTag _nametag = null;

    public RigIcon Icon { get; private set; } = null;

    public RigHeadUI HeadUI { get; private set; } = null;

    private RigArt _art = null;
    private RigPhysics _physics = null;

    public RigGrabber Grabber { get; private set; } = null;

    public RigAvatarSetter AvatarSetter { get; private set; } = null;

    public RigHealthBar HealthBar { get; private set; } = null;

    public RigLivesBar LivesBar { get; private set; } = null;

    public RigVoiceSource VoiceSource { get; private set; } = null;

    private bool _isPhysicsRigDirty = false;
    private Queue<PhysicsRigStateData> _physicsRigStates = new();

    private bool _isSettingsDirty = false;
    private bool _isServerDirty = false;

    public SerializedPlayerSettings playerSettings = null;

    public bool HasRig => RigRefs != null && RigRefs.IsValid;

    private bool _isCulled = false;

    /// <summary>
    /// Returns True if this NetworkPlayer is hidden due to being zone culled.
    /// </summary>
    public bool IsCulled
    {
        get
        {
            return _isCulled;
        }
        private set
        {
            _isCulled = value;

            OnApplyVisiblity();
        }
    }

    private bool _forceHide = false;

    /// <summary>
    /// Can be changed to forcefully hide this NetworkPlayer's rig.
    /// </summary>
    public bool ForceHide
    {
        get
        {
            return _forceHide;
        }
        set
        {
            _forceHide = value;

            OnApplyVisiblity();
        }
    }

    /// <summary>
    /// Returns True if this NetworkPlayer is hidden.
    /// </summary>
    public bool IsHidden
    {
        get
        {
            if (ForceHide)
            {
                return true;
            }

            return IsCulled;
        }
    }

    /// <summary>
    /// Callback invoked when the <see cref="IsHidden"/> property changes.
    /// </summary>
    public event Action<bool> OnHiddenChanged;

    /// <summary>
    /// The distance of this NetworkPlayer's head to the local player's head (squared).
    /// </summary>
    public float DistanceSqr { get; private set; }

    /// <summary>
    /// The manager for adding custom Update, FixedUpdate, and LateUpdate events for a NetworkPlayer.
    /// </summary>
    public PlayerUpdatableManager UpdatableManager { get; } = new();

    public event Action OnBeforeTeleportToPose, OnAfterTeleportToPose;

    public JawFlapper JawFlapper { get; private set; } = new();

    private readonly EntityPose _receivedEntityPose = new(1);

    public static NetworkPlayer CreatePlayer(NetworkEntity networkEntity, PlayerID playerID)
    {
        var networkPlayer = new NetworkPlayer(networkEntity, playerID);

        networkPlayer.Initialize();

        return networkPlayer;
    }

    private NetworkPlayer(NetworkEntity networkEntity, PlayerID playerID)
    {
        NetworkEntity = networkEntity;
        PlayerID = playerID;

        Puppet = new();

        _nametag = new()
        {
            CrownVisible = playerID.IsHost,
        };

        HeadUI = new();

        Icon = new()
        {
            Visible = false,
        };

        HealthBar = new()
        {
            Visible = false,
        };

        LivesBar = new()
        {
            Visible = false,
        };

        AvatarSetter = new(networkEntity);
        AvatarSetter.OnAvatarChanged += UpdateAvatarSettings;

        // The only synced body is the pelvis, so its initialized with one
        PoseReceiver.InitializePoses(1);

        // Register the default head UI elements so they're automatically spawned in
        HeadUI.RegisterElement(_nametag);
        HeadUI.RegisterElement(AvatarSetter.ProgressBar);
        HeadUI.RegisterElement(Icon);
        HeadUI.RegisterElement(HealthBar);
        HeadUI.RegisterElement(LivesBar);
    }

    private void Initialize()
    {
        NetworkEntity.ConnectExtender(this);
    }

    public void FindRigManager()
    {
        if (NetworkEntity.IsOwner)
        {
            OnFoundRigManager(RigData.Refs.RigManager);
        }
        else
        {
            MelonCoroutines.Start(WaitAndCreateRig());
        }
    }

    private IEnumerator WaitAndCreateRig()
    {
        // Delay some extra time
        for (var i = 0; i < 120; i++)
        {
            if (FusionSceneManager.IsLoading())
            {
                yield break;
            }

            yield return null;
        }

        // Wait for loading
        while (IsPlayerLoading())
        {
            if (FusionSceneManager.IsLoading())
            {
                yield break;
            }

            yield return null;
        }

        // Make sure the rep still exists
        if (PlayerID == null || !PlayerID.IsValid)
        {
            yield break;
        }

        Puppet.CreatePuppet(OnPuppetCreated);

        bool IsPlayerLoading()
        {
            if (FusionSceneManager.IsDelayedLoading())
            {
                return true;
            }

            if (PlayerID.Metadata.IsValid && PlayerID.Metadata.Loading.GetValue())
            {
                return true;
            }

            return false;
        }
    }

    internal void OnAvatarBarcodeChanged(string barcode)
    {
        if (NetworkEntity.IsOwner)
        {
            return;
        }

        if (!LocalAvatar.IsMatchingAvatar(barcode, AvatarSetter.AvatarBarcode))
        {
            AvatarSetter.SetAvatarDirty();
        }
    }

    private void OnPuppetCreated(RigManager rigManager)
    {
        // Spawn the head ui
        HeadUI.Spawn();

        // Mark our rig dirty for setting updates
        MarkDirty();

        // Rename the rig to match our ID
        rigManager.gameObject.name = NetRigSpawner.GetNetRigName(PlayerID.SmallID);

        // Hook into the rig
        // Wait one frame so that the rig is properly initialized
        DelayUtilities.InvokeNextFrame(() =>
        {
            OnFoundRigManager(rigManager);
        });
    }

    public void MarkDirty()
    {
        AvatarSetter.SetDirty();

        _isSettingsDirty = true;
        _isServerDirty = true;

        _isPhysicsRigDirty = true;
        _physicsRigStates.Clear();
    }

    private void OnLevelLoad()
    {
        if (NetworkSceneManager.Purgatory)
        {
            return;
        }

        FindRigManager();
    }

    private void OnPurgatoryChanged(bool purgatory)
    {
        // Don't care if this is our rig
        if (NetworkEntity.IsOwner)
        {
            return;
        }

        // Don't update while loading
        if (FusionSceneManager.IsLoading())
        {
            return;
        }

        // Puppet rig shouldn't exist in purgatory
        if (purgatory)
        {
            DestroyPuppet();
        }
        else
        {
            FindRigManager();
        }
    }

    private void HookPlayer()
    {
        // Lock the entity's owner to the player id
        NetworkEntity.SetOwner(PlayerID);
        NetworkEntity.LockOwner();

        // Hook into the player's events
        PlayerID.Metadata.Metadata.OnMetadataChanged += OnMetadataChanged;
        PlayerID.OnDestroyedEvent += OnPlayerDestroyed;

        LobbyInfoManager.OnLobbyInfoChanged += OnServerSettingsChanged;
        FusionOverrides.OnOverridesChanged += OnServerSettingsChanged;

        // Find the rig for the current scene, and hook into scene loads
        FindRigManager();
        MultiplayerHooking.OnMainSceneInitialized += OnLevelLoad;
        NetworkSceneManager.OnPurgatoryChanged += OnPurgatoryChanged;
    }

    private void UnhookPlayer()
    {
        // Unlock the owner
        NetworkEntity.UnlockOwner();

        // Unhook from the player's events
        PlayerID.Metadata.Metadata.OnMetadataChanged -= OnMetadataChanged;
        PlayerID.OnDestroyedEvent -= OnPlayerDestroyed;

        LobbyInfoManager.OnLobbyInfoChanged -= OnServerSettingsChanged;
        FusionOverrides.OnOverridesChanged -= OnServerSettingsChanged;

        // Remove cache
        if (HasRig)
        {
            UnhookRig();
        }

        // Unhook from scene loading events
        DestroyPuppet();
        MultiplayerHooking.OnMainSceneInitialized -= OnLevelLoad;
        NetworkSceneManager.OnPurgatoryChanged -= OnPurgatoryChanged;
    }

    private void DestroyPuppet()
    {
        if (Puppet.HasPuppet)
        {
            Puppet.DestroyPuppet();
        }

        _nametag.Despawn();

        // Despawn the head UI
        HeadUI.Despawn();
    }

    private void OnMetadataChanged(string key, string value)
    {
        OnMetadataChanged();
    }

    private void OnMetadataChanged()
    {
        // Read display name
        if (PlayerID.TryGetDisplayName(out var name))
        {
            Username = name;
        }

        // Update nametag
        if (!NetworkEntity.IsOwner)
        {
            _nametag.Username = Username;
        }
    }

    private void OnServerSettingsChanged()
    {
        _isServerDirty = true;

        OnMetadataChanged();
    }

    public void EnqueuePhysicsRigState(PhysicsRigStateData data)
    {
        _physicsRigStates.Enqueue(data);
        _isPhysicsRigDirty = true;
    }

    public void SetSettings(SerializedPlayerSettings settings)
    {
        playerSettings = settings;
        _isSettingsDirty = true;
    }

    private void UpdateAvatarSettings()
    {
        if (HasRig)
        {
            _nametag.UpdateText();

            HeadUI.UpdateScale(RigRefs.RigManager);

            VoiceSource?.SetVoiceRange(RigRefs.RigManager.avatar.height);
        }
    }

    private void OnPlayerDestroyed()
    {
        // Make sure the entity exists still
        if (NetworkEntity.IsDestroyed)
        {
            return;
        }

        // Unregister the entity
        NetworkEntityManager.IDManager.UnregisterEntity(NetworkEntity);
    }

    public void OnExtenderRegistered()
    {
        IsRegistered = true;

        Players.Add(this);

        HookPlayer();

        OnReregisterUpdates();

        // Update metadata
        OnMetadataChanged();

        // Invoke hook
        OnNetworkPlayerRegistered?.InvokeSafe(this, "executing OnNetworkPlayerRegistered hook");
    }

    public void OnExtenderUnregistered()
    {
        IsRegistered = false;

#if DEBUG
        FusionLogger.Log($"Unregistered NetworkPlayer with ID {PlayerID.SmallID}.");
#endif

        Players.Remove(this);

        UnhookPlayer();

        NetworkEntity = null;
        PlayerID = null;

        VoiceSource?.DestroyVoiceSource();
        VoiceSource = null;

        OnUnregisterUpdates();
    }

    public void OnHandUpdate(Hand hand)
    {
        switch (hand.handedness)
        {
            case Handedness.LEFT:
                RigPose.LeftController?.CopyTo(hand.Controller);
                break;
            case Handedness.RIGHT:
                RigPose.RightController?.CopyTo(hand.Controller);
                break;
        }
    }

    public void OnEntityUpdate(float deltaTime)
    {
        OnPlayerUpdate(deltaTime);

        UpdatableManager.OnPlayerUpdate(deltaTime);
    }

    private void OnPlayerUpdate(float deltaTime)
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
            OnOwnedUpdate();

            JawFlapper.UpdateJaw(VoiceInfo.VoiceAmplitude, deltaTime);
        }
        else
        {
            OnProcessReceivedPose(deltaTime);

            OnHandUpdate(RigRefs.LeftHand);
            OnHandUpdate(RigRefs.RightHand);

            VoiceSource?.UpdateVoiceSource(DistanceSqr, deltaTime);

            remapRig._crouchTarget = RigPose.CrouchTarget;
            remapRig._feetOffset = RigPose.FeetOffset;

            // Update the playspace rotation
            var trackedPlayspace = RigSkeleton.TrackedPlayspace;

            trackedPlayspace.rotation = Quaternion.Slerp(trackedPlayspace.rotation, RigPose.TrackedPlayspaceExpanded, NetworkTickManager.SmoothInterpolationTime);
        }

        Ignorer.Tick(deltaTime / TimeReferences.SafeTimeScale);
    }

    private void OnProcessReceivedPose(float deltaTime)
    {
        float unscaledDeltaTime = deltaTime / TimeReferences.SafeTimeScale;

        PoseReceiver.TickPose(unscaledDeltaTime);
    }

    public void OnEntityFixedUpdate(float deltaTime)
    {
        OnPlayerFixedUpdate(deltaTime);

        UpdatableManager.OnPlayerFixedUpdate(deltaTime);
    }

    private void OnPlayerFixedUpdate(float deltaTime)
    {
        if (!HasRig)
        {
            return;
        }

        if (!NetworkEntity.IsOwner)
        {
            OnApplyBodyForces(deltaTime);
        }
    }

    public void OnEntityLateUpdate(float deltaTime)
    {
        OnPlayerLateUpdate(deltaTime);

        UpdatableManager.OnPlayerLateUpdate(deltaTime);
    }

    private void OnPlayerLateUpdate(float deltaTime)
    {
        if (NetworkEntity.IsOwner)
        {
            return;
        }

        if (!HasRig)
        {
            return;
        }

        HeadUI.UpdateTransform(RigRefs.RigManager);

        // Update the player if its dirty and has an avatar
        var rm = RigRefs.RigManager;

        // Resolve avatar changes
        AvatarSetter.Resolve(RigRefs);

        // Apply physics rig states
        if (_isPhysicsRigDirty)
        {
            var physicsRig = rm.physicsRig;

            while (_physicsRigStates.Count > 0)
            {
                _physicsRigStates.Dequeue().Apply(physicsRig);
            }

            _isPhysicsRigDirty = false;
        }

        // Update settings
        if (_isSettingsDirty)
        {
            if (playerSettings != null)
            {
                // Make sure the alpha is 1 so that people cannot create invisible names
                var color = playerSettings.NametagColor;
                color.a = 1f;
                _nametag.Color = color;
            }

            _isSettingsDirty = false;
        }

        // Update server side settings
        if (_isServerDirty)
        {
            UpdateNametagVisibility();

            _isServerDirty = false;
        }

        // Update distance value
        DistanceSqr = (RigRefs.Head.position - RigData.Refs.Head.position).sqrMagnitude;
    }

    private void OnCullExtras()
    {
        HeadUI.Visible = false;

        if (HasRig)
        {
            _art.CullArt(true);
            _physics.CullPhysics(true);
        }
    }

    private void OnUncullExtras()
    {
        HeadUI.Visible = true;

        if (HasRig)
        {
            _art.CullArt(false);
            _physics.CullPhysics(false);
        }
    }

    private void UpdateNametagVisibility()
    {
        _nametag.Visible = CommonPreferences.NameTags && FusionOverrides.ValidateNametag(PlayerID);
    }

    public void OnEntityCull(bool isInactive)
    {
        if (NetworkEntity.IsOwner)
        {
            return;
        }

        IsCulled = isInactive;
    }

    private void OnApplyVisiblity()
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

        OnHiddenChanged?.InvokeSafe(hidden, "executing NetworkPlayer.OnHiddenChanged");
    }

    private void ApplyHidden(bool hidden)
    {
        if (hidden)
        {
            OnCullExtras();
            OnUnregisterUpdates();
        }
        else
        {
            OnUncullExtras();
            OnReregisterUpdates();

            TeleportToPose();
        }

        Grabber.OnEntityCull(hidden);
    }

    private void OnReregisterUpdates()
    {
        OnUnregisterUpdates();

        var updatableManager = NetworkPlayerManager.UpdatableManager;

        updatableManager.UpdateManager.Register(this);
        updatableManager.FixedUpdateManager.Register(this);
        updatableManager.LateUpdateManager.Register(this);
    }

    private void OnUnregisterUpdates()
    {
        var updatableManager = NetworkPlayerManager.UpdatableManager;

        updatableManager.UpdateManager.Unregister(this);
        updatableManager.FixedUpdateManager.Unregister(this);
        updatableManager.LateUpdateManager.Unregister(this);
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

    private void OnOwnedUpdate()
    {
        // Only send if on our tick rate
        if (!NetworkTickManager.IsTickThisFrame)
        {
            return;
        }

        RigPose.ReadSkeleton(RigSkeleton);

        var data = PlayerPoseUpdateData.Create(RigPose);

        MessageRelay.RelayNative(data, NativeMessageTag.PlayerPoseUpdate, CommonMessageRoutes.UnreliableToOtherClients);
    }

    private void OnApplyBodyForces(float deltaTime)
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

    public void ReceivePose(RigPose pose)
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
        HealthBar.Health = pose.Health;
        HealthBar.MaxHealth = pose.MaxHealth;

        RigSkeleton.Health.curr_Health = pose.Health;
        RigSkeleton.Health.max_Health = pose.MaxHealth;
    }

    private void CopyPosePointsToSmoothPoints()
    {
        if (!PoseReceiver.HasReceivedPose)
        {
            return;
        }

        for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
        {
            var posePoint = RigPose.TrackedPoints[i];
            SmoothTrackedTransforms[i] = new ManagedTransform(posePoint.position, posePoint.rotation);
        }
    }

    public void OnOverrideControllerRig()
    {
        if (!PoseReceiver.HasReceivedPose)
        {
            RigRefs.RigManager.remapHeptaRig.inWeight = 0f;
            return;
        }

        RigRefs.RigManager.remapHeptaRig.inWeight = 1f;

        for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
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

    private void OnRigDestroyed()
    {
        RigPose = null;
        PoseReceiver.ClearPoseState();

        NetworkEntity?.ClearDataCaughtUpPlayers();

        UnregisterComponents();
    }

    private void OnFoundRigManager(RigManager rigManager)
    {
        MarrowEntity = rigManager.physicsRig.marrowEntity;

        Ignorer = new(MarrowEntity);

        RigSkeleton = new(rigManager);
        RigRefs = new(rigManager);

        _smoothTrackedTransforms = new ManagedTransform[RigAbstractor.TransformSyncCount];

        RigRefs.HookOnDestroy(OnRigDestroyed);

        RigPose = new();

        Grabber = new RigGrabber(RigRefs);

        _art = new(rigManager);
        _physics = new(rigManager);

        HookRig();

        // Register components for the rig objects
        RegisterComponents();

        if (!NetworkEntity.IsOwner)
        {
            // Teleport to the received pose
            TeleportToPose();

            // Match the current cull state
            OnEntityCull(MarrowEntity.IsCulled);

            // Create voice source
            VoiceSource = new RigVoiceSource(JawFlapper, rigManager.physicsRig.headSfx.mouthSrc.transform);
            VoiceSource.CreateVoiceSource(PlayerID.SmallID);
        }

        // Run events
        OnNetworkRigCreated?.InvokeSafe(this, rigManager, "executing OnNetworkRigCreated hook");

        _onReadyCallback?.InvokeSafe("executing NetworkPlayer.OnReadyCallback");
        _onReadyCallback = null;

        // If this isn't us, then catch up any data
        if (!NetworkEntity.IsOwner)
        {
            CatchupManager.RequestEntityDataCatchup(new(NetworkEntity));
        }
    }

    private Il2CppSystem.Action _onAvatarSwappedAction = null;

    private void HookRig()
    {
        RigCache.Add(RigRefs.RigManager, this);
        IMarrowEntityExtender.Cache.Add(MarrowEntity, NetworkEntity);

        _onAvatarSwappedAction = (Action)OnAvatarSwapped;

        RigRefs.RigManager.onAvatarSwapped += _onAvatarSwappedAction;
    }

    private void UnhookRig()
    {
        RigCache.Remove(RigRefs.RigManager);
        IMarrowEntityExtender.Cache.Remove(MarrowEntity);

        RigRefs.RigManager.onAvatarSwapped -= _onAvatarSwappedAction;

        _onAvatarSwappedAction = null;
    }

    private void OnAvatarSwapped()
    {
        RegisterDynamicComponents();
    }

    private HashSet<IEntityComponentExtender> _registeredComponentExtenders = null;
    private HashSet<IEntityComponentExtender> _dynamicComponentExtenders = null;

    private void RegisterComponents()
    {
        if (!HasRig)
        {
            return;
        }

        var physicsRig = RigRefs.RigManager.physicsRig;

        var detacher = new TemporaryTransformDetacher();
        DetachSlottedTransforms(detacher);

        _registeredComponentExtenders = EntityComponentManager.ApplyComponents(NetworkEntity, physicsRig.gameObject);

        detacher.ReattachTransforms();

        RegisterDynamicComponents();
    }

    private void RegisterDynamicComponents()
    {
        if (!HasRig)
        {
            return;
        }

        UnregisterDynamicComponents();

        var avatar = RigRefs.RigManager.avatar;

        _dynamicComponentExtenders = EntityComponentManager.ApplyDynamicComponents(NetworkEntity, avatar.gameObject);
    }

    private void UnregisterComponents()
    {
        UnregisterDynamicComponents();

        if (_registeredComponentExtenders != null)
        {
            foreach (var extender in _registeredComponentExtenders)
            {
                extender.Unregister();
            }

            _registeredComponentExtenders.Clear();
        }
    }

    private void UnregisterDynamicComponents()
    {
        if (_registeredComponentExtenders != null)
        {
            foreach (var extender in _registeredComponentExtenders)
            {
                extender.UnregisterDynamics();
            }
        }

        if (_dynamicComponentExtenders != null)
        {
            foreach (var extender in _dynamicComponentExtenders)
            {
                extender.Unregister();
            }

            _dynamicComponentExtenders.Clear();
        }
    }

    private void DetachSlottedTransforms(TemporaryTransformDetacher detacher)
    {
        foreach (var slot in RigRefs.RigSlots)
        {
            var slottedWeapon = slot._slottedWeapon;

            if (slottedWeapon == null)
            {
                continue;
            }

            var host = slottedWeapon.interactableHost;

            if (host == null)
            {
                continue;
            }

            var entity = host.marrowEntity;

            if (entity == null)
            {
                continue;
            }

            detacher.DetachTransform(entity.transform);
        }

        foreach (var receiver in RigRefs.AmmoReceivers)
        {
            foreach (var artTarget in receiver._ammoArtTargets)
            {
                detacher.DetachTransform(artTarget.transform);
            }
        }
    }

    private Action _onReadyCallback = null;

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
}
