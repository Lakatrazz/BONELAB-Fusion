using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Combat;
using Il2CppSLZ.Marrow.Data;

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
using LabFusion.SDK.Triggers;
using LabFusion.Network;
using LabFusion.Extensions;
using LabFusion.UI.Popups;
using LabFusion.SDK.Points;
using LabFusion.Marrow.Pool;
using LabFusion.Senders;

using UnityEngine;

using System.Text.Json.Serialization;
using System.Text.Json;

namespace LabFusion.SDK.Gamemodes;

public class SmashBones : Gamemode
{
    [Serializable]
    public struct DamageInfo
    {
        [JsonPropertyName("platformID")]
        public ulong PlatformID { get; set; }

        [JsonPropertyName("damage")]
        public float Damage { get; set; }
    }

    [Serializable]
    public struct DeathInfo
    {
        [JsonPropertyName("platformID")]
        public ulong PlatformID { get; set; }

        [JsonPropertyName("position")]
        public JsonVector3 Position { get; set; }

        [JsonPropertyName("direction")]
        public JsonVector3 Direction { get; set; }
    }

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

    public const string NotificationTag = "SmashBones";

    public static class Defaults
    {
        public const float AirControlSpeed = 5f;

        public const float DashCooldown = 0.3f;

        public const float DashSpeed = 10f;

        public const float AirDashSpeed = 6f;

        public const float ExtraJumpVelocity = 8f;

        public const int StockCount = 3;

        public const float ExtraJumpCooldown = 0.2f;

        public const bool DropItems = true;

        public const float ItemFrequency = 10f;

        public const float MaxAvatarHeight = 3f;

        // Abilities
        public const float SecondDashHeight = 1.6f;

        public const float SecondJumpHeight = 1.3f;

        public const float WeakJumpHeight = 1.8f;

        public const float WeakestJumpHeight = MaxAvatarHeight;

        public const float WeakMobilityMass = 100f;

        public const float WeakestMobilityMass = 250f;

        // Rewards
        public const int BitRewardPerPlayer = 100;
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

    private readonly PlayerScoreKeeper _playerScoreKeeper = new();
    public PlayerScoreKeeper PlayerScoreKeeper => _playerScoreKeeper;

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

    public TriggerEvent PlayerDamageEvent { get; set; }
    public TriggerEvent PlayerDeathEvent { get; set; }

    private int _previousStocks = -1;

    private int _latestScore = 0;

    private MonoDiscReference _victorySongReference = null;
    private MonoDiscReference _failureSongReference = null;

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

    public override void OnGamemodeRegistered()
    {
        PlayerDamageEvent = new TriggerEvent("PlayerDamage", Relay, false);
        PlayerDamageEvent.OnTriggeredWithValue += OnPlayerDamageEvent;

        PlayerDeathEvent = new TriggerEvent("PlayerDeath", Relay, false);
        PlayerDeathEvent.OnTriggeredWithValue += OnPlayerDeathEvent;

        // Register teams
        TeamManager.Register(this);
        TeamManager.AddTeam(FreeForAllTeam);
        TeamManager.AddTeam(SpectatorTeam);
        TeamManager.OnAssignedToTeam += OnAssignedToTeam;
        TeamManager.OnRemovedFromTeam += OnRemovedFromTeam;

        // Register keepers
        PlayerScoreKeeper.Register(Metadata);

        PlayerStocksKeeper.Register(Metadata, CommonKeys.LivesKey);
        PlayerStocksKeeper.OnPlayerScoreChanged += OnLivesChanged;

        PlayerDamageKeeper.Register(Metadata, CommonKeys.DamageKey);
        PlayerDamageKeeper.OnPlayerVariableChanged += OnDamageChanged;

        LocalHealth.OnAttackedByPlayer += OnAttackedByPlayer;
    }

    public override void OnGamemodeUnregistered()
    {
        PlayerDamageEvent.UnregisterEvent();

        // Unregister teams
        TeamManager.Unregister();
        TeamManager.OnAssignedToTeam -= OnAssignedToTeam;
        TeamManager.OnRemovedFromTeam -= OnRemovedFromTeam;

        // Unregister keepers
        PlayerScoreKeeper.Unregister();

        PlayerStocksKeeper.Unregister();
        PlayerStocksKeeper.OnPlayerScoreChanged -= OnLivesChanged;

        PlayerDamageKeeper.Unregister();
        PlayerDamageKeeper.OnPlayerVariableChanged -= OnDamageChanged;

        LocalHealth.OnAttackedByPlayer -= OnAttackedByPlayer;
    }

