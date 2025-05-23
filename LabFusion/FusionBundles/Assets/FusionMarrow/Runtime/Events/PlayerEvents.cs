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
        private void OnPlayerLoadedIntoLevel(PlayerId playerId, string barcode)
        {
            if (barcode != FusionSceneManager.Barcode)
            {
                return;
            }

            _latestPlayerID = playerId.SmallId;

            onPlayerLoadedHolder.Get()?.Invoke();
        }

        [HideFromIl2Cpp]
        private void OnPlayerStartedLoading(PlayerId playerId)
        {
            _latestPlayerID = playerId.SmallId;

            onPlayerUnloadedHolder.Get()?.Invoke();
        }

        [HideFromIl2Cpp]
        private void OnAllPlayersLoaded()
        {
            _latestPlayerID = -1;

            onAllPlayersLoadedHolder.Get()?.Invoke();
        }

        [HideFromIl2Cpp]
        private void OnPlayerLeft(PlayerId playerId)
        {
            if (playerId.Metadata.Loading.GetValue())
            {
                return;
            }

            _latestPlayerID = playerId.SmallId;

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

            var player = PlayerIdManager.GetPlayerId((byte)playerID);

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

            return PlayerIdManager.LocalSmallId;
        }

        public int GetHostID()
        {
            if (!NetworkInfo.HasServer)
            {
                return -1;
            }

            return PlayerIdManager.HostSmallId;
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