using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.Marrow.Integration;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.SDK.Achievements;
using LabFusion.SDK.Points;
using LabFusion.Senders;
using LabFusion.Utilities;
using LabFusion.SDK.Triggers;
using LabFusion.Menu;
using LabFusion.Math;
using LabFusion.UI.Popups;

using UnityEngine;

using LabFusion.Menu.Data;
using LabFusion.SDK.Metadata;

namespace LabFusion.SDK.Gamemodes;

using System;

public class Deathmatch : Gamemode
{
    private const int _minPlayerBits = 60;
    private const int _maxPlayerBits = 600;

    private const int _defaultMinutes = 3;
    private const int _minMinutes = 2;
    private const int _maxMinutes = 60;

    public override string Title => "Deathmatch";
    public override string Author => FusionMod.ModAuthor;
    public override string Description =>
        "All players are pitted against each other in a free for all! " +
        "Kill other players to gain points before the timer runs out! " +
        "More Bits are given based on the amount of players you kill compared to other players.";
    public override Texture Logo => MenuResources.GetGamemodeIcon(Title);

    public override bool DisableDevTools => true;
    public override bool DisableSpawnGun => true;
    public override bool DisableManualUnragdoll => true;

    public TriggerEvent OneMinuteLeftTrigger { get; set; }
    public TriggerEvent NaturalEndTrigger { get; set; }

    public MetadataFloat Vitality { get; set; }

    private readonly PlayerScoreKeeper _scoreKeeper = new();
    public PlayerScoreKeeper ScoreKeeper => _scoreKeeper;

    private readonly MusicPlaylist _playlist = new();
    public MusicPlaylist Playlist => _playlist;

    private bool _hasDied;

    private float _timeOfStart;
    private bool _oneMinuteLeft;

    private int _savedMinutes = _defaultMinutes;
    private int _totalMinutes = _defaultMinutes;

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

    private float _savedVitality = 1f;

    private string _avatarOverride = null;

    private MonoDiscReference _victorySongReference = null;
    private MonoDiscReference _failureSongReference = null;

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

