using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.Marrow.Integration;
using LabFusion.Marrow.Player;
using LabFusion.Math;
using LabFusion.Menu;
using LabFusion.Menu.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.SDK.Achievements;
using LabFusion.SDK.Metadata;
using LabFusion.SDK.Points;
using LabFusion.SDK.Triggers;
using LabFusion.UI.Elements;
using LabFusion.UI.Popups;
using LabFusion.UI.Resources;
using LabFusion.UI.Styles;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.SDK.Gamemodes;

public class TeamDeathmatch : Gamemode
{
    public const string DefaultSabrelakeName = "Sabrelake";
    public const string DefaultLavaGangName = "Lava Gang";

    private const int _minPlayerBits = 80;
    private const int _maxPlayerBits = 800;

    public static TeamDeathmatch Instance { get; private set; }

    private const int _defaultMinutes = 3;
    private const int _minMinutes = 2;
    private const int _maxMinutes = 60;

    public override string Title => "Team Deathmatch";
    public override string Author => FusionMod.ModAuthor;
    public override string Description =>
        "Players are randomly selected to be placed in two separate teams. " +
        "Kill players on the opposite team to gain points before the timer runs out! " +
        "More Bits are given based on the amount of points your team gains compared to the other team.";
    public override Texture Logo => MenuResources.GetGamemodeIcon(Title);

    public override bool DisableDevTools => true;
    public override bool DisableSpawnGun => true;
    public override bool DisableManualUnragdoll => true;

    public TriggerEvent OneMinuteLeftTrigger { get; set; }
    public TriggerEvent NaturalEndTrigger { get; set; }

    public MetadataFloat Vitality { get; set; }

    private bool _oneMinuteLeft;

    private int _savedMinutes = _defaultMinutes;
    private int _totalMinutes = _defaultMinutes;

    private float _savedVitality = 1f;

    private int _minimumPlayers = 4;
    public int MinimumPlayers
    {
        get
        {
            return _minimumPlayers;
        }
        set
        {
            _minimumPlayers = value;

            if (!IsStarted && IsSelected)
            {
                GamemodeManager.ValidateReadyConditions();
            }
        }
    }

    public TeamManager TeamManager { get; } = new();

    public Team SabrelakeTeam { get; } = new(DefaultSabrelakeName);

    public Team LavaGangTeam { get; } = new(DefaultLavaGangName);

    private TeamScoreKeeper _scoreKeeper = null;
    public TeamScoreKeeper ScoreKeeper => _scoreKeeper;

    public TeamMusicManager TeamMusicManager { get; } = new();

    public MusicPlaylist MusicPlaylist { get; } = new();

    public TeamLogoManager TeamLogoManager { get; } = new();

    public LabelElement YourTeamLabel { get; private set; } = null;
    public LabelElement YourTeamCounterLabel { get; private set; } = null;

    public LabelElement OpposingTeamLabel { get; private set; } = null;
    public LabelElement OpposingTeamCounterLabel { get; private set; } = null;

    public LabelElement RankingCounterLabel { get; private set; } = null;

    private string _avatarOverride = null;

    public override GroupElementData CreateSettingsGroup()
    {
        var group = base.CreateSettingsGroup();

        var generalGroup = new GroupElementData("General");

        group.AddElement(generalGroup);

        var roundMinutesData = new IntElementData()
        {
            Title = "Round Minutes",
            Value = _totalMinutes,
            Increment = 1,
            MinValue = _minMinutes,
            MaxValue = _maxMinutes,
            OnValueChanged = (v) =>
            {
                _totalMinutes = v;
                _savedMinutes = v;
            },
        };

        generalGroup.AddElement(roundMinutesData);

        var minimumPlayersData = new IntElementData()
        {
            Title = "Minimum Players",
            Value = MinimumPlayers,
            Increment = 1,
            MinValue = 1,
            MaxValue = 255,
            OnValueChanged = (v) =>
            {
                MinimumPlayers = v;
            }
        };

        generalGroup.AddElement(minimumPlayersData);

        var vitalityData = new FloatElementData()
        {
            Title = "Vitality",
            Value = _savedVitality,
            Increment = 0.2f,
            MinValue = 0.2f,
            MaxValue = 100f,
            OnValueChanged = (v) =>
            {
                _savedVitality = v;
            }
        };

        generalGroup.AddElement(vitalityData);

        return group;
    }

    public override UIElement CreateWearableUI()
    {
        var root = base.CreateWearableUI();

        root.Add(CreateStatsRow());

        root.Add(CommonGamemodeUI.CreateRankingUI(out var rankingCounterLabel));
        RankingCounterLabel = rankingCounterLabel;

        UpdateLabels();

        return root;
    }

