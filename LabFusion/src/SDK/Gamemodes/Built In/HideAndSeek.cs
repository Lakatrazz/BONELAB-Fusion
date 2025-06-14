using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Bonelab;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.Menu;
using LabFusion.Menu.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.SDK.Points;
using LabFusion.SDK.Triggers;
using LabFusion.Utilities;
using LabFusion.UI.Popups;

using MelonLoader;

using System.Collections;

using UnityEngine;

namespace LabFusion.SDK.Gamemodes;

public class HideAndSeek : Gamemode
{
    public override string Title => "Hide And Seek";

    public override string Author => FusionMod.ModAuthor;

    public override string Description =>
        "Seekers are picked at random, while the rest of the players must hide! " +
        "After a 30 second countdown, the seekers must grab other players to turn them into seekers! " +
        "Once everyone becomes a seeker, the seekers win; however, if the time runs out before then, the hiders win!";

    public override Texture Logo => MenuResources.GetGamemodeIcon(Title);

    public static class Defaults
    {
        public const int SeekerCount = 1;

        public const int SeekerBitReward = 50;

        public const int HiderBitReward = 20;

        public const int TimeLimit = 10;

        public static readonly MonoDiscReference[] Tracks = new MonoDiscReference[]
        {
            BonelabMonoDiscReferences.TheRecurringDreamReference,
            BonelabMonoDiscReferences.HeavyStepsReference,
            BonelabMonoDiscReferences.StankFaceReference,
            BonelabMonoDiscReferences.AlexInWonderlandReference,
            BonelabMonoDiscReferences.ItDoBeGroovinReference,

            BonelabMonoDiscReferences.ConcreteCryptReference, // concrete crypt
        };
    }

    public int TimeLimit { get; set; } = Defaults.TimeLimit;

    public int SeekerCount { get; set; } = Defaults.SeekerCount;

    private readonly MusicPlaylist _playlist = new();
    public MusicPlaylist Playlist => _playlist;

    private readonly TeamManager _teamManager = new();
    public TeamManager TeamManager => _teamManager;

    private readonly Team _seekerTeam = new("Seekers");
    public Team SeekerTeam => _seekerTeam;

    private readonly Team _hiderTeam = new("Hiders");
    public Team HiderTeam => _hiderTeam;

    public TriggerEvent TagEvent { get; set; }
    public TriggerEvent OneMinuteLeftEvent { get; set; }
    public TriggerEvent SeekerVictoryEvent { get; set; }
    public TriggerEvent HiderVictoryEvent { get; set; }

    private bool _hasBeenTagged = false;
    private bool _assignedDefaultTeam = false;

    private readonly HashSet<ulong> _tagRewards = new();

    private float _elapsedTime = 0f;

    public float ElapsedSeconds => _elapsedTime;
    public int ElapsedMinutes => Mathf.FloorToInt(ElapsedSeconds / 60f);

    public override bool DisableDevTools => true;
    public override bool DisableSpawnGun => true;

    public override GroupElementData CreateSettingsGroup()
    {
        var group = base.CreateSettingsGroup();

        var generalGroup = new GroupElementData("General");

        group.AddElement(generalGroup);

        var seekerCountData = new IntElementData()
        {
            Title = "Seeker Count",
            Value = SeekerCount,
            Increment = 1,
            MinValue = 1,
            MaxValue = 8,
            OnValueChanged = (v) =>
            {
                SeekerCount = v;
            },
        };

        generalGroup.AddElement(seekerCountData);

        var timeLimitData = new IntElementData()
        {
            Title = "Time Limit",
            Value = TimeLimit,
            Increment = 1,
            MinValue = 1,
            MaxValue = 60,
            OnValueChanged = (v) =>
            {
                TimeLimit = v;
            },
        };

        generalGroup.AddElement(timeLimitData);

        return group;
    }

