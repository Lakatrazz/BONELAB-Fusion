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
using UnityEngine.UI;

namespace LabFusion.SDK.Gamemodes {
    public class TeamDeathmatch : Gamemode {
        private const int _minPlayerBits = 30;
        private const int _maxPlayerBits = 250;

        public static TeamDeathmatch Instance { get; private set; }

        protected enum Team {
            NO_TEAM = 0,
            SABRELAKE = 1,
            LAVA_GANG = 2,
        }

        public string LavaGangName => !string.IsNullOrWhiteSpace(_lavaGangOverride) ? _lavaGangOverride : "Lava Gang";
        public string SabrelakeName => !string.IsNullOrWhiteSpace(_sabrelakeOverride) ? _sabrelakeOverride : "Sabrelake";
        public Texture2D LavaGangLogo => _lavaGangLogoOverride != null ? _lavaGangLogoOverride : FusionContentLoader.LavaGangLogo;
        public Texture2D SabrelakeLogo => _sabrelakeLogoOverride != null ? _sabrelakeLogoOverride : FusionContentLoader.SabrelakeLogo;

        protected string _lavaGangOverride = null;
        protected string _sabrelakeOverride = null;

        protected Texture2D _lavaGangLogoOverride = null;
        protected Texture2D _sabrelakeLogoOverride = null;

        protected string ParseTeam(Team team) {
            switch (team) {
                default:
                case Team.NO_TEAM:
                    return "Invalid Team";
                case Team.LAVA_GANG:
                    return LavaGangName;
                case Team.SABRELAKE:
                    return SabrelakeName;
            }
        }

        protected class TeamLogoInstance {
            protected const float LogoDivider = 270f;

            public TeamDeathmatch deathmatch;

            public GameObject go;
            public Canvas canvas;
            public RawImage image;

            public PlayerId id;
            public PlayerRep rep;

            public Team team;

            public TeamLogoInstance(TeamDeathmatch deathmatch, PlayerId id, Team team) {
                this.deathmatch = deathmatch;

                go = new GameObject($"{id.SmallId} Team Logo");

                canvas = go.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.sortingOrder = 100000;
                go.transform.localScale = Vector3Extensions.one / LogoDivider;

                image = go.AddComponent<RawImage>();

                GameObject.DontDestroyOnLoad(go);
                go.hideFlags = HideFlags.DontUnloadUnusedAsset;

                this.id = id;
                PlayerRepManager.TryGetPlayerRep(id, out rep);

                this.team = team;

                UpdateLogo();
            }

            public void Toggle(bool value) {
                go.SetActive(value);
            }

            public void UpdateLogo() {
                switch (team) {
                    case Team.LAVA_GANG:
                        image.texture = deathmatch.LavaGangLogo;
                        break;
                    case Team.SABRELAKE:
                        image.texture = deathmatch.SabrelakeLogo;
                        break;
                }
            }

            public void Cleanup() {
                if (!go.IsNOC())
                    GameObject.Destroy(go);
            }

            public bool IsShown() => go.activeSelf;

            public void Update() {
                if (rep != null) {
                    var rm = rep.RigReferences.RigManager;

                    if (!rm.IsNOC()) {
                        var head = rm.physicsRig.m_head;

                        go.transform.position = head.position + Vector3Extensions.up * rep.GetNametagOffset();
                        go.transform.LookAtPlayer();

                        UpdateLogo();
                    }
                }
            }
        }

        private const int _defaultMinutes = 3;
        private const int _minMinutes = 2;
        private const int _maxMinutes = 60;

        // Prefix
        public const string DefaultPrefix = "InternalTeamDeathmatchMetadata";

        // Default metadata keys
        public const string TeamScoreKey = DefaultPrefix + ".Score";
        public const string PlayerTeamKey = DefaultPrefix + ".Team";

        public override string GamemodeCategory => "Fusion";
        public override string GamemodeName => "Team Deathmatch";

        public override bool DisableDevTools => true;
        public override bool DisableSpawnGun => true;
        public override bool DisableManualUnragdoll => true;

        private float _timeOfStart;
        private bool _oneMinuteLeft;

        private int _totalMinutes = _defaultMinutes;

        private Team _lastTeam = Team.NO_TEAM;
        private Team _localTeam = Team.NO_TEAM;

        private bool _hasOverridenValues = false;

        private readonly Dictionary<PlayerId, TeamLogoInstance> _logoInstances = new Dictionary<PlayerId, TeamLogoInstance>();

        private string _avatarOverride = null;
        private float? _vitalityOverride = null;

