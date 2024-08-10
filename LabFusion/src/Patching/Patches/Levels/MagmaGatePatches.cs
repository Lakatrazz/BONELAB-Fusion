using HarmonyLib;
using LabFusion.Network;
using LabFusion.Senders;
using Il2CppSLZ.Bonelab;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(GameControl_MagmaGate))]
    public static class MagmaGatePatches
    {
        public static bool IgnorePatches = false;

        // [HarmonyPrefix]
        // [HarmonyPatch(nameof(GameControl_MagmaGate.LevelSetup))]
        // public static bool LevelSetup(GameControl_MagmaGate __instance)
        // {
        //     if (IgnorePatches)
        //         return true;
        // 
        //     if (NetworkInfo.HasServer)
        //     {
        //         if (!NetworkInfo.IsServer)
        //             return false;
        //         else
        //         {
        //             CampaignSender.SendMagmaGateEvent(MagmaGateEventType.BUTTONS_SETUP);
        //         }
        //     }
        // 
        //     return true;
        // }

        // [HarmonyPrefix]
        // [HarmonyPatch(nameof(GameControl_MagmaGate.ObjectiveComplete))]
        // public static bool ObjectiveComplete(GameControl_MagmaGate __instance)
        // {
        //     if (IgnorePatches)
        //         return true;
        // 
        //     if (NetworkInfo.HasServer)
        //     {
        //         if (!NetworkInfo.IsServer)
        //             return false;
        //         else
        //         {
        //             CampaignSender.SendMagmaGateEvent(MagmaGateEventType.OBJECTIVE_COMPLETE_SETUP);
        //         }
        //     }
        // 
        //     return true;
        // }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_MagmaGate.LoseSequence))]
        public static bool LoseSequence(GameControl_MagmaGate __instance)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    CampaignSender.SendMagmaGateEvent(MagmaGateEventType.LOSE_SEQUENCE);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_MagmaGate.DoorDissolve))]
        public static bool DoorDissolve(GameControl_MagmaGate __instance)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    CampaignSender.SendMagmaGateEvent(MagmaGateEventType.DOOR_DISSOLVE);
                }
            }

            return true;
        }
    }
}
