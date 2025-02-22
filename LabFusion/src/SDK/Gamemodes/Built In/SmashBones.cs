using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Combat;

using LabFusion.Marrow.Integration;
using LabFusion.Marrow;
using LabFusion.Menu;
using LabFusion.Menu.Data;
using LabFusion.Player;
using LabFusion.Data;
using LabFusion.Utilities;
using LabFusion.SDK.Metadata;
using LabFusion.Entities;
using LabFusion.Math;

using UnityEngine;

namespace LabFusion.SDK.Gamemodes;

public class SmashBones : Gamemode
{
    public override string Title => "Smash Bones";

    public override string Author => FusionMod.ModAuthor;

    public override string Description =>
        "Attack other players to build up damage, and knock them off the map to take their stocks! " +
        "Use randomly spawned items to gain advantages over your opponents! " +
        "You are also given <color=#00b3ff>extra abilities</color> such as <color=#00b3ff>Double Jumping</color>, <color=#00b3ff>Dashing</color>, and <color=#00b3ff>Air Control</color>. " +
        "Last person standing wins. " +
        "<color=#ff0000>Requires a Smash Bones supported map.</color>";

    public override Texture Logo => MenuResources.GetGamemodeIcon(Title);

    public override bool AutoHolsterOnDeath => true;

    public override bool DisableDevTools => true;

    public override bool DisableSpawnGun => true;

    public override bool DisableManualUnragdoll => true;

    public static class Defaults
    {
        public const float AirControlSpeed = 5f;

        public const float DashCooldown = 0.3f;

        public const float DashSpeed = 10f;

        public const float AirDashSpeed = 4.25f;

        public const float DashFlickTimer = 0.3f;

        public const int StockCount = 3;
    }

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

    private readonly MusicPlaylist _playlist = new();
    public MusicPlaylist Playlist => _playlist;

    private readonly PlayerScoreKeeper _playerStocksKeeper = new();
    public PlayerScoreKeeper PlayerStocksKeeper => _playerStocksKeeper;

    private readonly MetadataPlayerDictionary<MetadataFloat> _playerDamageKeeper = new();
    public MetadataPlayerDictionary<MetadataFloat> PlayerDamageKeeper => _playerDamageKeeper;

    private readonly TeamManager _teamManager = new();
    public TeamManager TeamManager => _teamManager;

    private readonly Team _freeForAllTeam = new("Free For All");
    public Team FreeForAllTeam => _freeForAllTeam;

