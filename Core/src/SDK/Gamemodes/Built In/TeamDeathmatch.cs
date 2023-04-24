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
using UnityEngine;
using UnityEngine.Rendering.Universal.LibTessDotNet;

namespace LabFusion.SDK.Gamemodes
{
    public class TeamDeathmatch : Gamemode
    {
        private const int _minPlayerBits = 30;
        private const int _maxPlayerBits = 250;

        public static TeamDeathmatch Instance { get; private set; }

        public List<Team> teams;

        public string LavaGangName => !string.IsNullOrWhiteSpace(_lavaGangOverride) ? _lavaGangOverride : "Lava Gang";
        public string SabrelakeName => !string.IsNullOrWhiteSpace(_sabrelakeOverride) ? _sabrelakeOverride : "Sabrelake";
        public Texture2D LavaGangLogo => _lavaGangLogoOverride != null ? _lavaGangLogoOverride : FusionContentLoader.LavaGangLogo;
        public Texture2D SabrelakeLogo => _sabrelakeLogoOverride != null ? _sabrelakeLogoOverride : FusionContentLoader.SabrelakeLogo;

        protected string _lavaGangOverride = null;
        protected string _sabrelakeOverride = null;

        protected Texture2D _lavaGangLogoOverride = null;
        protected Texture2D _sabrelakeLogoOverride = null;