    public override void OnGamemodeRegistered()
    {
        FusionOverrides.OnValidateNametag += OnValidateNametag;

        TeamManager.Register(this);
        TeamManager.AddTeam(SeekerTeam);
        TeamManager.AddTeam(HiderTeam);

        TeamManager.OnAssignedToTeam += OnAssignedToTeam;

        TagEvent = new TriggerEvent("TagPlayer", Relay, false);
        TagEvent.OnTriggeredWithValue += OnTagTriggered;

        OneMinuteLeftEvent = new TriggerEvent("OneMinuteLeft", Relay, true);
        OneMinuteLeftEvent.OnTriggered += OnOneMinuteLeft;

        SeekerVictoryEvent = new TriggerEvent("SeekerVictory", Relay, true);
        SeekerVictoryEvent.OnTriggered += OnSeekerVictory;

        HiderVictoryEvent = new TriggerEvent("HiderVictory", Relay, true);
        HiderVictoryEvent.OnTriggered += OnHiderVictory;
    }

    public override void OnGamemodeUnregistered()
    {
        FusionOverrides.OnValidateNametag -= OnValidateNametag;

        TeamManager.Unregister();

        TeamManager.OnAssignedToTeam -= OnAssignedToTeam;

        TagEvent.UnregisterEvent();
        TagEvent = null;

        OneMinuteLeftEvent.UnregisterEvent();
        OneMinuteLeftEvent = null;

        SeekerVictoryEvent.UnregisterEvent();
        SeekerVictoryEvent = null;

        HiderVictoryEvent.UnregisterEvent();
        HiderVictoryEvent = null;
    }

    protected bool OnValidateNametag(PlayerID id)
    {
        if (!IsStarted)
        {
            return true;
        }

        return TeamManager.IsTeammate(id);
    }

    private void OnTagTriggered(string value)
    {
        if (!ulong.TryParse(value, out var userId))
        {
            FusionLogger.Warn($"Player Tag was triggered, but the value {value} is not a userId!");
            return;
        }

        var playerId = PlayerIDManager.GetPlayerID(userId);

        if (playerId == null)
        {
            return;
        }

        if (NetworkInfo.IsHost && HiderTeam.HasPlayer(playerId))
        {
            // If this was the last player, end the game
            if (HiderTeam.PlayerCount <= 1)
            {
                SeekerVictoryEvent.TryInvoke();
                GamemodeManager.StopGamemode();
            }
            else
            {
                TeamManager.TryAssignTeam(playerId, SeekerTeam);
            }
        }

        // Were we tagged? Give a notification
        if (playerId.IsMe && !_hasBeenTagged)
        {
            Notifier.Send(new Notification()
            {
                ShowPopup = true,
                Title = "Tagged",
                Message = "You've been tagged! You are now a seeker!",
                PopupLength = 4f,
                Type = NotificationType.INFORMATION,
            });

            _hasBeenTagged = true;
        }

        // Other player tagged?
        if (!playerId.IsMe)
        {
            playerId.TryGetDisplayName(out var name);

            Notifier.Send(new Notification()
            {
                ShowPopup = true,
                Title = $"{name} Tagged",
                Message = $"{name} has been tagged and is now a seeker!",
                PopupLength = 4f,
                Type = NotificationType.INFORMATION,
            });

            // Check bit reward
            if (_tagRewards.Remove(playerId.PlatformID))
            {
                PointItemManager.RewardBits(Defaults.SeekerBitReward);
            }
        }
    }

    private void OnOneMinuteLeft()
    {
        Notifier.Send(new Notification()
        {
            Title = "Hide And Seek Timer",
            Message = "One minute left!",
            SaveToMenu = false,
            ShowPopup = true,
        });
    }

    private void OnSeekerVictory()
    {
        Notifier.Send(new Notification()
        {
            ShowPopup = true,
            Title = "Seekers Won",
            Message = "All hiders have been found!",
            PopupLength = 4f,
            Type = NotificationType.INFORMATION,
        });
    }

