using LabFusion.Network;
using LabFusion.UI.Popups;
using LabFusion.Scene;
using LabFusion.SDK.Metadata;
using LabFusion.Utilities;

namespace LabFusion.SDK.Gamemodes;

public static class GamemodeManager
{
    private static Gamemode _activeGamemode = null;

    /// <summary>
    /// The currently active Gamemode.
    /// </summary>
    public static Gamemode ActiveGamemode
    {
        get
        {
            return _activeGamemode;
        }
        private set
        {
            if (_activeGamemode != null)
            {
                _activeGamemode.OnGamemodeDeselected();

                _activeGamemode.ClearMetadata();
            }

            _activeGamemode = value;

            if (value != null)
            {
                value.ClearMetadata();

                value.OnGamemodeSelected();
            }

            OnGamemodeChanged.InvokeSafe(value, "executing hook OnGamemodeChanged");

            SendGamemodeChangeNotification();

#if DEBUG
            FusionLogger.Log($"Active Gamemode changed to {(value != null ? value.Title : "none")}!");
#endif
        }
    }

    public static bool IsGamemodeStarted => ActiveGamemode != null && ActiveGamemode.IsStarted;

    public static bool IsGamemodeReady => ActiveGamemode != null && ActiveGamemode.IsReady;

    public static event Action<Gamemode> OnGamemodeChanged;

    public static event Action OnGamemodeStarted, OnGamemodeStopped, OnGamemodeReady, OnGamemodeUnready;

    public static event Action<float> OnStartTimerChanged;

    private static float _startTimer = DefaultStartTime;
    public static float StartTimer
    {
        get
        {
            return _startTimer;
        }
        set
        {
            _startTimer = value;

            OnStartTimerChanged?.InvokeSafe(value, "executing OnStartTimerChanged hook");
        }
    }

    private static bool _startTimerActive = false;
    public static bool StartTimerActive => _startTimerActive;

    public const float DefaultStartTime = 30f;

    public const string NotificationTag = "Gamemode";

    public static void OnInitializeMelon()
    {
        Gamemode.OnStartedKeyChanged += OnStartedKeyChanged;
        Gamemode.OnSelectedKeyChanged += OnSelectedKeyChanged;
        Gamemode.OnReadyKeyChanged += OnReadyKeyChanged;

        MultiplayerHooking.OnMainSceneInitialized += OnMainSceneInitialized;
        MultiplayerHooking.OnTargetLevelLoaded += OnTargetLevelLoaded;

        MultiplayerHooking.OnDisconnected += OnDisconnect;
    }

    private static void OnTargetLevelLoaded()
    {
        if (IsGamemodeStarted)
        {
            ActiveGamemode.OnLevelReady();
        }
    }

    private static void OnMainSceneInitialized()
    {
        // Stop the current gamemode
        if (NetworkInfo.IsHost && IsGamemodeStarted && ActiveGamemode.AutoStopOnSceneLoad)
        {
            StopGamemode();
        }
    }

    private static void OnDisconnect()
    {
        StopGamemode();

        // Reset all metadata
        var gamemode = ActiveGamemode;

        if (gamemode != null)
        {
            gamemode.Metadata.ForceSetLocalMetadata(GamemodeKeys.ReadyKey, bool.FalseString);
            gamemode.Metadata.ForceSetLocalMetadata(GamemodeKeys.StartedKey, bool.FalseString);
            gamemode.Metadata.ForceSetLocalMetadata(GamemodeKeys.SelectedKey, bool.FalseString);
        }
    }

    private static void SendGamemodeChangeNotification()
    {
        Notifier.Cancel(NotificationTag);

        if (ActiveGamemode != null)
        {
            Notifier.Send(new Notification()
            {
                Message = $"{ActiveGamemode.Title} is selected! Waiting until conditions are met...",
                Title = "Gamemode Selected",
                Tag = NotificationTag,
                Type = NotificationType.INFORMATION,
                ShowPopup = true,
                SaveToMenu = false,
                PopupLength = 1.5f,
            });
        }
        else
        {
            Notifier.Send(new Notification()
            {
                Message = "The server is now in Sandbox mode!",
                Title = "Gamemode Deselected",
                Tag = NotificationTag,
                Type = NotificationType.INFORMATION,
                ShowPopup = true,
                SaveToMenu = false,
                PopupLength = 1.5f,
            });
        }
    }

