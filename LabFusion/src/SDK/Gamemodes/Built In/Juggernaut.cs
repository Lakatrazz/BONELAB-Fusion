using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.Marrow.Integration;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.SDK.Gamemodes;

public class Juggernaut : Gamemode
{
    public override string Title => "Juggernaut";

    public override string Author => FusionMod.ModAuthor;

    public static class Defaults
    {
        public const float SurvivorVitality = 1f;

        public const float JuggernautVitality = 20f;

        public const float SurvivorHeight = 1.76f;

        public const float JuggernautHeight = 2.288f;

        public const int MaxPoints = 20;
    }

    private readonly TeamManager _teamManager = new();
    public TeamManager TeamManager => _teamManager;

    private readonly Team _survivorTeam = new("Survivors");
    public Team SurvivorTeam => _survivorTeam;

    private readonly Team _juggernautTeam = new("Juggernaut");
    public Team JuggernautTeam => _juggernautTeam;

    private readonly PlayerScoreKeeper _juggernautScoreKeeper = new();
    public PlayerScoreKeeper JuggernautScoreKeeper => _juggernautScoreKeeper;

    private readonly MusicPlaylist _playlist = new();
    public MusicPlaylist Playlist => _playlist;

    private MonoDiscReference _victorySongReference = null;
    private MonoDiscReference _failureSongReference = null;

    public override void OnGamemodeRegistered()
    {
        TeamManager.Register(this);
        TeamManager.AddTeam(SurvivorTeam);
        TeamManager.AddTeam(JuggernautTeam);

        TeamManager.OnAssignedToTeam += OnAssignedToTeam;

        MultiplayerHooking.OnPlayerAction += OnPlayerAction;

        // Register score keeper
        JuggernautScoreKeeper.Register(Metadata);
        JuggernautScoreKeeper.OnScoreChanged += OnScoreChanged;
    }

    public override void OnGamemodeUnregistered()
    {
        TeamManager.Unregister();

        TeamManager.OnAssignedToTeam -= OnAssignedToTeam;

        MultiplayerHooking.OnPlayerAction -= OnPlayerAction;

        // Unregister score keeper
        JuggernautScoreKeeper.Unregister();
        JuggernautScoreKeeper.OnScoreChanged -= OnScoreChanged;
    }

    public override void OnGamemodeStarted()
    {
        ApplyGamemodeSettings();

        Playlist.StartPlaylist();

        if (NetworkInfo.IsServer)
        {
            JuggernautScoreKeeper.ResetScores();

            AssignTeams();
        }
    }