    private void OnHiderVictory()
    {
        Notifier.Send(new Notification()
        {
            ShowPopup = true,
            Title = "Hiders Won",
            Message = "The hiders weren't found in time!",
            PopupLength = 4f,
            Type = NotificationType.INFORMATION,
        });

        // We are a hider and won!
        if (TeamManager.GetLocalTeam() == HiderTeam)
        {
            var seekerCount = SeekerTeam.PlayerCount;

            var bitReward = Defaults.HiderBitReward * seekerCount;

            PointItemManager.RewardBits(bitReward);
        }
    }

    private void OnAssignedToTeam(PlayerID player, Team team)
    {
        // Update nametags
        FusionOverrides.ForceUpdateOverrides();

        if (!player.IsMe)
        {
            return;
        }

        if (_assignedDefaultTeam)
        {
            return;
        }

        if (team == HiderTeam)
        {
            Notifier.Send(new Notification()
            {
                ShowPopup = true,
                Title = "Hider",
                Message = "You are a hider! Don't let the seekers grab you!",
                PopupLength = 4,
                Type = NotificationType.INFORMATION,
            });
        }

        if (team == SeekerTeam)
        {
            Notifier.Send(new Notification()
            {
                ShowPopup = true,
                Title = "Seeker",
                Message = "You are a seeker! Grab all hiders to win!",
                PopupLength = 2f,
                Type = NotificationType.INFORMATION,
            });

            MelonCoroutines.Start(HideVisionAndReveal());
        }

        _assignedDefaultTeam = true;
    }

    private static void TeleportToHost()
    {
        if (NetworkInfo.IsHost)
        {
            return;
        }

        var host = PlayerIDManager.GetHostID();

        if (!NetworkPlayerManager.TryGetPlayer(host.SmallID, out var player))
        {
            return;
        }

        if (player.HasRig)
        {
            var feetPosition = player.RigRefs.RigManager.physicsRig.feet.transform.position;

            LocalPlayer.TeleportToPosition(feetPosition, Vector3.forward);
        }
    }

    private static IEnumerator HideVisionAndReveal()
    {
        LocalVision.Blind = true;
        LocalVision.BlindColor = Color.black;

        // Lock movement so we can't move while vision is dark
        LocalControls.LockedMovement = true;

        float notificationWait = 0f;

        while (notificationWait < 4f)
        {
            notificationWait += TimeUtilities.DeltaTime;
            yield return null;
        }

        float fadeLength = 1f;

        float elapsed = 0f;
        float totalElapsed = 0f;

        int seconds = 0;
        int maxSeconds = 30;

        bool secondPassed = true;

        while (seconds < maxSeconds)
        {
            // Calculate fade-in
            float fadeStart = Mathf.Max(maxSeconds - fadeLength, 0f);
            float fadeProgress = Mathf.Max(totalElapsed - fadeStart, 0f) / fadeLength;

            LocalVision.BlindColor = Color.Lerp(Color.black, Color.clear, fadeProgress);

            // Check for second counter
            if (secondPassed)
            {
                int remainingSeconds = maxSeconds - seconds;
                switch (remainingSeconds)
                {
                    case 30:
                    case 25:
                    case 20:
                    case 15:
                    case 10:
                    case 5:
                    case 4:
                    case 3:
                    case 2:
                    case 1:
                        Notifier.Send(new Notification()
                        {
                            ShowPopup = true,
                            Title = "Countdown",
                            Message = $"{remainingSeconds}",
                            PopupLength = 1f,
                            Type = NotificationType.INFORMATION,
                        });
                        break;
                }

                secondPassed = false;
            }

            // Tick timer
            elapsed += TimeUtilities.DeltaTime;
            totalElapsed += TimeUtilities.DeltaTime;

            // If a second passed, send the notification next frame
            if (elapsed >= 1f)
            {
                elapsed -= 1f;
                seconds++;

                secondPassed = true;
            }

            yield return null;
        }

        LocalControls.LockedMovement = false;

        LocalVision.Blind = false;

        Notifier.Send(new Notification()
        {
            ShowPopup = true,
            Title = "Countdown Over",
            Message = $"GO!",
            PopupLength = 0.5f,
            Type = NotificationType.INFORMATION,
        });
    }

