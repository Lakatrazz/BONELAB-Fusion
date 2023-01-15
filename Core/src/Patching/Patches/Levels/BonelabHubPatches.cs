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
        public static bool ELEVATORBREAKOUT()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer) {
                if (NetworkInfo.IsServer)
                    CampaignSender.SendHubEvent(BonelabHubEventType.ELEVATOR_BREAKOUT);
                else
                    return false;
            }

            return true;
        }


        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.SETUPELEVATOR))]
        public static bool SETUPELEVATOR()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                    CampaignSender.SendHubEvent(BonelabHubEventType.SETUP_ELEVATOR);
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.OPENBWDOOR))]
        public static bool OPENBWDOOR()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                    CampaignSender.SendHubEvent(BonelabHubEventType.OPEN_BW_DOOR);
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.BWBOXDESTROYED))]
        public static bool BWBOXDESTROYED()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                    CampaignSender.SendHubEvent(BonelabHubEventType.BW_BOX_DESTROYED);
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.AIRLOCKENTERNORTH))]
        public static bool AIRLOCKENTERNORTH()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                    CampaignSender.SendHubEvent(BonelabHubEventType.AIR_LOCK_ENTER_NORTH);
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.AIRLOCKENTERSOUTH))]
        public static bool AIRLOCKENTERSOUTH()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                    CampaignSender.SendHubEvent(BonelabHubEventType.AIR_LOCK_ENTER_SOUTH);
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.AIRLOCKOCCUPIED))]
        public static bool AIRLOCKOCCUPIED()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                    CampaignSender.SendHubEvent(BonelabHubEventType.AIR_LOCK_OCCUPIED);
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.AIRLOCKUNOCCUPIED))]
        public static bool AIRLOCKUNOCCUPIED()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                    CampaignSender.SendHubEvent(BonelabHubEventType.AIR_LOCK_UNOCCUPIED);
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.AIRLOCKCYCLE))]
        public static bool AIRLOCKCYCLE()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                    CampaignSender.SendHubEvent(BonelabHubEventType.AIR_LOCK_CYCLE);
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.CANCELCYCLE))]
        public static bool CANCELCYCLE()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                    CampaignSender.SendHubEvent(BonelabHubEventType.CANCEL_CYCLE);
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.OpenSmallDoor))]
        public static bool OpenSmallDoor()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                    CampaignSender.SendHubEvent(BonelabHubEventType.OPEN_SMALL_DOOR);
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.CloseSmallDoor))]
        public static bool CloseSmallDoor()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                    CampaignSender.SendHubEvent(BonelabHubEventType.CLOSE_SMALL_DOOR);
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.OpenBigDoors))]
        public static bool OpenBigDoors()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                    CampaignSender.SendHubEvent(BonelabHubEventType.OPEN_BIG_DOORS);
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.CloseBigDoors))]
        public static bool CloseBigDoors()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                    CampaignSender.SendHubEvent(BonelabHubEventType.CLOSE_BIG_DOORS);
                else
                    return false;
            }

            return true;
        }

        // Prevent the hub controller from unloading parts of the level
        // This is due to it not using triggers, but instead manual chunk loading?
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.OnAirlockUnloaded))]
        public static bool OnAirlockUnloaded() {
            if (NetworkInfo.HasServer)
                return false;

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.OnFunicularUnloaded))]
        public static bool OnFunicularUnloaded()
        {
            if (NetworkInfo.HasServer)
                return false;

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.OnLabUnloaded))]
        public static bool OnLabUnloaded()
        {
            if (NetworkInfo.HasServer)
                return false;

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Hub.OnSLZRoomUnloaded))]
        public static bool OnSLZRoomUnloaded()
        {
            if (NetworkInfo.HasServer)
                return false;

            return true;
        }
    }
}
