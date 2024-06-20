using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Rig;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Audio;
using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow.Utilities;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Preferences;
using LabFusion.Voice;

using MelonLoader;

using UnityEngine;

using System.Collections;
using BoneLib;
using LabFusion.Marrow;

namespace LabFusion.Entities;

public class NetworkPlayer : IEntityExtender, IMarrowEntityExtender, IEntityUpdatable, IEntityFixedUpdatable, IEntityLateUpdatable
{
    public static readonly FusionComponentCache<RigManager, NetworkPlayer> RigCache = new();

    private NetworkEntity _networkEntity = null;

    private PlayerId _playerId = null;

    private string _username = "No Name";

    public NetworkEntity NetworkEntity => _networkEntity;

    private MarrowEntity _marrowEntity = null;
    public MarrowEntity MarrowEntity => _marrowEntity;

    public PlayerId PlayerId => _playerId;

    public string Username => _username;

    private RigReferenceCollection _rigReferences = null;
    public RigReferenceCollection RigReferences => _rigReferences;

    private RigSkeleton _rigSkeleton = null;
    public RigSkeleton RigSkeleton => _rigSkeleton;

    private RigPose _pose = null;
    public RigPose RigPose => _pose;

    private RigPuppet _puppet = null;
    public RigPuppet Puppet => _puppet;

    private RigNametag _nametag = null;

    private RigGrabber _grabber = null;
    public RigGrabber Grabber => _grabber;

    private RigAvatarSetter _avatarSetter = null;
    public RigAvatarSetter AvatarSetter => _avatarSetter;

    private bool _isRagdollDirty = false;
    private bool _ragdollState = false;

    private bool _isSettingsDirty = false;
    private bool _isServerDirty = false;

    private bool _isQuestUser = false;

    public SerializedPlayerSettings playerSettings = null;

    public bool HasRig => RigReferences != null && RigReferences.IsValid;

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

        _avatarSetter = new();
        _avatarSetter.OnAvatarChanged += UpdateAvatarSettings;

