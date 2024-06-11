using LabFusion.Data;
using LabFusion.Extensions;
using Il2CppSLZ.Rig;
using Il2CppSLZ.Bonelab;

using UnityEngine;
using LabFusion.Network;
using LabFusion.MarrowIntegration;
using Il2CppSLZ.Marrow.AI;

namespace LabFusion.Utilities
{
    public static class TriggerUtilities
    {
        public static readonly FusionDictionary<GenGameControl_Trigger, int> GenTriggerCount = new(new UnityComparer());

        internal static void Increment(GenGameControl_Trigger trigger)
        {
            if (!GenTriggerCount.ContainsKey(trigger))
                GenTriggerCount.Add(trigger, 0);

            GenTriggerCount[trigger]++;
        }

        internal static void Decrement(GenGameControl_Trigger trigger)
        {
            if (!GenTriggerCount.ContainsKey(trigger))
                GenTriggerCount.Add(trigger, 0);

            GenTriggerCount[trigger]--;
            GenTriggerCount[trigger] = ManagedMathf.Clamp(GenTriggerCount[trigger], 0, int.MaxValue);
        }

        public static bool CanEnter(GenGameControl_Trigger trigger)
        {
            if (!GenTriggerCount.ContainsKey(trigger))
                return false;

            return GenTriggerCount[trigger] <= 1;
        }

        public static bool CanExit(GenGameControl_Trigger trigger)
        {
            if (!GenTriggerCount.ContainsKey(trigger))
                return false;

            return GenTriggerCount[trigger] <= 0;
        }

        public static bool VerifyLevelTrigger(GenGameControl_Trigger trigger, Collider other, out bool runMethod)
        {
            runMethod = true;

            // Get transform of trigger
            var transform = trigger.transform;

            // Check if this is part of a launch pad/link data
            if (transform.GetComponentInParent<LinkData>() != null)
            {
                runMethod = IsMainRig(other);
                return true;
            }
            // Check if this is a taxi trigger for Home
            else if (HomeData.GameController != null && transform.parent.name.Contains("TaxiSequence_EnableWithTaxiStartChunk"))
            {
                var seat = HomeData.TaxiSeat;
                if (seat.rigManager != null)
                {
                    var proxy = other.gameObject.GetComponent<TriggerRefProxy>();
                    RigManager rig;

                    if (proxy && proxy.root && (rig = RigManager.Cache.Get(proxy.root)))
                    {
                        runMethod = seat.rigManager == rig;
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsMainRig(Collider other) => IsMatchingRig(other, RigData.RigReferences.RigManager);

        public static bool IsMatchingRig(Collider other, RigManager rig)
        {
            if (!NetworkInfo.HasServer || !RigData.HasPlayer)
                return true;

            var trigger = other.gameObject.GetComponent<TriggerRefProxy>();
            RigManager found;

            if (trigger && trigger.root && (found = RigManager.Cache.Get(trigger.root)))
            {
                return found == rig;
            }

            return false;
        }


        public static bool IsMatchingRig(TriggerRefProxy proxy, RigManager rig)
        {
            if (!NetworkInfo.HasServer || !RigData.HasPlayer)
                return true;

            RigManager found;

            if (proxy.root && (found = RigManager.Cache.Get(proxy.root)))
            {
                return found == rig;
            }

            return false;
        }
    }
}
