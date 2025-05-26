using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppUltEvents;

using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppInterop.Runtime.Attributes;

using LabFusion.Utilities;
using LabFusion.Player;
using LabFusion.Network;
using LabFusion.Scene;
#else
using UltEvents;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class PlayerEvents : MonoBehaviour
    {
#if MELONLOADER
        public PlayerEvents(IntPtr intPtr) : base(intPtr) { }

        public Il2CppReferenceField<UltEventHolder> onPlayerLoadedHolder;

        public Il2CppReferenceField<UltEventHolder> onPlayerUnloadedHolder;

        public Il2CppReferenceField<UltEventHolder> onAllPlayersLoadedHolder;

        private int _latestPlayerID = -1;

        private void Awake()
        {
            NetworkSceneManager.OnPlayerLoadedIntoLevel += OnPlayerLoadedIntoLevel;
            NetworkSceneManager.OnPlayerStartedLoading += OnPlayerStartedLoading;
            NetworkSceneManager.OnAllPlayersLoaded += OnAllPlayersLoaded;

            MultiplayerHooking.OnPlayerLeft += OnPlayerLeft;
        }

        private void OnDestroy()
        {
            NetworkSceneManager.OnPlayerLoadedIntoLevel -= OnPlayerLoadedIntoLevel;
            NetworkSceneManager.OnPlayerStartedLoading -= OnPlayerStartedLoading;
            NetworkSceneManager.OnAllPlayersLoaded -= OnAllPlayersLoaded;

            MultiplayerHooking.OnPlayerLeft -= OnPlayerLeft;
        }

        [HideFromIl2Cpp]
        private void OnPlayerLoadedIntoLevel(PlayerID playerId, string barcode)
        {
            if (barcode != FusionSceneManager.Barcode)
            {
                return;
            }

            _latestPlayerID = playerId.SmallID;

            onPlayerLoadedHolder.Get()?.Invoke();
        }

        [HideFromIl2Cpp]
        private void OnPlayerStartedLoading(PlayerID playerId)
        {
            _latestPlayerID = playerId.SmallID;

            onPlayerUnloadedHolder.Get()?.Invoke();
        }

        [HideFromIl2Cpp]
        private void OnAllPlayersLoaded()
        {
            _latestPlayerID = -1;

            onAllPlayersLoadedHolder.Get()?.Invoke();
        }

        [HideFromIl2Cpp]
        private void OnPlayerLeft(PlayerID playerId)
        {
            if (playerId.Metadata.Loading.GetValue())
            {
                return;
            }

            _latestPlayerID = playerId.SmallID;

            onPlayerUnloadedHolder.Get()?.Invoke();
        }

        public int GetLatestPlayerID()
        {
            return _latestPlayerID;
        }

        public string GetPlayerUsername(int playerID)
        {
            if (!NetworkInfo.HasServer)
            {
                return null;
            }

            var player = PlayerIDManager.GetPlayerID((byte)playerID);

            if (player == null)
            {
                return null;
            }

            return player.Metadata.Username.GetValue();
        }

        public int GetLocalPlayerID()
        {
            if (!NetworkInfo.HasServer)
            {
                return -1;
            }

            return PlayerIDManager.LocalSmallID;
        }

        public int GetHostID()
        {
            if (!NetworkInfo.HasServer)
            {
                return -1;
            }

            return PlayerIDManager.HostSmallID;
        }

        public int GetLevelHostID() => GetHostID();
#else
        public UltEventHolder onPlayerLoadedHolder;

        public UltEventHolder onPlayerUnloadedHolder;

        public UltEventHolder onAllPlayersLoadedHolder;

        public int GetLatestPlayerID()
        {
            return -1;
        }

        public string GetPlayerUsername(int playerID)
        {
            return null;
        }

        public int GetLocalPlayerID()
        {
            return -1;
        }

        public int GetHostID()
        {
            return -1;
        }

        public int GetLevelHostID()
        {
            return -1;
        }
#endif
    }
}