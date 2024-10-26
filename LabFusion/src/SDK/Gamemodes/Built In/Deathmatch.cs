using Il2CppSLZ.Marrow.Audio;
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
using LabFusion.Scene;
using LabFusion.SDK.Triggers;

using UnityEngine;

using BoneLib.BoneMenu;

namespace LabFusion.SDK.Gamemodes;

public class Deathmatch : Gamemode
{
    private const int _minPlayerBits = 30;
    private const int _maxPlayerBits = 250;

    public static Deathmatch Instance { get; private set; }

    private const int _defaultMinutes = 3;
    private const int _minMinutes = 2;
    private const int _maxMinutes = 60;

    // Default metadata keys
    public override string GamemodeCategory => "Fusion";
    public override string GamemodeName => "Deathmatch";

    public override bool DisableDevTools => true;
    public override bool DisableSpawnGun => true;
    public override bool DisableManualUnragdoll => true;

    public override bool PreventNewJoins => !_enabledLateJoining;

    public TriggerEvent OneMinuteLeftTrigger { get; set; }
    public TriggerEvent NaturalEndTrigger { get; set; }

    private readonly PlayerScoreKeeper _scoreKeeper = new();
    public PlayerScoreKeeper ScoreKeeper => _scoreKeeper;

    private readonly MusicPlaylist _playlist = new();
    public MusicPlaylist Playlist => _playlist;

    private bool _hasDied;

    private float _timeOfStart;
    private bool _oneMinuteLeft;

    private int _savedMinutes = _defaultMinutes;
    private int _totalMinutes = _defaultMinutes;

    private bool _hasOverridenValues = false;

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

        AudioReference[] playlist = AudioReference.CreateReferences(FusionMonoDiscReferences.CombatSongReferences);
        Playlist.SetPlaylist(playlist);

        _avatarOverride = null;
        _vitalityOverride = null;

