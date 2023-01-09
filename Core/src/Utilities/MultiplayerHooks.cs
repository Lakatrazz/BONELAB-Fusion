using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BoneLib;

using LabFusion.Representation;

namespace LabFusion.Utilities {
    public static class MultiplayerHooks {
        // Server hooks
        public static Action OnStartServer, OnJoinServer, OnDisconnect;
        public static Action<PlayerId> OnPlayerJoin;

        internal static void Internal_OnStartServer() => SafeActions.InvokeActionSafe(OnStartServer);
        internal static void Internal_OnJoinServer() => SafeActions.InvokeActionSafe(OnJoinServer);
        internal static void Internal_OnDisconnect() => SafeActions.InvokeActionSafe(OnDisconnect);
        internal static void Internal_OnPlayerJoin(PlayerId id) => SafeActions.InvokeActionSafe(OnPlayerJoin, id);

        // Unity hooks
        public static Action OnUpdate, OnFixedUpdate, OnLateUpdate,
            OnMainSceneInitialized;

        internal static void Internal_OnUpdate() => SafeActions.InvokeActionSafe(OnUpdate);
        internal static void Internal_OnFixedUpdate() => SafeActions.InvokeActionSafe(OnFixedUpdate);
        internal static void Internal_OnLateUpdate() => SafeActions.InvokeActionSafe(OnLateUpdate);
        internal static void Internal_OnMainSceneInitialized() => SafeActions.InvokeActionSafe(OnMainSceneInitialized);
    }
}
