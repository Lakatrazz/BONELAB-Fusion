using Il2CppSLZ.Marrow;

using LabFusion.Data;
using LabFusion.Marrow.Patching;
using LabFusion.SDK.Gamemodes;
using LabFusion.Utilities;
using LabFusion.Extensions;

using MelonLoader;

using System.Collections;

using UnityEngine;

namespace LabFusion.Player;

/// <summary>
/// Provides functions for interacting with the local player's ragdoll, including toggling the ragdoll or knocking out the player temporarily.
/// </summary>
public static class LocalRagdoll
{
    /// <summary>
    /// Invoked when the local player has been knocked out.
    /// </summary>
    public static event Action KnockoutStarted;

    /// <summary>
    /// Invoked when the local player is no longer knocked out.
    /// </summary>
    public static event Action KnockoutEnded;

    /// <summary>
    /// Returns true if the local player is currently knocked out (ragdolled and screen is fading from black).
    /// </summary>
    public static bool IsKnockedOut
    {
        get => _isKnockedOut;
        private set
        {
            if (_isKnockedOut == value)
            {
                return;
            }

            if (value)
            {
                KnockoutStarted?.InvokeSafe("invoking LocalRagdoll.KnockoutStarted event");
            }
            else
            {
                KnockoutEnded?.InvokeSafe("invoking LocalRagdoll.KnockoutEnded event");
            }

            _isKnockedOut = value;
        }
    }

    /// <summary>
    /// Returns true if the local player's ragdoll is currently locked, and they cannot unragdoll.
    /// </summary>
    public static bool IsRagdollLocked
    {
        get
        {
            // No ragdoll lock if there is no player
            if (!RigData.HasPlayer)
            {
                return false;
            }

            var rigManager = RigData.Refs.RigManager;

            // If the player is dying and there is a gamemode, check if unragdoll is allowed
            if (rigManager.health.deathIsImminent && GamemodeManager.IsGamemodeStarted)
            {
                var gamemode = GamemodeManager.ActiveGamemode;

                if (gamemode.DisableManualUnragdoll)
                {
                    return true;
                }
            }

            return IsKnockedOut;
        }
    }

    private static bool _isKnockedOut = false;

    /// <summary>
    /// Ragdolls or unragdolls the player.
    /// </summary>
    /// <param name="ragdolled"></param>
    public static void ToggleRagdoll(bool ragdolled)
    {
        if (!RigData.HasPlayer)
        {
            return;
        }

        PhysicsRigPatches.ForceAllowUnragdoll = true;

        var physicsRig = RigData.Refs.RigManager.physicsRig;

        if (ragdolled)
        {
            physicsRig.ShutdownRig();
            physicsRig.RagdollRig();
        }
        else
        {
            physicsRig.TurnOnRig();
            physicsRig.UnRagdollRig();
        }

        PhysicsRigPatches.ForceAllowUnragdoll = false;
    }

    /// <summary>
    /// Knocks out the player for a certain amount of time. This will ragdoll the player and cause their vision to go black.
    /// </summary>
    /// <param name="length"></param>
    public static void Knockout(float length) => Knockout(length, true);

    /// <summary>
    /// Knocks out the player for a certain amount of time. This will ragdoll the player and, if blind is true, cause their vision to go black.
    /// </summary>
    /// <param name="length"></param>
    /// <param name="blind"></param>
    public static void Knockout(float length, bool blind)
    {
        if (!RigData.HasPlayer)
        {
            return;
        }

        if (IsKnockedOut)
        {
            return;
        }

        MelonCoroutines.Start(KnockoutCoroutine(RigData.Refs.RigManager, length, blind));
    }

    private static IEnumerator KnockoutCoroutine(RigManager rigManager, float length, bool blind)
    {
        // Release all grips
        LocalPlayer.ReleaseGrips();

        // Ragdoll the rig
        ToggleRagdoll(true);

        // Blind the player
        LocalVision.Blind = blind;
        LocalVision.BlindColor = Color.black;

        IsKnockedOut = true;

        // Wait a certain amount of time to wake up
        float elapsed = 0f;

        float eyeLength = MathF.Min(10f, length);

        while (elapsed <= length)
        {
            elapsed += TimeReferences.DeltaTime;

            float eyeStart = MathF.Max(length - eyeLength, 0f);

            float eyeProgress = MathF.Max(elapsed - eyeStart, 0f) / eyeLength;
            LocalVision.BlindColor = Color.Lerp(Color.black, Color.clear, MathF.Pow(eyeProgress, 3f));

            yield return null;
        }

        IsKnockedOut = false;

        LocalVision.Blind = false;
        LocalVision.BlindColor = Color.black;

        // Make sure the rig still exists
        if (rigManager == null)
        {
            yield break;
        }

        // Revive fully
        rigManager.health.SetFullHealth();

        // Unragdoll the rig
        ToggleRagdoll(false);
    }
}
