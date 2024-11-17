using BoneLib.BoneMenu;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.Menu;
using LabFusion.Menu.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.SDK.Triggers;
using LabFusion.Utilities;

using MelonLoader;

using System.Collections;

using UnityEngine;
using UnityEngine.UI;

namespace LabFusion.SDK.Gamemodes;

public class HideAndSeek : Gamemode
{
    public override string Title => "Hide And Seek";

    public override string Author => FusionMod.ModAuthor;

    public override Texture Logo => MenuResources.GetGamemodeIcon(Title);

    public static class Defaults
    {
        public const int SeekerCount = 1;

        public static readonly MonoDiscReference[] Tracks = new MonoDiscReference[]
        {
            BONELABMonoDiscReferences.TheRecurringDreamReference,
            BONELABMonoDiscReferences.HeavyStepsReference,
            BONELABMonoDiscReferences.StankFaceReference,
            BONELABMonoDiscReferences.AlexInWonderlandReference,
            BONELABMonoDiscReferences.ItDoBeGroovinReference,

            BONELABMonoDiscReferences.ConcreteCryptReference, // concrete crypt
        };
    }

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

    public TriggerEvent SeekerVictoryEvent { get; set; }

    private bool _hasBeenTagged = false;
    private bool _assignedDefaultTeam = false;

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

