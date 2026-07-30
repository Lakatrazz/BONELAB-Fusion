using Il2CppSLZ.Marrow;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Scene;
using LabFusion.Preferences;
using LabFusion.Voice;
using LabFusion.Extensions;
using LabFusion.Marrow.Rig;

using MelonLoader;

using System.Collections;

namespace LabFusion.Entities;

public class NetworkPlayer : IEntityExtender, IEntityUpdatable, IEntityFixedUpdatable, IEntityLateUpdatable
{
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

    public NetworkRig NetworkRig { get; } = new();

    public bool IsRegistered { get; private set; } = false;

    public PlayerID PlayerID { get; private set; } = null;

    public string Username { get; private set; } = "No Name";

    public RigPuppet Puppet { get; private set; } = null;

    private RigNameTag _nametag = null;

    public RigIcon Icon { get; private set; } = null;

    public RigHeadUI HeadUI { get; private set; } = null;

    public RigHealthBar HealthBar { get; private set; } = null;

    public RigLivesBar LivesBar { get; private set; } = null;

    public RigVoiceSource VoiceSource { get; private set; } = null;

    private bool _isSettingsDirty = false;
    private bool _isServerDirty = false;

    public SerializedPlayerSettings playerSettings = null;

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

        NetworkRig.AvatarSetter.OnAvatarChanged += UpdateAvatarSettings;

        NetworkRig.HiddenChanged += OnHiddenChanged;

        // Register the default head UI elements so they're automatically spawned in
        HeadUI.RegisterElement(_nametag);
        HeadUI.RegisterElement(NetworkRig.AvatarSetter.ProgressBar);
        HeadUI.RegisterElement(Icon);
        HeadUI.RegisterElement(HealthBar);
        HeadUI.RegisterElement(LivesBar);
    }

    private void Initialize()
    {
        NetworkEntity.ConnectExtender(this);
        NetworkRig.ConnectToEntity(NetworkEntity);
    }

    public void FindRigManager()
    {
        if (NetworkEntity.IsOwner)
        {
            OnRigManagerFound(RigData.Refs.RigManager);
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

        // TODO: Move to NetworkRig
        if (!LocalAvatar.IsMatchingAvatar(barcode, NetworkRig.AvatarSetter.AvatarBarcode))
        {
            NetworkRig.AvatarSetter.SetAvatarDirty();
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
            OnRigManagerFound(rigManager);
        });
    }

    public void MarkDirty()
    {
        _isSettingsDirty = true;
        _isServerDirty = true;
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

        // Unhook from scene loading events
        DestroyPuppet();
        MultiplayerHooking.OnMainSceneInitialized -= OnLevelLoad;
        NetworkSceneManager.OnPurgatoryChanged -= OnPurgatoryChanged;
    }

    private void OnRigManagerFound(RigManager rigManager)
    {
        NetworkRig.AssignRig(rigManager);

        bool isLocalPlayer = NetworkEntity.IsOwner;

        // Create the audio source for voice chat
        if (!isLocalPlayer)
        {
            VoiceSource = new RigVoiceSource(JawFlapper, rigManager.physicsRig.headSfx.mouthSrc.transform);
            VoiceSource.CreateVoiceSource(PlayerID.SmallID);
        }

        OnNetworkRigCreated?.InvokeSafe(this, rigManager, "executing OnNetworkRigCreated hook");

        // If this isn't us, then catch up any data
        if (!isLocalPlayer)
        {
            CatchupManager.RequestEntityDataCatchup(new(NetworkEntity));
        }
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

    public void SetSettings(SerializedPlayerSettings settings)
    {
        playerSettings = settings;
        _isSettingsDirty = true;
    }

    private void UpdateAvatarSettings()
    {
        if (NetworkRig.HasRig)
        {
            _nametag.UpdateText();

            HeadUI.UpdateScale(NetworkRig.RigRefs.RigManager);

            VoiceSource?.SetVoiceRange(NetworkRig.RigRefs.RigManager.avatar.height);
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

        NetworkRig.DisconnectFromEntity();
    }

    public void OnEntityUpdate(float deltaTime)
    {
        OnPlayerUpdate(deltaTime);

        UpdatableManager.OnPlayerUpdate(deltaTime);
    }

    private void OnPlayerUpdate(float deltaTime)
    {
        if (!NetworkRig.HasRig)
        {
            return;
        }

        NetworkRig.TickRig(deltaTime);

        if (NetworkEntity.IsOwner)
        {
            JawFlapper.UpdateJaw(VoiceInfo.VoiceAmplitude, deltaTime);
        }
        else
        {
            VoiceSource?.UpdateVoiceSource(DistanceSqr, deltaTime);
        }
    }

    public void OnEntityFixedUpdate(float deltaTime)
    {
        OnPlayerFixedUpdate(deltaTime);

        UpdatableManager.OnPlayerFixedUpdate(deltaTime);
    }

    private void OnPlayerFixedUpdate(float deltaTime)
    {
        NetworkRig.TickPhysics(deltaTime);
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

        if (!NetworkRig.HasRig)
        {
            return;
        }

        HealthBar.Health = NetworkRig.Health;
        HealthBar.MaxHealth = NetworkRig.MaxHealth;

        var rigRefs = NetworkRig.RigRefs;
        var rigManager = NetworkRig.RigRefs.RigManager;

        HeadUI.UpdateTransform(rigManager);

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
        DistanceSqr = (rigRefs.Head.position - RigData.Refs.Head.position).sqrMagnitude;
    }

    private void OnHiddenChanged(bool hidden)
    {
        HeadUI.Visible = !hidden;

        if (hidden)
        {
            OnUnregisterUpdates();
        }
        else
        {
            OnReregisterUpdates();
        }
    }

    private void UpdateNametagVisibility()
    {
        _nametag.Visible = CommonPreferences.NameTags && FusionOverrides.ValidateNametag(PlayerID);
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
}
