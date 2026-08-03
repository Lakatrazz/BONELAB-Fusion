using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.Marrow.Integration;
using LabFusion.Menu;
using LabFusion.Menu.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.UI.Popups;
using LabFusion.SDK.Points;
using LabFusion.Senders;
using LabFusion.Utilities;
using LabFusion.UI.Elements;

using UnityEngine;

using Il2CppTMPro;
using LabFusion.Marrow.Player;

namespace LabFusion.SDK.Gamemodes;

public class Juggernaut : Gamemode
{
    public override string Title => "Juggernaut";

    public override string Author => FusionMod.ModAuthor;

    public override string Description => 
        "One player becomes the Juggernaut, a massive beast with copious amounts of health! " +
        "Defeat the Juggernaut to gain their power, and kill the remaining survivors! " +
        "First Juggernaut to reach 20 kills wins. More Bits are given for each kill.";

    public override Texture Logo => MenuResources.GetGamemodeIcon(Title);

    public override bool AutoHolsterOnDeath => true;

    public override bool DisableDevTools => true;

    public override bool DisableSpawnGun => true;

    public override bool DisableManualUnragdoll => true;

    public static class Defaults
    {
        public const float SurvivorVitality = 1f;

        public const float JuggernautVitality = 10f;

        public const float SurvivorHeight = MarrowConstants.StandardHeight;

        public const float JuggernautHeight = MarrowConstants.StandardHeight * 1.3f;

        public const int MaxPoints = 20;

        public const int MaxBits = 1000;

        public const string SurvivorTeamName = "Survivors";

        public const string JuggernautTeamName = "Juggernaut";
    }

    public TeamManager TeamManager { get; } = new();

    public Team SurvivorTeam { get; } = new(Defaults.SurvivorTeamName);

    public Team JuggernautTeam { get; } = new(Defaults.JuggernautTeamName);

    public PlayerScoreKeeper JuggernautScoreKeeper { get; } = new();

    public MusicPlaylist Playlist { get; } = new();

    public LabelElement RoleLabel { get; private set; } = null;
    public LabelElement ObjectiveLabel { get; private set; } = null;
    public LabelElement ScoreLabel { get; private set; } = null;

    private MonoDiscReference _victorySongReference = null;
    private MonoDiscReference _failureSongReference = null;

    private int _minimumPlayers = 2;
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

    public override UIElement CreateWearableUI()
    {
        var root = base.CreateWearableUI();

        var roleObjectiveRoot = CommonGamemodeUI.CreateRoleObjectiveUI(out var roleLabel, out var objectiveLabel);

        RoleLabel = roleLabel;
        ObjectiveLabel = objectiveLabel;

        ScoreLabel = new LabelElement("Score 0 / 0");
        roleObjectiveRoot.Add(ScoreLabel);

        root.Add(roleObjectiveRoot);

        UpdateLabels();

        return root;
    }

    public override void OnGamemodeRegistered()
    {
        TeamManager.Register(this);
        TeamManager.AddTeam(SurvivorTeam);
        TeamManager.AddTeam(JuggernautTeam);

        TeamManager.AssignedToTeam += OnAssignedToTeam;
        TeamManager.RemovedFromTeam += OnRemovedFromTeam;

        PlayerInteractManager.PlayersInteracted += OnPlayersInteracted;
        MultiplayerHooking.OnPlayerJoined += OnPlayerJoin;
        MultiplayerHooking.OnPlayerLeft += OnPlayerLeave;

        // Register score keeper
        JuggernautScoreKeeper.Register(Metadata);
        JuggernautScoreKeeper.PlayerScoreChanged += OnScoreChanged;
    }

    public override void OnGamemodeUnregistered()
    {
        TeamManager.Unregister();

        TeamManager.AssignedToTeam -= OnAssignedToTeam;
        TeamManager.RemovedFromTeam -= OnRemovedFromTeam;

        PlayerInteractManager.PlayersInteracted -= OnPlayersInteracted;
        MultiplayerHooking.OnPlayerJoined -= OnPlayerJoin;
        MultiplayerHooking.OnPlayerLeft -= OnPlayerLeave;

        // Unregister score keeper
        JuggernautScoreKeeper.Unregister();
        JuggernautScoreKeeper.PlayerScoreChanged -= OnScoreChanged;
    }

    public override void OnGamemodeStarted()
    {
        if (ServerManager.IsServerRunning)
        {
            JuggernautScoreKeeper.ResetScores();

            AssignTeams();
        }

        LocalHealth.MortalityOverride = true;
        LocalControls.DisableSlowMo = true;
    }