        public override void OnBoneMenuCreated(MenuCategory category) {
            base.OnBoneMenuCreated(category);

            category.CreateIntElement("Round Minutes", Color.white, _totalMinutes, 1, _minMinutes, _maxMinutes, (v) =>
            {
                _totalMinutes = v;
            });
        }

        public void SetRoundLength(int minutes) {
            _totalMinutes = minutes;
        }

        public void SetAvatarOverride(string barcode)
        {
            _avatarOverride = barcode;

            if (IsActive())
                FusionPlayer.SetAvatarOverride(barcode);
        }

        public void SetPlayerVitality(float vitality)
        {
            _vitalityOverride = vitality;

            if (IsActive())
                FusionPlayer.SetPlayerVitality(vitality);
        }

        public void SetLavaGangName(string name) {
            _lavaGangOverride = name;
        }

        public void SetSabrelakeName(string name) {
            _sabrelakeOverride = name;
        }

        public void SetLavaGangLogo(Texture2D logo)
        {
            _lavaGangLogoOverride = logo;
        }

        public void SetSabrelakeLogo(Texture2D logo)
        {
            _sabrelakeLogoOverride = logo;
        }

        public override void OnGamemodeRegistered() {
            base.OnGamemodeRegistered();

            Instance = this;

            MultiplayerHooking.OnPlayerJoin += OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
            MultiplayerHooking.OnPlayerAction += OnPlayerAction;
            FusionOverrides.OnValidateNametag += OnValidateNametag;

            SetDefaultValues();
        }

        public override void OnGamemodeUnregistered() {
            base.OnGamemodeUnregistered();

            if (Instance == this)
                Instance = null;

            MultiplayerHooking.OnPlayerJoin -= OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
            MultiplayerHooking.OnPlayerAction -= OnPlayerAction;
            FusionOverrides.OnValidateNametag -= OnValidateNametag;
        }