    private void OnAssignedToTeam(PlayerID player, Team team)
    {
        bool spectator = team == SpectatorTeam;
        bool selfSpectator = TeamManager.GetLocalTeam() == SpectatorTeam;

        if (NetworkPlayerManager.TryGetPlayer(player, out var networkPlayer))
        {
            networkPlayer.LivesBar.Visible = !spectator;
            networkPlayer.ForceHide = spectator && !selfSpectator;
        }

        if (player.IsMe)
        {
            OnSelfAssignedToTeam(team);
        }
    }

    private void OnRemovedFromTeam(PlayerID player, Team team)
    {
        if (NetworkPlayerManager.TryGetPlayer(player, out var networkPlayer))
        {
            networkPlayer.LivesBar.Visible = false;
            networkPlayer.ForceHide = false;
        }
    }

    private void OnSelfAssignedToTeam(Team team)
    {
        bool spectator = team == SpectatorTeam;

        LocalControls.DisableInteraction = spectator;

        OnSetSpawn(spectator);

        OnCheckPlayerHiding(spectator);
    }

    private void OnCheckPlayerHiding(bool selfSpectator)
    {
        foreach (var player in PlayerIDManager.PlayerIDs)
        {
            if (player.IsMe)
            {
                continue;
            }

            if (!NetworkPlayerManager.TryGetPlayer(player, out var networkPlayer))
            {
                continue;
            }

            bool spectator = TeamManager.GetPlayerTeam(player) == SpectatorTeam;

            networkPlayer.ForceHide = spectator && !selfSpectator;
        }
    }

    private void OnAttackedByPlayer(Attack attack, PlayerDamageReceiver.BodyPart bodyPart, PlayerID player)
    {
        if (!IsStarted)
        {
            return;
        }

        // Spectators cannot attack
        if (TeamManager.GetPlayerTeam(player) == SpectatorTeam)
        {
            return;
        }

        // Spectators also cannot be attacked
        if (TeamManager.GetLocalTeam() == SpectatorTeam)
        {
            return;
        }

        // Increase damage
        var damageVariable = PlayerDamageKeeper.GetVariable(PlayerIDManager.LocalID);
        float damage = damageVariable.GetValue();

        float addedDamage = attack.damage;

        switch (attack.attackType)
        {
            case AttackType.Blunt:
                addedDamage *= 5f;
                break;
        }

        addedDamage = ManagedMathf.Clamp(addedDamage, 0f, 12f);

        damage += addedDamage;

        // Damage can only go between 0 -> 999
        damage = ManagedMathf.Clamp(damage, 0f, 999f);

        PlayerDamageEvent.TryInvoke(JsonSerializer.Serialize(new DamageInfo()
        {
            PlatformID = PlayerIDManager.LocalPlatformID,
            Damage = damage,
        }));

        // Apply knockback
        var direction = attack.direction.normalized;
        direction.y = 0f;
        direction.Normalize();

        var magnitude = (1f + MathF.Pow(damage, 0.8f)) * 3f;

        if (RigData.HasPlayer)
        {
            var rigManager = RigData.Refs.RigManager;
            var pelvisRb = rigManager.physicsRig.torso._pelvisRb;
            var avatarMass = rigManager.avatar.massTotal;
            
            magnitude *= CalculateMassContribution(avatarMass);

            var punchForce = direction * magnitude;
            var upForce = -Physics.gravity.normalized * magnitude * 0.2f;

            var force = punchForce + upForce;

            pelvisRb.AddForce(force, ForceMode.Impulse);

            SlipGrip(rigManager.physicsRig.leftHand);
            SlipGrip(rigManager.physicsRig.rightHand);
        }
    }

    private static float CalculateMassContribution(float avatarMass)
    {
        return MathF.Pow(avatarMass / 80f, 0.8f) * 10f;
    }

    private static void SlipGrip(Hand hand)
    {
        var attachedReceiver = hand.AttachedReceiver;

        if (attachedReceiver == null)
        {
            return;
        }

        var grip = attachedReceiver.TryCast<Grip>();

        if (grip == null)
        {
            return;
        }

        // Always detach static grips
        if (grip.IsStatic)
        {
            hand.TryDetach();
        }
        else if (IsPlayerGrip(grip))
        {
            RandomDetachHand(hand, 50f);
        }
        else
        {
            RandomDetachHand(hand, 5f);
        }
    }