    public override void OnLevelReady()
    {
        ApplyGamemodeSettings();

        Playlist.StartPlaylist();

        GamemodeHelper.SetSpawnPoints(GamemodeMarker.FilterMarkers(null));
        GamemodeHelper.TeleportToSpawnPoint();

        LocalInventory.SetAmmo(10000);
    }

    public override void OnGamemodeStopped()
    {
        Playlist.StopPlaylist();

        CheckFinalScore();

        if (ServerManager.IsServerRunning)
        {
            ClearTeams();
        }

        LocalAvatar.HeightOverride = null;

        LocalHealth.MortalityOverride = null;
        LocalHealth.RegenerationOverride = null;
        LocalHealth.VitalityOverride = null;

        LocalControls.DisableSlowMo = false;

        GamemodeHelper.ResetSpawnPoints();
    }

    public override GroupElementData CreateSettingsGroup()
    {
        var group = base.CreateSettingsGroup();

        var generalGroup = new GroupElementData("General");

        group.AddElement(generalGroup);

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

        return group;
    }

    public override bool CheckReadyConditions()
    {
        return PlayerIDManager.PlayerCount >= MinimumPlayers;
    }

    protected override void OnUpdate()
    {
        // Make sure the gamemode is active
        if (!IsStarted)
        {
            return;
        }

        // Update music
        Playlist.Update();
    }

    public override bool CanAttack(PlayerID player)
    {
        if (!IsStarted)
        {
            return true;
        }

        return !TeamManager.IsTeammate(player);
    }

    public void ApplyGamemodeSettings()
    {
        _victorySongReference = FusionMonoDiscReferences.LavaGangVictoryReference;
        _failureSongReference = FusionMonoDiscReferences.LavaGangFailureReference;

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

        if (musicSettings != null && !string.IsNullOrWhiteSpace(musicSettings.VictorySongOverride))
        {
            _victorySongReference = new MonoDiscReference(musicSettings.VictorySongOverride);
        }

        if (musicSettings != null && !string.IsNullOrWhiteSpace(musicSettings.FailureSongOverride))
        {
            _failureSongReference = new MonoDiscReference(musicSettings.FailureSongOverride);
        }

        AudioReference[] playlist = AudioReference.CreateReferences(songReferences);

        Playlist.SetPlaylist(playlist);
        Playlist.Shuffle();
    }

    public static string GetTeamRole(Team team)
    {
        if (team == null)
        {
            return "No Role";
        }

        return team.TeamName switch
        {
            Defaults.JuggernautTeamName => "Juggernaut",
            Defaults.SurvivorTeamName => "Survivor",
            _ => team.TeamName,
        };
    }

    public static string GetTeamObjective(Team team)
    {
        if (team == null)
        {
            return "No Objective";
        }

        return team.TeamName switch
        {
            Defaults.JuggernautTeamName => "Kill the Survivors",
            Defaults.SurvivorTeamName => "Kill the Juggernaut and Gain its Power",
            _ => "Unknown Objective",
        };
    }

    private void OnPlayersInteracted(PlayerID playerID, PlayerID otherPlayerID, PlayerInteractType type)
    {
        if (!IsStarted)
        {
            return;
        }

        if (!ServerManager.IsServerRunning)
        {
            return;
        }

        if (type != PlayerInteractType.KilledByOtherPlayer)
        {
            return;
        }

        bool selfKill = playerID == otherPlayerID;

        bool juggernautWasKilled = TeamManager.GetPlayerTeam(playerID) == JuggernautTeam;

        bool juggernautGotKill = TeamManager.GetPlayerTeam(otherPlayerID) == JuggernautTeam;

        if (selfKill)
        {
            // Juggernaut killed themselves? Give the title to a random player
            if (juggernautWasKilled && PlayerIDManager.HasOtherPlayers)
            {
                var otherPlayers = PlayerIDManager.PlayerIDs.Where(id => id.SmallID != playerID.SmallID);
                SwapJuggernaut(otherPlayers.GetRandom(), playerID);
            }

            return;
        }

        // Juggernaut was killed?
        if (juggernautWasKilled)
        {
            SwapJuggernaut(otherPlayerID, playerID);
        }

        // Juggernaut killed the player?
        if (juggernautGotKill)
        {
            var score = JuggernautScoreKeeper.GetScore(otherPlayerID);
            var nextScore = score + 1;

            JuggernautScoreKeeper.SetScore(otherPlayerID, nextScore);

            if (nextScore >= Defaults.MaxPoints)
            {
                GamemodeManager.StopGamemode();
            }
        }
    }

