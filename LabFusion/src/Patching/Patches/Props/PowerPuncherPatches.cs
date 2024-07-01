using HarmonyLib;

using Il2CppSLZ.Combat;
using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Rig;

using LabFusion.Network;
using LabFusion.Utilities;

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

        var rigManager = RigManager.Cache.Get(marrowBody.Entity.gameObject);

        if (rigManager != null && rigManager.IsSelf())
        {
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
                MelonCoroutines.Start(KnockoutRig(rigManager, knockoutTime));
            }
        }
    }

    private static bool _isKnockedOut = false;
    private static float _knockedElapsed = 0f;

    // In the future, replace with a universal knockout function
    // This works for now though
    private static IEnumerator KnockoutRig(RigManager rigManager, float time)
    {
        _isKnockedOut = true;

        var physicsRig = rigManager.physicsRig;

        physicsRig.RagdollRig();

        _knockedElapsed = 0f;

        while (_knockedElapsed < time)
        {
            _knockedElapsed += TimeUtilities.DeltaTime;
            yield return null;
        }

        _knockedElapsed = 0f;

        physicsRig.UnRagdollRig();

        _isKnockedOut = false;
    }
}