        protected bool OnValidateNametag(PlayerId id)
        {
            if (!IsActive())
                return true;

            return GetTeam(id) == _localTeam;
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

        public void SetDefaultValues()
        {
            _totalMinutes = _defaultMinutes;
            SetPlaylist(DefaultMusicVolume, FusionContentLoader.CombatPlaylist);

            _lavaGangOverride = null;
            _sabrelakeOverride = null;

            _lavaGangLogoOverride = null;
            _sabrelakeLogoOverride = null;

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

        public int GetTotalScore() {
            return GetScore(Team.LAVA_GANG) + GetScore(Team.SABRELAKE);
        }

        private int GetRewardedBits()
        {
            // Change the max bit count based on player count
            int playerCount = PlayerIdManager.PlayerCount - 1;

            // 10 and 100 are the min and max values for the max bit count
            float playerPercent = (float)playerCount / 4f;
            int maxBits = Mathf.FloorToInt(Mathf.Lerp(_minPlayerBits, _maxPlayerBits, playerPercent));
            int maxRand = maxBits / 10;

            // Get the scores
            int score = GetScore(_localTeam);
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

        protected void OnPlayerAction(PlayerId player, PlayerActionType type, PlayerId otherPlayer = null)
        {
            if (IsActive() && NetworkInfo.IsServer)
            {
                switch (type)
                {
                    case PlayerActionType.DEATH_BY_OTHER_PLAYER:
                        if (otherPlayer != null && otherPlayer != player) {
                            var killerTeam = GetTeam(otherPlayer);
                            var killedTeam = GetTeam(player);

                            if (killerTeam != killedTeam) {
                                IncrementScore(killerTeam);
                            }
                        }
                        break;
                }
            }
        }

        protected void OnPlayerJoin(PlayerId id) {
            if (NetworkInfo.IsServer && IsActive()) {
                AssignTeam(id);
            }
        }

        protected void OnPlayerLeave(PlayerId id) {
            if (_logoInstances.TryGetValue(id, out var instance)) {
                instance.Cleanup();
                _logoInstances.Remove(id);
            }
        }

        protected override void OnStartGamemode() {
            base.OnStartGamemode();

            if (NetworkInfo.IsServer) {
                ResetTeams();
                SetTeams();
            }

            _timeOfStart = Time.realtimeSinceStartup;
            _oneMinuteLeft = false;

            // Invoke player changes on level load
            FusionSceneManager.HookOnLevelLoad(() =>
            {
                // Force mortality
                FusionPlayer.SetMortality(true);

                // Setup ammo
                FusionPlayer.SetAmmo(1000);

                // Push nametag updates
                FusionOverrides.ForceUpdateOverrides();

                // Apply vitality and avatar overrides
                if (_avatarOverride != null)
                    FusionPlayer.SetAvatarOverride(_avatarOverride);

                if (_vitalityOverride.HasValue)
                    FusionPlayer.SetPlayerVitality(_vitalityOverride.Value);
            });
        }

        protected override void OnStopGamemode() {
            base.OnStopGamemode();

            // Get the winner and loser
            var lavaGang = GetScore(Team.LAVA_GANG);
            var sabrelake = GetScore(Team.SABRELAKE);

            string message;

            if (lavaGang > sabrelake) {
                message = $"WINNER: {ParseTeam(Team.LAVA_GANG)}! (Score: {lavaGang})\n" +
    $"Loser: {ParseTeam(Team.SABRELAKE)} (Score: {sabrelake})\n";

                message += GetTeamStatus(Team.LAVA_GANG); ;
            }
            else if (sabrelake > lavaGang) {
                message = $"WINNER: {ParseTeam(Team.SABRELAKE)}! (Score: {sabrelake})\n" +
                    $"Loser: {ParseTeam(Team.LAVA_GANG)} (Score: {lavaGang})\n";

                message += GetTeamStatus(Team.SABRELAKE); ;
            }
            else {
                message = $"Tie! (Both Scores: ({lavaGang}))";

                OnTeamTied();
            }

            // Show the winners in a notification
            FusionNotifier.Send(new FusionNotification()
            {
                title = "Team Deathmatch Completed",
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

            // Remove all team logos
            RemoveLogos();

            // Push nametag updates
            FusionOverrides.ForceUpdateOverrides();

            // Reset overrides
            FusionPlayer.ClearAvatarOverride();
            FusionPlayer.ClearPlayerVitality();
        }

        public float GetTimeElapsed() => Time.realtimeSinceStartup - _timeOfStart;
        public float GetMinutesLeft()
        {
            float elapsed = GetTimeElapsed();
            return _totalMinutes - (elapsed / 60f);
        }

        protected override void OnUpdate() {
            base.OnUpdate();

            if (IsActive()) {
                UpdateLogos();

                // Update minute check/game end
                if (NetworkInfo.IsServer) {
                    // Get time left
                    float minutesLeft = GetMinutesLeft();

                    // Check for minute barrier
                    if (!_oneMinuteLeft)
                    {
                        if (minutesLeft <= 1f)
                        {
                            TryInvokeTrigger("OneMinuteLeft");
                            _oneMinuteLeft = true;
                        }
                    }

                    // Should the gamemode end?
                    if (minutesLeft <= 0f)
                    {
                        StopGamemode();
                        TryInvokeTrigger("NaturalEnd");
                    }
                }
            }
        }

        protected void UpdateLogos() {
            // Update logos
            foreach (var logo in _logoInstances.Values)
            {
                // Change visibility
                bool visible = logo.team == _localTeam;
                if (visible != logo.IsShown())
                {
                    logo.Toggle(visible);
                }

                // Update position
                logo.Update();
            }
        }

        protected void AddLogo(PlayerId id, Team team) {
            var logo = new TeamLogoInstance(this, id, team);
            _logoInstances.Add(id, logo);
        }

        protected void RemoveLogos() {
            foreach (var logo in _logoInstances.Values) {
                logo.Cleanup();
            }

            _logoInstances.Clear();
        }

        protected override void OnEventTriggered(string value)
        {
            // Check event
            switch (value) {
                case "OneMinuteLeft":
                    FusionNotifier.Send(new FusionNotification()
                    {
                        title = "Team Deathmatch Timer",
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

        protected string GetTeamStatus(Team winner) {
            if (_localTeam == winner) {
                OnTeamVictory(_localTeam);
                return "You Won!";
            }
            else {
                OnTeamLost(_localTeam);
                return "You Lost...";
            }
        }

        protected void OnTeamVictory(Team team) {
            switch (team) {
                case Team.LAVA_GANG:
                    FusionAudio.Play2D(FusionContentLoader.LavaGangVictory, DefaultMusicVolume);
                    break;
                case Team.SABRELAKE:
                    FusionAudio.Play2D(FusionContentLoader.SabrelakeVictory, DefaultMusicVolume);
                    break;
            }
        }

        protected void OnTeamLost(Team team) {
            switch (team)
            {
                case Team.LAVA_GANG:
                    FusionAudio.Play2D(FusionContentLoader.LavaGangFailure, DefaultMusicVolume);
                    break;
                case Team.SABRELAKE:
                    FusionAudio.Play2D(FusionContentLoader.SabrelakeFailure, DefaultMusicVolume);
                    break;
            }
        }

        protected void OnTeamTied() {
            FusionAudio.Play2D(FusionContentLoader.DMTie, DefaultMusicVolume);
        }

        protected void OnTeamReceived(Team team) {
            if (team == Team.NO_TEAM) {
                _localTeam = team;
                return;
            }

            FusionNotifier.Send(new FusionNotification()
            {
                title = "Team Deathmatch Assignment",
                showTitleOnPopup = true,
                message = $"Your team is: {ParseTeam(team)}",
                isMenuItem = false,
                isPopup = true,
                popupLength = 5f,
            });

            _localTeam = team;

            // Invoke spawn point changes on level load
            FusionSceneManager.HookOnLevelLoad(() =>
            {
                // Get all spawn points
                List<Transform> transforms = new List<Transform>();

                if (team == Team.SABRELAKE)
                {
                    foreach (var point in SabrelakeSpawnpoint.Cache.Components)
                    {
                        transforms.Add(point.transform);
                    }
                }
                else
                {
                    foreach (var point in LavaGangSpawnpoint.Cache.Components)
                    {
                        transforms.Add(point.transform);
                    }
                }

                FusionPlayer.SetSpawnPoints(transforms.ToArray());

                // Teleport to a random spawn point
                if (FusionPlayer.TryGetSpawnPoint(out var spawn))
                {
                    FusionPlayer.Teleport(spawn.position, spawn.forward);
                }
            });
        }

        protected override void OnMetadataChanged(string key, string value) {
            base.OnMetadataChanged(key, value);

            // Check if this is a point
            if (key.StartsWith(TeamScoreKey) && int.TryParse(value, out var score)) {
                var ourKey = GetScoreKey(_localTeam);

                if (ourKey == key && score != 0) {
                    FusionNotifier.Send(new FusionNotification()
                    {
                        title = "Team Deathmatch Point",
                        showTitleOnPopup = true,
                        message = $"{ParseTeam(_localTeam)}'s score is {value}!",
                        isMenuItem = false,
                        isPopup = true,
                        popupLength = 0.7f,
                    });
                }
            }
            // Check if this is a team change
            else if (key.StartsWith(PlayerTeamKey) && Enum.TryParse<Team>(value, out var team)) {
                // Find the player that changed
                foreach (var playerId in PlayerIdManager.PlayerIds) {
                    var playerKey = GetTeamKey(playerId);

                    if (playerKey == key) {
                        // Check who this is
                        if (playerId.IsSelf) {
                            OnTeamReceived(team);
                        }
                        else if (team != Team.NO_TEAM) {
                            AddLogo(playerId, team);
                        }

                        // Push nametag updates
                        FusionOverrides.ForceUpdateOverrides();

                        break;
                    }
                }
            }
        }

        protected void SetTeams() {
            // Shuffle the player teams
            var players = new List<PlayerId>(PlayerIdManager.PlayerIds);
            players.Shuffle();

            // Assign every team
            foreach (var player in players) {
                AssignTeam(player);
            }
        }

        protected void ResetTeams() {
            // Reset the last team
            _lastTeam = Team.NO_TEAM;

            // Set every team to none
            foreach (var player in PlayerIdManager.PlayerIds) {
                SetTeam(player, Team.NO_TEAM);
            }

            // Set every score to 0
            SetScore(Team.LAVA_GANG, 0);
            SetScore(Team.SABRELAKE, 0);
        }

        protected void AssignTeam(PlayerId id) {
            // Get the opposite team of the last
            Team newTeam = _lastTeam;
            if (newTeam == Team.SABRELAKE)
                newTeam = Team.LAVA_GANG;
            else
                newTeam = Team.SABRELAKE;

            // Assign it
            SetTeam(id, newTeam);

            // Save the team
            _lastTeam = newTeam;
        }

        protected void IncrementScore(Team team) {
            var currentScore = GetScore(team);
            SetScore(team, currentScore + 1);
        }

        protected void SetScore(Team team, int score) {
            TrySetMetadata(GetScoreKey(team), score.ToString());
        }

        protected void SetTeam(PlayerId id, Team team) {
            TrySetMetadata(GetTeamKey(id), team.ToString());
        }

        protected int GetScore(Team team) {
            if (TryGetMetadata(GetScoreKey(team), out var value) && int.TryParse(value, out var score)) {
                return score;
            }

            return 0;
        }

        protected Team GetTeam(PlayerId id) {
            if (TryGetMetadata(GetTeamKey(id), out var value) && Enum.TryParse<Team>(value, out var team)) {
                return team;
            }

            return Team.NO_TEAM;
        }

        protected string GetScoreKey(Team team) {
            return $"{TeamScoreKey}.{team}";
        }

        protected string GetTeamKey(PlayerId id) {
            return $"{PlayerTeamKey}.{id.LongId}";
        }
    }
}