    private void OnPlayerJoin(PlayerID playerId)
    {
        if (!IsStarted || !ServerManager.IsServerRunning)
        {
            return;
        }

        TeamManager.TryAssignTeam(playerId, SurvivorTeam);
    }

    private void OnPlayerLeave(PlayerID playerId)
    {
        if (!IsStarted || !ServerManager.IsServerRunning)
        {
            return;
        }

        bool isJuggernaut = TeamManager.GetPlayerTeam(playerId) == JuggernautTeam;

        if (isJuggernaut)
        {
            TeamManager.TryAssignTeam(PlayerIDManager.PlayerIDs.GetRandom(), JuggernautTeam);
        }

        TeamManager.TryUnassignTeam(playerId);
    }

    private void CheckFinalScore()
    {
        // Get the winner message
        var firstPlace = JuggernautScoreKeeper.GetPlayerByPlace(0);
        var secondPlace = JuggernautScoreKeeper.GetPlayerByPlace(1);
        var thirdPlace = JuggernautScoreKeeper.GetPlayerByPlace(2);

        var selfPlace = JuggernautScoreKeeper.GetPlace(PlayerIDManager.LocalID) + 1;
        var ordinalPlace = selfPlace.ToOrdinal();

        var selfScore = JuggernautScoreKeeper.GetScore(PlayerIDManager.LocalID);

        string message = "No one scored points!";

        if (firstPlace != null && firstPlace.TryGetDisplayName(out var name))
        {
            message = $"First Place: {name} (Score: {JuggernautScoreKeeper.GetScore(firstPlace)}) \n";
        }

        if (secondPlace != null && secondPlace.TryGetDisplayName(out name))
        {
            message += $"Second Place: {name} (Score: {JuggernautScoreKeeper.GetScore(secondPlace)}) \n";
        }

        if (thirdPlace != null && thirdPlace.TryGetDisplayName(out name))
        {
            message += $"Third Place: {name} (Score: {JuggernautScoreKeeper.GetScore(thirdPlace)}) \n";
        }

        if (selfPlace > 3)
        {
            message += $"Your Place: {ordinalPlace} (Score: {selfScore})";
        }

        // Play victory/failure sounds
        int playerCount = PlayerIDManager.PlayerCount;

        if (playerCount > 1)
        {
            bool isVictory = selfPlace <= 1;

            OnVictoryStatus(isVictory);
        }

        Notifier.Send(new Notification()
        {
            Title = "Juggernaut Completed",

            Message = message,

            PopupLength = 6f,

            SaveToMenu = false,
            ShowPopup = true,
        });

        // Reward bits
        var bitReward = CalculateBitReward(selfScore);

        if (bitReward > 0)
        {
            PointItemManager.RewardBits(bitReward);
        }
    }

    private void OnVictoryStatus(bool isVictory = false)
    {
        MonoDiscReference stingerReference;

        if (isVictory)
        {
            stingerReference = _victorySongReference;
        }
        else
        {
            stingerReference = _failureSongReference;
        }

        if (stingerReference == null)
        {
            return;
        }

        LocalAudioPlayer.Play2dOneShot(new AudioReference(stingerReference), LocalAudioPlayer.MusicSettings);
    }

    private void OnScoreChanged(PlayerID player, int score)
    {
        if (player.IsMe)
        {
            UpdateScoreLabel();
        }

        if (score == 0)
        {
            return;
        }

        if (TeamManager.GetPlayerTeam(player) != JuggernautTeam)
        {
            FusionLogger.Warn($"Player {player.SmallID} increased in score, but they aren't the Juggernaut?");

            return;
        }

        if (player.IsMe)
        {
            Notifier.Send(new Notification()
            {
                ShowPopup = true,
                Title = "Juggernaut Point",
                Message = $"Your Juggernaut score is now {score}!",
                PopupLength = 2f,
                Type = NotificationType.INFORMATION,
            });
        }
    }

    private static float CalculateJuggernautHealth()
    {
        var otherPlayers = Mathf.Max(1, PlayerIDManager.PlayerCount - 1);

        float health = Defaults.JuggernautVitality * Mathf.Sqrt(otherPlayers);

        return Mathf.Round(health * 0.1f) * 10f;
    }