    public void ApplyGamemodeSettings()
    {
        _totalMinutes = _savedMinutes;

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
        float playerPercent = (float)playerCount / 3f;
        int maxBits = ManagedMathf.FloorToInt(ManagedMathf.Lerp(_minPlayerBits, _maxPlayerBits, playerPercent));
        int maxRand = maxBits / 10;

        // Get the scores
        int score = ScoreKeeper.GetScore(PlayerIDManager.LocalID);
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

    public override void OnGamemodeRegistered()
    {
        // Add hooks
        MultiplayerHooking.OnPlayerAction += OnPlayerAction;
        FusionOverrides.OnValidateNametag += OnValidateNametag;

        // Create triggers
        OneMinuteLeftTrigger = new TriggerEvent(nameof(OneMinuteLeftTrigger), Relay, true);
        OneMinuteLeftTrigger.OnTriggered += OnOneMinuteLeft;

        NaturalEndTrigger = new TriggerEvent(nameof(NaturalEndTrigger), Relay, true);
        NaturalEndTrigger.OnTriggered += OnNaturalEnd;

        // Create metadata
        Vitality = new MetadataFloat(nameof(Vitality), Metadata);

        Metadata.OnMetadataChanged += OnMetadataChanged;

        // Register score keeper
        ScoreKeeper.Register(Metadata);
        ScoreKeeper.OnPlayerScoreChanged += OnScoreChanged;
    }

    public override void OnGamemodeUnregistered()
    {
        // Remove hooks
        MultiplayerHooking.OnPlayerAction -= OnPlayerAction;
        FusionOverrides.OnValidateNametag -= OnValidateNametag;

        Metadata.OnMetadataChanged -= OnMetadataChanged;

        // Destroy triggers
        OneMinuteLeftTrigger.UnregisterEvent();
        NaturalEndTrigger.UnregisterEvent();

        // Unregister score keeper
        ScoreKeeper.Unregister();
        ScoreKeeper.OnPlayerScoreChanged -= OnScoreChanged;
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

    protected bool OnValidateNametag(PlayerID id)
    {
        if (!IsStarted)
            return true;

        return false;
    }

    protected void OnPlayerAction(PlayerID player, PlayerActionType type, PlayerID otherPlayer = null)
    {
        if (!IsStarted)
        {
            return;
        }

        switch (type)
        {
            case PlayerActionType.DEATH:
                // If we died, we can't get the Rampage achievement
                if (player.IsMe)
                {
                    _hasDied = true;
                }
                break;
            case PlayerActionType.DYING_BY_OTHER_PLAYER:
                if (otherPlayer != null && otherPlayer != player)
                {
                    // Increment score for that player
                    if (NetworkInfo.IsHost)
                    {
                        ScoreKeeper.AddScore(otherPlayer);
                    }

                    // If we are the killer, increment our achievement
                    if (otherPlayer.IsMe)
                    {
                        AchievementManager.IncrementAchievements<KillerAchievement>();
                    }
                }
                break;
        }
    }

    public override bool CheckReadyConditions()
    {
        return PlayerIDManager.PlayerCount >= MinimumPlayers;
    }

    public override void OnGamemodeStarted()
    {
        base.OnGamemodeStarted();

        if (NetworkInfo.IsHost)
        {
            ScoreKeeper.ResetScores();
        }

        Notifier.Send(new Notification()
        {
            Title = "Deathmatch Started",
            Message = "Good luck!",
            SaveToMenu = false,
            ShowPopup = true,
        });

        // Reset time
        _timeOfStart = TimeUtilities.TimeSinceStartup;
        _oneMinuteLeft = false;

        // Reset death status
        _hasDied = false;

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

        Playlist.StartPlaylist();

        // Setup ammo
        LocalInventory.SetAmmo(10000);

        // Get all spawn points
        GamemodeHelper.SetSpawnPoints(GamemodeMarker.FilterMarkers(null));
        GamemodeHelper.TeleportToSpawnPoint();

        // Push nametag updates
        FusionOverrides.ForceUpdateOverrides();
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

    protected void OnVictoryStatus(bool isVictory = false)
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

    public override void OnGamemodeStopped()
    {
        base.OnGamemodeStopped();

        Playlist.StopPlaylist();

        // Get the winner message
        var firstPlace = ScoreKeeper.GetPlayerByPlace(0);
        var secondPlace = ScoreKeeper.GetPlayerByPlace(1);
        var thirdPlace = ScoreKeeper.GetPlayerByPlace(2);

        var selfPlace = ScoreKeeper.GetPlace(PlayerIDManager.LocalID);
        var selfScore = ScoreKeeper.GetScore(PlayerIDManager.LocalID);

        string message = "No one scored points!";

        if (firstPlace != null && firstPlace.TryGetDisplayName(out var name))
        {
            message = $"First Place: {name} (Score: {ScoreKeeper.GetScore(firstPlace)}) \n";
        }

        if (secondPlace != null && secondPlace.TryGetDisplayName(out name))
        {
            message += $"Second Place: {name} (Score: {ScoreKeeper.GetScore(secondPlace)}) \n";
        }

        if (thirdPlace != null && thirdPlace.TryGetDisplayName(out name))
        {
            message += $"Third Place: {name} (Score: {ScoreKeeper.GetScore(thirdPlace)}) \n";
        }

        if (selfPlace != -1 && selfPlace > 3)
        {
            message += $"Your Place: {selfPlace} (Score: {selfScore})";
        }

        // Play victory/failure sounds
        int playerCount = PlayerIDManager.PlayerCount;

        if (playerCount > 1)
        {
            bool isVictory = false;

            if (selfPlace < Math.Min(playerCount, 3))
                isVictory = true;

            OnVictoryStatus(isVictory);

            // If we are first place and haven't died, give Rampage achievement
            if (selfPlace == 1 && !_hasDied)
            {
                if (AchievementManager.TryGetAchievement<Rampage>(out var achievement))
                    achievement.IncrementTask();
            }
        }

        // Show the winners in a notification
        Notifier.Send(new Notification()
        {
            Title = "Deathmatch Completed",

            Message = message,

            PopupLength = 6f,

            SaveToMenu = false,
            ShowPopup = true,
        });

        _timeOfStart = 0f;
        _oneMinuteLeft = false;

        // Reset mortality
        LocalHealth.MortalityOverride = null;

        // Remove spawn points
        FusionPlayer.ResetSpawnPoints();

        // Push nametag updates
        FusionOverrides.ForceUpdateOverrides();

        // Reset overrides
        LocalAvatar.AvatarOverride = null;
        LocalHealth.VitalityOverride = null;
        LocalControls.DisableSlowMo = false;
    }

    public float GetTimeElapsed() => TimeUtilities.TimeSinceStartup - _timeOfStart;
    public float GetMinutesLeft()
    {
        float elapsed = GetTimeElapsed();
        return _totalMinutes - (elapsed / 60f);
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

        // Make sure we are a server
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
        Notifier.Send(new Notification()
        {
            Title = "Deathmatch Timer",
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

    private void OnScoreChanged(PlayerID player, int score)
    {
        if (player.IsMe && score != 0)
        {
            Notifier.Send(new Notification()
            {
                Title = "Deathmatch Point",
                Message = $"New score is {score}!",
                SaveToMenu = false,
                ShowPopup = true,
                PopupLength = 0.7f,
            });
        }
    }
}