    private static void OnStartedKeyChanged(Gamemode gamemode, bool started)
    {
        if (started)
        {
            gamemode.OnGamemodeStarted();

            if (FusionSceneManager.HasTargetLoaded())
            {
                gamemode.OnLevelReady();
            }

            OnGamemodeStarted?.InvokeSafe("executing OnGamemodeStarted");

#if DEBUG
            FusionLogger.Log($"Gamemode {gamemode.Title} started!");
#endif
        }
        else
        {
            gamemode.OnGamemodeStopped();

            OnGamemodeStopped?.InvokeSafe("executing OnGamemodeStopped");

            // Restart the ready timer
            if (IsGamemodeReady)
            {
                StartReadyTimer();
            }

#if DEBUG
            FusionLogger.Log($"Gamemode {gamemode.Title} stopped!");
#endif
        }
    }

    private static void OnSelectedKeyChanged(Gamemode gamemode, bool selected)
    {
        if (selected)
        {
            ActiveGamemode = gamemode;
        }
        else
        {
            ActiveGamemode = null;
        }
    }

    private static void OnReadyKeyChanged(Gamemode gamemode, bool ready)
    {
        if (ready)
        {
            gamemode.OnGamemodeReady();

            OnGamemodeReady?.InvokeSafe("executing OnGamemodeReady hook");
        }
        else
        {
            gamemode.OnGamemodeUnready();

            OnGamemodeUnready?.InvokeSafe("executing OnGamemodeUnready hook");
        }

        if (IsGamemodeStarted)
        {
            return;
        }

        if (ready)
        {
            StartReadyTimer();

#if DEBUG
            FusionLogger.Log($"Gamemode {gamemode.Title} is now ready!");
#endif
        }
        else
        {
            StopReadyTimer();

#if DEBUG
            FusionLogger.Log($"Gamemode {gamemode.Title} is no longer ready!");
#endif
        }

        gamemode.ClearMetadata();
    }

    private static void StartReadyTimer()
    {
        if (ActiveGamemode != null)
        {
            Notifier.Cancel(NotificationTag);

            Notifier.Send(new Notification()
            {
                Message = $"{ActiveGamemode.Title} is ready! Starting in {GetInitialStartTime()} seconds!",
                Title = "Gamemode Ready",
                Tag = NotificationTag,
                Type = NotificationType.SUCCESS,
                ShowPopup = true,
                SaveToMenu = false,
                PopupLength = 2f,
            });
        }

        _startTimerActive = true;

        StartTimer = GetInitialStartTime();
    }

    private static void StopReadyTimer()
    {
        if (ActiveGamemode != null)
        {
            Notifier.Cancel(NotificationTag);

            Notifier.Send(new Notification()
            {
                Message = $"{ActiveGamemode.Title} is no longer ready.",
                Title = "Gamemode Unready",
                Tag = NotificationTag,
                Type = NotificationType.ERROR,
                ShowPopup = true,
                PopupLength = 2f,
            });
        }

        _startTimerActive = false;

        StartTimer = GetInitialStartTime();
    }

    private static float GetInitialStartTime()
    {
        if (!NetworkInfo.HasServer)
        {
            return DefaultStartTime;
        }

        return LobbyInfoManager.LobbyInfo.TimeBetweenGamemodeRounds;
    }

    /// <summary>
    /// Selects a Gamemode.
    /// </summary>
    /// <param name="gamemode">The Gamemode to select.</param>
    public static void SelectGamemode(Gamemode gamemode)
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        if (ActiveGamemode != null)
        {
            DeselectGamemode();
        }

