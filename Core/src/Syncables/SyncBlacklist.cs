using LabFusion.Utilities;

using SLZ.Bonelab;
using SLZ.Combat;
using SLZ.Marrow.Pool;
using SLZ.Props.Weapons;
using SLZ.Props;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using UnhollowerRuntimeLib;

using Il2Type = Il2CppSystem.Type;

using SLZ.Rig;

namespace LabFusion.Syncables {
    public static class SyncBlacklist {
        private static readonly Il2Type[] BlacklistedChildComponents = new Il2Type[3] {
            Il2CppType.Of<GetVelocity>(),
            Il2CppType.Of<SpawnFragment>(),
            Il2CppType.Of<ProjectileBalloon>(),
        };

        private static readonly Il2Type[] BlacklistedParentComponents = new Il2Type[1] {
            Il2CppType.Of<RigManager>(),
        };

        public static bool IsSyncWhitelisted(this GameObject go)
        {
            // Check children components
            foreach (var type in BlacklistedChildComponents) {
                if (go.GetComponentInChildren(type, true) != null) {
                    return false;
                } 
            }

            // Check parent components
            foreach (var type in BlacklistedParentComponents) {
                if (go.GetComponentInParent(type, true) != null) {
                    return false;
                }
            }

            // Other hardcoded stuff (probably cleanup later)
            bool hasRigidbody = go.GetComponentInChildren<Rigidbody>(true) != null;

            bool hasGunProperties = go.GetComponentInChildren<FirearmCartridge>(true) == null || go.GetComponentInChildren<Gun>(true) != null;

            bool spawnableProperties = true;

            var assetPoolee = go.GetComponentInChildren<AssetPoolee>();
            if (assetPoolee)
                spawnableProperties = assetPoolee.spawnableCrate.Barcode != SpawnableWarehouseUtilities.BOARD_BARCODE;

            bool isValid = hasRigidbody && hasGunProperties && spawnableProperties;

            return isValid;
        }
    }
}