    private static void RandomDetachHand(Hand hand, float percentChance)
    {
        var random = UnityEngine.Random.Range(0f, 100f);

        if (random <= percentChance)
        {
            hand.TryDetach();
        }
    }

    private static bool IsPlayerGrip(Grip grip)
    {
        var marrowEntity = grip._marrowEntity;

        if (marrowEntity == null)
        {
            return false;
        }

        if (!IMarrowEntityExtender.Cache.TryGet(marrowEntity, out var networkEntity))
        {
            return false;
        }

        var networkPlayer = networkEntity.GetExtender<NetworkPlayer>();

        return networkPlayer != null;
    }

    private void OnPlayerDamageEvent(string value)
    {
        if (!IsStarted || !NetworkInfo.IsHost)
        {
            return;
        }

        var damageInfo = JsonSerializer.Deserialize<DamageInfo>(value);

        var playerId = PlayerIDManager.GetPlayerID(damageInfo.PlatformID);

        PlayerDamageKeeper.GetVariable(playerId).SetValue(damageInfo.Damage);
    }

    private void OnPlayerDeathEvent(string value)
    {
        if (!IsStarted)
        {
            return;
        }

        var deathInfo = JsonSerializer.Deserialize<DeathInfo>(value);

        var playerID = PlayerIDManager.GetPlayerID(deathInfo.PlatformID);

        if (NetworkInfo.IsHost)
        {
            var stocks = PlayerStocksKeeper.GetScore(playerID);

            var newStocks = stocks - 1;

            if (newStocks < 0)
            {
                newStocks = 0;
            }

            PlayerStocksKeeper.SetScore(playerID, newStocks);
            PlayerDamageKeeper.GetVariable(playerID).SetValue(0f);

            // Move the player to the spectator team if they lost all stocks
            if (newStocks <= 0)
            {
                TeamManager.TryAssignTeam(playerID, SpectatorTeam);
            }
        }

        SpawnExplosion(deathInfo.Position.ToUnityVector3(), -deathInfo.Direction.ToUnityVector3());

        MultiplayerHooking.InvokeOnPlayerAction(playerID, PlayerActionType.DEATH);
    }

    private void OnLivesChanged(PlayerID playerID, int lives)
    {
        if (!IsStarted)
        {
            return;
        }

        if (NetworkPlayerManager.TryGetPlayer(playerID, out var networkPlayer))
        {
            networkPlayer.LivesBar.Lives = lives;
        }

        if (playerID.IsMe)
        {
            OnSelfLivesChanged(lives);
        }

        if (NetworkInfo.IsHost && lives <= 0)
        {
            _latestScore++;
            PlayerScoreKeeper.SetScore(playerID, _latestScore);

            CheckFreeForAllStocksVictory();
        }
    }

    private void CheckFreeForAllStocksVictory()
    {
        var livingPlayers = new List<PlayerID>();

        foreach (var player in PlayerIDManager.PlayerIDs)
        {
            var stocks = PlayerStocksKeeper.GetScore(player);
            bool spectator = SpectatorTeam.HasPlayer(player);

            if (stocks > 0 && !spectator)
            {
                livingPlayers.Add(player);
            }
        }

        int minimumCount = PlayerIDManager.HasOtherPlayers ? 1 : 0;

        if (livingPlayers.Count <= minimumCount)
        {
            _latestScore++;

            foreach (var player in livingPlayers)
            {
                PlayerScoreKeeper.SetScore(player, _latestScore);
            }

            GamemodeManager.StopGamemode();
        }
    }

    private void OnSelfLivesChanged(int lives)
    {
        bool stocksDecreased = _previousStocks > lives;

        if (stocksDecreased)
        {
            Notifier.Cancel(NotificationTag);

            if (lives > 0)
            {
                Notifier.Send(new Notification()
                {
                    Title = "Lost a Stock",
                    Message = $"You lost a stock! You are now down to {lives} stock{(lives != 1 ? "s" : "")}!",
                    Tag = NotificationTag,
                    SaveToMenu = false,
                    ShowPopup = true,
                    PopupLength = 4f,
                    Type = NotificationType.INFORMATION,
                });
            }
            else
            {
                Notifier.Send(new Notification()
                {
                    Title = "Lost All Stocks!",
                    Message = $"You lost all of your stocks! You are now a spectator!",
                    Tag = NotificationTag,
                    SaveToMenu = false,
                    ShowPopup = true,
                    PopupLength = 4f,
                    Type = NotificationType.INFORMATION,
                });
            }
        }

        _previousStocks = lives;
    }

