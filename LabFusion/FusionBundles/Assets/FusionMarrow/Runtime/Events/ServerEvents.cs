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
    public class ServerEvents : MonoBehaviour
    {
#if MELONLOADER
        public ServerEvents(IntPtr intPtr) : base(intPtr) { }

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

        public bool IsHost()
        {
            return NetworkInfo.IsServer;
        }

        public bool HasServer()
        {
            return NetworkInfo.HasServer;
        }
#else
        public UltEvent onServerJoined;

        public UltEvent onServerLeft;

        public bool IsHost()
        {
            return false;
        }
        
        public bool HasServer()
        {
            return false;
        }
#endif
    }
}