        _enabledLateJoining = true;
    }

    public void SetOverriden()
    {
        if (FusionSceneManager.IsLoading())
        {
            if (!_hasOverridenValues)
                SetDefaultValues();

            _hasOverridenValues = true;
        }
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
            FusionPlayer.SetAvatarOverride(barcode);
    }

    public void SetPlayerVitality(float vitality)
    {
        _vitalityOverride = vitality;

        if (IsActive())
            FusionPlayer.SetPlayerVitality(vitality);
    }

    public IReadOnlyList<PlayerId> GetPlayersByScore()
    {
        List<PlayerId> leaders = new(PlayerIdManager.PlayerIds);
        leaders = leaders.OrderBy(id => ScoreKeeper.GetScore(id)).ToList();
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

    private int GetRewardedBits()
    {
        // Change the max bit count based on player count
        int playerCount = PlayerIdManager.PlayerCount - 1;

        // 10 and 100 are the min and max values for the max bit count
        float playerPercent = (float)playerCount / 3f;
        int maxBits = ManagedMathf.FloorToInt(ManagedMathf.Lerp(_minPlayerBits, _maxPlayerBits, playerPercent));
        int maxRand = maxBits / 10;

        // Get the scores
        int score = ScoreKeeper.GetScore(PlayerIdManager.LocalId);
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
        Instance = this;

        // Add hooks
        MultiplayerHooking.OnPlayerAction += OnPlayerAction;
        FusionOverrides.OnValidateNametag += OnValidateNametag;

        // Create triggers
        OneMinuteLeftTrigger = new TriggerEvent(nameof(OneMinuteLeftTrigger), Relay, true);
        OneMinuteLeftTrigger.OnTriggered += OnOneMinuteLeft;

        NaturalEndTrigger = new TriggerEvent(nameof(NaturalEndTrigger), Relay, true);
        NaturalEndTrigger.OnTriggered += OnNaturalEnd;

        // Register score keeper
        ScoreKeeper.Register(Metadata);
        ScoreKeeper.OnScoreChanged += OnScoreChanged;

        SetDefaultValues();
    }

    public override void OnGamemodeUnregistered()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        // Remove hooks
        MultiplayerHooking.OnPlayerAction -= OnPlayerAction;
        FusionOverrides.OnValidateNametag -= OnValidateNametag;

        // Destroy triggers
        OneMinuteLeftTrigger.UnregisterEvent();
        NaturalEndTrigger.UnregisterEvent();

        // Unregister score keeper
        ScoreKeeper.Unregister();
        ScoreKeeper.OnScoreChanged -= OnScoreChanged;
    }

    protected bool OnValidateNametag(PlayerId id)
    {
        if (!IsActive())
            return true;

        return false;
    }

    protected void OnPlayerAction(PlayerId player, PlayerActionType type, PlayerId otherPlayer = null)
    {
        if (!IsActive())
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
            case PlayerActionType.DEATH_BY_OTHER_PLAYER:
                if (otherPlayer != null && otherPlayer != player)
                {
                    // Increment score for that player
                    if (NetworkInfo.IsServer)
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

    protected override void OnStartGamemode()
    {
        base.OnStartGamemode();

        Playlist.StartPlaylist();

        if (NetworkInfo.IsServer)
        {
            ScoreKeeper.ResetScores();
        }

        FusionNotifier.Send(new FusionNotification()
        {
            title = "Deathmatch Started",
            showTitleOnPopup = true,
            message = "Good luck!",
            isMenuItem = false,
            isPopup = true,
        });

        // Reset time
        _timeOfStart = TimeUtilities.TimeSinceStartup;
        _oneMinuteLeft = false;

        // Reset death status
        _hasDied = false;

        // Invoke player changes on level load
        FusionSceneManager.HookOnTargetLevelLoad(() =>
        {
            // Force mortality
            FusionPlayer.SetMortality(true);

            // Setup ammo
            FusionPlayer.SetAmmo(1000);

            // Get all spawn points
            var transforms = new List<Transform>();
            var markers = GamemodeMarker.FilterMarkers(null);

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

            // Push nametag updates
            FusionOverrides.ForceUpdateOverrides();

            // Apply vitality and avatar overrides
            if (_avatarOverride != null)
                FusionPlayer.SetAvatarOverride(_avatarOverride);

            if (_vitalityOverride.HasValue)
                FusionPlayer.SetPlayerVitality(_vitalityOverride.Value);
        });
    }

    protected static void OnVictoryStatus(bool isVictory = false)
    {
        MonoDiscReference stingerReference;
        if (isVictory)
        {
            stingerReference = FusionMonoDiscReferences.LavaGangVictoryReference;
        }
        else
        {
            stingerReference = FusionMonoDiscReferences.LavaGangFailureReference;
        }

        var dataCard = stingerReference.DataCard;
        if (dataCard == null)
        {
            return;
        }

        dataCard.AudioClip.LoadAsset((Il2CppSystem.Action<AudioClip>)((c) => {
            Audio3dManager.Play2dOneShot(c, Audio3dManager.nonDiegeticMusic, new Il2CppSystem.Nullable<float>(Audio3dManager.audio_MusicVolume), new Il2CppSystem.Nullable<float>(1f));
        }));
    }

    protected override void OnStopGamemode()
    {
        base.OnStopGamemode();

        Playlist.StopPlaylist();

        // Get the winner message
        var firstPlace = GetByScore(0);
        var secondPlace = GetByScore(1);
        var thirdPlace = GetByScore(2);

        var selfPlace = GetPlace(PlayerIdManager.LocalId);
        var selfScore = ScoreKeeper.GetScore(PlayerIdManager.LocalId);

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
        int playerCount = PlayerIdManager.PlayerCount;

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

    public float GetTimeElapsed() => TimeUtilities.TimeSinceStartup - _timeOfStart;
    public float GetMinutesLeft()
    {
        float elapsed = GetTimeElapsed();
        return _totalMinutes - (elapsed / 60f);
    }

    protected override void OnUpdate()
    {
        // Also make sure the gamemode is active
        if (!IsActive())
        {
            return;
        }

        // Update music
        Playlist.Update();

        // Make sure we are a server
        if (!NetworkInfo.IsServer)
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
            StopGamemode();
            NaturalEndTrigger.TryInvoke();
        }
    }

    private void OnOneMinuteLeft()
    {
        FusionNotifier.Send(new FusionNotification()
        {
            title = "Deathmatch Timer",
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

    private void OnScoreChanged(PlayerId player, int score)
    {
        if (player.IsMe && score != 0)
        {
            FusionNotifier.Send(new FusionNotification()
            {
                title = "Deathmatch Point",
                showTitleOnPopup = true,
                message = $"New score is {score}!",
                isMenuItem = false,
                isPopup = true,
                popupLength = 0.7f,
            });
        }
    }
}