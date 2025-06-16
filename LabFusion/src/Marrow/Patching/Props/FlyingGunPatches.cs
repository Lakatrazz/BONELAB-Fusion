using HarmonyLib;

using LabFusion.Network;
using LabFusion.Marrow.Extenders;
using LabFusion.Marrow.Messages;
using LabFusion.Utilities;
using LabFusion.Scene;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(FlyingGun))]
public static class FlyingGunPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(FlyingGun.OnTriggerGripUpdate))]
    public static bool OnTriggerGripUpdatePrefix(FlyingGun __instance, Hand hand, ref bool __state)
    {
        // If dev tools are disabled, don't allow the nimbus gun to function at all
        if (FusionDevTools.DevToolsDisabled)
        {
            return false;
        }

        // In a server, prevent two nimbus guns from sending you flying out of the map
        // Due to SLZ running these forces on update for whatever reason, the forces are inconsistent
        if (NetworkSceneManager.IsLevelNetworked && hand.handedness == Handedness.LEFT)
        {
            var otherHand = hand.otherHand;

            if (otherHand.m_CurrentAttachedGO)
            {
                var otherGrip = Grip.Cache.Get(otherHand.m_CurrentAttachedGO);

                if (otherGrip.HasHost)
                {
                    var host = otherGrip.Host.GetHostGameObject();

                    if (host.GetComponent<FlyingGun>() != null)
                    {
                        return false;
                    }
                }
            }
        }

        __state = __instance._noClipping;

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(FlyingGun.OnTriggerGripUpdate))]
    public static void OnTriggerGripUpdatePostfix(FlyingGun __instance, Hand hand, bool __state)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        if (!hand.manager.IsLocalPlayer())
        {
            return;
        }

        // State didn't change, don't send a message
        if (__state == __instance._noClipping)
        {
            return;
        }

        var entity = FlyingGunExtender.Cache.Get(__instance);

        if (entity == null)
        {
            return;
        }

        var data = new NimbusGunNoClipData()
        {
            NimbusGunID = entity.ID,
            NoClip = __instance._noClipping,
        };

        MessageRelay.RelayModule<NimbusGunNoClipMessage, NimbusGunNoClipData>(data, CommonMessageRoutes.ReliableToOtherClients);
    }
}