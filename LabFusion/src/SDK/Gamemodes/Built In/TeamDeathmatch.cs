using BoneLib.BoneMenu;

using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.Marrow.Integration;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.SDK.Achievements;
using LabFusion.SDK.Points;
using LabFusion.SDK.Triggers;
using LabFusion.Senders;
using LabFusion.Utilities;
using LabFusion.Scene;

using UnityEngine;

namespace LabFusion.SDK.Gamemodes;

public class TeamDeathmatch : Gamemode
{
    public const string DefaultSabrelakeName = "Sabrelake";
    public const string DefaultLavaGangName = "Lava Gang";

    private const int _minPlayerBits = 30;
    private const int _maxPlayerBits = 250;

    public static TeamDeathmatch Instance { get; private set; }

    public bool OverrideValues { get => _overrideValues; }

    protected string _lavaGangOverride = null;
    protected string _sabrelakeOverride = null;

    protected Texture2D _lavaGangLogoOverride = null;
    protected Texture2D _sabrelakeLogoOverride = null;

    private const int _defaultMinutes = 3;
    private const int _minMinutes = 2;
    private const int _maxMinutes = 60;

    public override string GamemodeCategory => "Fusion";
    public override string GamemodeName => "Team Deathmatch";

    public override bool DisableDevTools => true;
    public override bool DisableSpawnGun => true;
    public override bool DisableManualUnragdoll => true;

    public override bool PreventNewJoins => !_enabledLateJoining;

    public TriggerEvent OneMinuteLeftTrigger { get; set; }
    public TriggerEvent NaturalEndTrigger { get; set; }

    private float _timeOfStart;
    private bool _oneMinuteLeft;

    private bool _overrideValues;

    private int _savedMinutes = _defaultMinutes;
    private int _totalMinutes = _defaultMinutes;

    private readonly TeamManager _teamManager = new();
    public TeamManager TeamManager => _teamManager;

    private TeamScoreKeeper _scoreKeeper = null;
    public TeamScoreKeeper ScoreKeeper => _scoreKeeper;

    private readonly TeamMusicManager _teamMusicManager = new();
    public TeamMusicManager TeamMusicManager => _teamMusicManager;

    private readonly MusicPlaylist _musicPlaylist = new();
    public MusicPlaylist MusicPlaylist => _musicPlaylist;

    private readonly FusionDictionary<PlayerId, TeamLogoInstance> _logoInstances = new();

    private string _avatarOverride = null;
    private float? _vitalityOverride = null;

    private bool _enabledLateJoining = true;