    public override void OnGamemodeStopped()
    {
        Playlist.StopPlaylist();

        CheckFinalScore();

        if (NetworkInfo.IsServer)
        {
            ClearTeams();
        }

        LocalAvatar.HeightOverride = null;
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

    private void OnPlayerAction(PlayerId player, PlayerActionType type, PlayerId otherPlayer = null)
    {
        if (!IsStarted)
        {
            return;
        }

        if (!NetworkInfo.IsServer)
        {
            return;
        }

        if (type != PlayerActionType.DYING_BY_OTHER_PLAYER)
        {
            return;
        }

        // Don't count self kills
        if (player == otherPlayer)
        {
            return;
        }

        // Juggernaut was killed?
        if (TeamManager.GetPlayerTeam(player) == JuggernautTeam)
        {
            SwapJuggernaut(otherPlayer, player);
        }

        // Juggernaut killed the player?
        if (TeamManager.GetPlayerTeam(otherPlayer) == JuggernautTeam)
        {
            var score = JuggernautScoreKeeper.GetScore(otherPlayer);
            var nextScore = score + 1;

            JuggernautScoreKeeper.SetScore(otherPlayer, nextScore);

            if (nextScore > Defaults.MaxPoints)
            {
                GamemodeManager.StopGamemode();
            }
        }
    }

    private void CheckFinalScore()
    {
        // Get the winner message
        var firstPlace = GetByScore(0);
        var secondPlace = GetByScore(1);
        var thirdPlace = GetByScore(2);

        var selfPlace = GetPlace(PlayerIdManager.LocalId);
        var selfScore = JuggernautScoreKeeper.GetScore(PlayerIdManager.LocalId);

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

        if (selfPlace != -1 && selfPlace > 3)
        {
            message += $"Your Place: {selfPlace} (Score: {selfScore})";
        }

        // Play victory/failure sounds
        int playerCount = PlayerIdManager.PlayerCount;

        if (playerCount > 1)
        {
            bool isVictory = selfPlace <= 1;

            OnVictoryStatus(isVictory);
        }

        FusionNotifier.Send(new FusionNotification()
        {
            Title = "Juggernaut Completed",

            Message = message,

            PopupLength = 6f,

            SaveToMenu = false,
            ShowPopup = true,
        });
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

        var dataCard = stingerReference.DataCard;

        if (dataCard == null)
        {
            return;
        }

        dataCard.AudioClip.LoadAsset((Il2CppSystem.Action<AudioClip>)((c) => {
            SafeAudio3dPlayer.Play2dOneShot(c, SafeAudio3dPlayer.NonDiegeticMusic, SafeAudio3dPlayer.MusicVolume);
        }));
    }

    private void OnScoreChanged(PlayerId player, int score)
    {
        if (score == 0)
        {
            return;
        }

        if (TeamManager.GetPlayerTeam(player) != JuggernautTeam)
        {
            FusionLogger.Warn($"Player {player.SmallId} increased in score, but they aren't the Juggernaut?");

            return;
        }

        if (player.IsMe)
        {
            FusionNotifier.Send(new FusionNotification()
            {
                ShowPopup = true,
                Title = "Juggernaut Point",
                Message = $"Your Juggernaut score is now {score}!",
                PopupLength = 2f,
                Type = NotificationType.INFORMATION,
            });
        }
    }

    private void OnAssignedToTeam(PlayerId player, Team team)
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

    private void OnSelfAssignedToTeam(Team team)
    {
        if (team == SurvivorTeam)
        {
            FusionNotifier.Send(new FusionNotification()
            {
                ShowPopup = true,
                Title = "Survivor",
                Message = "You are a survivor! Defeat the Juggernaut to gain its strength!",
                PopupLength = 4f,
                Type = NotificationType.INFORMATION,
            });

            FusionPlayer.SetPlayerVitality(Defaults.SurvivorVitality);
            LocalAvatar.HeightOverride = Defaults.SurvivorHeight;
        }

        if (team == JuggernautTeam)
        {
            FusionNotifier.Send(new FusionNotification()
            {
                ShowPopup = true,
                Title = "Juggernaut",
                Message = "You are the Juggernaut! Keep your position and kill survivors!",
                PopupLength = 4f,
                Type = NotificationType.INFORMATION,
            });

            FusionPlayer.SetPlayerVitality(Defaults.JuggernautVitality);
            LocalAvatar.HeightOverride = Defaults.JuggernautHeight;
        }
    }

    private void OnOtherAssignedToTeam(PlayerId player, Team team)
    {
        if (team == JuggernautTeam)
        {
            player.TryGetDisplayName(out var name);

            FusionNotifier.Send(new FusionNotification()
            {
                ShowPopup = true,
                Title = $"Juggernaut {name}",
                Message = $"{name} became the Juggernaut!",
                PopupLength = 2f,
                Type = NotificationType.INFORMATION,
            });
        }
    }

    private void SwapJuggernaut(PlayerId killer, PlayerId juggernaut)
    {
        TeamManager.TryAssignTeam(killer, JuggernautTeam);
        TeamManager.TryAssignTeam(juggernaut, SurvivorTeam);
    }

    private void AssignTeams()
    {
        // Shuffle the players for randomness
        var players = new List<PlayerId>(PlayerIdManager.PlayerIds);
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

    public IReadOnlyList<PlayerId> GetPlayersByScore()
    {
        List<PlayerId> leaders = new(PlayerIdManager.PlayerIds);
        leaders = leaders.OrderBy(id => JuggernautScoreKeeper.GetScore(id)).ToList();
        leaders.Reverse();

        return leaders;
    }

    public PlayerId GetByScore(int place)
    {
        var players = GetPlayersByScore();

        if (players != null && players.Count > place)
            return players[place];
        return null;
    }

    public int GetPlace(PlayerId id)
    {
        var players = GetPlayersByScore();

        if (players == null)
        {
            return -1;
        }

        for (var i = 0; i < players.Count; i++)
        {
            if (players[i] == id)
            {
                return i + 1;
            }
        }

        return -1;
    }
}
