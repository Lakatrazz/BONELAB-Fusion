using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Senders;

namespace LabFusion.Utilities {
    public delegate void ServerEvent();
    public delegate void UpdateEvent();
    public delegate void PlayerUpdate(PlayerId playerId);
    public delegate void PlayerAction(PlayerId playerId, PlayerActionType type);

    /// <summary>
    /// Hooks for getting events from the server, players, etc.
    /// <para> All hooks are events. You cannot invoke them yourself. </para>
    /// </summary>
    public static class MultiplayerHooking {
        // Server hooks
        public static event ServerEvent OnStartServer, OnJoinServer, OnDisconnect;
        public static event PlayerUpdate OnPlayerJoin;
        public static event PlayerAction OnPlayerAction;

        internal static void Internal_OnStartServer() => OnStartServer.InvokeSafe();

        internal static void Internal_OnJoinServer() => OnJoinServer.InvokeSafe();

        internal static void Internal_OnDisconnect() => OnDisconnect.InvokeSafe();

        internal static void Internal_OnPlayerJoin(PlayerId id) => OnPlayerJoin.InvokeSafe(id);

        internal static void Internal_OnPlayerAction(PlayerId id, PlayerActionType type) => OnPlayerAction.InvokeSafe(id, type);

        // Unity hooks
        public static event UpdateEvent OnUpdate, OnFixedUpdate, OnLateUpdate,
            OnMainSceneInitialized;

        internal static void Internal_OnUpdate() => OnUpdate.InvokeSafe();
        internal static void Internal_OnFixedUpdate() => OnFixedUpdate.InvokeSafe();
        internal static void Internal_OnLateUpdate() => OnLateUpdate.InvokeSafe();
        internal static void Internal_OnMainSceneInitialized() => OnMainSceneInitialized.InvokeSafe();
    }
}
