﻿using BoneLib.BoneMenu.Elements;

using LabFusion.BoneMenu;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using LabFusion.SDK.Metadata;
using LabFusion.SDK.Triggers;

using UnityEngine;

namespace LabFusion.SDK.Gamemodes;

public abstract class Gamemode
{
    internal static bool _isGamemodeRunning = false;
    internal static Gamemode _activeGamemode = null;
    internal static Gamemode _markedGamemode = null;

    /// <summary>
    /// Is a Gamemode currently running?
    /// </summary>
    public static bool IsGamemodeRunning => _isGamemodeRunning;

    /// <summary>
    /// The active Gamemode.
    /// </summary>
    public static Gamemode ActiveGamemode => _activeGamemode;

    /// <summary>
    /// The Gamemode that the current server is marked as.
    /// </summary>
    public static Gamemode MarkedGamemode => _markedGamemode;

    /// <summary>
    /// The target Gamemode. Returns active Gamemode if it exists, or the marked Gamemode.
    /// </summary>
    public static Gamemode TargetGamemode => _activeGamemode ?? _markedGamemode;

    public static bool LateJoining { get; internal set; } = false;

    internal ushort? _tag = null;
    public ushort? Tag => _tag;

    // Gamemode info
    public bool IsStarted { get; private set; }

    // Gamemode settings
    public abstract string GamemodeCategory { get; }
    public abstract string GamemodeName { get; }

    public virtual bool VisibleInBonemenu { get; } = true;
    public virtual bool PreventNewJoins { get; } = false;
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

        Metadata.OnMetadataChanged += OnMetadataChanged;
        Metadata.OnMetadataChanged += OnInternalMetadataChanged;
        Metadata.OnMetadataRemoved -= OnMetadataRemoved;
        
        // Triggers
        Relay.OnTryInvokeTrigger -= OnTryInvokeTrigger;
        Relay.OnTryInvokeTriggerWithValue -= OnTryInvokeTriggerWithValue;

