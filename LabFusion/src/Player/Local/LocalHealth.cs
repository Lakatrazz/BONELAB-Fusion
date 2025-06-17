using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Combat;
using Il2CppSLZ.VRMK;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Utilities;

namespace LabFusion.Player;

public static class LocalHealth
{
    private static float? _vitalityOverride = null;
    private static bool _wasOverridingVitality = false;

    public static float? VitalityOverride
    {
        get
        {
            return _vitalityOverride;
        }
        set
        {
            _vitalityOverride = value;

            OnOverrideHealth();
        }
    }

    private static bool? _regenerationOverride = null;
    public static bool? RegenerationOverride
    {
        get
        {
            return _regenerationOverride;
        }
        set
        {
            _regenerationOverride = value;

            OnOverrideHealth();
        }
    }

    private static bool? _mortalityOverride = null;
    public static bool? MortalityOverride
    {
        get
        {
            return _mortalityOverride;
        }
        set
        {
            _mortalityOverride = value;

            OnOverrideHealth();
        }
    }

    /// <summary>
    /// Callback that is invoked when the local player respawns.
    /// </summary>
    public static event Action OnRespawn;

    /// <summary>
    /// Callback that is invoked when the local player is attacked by another player. Passes in the Attack, the hit BodyPart, and the attacking PlayerId.
    /// </summary>
    public static event Action<Attack, PlayerDamageReceiver.BodyPart, PlayerID> OnAttackedByPlayer;

    /// <summary>
    /// Sets the Local Player's health to full.
    /// </summary>
    public static void SetFullHealth()
    {
        if (!RigData.HasPlayer)
        {
            return;
        }

        var health = RigData.Refs.Health;

        health.SetFullHealth();
        health.isInstaDying = false;
    }

    internal static void OnInitializeMelon()
    {
        LocalAvatar.OnAvatarChanged += OnAvatarChanged;
        LobbyInfoManager.OnLobbyInfoChanged += OnLobbyInfoChanged;
        OnRespawn += OnRespawned;
    }

    private static void OnRespawned()
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        if (!RigData.HasPlayer)
        {
            return;
        }

        var health = RigData.Refs.Health;

        // SLZ doesn't reset this, so instant death health mode can only die once normally
        health.isInstaDying = false;
    }

    internal static void InvokeRespawn()
    {
        OnRespawn?.InvokeSafe("executing LocalHealth.OnRespawn");
    }

    internal static void InvokeAttackedByPlayer(Attack attack, PlayerDamageReceiver.BodyPart bodyPart, PlayerID playerId)
    {
        OnAttackedByPlayer?.InvokeSafe(attack, bodyPart, playerId, "executing LocalHealth.OnAttackedByPlayer");
    }

    private static void OnLobbyInfoChanged()
    {
        OnOverrideHealth();
    }

    private static void OnAvatarChanged(Avatar avatar, string barcode)
    {
        OnOverrideHealth();
    }

    private static void OnOverrideHealth()
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        if (!RigData.HasPlayer)
        {
            return;
        }

        var rigManager = RigData.Refs.RigManager;
        var avatar = rigManager.avatar;
        var health = RigData.Refs.Health;

        // Apply vitality
        if (VitalityOverride.HasValue)
        {
            avatar._vitality = VitalityOverride.Value;
            health.SetAvatar(avatar);

            _wasOverridingVitality = true;
        }
        else if (_wasOverridingVitality)
        {
            avatar.RefreshBodyMeasurements();
            health.SetAvatar(avatar);

            _wasOverridingVitality = false;
        }

        // Apply mortality
        bool mortal = CommonPreferences.Mortality && !CommonPreferences.Knockout;

        if (MortalityOverride.HasValue)
        {
            mortal = MortalityOverride.Value;
        }

        bool regenerate = RegenerationOverride ?? true;

        if (mortal && regenerate)
        {
            health.healthMode = Health.HealthMode.Mortal;
        }
        else if (mortal && !regenerate)
        {
            health.healthMode = Health.HealthMode.InsantDeath;
        }
        else
        {
            health.healthMode = Health.HealthMode.Invincible;
        }
    }
}