    private readonly Team _spectatorTeam = new("Spectators");
    public Team SpectatorTeam => _spectatorTeam;

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
        return PlayerIdManager.PlayerCount >= MinimumPlayers;
    }

    public override void OnGamemodeRegistered()
    {
        // Register teams
        TeamManager.Register(this);
        TeamManager.AddTeam(FreeForAllTeam);
        TeamManager.AddTeam(SpectatorTeam);

        // Register keepers
        PlayerStocksKeeper.Register(Metadata, CommonKeys.LivesKey);
        PlayerStocksKeeper.OnScoreChanged += OnLivesChanged;

        PlayerDamageKeeper.Register(Metadata, CommonKeys.DamageKey);
        PlayerDamageKeeper.OnVariableChanged += OnDamageChanged;

        LocalHealth.OnAttackedByPlayer += OnAttackedByPlayer;
    }

    public override void OnGamemodeUnregistered()
    {
        // Unregister teams
        TeamManager.Unregister();

        // Unregister keepers
        PlayerStocksKeeper.Unregister();
        PlayerStocksKeeper.OnScoreChanged -= OnLivesChanged;

        PlayerDamageKeeper.Unregister();
        PlayerDamageKeeper.OnVariableChanged -= OnDamageChanged;

        LocalHealth.OnAttackedByPlayer -= OnAttackedByPlayer;
    }

    private void OnAttackedByPlayer(Attack attack, PlayerDamageReceiver.BodyPart bodyPart, PlayerId player)
    {
        if (!IsStarted)
        {
            return;
        }

        // Increase damage
        var damageVariable = PlayerDamageKeeper.GetVariable(PlayerIdManager.LocalId);
        float damage = damageVariable.GetValue();

        damage += attack.damage;

        // Damage can only go between 0 -> 999
        damage = ManagedMathf.Clamp(damage, 0f, 999f);

        damageVariable.SetValue(damage);

        // Apply knockback
        var direction = attack.direction;

        var magnitude = 10f + damage;

        if (RigData.HasPlayer)
        {
            RigData.Refs.RigManager.physicsRig.torso._pelvisRb.AddForce(direction * magnitude, ForceMode.Impulse);
        }
    }

    private void OnLivesChanged(PlayerId player, int lives)
    {
        if (!IsStarted)
        {
            return;
        }

        if (NetworkPlayerManager.TryGetPlayer(player, out var networkPlayer))
        {
            networkPlayer.LivesBar.Lives = lives;
        }
    }

    private void OnDamageChanged(PlayerId player, MetadataFloat damage)
    {
        if (!IsStarted)
        {
            return;
        }

        if (NetworkPlayerManager.TryGetPlayer(player, out var networkPlayer))
        {
            networkPlayer.LivesBar.Damage = damage.GetValue();
        }
    }

    public override void OnGamemodeStarted()
    {
        LocalHealth.MortalityOverride = false;
        LocalControls.DoubleJumpOverride = true;
    }

    public override void OnLevelReady()
    {
        ApplyGamemodeSettings();

        Playlist.StartPlaylist();

        var spawnPoints = GamemodeMarker.FilterMarkers(null);

        if (spawnPoints.Count > 0)
        {
            var playerIndex = PlayerIdManager.LocalId.SmallId % spawnPoints.Count;

            GamemodeHelper.SetSpawnPoint(spawnPoints[playerIndex]);
            GamemodeHelper.TeleportToSpawnPoint();
        }
    }

    public override void OnGamemodeStopped()
    {
        Playlist.StopPlaylist();

        LocalHealth.MortalityOverride = null;
        LocalControls.DoubleJumpOverride = null;

        GamemodeHelper.ResetSpawnPoints();
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


        if (RigData.HasPlayer)
        {
            var rigManager = RigData.Refs.RigManager;

            ApplyDashing(rigManager);
        }
    }

    protected override void OnFixedUpdate()
    {
        if (!IsStarted)
        {
            return;
        }

        if (RigData.HasPlayer)
        {
            var rigManager = RigData.Refs.RigManager;

            ApplyAirControl(rigManager);
        }
    }

    private static void ApplyAirControl(RigManager rigManager)
    {
        var physicsRig = rigManager.physicsRig;

        if (!physicsRig.ballLocoEnabled)
        {
            return;
        }

        if (physicsRig.physG.isGrounded)
        {
            return;
        }

        var controllerRig = rigManager.ControllerRig;

        var movementAxis = controllerRig.GetPrimaryAxis();
        var movementDirection = controllerRig.m_head.rotation * new Vector3(movementAxis.x, 0f, movementAxis.y);

        movementDirection.y = 0f;
        movementDirection.Normalize();

        var knee = physicsRig.rbKnee;

        var targetVelocity = movementDirection * Defaults.AirControlSpeed;
        targetVelocity.y = 0f;

        var kneeVelocity = knee.velocity;
        kneeVelocity.y = 0f;

        var limitVelocity = Vector3.ClampMagnitude(kneeVelocity, Defaults.AirControlSpeed);
        var limitError = (limitVelocity - kneeVelocity) * 5f;

        knee.AddForce((limitError + targetVelocity) * rigManager.avatar.massTotal, ForceMode.Force);
    }

    private static int _flickCount = 0;
    private static float _dashCooldown = 0f;
    private static float _flickTimer = 0f;
    private static bool _wasFlicking = false;
    private static bool _dashedMidAir = false;

    private static void ApplyDashing(RigManager rigManager)
    {
        if (_dashCooldown > 0f)
        {
            _dashCooldown -= TimeUtilities.DeltaTime;
            _flickCount = 0;
            return;
        }

        var physicsRig = rigManager.physicsRig;

        if (!physicsRig.ballLocoEnabled)
        {
            _flickCount = 0;
            return;
        }

        bool grounded = physicsRig.physG.isGrounded;

        if (!grounded && _dashedMidAir)
        {
            _flickCount = 0;
            return;
        }
        else if (grounded)
        {
            _dashedMidAir = false;
        }

        if (_flickTimer > 0f)
        {
            _flickTimer -= TimeUtilities.DeltaTime;

            if (_flickTimer <= 0f)
            {
                _flickCount = 0;
                _wasFlicking = false;
                return;
            }
        }

        var controllerRig = rigManager.ControllerRig.TryCast<OpenControllerRig>();

        bool flicking = controllerRig._wasOverFlickThresh;

        if (flicking && !_wasFlicking)
        {
            _flickCount++;
            _flickTimer = Defaults.DashFlickTimer;
        }

        _wasFlicking = flicking;

        if (_flickCount >= 2)
        {
            Dash(rigManager, controllerRig, physicsRig);
            return;
        }
    }

    private static void Dash(RigManager rigManager, OpenControllerRig controllerRig, PhysicsRig physicsRig)
    {
        _flickCount = 0;
        _flickTimer = 0f;
        _dashCooldown = Defaults.DashCooldown;

        float speed = Defaults.DashSpeed;

        if (!physicsRig.physG.isGrounded)
        {
            _dashedMidAir = true;
            speed = Defaults.AirDashSpeed;
        }

        var movementAxis = controllerRig.GetPrimaryAxis();
        var movementDirection = controllerRig.m_head.rotation * new Vector3(movementAxis.x, 0f, movementAxis.y);

        movementDirection.y = 0f;
        movementDirection.Normalize();

        var pelvisVelocity = physicsRig.torso.rbPelvis.velocity;

        var targetVelocity = movementDirection * speed;
        targetVelocity.y = pelvisVelocity.y;

        foreach (var rb in physicsRig.selfRbs)
        {
            if (rb == null)
            {
                continue;
            }

            rb.velocity = rb.velocity - pelvisVelocity + targetVelocity;
        }
    }

    public void ApplyGamemodeSettings()
    {
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

        Playlist.SetPlaylist(playlist);
        Playlist.Shuffle();
    }
}