        OnGamemodeUnregistered();
    }

    private bool OnTryInvokeTrigger(string name)
    {
        GamemodeSender.SendGamemodeTriggerResponse(Tag.Value, name, null);
        return true;
    }

    private bool OnTryInvokeTriggerWithValue(string name, string value)
    {
        GamemodeSender.SendGamemodeTriggerResponse(Tag.Value, name, value);
        return true;
    }

    private bool OnTrySetMetadata(string key, string value)
    {
        // We can only change metadata as the server!
        if (!NetworkInfo.IsServer)
        {
            return false;
        }

        GamemodeSender.SendGamemodeMetadataSet(Tag.Value, key, value);
        return true;
    }

    private bool OnTryRemoveMetadata(string key)
    {
        // We can only remove metadata as the server!
        if (!NetworkInfo.IsServer)
        {
            return false;
        }

        GamemodeSender.SendGamemodeMetadataRemove(Tag.Value, key);
        return true;
    }

    public bool IsActive() => ActiveGamemode == this;

    public virtual void OnGamemodeRegistered() { }

    public virtual void OnGamemodeUnregistered() { }

    public virtual void OnMainSceneInitialized() { }

    public virtual void OnLoadingBegin() { }

    protected FunctionElement _gamemodeToggleElement = null;
    protected FunctionElement _gamemodeMarkElement = null;

    public virtual void OnBoneMenuCreated(MenuCategory category)
    {
        // Default elements
        _gamemodeToggleElement = category.CreateFunctionElement("Start Gamemode", Color.yellow, () =>
        {
            if (!IsActive())
            {
                StartGamemode(true);
            }
            else
            {
                StopGamemode();
            }
        });

        _gamemodeMarkElement = category.CreateFunctionElement("Mark Gamemode", Color.yellow, () =>
        {
            if (MarkedGamemode != this)
            {
                MarkGamemode();
            }
            else
            {
                UnmarkGamemode();
            }
        });
    }

    public bool StartGamemode(bool forceStopCurrent = false)
    {
        // We can only start the gamemode as a server!
        if (!NetworkInfo.IsServer)
        {
            return false;
        }

        // If the server is marked under a different Gamemode, don't start
        if (MarkedGamemode != null && MarkedGamemode != this)
        {
            FusionNotifier.Send(new FusionNotification()
            {
                isMenuItem = false,
                isPopup = true,
                type = NotificationType.ERROR,
                showTitleOnPopup = true,
                title = "Failed To Start",
                message = "You cannot start this Gamemode because another Gamemode is marked!"
            });
            return false;
        }

        // Check if theres an already active gamemode
        if (ActiveGamemode != null)
        {
            // If we can't force stop, just return.
            if (!forceStopCurrent)
                return false;

            // Otherwise, force stop the active gamemode
            ActiveGamemode.StopGamemode();
        }

        Metadata.TrySetMetadata(GamemodeHelper.IsStartedKey, "True");
        return true;
    }

    public bool StopGamemode()
    {
        // We can only stop the gamemode as a server!
        if (!NetworkInfo.IsServer)
        {
            return false;
        }

        // Make sure the active gamemode is us
        if (!IsActive())
            return false;

        Metadata.TrySetMetadata(GamemodeHelper.IsStartedKey, "False");
        return true;
    }

    public bool MarkGamemode()
    {
        if (_isGamemodeRunning)
            return false;

        if (MarkedGamemode != null && MarkedGamemode != this)
            MarkedGamemode.UnmarkGamemode();

        _markedGamemode = this;

        OnMarkGamemode();

        BoneMenuCreator.SetMarkedGamemodeText($"Unmark {GamemodeName}");
        return true;
    }

    public bool UnmarkGamemode()
    {
        if (_markedGamemode != this)
            return false;

        _markedGamemode = null;

        OnUnmarkGamemode();

        BoneMenuCreator.SetMarkedGamemodeText("No Marked Gamemode");
        return true;
    }

    internal void Internal_SetGamemodeState(bool isStarted)
    {
        // Make sure we aren't doing this twice
        if (IsStarted == isStarted)
            return;

        if (isStarted)
        {
            MultiplayerHooking.OnShouldAllowConnection += Internal_UserJoinCheck;

            GamemodeManager.Internal_SetActiveGamemode(this);
            IsStarted = true;
            OnStartGamemode();
        }
        else
        {
            MultiplayerHooking.OnShouldAllowConnection -= Internal_UserJoinCheck;

            GamemodeManager.Internal_SetActiveGamemode(null);
            IsStarted = false;
            OnStopGamemode();
        }
    }

    private bool Internal_UserJoinCheck(PlayerId playerId, out string reason)
    {
        if (ActiveGamemode == this && (PreventNewJoins || !LateJoining))
        {
            reason = $"Gamemode {GamemodeName} is currently running!";
            return false;
        }

        reason = "";
        return true;
    }

    // Gamemode state
    protected virtual void OnStartGamemode()
    {
        // Set default bonemenu element
        if (_gamemodeToggleElement != null)
        {
            _gamemodeToggleElement.SetName("Stop Gamemode");
        }
    }

    protected virtual void OnStopGamemode()
    {
        // Set default bonemenu element
        if (_gamemodeToggleElement != null)
        {
            _gamemodeToggleElement.SetName("Start Gamemode");
        }
    }

    protected virtual void OnMarkGamemode()
    {
        // Set default bonemenu element
        if (_gamemodeMarkElement != null)
        {
            _gamemodeMarkElement.SetName("Unmark Gamemode");
        }
    }

    protected virtual void OnUnmarkGamemode()
    {
        // Set default bonemenu element
        if (_gamemodeMarkElement != null)
        {
            _gamemodeMarkElement.SetName("Mark Gamemode");
        }
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
        switch (key)
        {
            case GamemodeHelper.IsStartedKey:
                Internal_SetGamemodeState(value == bool.TrueString);
                break;
        }
    }

    protected virtual void OnMetadataChanged(string key, string value) { }

    protected virtual void OnMetadataRemoved(string key, string value) { }
}