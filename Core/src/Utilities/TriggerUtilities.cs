using LabFusion.Data;
using LabFusion.Extensions;

using SLZ.AI;
using SLZ.Rig;
using SLZ.Zones;
using SLZ.Bonelab;

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using LabFusion.Network;
using SLZ.Marrow.SceneStreaming;
using MelonLoader;
using LabFusion.MarrowIntegration;

namespace LabFusion.Utilities {
    public static class TriggerUtilities {
        public static readonly Dictionary<TriggerLasers, int> TriggerCount = new Dictionary<TriggerLasers, int>(new UnityComparer());
        public static readonly Dictionary<GenGameControl_Trigger, int> GenTriggerCount = new Dictionary<GenGameControl_Trigger, int>(new UnityComparer());
        public static readonly Dictionary<TriggerRefProxy, List<Chunk>> PlayerChunks = new Dictionary<TriggerRefProxy, List<Chunk>>(new UnityComparer());

        internal static void Increment(TriggerLasers trigger) {
            if (!TriggerCount.ContainsKey(trigger))
                TriggerCount.Add(trigger, 0);

            TriggerCount[trigger]++;
        }

        internal static void Decrement(TriggerLasers trigger) {
            if (!TriggerCount.ContainsKey(trigger))
                TriggerCount.Add(trigger, 0);

            TriggerCount[trigger]--;
            TriggerCount[trigger] = Mathf.Clamp(TriggerCount[trigger], 0, int.MaxValue);
        }

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
            GenTriggerCount[trigger] = Mathf.Clamp(GenTriggerCount[trigger], 0, int.MaxValue);
        }

        internal static void SetChunk(Collider other, Chunk chunk) {
            var proxy = TriggerRefProxy.Cache.Get(other.gameObject);
            if (!proxy)
                return;

            if (!PlayerChunks.ContainsKey(proxy)) {
                PlayerChunks.Add(proxy, new List<Chunk>());
            }

            PlayerChunks[proxy] = chunk.GetChunks();
        }

        internal static void AddChunk(Collider other, Chunk chunk) {
            var proxy = TriggerRefProxy.Cache.Get(other.gameObject);
            if (!proxy)
                return;

            if (!PlayerChunks.ContainsKey(proxy)) {
                PlayerChunks.Add(proxy, new List<Chunk>());
            }

            var chunks = chunk.GetChunks();
            for (var i = 0; i < chunks.Count; i++) {
                var found = chunks[i];

                if (!PlayerChunks[proxy].Has(found))
                    PlayerChunks[proxy].Add(found);
            }
        }

        internal static List<Chunk> GetChunks(Collider other) {
            var proxy = TriggerRefProxy.Cache.Get(other.gameObject);
            if (!proxy)
                return new List<Chunk>();

            if (!PlayerChunks.ContainsKey(proxy)) {
                PlayerChunks.Add(proxy, new List<Chunk>());
            }

            return PlayerChunks[proxy];
        }

        internal static bool CanUnload(Chunk chunk) {
            foreach (var pair in PlayerChunks) {
                if (pair.Value.Has(chunk))
                    return false;
            }

            return true;
        }

        public static bool CanEnter(TriggerLasers trigger)
        {
            if (!TriggerCount.ContainsKey(trigger))
                return false;

            return TriggerCount[trigger] <= 1;
        }

        public static bool CanExit(TriggerLasers trigger)
        {
            if (!TriggerCount.ContainsKey(trigger))
                return false;

            return TriggerCount[trigger] <= 0;
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

        public static bool VerifyLevelTrigger(TriggerLasers trigger, Collider other, out bool runMethod) {
            runMethod = false;

            // Get transform of trigger
            var transform = trigger.transform;

            // Check if this has a marrow sdk addon
            if (OnlyTriggerOnLocalPlayer.Cache.ContainsSource(trigger.gameObject)) {
                runMethod = IsMainRig(other);
                return true;
            }
            // Check if this is a lap trigger for Monogon Motorway
            else if (KartRaceData.GameController != null && KartRaceData.GameController.transform == transform.parent) {
                runMethod = IsMainRig(other);
                return true;
            }

            return false;
        }

        public static bool VerifyLevelTrigger(GenGameControl_Trigger trigger, Collider other, out bool runMethod)
        {
            runMethod = true;

            // Get transform of trigger
            var transform = trigger.transform;

            // Check if this has a marrow sdk addon
            if (OnlyTriggerOnLocalPlayer.Cache.ContainsSource(trigger.gameObject)) {
                runMethod = IsMainRig(other);
                return true;
            }
            // Check if this is part of a launch pad/link data
            else if (transform.GetComponentInParent<LinkData>() != null) {
                runMethod = IsMainRig(other);
                return true;
            }
            // Check if this is a taxi trigger for Home
            else if (HomeData.GameController != null && transform.parent.name.Contains("TaxiSequence_EnableWithTaxiStartChunk"))
            {
                var seat = HomeData.TaxiSeat;
                if (seat.rigManager != null) {
                    var proxy = TriggerRefProxy.Cache.Get(other.gameObject);
                    RigManager rig;

                    if (proxy && proxy.root && (rig = RigManager.Cache.Get(proxy.root))) {
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
            if (!NetworkInfo.HasServer || RigData.RigReferences.RigManager.IsNOC())
                return true;

            var trigger = TriggerRefProxy.Cache.Get(other.gameObject);
            RigManager found;

            if (trigger && trigger.root && (found = RigManager.Cache.Get(trigger.root)))
            {
                return found == rig;
            }

            return false;
        }
    }
}
