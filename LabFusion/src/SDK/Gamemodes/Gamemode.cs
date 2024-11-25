using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Utilities;
using LabFusion.SDK.Metadata;
using LabFusion.SDK.Triggers;
using LabFusion.Menu.Data;

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
        MultiplayerHooking.OnLoadingBegin += OnLoadingBegin;

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
        MultiplayerHooking.OnLoadingBegin -= OnLoadingBegin;

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
        if (!NetworkInfo.IsServer)
        {
            return false;
        }

        GamemodeSender.SendGamemodeMetadataSet(Barcode, key, value);
        return true;
    }

    private bool OnTryRemoveMetadata(string key)
    {
        // We can only remove metadata as the server!
        if (!NetworkInfo.IsServer)
        {
            return false;
        }

        GamemodeSender.SendGamemodeMetadataRemove(Barcode, key);
        return true;
    }

    public virtual void OnGamemodeSelected() { }
    public virtual void OnGamemodeDeselected() { }

    public virtual void OnGamemodeStarted() { }
    public virtual void OnGamemodeStopped() { }

    public virtual void OnGamemodeReady() { }
    public virtual void OnGamemodeUnready() { }

    public virtual void OnGamemodeRegistered() { }
    public virtual void OnGamemodeUnregistered() { }

    public virtual void OnMainSceneInitialized() { }

    public virtual void OnLoadingBegin() { }

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
}