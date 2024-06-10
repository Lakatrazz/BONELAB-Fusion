using BoneLib.BoneMenu.Elements;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.MarrowIntegration;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.SDK.Achievements;
using LabFusion.SDK.Points;
using LabFusion.Senders;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.SDK.Gamemodes
{
    public class TeamDeathmatch : Gamemode
    {
        public const string DefaultSabrelakeName = "Sabrelake";
        public const string DefaultLavaGangName = "Lava Gang";

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

        private readonly FusionDictionary<PlayerId, TeamLogoInstance> _logoInstances = new();

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

        public void SetTeamDisplayName(string teamName, string displayName)
        {
            var team = teams.FirstOrDefault((t) => t.TeamName == teamName);

            if (team != null)
            {
                team.SetDisplayName(displayName);
            }
        }

        public void SetTeamLogo(string teamName, Texture2D logo)
        {
            var team = teams.FirstOrDefault((t) => t.TeamName == teamName);

            if (team != null)
            {
                team.SetLogo(logo);
            }
        }

        public void AddTeam(Team team)
        {
            teams.Add(team);
        }

        public void AddDefaultTeams()
        {
            teams.Clear();

            Team sabrelake = new(DefaultSabrelakeName, Color.yellow);
            Team lavaGang = new(DefaultLavaGangName, Color.magenta);

            AudioLoader.LoadMonoDisc(FusionMonoDiscReferences.SabrelakeVictoryReference, (victory) =>
            {
                AudioLoader.LoadMonoDisc(FusionMonoDiscReferences.SabrelakeFailureReference, (failure) =>
                {
                    sabrelake.SetMusic(victory, failure);
                });
            });

            AudioLoader.LoadMonoDisc(FusionMonoDiscReferences.LavaGangVictoryReference, (victory) =>
            {
                AudioLoader.LoadMonoDisc(FusionMonoDiscReferences.LavaGangFailureReference, (failure) =>
                {
                    lavaGang.SetMusic(victory, failure);
                });
            });

            sabrelake.SetLogo(FusionContentLoader.SabrelakeLogo.Asset);
            lavaGang.SetLogo(FusionContentLoader.LavaGangLogo.Asset);

            AddTeam(sabrelake);
            AddTeam(lavaGang);
        }

        public Team GetTeam(string teamName)
        {
            foreach (Team team in teams)
            {
                if (team.TeamName == teamName)
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

            AudioLoader.LoadMonoDiscs(FusionMonoDiscReferences.CombatSongReferences, (clips) =>
            {
                SetPlaylist(DefaultMusicVolume, clips);
            });

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

            for (int i = 0; i < teams.Count; i++)
            {
                accumulatedScore += GetScoreFromTeam(teams[i]);
            }

            return accumulatedScore;
        }

        private int GetRewardedBits()
        {
            // Change the max bit count based on player count
            int playerCount = PlayerIdManager.PlayerCount - 1;

            // 10 and 100 are the min and max values for the max bit count
            float playerPercent = (float)playerCount / 4f;
            int maxBits = ManagedMathf.FloorToInt(ManagedMathf.Lerp(_minPlayerBits, _maxPlayerBits, playerPercent));
            int maxRand = maxBits / 10;

            // Get the scores
            int score = GetScoreFromTeam(_localTeam);
            int totalScore = GetTotalScore();

            // Prevent divide by 0
            if (totalScore <= 0)
                return 0;

            float percent = ManagedMathf.Clamp01((float)score / (float)totalScore);
            int reward = ManagedMathf.FloorToInt((float)maxBits * percent);

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

                if (otherPlayer == null)
                {
                    return;
                }

                if (otherPlayer == player)
                {
                    return;
                }

                var killerTeam = GetTeamFromMember(otherPlayer);
                var killedTeam = GetTeamFromMember(player);

                if (killerTeam != killedTeam)
                {
                    // Increment score for that team
                    if (NetworkInfo.IsServer)
                    {
                        IncrementScore(killerTeam);
                    }

                    // If we are the killer, increment our achievement
                    if (otherPlayer.IsSelf)
                    {
                        AchievementManager.IncrementAchievements<KillerAchievement>();
                    }
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

            _timeOfStart = TimeUtilities.TimeSinceStartup;
            _oneMinuteLeft = false;

            // Invoke player changes on level load
            FusionSceneManager.HookOnTargetLevelLoad(() =>
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
            leaders = leaders.OrderBy(team => GetScoreFromTeam(team)).Reverse().ToList();

            Team winningTeam = leaders.First();
            Team secondPlaceTeam = leaders[1];

            string message = "";

            bool tied = leaders.All((team) => GetScoreFromTeam(team) == GetScoreFromTeam(winningTeam));

            if (!tied)
            {
                message = $"First Place: {winningTeam.DisplayName} (Score: {GetScoreFromTeam(winningTeam)}) \n";
                message += $"Second Place: {secondPlaceTeam.DisplayName} (Score: {GetScoreFromTeam(secondPlaceTeam)}) \n";

                if (leaders.Count > 2)
                {
                    Team thirdPlaceTeam = leaders[2];
                    message += $"Third Place: {thirdPlaceTeam.DisplayName} (Score: {GetScoreFromTeam(thirdPlaceTeam)}) \n";
                }

                message += GetTeamStatus(winningTeam);
            }
            else
            {
                message += $"Tie! (All Scored: {GetScoreFromTeam(winningTeam)})";
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

        public float GetTimeElapsed()
        {
            return TimeUtilities.TimeSinceStartup - _timeOfStart;
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
            FusionNotification oneMinuteNotification = new()
            {
                title = "Team Deathmatch Timer",
                showTitleOnPopup = true,
                message = "One minute left!",
                isMenuItem = false,
                isPopup = true,
            };

            if (value == "OneMinuteLeft")
            {
                FusionNotifier.Send(oneMinuteNotification);
            }

            if (value == "NaturalEnd")
            {
                int bitReward = GetRewardedBits();

                if (bitReward > 0)
                {
                    PointItemManager.RewardBits(bitReward);
                }
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
            if (team.WinMusic != null)
            {
                FusionAudio.Play2D(team.WinMusic);
                return;
            }

            MonoDiscReference randomChoice = UnityEngine.Random.Range(0, 4) % 2 == 0 ? FusionMonoDiscReferences.LavaGangVictoryReference : FusionMonoDiscReferences.SabrelakeVictoryReference;

            AudioLoader.LoadMonoDisc(randomChoice, (c) =>
            {
                FusionAudio.Play2D(c, DefaultMusicVolume);
            });
        }

        protected void OnTeamLost(Team team)
        {
            if (team.LossMusic != null)
            {
                FusionAudio.Play2D(team.LossMusic);
                return;
            }

            MonoDiscReference randomChoice = UnityEngine.Random.Range(0, 4) % 2 == 0 ? FusionMonoDiscReferences.LavaGangFailureReference : FusionMonoDiscReferences.SabrelakeFailureReference;

            AudioLoader.LoadMonoDisc(randomChoice, (c) =>
            {
                FusionAudio.Play2D(c, DefaultMusicVolume);
            });
        }

        protected void OnTeamTied()
        {
            AudioLoader.LoadMonoDisc(FusionMonoDiscReferences.ErmReference, (c) =>
            {
                FusionAudio.Play2D(c, DefaultMusicVolume);
            });
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
                message = $"Your team is: {team.DisplayName}",
                isMenuItem = false,
                isPopup = true,
                popupLength = 5f,
            };

            FusionNotifier.Send(assignmentNotification);

            _localTeam = team;

            // Invoke ult events
            if (team.TeamName == DefaultSabrelakeName)
            {
                foreach (var ultEvent in InvokeUltEventIfTeamSabrelake.Cache.Components)
                {
                    ultEvent.Invoke();
                }
            }
            else if (team.TeamName == DefaultLavaGangName)
            {
                foreach (var ultEvent in InvokeUltEventIfTeamLavaGang.Cache.Components)
                {
                    ultEvent.Invoke();
                }
            }
            else
            {
                // Likely a custom event for a team
                foreach (var holder in InvokeUltEventIfTeam.Cache.Components)
                {
                    if (team.TeamName != holder.TeamName)
                    {
                        continue;
                    }

                    holder.Invoke();
                }
            }

            // Invoke spawn point changes on level load
            FusionSceneManager.HookOnTargetLevelLoad(() => InitializeTeamSpawns(team));
        }

        protected void InitializeTeamSpawns(Team team)
        {
            // Get all spawn points
            List<Transform> transforms = new List<Transform>();

            if (team.TeamName == DefaultSabrelakeName)
            {
                foreach (var point in SabrelakeSpawnpoint.Cache.Components)
                {
                    transforms.Add(point.transform);
                }
            }
            else if (team.TeamName == DefaultLavaGangName)
            {
                foreach (var point in LavaGangSpawnpoint.Cache.Components)
                {
                    transforms.Add(point.transform);
                }
            }
            else
            {
                // Likely a custom event for a team
                foreach (var point in TeamSpawnpoint.Cache.Components)
                {
                    if (team.TeamName != point.TeamName)
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
                    message = $"{_localTeam.DisplayName}'s score is {value}!",
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
            foreach (var team in teams)
            {
                SetScore(team, 0);
            }
        }

        protected void AssignTeam(PlayerId id)
        {
            // Assign a random team
            List<Team> teamPool = new List<Team>(teams);

            // Remove our last team from the list
            if (teamPool.Count > 1 && _lastTeam != null)
            {
                teamPool.Remove(_lastTeam);
            }

            Team newTeam = teamPool[UnityEngine.Random.Range(0, teamPool.Count)];

            // Assign it
            SetTeam(id, newTeam);

            // Save the team
            _lastTeam = newTeam;

            // Add the player to the team members list
            newTeam.AddPlayer(id);
        }

        protected void IncrementScore(Team team)
        {
            if (team == null)
                return;

            var currentScore = GetScoreFromTeam(team);
            SetScore(team, currentScore + 1);
        }

        public void SetScore(Team team, int score)
        {
            if (team == null)
                return;

            Metadata.TrySetMetadata(GetScoreKey(team), score.ToString());
        }

        protected void SetTeam(PlayerId id, Team team)
        {
            if (team == null)
            {
                return;
            }

            Metadata.TrySetMetadata(GetTeamMemberKey(id), team.TeamName);
        }

        public int GetScoreFromTeam(Team team)
        {
            Metadata.TryGetMetadata(GetScoreKey(team), out string teamKey);
            int score = int.Parse(teamKey);

            return score;
        }

        protected Team GetTeamFromValue(string nameValue)
        {
            foreach (Team team in teams)
            {
                if (team.TeamName == nameValue)
                {
                    return team;
                }
            }

            return null;
        }

        protected Team GetTeamFromMember(PlayerId id)
        {
            Metadata.TryGetMetadata(GetTeamMemberKey(id), out string teamName);

            foreach (Team team in teams)
            {
                if (team.TeamName == teamName)
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