    private void OnDamageChanged(PlayerID player, MetadataFloat damage)
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
        DeathTrigger.KillDamageOverride = false;
        LocalControls.DisableSlowMo = true;
        LocalControls.DisableAmmoPouch = true;
        LocalControls.DisableInventory = true;

        // Lock the avatar and limit max height
        LocalAvatar.HeightOverride = ManagedMathf.Clamp(LocalAvatar.AvatarHeight, 0f, Defaults.MaxAvatarHeight);
        LocalAvatar.AvatarOverride = LocalAvatar.AvatarBarcode;

        DeathTrigger.OnKillPlayer += OnKillPlayer;

        if (NetworkInfo.IsHost)
        {
            PlayerScoreKeeper.ResetScores();
            _latestScore = 0;

            AssignTeams();
        }

        Notifier.Cancel(NotificationTag);

        Notifier.Send(new Notification()
        {
            Title = "Stock Smash",
            Message = $"All players have {Defaults.StockCount} stocks! Knock players off the map to deplete their stocks and be the last one standing!",
            Tag = NotificationTag,
            ShowPopup = true,
            SaveToMenu = false,
            PopupLength = 4f,
            Type = NotificationType.INFORMATION,
        });
    }

    public override void OnLevelReady()
    {
        ApplyGamemodeSettings();

        Playlist.StartPlaylist();

        OnSetSpawn(SpectatorTeam.HasPlayer(PlayerIDManager.LocalID));
    }

    protected override void OnPlayerJoined(PlayerID playerID)
    {
        if (NetworkInfo.IsHost)
        {
            PlayerStocksKeeper.SetScore(playerID, 0);
            PlayerScoreKeeper.SetScore(playerID, 0);
            PlayerDamageKeeper.GetVariable(playerID).SetValue(0f);

            TeamManager.TryAssignTeam(playerID, SpectatorTeam);
        }
    }

    protected override void OnPlayerLeft(PlayerID playerID)
    {
        if (NetworkInfo.IsHost)
        {
            CheckFreeForAllStocksVictory();
        }
    }

    public override void OnGamemodeStopped()
    {
        Playlist.StopPlaylist();

        LocalHealth.MortalityOverride = null;
        DeathTrigger.KillDamageOverride = null;
        LocalControls.DisableInteraction = false;
        LocalControls.DisableSlowMo = false;
        LocalControls.DisableAmmoPouch = false;
        LocalControls.DisableInventory = false;

        LocalAvatar.HeightOverride = null;
        LocalAvatar.AvatarOverride = null;

        DeathTrigger.OnKillPlayer -= OnKillPlayer;

        GamemodeHelper.ResetSpawnPoints();
        GamemodeHelper.TeleportToSpawnPoint();

        TeamManager.UnassignAllPlayers();

        _previousStocks = -1;

        CheckFinalScore();

        if (NetworkInfo.IsHost)
        {
            GamemodeDropper.DespawnItems();
        }
    }

    private void CheckFinalScore()
    {
        // Get the winner message
        var firstPlace = PlayerScoreKeeper.GetPlayerByPlace(0);
        var secondPlace = PlayerScoreKeeper.GetPlayerByPlace(1);
        var thirdPlace = PlayerScoreKeeper.GetPlayerByPlace(2);

        var selfPlace = PlayerScoreKeeper.GetPlace(PlayerIDManager.LocalID) + 1;

        string message = "No one ran out of stocks!";

        if (firstPlace != null && firstPlace.TryGetDisplayName(out var name))
        {
            message = $"First Place: {name} \n";
        }

        if (secondPlace != null && secondPlace.TryGetDisplayName(out name))
        {
            message += $"Second Place: {name} \n";
        }

        if (thirdPlace != null && thirdPlace.TryGetDisplayName(out name))
        {
            message += $"Third Place: {name} \n";
        }

        if (selfPlace != -1 && selfPlace > 3)
        {
            message += $"Your Place: {selfPlace}";
        }

        // Play victory/failure sounds
        int playerCount = PlayerIDManager.PlayerCount;

        if (playerCount > 1)
        {
            bool isVictory = selfPlace <= 1;

            OnVictoryStatus(isVictory);
        }

        Notifier.Cancel(NotificationTag);

        Notifier.Send(new Notification()
        {
            Title = "Smash Bones Completed",
            Tag = NotificationTag,

            Message = message,

            PopupLength = 6f,

            SaveToMenu = false,
            ShowPopup = true,
        });

        // Give bit reward
        RewardBits(selfPlace, playerCount);
    }

