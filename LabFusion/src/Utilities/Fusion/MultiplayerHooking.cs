using LabFusion.Player;
using LabFusion.Senders;

namespace LabFusion.Utilities;

public delegate bool UserAccessEvent(PlayerID playerId, out string reason);
public delegate void ServerEvent();
public delegate void UpdateEvent();
public delegate void PlayerUpdate(PlayerID playerId);
public delegate void PlayerAction(PlayerID playerId, PlayerActionType type, PlayerID otherPlayer = null);

/// <summary>
/// Hooks for getting events from the server, players, etc.
/// <para> All hooks are events. You cannot invoke them yourself. </para>
/// </summary>
public static class MultiplayerHooking
{
    // Confirmation hooks
    public static event UserAccessEvent OnShouldAllowConnection;

    // Server hooks
    public static event ServerEvent OnStartedServer, OnJoinedServer, OnDisconnected;
    public static event PlayerUpdate OnPlayerJoined, OnPlayerLeft;
    public static event PlayerAction OnPlayerAction;

    internal static bool CheckShouldAllowConnection(PlayerID playerId, out string reason)
    {
        reason = "";

        if (OnShouldAllowConnection == null)
            return true;

        foreach (var invocation in OnShouldAllowConnection.GetInvocationList())
        {
            var accessEvent = (UserAccessEvent)invocation;

            if (!accessEvent.Invoke(playerId, out reason))
                return false;
        }

        return true;
    }

    internal static void InvokeOnStartedServer() => OnStartedServer.InvokeSafe("executing OnStartedServer hook");

    internal static void InvokeOnJoinedServer() => OnJoinedServer.InvokeSafe("executing OnJoinedServer hook");

    internal static void InvokeOnDisconnected() => OnDisconnected.InvokeSafe("executing OnDisconnected hook");

    internal static void InvokeOnPlayerJoined(PlayerID id) => OnPlayerJoined.InvokeSafe(id, "executing OnPlayerJoined hook");

    internal static void InvokeOnPlayerLeft(PlayerID id) => OnPlayerLeft.InvokeSafe(id, "executing OnPlayerLeft hook");

    internal static void InvokeOnPlayerAction(PlayerID id, PlayerActionType type, PlayerID otherPlayer = null) => OnPlayerAction.InvokeSafe(id, type, otherPlayer, "executing OnPlayerAction hook");

    // Unity hooks
    /// <summary>
    /// A hook for frame updates. Errors are not caught for performance reasons, please use carefully!
    /// </summary>
    public static event UpdateEvent OnUpdate, OnFixedUpdate, OnLateUpdate;

    public static event UpdateEvent OnMainSceneInitialized, OnLoadingBegin, OnTargetLevelLoaded;

    internal static void InvokeOnUpdate() => OnUpdate?.Invoke();
    internal static void InvokeOnFixedUpdate() => OnFixedUpdate?.Invoke();
    internal static void InvokeOnLateUpdate() => OnLateUpdate?.Invoke();
    internal static void InvokeOnMainSceneInitialized() => OnMainSceneInitialized.InvokeSafe("executing OnMainSceneInitialized hook");
    internal static void InvokeOnLoadingBegin() => OnLoadingBegin.InvokeSafe("executing OnLoadingBegin hook");
    internal static void InvokeTargetLevelLoaded() => OnTargetLevelLoaded.InvokeSafe("executing OnTargetLevelLoaded hook");
}