    public override void OnBoneMenuCreated(Page page)
    {
        base.OnBoneMenuCreated(page);

        page.CreateInt("Round Minutes", Color.white, startingValue: _totalMinutes, increment: 1, minValue: _minMinutes, maxValue: _maxMinutes, callback: (v) =>
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
        var team = TeamManager.GetTeamByName(teamName);

        if (team != null)
        {
            team.DisplayName = displayName;
        }
    }

    public void SetTeamLogo(string teamName, Texture2D logo)
    {
        // var team = teams.FirstOrDefault((t) => t.TeamName == teamName);
        // 
        // if (team != null)
        // {
        //     team.SetLogo(logo);
        // }
    }

    public void AddDefaultTeams()
    {
        TeamManager.ClearTeams();
        TeamMusicManager.ClearTeams();

        Team sabrelake = new(DefaultSabrelakeName);
        Team lavaGang = new(DefaultLavaGangName);

        TeamMusicManager.SetMusic(sabrelake, new TeamMusic()
        {
            WinMusic = new AudioReference(FusionMonoDiscReferences.SabrelakeVictoryReference),
            LoseMusic = new AudioReference(FusionMonoDiscReferences.SabrelakeFailureReference),
        });

        TeamMusicManager.SetMusic(lavaGang, new TeamMusic()
        {
            WinMusic = new AudioReference(FusionMonoDiscReferences.LavaGangVictoryReference),
            LoseMusic = new AudioReference(FusionMonoDiscReferences.LavaGangFailureReference),
        });

        TeamMusicManager.TieMusic = new AudioReference(FusionMonoDiscReferences.ErmReference);

        //sabrelake.SetLogo(FusionContentLoader.SabrelakeLogo.Asset);
        //lavaGang.SetLogo(FusionContentLoader.LavaGangLogo.Asset);

        TeamManager.AddTeam(sabrelake);
        TeamManager.AddTeam(lavaGang);
    }

    public override void OnGamemodeRegistered()
    {
        base.OnGamemodeRegistered();

        Instance = this;

        MultiplayerHooking.OnPlayerJoin += OnPlayerJoin;
        MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
        MultiplayerHooking.OnPlayerAction += OnPlayerAction;
        FusionOverrides.OnValidateNametag += OnValidateNametag;

        // Register team manager
        TeamManager.Register(this);
        TeamManager.OnAssignedToTeam += OnAssignedToTeam;

        // Register score keeper
        _scoreKeeper = new(TeamManager);
        ScoreKeeper.Register(Metadata);

        ScoreKeeper.OnScoreChanged += OnScoreChanged;

        // Create triggers
        OneMinuteLeftTrigger = new TriggerEvent(nameof(OneMinuteLeftTrigger), Relay, true);
        OneMinuteLeftTrigger.OnTriggered += OnOneMinuteLeft;

        NaturalEndTrigger = new TriggerEvent(nameof(NaturalEndTrigger), Relay, true);
        NaturalEndTrigger.OnTriggered += OnNaturalEnd;

        SetDefaultValues();
    }

    public override void OnGamemodeUnregistered()
    {
        base.OnGamemodeUnregistered();

        if (Instance == this)
        {
            Instance = null;
        }

        // Unregister team manager
        TeamManager.Unregister();
        TeamManager.OnAssignedToTeam -= OnAssignedToTeam;

        // Unregister score keeper
        ScoreKeeper.Unregister();
        ScoreKeeper.OnScoreChanged -= OnScoreChanged;

        _scoreKeeper = null;

        MultiplayerHooking.OnPlayerJoin -= OnPlayerJoin;
        MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
        MultiplayerHooking.OnPlayerAction -= OnPlayerAction;
        FusionOverrides.OnValidateNametag -= OnValidateNametag;

        // Destroy triggers
        OneMinuteLeftTrigger.UnregisterEvent();
        NaturalEndTrigger.UnregisterEvent();
    }

    private void OnAssignedToTeam(PlayerId playerId, Team team)
    {
        if (playerId.IsMe)
        {
            OnSelfAssigned(team);
        }
        else
        {
            OnOtherAssigned(playerId, team);
        }

        // Update overrides
        FusionOverrides.ForceUpdateOverrides();
    }

    private void OnSelfAssigned(Team team)
    {
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

        // Invoke spawn point changes on level load
        FusionSceneManager.HookOnTargetLevelLoad(() => InitializeTeamSpawns(team));
    }

    private void OnOtherAssigned(PlayerId playerId, Team team)
    {
        AddLogo(playerId, team);
    }

    protected bool OnValidateNametag(PlayerId id)
    {
        if (!IsActive())
        {
            return true;
        }

        return TeamManager.GetPlayerTeam(id) == TeamManager.GetLocalTeam();
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

        AudioReference[] playlist = AudioReference.CreateReferences(FusionMonoDiscReferences.CombatSongReferences);
        MusicPlaylist.SetPlaylist(playlist);

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

    private int GetRewardedBits()
    {
        // Change the max bit count based on player count
        int playerCount = PlayerIdManager.PlayerCount - 1;

        // 10 and 100 are the min and max values for the max bit count
        float playerPercent = (float)playerCount / 4f;
        int maxBits = ManagedMathf.FloorToInt(ManagedMathf.Lerp(_minPlayerBits, _maxPlayerBits, playerPercent));
        int maxRand = maxBits / 10;

        // Get the scores
        int score = ScoreKeeper.GetScore(TeamManager.GetLocalTeam());
        int totalScore = ScoreKeeper.GetTotalScore();

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
        if (!NetworkInfo.IsServer)
        {
            return;
        }

        if (!IsActive())
        {
            return;
        }

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

        var killerTeam = TeamManager.GetPlayerTeam(otherPlayer);
        var killedTeam = TeamManager.GetPlayerTeam(player);

        if (killerTeam != killedTeam)
        {
            // Increment score for that team
            if (NetworkInfo.IsServer)
            {
                ScoreKeeper.AddScore(killerTeam);
            }

            // If we are the killer, increment our achievement
            if (otherPlayer.IsMe)
            {
                AchievementManager.IncrementAchievements<KillerAchievement>();
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

        MusicPlaylist.StartPlaylist();

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

        MusicPlaylist.StopPlaylist();

        var leaders = TeamManager.Teams.OrderBy(team => ScoreKeeper.GetScore(team)).Reverse().ToList();

        Team winningTeam = leaders.First();
        Team secondPlaceTeam = leaders[1];

        string message = "";

        bool tied = leaders.All((team) => ScoreKeeper.GetScore(team) == ScoreKeeper.GetScore(winningTeam));

        if (!tied)
        {
            message = $"First Place: {winningTeam.DisplayName} (Score: {ScoreKeeper.GetScore(winningTeam)}) \n";
            message += $"Second Place: {secondPlaceTeam.DisplayName} (Score: {ScoreKeeper.GetScore(secondPlaceTeam)}) \n";

            if (leaders.Count > 2)
            {
                Team thirdPlaceTeam = leaders[2];
                message += $"Third Place: {thirdPlaceTeam.DisplayName} (Score: {ScoreKeeper.GetScore(thirdPlaceTeam)}) \n";
            }

            message += GetTeamStatus(winningTeam);
        }
        else
        {
            message += $"Tie! (All Scored: {ScoreKeeper.GetScore(winningTeam)})";
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

        // Make sure the gamemode is active
        if (!IsActive())
        {
            return;
        }

        // Update music
        MusicPlaylist.Update();

        // Make sure this is the host
        if (!NetworkInfo.IsServer)
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
                OneMinuteLeftTrigger.TryInvoke();
                _oneMinuteLeft = true;
            }
        }

        // Should the gamemode end?
        if (minutesLeft <= 0f)
        {
            StopGamemode();
            NaturalEndTrigger.TryInvoke();
        }
    }

    protected void UpdateLogos()
    {
        // Update logos
        foreach (var logo in _logoInstances.Values)
        {
            // Change visibility
            bool visible = logo.team == TeamManager.GetLocalTeam();
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

    private void OnOneMinuteLeft()
    {
        FusionNotifier.Send(new()
        {
            title = "Team Deathmatch Timer",
            showTitleOnPopup = true,
            message = "One minute left!",
            isMenuItem = false,
            isPopup = true,
        });
    }

    private void OnNaturalEnd()
    {
        int bitReward = GetRewardedBits();

        if (bitReward > 0)
        {
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
        var localTeam = TeamManager.GetLocalTeam();

        if (localTeam == winner)
        {
            OnTeamVictory(localTeam);
            return "You Won!";
        }
        else
        {
            OnTeamLost(localTeam);
            return "You Lost...";
        }
    }

    protected void OnTeamVictory(Team team)
    {
        var winMusic = TeamMusicManager.GetMusic(team).WinMusic;

        if (winMusic.HasClip())
        {
            winMusic.LoadClip((clip) =>
            {
                FusionAudio.Play2D(clip, 0.2f);
            });

            return;
        }

        MonoDiscReference randomChoice = UnityEngine.Random.Range(0, 4) % 2 == 0 ? FusionMonoDiscReferences.LavaGangVictoryReference : FusionMonoDiscReferences.SabrelakeVictoryReference;

        AudioLoader.LoadMonoDisc(randomChoice, (c) =>
        {
            FusionAudio.Play2D(c, 0.2f);
        });
    }

    protected void OnTeamLost(Team team)
    {
        var loseMusic = TeamMusicManager.GetMusic(team).LoseMusic;

        if (loseMusic.HasClip())
        {
            loseMusic.LoadClip((clip) =>
            {
                FusionAudio.Play2D(clip, 0.2f);
            });

            return;
        }

        MonoDiscReference randomChoice = UnityEngine.Random.Range(0, 4) % 2 == 0 ? FusionMonoDiscReferences.LavaGangFailureReference : FusionMonoDiscReferences.SabrelakeFailureReference;

        AudioLoader.LoadMonoDisc(randomChoice, (c) =>
        {
            FusionAudio.Play2D(c, 0.2f);
        });
    }

    protected void OnTeamTied()
    {
        var tieMusic = TeamMusicManager.TieMusic;

        if (!tieMusic.HasClip())
        {
            return;
        }

        tieMusic.LoadClip((clip) =>
        {
            FusionAudio.Play2D(clip, 0.2f);
        });
    }

    protected static void InitializeTeamSpawns(Team team)
    {
        // Get all spawn points
        List<Transform> transforms = new List<Transform>();
        BoneTagReference tag = null;

        if (team.TeamName == DefaultSabrelakeName)
        {
            tag = FusionBoneTagReferences.TeamSabrelakeReference;
        }
        else if (team.TeamName == DefaultLavaGangName)
        {
            tag = FusionBoneTagReferences.TeamLavaGangReference;
        }

        var markers = GamemodeMarker.FilterMarkers(tag);

        foreach (var marker in markers)
        {
            transforms.Add(marker.transform);
        }

        FusionPlayer.SetSpawnPoints(transforms.ToArray());

        // Teleport to a random spawn point
        if (FusionPlayer.TryGetSpawnPoint(out var spawn))
        {
            FusionPlayer.Teleport(spawn.position, spawn.forward);
        }
    }

    private void OnScoreChanged(Team team, int score)
    {
        var localTeam = TeamManager.GetLocalTeam();

        if (team == localTeam && score > 0)
        {
            FusionNotifier.Send(new FusionNotification()
            {
                title = "Team Deathmatch Point",
                showTitleOnPopup = true,
                message = $"{localTeam.DisplayName}'s score is {score}!",
                isMenuItem = false,
                isPopup = true,
                popupLength = 0.7f,
            });
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
        // Set every team to none
        TeamManager.UnassignAllPlayers();

        // Reset all scores
        ScoreKeeper.ResetScores();
    }

    protected void AssignTeam(PlayerId id)
    {
        // Get team with fewest players
        var newTeam = TeamManager.GetTeamWithFewestPlayers();

        TeamManager.TryAssignTeam(id, newTeam);
    }
}