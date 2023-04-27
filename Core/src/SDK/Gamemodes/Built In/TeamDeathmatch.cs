﻿using BoneLib.BoneMenu.Elements;

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
using UnityEngine.SocialPlatforms.Impl;

namespace LabFusion.SDK.Gamemodes
{
    public class TeamDeathmatch : Gamemode
    {
        private const int _minPlayerBits = 30;
        private const int _maxPlayerBits = 250;

        public static TeamDeathmatch Instance { get; private set; }

        public List<Team> teams;

        public bool OverrideValues { get => _overrideValues; }

        protected string _lavaGangOverride = null;
        protected string _sabrelakeOverride = null;

        protected Texture2D _lavaGangLogoOverride = null;
        protected Texture2D _sabrelakeLogoOverride = null;

        private const int _defaultMinutes = 3;
        private const int _minMinutes = 2;
        private const int _maxMinutes = 60;

        // Prefix
        public const string DefaultPrefix = "FusionTDM";

        // Default metadata keys
        public const string TeamScoreKey = TeamKey + ".Score";
        public const string TeamKey = DefaultPrefix + ".Team";

        public override string GamemodeCategory => "Fusion";
        public override string GamemodeName => "Team Deathmatch";

        public override bool DisableDevTools => true;
        public override bool DisableSpawnGun => true;
        public override bool DisableManualUnragdoll => true;

        public override bool PreventNewJoins => !_enabledLateJoining;

        private float _timeOfStart;
        private bool _oneMinuteLeft;

        private bool _overrideValues;

        private int _savedMinutes = _defaultMinutes;
        private int _totalMinutes = _defaultMinutes;

        private Team _lastTeam = null;
        private Team _localTeam = null;

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

        public void AddTeam(Team team)
        {
            teams.Add(team);
        }

        public void AddDefaultTeams()
        {
            Team sabrelake = new Team("Sabrelake", Color.yellow);
            Team lavaGang = new Team("Lava Gang", Color.magenta);

            sabrelake.SetMusic(FusionContentLoader.SabrelakeVictory, FusionContentLoader.SabrelakeFailure);
            lavaGang.SetMusic(FusionContentLoader.LavaGangVictory, FusionContentLoader.LavaGangFailure);

            sabrelake.SetLogo(FusionContentLoader.SabrelakeLogo);
            lavaGang.SetLogo(FusionContentLoader.LavaGangLogo);

            if(!teams.Exists((team) => team.TeamName == sabrelake.TeamName))
            {
                AddTeam(sabrelake);
            }
            else if(!teams.Exists((team) => team.TeamName == lavaGang.TeamName))
            {
                AddTeam(lavaGang);
            }
        }

        public Team GetTeam(string teamName)
        {
            foreach(Team team in teams)
            {
                if(team.TeamName == teamName)
                {
                    return team;
                }
            }

            return null;
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

            return GetTeamFromMember(id) == _localTeam;
        }

        public override void OnMainSceneInitialized()
        {
            if (!_overrideValues)
            {
                SetDefaultValues();
            }
            else
            {
                _overrideValues = false;
            }
        }

        public override void OnLoadingBegin()
        {
            _overrideValues = false;
        }

        public void SetDefaultValues()
        {
            _totalMinutes = _savedMinutes;
            SetPlaylist(DefaultMusicVolume, FusionContentLoader.CombatPlaylist);

            AddDefaultTeams();

            _avatarOverride = null;
            _vitalityOverride = null;

            _enabledLateJoining = true;
        }