    private static void RewardBits(int placement, int playerCount)
    {
        // There needs to be other players
        if (playerCount <= 1)
        {
            return;
        }

        // Only first three places should get bits
        if (placement > 3)
        {
            return;
        }

        var bitReward = (Defaults.BitRewardPerPlayer * playerCount) / placement;

        PointItemManager.RewardBits(bitReward);
    }

    private static void OnSetSpawn(bool spectator)
    {
        if (!RigData.HasPlayer)
        {
            return;
        }

        if (spectator)
        {
            GamemodeHelper.SetSpawnPoints(GamemodeMarker.FilterMarkers(FusionBoneTagReferences.SpectatorReference));
        }
        else
        {
            GamemodeHelper.SetSpawnPoints(GamemodeMarker.FilterMarkers(null));
        }

        GamemodeHelper.TeleportToSpawnPoint();
    }

    private void OnKillPlayer()
    {
        var pelvis = RigData.Refs.RigManager.physicsRig.torso._pelvisRb;

        PlayerDeathEvent.TryInvoke(JsonSerializer.Serialize(new DeathInfo()
        {
            PlatformID = PlayerIDManager.LocalPlatformID,
            Position = new(pelvis.position),
            Direction = new(pelvis.velocity.normalized),
        }));

        GamemodeHelper.TeleportToSpawnPoint();
    }

    private static void SpawnExplosion(Vector3 position, Vector3 forward)
    {
        var spawnable = LocalAssetSpawner.CreateSpawnable(FusionSpawnableReferences.DeathExplosionReference);

        LocalAssetSpawner.Register(spawnable);

        LocalAssetSpawner.Spawn(spawnable, position, Quaternion.LookRotation(forward));

        var sfxReference = new AudioReference(FusionMonoDiscReferences.DeathExplosionReference);

        LocalAudioPlayer.PlayAtPoint(sfxReference, position, new AudioPlayerSettings()
        {
            Mixer = LocalAudioPlayer.HardInteraction,
            MinDistance = 50f,
        });
    }

    private void AssignTeams()
    {
        foreach (var player in PlayerIDManager.PlayerIDs)
        {
            SetupPlayer(player);
        }
    }

    private void SetupPlayer(PlayerID player)
    {
        TeamManager.TryAssignTeam(player, FreeForAllTeam);
        PlayerDamageKeeper.GetVariable(player).SetValue(0f);
        PlayerStocksKeeper.SetScore(player, Defaults.StockCount);
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
            ApplyAutoRun(rigManager);
            ApplyDoubleJump(rigManager);
        }