    private UIElement CreateStatsRow()
    {
        var statsRow = new UIElement();
        statsRow.Style.Direction = Direction.Row;
        statsRow.Style.JustifyContent = Justify.Center;
        statsRow.Style.FontSize = Length.FromRatio(1.17f);

        var yourTeamColumn = CreateTeamColumn(out var yourTeamLabel, out var yourTeamCounterLabel);
        yourTeamLabel.Style.TextColor = new Color(0f, 0.8f, 1f);
        statsRow.Add(yourTeamColumn);

        var verticalSeparator = new UIElement();
        verticalSeparator.AddStyleClass(CommonStyleClasses.VerticalLine);
        verticalSeparator.Style.Margins = new BorderOffsets(5, 5, 0, 0);

        statsRow.Add(verticalSeparator);

        var opposingTeamColumn = CreateTeamColumn(out var opposingTeamLabel, out var opposingTeamCounterLabel);
        opposingTeamLabel.Style.TextColor = new Color(1f, 0.2f, 0f);
        statsRow.Add(opposingTeamColumn);

        YourTeamLabel = yourTeamLabel;
        YourTeamCounterLabel = yourTeamCounterLabel;

        OpposingTeamLabel = opposingTeamLabel;
        OpposingTeamCounterLabel = opposingTeamCounterLabel;

        return statsRow;
    }

    private static UIElement CreateTeamColumn(out LabelElement teamLabel, out LabelElement counterLabel)
    {
        var teamColumn = new UIElement();
        teamColumn.Style.AlignItems = Align.Center;

        teamLabel = new LabelElement("Your Team");
        teamColumn.Add(teamLabel);

        counterLabel = new LabelElement("0");
        teamColumn.Add(counterLabel);

        return teamColumn;
    }

    private void ApplyTeamSettings()
    {
        TeamMusicManager.ClearTeams();
        TeamLogoManager.ClearTeams();

        // Get the default values
        var sabrelakeBarcode = FusionBoneTagReferences.TeamSabrelakeReference.Barcode.ID;
        var lavaGangBarcode = FusionBoneTagReferences.TeamLavaGangReference.Barcode.ID;

        var sabrelakeVictoryReference = FusionMonoDiscReferences.SabrelakeVictoryReference;
        var sabrelakeFailureReference = FusionMonoDiscReferences.SabrelakeFailureReference;

        var lavaGangVictoryReference = FusionMonoDiscReferences.LavaGangVictoryReference;
        var lavaGangFailureReference = FusionMonoDiscReferences.LavaGangFailureReference;

        var tieReference = FusionMonoDiscReferences.ErmReference;

        var sabrelakeLogo = MenuResources.GetLogoIcon("Sabrelake");
        var lavaGangLogo = MenuResources.GetLogoIcon("LavaGang");

        var sabrelakeDisplayName = DefaultSabrelakeName;
        var lavaGangDisplayName = DefaultLavaGangName;

        // Get overrides
        var musicSettings = GamemodeMusicSettings.Instance;

        if (musicSettings != null)
        {
            musicSettings.ApplyTeamOverrides(sabrelakeBarcode, ref sabrelakeVictoryReference, ref sabrelakeFailureReference);
            musicSettings.ApplyTeamOverrides(lavaGangBarcode, ref lavaGangVictoryReference, ref lavaGangFailureReference);

            if (!string.IsNullOrWhiteSpace(musicSettings.TieSongOverride))
            {
                tieReference = new(musicSettings.TieSongOverride);
            }
        }

        var teamSettings = GamemodeTeamSettings.Instance;

        if (teamSettings != null)
        {
            teamSettings.ApplyOverrides(sabrelakeBarcode, ref sabrelakeDisplayName, ref sabrelakeLogo);
            teamSettings.ApplyOverrides(lavaGangBarcode, ref lavaGangDisplayName, ref lavaGangLogo);
        }

        // Apply the settings
        SabrelakeTeam.DisplayName = sabrelakeDisplayName;
        LavaGangTeam.DisplayName = lavaGangDisplayName;

        TeamMusicManager.SetMusic(SabrelakeTeam, new TeamMusic()
        {
            WinMusic = new AudioReference(sabrelakeVictoryReference),
            LoseMusic = new AudioReference(sabrelakeFailureReference),
        });

        TeamMusicManager.SetMusic(LavaGangTeam, new TeamMusic()
        {
            WinMusic = new AudioReference(lavaGangVictoryReference),
            LoseMusic = new AudioReference(lavaGangFailureReference),
        });

        TeamMusicManager.TieMusic = new AudioReference(tieReference);

        TeamLogoManager.SetLogo(SabrelakeTeam, sabrelakeLogo);
        TeamLogoManager.SetLogo(LavaGangTeam, lavaGangLogo);
    }

