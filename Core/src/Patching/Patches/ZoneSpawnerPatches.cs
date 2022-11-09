using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.Network;
using SLZ.Zones;
using UnityEngine;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(ZoneSpawner), nameof(ZoneSpawner.Spawn))]
    public class ZoneSpawnerSpawnPatch
    {
        public static bool Prefix(GameObject playerObject) {
            if (NetworkInfo.HasServer && !NetworkInfo.IsServer)
                return false;

            return true;
        }
    }
}
