using LabFusion.Player;
using LabFusion.Senders;

namespace LabFusion.Utilities;

public delegate bool UserAccessEvent(PlayerId playerId, out string reason);
public delegate void ServerEvent();
public delegate void UpdateEvent();
public delegate void PlayerUpdate(PlayerId playerId);
public delegate void PlayerAction(PlayerId playerId, PlayerActionType type, PlayerId otherPlayer = null);

/// <summary>
/// Hooks for getting events from the server, players, etc.
/// <para> All hooks are events. You cannot invoke them yourself. </para>
/// </summary>
public static class MultiplayerHooking
{
    // Confirmation hooks
    public static event UserAccessEvent OnShouldAllowConnection;

    // Server hooks
    public static event ServerEvent OnStartServer, OnJoinServer, OnDisconnect;
    public static event PlayerUpdate OnPlayerJoin, OnPlayerLeave;
    public static event PlayerAction OnPlayerAction;
    public static event PlayerUpdate OnPlayerCatchup;

    internal static bool Internal_OnShouldAllowConnection(PlayerId playerId, out string reason)
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

    internal static void Internal_OnStartServer() => OnStartServer.InvokeSafe("executing OnStartServer hook");

    internal static void Internal_OnJoinServer() => OnJoinServer.InvokeSafe("executing OnJoinServer hook");

    internal static void Internal_OnDisconnect() => OnDisconnect.InvokeSafe("executing OnDisconnect hook");

    internal static void Internal_OnPlayerJoin(PlayerId id) => OnPlayerJoin.InvokeSafe(id, "executing OnPlayerJoin hook");

    internal static void Internal_OnPlayerLeave(PlayerId id) => OnPlayerLeave.InvokeSafe(id, "executing OnPlayerLeave hook");

    internal static void Internal_OnPlayerAction(PlayerId id, PlayerActionType type, PlayerId otherPlayer = null) => OnPlayerAction.InvokeSafe(id, type, otherPlayer, "executing OnPlayerAction hook");

    internal static void Internal_OnPlayerCatchup(PlayerId playerId) => OnPlayerCatchup.InvokeSafe(playerId, "executing OnPlayerCatchup hook");

    // Unity hooks
    /// <summary>
    /// A hook for frame updates. Errors are not caught for performance reasons, please use carefully!
    /// </summary>
    public static event UpdateEvent OnUpdate, OnFixedUpdate, OnLateUpdate;

    public static event UpdateEvent OnMainSceneInitialized, OnLoadingBegin;

    internal static void Internal_OnUpdate() => OnUpdate?.Invoke();
    internal static void Internal_OnFixedUpdate() => OnFixedUpdate?.Invoke();
    internal static void Internal_OnLateUpdate() => OnLateUpdate?.Invoke();
    internal static void Internal_OnMainSceneInitialized() => OnMainSceneInitialized.InvokeSafe("executing OnMainSceneInitialized hook");
    internal static void Internal_OnLoadingBegin() => OnLoadingBegin.InvokeSafe("executing OnLoadingBegin hook");
}