        SeekerVictoryEvent = new TriggerEvent("SeekerVictory", Relay, true);
        SeekerVictoryEvent.OnTriggered += OnSeekerVictory;
    }

    public override void OnGamemodeUnregistered()
    {
        FusionOverrides.OnValidateNametag -= OnValidateNametag;

        TeamManager.Unregister();

        TagEvent.UnregisterEvent();
        TagEvent = null;

        SeekerVictoryEvent.UnregisterEvent();
        SeekerVictoryEvent = null;
    }

    protected bool OnValidateNametag(PlayerId id)
    {
        if (!IsStarted)
        {
            return true;
        }

        return TeamManager.GetPlayerTeam(id) == TeamManager.GetLocalTeam();
    }

    private void OnTagTriggered(string value)
    {
        if (!ulong.TryParse(value, out var userId))
        {
            FusionLogger.Warn($"Player Tag was triggered, but the value {value} is not a userId!");
            return;
        }

        var playerId = PlayerIdManager.GetPlayerId(userId);

        if (playerId == null)
        {
            return;
        }

        if (NetworkInfo.IsServer && HiderTeam.HasPlayer(playerId))
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
            FusionNotifier.Send(new FusionNotification()
            {
                isPopup = true,
                Title = "Tagged",
                Message = "You've been tagged! You are now a seeker!",
                Length = 4f,
                type = NotificationType.INFORMATION,
            });

            _hasBeenTagged = true;
        }

        // Other player tagged?
        if (!playerId.IsMe)
        {
            playerId.TryGetDisplayName(out var name);

            FusionNotifier.Send(new FusionNotification()
            {
                isPopup = true,
                Title = $"{name} Tagged",
                Message = $"{name} has been tagged and is now a seeker!",
                Length = 4f,
                type = NotificationType.INFORMATION,
            });
        }
    }

    public void OnSeekerVictory()
    {
        FusionNotifier.Send(new FusionNotification()
        {
            isPopup = true,
            Title = "Seekers Won",
            Message = "All hiders have been found!",
            Length = 4f,
            type = NotificationType.INFORMATION,
        });
    }

    private void OnAssignedToTeam(PlayerId player, Team team)
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
            FusionNotifier.Send(new FusionNotification()
            {
                isPopup = true,
                Title = "Hider",
                Message = "You are a hider! Don't let the seekers grab you!",
                Length = 4,
                type = NotificationType.INFORMATION,
            });
        }

        if (team == SeekerTeam)
        {
            FusionNotifier.Send(new FusionNotification()
            {
                isPopup = true,
                Title = "Seeker",
                Message = "You are a seeker! Grab all hiders to win!",
                Length = 2f,
                type = NotificationType.INFORMATION,
            });

            MelonCoroutines.Start(HideVisionAndReveal());
        }

        _assignedDefaultTeam = true;
    }

    private static void TeleportToHost()
    {
        if (NetworkInfo.IsServer)
        {
            return;
        }

        var host = PlayerIdManager.GetHostId();

        if (!NetworkPlayerManager.TryGetPlayer(host.SmallId, out var player))
        {
            return;
        }

        if (player.HasRig)
        {
            var feetPosition = player.RigRefs.RigManager.physicsRig.feet.transform.position;

            FusionPlayer.Teleport(feetPosition, Vector3.forward, true);
        }
    }

    private static IEnumerator HideVisionAndReveal()
    {
        // Move to LocalVision later
        var canvasGameObject = new GameObject("VISION OBSTRUCTION");
        var canvas = canvasGameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.WorldSpace;
        var canvasTransform = canvasGameObject.transform;
        canvasTransform.parent = RigData.Refs.Headset;
        canvasTransform.localPosition = Vector3Extensions.forward * 0.1f;
        canvasTransform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        canvasTransform.localScale = Vector3Extensions.one * 10f;

        var image = canvas.gameObject.AddComponent<RawImage>();
        image.color = Color.black;

        // Lock movement so we can't move while vision is dark
        LocalControls.LockMovement();

        float notificationWait = 0f;

        while (notificationWait < 4f)
        {
            notificationWait += TimeUtilities.DeltaTime;
            yield return null;
        }

        float elapsed = 0f;
        int seconds = 0;
        int maxSeconds = 30;

        while (seconds < maxSeconds)
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
                    FusionNotifier.Send(new FusionNotification()
                    {
                        isPopup = true,
                        Title = "Countdown",
                        Message = $"{remainingSeconds}",
                        Length = 1f,
                        type = NotificationType.INFORMATION,
                    });
                    break;
            }

            while (elapsed < 1f)
            {
                elapsed += TimeUtilities.DeltaTime;
                yield return null;
            }

            seconds++;
            elapsed -= 1f;
        }

        LocalControls.UnlockMovement();

        FusionNotifier.Send(new FusionNotification()
        {
            isPopup = true,
            Title = "Countdown Over",
            Message = $"GO!",
            Length = 0.5f,
            type = NotificationType.INFORMATION,
        });

        GameObject.Destroy(canvasGameObject);
    }

    private void SetDefaults()
    {
        Playlist.SetPlaylist(AudioReference.CreateReferences(Defaults.Tracks));
        Playlist.Shuffle();
    }

    public override void OnGamemodeStarted()
    {
        base.OnGamemodeStarted();

        _hasBeenTagged = false;

        SetDefaults();

        Playlist.StartPlaylist();

        LocalPlayer.OnGrab += OnLocalPlayerGrab;

        if (NetworkInfo.IsServer)
        {
            AssignTeams();
        }

        TeleportToHost();

        // Update nametags
        FusionOverrides.ForceUpdateOverrides();
    }

    public override void OnGamemodeStopped()
    {
        base.OnGamemodeStopped();

        _hasBeenTagged = false;
        _assignedDefaultTeam = false;

        Playlist.StopPlaylist();

        LocalPlayer.OnGrab -= OnLocalPlayerGrab;

        if (NetworkInfo.IsServer)
        {
            ClearTeams();
        }

        // Update nametags
        FusionOverrides.ForceUpdateOverrides();
    }

    protected override void OnUpdate()
    {
        if (!IsStarted)
        {
            return;
        }

        Playlist.Update();
    }

    private void AssignTeams()
    {
        // Shuffle the players for randomness
        var players = new List<PlayerId>(PlayerIdManager.PlayerIds);
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
        if (!SeekerTeam.HasPlayer(PlayerIdManager.LocalId))
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
        if (HiderTeam.HasPlayer(player.PlayerId))
        {
            TagEvent.TryInvoke(player.PlayerId.LongId.ToString());
        }
    }
}
