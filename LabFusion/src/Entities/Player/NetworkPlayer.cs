using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Audio;
using Il2CppSLZ.Marrow;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Scene;
using LabFusion.Preferences;
using LabFusion.Preferences.Server;
using LabFusion.Voice;

using MelonLoader;

using UnityEngine;

using System.Collections;

namespace LabFusion.Entities;

public class NetworkPlayer : IEntityExtender, IMarrowEntityExtender, IEntityUpdatable, IEntityFixedUpdatable, IEntityLateUpdatable
{
    public static readonly FusionComponentCache<RigManager, NetworkPlayer> RigCache = new();

    public static readonly HashSet<NetworkPlayer> Players = new();

    public static event Action<NetworkPlayer, RigManager> OnNetworkRigCreated;

    private NetworkEntity _networkEntity = null;

    private PlayerId _playerId = null;

    private string _username = "No Name";

    public NetworkEntity NetworkEntity => _networkEntity;

    private MarrowEntity _marrowEntity = null;
    public MarrowEntity MarrowEntity => _marrowEntity;

    public PlayerId PlayerId => _playerId;

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

    private RigHeadUI _headUI = null;
    public RigHeadUI HeadUI => _headUI;

    private RigArt _art = null;
    private RigPhysics _physics = null;

    private RigGrabber _grabber = null;
    public RigGrabber Grabber => _grabber;

    private RigAvatarSetter _avatarSetter = null;
    public RigAvatarSetter AvatarSetter => _avatarSetter;

    private bool _isRagdollDirty = false;
    private bool _ragdollState = false;

    private bool _isSettingsDirty = false;
    private bool _isServerDirty = false;

    public SerializedPlayerSettings playerSettings = null;

    public bool HasRig => RigRefs != null && RigRefs.IsValid;

    private PDController _pelvisPDController = null;

    // Voice chat integration
    private float _maxMicrophoneDistance = 30f;

    private IVoiceSpeaker _speaker = null;
    private AudioSource _voiceSource = null;
    private bool _hasVoice = false;

    private bool _spatialized = false;

    private readonly JawFlapper _flapper = new();

    /// <summary>
    /// The distance of this PlayerRep's head to the local player's head (squared).
    /// </summary>
    public float DistanceSqr { get; private set; }

    /// <summary>
    /// Whether or not the player's microphone logic is disabled.
    /// </summary>
    public bool MicrophoneDisabled { get; private set; }

    public JawFlapper JawFlapper => _flapper;

    public NetworkPlayer(NetworkEntity networkEntity, PlayerId playerId)
    {
        _networkEntity = networkEntity;
        _playerId = playerId;

        _pelvisPDController = new();
        _puppet = new();
        _nametag = new();
        _headUI = new();

        _avatarSetter = new(networkEntity);
        _avatarSetter.OnAvatarChanged += UpdateAvatarSettings;

        // Register the default head UI elements so they're automatically spawned in
        HeadUI.RegisterElement(_nametag);
        HeadUI.RegisterElement(_avatarSetter.ProgressBar);

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
        while (FusionSceneManager.IsDelayedLoading() || PlayerId.Metadata.GetMetadata(MetadataHelper.LoadingKey) == bool.TrueString)
        {
            if (FusionSceneManager.IsLoading())
            {
                yield break;
            }

            yield return null;
        }

        // Make sure the rep still exists
        if (PlayerId == null || !PlayerId.IsValid)
        {
            yield break;
        }

        _puppet.CreatePuppet(OnPuppetCreated);
    }

    internal void Internal_OnAvatarChanged(string barcode)
    {
        if (!FusionAvatar.IsMatchingAvatar(barcode, AvatarSetter.AvatarBarcode))
            AvatarSetter.SetAvatarDirty();
    }

    public void PlayPullCordEffects()
    {
        if (!HasRig)
            return;

        var pullCord = RigRefs.RigManager.GetComponentInChildren<PullCordDevice>(true);
        pullCord.PlayAvatarParticleEffects();

        pullCord._map3.PlayAtPoint(pullCord.switchAvatar, pullCord.transform.position, null, pullCord.switchVolume, 1f, new(0f), 1f, 1f);
    }

    public void SetBallEnabled(bool isEnabled)
    {
        if (!HasRig)
        {
            return;
        }

        var pullCord = RigRefs.RigManager.GetComponentInChildren<PullCordDevice>(true);

        // If the ball should be enabled, make the distance required infinity so it always shows
        if (isEnabled)
        {
            pullCord.handShowDist = float.PositiveInfinity;
        }
        // If it should be disabled, make the distance zero so that it disables itself
        else
        {
            pullCord.handShowDist = 0f;
        }
    }

