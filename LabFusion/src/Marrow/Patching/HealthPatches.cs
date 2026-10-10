using HarmonyLib;

using LabFusion.Entities;
using LabFusion.Marrow.Rig;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.Utilities;

using UnityEngine;

using Il2CppSLZ.Marrow;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(Health))]
public static class HealthPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Health.SetAvatar))]
    public static void SetAvatarPostfix(Health __instance, Avatar avatar)
    {
        if (!NetworkManager.HasServer)
        {
            return;
        }

        // Clear renderers of any null renderers
        // Prevents immortal avatars with damage decals enabled
        var renderers = new List<Renderer>();

        foreach (var renderer in __instance.Renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            renderers.Add(renderer);
        }

        __instance.Renderers = renderers.ToArray();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Health.Respawn))]
    public static void Respawn(Health __instance)
    {
        var rigManager = __instance._rigManager;

        if (rigManager.IsLocalPlayer())
        {
            LocalHealth.InvokeRespawn();
        }

        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        if (!NetworkBeingManager.TryGetNetworkRig(rigManager, out var networkRig))
        {
            return;
        }

        var networkEntity = networkRig.NetworkEntity;

        if (!networkEntity.IsOwner)
        {
            return;
        }

        RigActionManager.RelayRigAction(new(networkEntity), RigActionType.Respawn);

        var networkPlayer = networkEntity.GetExtender<NetworkPlayer>();

        // If there's a NetworkPlayer, and we own it, then it can only be the local player
        if (networkPlayer != null)
        {
            OnLocalPlayerRespawn();
        }
    }

    private static void OnLocalPlayerRespawn()
    {
        LocalPlayer.ClearConstraints();

        // Unragdoll after respawning
        if (LocalPlayer.RagdollOnDeath)
        {
            LocalRagdoll.ToggleRagdoll(false);

            // Teleport so we don't fling
            LocalPlayer.TeleportToCheckpoint();
        }
    }
}
