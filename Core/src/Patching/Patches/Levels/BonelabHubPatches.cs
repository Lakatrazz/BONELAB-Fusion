using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Senders;

using SLZ.Bonelab;
using SLZ.UI;

namespace LabFusion.Patching {
    // Hub sync: The Larginator
    [HarmonyPatch(typeof(GameControl_Hub))]
    public static class GameControl_HubPatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.ELEVATORBREAKOUT))]
        public static bool ELEVATORBREAKOUT() {
            return IgnorePatches || QuickSender.SendServerMessage(() => { 
                CampaignSender.SendHubEvent(BonelabHubEventType.ELEVATOR_BREAKOUT); 
            });
        }


        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.SETUPELEVATOR))]
        public static bool SETUPELEVATOR()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                CampaignSender.SendHubEvent(BonelabHubEventType.SETUP_ELEVATOR);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.OPENBWDOOR))]
        public static bool OPENBWDOOR()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                CampaignSender.SendHubEvent(BonelabHubEventType.OPEN_BW_DOOR);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.BWBOXDESTROYED))]
        public static bool BWBOXDESTROYED()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                CampaignSender.SendHubEvent(BonelabHubEventType.BW_BOX_DESTROYED);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.AIRLOCKENTERNORTH))]
        public static bool AIRLOCKENTERNORTH()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                CampaignSender.SendHubEvent(BonelabHubEventType.AIR_LOCK_ENTER_NORTH);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.AIRLOCKENTERSOUTH))]
        public static bool AIRLOCKENTERSOUTH()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                CampaignSender.SendHubEvent(BonelabHubEventType.AIR_LOCK_ENTER_SOUTH);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.AIRLOCKOCCUPIED))]
        public static bool AIRLOCKOCCUPIED()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                CampaignSender.SendHubEvent(BonelabHubEventType.AIR_LOCK_OCCUPIED);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.AIRLOCKUNOCCUPIED))]
        public static bool AIRLOCKUNOCCUPIED()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                CampaignSender.SendHubEvent(BonelabHubEventType.AIR_LOCK_UNOCCUPIED);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.AIRLOCKCYCLE))]
        public static bool AIRLOCKCYCLE()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                CampaignSender.SendHubEvent(BonelabHubEventType.AIR_LOCK_CYCLE);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.CANCELCYCLE))]
        public static bool CANCELCYCLE()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                CampaignSender.SendHubEvent(BonelabHubEventType.CANCEL_CYCLE);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.OpenSmallDoor))]
        public static bool OpenSmallDoor()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                CampaignSender.SendHubEvent(BonelabHubEventType.OPEN_SMALL_DOOR);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.CloseSmallDoor))]
        public static bool CloseSmallDoor()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                CampaignSender.SendHubEvent(BonelabHubEventType.CLOSE_SMALL_DOOR);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.OpenBigDoors))]
        public static bool OpenBigDoors()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                CampaignSender.SendHubEvent(BonelabHubEventType.OPEN_BIG_DOORS);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.CloseBigDoors))]
        public static bool CloseBigDoors()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                CampaignSender.SendHubEvent(BonelabHubEventType.CLOSE_BIG_DOORS);
            });
        }
    }
}