    private void OnPuppetCreated(RigManager rigManager)
    {
        // Disable the bodylog
        SetBallEnabled(false);

        // Spawn the head ui
        _headUI.Spawn();

        // Mark our rig dirty for setting updates
        MarkDirty();

        // Rename the rig to match our ID
        rigManager.gameObject.name = $"{PlayerRepUtilities.PlayerRepName} (ID {PlayerId.SmallId})";

        // Hook into the rig
        OnFoundRigManager(rigManager);
    }

    public void MarkDirty()
    {
        AvatarSetter.SetDirty();

        _isSettingsDirty = true;
        _isServerDirty = true;

        _isRagdollDirty = true;
        _ragdollState = false;
    }

    private void OnLevelLoad()
    {
        FindRigManager();
    }

    private void HookPlayer()
    {
        // Lock the entity's owner to the player id
        NetworkEntity.SetOwner(PlayerId);
        NetworkEntity.LockOwner();

        // Hook into the player's events
        PlayerId.Metadata.OnMetadataChanged += OnMetadataChanged;
        PlayerId.OnDestroyedEvent += OnPlayerDestroyed;

        LobbyInfoManager.OnLobbyInfoChanged += OnServerSettingsChanged;
        FusionOverrides.OnOverridesChanged += OnServerSettingsChanged;

        // Find the rig for the current scene, and hook into scene loads
        FindRigManager();
        MultiplayerHooking.OnMainSceneInitialized += OnLevelLoad;
    }

    private void UnhookPlayer()
    {
        // Unlock the owner
        NetworkEntity.UnlockOwner();

        // Unhook from the player's events
        PlayerId.Metadata.OnMetadataChanged -= OnMetadataChanged;
        PlayerId.OnDestroyedEvent -= OnPlayerDestroyed;

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
        if (PlayerId.TryGetDisplayName(out var name))
        {
            _username = name;
        }

        // Update nametag
        if (!NetworkEntity.IsOwner)
        {
            _nametag.SetUsername(Username);

            if (HasRig)
            {
                _nametag.UpdateText();
            }
        }
    }

    private void OnServerSettingsChanged()
    {
        _isServerDirty = true;

        OnMetadataChanged();
    }

    public void SetRagdoll(bool isRagdolled)
    {
        _ragdollState = isRagdolled;
        _isRagdollDirty = true;
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

            _headUI.UpdateScale(RigRefs.RigManager);
        }