        gamemode.Metadata.TrySetMetadata(GamemodeKeys.SelectedKey, bool.TrueString);
    }

    /// <summary>
    /// Deselects the <see cref="ActiveGamemode"/> if this is the server.
    /// </summary>
    public static void DeselectGamemode()
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        if (ActiveGamemode == null)
        {
            return;
        }

        if (IsGamemodeStarted)
        {
            StopGamemode();
        }

        if (IsGamemodeReady)
        {
            UnreadyGamemode();
        }

        ActiveGamemode.Metadata.TrySetMetadata(GamemodeKeys.SelectedKey, bool.FalseString);
    }

    /// <summary>
    /// Starts the <see cref="ActiveGamemode"/> if one is selected and this is the server.
    /// </summary>
    public static void StartGamemode()
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        if (ActiveGamemode == null)
        {
            return;
        }

        if (IsGamemodeStarted)
        {
            StopGamemode();
        }

        if (!IsGamemodeReady)
        {
            ReadyGamemode();
        }

        ActiveGamemode.Metadata.TrySetMetadata(GamemodeKeys.StartedKey, bool.TrueString);
    }

    /// <summary>
    /// Stops the <see cref="ActiveGamemode"/> if it started and this is the server.
    /// </summary>
    public static void StopGamemode()
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        if (!IsGamemodeStarted)
        {
            return;
        }

        ActiveGamemode.Metadata.TrySetMetadata(GamemodeKeys.StartedKey, bool.FalseString);
    }

    /// <summary>
    /// Checks the <see cref="ActiveGamemode"/>'s ready conditions and either readies or unreadies it.
    /// </summary>
    public static void ValidateReadyConditions()
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        if (ActiveGamemode == null)
        {
            return;
        }

        bool ready = ActiveGamemode.CheckReadyConditions();

        if (ready && !IsGamemodeReady)
        {
            ReadyGamemode();
        }
        else if (!ready && IsGamemodeReady)
        {
            UnreadyGamemode();
        }
    }

    /// <summary>
    /// Readies the <see cref="ActiveGamemode"/> to be started if this is the server.
    /// </summary>
    public static void ReadyGamemode()
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        if (ActiveGamemode == null || IsGamemodeReady)
        {
            return;
        }

        ActiveGamemode.Metadata.TrySetMetadata(GamemodeKeys.ReadyKey, bool.TrueString);
    }

    /// <summary>
    /// Unreadies the <see cref="ActiveGamemode"/> if this is the server.
    /// </summary>
    public static void UnreadyGamemode()
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        if (ActiveGamemode == null || !IsGamemodeReady)
        {
            return;
        }

        ActiveGamemode.Metadata.TrySetMetadata(GamemodeKeys.ReadyKey, bool.FalseString);
    }

    internal static void OnFixedUpdate()
    {
        ActiveGamemode?.FixedUpdate();
    }

    internal static void OnUpdate()
    {
        ActiveGamemode?.Update();

        // Countdown timer
        if (!FusionSceneManager.IsLoading() && !IsGamemodeStarted && IsGamemodeReady && _startTimerActive)
        {
            StartTimer -= TimeUtilities.DeltaTime;

            if (StartTimer <= 0f)
            {
                StartTimer = 0f;
                _startTimerActive = false;

                StartGamemode();
            }
        }
    }

    internal static void OnLateUpdate()
    {
        ActiveGamemode?.LateUpdate();
    }

    /// <summary>
    /// Attempts to get a Gamemode from its barcode.
    /// </summary>
    /// <param name="barcode">The barcode.</param>
    /// <param name="gamemode">The Gamemode.</param>
    /// <returns>If it successfully found a Gamemode.</returns>
    public static bool TryGetGamemode(string barcode, out Gamemode gamemode)
    {
        return GamemodeRegistration.TryGetGamemode(barcode, out gamemode);
    }

    /// <summary>
    /// Gets a Gamemode from its barcode.
    /// </summary>
    /// <param name="barcode">The barcode.</param>
    /// <returns>The Gamemode</returns>
    public static Gamemode GetGamemode(string barcode)
    {
        TryGetGamemode(barcode, out var gamemode);
        return gamemode;
    }

    /// <summary>
    /// Attempts to get a Gamemode with a specific type.
    /// </summary>
    /// <typeparam name="TGamemode">The type of the Gamemode.</typeparam>
    /// <param name="gamemode">The Gamemode.</param>
    /// <returns>If it successfully found a Gamemode.</returns>
    public static bool TryGetGamemode<TGamemode>(out TGamemode gamemode) where TGamemode : Gamemode
    {
        // Try find the gamemode from the type
        foreach (var other in Gamemodes)
        {
            if (other is TGamemode)
            {
                gamemode = other as TGamemode;
                return true;
            }
        }

        gamemode = null;
        return false;
    }

    /// <summary>
    /// The list of all registered Gamemodes.
    /// </summary>
    public static IReadOnlyCollection<Gamemode> Gamemodes => GamemodeRegistration.Gamemodes;
}