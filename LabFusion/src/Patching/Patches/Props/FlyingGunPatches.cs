using HarmonyLib;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Entities;
using LabFusion.Utilities;

using Il2CppSLZ.Interaction;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Interaction;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(FlyingGun))]
public static class FlyingGunPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(FlyingGun.OnTriggerGripUpdate))]
    public static bool OnTriggerGripUpdate(FlyingGun __instance, Hand hand, ref bool __state)
    {
        // In a server, prevent two nimbus guns from sending you flying out of the map
        // Due to SLZ running these forces on update for whatever reason, the forces are inconsistent
        if (NetworkInfo.HasServer && hand.handedness == Handedness.LEFT)
        {
            var otherHand = hand.otherHand;

            if (otherHand.m_CurrentAttachedGO)
            {
                var otherGrip = Grip.Cache.Get(otherHand.m_CurrentAttachedGO);

                if (otherGrip.HasHost)
                {
                    var host = otherGrip.Host.GetHostGameObject();

                    if (host.GetComponent<FlyingGun>() != null)
                        return false;
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
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        if (!hand.manager.IsSelf())
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

        using var writer = FusionWriter.Create(NimbusGunNoclipData.Size);
        var data = NimbusGunNoclipData.Create(PlayerIdManager.LocalSmallId, entity.Id, __instance._noClipping);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.NimbusGunNoclip, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);
    }
}