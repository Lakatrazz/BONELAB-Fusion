using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppUltEvents;

using Il2CppInterop.Runtime.InteropTypes.Fields;

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

        public Il2CppReferenceField<UltEventHolder> onPlayerJoinedHolder;

        public Il2CppReferenceField<UltEventHolder> onPlayerLeftHolder;

        public Il2CppReferenceField<UltEventHolder> onAllPlayersLoadedHolder;

        private int _latestPlayerID = -1;

        private void Awake()
        {
            MultiplayerHooking.OnPlayerJoined += OnPlayerJoined;
            MultiplayerHooking.OnPlayerLeft += OnPlayerLeft;
            NetworkSceneManager.OnAllPlayersLoaded += OnAllPlayersLoaded;
        }

        private void OnDestroy()
        {
            MultiplayerHooking.OnPlayerJoined -= OnPlayerJoined;
            MultiplayerHooking.OnPlayerLeft -= OnPlayerLeft;
            NetworkSceneManager.OnAllPlayersLoaded -= OnAllPlayersLoaded;
        }

        private void OnPlayerJoined(PlayerId playerId)
        {
            _latestPlayerID = playerId.SmallId;

            onPlayerJoinedHolder.Get()?.Invoke();
        }

        private void OnPlayerLeft(PlayerId playerId)
        {
            _latestPlayerID = playerId.SmallId;

            onPlayerLeftHolder.Get()?.Invoke();
        }

        private void OnAllPlayersLoaded()
        {
            _latestPlayerID = -1;

            onAllPlayersLoadedHolder.Get()?.Invoke();
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
        public UltEventHolder onPlayerJoinedHolder;

        public UltEventHolder onPlayerLeftHolder;

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