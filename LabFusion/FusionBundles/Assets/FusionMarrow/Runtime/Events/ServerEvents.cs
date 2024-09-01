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

        public Il2CppReferenceField<UltEventHolder> onServerJoinedHolder;

        public Il2CppReferenceField<UltEventHolder> onServerLeftHolder;

        private void Awake()
        {
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
            onServerJoinedHolder.Get()?.Invoke();
        }

        private void OnServerLeft()
        {
            onServerLeftHolder.Get()?.Invoke();
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
        public UltEventHolder onServerJoinedHolder;

        public UltEventHolder onServerLeftHolder;

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