    private static int CalculateBitReward(int score)
    {
        float percent = Mathf.Pow((float)score / (float)Defaults.MaxPoints, 2f);

        return Mathf.Clamp(Mathf.RoundToInt(percent * Defaults.MaxBits), 0, Defaults.MaxBits);
    }

    private void OnAssignedToTeam(PlayerID player, Team team)
    {
        if (player.IsMe)
        {
            OnSelfAssignedToTeam(team);
        }
        else
        {
            OnOtherAssignedToTeam(player, team);
        }
    }

    private void OnRemovedFromTeam(PlayerID player, Team team)
    {
        if (team == JuggernautTeam && NetworkPlayerManager.TryGetPlayer(player.SmallID, out var networkPlayer))
        {
            networkPlayer.HealthBar.Visible = false;
        }
    }

    private void OnSelfAssignedToTeam(Team team)
    {
        if (team == SurvivorTeam)
        {
            Notifier.Send(new Notification()
            {
                ShowPopup = true,
                Title = "Survivor",
                Message = "You are a survivor! Defeat the Juggernaut to gain its strength!",
                PopupLength = 4f,
                Type = NotificationType.INFORMATION,
            });

            LocalHealth.VitalityOverride = Defaults.SurvivorVitality;
            LocalHealth.RegenerationOverride = false;

            LocalAvatar.HeightOverride = Defaults.SurvivorHeight;
        }

        if (team == JuggernautTeam)
        {
            Notifier.Send(new Notification()
            {
                ShowPopup = true,
                Title = "Juggernaut",
                Message = "You are the Juggernaut! Keep your position and kill survivors!",
                PopupLength = 4f,
                Type = NotificationType.INFORMATION,
            });

            LocalHealth.VitalityOverride = CalculateJuggernautHealth();
            LocalHealth.RegenerationOverride = false;
            LocalHealth.SetFullHealth();

            LocalAvatar.HeightOverride = Defaults.JuggernautHeight;
        }

        UpdateLabels();
    }

    private void OnOtherAssignedToTeam(PlayerID player, Team team)
    {
        bool healthBarVisible = false;

        if (team == JuggernautTeam)
        {
            player.TryGetDisplayName(out var name);

            Notifier.Send(new Notification()
            {
                ShowPopup = true,
                Title = $"Juggernaut {name}",
                Message = $"{name} became the Juggernaut!",
                PopupLength = 2f,
                Type = NotificationType.INFORMATION,
            });

            healthBarVisible = true;
        }

        if (NetworkPlayerManager.TryGetPlayer(player.SmallID, out var networkPlayer))
        {
            networkPlayer.HealthBar.Visible = healthBarVisible;
        }
    }

    private void SwapJuggernaut(PlayerID killer, PlayerID juggernaut)
    {
        TeamManager.TryAssignTeam(killer, JuggernautTeam);
        TeamManager.TryAssignTeam(juggernaut, SurvivorTeam);
    }

    private void AssignTeams()
    {
        // Shuffle the players for randomness
        var players = new List<PlayerID>(PlayerIDManager.PlayerIDs);
        players.Shuffle();

        // Assign Juggernaut
        if (players.Count > 0)
        {
            var player = players[0];
            TeamManager.TryAssignTeam(player, JuggernautTeam);

            // Remove the player from the list
            players.Remove(player);
        }

        // Assign the rest as survivors
        foreach (var player in players)
        {
            TeamManager.TryAssignTeam(player, SurvivorTeam);
        }
    }

    private void ClearTeams()
    {
        TeamManager.UnassignAllPlayers();
    }

    private void UpdateLabels()
    {
        UpdateRoleLabel();
        UpdateObjectiveLabel();
        UpdateScoreLabel();
    }

    private void UpdateRoleLabel()
    {
        if (RoleLabel == null)
        {
            return;
        }

        RoleLabel.Text = GetTeamRole(TeamManager.GetLocalTeam());
    }

    private void UpdateObjectiveLabel()
    {
        if (ObjectiveLabel == null)
        {
            return;
        }

        ObjectiveLabel.Text = GetTeamObjective(TeamManager.GetLocalTeam());
    }

    private void UpdateScoreLabel()
    {
        if (ScoreLabel == null)
        {
            return;
        }

        ScoreLabel.Text = $"Score {JuggernautScoreKeeper.GetScore(PlayerIDManager.LocalID)} / {Defaults.MaxPoints}";
    }
}
