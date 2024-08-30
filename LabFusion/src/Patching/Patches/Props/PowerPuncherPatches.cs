using HarmonyLib;

using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;

using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Entities;

using UnityEngine;

using System.Collections;

using MelonLoader;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(PowerPuncher))]
public static class PowerPuncherPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PowerPuncher.OnSignificantCollisionEnter))]
    public static void OnSignificantCollisionEnter(PowerPuncher __instance, CollisionCollector.RelevantCollision collision)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        float cooldownTime = __instance._cooldownStartTime;
        bool punchedObject = Mathf.Approximately(cooldownTime, Time.time);

        if (!punchedObject)
        {
            return;
        }

        var rigidbody = collision.rigidbody;

        if (rigidbody == null)
        {
            return;
        }

        var marrowBody = MarrowBody.Cache.Get(rigidbody.gameObject);

        if (marrowBody == null)
        {
            return;
        }

        // Check if the hit MarrowEntity has a NetworkEntity
        var networkEntity = IMarrowEntityExtender.Cache.Get(marrowBody.Entity);

        if (networkEntity == null)
        {
            return;
        }

        // Get the network player from the entity
        var networkPlayer = networkEntity.GetExtender<NetworkPlayer>();

        if (networkPlayer == null)
        {
            return;
        }

        // Make sure the player is us
        if (!networkEntity.IsOwner)
        {
            return;
        }

        // Already knocked out?
        if (_isKnockedOut)
        {
            // Reset timer
            _knockedElapsed = 0f;
        }
        // Not knocked out?
        else
        {
            // Get knockout time
            var relativeVelocity = (__instance._host.Rb.velocity - marrowBody._rigidbody.velocity).magnitude;
            var knockoutTime = Mathf.Clamp(relativeVelocity, 0f, 2f);
        
            // Start knockout
            MelonCoroutines.Start(KnockoutRig(networkPlayer.RigRefs.RigManager, knockoutTime));
        }
    }

    private static bool _isKnockedOut = false;
    private static float _knockedElapsed = 0f;

    // In the future, replace with a universal knockout function
    // This works for now though
    private static IEnumerator KnockoutRig(RigManager rigManager, float time)
    {
        _isKnockedOut = true;

        rigManager.bodyState = RigManager.BodyState.Ragdoll;

        _knockedElapsed = 0f;

        while (_knockedElapsed < time)
        {
            _knockedElapsed += TimeUtilities.DeltaTime;
            yield return null;
        }

        _knockedElapsed = 0f;

        var health = rigManager.health;

        if (health.alive && !health.deathIsImminent)
        {
            rigManager.bodyState = RigManager.BodyState.OnFoot;
        }

        _isKnockedOut = false;
    }
}