using BoneLib;
using BoneLib.BoneMenu.Elements;
using LabFusion.Extensions;
using LabFusion.MarrowIntegration;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.SDK.Points;
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
        private const int _minPlayerBits = 30;
        private const int _maxPlayerBits = 250;

        public static Deathmatch Instance { get; private set; }

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

        private bool _hasOverridenValues = false;

        private string _avatarOverride = null;
        private float? _vitalityOverride = null;

        public override void OnBoneMenuCreated(MenuCategory category) {
            base.OnBoneMenuCreated(category);

            category.CreateIntElement("Round Minutes", Color.white, _totalMinutes, 1, _minMinutes, _maxMinutes, (v) => {
                _totalMinutes = v;
            });
        }

        public override void OnMainSceneInitialized() {
            if (!_hasOverridenValues) {
                SetDefaultValues();
            }
            else {
                _hasOverridenValues = false;
            }
        }

        public override void OnLoadingBegin() {
            _hasOverridenValues = false;
        }

        public void SetDefaultValues() {
            _totalMinutes = _defaultMinutes;
            SetPlaylist(DefaultMusicVolume, FusionContentLoader.CombatPlaylist);

            _avatarOverride = null;
            _vitalityOverride = null;
        }

        public void SetOverriden() {
            if (FusionSceneManager.IsLoading()) {
                if (!_hasOverridenValues)
                    SetDefaultValues();

                _hasOverridenValues = true;
            }
        }

        public void SetRoundLength(int minutes) {
            _totalMinutes = minutes;
        }

        public void SetAvatarOverride(string barcode) {
            _avatarOverride = barcode;

            if (IsActive())
                FusionPlayer.SetAvatarOverride(barcode);
        }

        public void SetPlayerVitality(float vitality) {
            _vitalityOverride = vitality;

            if (IsActive())
                FusionPlayer.SetPlayerVitality(vitality);
        }

        public IReadOnlyList<PlayerId> GetPlayersByScore() {
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

            if (players == null)
                return -1;

            for (var i = 0; i < players.Count; i++) {
                if (players[i] == id) {
                    return i + 1;
                }
            }

            return -1;
        }

        public int GetTotalScore() {
            int score = 0;

            foreach (var player in PlayerIdManager.PlayerIds) {
                score += GetScore(player);
            }

            return score;
        }

        private int GetRewardedBits() {
            // Change the max bit count based on player count
            int playerCount = PlayerIdManager.PlayerCount - 1;

            // 10 and 100 are the min and max values for the max bit count
            float playerPercent = (float)playerCount / 3f;
            int maxBits = Mathf.FloorToInt(Mathf.Lerp(_minPlayerBits, _maxPlayerBits, playerPercent));
            int maxRand = maxBits / 10;

            // Get the scores
            int score = GetScore(PlayerIdManager.LocalId);
            int totalScore = GetTotalScore();

            // Prevent divide by 0
            if (totalScore <= 0)
                return 0;

            float percent = Mathf.Clamp01((float)score / (float)totalScore);
            int reward = Mathf.FloorToInt((float)maxBits * percent);

            // Add randomness
            reward += UnityEngine.Random.Range(-maxRand, maxRand);

            // Make sure the reward isn't invalid
            if (reward.IsNaN()) {
                FusionLogger.ErrorLine("Prevented attempt to give invalid bit reward. Please notify a Fusion developer and send them your log.");
                return 0;
            }

            return reward;
        }

        public override void OnGamemodeRegistered() {
            Instance = this;

            // Add hooks
            MultiplayerHooking.OnPlayerAction += OnPlayerAction;
            FusionOverrides.OnValidateNametag += OnValidateNametag;

            SetDefaultValues();
        }

        public override void OnGamemodeUnregistered() {
            if (Instance == this)
                Instance = null;

            // Remove hooks
            MultiplayerHooking.OnPlayerAction -= OnPlayerAction;
            FusionOverrides.OnValidateNametag -= OnValidateNametag;
        }

        protected bool OnValidateNametag(PlayerId id) {
            if (!IsActive())
                return true;

            return false;
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

            // Invoke player changes on level load
            FusionSceneManager.HookOnLevelLoad(() => {
                // Force mortality
                FusionPlayer.SetMortality(true);

                // Setup ammo
                FusionPlayer.SetAmmo(1000);

                // Get all spawn points
                List<Transform> transforms = new List<Transform>();
                foreach (var point in DeathmatchSpawnpoint.Cache.Components) {
                    transforms.Add(point.transform);
                }

                FusionPlayer.SetSpawnPoints(transforms.ToArray());

                // Teleport to a random spawn point
                if (FusionPlayer.TryGetSpawnPoint(out var spawn)) {
                    FusionPlayer.Teleport(spawn.position, spawn.forward);
                }

                // Push nametag updates
                FusionOverrides.ForceUpdateOverrides();

                // Apply vitality and avatar overrides
                if (_avatarOverride != null)
                    FusionPlayer.SetAvatarOverride(_avatarOverride);

                if (_vitalityOverride.HasValue)
                    FusionPlayer.SetPlayerVitality(_vitalityOverride.Value);
            });
        }

        protected void OnVictoryStatus(bool isVictory = false) {
            if (isVictory) {
                FusionAudio.Play2D(FusionContentLoader.LavaGangVictory, DefaultMusicVolume);
            }
            else {
                FusionAudio.Play2D(FusionContentLoader.LavaGangFailure, DefaultMusicVolume);
            }
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

            // Play victory/failure sounds
            int playerCount = PlayerIdManager.PlayerCount;

            if (playerCount > 1) {
                bool isVictory = false;

                if (selfPlace < Mathf.Min(playerCount, 3))
                    isVictory = true;

                OnVictoryStatus(isVictory);
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

            // Remove spawn points
            FusionPlayer.ResetSpawnPoints();

            // Push nametag updates
            FusionOverrides.ForceUpdateOverrides();

            // Reset overrides
            FusionPlayer.ClearAvatarOverride();
            FusionPlayer.ClearPlayerVitality();
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
                    TryInvokeTrigger("NaturalEnd");
                }
            }
        }

        protected override void OnEventTriggered(string value) {
            // Check event
            switch (value) {
                case "OneMinuteLeft":
                    FusionNotifier.Send(new FusionNotification()
                    {
                        title = "Deathmatch Timer",
                        showTitleOnPopup = true,
                        message = "One minute left!",
                        isMenuItem = false,
                        isPopup = true,
                    });
                    break;
                case "NaturalEnd":
                    int bitReward = GetRewardedBits();

                    if (bitReward > 0) {
                        FusionNotifier.Send(new FusionNotification() {
                            title = "Bits Rewarded",
                            showTitleOnPopup = true,

                            message = $"You Won {bitReward} Bits",

                            popupLength = 3f,

                            isMenuItem = false,
                            isPopup = true,
                        });

                        PointItemManager.RewardBits(bitReward);
                    }
                    break;
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
