using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppUltEvents;

using Il2CppInterop.Runtime.InteropTypes.Fields;

using LabFusion.Network;
using LabFusion.Utilities;
#else
using UltEvents;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class FusionServerEvents : MonoBehaviour
    {
#if MELONLOADER
        public FusionServerEvents(IntPtr intPtr) : base(intPtr) { }

        public Il2CppReferenceField<UltEvent> onServerJoined;

        public Il2CppReferenceField<UltEvent> onServerLeft;

        private UltEvent _onServerJoinedCached = null;
        private UltEvent _onServerLeftCached = null;

        private void Awake()
        {
            _onServerJoinedCached = onServerJoined.Get();
            _onServerLeftCached = onServerLeft.Get();

            MultiplayerHooking.OnJoinServer += OnServerJoined;
            MultiplayerHooking.OnStartServer += OnServerJoined;
            MultiplayerHooking.OnDisconnect += OnServerLeft;
            
            // If we're already in a server, invoke the UltEvent
            if (HasServer())
            {
                OnServerJoined();
            }
        }

        private void OnDestroy()
        {
            MultiplayerHooking.OnJoinServer -= OnServerJoined;
            MultiplayerHooking.OnStartServer -= OnServerJoined;
            MultiplayerHooking.OnDisconnect -= OnServerLeft;
        }

        private void OnServerJoined()
        {
            _onServerJoinedCached?.Invoke();
        }

        private void OnServerLeft()
        {
            _onServerLeftCached?.Invoke();
        }

        public static bool IsHost()
        {
            return NetworkInfo.IsServer;
        }

        public static bool HasServer()
        {
            return NetworkInfo.HasServer;
        }
#else
        public UltEvent onServerJoined;

        public UltEvent onServerLeft;

        public static bool IsHost()
        {
            return false;
        }
        
        public static bool HasServer()
        {
            return false;
        }
#endif
    }
}