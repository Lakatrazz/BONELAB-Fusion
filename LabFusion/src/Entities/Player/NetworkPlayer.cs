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

using MelonLoader;

using UnityEngine;

using System.Collections;

namespace LabFusion.Entities;

public class NetworkPlayer : IEntityExtender, IMarrowEntityExtender, IEntityUpdatable, IEntityFixedUpdatable, IEntityLateUpdatable
{
    public static readonly FusionComponentCache<RigManager, NetworkPlayer> RigCache = new();

    public static readonly HashSet<NetworkPlayer> Players = new();

    /// <summary>
    /// Invoked when a NetworkPlayer's RigManager is created. This is also invoked for the Local Player's RigManager.
    /// </summary>
    public static event Action<NetworkPlayer, RigManager> OnNetworkRigCreated;

    private NetworkEntity _networkEntity = null;

    private PlayerID _playerID = null;

    private string _username = "No Name";

    public NetworkEntity NetworkEntity => _networkEntity;

    private MarrowEntity _marrowEntity = null;
    public MarrowEntity MarrowEntity => _marrowEntity;

    public PlayerID PlayerID => _playerID;

    public string Username => _username;

    private RigRefs _rigRefs = null;
    public RigRefs RigRefs => _rigRefs;

    private RigSkeleton _rigSkeleton = null;
    public RigSkeleton RigSkeleton => _rigSkeleton;

    private RigPose _pose = null;
    public RigPose RigPose => _pose;

    private bool _receivedPose = false;
    public bool ReceivedPose => _receivedPose;

    private RigPuppet _puppet = null;
    public RigPuppet Puppet => _puppet;

    private RigNameTag _nametag = null;

    private RigIcon _icon = null;
    public RigIcon Icon => _icon;

    private RigHeadUI _headUI = null;
    public RigHeadUI HeadUI => _headUI;

    private RigArt _art = null;
    private RigPhysics _physics = null;

    private RigGrabber _grabber = null;
    public RigGrabber Grabber => _grabber;

    private RigAvatarSetter _avatarSetter = null;
    public RigAvatarSetter AvatarSetter => _avatarSetter;

    private RigHealthBar _healthBar = null;
    public RigHealthBar HealthBar => _healthBar;

    private RigLivesBar _livesBar = null;
    public RigLivesBar LivesBar => _livesBar;

    private RigVoiceSource _voiceSource = null;
    public RigVoiceSource VoiceSource => _voiceSource;

    private bool _isPhysicsRigDirty = false;
    private Queue<PhysicsRigStateData> _physicsRigStates = new();

    private bool _isSettingsDirty = false;
    private bool _isServerDirty = false;

    public SerializedPlayerSettings playerSettings = null;

    public bool HasRig => RigRefs != null && RigRefs.IsValid;

    private PDController _pelvisPDController = null;

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
    /// The distance of this PlayerRep's head to the local player's head (squared).
    /// </summary>
    public float DistanceSqr { get; private set; }

    private readonly JawFlapper _jawFlapper = new();
    public JawFlapper JawFlapper => _jawFlapper;