    public override void OnGamemodeRegistered()
    {
        base.OnGamemodeRegistered();

        Instance = this;

        MultiplayerHooking.OnPlayerJoined += OnPlayerJoin;
        PlayerInteractManager.PlayersInteracted += OnPlayersInteracted;
        FusionOverrides.OnValidateNametag += OnValidateNametag;

        // Register team manager
        TeamManager.Register(this);
        TeamManager.AssignedToTeam += OnAssignedToTeam;

        TeamManager.AddTeam(SabrelakeTeam);
        TeamManager.AddTeam(LavaGangTeam);

        TeamLogoManager.Register(TeamManager);

        // Register score keeper
        _scoreKeeper = new(TeamManager);
        ScoreKeeper.Register(Metadata);

        ScoreKeeper.ScoreChanged += OnScoreChanged;

        // Create triggers
        OneMinuteLeftTrigger = new TriggerEvent(nameof(OneMinuteLeftTrigger), Relay, true);
        OneMinuteLeftTrigger.Triggered += OnOneMinuteLeft;

        NaturalEndTrigger = new TriggerEvent(nameof(NaturalEndTrigger), Relay, true);
        NaturalEndTrigger.Triggered += OnNaturalEnd;

        // Create metadata
        Vitality = new MetadataFloat(nameof(Vitality), Metadata);

        Metadata.OnMetadataChanged += OnMetadataChanged;
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
        TeamManager.AssignedToTeam -= OnAssignedToTeam;

        TeamLogoManager.Unregister();

        // Unregister score keeper
        ScoreKeeper.Unregister();
        ScoreKeeper.ScoreChanged -= OnScoreChanged;

        _scoreKeeper = null;

        MultiplayerHooking.OnPlayerJoined -= OnPlayerJoin;
        PlayerInteractManager.PlayersInteracted -= OnPlayersInteracted;
        FusionOverrides.OnValidateNametag -= OnValidateNametag;

        Metadata.OnMetadataChanged -= OnMetadataChanged;

        // Destroy triggers
        OneMinuteLeftTrigger.UnregisterEvent();
        NaturalEndTrigger.UnregisterEvent();
    }

    private new void OnMetadataChanged(string key, string value)
    {
        switch (key)
        {
            case nameof(Vitality):
                OnApplyVitality();
                break;
        }
    }

    private void OnApplyVitality()
    {
        if (!IsStarted)
        {
            return;
        }

        var vitality = Vitality.GetValue();

        LocalHealth.VitalityOverride = vitality;
    }

    public override bool CheckReadyConditions()
    {
        return PlayerIDManager.PlayerCount >= MinimumPlayers;
    }

    public override bool CanAttack(PlayerID player)
    {
        if (!IsStarted)
        {
            return true;
        }

        return !TeamManager.IsTeammate(player);
    }

    private void OnAssignedToTeam(PlayerID playerId, Team team)
    {
        if (playerId.IsMe)
        {
            OnSelfAssigned(team);
        }

        // Update overrides
        FusionOverrides.ForceUpdateOverrides();
    }

    private void OnSelfAssigned(Team team)
    {
        Notification assignmentNotification = new Notification()
        {
            Title = "Team Deathmatch Assignment",
            Message = $"Your team is: {team.DisplayName}",
            SaveToMenu = false,
            ShowPopup = true,
            PopupLength = 5f,
        };

        Notifier.Send(assignmentNotification);

        // Invoke spawn point changes on level load
        FusionSceneManager.HookOnTargetLevelLoad(() => InitializeTeamSpawns(team));

        UpdateLabels();
    }

    protected bool OnValidateNametag(PlayerID id)
    {
        if (!IsStarted)
        {
            return true;
        }

        return TeamManager.IsTeammate(id);
    }

