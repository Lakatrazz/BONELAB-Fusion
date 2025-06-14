using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Utilities;
using LabFusion.SDK.Metadata;
using LabFusion.SDK.Triggers;
using LabFusion.Menu.Data;
using LabFusion.Player;

using UnityEngine;

namespace LabFusion.SDK.Gamemodes;

/// <summary>
/// The base class for a multiplayer Gamemode.
/// </summary>
public abstract class Gamemode
{
    public static event Action<Gamemode, bool> OnStartedKeyChanged, OnSelectedKeyChanged, OnReadyKeyChanged;

    private bool _isStarted = false;
    
    /// <summary>
    /// Returns if this Gamemode has been started and is running.
    /// </summary>
    public bool IsStarted => _isStarted;

    private bool _isSelected = false;

    /// <summary>
    /// Returns if this is the currently selected Gamemode for this server.
    /// </summary>
    public bool IsSelected => _isSelected;

    private bool _isReady = false;

    /// <summary>
    /// Returns if this Gamemode has enough players and is ready to be started.
    /// </summary>
    public bool IsReady => _isReady;

    /// <summary>
    /// The title of the Gamemode.
    /// </summary>
    public abstract string Title { get; }

    /// <summary>
    /// The author of the Gamemode.
    /// </summary>
    public abstract string Author { get; }

    /// <summary>
    /// A short description of the Gamemode. Defaults to null.
    /// </summary>
    public virtual string Description => null;

    /// <summary>
    /// A unique string that identifies the Gamemode. Defaults to "Author.Title".
    /// </summary>
    public virtual string Barcode => $"{Author}.{Title}";

    /// <summary>
    /// A simple square logo for this Gamemode. Defaults to null.
    /// </summary>
    public virtual Texture Logo => null;

    /// <summary>
    /// Should the Gamemode handle when it is ready or not? 
    /// <para>If false, the GamemodeManager will automatically check for <see cref="CheckReadyConditions"/> to ready the Gamemode.</para>
    /// <para>If true, the Gamemode should manually call <see cref="GamemodeManager.ReadyGamemode"/> and <see cref="GamemodeManager.UnreadyGamemode"/>.</para>
    /// <para>Defaults to false.</para>
    /// </summary>
    public virtual bool ManualReady => false;

    // Gamemode settings
    public virtual bool AutoStopOnSceneLoad { get; } = true;
    public virtual bool AutoHolsterOnDeath { get; } = true;

    // Cheats
    public virtual bool DisableDevTools { get; } = false;
    public virtual bool DisableSpawnGun { get; } = false;
    public virtual bool DisableManualUnragdoll { get; } = false;


    private readonly NetworkMetadata _metadata = new();
    public NetworkMetadata Metadata => _metadata;

    private readonly TriggerRelay _relay = new();
    public TriggerRelay Relay => _relay;

    internal void GamemodeRegistered()
    {
        MultiplayerHooking.OnMainSceneInitialized += OnMainSceneInitialized;
        MultiplayerHooking.OnPlayerJoined += OnPlayerJoinedCallback;
        MultiplayerHooking.OnPlayerLeft += OnPlayerLeftCallback;

        // Metadata
        Metadata.OnTrySetMetadata += OnTrySetMetadata;
        Metadata.OnTryRemoveMetadata += OnTryRemoveMetadata;

        Metadata.OnMetadataChanged += OnMetadataChanged;
        Metadata.OnMetadataChanged += OnInternalMetadataChanged;
        Metadata.OnMetadataRemoved += OnMetadataRemoved;

        // Triggers
        Relay.OnTryInvokeTrigger += OnTryInvokeTrigger;
        Relay.OnTryInvokeTriggerWithValue += OnTryInvokeTriggerWithValue;

        OnGamemodeRegistered();
    }

    internal void GamemodeUnregistered()
    {
        MultiplayerHooking.OnMainSceneInitialized -= OnMainSceneInitialized;

        // Metadata
        Metadata.OnTrySetMetadata -= OnTrySetMetadata;
        Metadata.OnTryRemoveMetadata -= OnTryRemoveMetadata;

        Metadata.OnMetadataChanged -= OnMetadataChanged;
        Metadata.OnMetadataChanged -= OnInternalMetadataChanged;
        Metadata.OnMetadataRemoved -= OnMetadataRemoved;
        
        // Triggers
        Relay.OnTryInvokeTrigger -= OnTryInvokeTrigger;
        Relay.OnTryInvokeTriggerWithValue -= OnTryInvokeTriggerWithValue;

        OnGamemodeUnregistered();
    }

    private bool OnTryInvokeTrigger(string name)
    {
        GamemodeSender.SendGamemodeTriggerResponse(Barcode, name, null);
        return true;
    }

    private bool OnTryInvokeTriggerWithValue(string name, string value)
    {
        GamemodeSender.SendGamemodeTriggerResponse(Barcode, name, value);
        return true;
    }