        public void SetOverriden()
        {
            if (FusionSceneManager.IsLoading())
            {
                if (!_overrideValues)
                {
                    SetDefaultValues();
                }

                _overrideValues = true;
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
            int score = GetScoreFromTeam(_localTeam);
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

        /// <summary>
        /// Method for handling in-game player events, like when players kill other players.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="type"></param>
        /// <param name="otherPlayer"></param>
        protected void OnPlayerAction(PlayerId player, PlayerActionType type, PlayerId otherPlayer = null)
        {
            if (IsActive() && NetworkInfo.IsServer)
            {
                if (type != PlayerActionType.DYING_BY_OTHER_PLAYER)
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

                var killerTeam = GetTeamFromMember(otherPlayer);
                var killedTeam = GetTeamFromMember(player);

                if (killerTeam != killedTeam)
                {
                    IncrementScore(killerTeam);
                }
            }
        }

        /// <summary>
        /// Automatically has the host assign a team to a newly joined player.
        /// </summary>
        /// <param name="id"></param>
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

        /// <summary>
        /// Called when the host begins the gamemode. Affects the local player, but also the host so it can send messages to other players.
        /// </summary>
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

        /// <summary>
        /// Called when the host ends the active gamemode. Called events affect the local player.
        /// </summary>
        protected override void OnStopGamemode()
        {
            base.OnStopGamemode();

            List<Team> leaders = teams;
            leaders.OrderBy(team => GetScoreFromTeam(team)).Reverse();

            Team winningTeam = leaders.First();
            Team secondPlaceTeam = leaders[1];

            string message = "";

            message = $"First Place: {winningTeam.TeamName} (Score: {GetScoreFromTeam(winningTeam)}) \n";
            message += $"Second Place: {secondPlaceTeam.TeamName} (Score: {GetScoreFromTeam(secondPlaceTeam)}) \n";

            if (leaders.Count > 2)
            {
                Team thirdPlaceTeam = leaders[2];
                message += $"Third Place: {thirdPlaceTeam.TeamName} (Score: {GetScoreFromTeam(thirdPlaceTeam)}) \n";
            }

            bool tied = leaders.All((team) => team.TeamScore == GetScoreFromTeam(winningTeam));

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
            var logo = new TeamLogoInstance(id, team);
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

        /// <summary>
        /// Called when named events get sent, which gets broadcasted to every player in the lobby.
        /// </summary>
        /// <param name="value"></param>
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

        /// <summary>
        /// Returns a message showing if the local player won or lost.
        /// </summary>
        /// <param name="winner"></param>
        /// <returns></returns>
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
            AudioClip randomChoice = UnityEngine.Random.Range(0, 4) % 2 == 0 ? FusionContentLoader.LavaGangVictory : FusionContentLoader.SabrelakeVictory;

            AudioClip winMusic = team.WinMusic != null ? team.WinMusic : randomChoice;
            FusionAudio.Play2D(winMusic, DefaultMusicVolume);
        }

        protected void OnTeamLost(Team team)
        {
            AudioClip randomChoice = UnityEngine.Random.Range(0, 4) % 2 == 0 ? FusionContentLoader.LavaGangFailure : FusionContentLoader.SabrelakeFailure;

            AudioClip lossMusic = team.LossMusic != null ? team.LossMusic : randomChoice;
            FusionAudio.Play2D(lossMusic, DefaultMusicVolume);
        }

        protected void OnTeamTied()
        {
            FusionAudio.Play2D(FusionContentLoader.DMTie, DefaultMusicVolume);
        }

        /// <summary>
        /// Called when we're assigned a team locally.
        /// </summary>
        /// <param name="team"></param>
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
                message = $"Your team is: {team.TeamName}",
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
                // Likely a custom event for a team
                foreach(var holder in InvokeUltEventIfTeam.Cache.Components)
                {
                    if(team.TeamName != holder.TeamName)
                    {
                        continue;
                    }

                    holder.Invoke();
                }
            }

            // Invoke spawn point changes on level load
            FusionSceneManager.HookOnLevelLoad(() => InitializeTeamSpawns(team));
        }

        protected void InitializeTeamSpawns(Team team)
        {
            // Get all spawn points
            List<Transform> transforms = new List<Transform>();

            if (team.TeamName == "Sabrelake")
            {
                foreach (var point in SabrelakeSpawnpoint.Cache.Components)
                {
                    transforms.Add(point.transform);
                }
            }
            else if(team.TeamName == "Lava Gang")
            {
                foreach (var point in LavaGangSpawnpoint.Cache.Components)
                {
                    transforms.Add(point.transform);
                }
            }
            else
            {
                // Likely a custom event for a team
                foreach(var point in TeamSpawnpoint.Cache.Components)
                {
                    if(team.TeamName != point.TeamName)
                    {
                        continue;
                    }

                    transforms.Add(point.transform);
                }
            }

            FusionPlayer.SetSpawnPoints(transforms.ToArray());

            // Teleport to a random spawn point
            if (FusionPlayer.TryGetSpawnPoint(out var spawn))
            {
                FusionPlayer.Teleport(spawn.position, spawn.forward);
            }
        }