        if (NetworkInfo.IsHost && Defaults.DropItems)
        {
            UpdateItemDroppers();
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

    private static float _itemDropTimer = 0f;

    private static void UpdateItemDroppers()
    {
        _itemDropTimer += TimeUtilities.DeltaTime;

        if (_itemDropTimer < Defaults.ItemFrequency)
        {
            return;
        }

        _itemDropTimer = 0f;

        if (GamemodeDropper.DroppedItemCount < GamemodeDropperSettings.GetMaxItems())
        {
            GamemodeDropper.DropItem();
        }
    }

    private static void ApplyAutoRun(RigManager rigManager) 
    {
        var remapHeptaRig = rigManager.remapHeptaRig;

        if (remapHeptaRig.travState == RemapRig.TraversalState.Walk)
        {
            remapHeptaRig.travState = RemapRig.TraversalState.Jog;
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

        float airControlMultiplier = CalculateMobilityMultiplier(LocalAvatar.AvatarMass);

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

        knee.AddForce((limitError + targetVelocity) * rigManager.avatar.massTotal * airControlMultiplier, ForceMode.Force);
    }

    private static int _remainingJumps = 0;
    private static float _airTime = 0f;

    private static void ApplyDoubleJump(RigManager rigManager)
    {
        int maxJumps = 1;
        float jumpVelocityMultiplier = CalculateJumpMultiplier(LocalAvatar.AvatarHeight);

        if (LocalAvatar.AvatarHeight <= Defaults.SecondJumpHeight)
        {
            maxJumps++;
            jumpVelocityMultiplier *= 0.7f;
        }

        var physicsRig = rigManager.physicsRig;

        if (!physicsRig.ballLocoEnabled)
        {
            return;
        }

        if (physicsRig.physG.isGrounded)
        {
            _remainingJumps = maxJumps;
            _airTime = 0f;
            return;
        }

        _airTime += TimeUtilities.DeltaTime;

        if (_airTime <= Defaults.ExtraJumpCooldown)
        {
            return;
        }

        if (_remainingJumps <= 0)
        {
            return;
        }

        var controllerRig = rigManager.ControllerRig;

        if (controllerRig._secondaryAButtonUp)
        {
            _remainingJumps--;
            _airTime = 0f;

            var jumpVelocity = physicsRig.torso.rbPelvis.velocity;
            jumpVelocity.y = Defaults.ExtraJumpVelocity * jumpVelocityMultiplier;

            SetPhysicsRigVelocity(physicsRig, jumpVelocity);

            physicsRig.headSfx.JumpEffort();
        }
    }

    private static float _dashCooldown = 0f;
    private static int _midAirDashCount = 0;

    private static void ApplyDashing(RigManager rigManager)
    {
        int maxAirDashes = 1;
        float dashSpeedMultiplier = CalculateMobilityMultiplier(LocalAvatar.AvatarMass);

        if (LocalAvatar.AvatarHeight <= Defaults.SecondDashHeight)
        {
            maxAirDashes++;
        }

        if (_dashCooldown > 0f)
        {
            _dashCooldown -= TimeUtilities.DeltaTime;
            return;
        }

        var physicsRig = rigManager.physicsRig;

        if (!physicsRig.ballLocoEnabled)
        {
            return;
        }

        bool grounded = physicsRig.physG.isGrounded;

        if (!grounded && (_midAirDashCount >= maxAirDashes))
        {
            return;
        }
        else if (grounded)
        {
            _midAirDashCount = 0;
        }

        var controllerRig = rigManager.ControllerRig.TryCast<OpenControllerRig>();

        bool stickDown = controllerRig._primaryStickDown;

        if (stickDown)
        {
            Dash(controllerRig, physicsRig, dashSpeedMultiplier);
            return;
        }
    }

    private static void Dash(OpenControllerRig controllerRig, PhysicsRig physicsRig, float speedMultiplier = 1f)
    {
        _dashCooldown = Defaults.DashCooldown;

        float speed = Defaults.DashSpeed;

        if (!physicsRig.physG.isGrounded)
        {
            _midAirDashCount++;
            speed = Defaults.AirDashSpeed;
        }

        speed *= speedMultiplier;

        var movementAxis = controllerRig.GetPrimaryAxis();
        var movementDirection = controllerRig.m_head.rotation * new Vector3(movementAxis.x, 0f, movementAxis.y);

        movementDirection.y = 0f;
        movementDirection.Normalize();

        var pelvisVelocity = physicsRig.torso.rbPelvis.velocity;

        var targetVelocity = movementDirection * speed;
        targetVelocity.y = pelvisVelocity.y;

        SetPhysicsRigVelocity(physicsRig, targetVelocity);
    }

    private static float CalculateJumpMultiplier(float avatarHeight)
    {
        float jumpMultiplier = 1f;

        if (avatarHeight >= Defaults.WeakJumpHeight)
        {
            float percentHeight = ManagedMathf.Clamp01((avatarHeight - Defaults.WeakJumpHeight) / (Defaults.MaxAvatarHeight - Defaults.WeakJumpHeight)) * 0.4f;

            jumpMultiplier = 1f - percentHeight;
        }

        return jumpMultiplier;
    }

    private static float CalculateMobilityMultiplier(float avatarMass)
    {
        float mobilityMultiplier = 1f;

        if (avatarMass >= Defaults.WeakMobilityMass)
        {
            float percentMass = ManagedMathf.Clamp01((avatarMass - Defaults.WeakMobilityMass) / (Defaults.WeakestMobilityMass - Defaults.WeakMobilityMass)) * 0.8f;

            mobilityMultiplier = 1f - percentMass;
        }

        return mobilityMultiplier;
    }

    private static void SetPhysicsRigVelocity(PhysicsRig physicsRig, Vector3 velocity)
    {
        var pelvisVelocity = physicsRig.torso.rbPelvis.velocity;

        foreach (var rb in physicsRig.selfRbs)
        {
            if (rb == null)
            {
                continue;
            }

            rb.velocity = rb.velocity - pelvisVelocity + velocity;
        }
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
}