    public NetworkPlayer(NetworkEntity networkEntity, PlayerID playerId)
    {
        _networkEntity = networkEntity;
        _playerID = playerId;

        _pelvisPDController = new();

        _puppet = new();

        _nametag = new()
        {
            CrownVisible = playerId.IsHost,
        };

        _headUI = new();

        _icon = new()
        {
            Visible = false,
        };

        _healthBar = new()
        {
            Visible = false,
        };

        _livesBar = new()
        {
            Visible = false,
        };

        _avatarSetter = new(networkEntity);
        _avatarSetter.OnAvatarChanged += UpdateAvatarSettings;

        // Register the default head UI elements so they're automatically spawned in
        HeadUI.RegisterElement(_nametag);
        HeadUI.RegisterElement(_avatarSetter.ProgressBar);
        HeadUI.RegisterElement(_icon);
        HeadUI.RegisterElement(_healthBar);
        HeadUI.RegisterElement(_livesBar);

        networkEntity.HookOnRegistered(OnPlayerRegistered);
        networkEntity.OnEntityUnregistered += OnPlayerUnregistered;
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

        _puppet.CreatePuppet(OnPuppetCreated);

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

    internal void Internal_OnAvatarChanged(string barcode)
    {
        if (!LocalAvatar.IsMatchingAvatar(barcode, AvatarSetter.AvatarBarcode))
        {
            AvatarSetter.SetAvatarDirty();
        }
    }

    private void OnPuppetCreated(RigManager rigManager)
    {
        // Spawn the head ui
        _headUI.Spawn();

        // Mark our rig dirty for setting updates
        MarkDirty();

        // Rename the rig to match our ID
        rigManager.gameObject.name = $"{PlayerRepUtilities.PlayerRepName} (ID {PlayerID.SmallID})";

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
        if (_puppet.HasPuppet)
        {
            _puppet.DestroyPuppet();
        }

        _nametag.Despawn();

        // Despawn the head UI
        _headUI.Despawn();
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
            _username = name;
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

    private void OnPlayerRegistered(NetworkEntity entity)
    {
        Players.Add(this);

        HookPlayer();

        entity.ConnectExtender(this);

        OnReregisterUpdates();

        // Update metadata
        OnMetadataChanged();
    }

    private void OnPlayerUnregistered(NetworkEntity entity)
    {
#if DEBUG
        FusionLogger.Log($"Unregistered NetworkPlayer with ID {PlayerID.SmallID}.");
#endif

        Players.Remove(this);

        UnhookPlayer();

        entity.DisconnectExtender(this);

        _networkEntity = null;
        _playerID = null;

        VoiceSource?.DestroyVoiceSource();
        _voiceSource = null;

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
        if (!HasRig)
        {
            return;
        }

        var remapRig = RigSkeleton.remapRig;

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
            OnHandUpdate(RigRefs.LeftHand);
            OnHandUpdate(RigRefs.RightHand);

            VoiceSource?.UpdateVoiceSource(DistanceSqr, deltaTime);

            remapRig._crouchTarget = RigPose.CrouchTarget;
            remapRig._feetOffset = RigPose.FeetOffset;
        }
    }

    public void OnEntityFixedUpdate(float deltaTime)
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
        if (NetworkEntity.IsOwner)
        {
            return;
        }

        if (!HasRig)
        {
            return;
        }

        _headUI.UpdateTransform(RigRefs.RigManager);

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
                var color = playerSettings.nametagColor;
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
        _headUI.Visible = false;

        if (HasRig)
        {
            _art.CullArt(true);
            _physics.CullPhysics(true);
        }
    }

    private void OnUncullExtras()
    {
        _headUI.Visible = true;

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

        NetworkPlayerManager.UpdateManager.Register(this);
        NetworkPlayerManager.FixedUpdateManager.Register(this);
        NetworkPlayerManager.LateUpdateManager.Register(this);
    }

    private void OnUnregisterUpdates()
    {
        NetworkPlayerManager.UpdateManager.Unregister(this);
        NetworkPlayerManager.FixedUpdateManager.Unregister(this);
        NetworkPlayerManager.LateUpdateManager.Unregister(this);
    }

    public void TeleportToPose()
    {
        // Don't teleport if no pose
        if (!ReceivedPose || !HasRig)
        {
            return;
        }

        // Find the target centerOfPressure position and teleport
        var targetPelvis = RigPose.PelvisPose.PredictedPosition;
        var offset = targetPelvis - RigSkeleton.physicsPelvis.transform.position;
        var newPosition = RigRefs.RigManager.physicsRig.centerOfPressure.position + offset;

        RigRefs.RigManager.TeleportToPosition(newPosition, true);

        // Reset PD controller
        _pelvisPDController.Reset();
    }