    public void ApplyGamemodeSettings()
    {
        _totalMinutes = _savedMinutes;

        var songReferences = FusionMonoDiscReferences.CombatSongReferences;

        var musicSettings = GamemodeMusicSettings.Instance;

        if (musicSettings != null && musicSettings.SongOverrides.Count > 0)
        {
            songReferences = new MonoDiscReference[musicSettings.SongOverrides.Count];

            for (var i = 0; i < songReferences.Length; i++)
            {
                songReferences[i] = new(musicSettings.SongOverrides[i]);
            }
        }

        AudioReference[] playlist = AudioReference.CreateReferences(songReferences);

        MusicPlaylist.SetPlaylist(playlist);
        MusicPlaylist.Shuffle();

        ApplyTeamSettings();

        _avatarOverride = null;

        float newVitality = _savedVitality;

        var playerSettings = GamemodePlayerSettings.Instance;

        if (playerSettings != null)
        {
            _avatarOverride = playerSettings.AvatarOverride;

            if (playerSettings.VitalityOverride.HasValue)
            {
                newVitality = playerSettings.VitalityOverride.Value;
            }
        }

        Vitality.SetValue(newVitality);
    }

    private int GetRewardedBits()
    {
        // Change the max bit count based on player count
        int playerCount = PlayerIDManager.PlayerCount - 1;

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

    protected void OnPlayersInteracted(PlayerID playerID, PlayerID otherPlayerID, PlayerInteractType type)
    {
        if (!IsStarted)
        {
            return;
        }

        if (type != PlayerInteractType.KilledByOtherPlayer)
        {
            return;
        }

        if (otherPlayerID == null)
        {
            return;
        }

        if (otherPlayerID == playerID)
        {
            return;
        }

        var killerTeam = TeamManager.GetPlayerTeam(otherPlayerID);
        var killedTeam = TeamManager.GetPlayerTeam(playerID);

        if (killerTeam != killedTeam)
        {
            // Increment score for that team
            if (NetworkInfo.IsHost)
            {
                ScoreKeeper.AddScore(killerTeam);
            }

            // If we are the killer, increment our achievement
            if (otherPlayerID.IsMe)
            {
                AchievementManager.IncrementAchievements<KillerAchievement>();
            }
        }
    }

    /// <summary>
    /// Automatically has the host assign a team to a newly joined player.
    /// </summary>
    /// <param name="id"></param>
    protected void OnPlayerJoin(PlayerID id)
    {
        if (NetworkInfo.IsHost && IsStarted)
        {
            TeamManager.AssignToSmallestTeam(id);
        }
    }

    /// <summary>
    /// Called when the host begins the gamemode. Affects the local player, but also the host so it can send messages to other players.
    /// </summary>
    public override void OnGamemodeStarted()
    {
        base.OnGamemodeStarted();

        ApplyTeamSettings();

        if (NetworkInfo.IsHost)
        {
            ResetTeams();
            SetTeams();
        }

        _oneMinuteLeft = false;

        // Apply overrides
        LocalHealth.MortalityOverride = true;
        LocalControls.DisableSlowMo = true;

        if (_avatarOverride != null)
        {
            LocalAvatar.AvatarOverride = _avatarOverride;
        }

        OnApplyVitality();
    }

    public override void OnLevelReady()
    {
        ApplyGamemodeSettings();

        MusicPlaylist.StartPlaylist();

        // Setup ammo
        LocalInventory.SetAmmo(10000);

        // Push nametag updates
        FusionOverrides.ForceUpdateOverrides();
    }

    /// <summary>
    /// Called when the host ends the active gamemode. Called events affect the local player.
    /// </summary>
    public override void OnGamemodeStopped()
    {
        base.OnGamemodeStopped();

        MusicPlaylist.StopPlaylist();

        TeamLogoManager.ClearTeams();

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
        Notifier.Send(new Notification()
        {
            Title = "Team Deathmatch Completed",

            Message = message,

            PopupLength = 6f,

            SaveToMenu = false,
            ShowPopup = true,
        });

        _oneMinuteLeft = false;

        // Reset mortality
        LocalHealth.MortalityOverride = null;

        // Push nametag updates
        FusionOverrides.ForceUpdateOverrides();

        // Reset overrides
        LocalAvatar.AvatarOverride = null;
        LocalHealth.VitalityOverride = null;
        LocalControls.DisableSlowMo = false;
    }

    public float GetMinutesLeft()
    {
        return _totalMinutes - (ElapsedSeconds / 60f);
    }

    public Team GetHighestOpposingTeam()
    {
        var teams = TeamManager.Teams.OrderBy(team => ScoreKeeper.GetScore(team)).Reverse();
        var localTeam = TeamManager.GetLocalTeam();

        for (var i = 0; i < teams.Count(); i++)
        {
            var team = teams.ElementAt(i);

            if (team == localTeam)
            {
                continue;
            }

            return team;
        }

        return null;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        // Make sure the gamemode is active
        if (!IsStarted)
        {
            return;
        }

        // Update music
        MusicPlaylist.Update();

        // Make sure this is the host
        if (!NetworkInfo.IsHost)
        {
            return;
        }

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
            GamemodeManager.StopGamemode();
            NaturalEndTrigger.TryInvoke();
        }
    }

