using HarmonyLib;

using LabFusion.Network;
using LabFusion.Senders;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(GameControl_Hub))]
    public static class GameControl_HubPatches
    {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.ELEVATORBREAKOUT))]
        public static bool ELEVATORBREAKOUT()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() =>
            {
                CampaignSender.SendHubEvent(BonelabHubEventType.ELEVATOR_BREAKOUT);
            });
        }


        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.SETUPELEVATOR))]
        public static bool SETUPELEVATOR()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() =>
            {
                CampaignSender.SendHubEvent(BonelabHubEventType.SETUP_ELEVATOR);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.OPENBWDOOR))]
        public static bool OPENBWDOOR()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() =>
            {
                CampaignSender.SendHubEvent(BonelabHubEventType.OPEN_BW_DOOR);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.BWBOXDESTROYED))]
        public static bool BWBOXDESTROYED()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() =>
            {
                CampaignSender.SendHubEvent(BonelabHubEventType.BW_BOX_DESTROYED);
            });
        }
    }
}