    private bool OnTrySetMetadata(string key, string value)
    {
        // We can only change metadata as the server!
        if (!NetworkInfo.IsHost)
        {
            return false;
        }

        GamemodeSender.SendGamemodeMetadataSet(Barcode, key, value);
        return true;
    }

    private bool OnTryRemoveMetadata(string key)
    {
        // We can only remove metadata as the server!
        if (!NetworkInfo.IsHost)
        {
            return false;
        }

        GamemodeSender.SendGamemodeMetadataRemove(Barcode, key);
        return true;
    }

    /// <summary>
    /// Invoked when this Gamemode is selected for the server.
    /// </summary>
    public virtual void OnGamemodeSelected() { }

    /// <summary>
    /// Invoked when this Gamemode is deselected for the server.
    /// </summary>
    public virtual void OnGamemodeDeselected() { }

    /// <summary>
    /// Invoked when this Gamemode starts.
    /// </summary>
    public virtual void OnGamemodeStarted() { }

    /// <summary>
    /// Invoked when this Gamemode stops.
    /// </summary>
    public virtual void OnGamemodeStopped() { }

    /// <summary>
    /// Invoked when this Gamemode meets all ready conditions.
    /// </summary>
    public virtual void OnGamemodeReady() { }

    /// <summary>
    /// Invoked when this Gamemode no longer meets its ready conditions.
    /// </summary>
    public virtual void OnGamemodeUnready() { }

    /// <summary>
    /// Invoked when this Gamemode is registered.
    /// </summary>
    public virtual void OnGamemodeRegistered() { }

    /// <summary>
    /// Invoked when this Gamemode is unregistered.
    /// </summary>
    public virtual void OnGamemodeUnregistered() { }

    public virtual void OnMainSceneInitialized() { }

    /// <summary>
    /// Invoked after the Gamemode starts if a level is not loading.
    /// While the Gamemode is started, this will be invoked every time the player loads into the server's target level.
    /// </summary>
    public virtual void OnLevelReady() { }

    /// <summary>
    /// Invoked if a new Player joins while the Gamemode is already started.
    /// </summary>
    /// <param name="playerId"></param>
    protected virtual void OnPlayerJoined(PlayerID playerId) { }

    /// <summary>
    /// Invoked if a Player leaves while the Gamemode is still active.
    /// </summary>
    /// <param name="playerId"></param>
    protected virtual void OnPlayerLeft(PlayerID playerId) { }

    private void OnPlayerJoinedCallback(PlayerID playerId)
    {
        if (!IsStarted)
        {
            return;
        }

        OnPlayerJoined(playerId);
    }

    private void OnPlayerLeftCallback(PlayerID playerId)
    {
        if (!IsStarted)
        {
            return;
        }

        OnPlayerLeft(playerId);
    }

    public virtual GroupElementData CreateSettingsGroup()
    {
        return new GroupElementData()
        {
            Title = Title,
        };
    }

    /// <summary>
    /// Checks if all of the Gamemode's conditions to start are met.
    /// </summary>
    /// <returns>If the conditions are met.</returns>
    public virtual bool CheckReadyConditions()
    {
        return true;
    }

    // Update methods
    public void FixedUpdate()
    {
        OnFixedUpdate();
    }
    protected virtual void OnFixedUpdate() { }

    public void Update()
    {
        OnUpdate();
    }
    protected virtual void OnUpdate() { }

    public void LateUpdate()
    {
        OnLateUpdate();
    }
    protected virtual void OnLateUpdate() { }

    private void OnInternalMetadataChanged(string key, string value)
    {
        bool parsed = value == bool.TrueString;

        switch (key)
        {
            case GamemodeKeys.StartedKey:
                if (_isStarted == parsed)
                {
                    return;
                }

                _isStarted = parsed;

                OnStartedKeyChanged?.Invoke(this, _isStarted);
                break;
            case GamemodeKeys.SelectedKey:
                if (_isSelected == parsed)
                {
                    return;
                }

                _isSelected = parsed;

                OnSelectedKeyChanged?.Invoke(this, _isSelected);
                break;
            case GamemodeKeys.ReadyKey:
                if (_isReady == parsed)
                {
                    return;
                }

                _isReady = parsed;

                OnReadyKeyChanged?.Invoke(this, _isReady);
                break;
        }
    }

    protected virtual void OnMetadataChanged(string key, string value) { }

    protected virtual void OnMetadataRemoved(string key, string value) { }

    /// <summary>
    /// Clears all of the Gamemode's non persistent metadata locally. Does not remove it from the server. This should only be used when the Gamemode is finished.
    /// </summary>
    public void ClearMetadata() => Metadata.ClearLocalMetadataExcept(GamemodeKeys.PersistentKeys);

    /// <summary>
    /// Checks if a player can be attacked by the local player.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player can be attacked, False otherwise.</returns>
    public virtual bool CanAttack(PlayerID player) => true;
}