        /// <summary>
        /// Method for handling changes in gamemode metadata.
        /// Metadata is broken up into key value pairs to group requests and events when other players request them.
        /// Things like team changes, point changes, wins, and losses, get handled here.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected override void OnMetadataChanged(string key, string value)
        {
            base.OnMetadataChanged(key, value);

            bool isScoreRequest = key.StartsWith(TeamScoreKey);
            bool isTeamRequest = key.StartsWith(TeamKey);

            if (isScoreRequest)
            {
                OnRequestTeamPoint(key, value, int.Parse(value));
            }

            if (isTeamRequest) 
            {
                Team team = GetTeamFromValue(value);
                OnRequestTeamChanged(key, value, team);
            }
        }

        protected void OnRequestTeamPoint(string key, string value, int score)
        {
            var ourKey = GetScoreKey(_localTeam);

            if (ourKey == key && score != 0)
            {
                FusionNotifier.Send(new FusionNotification()
                {
                    title = "Team Deathmatch Point",
                    showTitleOnPopup = true,
                    message = $"{_localTeam.TeamName}'s score is {value}!",
                    isMenuItem = false,
                    isPopup = true,
                    popupLength = 0.7f,
                });
            }
        }

        protected void OnRequestTeamChanged(string key, string value, Team team)
        {
            // Find the player that changed
            foreach (var playerId in PlayerIdManager.PlayerIds)
            {
                var playerKey = GetTeamMemberKey(playerId);

                if (playerKey == key)
                {
                    // Check who this is
                    if (playerId.IsSelf)
                    {
                        OnTeamReceived(team);
                    }
                    else if (team != null)
                    {
                        AddLogo(playerId, team);
                    }

                    // Push nametag updates
                    FusionOverrides.ForceUpdateOverrides();

                    break;
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
            _lastTeam = null;

            // Set every team to none
            foreach (var player in PlayerIdManager.PlayerIds)
            {
                SetTeam(player, null);
            }

            // Set every score to 0
            foreach(var team in teams)
            {
                SetScore(team, 0);
            }
        }

        protected void AssignTeam(PlayerId id)
        {
            Team newTeam = _lastTeam;

            // Assign a random team
            newTeam = teams[UnityEngine.Random.Range(0, teams.Count)];

            // Assign it
            SetTeam(id, newTeam);

            // Save the team
            _lastTeam = newTeam;

            // Add the player to the team members list
            newTeam.AddPlayer(id);
        }

        protected void IncrementScore(Team team)
        {
            var currentScore = GetScoreFromTeam(team);
            SetScore(team, currentScore + 1);
        }

        public void SetScore(Team team, int score)
        {
            TrySetMetadata(GetScoreKey(team), score.ToString());
        }

        protected void SetTeam(PlayerId id, Team team)
        {
            if(team == null)
            {
                return;
            }

            TrySetMetadata(GetTeamMemberKey(id), team.TeamName);
        }

        public int GetScoreFromTeam(Team team)
        {
            TryGetMetadata(GetScoreKey(team), out string teamKey);
            int score = int.Parse(teamKey);

            return score;
        }

        protected Team GetTeamFromValue(string nameValue)
        {
            foreach(Team team in teams)
            {
                if(team.TeamName == nameValue)
                {
                    return team;
                }
            }

            return null;
        }

        protected Team GetTeamFromMember(PlayerId id)
        {
            TryGetMetadata(GetTeamMemberKey(id), out string teamName);

            foreach(Team team in teams)
            {
                if(team.TeamName == teamName)
                {
                    return team;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a string key that contains the referenced team's score.
        /// </summary>
        /// <returns>"FusionTDM.Team.Sabrelake",
        /// which can be used to get or set the score of the target team.</returns>
        protected string GetScoreKey(Team team)
        {
            return $"{TeamScoreKey}.{team?.TeamName}";
        }

        /// <summary>
        /// Returns a string key that contains the team member of a team.
        /// </summary>
        /// <returns>FusionTDM.Team.76561197960287930</returns>
        protected string GetTeamMemberKey(PlayerId id)
        {
            return $"{TeamKey}.{id.LongId}";
        }
    }
}