        protected string ParseTeam(Team team)
        {
            return team.TeamName;
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

        public override bool PreventNewJoins => !_enabledLateJoining;

        private float _timeOfStart;
        private bool _oneMinuteLeft;

        private int _savedMinutes = _defaultMinutes;
        private int _totalMinutes = _defaultMinutes;

        private Team _lastTeam = null;
        private Team _localTeam = null;

        private bool _hasOverridenValues = false;

        private readonly Dictionary<PlayerId, TeamLogoInstance> _logoInstances = new Dictionary<PlayerId, TeamLogoInstance>();

        private string _avatarOverride = null;
        private float? _vitalityOverride = null;

        private bool _enabledLateJoining = true;

        public override void OnBoneMenuCreated(MenuCategory category)
        {
            base.OnBoneMenuCreated(category);

            category.CreateIntElement("Round Minutes", Color.white, _totalMinutes, 1, _minMinutes, _maxMinutes, (v) =>
            {
                _totalMinutes = v;
                _savedMinutes = v;
            });
        }

        public void SetLateJoining(bool enabled)
        {
            _enabledLateJoining = enabled;
        }

        public void SetRoundLength(int minutes)
        {
            _totalMinutes = minutes;
        }

        public void SetAvatarOverride(string barcode)
        {
            _avatarOverride = barcode;

            if (IsActive())
            {
                FusionPlayer.SetAvatarOverride(barcode);
            }
        }

        public void SetPlayerVitality(float vitality)
        {
            _vitalityOverride = vitality;

            if (IsActive())
            {
                FusionPlayer.SetPlayerVitality(vitality);
            }
        }

        public void SetLavaGangName(string name)
        {
            _lavaGangOverride = name;
        }

        public void SetSabrelakeName(string name)
        {
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

        public override void OnGamemodeRegistered()
        {
            base.OnGamemodeRegistered();

            Instance = this;

            MultiplayerHooking.OnPlayerJoin += OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
            MultiplayerHooking.OnPlayerAction += OnPlayerAction;
            FusionOverrides.OnValidateNametag += OnValidateNametag;

            teams = new List<Team>();

            SetDefaultValues();
        }

        public override void OnGamemodeUnregistered()
        {
            base.OnGamemodeUnregistered();

            if (Instance == this)
            {
                Instance = null;
            }

            MultiplayerHooking.OnPlayerJoin -= OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
            MultiplayerHooking.OnPlayerAction -= OnPlayerAction;
            FusionOverrides.OnValidateNametag -= OnValidateNametag;
        }

        protected bool OnValidateNametag(PlayerId id)
        {
            if (!IsActive())
            {
                return true;
            }

            return GetTeam(id) == _localTeam;
        }

        public override void OnMainSceneInitialized()
        {
            if (!_hasOverridenValues)
            {
                SetDefaultValues();
            }
            else
            {
                _hasOverridenValues = false;
            }
        }

        public override void OnLoadingBegin()
        {
            _hasOverridenValues = false;
        }

        public void SetDefaultValues()
        {
            _totalMinutes = _savedMinutes;
            SetPlaylist(DefaultMusicVolume, FusionContentLoader.CombatPlaylist);

            _lavaGangOverride = null;
            _sabrelakeOverride = null;

            _lavaGangLogoOverride = null;
            _sabrelakeLogoOverride = null;

            _avatarOverride = null;
            _vitalityOverride = null;

            _enabledLateJoining = true;
        }

        public void SetOverriden()
        {
            if (FusionSceneManager.IsLoading())
            {
                if (!_hasOverridenValues)
                {
                    SetDefaultValues();
                }

                _hasOverridenValues = true;
            }
        }

        public int GetTotalScore()
        {
            int accumulatedScore = 0;

            for(int i = 0; i < teams.Count; i++)
            {
                accumulatedScore = accumulatedScore + teams[i].TeamScore;
            }

            return accumulatedScore;
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
            if (reward.IsNaN())
            {
                FusionLogger.ErrorLine("Prevented attempt to give invalid bit reward. Please notify a Fusion developer and send them your log.");
                return 0;
            }

            return reward;
        }

        protected void OnPlayerAction(PlayerId player, PlayerActionType type, PlayerId otherPlayer = null)
        {
            if (IsActive() && NetworkInfo.IsServer)
            {
                if (type != PlayerActionType.DEATH_BY_OTHER_PLAYER)
                {
                    return;
                }

                if(otherPlayer == null)
                {
                    return;
                }

                if(otherPlayer == player)
                {
                    return;
                }

                var killerTeam = GetTeam(otherPlayer);
                var killedTeam = GetTeam(player);

                if (killerTeam != killedTeam)
                {
                    IncrementScore(killerTeam);
                }
            }
        }

        protected void OnPlayerJoin(PlayerId id)
        {
            if (NetworkInfo.IsServer && IsActive())
            {
                AssignTeam(id);
            }
        }

        protected void OnPlayerLeave(PlayerId id)
        {
            if (_logoInstances.TryGetValue(id, out var instance))
            {
                instance.Cleanup();
                _logoInstances.Remove(id);
            }
        }

        protected override void OnStartGamemode()
        {
            base.OnStartGamemode();

            if (NetworkInfo.IsServer)
            {
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
                {
                    FusionPlayer.SetAvatarOverride(_avatarOverride);
                }

                if (_vitalityOverride.HasValue)
                {
                    FusionPlayer.SetPlayerVitality(_vitalityOverride.Value);
                }
            });
        }

        protected override void OnStopGamemode()
        {
            base.OnStopGamemode();

            List<Team> leaders = teams;
            leaders.OrderBy(team => GetScore(team)).Reverse();

            Team winningTeam = leaders.First();
            Team secondPlaceTeam = leaders[1];
            Team thirdPlaceTeam = leaders[2];

            bool tied = leaders.All((team) => team.TeamScore == GetScore(winningTeam));
            string message = "";

            if(winningTeam != null)
            {
                message = $"First Place: {winningTeam.TeamName} (Score: {GetScore(winningTeam)}) \n";
            }

            if(secondPlaceTeam != null)
            {
                message += $"Second Place: {secondPlaceTeam.TeamName} (Score: {GetScore(secondPlaceTeam)}) \n";
            }

            if(thirdPlaceTeam != null)
            {
                message += $"Third Place: {thirdPlaceTeam.TeamName} (Score: {GetScore(thirdPlaceTeam)}) \n";
            }

            if (tied)
            {
                message += "Tie! (All teams scored the same score!)";
                OnTeamTied();
            }
            else
            {
                message += GetTeamStatus(winningTeam);
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

        public float GetTimeElapsed()
        {
            return Time.realtimeSinceStartup - _timeOfStart;
        }

        public float GetMinutesLeft()
        {
            float elapsed = GetTimeElapsed();
            return _totalMinutes - (elapsed / 60f);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (!IsActive() || !NetworkInfo.IsServer)
            {
                return;
            }

            UpdateLogos();

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

        protected void UpdateLogos()
        {
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

        protected void AddLogo(PlayerId id, Team team)
        {
            var logo = new TeamLogoInstance(this, id, team);
            _logoInstances.Add(id, logo);
        }

        protected void RemoveLogos()
        {
            foreach (var logo in _logoInstances.Values)
            {
                logo.Cleanup();
            }

            _logoInstances.Clear();
        }

        protected override void OnEventTriggered(string value)
        {
            FusionNotification oneMinuteNotification = new FusionNotification()
            {
                title = "Team Deathmatch Timer",
                showTitleOnPopup = true,
                message = "One minute left!",
                isMenuItem = false,
                isPopup = true,
            };

            FusionNotification bitRewardNotification = new FusionNotification()
            {
                title = "Bits Rewarded",
                showTitleOnPopup = true,
                popupLength = 3f,
                isMenuItem = false,
                isPopup = true,
            };

            if (value == "OneMinuteLeft")
            {
                FusionNotifier.Send(oneMinuteNotification);
            }

            if(value == "NaturalEnd")
            {
                int bitReward = GetRewardedBits();
                string message = bitReward == 1 ? "Bit" : "Bits";

                bitRewardNotification.message = $"You Won {bitReward}" + message;
                PointItemManager.RewardBits(bitReward);
            }
        }

        protected string GetTeamStatus(Team winner)
        {
            if (_localTeam == winner)
            {
                OnTeamVictory(_localTeam);
                return "You Won!";
            }
            else
            {
                OnTeamLost(_localTeam);
                return "You Lost...";
            }
        }

        protected void OnTeamVictory(Team team)
        {
            FusionAudio.Play2D(team.WinMusic, DefaultMusicVolume);
        }

        protected void OnTeamLost(Team team)
        {
            FusionAudio.Play2D(team.LossMusic, DefaultMusicVolume);
        }

        protected void OnTeamTied()
        {
            FusionAudio.Play2D(FusionContentLoader.DMTie, DefaultMusicVolume);
        }

        protected void OnTeamReceived(Team team)
        {
            if (team == null)
            {
                _localTeam = null;
                return;
            }

            FusionNotification assignmentNotification = new FusionNotification()
            {
                title = "Team Deathmatch Assignment",
                showTitleOnPopup = true,
                message = $"Your team is: {ParseTeam(team)}",
                isMenuItem = false,
                isPopup = true,
                popupLength = 5f,
            };

            FusionNotifier.Send(assignmentNotification);

            _localTeam = team;

            // Invoke ult events
            if (team.TeamName == "Sabrelake")
            {
                foreach (var ultEvent in InvokeUltEventIfTeamSabrelake.Cache.Components)
                {
                    ultEvent.Invoke();
                }
            }
            else if(team.TeamName == "Lava Gang")
            {
                foreach (var ultEvent in InvokeUltEventIfTeamLavaGang.Cache.Components)
                {
                    ultEvent.Invoke();
                }
            }
            else
            {

            }

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

        protected override void OnMetadataChanged(string key, string value)
        {
            base.OnMetadataChanged(key, value);

            // Check if this is a point
            if (key.StartsWith(TeamScoreKey) && int.TryParse(value, out var score))
            {
                var ourKey = GetScoreKey(_localTeam);

                if (ourKey == key && score != 0)
                {
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
            else if (key.StartsWith(PlayerTeamKey) && Enum.TryParse<Team>(value, out var team))
            {
                // Find the player that changed
                foreach (var playerId in PlayerIdManager.PlayerIds)
                {
                    var playerKey = GetTeamKey(playerId);

                    if (playerKey == key)
                    {
                        // Check who this is
                        if (playerId.IsSelf)
                        {
                            OnTeamReceived(team);
                        }
                        else if (team != Team.NO_TEAM)
                        {
                            AddLogo(playerId, team);
                        }

                        // Push nametag updates
                        FusionOverrides.ForceUpdateOverrides();

                        break;
                    }
                }
            }
        }

        protected void SetTeams()
        {
            // Shuffle the player teams
            var players = new List<PlayerId>(PlayerIdManager.PlayerIds);
            players.Shuffle();

            // Assign every team
            foreach (var player in players)
            {
                AssignTeam(player);
            }
        }

        protected void ResetTeams()
        {
            // Reset the last team
            _lastTeam = Team.NO_TEAM;

            // Set every team to none
            foreach (var player in PlayerIdManager.PlayerIds)
            {
                SetTeam(player, Team.NO_TEAM);
            }

            // Set every score to 0
            SetScore(Team.LAVA_GANG, 0);
            SetScore(Team.SABRELAKE, 0);
        }

        protected void AssignTeam(PlayerId id)
        {
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

        protected void IncrementScore(Team team)
        {
            var currentScore = GetScore(team);
            SetScore(team, currentScore + 1);
        }

        public void SetScore(Team team, int score)
        {
            TrySetMetadata(GetScoreKey(team), score.ToString());
        }

        protected void SetTeam(PlayerId id, Team team)
        {
            TrySetMetadata(GetTeamKey(id), team.TeamName);
        }

        public int GetScore(Team team)
        {
            TryGetMetadata(GetScoreKey(team), out string teamKey);
            int score = int.Parse(teamKey);

            return score;
        }

        protected Team GetTeam(PlayerId id)
        {
            Team team = null;
            TryGetMetadata(GetTeamKey(id), out string teamNameKey);

            if (Enum.TryParse<Team>(value, out var team))
            {
                return team;
            }

            return team;
        }

        protected string GetScoreKey(Team team)
        {
            return $"{TeamScoreKey}.{team.TeamName}";
        }
        
        protected string GetTeamKey(PlayerId id)
        {
            return $"{PlayerTeamKey}.{id.LongId}";
        }
    }
}