    private void OnOneMinuteLeft()
    {
        Notifier.Send(new()
        {
            Title = "Team Deathmatch Timer",
            Message = "One minute left!",
            SaveToMenu = false,
            ShowPopup = true,
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
            LocalAudioPlayer.Play2dOneShot(winMusic, LocalAudioPlayer.MusicSettings);

            return;
        }

        MonoDiscReference randomChoice = UnityEngine.Random.Range(0, 4) % 2 == 0 ? FusionMonoDiscReferences.LavaGangVictoryReference : FusionMonoDiscReferences.SabrelakeVictoryReference;

        LocalAudioPlayer.Play2dOneShot(new AudioReference(randomChoice), LocalAudioPlayer.MusicSettings);
    }

    protected void OnTeamLost(Team team)
    {
        var loseMusic = TeamMusicManager.GetMusic(team).LoseMusic;

        if (loseMusic.HasClip())
        {
            LocalAudioPlayer.Play2dOneShot(loseMusic, LocalAudioPlayer.MusicSettings);

            return;
        }

        MonoDiscReference randomChoice = UnityEngine.Random.Range(0, 4) % 2 == 0 ? FusionMonoDiscReferences.LavaGangFailureReference : FusionMonoDiscReferences.SabrelakeFailureReference;

        LocalAudioPlayer.Play2dOneShot(new AudioReference(randomChoice), LocalAudioPlayer.MusicSettings);
    }

    protected void OnTeamTied()
    {
        var tieMusic = TeamMusicManager.TieMusic;

        LocalAudioPlayer.Play2dOneShot(tieMusic, LocalAudioPlayer.MusicSettings);
    }

    protected static void InitializeTeamSpawns(Team team)
    {
        BoneTagReference tag = null;

        if (team.TeamName == DefaultSabrelakeName)
        {
            tag = FusionBoneTagReferences.TeamSabrelakeReference;
        }
        else if (team.TeamName == DefaultLavaGangName)
        {
            tag = FusionBoneTagReferences.TeamLavaGangReference;
        }

        GamemodeHelper.SetSpawnPoints(GamemodeMarker.FilterMarkers(tag));
        GamemodeHelper.TeleportToSpawnPoint();
    }

    private void OnScoreChanged(Team team, int score)
    {
        var localTeam = TeamManager.GetLocalTeam();

        if (team == localTeam && score > 0)
        {
            Notifier.Send(new Notification()
            {
                Title = "Team Deathmatch Point",
                Message = $"{localTeam.DisplayName}'s score is {score}!",
                SaveToMenu = false,
                ShowPopup = true,
                PopupLength = 0.7f,
            });
        }

        UpdateLabels();
    }

    protected void SetTeams()
    {
        // Assign every team
        TeamManager.AssignToRandomTeams();
    }

    protected void ResetTeams()
    {
        // Set every team to none
        TeamManager.UnassignAllPlayers();

        // Reset all scores
        ScoreKeeper.ResetScores();
    }

    private void UpdateLabels()
    {
        UpdateYourTeamLabels();
        UpdateOpposingTeamLabels();
        UpdateRankingCounterLabel();
    }

    private void UpdateYourTeamLabels()
    {
        if (YourTeamLabel == null)
        {
            return;
        }

        var localTeam = TeamManager.GetLocalTeam();

        string teamName = localTeam != null ? localTeam.DisplayName : "No Team";

        YourTeamLabel.Text = teamName;
        YourTeamCounterLabel.Text = ScoreKeeper.GetScore(localTeam).ToString();
    }

    private void UpdateOpposingTeamLabels()
    {
        if (OpposingTeamLabel == null)
        {
            return;
        }

        var highestOpposingTeam = GetHighestOpposingTeam();

        string teamName = highestOpposingTeam != null ? highestOpposingTeam.DisplayName : "No Team";

        OpposingTeamLabel.Text = teamName;
        OpposingTeamCounterLabel.Text = ScoreKeeper.GetScore(highestOpposingTeam).ToString();
    }

    private void UpdateRankingCounterLabel()
    {
        if (RankingCounterLabel == null)
        {
            return;
        }

        var localTeam = TeamManager.GetLocalTeam();

        var placedTeams = TeamManager.Teams.OrderBy(team => ScoreKeeper.GetScore(team)).Reverse().ToList();

        var place = placedTeams.IndexOf(localTeam) + 1;

        RankingCounterLabel.Text = place.ToOrdinal();
    }
}