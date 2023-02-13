using BoneLib;
using BoneLib.BoneMenu.Elements;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Gamemodes {
    public class Deathmatch : Gamemode {
        private const int _defaultMinutes = 3;
        private const int _minMinutes = 2;
        private const int _maxMinutes = 60;

        // Prefix
        public const string DefaultPrefix = "InternalDeathmatchMetadata";

        // Default metadata keys
        public const string PlayerScoreKey = DefaultPrefix + ".Score";

        public override string GamemodeCategory => "Fusion";
        public override string GamemodeName => "Deathmatch";

        public override bool DisableDevTools => true;
        public override bool DisableSpawnGun => true;
        public override bool DisableManualUnragdoll => true;

        private float _timeOfStart;
        private bool _oneMinuteLeft;

        private int _totalMinutes = _defaultMinutes;

        public override void OnBoneMenuCreated(MenuCategory category) {
            base.OnBoneMenuCreated(category);

            category.CreateIntElement("Round Minutes", Color.white, _totalMinutes, 1, _minMinutes, _maxMinutes, (v) => {
                _totalMinutes = v;
            });
        }

        public override void OnMainSceneInitialized() {
            DefaultPlaylist();
        }

        private void DefaultPlaylist() {
            SetPlaylist(0.7f, FusionBundleLoader.SyntheticCavernsRemix, FusionBundleLoader.WWWWonderLan);
        }

        public IReadOnlyList<PlayerId> GetPlayersByScore() {
            if (!IsActive())
                return null;

            List<PlayerId> leaders = new List<PlayerId>(PlayerIdManager.PlayerIds);
            leaders = leaders.OrderBy(id => GetScore(id)).ToList();
            leaders.Reverse();

            return leaders;
        }

        public PlayerId GetByScore(int place) {
            var players = GetPlayersByScore();

            if (players != null && players.Count > place)
                return players[place];
            return null;
        }

        public int GetPlace(PlayerId id) {
            var players = GetPlayersByScore();

            for (var i = 0; i < players.Count; i++) {
                if (players[i] == id) {
                    return i + 1;
                }
            }

            return -1;
        }

        public override void OnGamemodeRegistered() {
            // Add hooks
            MultiplayerHooking.OnPlayerAction += OnPlayerAction;

            DefaultPlaylist();
        }

        public override void OnGamemodeUnregistered() {
            // Remove hooks
            MultiplayerHooking.OnPlayerAction -= OnPlayerAction;
        }

        protected void OnPlayerAction(PlayerId player, PlayerActionType type, PlayerId otherPlayer = null) {
            if (IsActive() && NetworkInfo.IsServer) {
                switch (type) {
                    case PlayerActionType.DEATH_BY_OTHER_PLAYER:
                        if (otherPlayer != null && otherPlayer != player) {
                            IncrementScore(otherPlayer);
                        }
                        break;
                }
            }
        }

        protected override void OnStartGamemode() {
            base.OnStartGamemode();

            if (NetworkInfo.IsServer) {
                ResetScores();
            }

            FusionNotifier.Send(new FusionNotification()
            {
                title = "Deathmatch Started",
                showTitleOnPopup = true,
                message = "Good luck!",
                isMenuItem = false,
                isPopup = true,
            });

            _timeOfStart = Time.realtimeSinceStartup;
            _oneMinuteLeft = false;

            // Force mortality
            FusionPlayer.SetMortality(true);

            // Setup ammo
            FusionPlayer.SetAmmo(1000);
        }

        protected override void OnStopGamemode() {
            base.OnStopGamemode();

            // Get the winner message
            var firstPlace = GetByScore(0);
            var secondPlace = GetByScore(1);
            var thirdPlace = GetByScore(2);

            var selfPlace = GetPlace(PlayerIdManager.LocalId);
            var selfScore = GetScore(PlayerIdManager.LocalId);

            string message = "No one scored points!";

            if (firstPlace != null && firstPlace.TryGetDisplayName(out var name)) {
                message = $"First Place: {name} (Score: {GetScore(firstPlace)}) \n";
            }

            if (secondPlace != null && secondPlace.TryGetDisplayName(out name)) {
                message += $"Second Place: {name} (Score: {GetScore(secondPlace)}) \n";
            }

            if (thirdPlace != null && thirdPlace.TryGetDisplayName(out name)) {
                message += $"Third Place: {name} (Score: {GetScore(thirdPlace)}) \n";
            }

            if (selfPlace != -1 && selfPlace > 3) {
                message += $"Your Place: {selfPlace} (Score: {selfScore})";
            }

            // Show the winners in a notification
            FusionNotifier.Send(new FusionNotification()
            {
                title = "Deathmatch Completed",
                showTitleOnPopup = true,

                message = message,

                popupLength = 6f,

                isMenuItem = false,
                isPopup = true,
            });

            _timeOfStart = 0f;
            _oneMinuteLeft = false;

            // Reset mortality
            FusionPlayer.ResetMortality();

            // Remove ammo
            FusionPlayer.SetAmmo(0);
        }

        public float GetTimeElapsed() => Time.realtimeSinceStartup - _timeOfStart;
        public float GetMinutesLeft() {
            float elapsed = GetTimeElapsed();
            return _totalMinutes - (elapsed / 60f);
        }

        protected override void OnUpdate() {
            // Active update
            if (IsActive() && NetworkInfo.IsServer) {
                // Get time left
                float minutesLeft = GetMinutesLeft();

                // Check for minute barrier
                if (!_oneMinuteLeft) {
                    if (minutesLeft <= 1f) {
                        TryInvokeTrigger("OneMinuteLeft");
                        _oneMinuteLeft = true;
                    }
                }
                
                // Should the gamemode end?
                if (minutesLeft <= 0f) {
                    StopGamemode();
                }
            }
        }

        protected override void OnEventTriggered(string value) {
            // Check event
            if (value == "OneMinuteLeft") {
                FusionNotifier.Send(new FusionNotification()
                {
                    title = "Deathmatch Timer",
                    showTitleOnPopup = true,
                    message = "One minute left!",
                    isMenuItem = false,
                    isPopup = true,
                });
            }
        }

        protected override void OnMetadataChanged(string key, string value) {
            // Check if our score increased
            var playerKey = GetScoreKey(PlayerIdManager.LocalId);

            if (playerKey == key && value != "0") {
                FusionNotifier.Send(new FusionNotification()
                {
                    title = "Deathmatch Point",
                    showTitleOnPopup = true,
                    message = $"New score is {value}!",
                    isMenuItem = false,
                    isPopup = true,
                    popupLength = 0.7f,
                });
            }
        }

        protected void ResetScores() {
            foreach (var player in PlayerIdManager.PlayerIds) {
                TrySetMetadata(GetScoreKey(player), "0");
            }
        }

        protected void IncrementScore(PlayerId id) {
            var score = GetScore(id);
            score++;

            TrySetMetadata(GetScoreKey(id), score.ToString());
        }

        protected string GetScoreKey(PlayerId id) {
            if (id == null)
                return "";

            return $"{PlayerScoreKey}.{id.LongId}";
        }

        protected int GetScore(PlayerId id) {
            if (id == null)
                return 0;

            if (TryGetMetadata(GetScoreKey(id), out var value) && int.TryParse(value, out var score)) {
                return score;
            }

            return 0;
        }
    }
}