        networkEntity.HookOnRegistered(OnPlayerRegistered);
        networkEntity.OnEntityUnregistered += OnPlayerUnregistered;
    }

    public void FindRigManager()
    {
        if (NetworkEntity.IsOwner)
        {
            OnFoundRigManager(RigData.RigReferences.RigManager);
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
                yield break;

            yield return null;
        }

        // Wait for loading
        while (FusionSceneManager.IsDelayedLoading() || PlayerId.GetMetadata(MetadataHelper.LoadingKey) == bool.TrueString)
        {
            if (FusionSceneManager.IsLoading())
                yield break;

            yield return null;
        }

        // Make sure the rep still exists
        if (PlayerId == null || !PlayerId.IsValid)
            yield break;

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

        var pullCord = RigReferences.RigManager.GetComponentInChildren<PullCordDevice>(true);
        pullCord.PlayAvatarParticleEffects();

        pullCord._map3.PlayAtPoint(pullCord.switchAvatar, pullCord.transform.position, null, pullCord.switchVolume, 1f, new(0f), 1f, 1f);
    }

    public void SetBallEnabled(bool isEnabled)
    {
        if (!HasRig)
            return;

        var pullCord = RigReferences.RigManager.GetComponentInChildren<PullCordDevice>(true);

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
        SetBallEnabled(false);

        _nametag.CreateNametag();

        MarkDirty();

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
        PlayerId.OnMetadataChanged += OnMetadataChanged;
        PlayerId.OnDestroyedEvent += OnPlayerDestroyed;

        MultiplayerHooking.OnServerSettingsChanged += OnServerSettingsChanged;
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
        PlayerId.OnMetadataChanged -= OnMetadataChanged;
        PlayerId.OnDestroyedEvent -= OnPlayerDestroyed;

        MultiplayerHooking.OnServerSettingsChanged -= OnServerSettingsChanged;
        FusionOverrides.OnOverridesChanged -= OnServerSettingsChanged;

        // Remove cache
        if (HasRig)
        {
            RemoveFromCache();
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

        _nametag.DestroyNametag();
    }

    private void OnMetadataChanged(PlayerId playerId)
    {
        // Read display name
        if (playerId.TryGetDisplayName(out var name))
        {
            _username = name;
        }

        _isQuestUser = PlayerId.GetMetadata(MetadataHelper.PlatformKey) == "QUEST";

        // Update nametag
        if (!NetworkEntity.IsOwner)
        {
            _nametag.SetUsername(Username, _isQuestUser);

            if (HasRig)
            {
                _nametag.UpdateSettings(RigReferences.RigManager);
            }
        }
    }

    private void OnServerSettingsChanged()
    {
        _isServerDirty = true;

        OnMetadataChanged(PlayerId);
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
            _nametag.UpdateSettings(RigReferences.RigManager);
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
            return;

        // Modify the source settings
        var rm = RigReferences.RigManager;
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
            _flapper.ClearJaw();
        }
        else
        {
            _flapper.UpdateJaw(_speaker.GetVoiceAmplitude());
        }
    }

    private void UpdateVoiceSourceSettings()
    {
        if (_voiceSource == null)
            return;

        var rm = RigReferences.RigManager;
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
        HookPlayer();

        entity.ConnectExtender(this);

        OnReregisterUpdates();

        // Update metadata
        OnMetadataChanged(PlayerId);
    }

    private void OnPlayerUnregistered(NetworkEntity entity)
    {
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
            OnHandUpdate(RigReferences.LeftHand);
            OnHandUpdate(RigReferences.RightHand);

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

        _nametag.UpdateTransform(RigReferences.RigManager);

        // Update the player if its dirty and has an avatar
        var rm = RigReferences.RigManager;

        // Resolve avatar changes
        AvatarSetter.Resolve(RigReferences);

        // Toggle ragdoll mode
        if (_isRagdollDirty)
        {
            if (_ragdollState)
            {
                rm.physicsRig.RagdollRig();
            }
            else
            {
                rm.physicsRig.UnRagdollRig();
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
                _nametag.text.color = color;
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
        DistanceSqr = (RigReferences.Head.position - RigData.RigReferences.Head.position).sqrMagnitude;
    }

    private void OnCullExtras()
    {
        _nametag.ToggleNametag(false);
    }

    private void OnUncullExtras()
    {
        UpdateNametagVisibility();
    }

    private void UpdateNametagVisibility()
    {
        _nametag.ToggleNametag(FusionPreferences.NametagsEnabled && FusionOverrides.ValidateNametag(PlayerId));
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
        if (RigPose == null || !HasRig)
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
        MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Unreliable, message);
    }

    private void OnApplyPelvisForces(float deltaTime)
    {
        if (RigPose == null)
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
        var rigManager = RigReferences.RigManager;

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
        if (SafetyUtilities.IsValidTime)
        {
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
    }

    public void OnReceivePose(RigPose pose)
    {
        _pose = pose;

        if (HasRig)
        {
            RigSkeleton.trackedPlayspace.rotation = RigPose.trackedPlayspace.Expand();
        }
    }

    public void OnOverrideControllerRig()
    {
        if (RigPose == null)
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

    private void OnFoundRigManager(RigManager rigManager)
    {
        _marrowEntity = rigManager.marrowEntity;

        _rigSkeleton = new(rigManager);
        _rigReferences = new(rigManager);

        _pose = new();

        _grabber = new RigGrabber(RigReferences);

        AddToCache();

        // Register components for the physics rig
        // TODO: register components for the avatar on avatar change
        RegisterComponents(rigManager.physicsRig.gameObject);

        // Uncull
        if (!MarrowEntity.IsCulled && !NetworkEntity.IsOwner)
        {
            OnEntityCull(false);
        }
    }

    private void AddToCache()
    {
        RigCache.Add(RigReferences.RigManager, this);
        IMarrowEntityExtender.Cache.Add(MarrowEntity, NetworkEntity);
    }

    private void RemoveFromCache()
    {
        RigCache.Remove(RigReferences.RigManager);
        IMarrowEntityExtender.Cache.Remove(MarrowEntity);
    }

    private List<IEntityComponentExtender> _componentExtenders = null;

    private void RegisterComponents(params GameObject[] parents)
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