        UpdateVoiceSourceSettings();
    }

    public void InsertVoiceSource(IVoiceSpeaker speaker, AudioSource source)
    {
        _speaker = speaker;
        _voiceSource = source;
        _hasVoice = true;
    }

    private void OnUpdateVoiceSource()
    {
        if (!_hasVoice)
        {
            return;
        }

        // Modify the source settings
        var rm = RigRefs.RigManager;
        if (HasRig)
        {
            var mouthSource = rm.physicsRig.headSfx.mouthSrc;
            _voiceSource.transform.position = mouthSource.transform.position;

            if (!_spatialized)
            {
                UpdateVoiceSourceSettings();
                _spatialized = true;
            }

            MicrophoneDisabled = DistanceSqr > (_maxMicrophoneDistance * _maxMicrophoneDistance) * 1.2f;
        }
        else
        {
            _voiceSource.spatialBlend = 0f;
            _voiceSource.minDistance = 0f;
            _voiceSource.maxDistance = 30f;
            _voiceSource.reverbZoneMix = 0f;
            _voiceSource.dopplerLevel = 0.5f;

            _spatialized = false;
            MicrophoneDisabled = false;
        }

        // Update the jaw movement
        if (MicrophoneDisabled)
        {
            JawFlapper.ClearJaw();
        }
        else
        {
            JawFlapper.UpdateJaw(_speaker.GetVoiceAmplitude());
        }
    }

    private void UpdateVoiceSourceSettings()
    {
        if (_voiceSource == null)
            return;

        var rm = RigRefs.RigManager;
        if (HasRig && rm._avatar)
        {
            float heightMult = rm._avatar.height / 1.76f;

            _voiceSource.spatialBlend = 1f;
            _voiceSource.reverbZoneMix = 0.1f;

            _maxMicrophoneDistance = 30f * heightMult;

            _voiceSource.SetRealisticRolloff(0.5f, _maxMicrophoneDistance);

            // Set the mixer
            if (_voiceSource.outputAudioMixerGroup == null)
            {
                _voiceSource.outputAudioMixerGroup = Audio3dManager.npcVocals;
            }
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
        NetworkEntityManager.IdManager.UnregisterEntity(NetworkEntity);
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
        FusionLogger.Log($"Unregistered NetworkPlayer with ID {PlayerId.SmallId}.");
#endif

        Players.Remove(this);

        UnhookPlayer();

        entity.DisconnectExtender(this);

        _networkEntity = null;
        _playerId = null;

        OnUnregisterUpdates();
    }

    public void OnHandUpdate(Hand hand)
    {
        switch (hand.handedness)
        {
            case Handedness.LEFT:
                RigPose.physicsLeftHand?.CopyTo(hand, hand.Controller);
                break;
            case Handedness.RIGHT:
                RigPose.physicsRightHand?.CopyTo(hand, hand.Controller);
                break;
        }
    }

    public void OnEntityUpdate(float deltaTime)
    {
        if (!HasRig)
        {
            return;
        }

        if (NetworkEntity.IsOwner)
        {
            OnOwnedUpdate();

            JawFlapper.UpdateJaw(VoiceInfo.VoiceAmplitude);
        }
        else
        {
            OnHandUpdate(RigRefs.LeftHand);
            OnHandUpdate(RigRefs.RightHand);

            OnUpdateVoiceSource();

            RigSkeleton.remapRig._feetOffset = RigPose.feetOffset;
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
            OnApplyPelvisForces(deltaTime);
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

        // Toggle ragdoll mode
        if (_isRagdollDirty)
        {
            var physicsRig = rm.physicsRig;

            if (_ragdollState)
            {
                physicsRig.RagdollRig();
            }
            else
            {
                physicsRig.UnRagdollRig();
            }

            _isRagdollDirty = false;
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
        _nametag.Visible = CommonPreferences.NameTags && FusionOverrides.ValidateNametag(PlayerId);
    }

    public void OnEntityCull(bool isInactive)
    {
        if (NetworkEntity.IsOwner)
        {
            return;
        }

        if (isInactive)
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

        Grabber.OnEntityCull(isInactive);
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

    private void TeleportToPose()
    {
        // Don't teleport if no pose
        if (!ReceivedPose || !HasRig)
        {
            return;
        }

        // Get teleport position
        var pos = RigPose.pelvisPose.position;

        // Get offset
        var offset = pos - RigSkeleton.physicsPelvis.position;

        // Apply offset to the marrow entity
        MarrowEntity.transform.position += offset;

        _pelvisPDController.Reset();
    }

    private void OnOwnedUpdate()
    {
        RigPose.ReadSkeleton(RigSkeleton);

        using var writer = FusionWriter.Create(PlayerPoseUpdateData.Size);
        var data = PlayerPoseUpdateData.Create(PlayerId, RigPose);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.PlayerPoseUpdate, writer);
        MessageSender.SendToServer(NetworkChannel.Unreliable, message);
    }

    private void OnApplyPelvisForces(float deltaTime)
    {
        if (!ReceivedPose)
        {
            return;
        }

        var pelvisPose = RigPose.pelvisPose;

        // Stop pelvis
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
        Vector3 pelvisPosition = pelvis.position;
        Quaternion pelvisRotation = pelvis.rotation;

        // Move position with prediction
        pelvisPose.PredictPosition(deltaTime);

        // Check for stability teleport
        float distSqr = (pelvisPosition - pelvisPose.position).sqrMagnitude;
        if (distSqr > (2f * (pelvisPose.velocity.magnitude + 1f)))
        {
            TeleportToPose();
            return;
        }

        // Apply forces
        pelvis.AddForce(_pelvisPDController.GetForce(pelvisPosition, pelvis.velocity, pelvisPose.position, pelvisPose.velocity), ForceMode.Acceleration);

        // We only want to apply angular force when ragdolled
        if (rigManager.physicsRig.torso.spineInternalMult <= 0f)
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
        RigSkeleton.trackedPlayspace.rotation = RigPose.trackedPlayspace.Expand();
    }

    public void OnOverrideControllerRig()
    {
        if (!ReceivedPose)
        {
            return;
        }

        for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
        {
            var trackedPoint = RigSkeleton.trackedPoints[i];
            var posePoint = RigPose.trackedPoints[i];

            trackedPoint.SetLocalPositionAndRotation(posePoint.position, posePoint.rotation);
        }
    }

    private void OnRigDestroyed()
    {
        _pose = null;
        _receivedPose = false;
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
        }

        // Run events
        OnNetworkRigCreated?.InvokeSafe(this, rigManager, "executing OnNetworkRigCreated hook");
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
        RegisterComponents();
    }

    private HashSet<IEntityComponentExtender> _componentExtenders = null;

    private void RegisterComponents()
    {
        if (!HasRig)
        {
            return;
        }

        var physicsRig = RigRefs.RigManager.physicsRig;
        var avatar = RigRefs.RigManager.avatar;

        var parents = new GameObject[] { physicsRig.gameObject, avatar.gameObject };

        RegisterComponents(parents);
    }

    private void RegisterComponents(GameObject[] parents)
    {
        UnregisterComponents();

        _componentExtenders = EntityComponentManager.ApplyComponents(NetworkEntity, parents);
    }

    private void UnregisterComponents()
    {
        if (_componentExtenders == null)
        {
            return;
        }

        foreach (var extender in _componentExtenders)
        {
            extender.Unregister();
        }

        _componentExtenders.Clear();
    }
}
