using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppUltEvents;

using Il2CppInterop.Runtime.InteropTypes.Fields;

using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Player;
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
            MultiplayerHooking.OnJoinedServer += OnServerJoined;
            MultiplayerHooking.OnStartedServer += OnServerJoined;
            MultiplayerHooking.OnDisconnected += OnServerLeft;
            
            // If we're already in a server, invoke the UltEvent
            if (HasServer())
            {
                OnServerJoined();
            }
        }

        private void OnDestroy()
        {
            MultiplayerHooking.OnJoinedServer -= OnServerJoined;
            MultiplayerHooking.OnStartedServer -= OnServerJoined;
            MultiplayerHooking.OnDisconnected -= OnServerLeft;
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
            return NetworkInfo.IsHost;
        }

        public bool HasServer()
        {
            return NetworkInfo.HasServer;
        }

        public string GetServerName()
        {
            if (!NetworkInfo.HasServer)
            {
                return null;
            }

            return LobbyInfoManager.LobbyInfo.LobbyName;
        }

        public int GetPlayerCount()
        {
            return PlayerIDManager.PlayerCount;
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

        public string GetServerName()
        {
            return null;
        }

        public int GetPlayerCount()
        {
            return 0;
        }
#endif
    }
}