    private void SetDefaults()
    {
        Playlist.SetPlaylist(AudioReference.CreateReferences(Defaults.Tracks));
        Playlist.Shuffle();
    }

    public override void OnGamemodeStarted()
    {
        base.OnGamemodeStarted();

        _tagRewards.Clear();

        _hasBeenTagged = false;

        SetDefaults();

        Playlist.StartPlaylist();

        LocalPlayer.OnGrab += OnLocalPlayerGrab;
        LocalControls.DisableSlowMo = true;

        if (NetworkInfo.IsHost)
        {
            AssignTeams();
        }

        TeleportToHost();

        // Update nametags
        FusionOverrides.ForceUpdateOverrides();

        _elapsedTime = 0f;
        _lastCheckedMinutes = 0;
        _oneMinuteLeft = false;
    }

    public override void OnGamemodeStopped()
    {
        base.OnGamemodeStopped();

        _hasBeenTagged = false;
        _assignedDefaultTeam = false;

        Playlist.StopPlaylist();

        LocalPlayer.OnGrab -= OnLocalPlayerGrab;
        LocalControls.DisableSlowMo = false;

        if (NetworkInfo.IsHost)
        {
            ClearTeams();
        }

        // Update nametags
        FusionOverrides.ForceUpdateOverrides();

        _elapsedTime = 0f;
        _lastCheckedMinutes = 0;
        _oneMinuteLeft = false;
    }

    private bool _oneMinuteLeft = false;
    protected override void OnUpdate()
    {
        if (!IsStarted)
        {
            return;
        }

        _elapsedTime += TimeUtilities.DeltaTime;

        Playlist.Update();

        if (TeamManager.GetLocalTeam() == HiderTeam)
        {
            OnHiderUpdate();
        }

        // Check for one minute left
        if (NetworkInfo.IsHost && !_oneMinuteLeft && (TimeLimit - ElapsedMinutes) == 1)
        {
            OneMinuteLeftEvent.TryInvoke();
            _oneMinuteLeft = true;
        }

        // Check for time limit
        if (ElapsedMinutes >= TimeLimit)
        {
            HiderVictoryEvent?.TryInvoke();
            GamemodeManager.StopGamemode();
        }
    }

    private int _lastCheckedMinutes = 0;
    private void OnHiderUpdate()
    {
        if (_lastCheckedMinutes != ElapsedMinutes)
        {
            _lastCheckedMinutes = ElapsedMinutes;

            PointItemManager.RewardBits(Defaults.HiderBitReward);
        }
    }

    private void AssignTeams()
    {
        // Shuffle the players for randomness
        var players = new List<PlayerID>(PlayerIDManager.PlayerIDs);
        players.Shuffle();

        // Assign seekers
        for (var i = 0; i < SeekerCount && i < players.Count; i++)
        {
            var player = players[i];
            TeamManager.TryAssignTeam(player, SeekerTeam);

            // Remove the player from the list
            players.Remove(player);
        }

        // Assign the rest as hiders
        foreach (var player in players)
        {
            TeamManager.TryAssignTeam(player, HiderTeam);
        }
    }

    private void ClearTeams()
    {
        TeamManager.UnassignAllPlayers();
    }


    private void OnLocalPlayerGrab(Hand hand, Grip grip)
    {
        // If we aren't a seeker, ignore
        if (!SeekerTeam.HasPlayer(PlayerIDManager.LocalID))
        {
            return;
        }
        
        // Check if the grabbed object is a player
        if (!grip._marrowEntity)
        {
            return;
        }

        if (!NetworkPlayerManager.TryGetPlayer(grip._marrowEntity, out var player))
        {
            return;
        }

        // Check if they're a hider, and if they are, tag them
        if (HiderTeam.HasPlayer(player.PlayerID))
        {
            var longId = player.PlayerID.PlatformID;

            _tagRewards.Add(longId);

            TagEvent.TryInvoke(longId.ToString());
        }
    }
}