    private void OnOwnedUpdate()
    {
        RigPose.ReadSkeleton(RigSkeleton);

        var data = PlayerPoseUpdateData.Create(RigPose);

        MessageRelay.RelayNative(data, NativeMessageTag.PlayerPoseUpdate, CommonMessageRoutes.UnreliableToOtherClients);
    }

    private void OnApplyBodyForces(float deltaTime)
    {
        if (!ReceivedPose)
        {
            return;
        }

        var pelvisPose = RigPose.PelvisPose;

        // Stop bodies
        if (pelvisPose == null)
        {
            _pelvisPDController.Reset();
            return;
        }

        // Check for seating
        var rigManager = RigRefs.RigManager;

        if (rigManager.activeSeat)
        {
            _pelvisPDController.Reset();
            return;
        }

        var pelvis = RigSkeleton.physicsPelvis;
        var pelvisPosition = pelvis.position;
        var pelvisRotation = pelvis.rotation;

        // Move position with prediction
        pelvisPose.PredictPosition(deltaTime);

        // Check for stability teleport
        float distSqr = (pelvisPosition - pelvisPose.PredictedPosition).sqrMagnitude;
        if (distSqr > (2f * (pelvisPose.velocity.magnitude + 1f)))
        {
            TeleportToPose();
            return;
        }

        // Apply forces
        pelvis.AddForce(_pelvisPDController.GetForce(pelvisPosition, pelvis.velocity, pelvisPose.PredictedPosition, pelvisPose.velocity), ForceMode.Acceleration);

        // Only apply angular force when the pelvis is free
        if (!rigManager.physicsRig.ballLocoEnabled)
        {
            pelvis.AddTorque(_pelvisPDController.GetTorque(pelvisRotation, pelvis.angularVelocity, pelvisPose.rotation, pelvisPose.angularVelocity), ForceMode.Acceleration);
        }
        else
        {
            _pelvisPDController.ResetRotation();
        }
    }

    public void OnReceivePose(RigPose pose)
    {
        // If we don't have a rig yet, don't store the pose
        if (!HasRig)
        {
            return;
        }

        _pose = pose;

        // Teleport to the pose if this is our first
        if (!ReceivedPose)
        {
            _receivedPose = true;
            TeleportToPose();
        }

        // Update the playspace rotation
        RigSkeleton.trackedPlayspace.rotation = RigPose.TrackedPlayspace.Expand();

        // Update the health
        HealthBar.Health = pose.Health;
        HealthBar.MaxHealth = pose.MaxHealth;

        RigSkeleton.health.curr_Health = pose.Health;
        RigSkeleton.health.max_Health = pose.MaxHealth;
    }

    public void OnOverrideControllerRig()
    {
        if (!ReceivedPose)
        {
            RigRefs.RigManager.remapHeptaRig.inWeight = 0f;
            return;
        }

        RigRefs.RigManager.remapHeptaRig.inWeight = 1f;

        for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
        {
            var trackedPoint = RigSkeleton.trackedPoints[i];
            var posePoint = RigPose.TrackedPoints[i];

            trackedPoint.SetLocalPositionAndRotation(posePoint.position, posePoint.rotation);
        }
    }

    private void OnRigDestroyed()
    {
        _pose = null;
        _receivedPose = false;

        NetworkEntity?.ClearDataCaughtUpPlayers();

        UnregisterComponents();
    }

    private void OnFoundRigManager(RigManager rigManager)
    {
        _marrowEntity = rigManager.physicsRig.marrowEntity;

        _rigSkeleton = new(rigManager);
        _rigRefs = new(rigManager);

        _rigRefs.HookOnDestroy(OnRigDestroyed);

        _pose = new();

        _grabber = new RigGrabber(RigRefs);

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
            _voiceSource = new RigVoiceSource(JawFlapper, rigManager.physicsRig.headSfx.mouthSrc.transform);
            _voiceSource.CreateVoiceSource(PlayerID.SmallID);
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

        _registeredComponentExtenders = EntityComponentManager.ApplyComponents(NetworkEntity, physicsRig.gameObject);

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
